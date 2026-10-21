#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Harness lộ trình Map 2 (CVM2 11; khuôn Map1RouteProbe) — CHỈ Editor, scene Main.unity khi Play. Qua MCP:
///   Map2RouteProbe.Record(tpu)            ⇒ phát map1_route.txt từ đầu Stage 01 (GameRules.NewGame + GoTo(Stage1,0)) tới tick màn STAGE 04 bật,
///                                            rồi AUTOPILOT Map 2 (Pilot bên dưới — chỉ đọc trạng thái, bấm nút qua NesPad.Inject) tới STAGE 07 stub,
///                                            ghi input từ tick STAGE 04 ⇒ Assets/Data/Tests/Replays/map2_route.txt
///   Map2RouteProbe.Replay(shotDir, tpu)   ⇒ map1_route → map2_route (NesPad.PlayReplay), hash trạng thái từng tick CÙNG công thức
///                                            Map1RouteProbe (từ tick 1 Stage 01 ⇒ hash cuối = hash luồng thật Title → STAGE 07 của
///                                            Map1AcceptProbe.StartFlow), mốc luồng, chụp Game view ở các mốc chuyển cảnh (pairs11.py)
///   Map2RouteProbe.Try(stage, tpu)        ⇒ THỬ autopilot từ đầu stage 4/5/6 (phiên dev — session giả, KHÔNG ghi file) để chỉnh lộ trình
///   Map2RouteProbe.Report()               ⇒ mốc + hash + "trước STAGE 04" (phải = RegressionRunner.PRE4 — Map 1 không đổi; CVM3 04: 59cafbaa) + vết autopilot
/// Chạy: NesClock TẮT, tự gọi NesClock.Step tpu tick mỗi vòng EditorApplication.update. Cheat BẤT TỬ bật suốt (như Map 1; Map 2 mọi section có
/// quái/bẫy/hố: Spear Knight, Medusa Head, bẫy ép, Bone Tower, Ghost, Queen Medusa — autopilot không né quái; hố vẫn chết ⇒ autopilot tự tránh).
/// </summary>
public static class Map2RouteProbe
{
    public const string REPLAY = "Assets/Data/Tests/Replays/map2_route.txt";
    public const int MAX_TICKS = 60000;
    /// <summary>Số tick chạy sau khi STAGE 07 stub hiện rồi dừng.</summary>
    public const int TAIL_TICKS = 60;

    public static bool Running { get; private set; }
    public static bool Finished { get; private set; }
    public static string Mode { get; private set; } = "";
    public static string Error { get; private set; } = "";
    public static int Tick { get; private set; }
    public static uint Hash { get; private set; }
    public static uint PreStage4Hash { get; private set; }
    public static int Stage4Tick { get; private set; } = -1;
    public static int Stage7Tick { get; private set; } = -1;
    public static int ShotsTaken { get; private set; }
    public static int TicksPerUpdate = 40;
    /// <summary>Vết autopilot mỗi N tick; TraceStep ≠ null ⇒ thêm MỌI tick của bước có tên chứa chuỗi này (chỉnh lộ trình).</summary>
    public static int TraceEvery = 30;
    public static string TraceStep;
    /// <summary>Chỉnh x của bước autopilot lúc chạy (khoá = tên bước, chỉ khi THỬ/GHI; mặc định = số trong Pilot).</summary>
    public static readonly Dictionary<string, int> Tune = new Dictionary<string, int>();
    public static readonly List<string> Marks = new List<string>();
    public static readonly List<uint> Checkpoints = new List<uint>();
    public static readonly List<int> ObjectCounts = new List<int>();
    /// <summary>Vết autopilot (mỗi 30 tick + mỗi bước) — chỉnh lộ trình.</summary>
    public static readonly List<string> Trace = new List<string>();
    /// <summary>Tick lộ trình của các mốc luồng (StageFlow.Milestone + màn) — ghép cặp ảnh.</summary>
    public static readonly List<string> Events = new List<string>();

    private static MethodInfo stepMethod;
    private static Pilot pilot;
    private static bool prevInvincible;
    private static eGameScreen lastScreen;
    private static string lastSection = "";
    private static List<int> map2;
    private static bool switched, tryMode;
    private static int tryStage;
    private static List<Shot> shots;
    private static string shotDir, waitFile;
    private static int waited;
    private static int bossWakeTick = -1, lockTick = -1, mapEnterFrame = -1, stage7Frame = -1, crystalTick = -1, tallyTimeTick = -1;
    private static readonly Dictionary<string, int> milestoneTick = new Dictionary<string, int>();

    private static StageWorld W => StageWorld.Current;
    private static SimonController S => W != null ? W.Player as SimonController : null;
    private static int Cx => W != null && W.CameraCtl != null ? W.CameraCtl.X : -1;
    private static string Sec => W != null && W.Section != null ? W.Section.name : "";
    private static bool InStage => GameManager.I != null && GameManager.I.State == eGameScreen.Stage1 && !GameManager.I.IsTransitioning && W != null;
    private static StageFlow Flow => W != null ? W.GetComponent<StageFlow>() : null;
    /// <summary>Đã vào STAGE 04 (phần Map 2 của lộ trình) — mốc ảnh boss/cầu/tally/bản đồ chỉ tính ở đây (Map 1 cũng có).</summary>
    private static bool InMap2 => tryMode || Stage4Tick >= 0;

    // ---------------------------------------------------------------- điểm chụp (mốc luồng — video Map_2 ghép bằng Tools/cvm2/pairs11.py)

    private class Shot
    {
        public string name;
        public Func<bool> when;
        public bool done;
        public Shot(string n, Func<bool> w) { name = n; when = w; }
    }

    private static int Ms(string key)
    {
        int t;
        return milestoneTick.TryGetValue(key, out t) ? t : -1;
    }

    private static bool AtMs(string key, int dt)
    {
        int t = Ms(key);
        return t >= 0 && InStage && W.TickCount == t + dt;
    }

    /// <summary>Tên ảnh = &lt;mốc&gt;_r&lt;tick tương đối&gt; — pairs11.py ánh xạ sang frame video theo cùng mốc (MAP2_TIMELINE §7 + CVM2_MEMORY §2m).</summary>
    private static List<Shot> BuildShots()
    {
        var l = new List<Shot>
        {
            new Shot("st04_r001", () => InStage && GameSession.Current.stage == 4 && Sec == "TowerEntranceA" && W.TickCount == 1),
            new Shot("st04_r020", () => InStage && GameSession.Current.stage == 4 && Sec == "TowerEntranceA" && W.TickCount == 20),
            // đổi tầng: mốc ghi ở đầu tick (PreTick) ⇒ khung hình đầu section mới = mốc + 1
            new Shot("teb_r001", () => AtMs("arr:TowerEntranceB", 1)),
            new Shot("teb_r006", () => AtMs("arr:TowerEntranceB", 6)),
            new Shot("hallb_r001", () => AtMs("arr:TowerHallB", 1)),
            new Shot("medusa_r001", () => AtMs("arr:Medusa", 1)),
        };
        // cửa trái: mốc door (cuộn 1 bắt đầu) / door_stop (cuộn 1 xong) / stageN (tới section mới)
        foreach (var d in new[] { "1", "2" })
        {
            string k = d;
            l.Add(new Shot("door" + k + "_scroll1_r064", () => AtMs("door" + k, 64)));
            foreach (int r in new[] { 2, 9, 15, 44, 70, 106, 126, 150 })
            {
                int rr = r;
                l.Add(new Shot("door" + k + "_stop_r" + rr.ToString("000"), () => AtMs("door_stop" + k, rr)));
            }
        }
        l.Add(new Shot("door1_scroll2_r064", () => AtMs("door_stop1", Map2FlowTiming.DOOR_TO_HALL_A.scroll2 + 64)));
        l.Add(new Shot("door2_scroll2_r064", () => AtMs("door_stop2", Map2FlowTiming.DOOR_TO_HALL_C.scroll2 + 64)));
        l.Add(new Shot("halla_r002", () => AtMs("arr:TowerHallA", 2)));
        l.Add(new Shot("hallc_r002", () => AtMs("arr:TowerHallC", 2)));
        l.Add(new Shot("lock_r000", () => lockTick >= 0 && InStage && W.TickCount == lockTick));
        l.Add(new Shot("wake_r002", () => bossWakeTick >= 0 && InStage && W.TickCount == bossWakeTick + 2));
        l.Add(new Shot("crystal_r030", () => crystalTick >= 0 && InStage && W.TickCount == crystalTick + 30));
        l.Add(new Shot("tally_r040", () => tallyTimeTick >= 0 && InStage && W.TickCount == tallyTimeTick + 40));
        l.Add(new Shot("map2_r100", () => mapEnterFrame >= 0 && GameManager.I.State == eGameScreen.CastleMap && NesClock.Frame - mapEnterFrame == 100));
        l.Add(new Shot("map2_r211", () => mapEnterFrame >= 0 && GameManager.I.State == eGameScreen.CastleMap && NesClock.Frame - mapEnterFrame == 211));
        l.Add(new Shot("map2_r300", () => mapEnterFrame >= 0 && GameManager.I.State == eGameScreen.CastleMap && NesClock.Frame - mapEnterFrame == 300));
        l.Add(new Shot("st07_r006", () => stage7Frame >= 0 && GameManager.I.State == eGameScreen.Stage7Stub && NesClock.Frame - stage7Frame == 6));
        return l;
    }

    private static BossBase Boss()
    {
        if (W == null)
            return null;
        foreach (var sp in W.Spawners)
        {
            var a = sp as BossArena;
            if (a != null)
                return a.Boss;
        }
        return null;
    }

    // ---------------------------------------------------------------- chạy

    /// <summary>Phát map1_route rồi autopilot Map 2 + ghi map2_route.txt.</summary>
    public static string Record(int tpu)
    {
        return Start(false, tpu, null, 0);
    }

    /// <summary>Phát map1_route → map2_route (shotDir null ⇒ không chụp).</summary>
    public static string Replay(string dir, int tpu)
    {
        if (!File.Exists(REPLAY))
            return "thiếu " + REPLAY + " — chạy Record trước";
        map2 = NesReplay.Decode(File.ReadAllText(REPLAY));
        return Start(true, tpu, dir, 0);
    }

    /// <summary>Thử autopilot từ đầu stage 4/5/6 (session giả như cuối Map 1: SCORE 6640, roi 3, dao, tim 5) — không ghi file, không hash chuẩn.</summary>
    public static string Try(int stage, int tpu)
    {
        return Start(false, tpu, null, Mathf.Clamp(stage, 4, 6));
    }

    private static string Start(bool replay, int tpu, string dir, int devStage)
    {
        if (!EditorApplication.isPlaying || GameManager.I == null || NesClock.I == null || NesPad.I == null)
            return "cần Play Main.unity";
        if (Running || Map1RouteProbe.Running || Map1AcceptProbe.Running)
            return "đang chạy";
        if (!File.Exists(Map1RouteProbe.REPLAY))
            return "thiếu " + Map1RouteProbe.REPLAY;
        TicksPerUpdate = Mathf.Max(1, tpu);
        tryMode = devStage > 0;
        tryStage = devStage;
        Mode = replay ? "replay" : tryMode ? "try" + devStage : "record";
        Error = "";
        Finished = false;
        Tick = 0;
        Hash = 2166136261u;
        PreStage4Hash = 0;
        Stage4Tick = Stage7Tick = -1;
        ShotsTaken = 0;
        Marks.Clear();
        Checkpoints.Clear();
        Trace.Clear();
        Events.Clear();
        milestoneTick.Clear();
        lastSection = "";
        switched = false;
        bossWakeTick = lockTick = mapEnterFrame = stage7Frame = crystalTick = tallyTimeTick = -1;
        shotDir = dir;
        waitFile = null;
        shots = dir != null ? BuildShots() : new List<Shot>();
        if (dir != null)
            Directory.CreateDirectory(dir);
        if (!replay)
            map2 = null;
        prevInvincible = EditorPrefs.GetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, false);
        EditorPrefs.SetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, true);
        CvgpDebug.CheatInvincible = true;
        stepMethod = typeof(NesClock).GetMethod("Step", BindingFlags.Instance | BindingFlags.NonPublic);
        NesClock.I.enabled = false;
        StageFlow.Milestone += OnMilestone;
        BossBase.BossDefeated += OnBossDefeated;
        BossBase.CrystalSpawned += OnCrystalSpawned;
        ItemEffects.CrystalPicked += OnCrystalPicked;
        GameRules.NewGame();
        if (tryMode)
        {
            // session giả = trạng thái cuối lộ trình Map 1 (Map1RouteProbe tick 8464: SCORE 6650 — CVM3 04 ghi lại lộ trình, trước 8449 / 6640; P3, roi 3, dao ×1, tim 5)
            var s = GameSession.Current;
            s.score = 6650;
            s.whipLevel = 3;
            s.SetSubWeapon(eSubWeapon.Dagger);
            s.stage = devStage;
            s.NotifyChanged();
        }
        if (!GameManager.I.GoTo(eGameScreen.Stage1, 0))
        {
            Stop("GameManager đang chuyển cảnh");
            return Error;
        }
        lastScreen = GameManager.I.State;
        if (tryMode)
            SwitchToMap2();
        else
            NesPad.I.PlayReplay(NesReplay.Decode(File.ReadAllText(Map1RouteProbe.REPLAY)));
        Running = true;
        EditorApplication.update += Pump;
        Mark("start " + Mode);
        return Mode + " bắt đầu";
    }

    /// <summary>Tick màn STAGE 04 bật (hoặc đầu phiên thử): bỏ phần đuôi map1_route, phát map2_route / bắt đầu autopilot + ghi.</summary>
    private static void SwitchToMap2()
    {
        switched = true;
        if (map2 != null)
        {
            NesPad.I.PlayReplay(map2);
            Mark("phát map2_route (" + map2.Count + " tick)");
            return;
        }
        NesPad.I.StopReplay();
        pilot = new Pilot(tryMode ? tryStage : 4);
        if (!tryMode)
            NesPad.I.StartRecord();
        Mark("autopilot Map 2");
    }

    private static void Stop(string err)
    {
        EditorApplication.update -= Pump;
        StageFlow.Milestone -= OnMilestone;
        BossBase.BossDefeated -= OnBossDefeated;
        BossBase.CrystalSpawned -= OnCrystalSpawned;
        ItemEffects.CrystalPicked -= OnCrystalPicked;
        if (NesPad.I != null)
        {
            NesPad.I.StopReplay();
            if (NesPad.I.IsRecording)
            {
                var rec = NesPad.I.StopRecord();
                if (string.IsNullOrEmpty(err) && !tryMode)
                    SaveReplay(rec);
            }
        }
        if (NesClock.I != null)
            NesClock.I.enabled = true;
        EditorPrefs.SetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, prevInvincible);
        CvgpDebug.CheatInvincible = prevInvincible;
        Running = false;
        Finished = true;
        Error = err ?? "";
        if (pilot != null)
            Trace.Add("pilot dừng ở bước '" + pilot.Current + "' (tuổi " + pilot.Age + ")");
        pilot = null;
        Checkpoints.Add(Hash);
        ObjectCounts.Add(UnityEngine.Object.FindObjectsOfType<Transform>(true).Length);
        Mark(string.IsNullOrEmpty(err) ? "end" : "LỖI " + err);
    }

    private static void SaveReplay(List<int> rec)
    {
        var sb = new StringBuilder();
        sb.Append("# ").Append(rec.Count).Append(" tick, hash ").Append(NesReplay.Hash(rec).ToString("x8")).Append('\n');
        sb.Append("# CVM2 11 map2_route: lộ trình Map 2 từ tick màn STAGE 04 bật (sau map1_route — Map2RouteProbe phát map1_route trước) tới STAGE 07 ")
          .Append("stub — ghi bằng autopilot Map2RouteProbe.Pilot, CHEAT BẤT TỬ bật suốt (CVGP.CheatInvincible) — phát lại: Map2RouteProbe.Replay\n");
        sb.Append(NesReplay.Encode(rec)).Append('\n');
        File.WriteAllText(REPLAY, sb.ToString(), new UTF8Encoding(false));
        AssetDatabase.ImportAsset(REPLAY);
    }

    private static eGameScreen PendingNext()
    {
        var f = typeof(GameManager).GetField("pendingNext", BindingFlags.Instance | BindingFlags.NonPublic);
        return f != null ? (eGameScreen)f.GetValue(GameManager.I) : eGameScreen.Boot;
    }

    private static int BlackLeft()
    {
        var f = typeof(GameManager).GetField("blackLeft", BindingFlags.Instance | BindingFlags.NonPublic);
        return f != null ? (int)f.GetValue(GameManager.I) : -1;
    }

    private static void Pump()
    {
        if (!Running)
            return;
        if (waitFile != null)
        {
            if (File.Exists(waitFile) && new FileInfo(waitFile).Length > 0 && ++waited >= 2)
                waitFile = null;
            else
                return;
        }
        for (int k = 0; k < TicksPerUpdate && Running; k++)
        {
            if (Tick >= MAX_TICKS)
            {
                Stop("quá " + MAX_TICKS + " tick");
                return;
            }
            var gm = GameManager.I;
            // tick màn đen cuối trước STAGE 04 (bản đồ lâu đài → StageScreen stage 4): NesPad.Tick của tick này đọc phần tử 0 của map2
            if (!switched && gm.IsTransitioning && PendingNext() == eGameScreen.Stage1 && BlackLeft() == 1 && GameSession.Current.stage == 4)
                SwitchToMap2();
            if (pilot != null)
            {
                int mask = pilot.Decide();
                if (pilot.Failed != null)
                {
                    Stop("autopilot: " + pilot.Failed);
                    return;
                }
                for (int b = 0; b < NesPad.BUTTON_COUNT; b++)
                    if ((mask & (1 << b)) != 0)
                        NesPad.I.Inject((eNesButton)b, 1);
            }
            else if (switched && Mode == "replay" && !NesPad.I.IsReplaying && Stage7Tick < 0)
            {
                Stop("hết map2_route trước STAGE 07");
                return;
            }
            else if (!switched && !NesPad.I.IsReplaying)
            {
                Stop("hết map1_route trước STAGE 04");
                return;
            }
            stepMethod.Invoke(NesClock.I, null);
            Tick++;
            Observe();
            if (Stage7Tick >= 0 && Tick - Stage7Tick >= TAIL_TICKS)
            {
                Stop(null);
                return;
            }
            if (TakeShots())
                return;   // chờ file ảnh
        }
    }

    private static void Observe()
    {
        var gm = GameManager.I;
        if (gm.State != lastScreen && !gm.IsTransitioning)
        {
            lastScreen = gm.State;
            Mark("màn " + gm.State);
            Ev("màn " + gm.State);
            if (Map1RouteProbe.IsStage4Screen(gm.State) && Stage4Tick < 0 && !tryMode)
            {
                Stage4Tick = Tick;
                PreStage4Hash = Hash;
            }
            if (gm.State == eGameScreen.CastleMap && InMap2)
                mapEnterFrame = NesClock.Frame;
            if (gm.State == eGameScreen.Stage7Stub)
            {
                Stage7Tick = Tick;
                stage7Frame = NesClock.Frame;
            }
        }
        if (InStage)
        {
            if (Sec != lastSection)
            {
                lastSection = Sec;
                Mark("section " + Sec);
            }
            var b = Boss();
            if (b != null && !b.dormant && bossWakeTick < 0 && InMap2)
            {
                bossWakeTick = W.TickCount;
                Mark("boss thức");
                Ev("boss thức");
            }
            if (lockTick < 0 && Sec == "Medusa" && W.CameraCtl != null && W.CameraCtl.Locked)
            {
                lockTick = W.TickCount;
                Mark("khoá camera cx " + Cx);
                Ev("khoá camera");
            }
            var r = StageRules.Current;
            if (tallyTimeTick < 0 && InMap2 && r != null && r.Tally.Phase == eTallyPhase.CountTime)
            {
                tallyTimeTick = W.TickCount;
                Ev("tally đếm TIME");
            }
            if (pilot != null && (Tick % TraceEvery == 0 || (TraceStep != null && pilot.Current.Contains(TraceStep))))
                Trace.Add(Tick + "\t" + pilot.Current + "\t" + Sec + " cx " + Cx + " (" + S.X + "," + S.Y + ") " + S.Fsm.CurState + " f" + S.Facing);
        }
        // hash trạng thái — NGUYÊN công thức Map1RouteProbe.Observe
        uint h = Hash;
        h = Mix(h, (int)gm.State);
        h = Mix(h, gm.IsTransitioning ? 1 : 0);
        var s = GameSession.Current;
        h = Mix(h, s.score); h = Mix(h, s.time); h = Mix(h, s.hearts); h = Mix(h, s.stage); h = Mix(h, s.playerHp);
        h = Mix(h, s.enemyHp); h = Mix(h, s.lives); h = Mix(h, s.whipLevel); h = Mix(h, (int)s.subWeapon); h = Mix(h, s.shotMultiplier);
        if (InStage)
        {
            h = Mix(h, W.TickCount);
            h = Mix(h, Cx);
            h = Mix(h, Sec.Length * 31 + (Sec.Length > 0 ? Sec[0] : 0));
            for (int g = 0; g < Entity.GROUP_COUNT; g++)
            {
                var list = W.GroupList(g);
                h = Mix(h, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    h = Mix(h, list[i].fx);
                    h = Mix(h, list[i].fy);
                }
            }
            var simon = S;
            if (simon != null)
                h = Mix(h, (int)simon.Fsm.CurState);
        }
        Hash = h;
        if (Tick % 500 == 0)
            Checkpoints.Add(Hash);
    }

    private static uint Mix(uint h, int v)
    {
        return (h ^ (uint)v) * 16777619u;
    }

    private static bool TakeShots()
    {
        if (shots == null || shotDir == null)
            return false;
        foreach (var sh in shots)
        {
            if (sh.done)
                continue;
            bool ok;
            try { ok = sh.when(); }
            catch (Exception) { ok = false; }
            if (!ok)
                continue;
            sh.done = true;
            waitFile = Path.Combine(shotDir, sh.name + ".png");
            if (File.Exists(waitFile))
                File.Delete(waitFile);
            ScreenCapture.CaptureScreenshot(waitFile);
            waited = 0;
            ShotsTaken++;
            Mark("ảnh " + sh.name);
            return true;
        }
        return false;
    }

    private static int doorCount;

    private static void OnMilestone(StageFlow f, string m)
    {
        int t = f.World.TickCount;
        string key = m;
        // cửa trái Map 2: #1 ở TEB, #2 ở Hall B (cửa Map 1 ghi khoá riêng — không trùng mốc ảnh)
        string door = Sec == "TowerEntranceB" ? "1" : Sec == "TowerHallB" ? "2" : "_m1_" + (++doorCount);
        if (m == "door")
            key = "door" + door;
        else if (m == "door_stop")
            key = "door_stop" + door;
        else if (m == "ca_from_cb" || m == "cb_from_ca" || m.StartsWith("stage"))
            key = "arr:" + Sec;
        milestoneTick[key] = t;
        Mark("flow " + m);
        Ev("flow " + m + " [" + key + "]");
    }

    private static void OnBossDefeated(BossBase b)
    {
        Mark("boss hạ (+" + b.Score + ")");
        Ev("boss hạ");
    }

    private static void OnCrystalSpawned(BossBase b, ItemPickup p)
    {
        Mark("quả cầu hiện (" + (p != null ? p.X + "," + p.Y : "?") + ")");
        Ev("quả cầu hiện");
    }

    private static void OnCrystalPicked(StageWorld w)
    {
        if (InMap2)
            crystalTick = w.TickCount;
        Mark("nhặt quả cầu");
        Ev("nhặt quả cầu");
    }

    private static void Ev(string m)
    {
        Events.Add(Tick + "\t" + (InStage ? Sec + "\tt" + W.TickCount + "\tcx " + Cx : (GameManager.I != null ? GameManager.I.State.ToString() : "?") + "\tf" + NesClock.Frame + "\t")
                   + "\t" + m);
    }

    private static void Mark(string m)
    {
        var s = GameSession.Current;
        var simon = S;
        Marks.Add(Tick + "\t" + m + "\t" + (InStage ? Sec + " cx " + Cx + " Simon " + (simon != null ? simon.X + "," + simon.Y : "?") + " t" + W.TickCount : (GameManager.I != null ? GameManager.I.State.ToString() : "?"))
                  + "\tSCORE " + s.score.ToString("000000") + " TIME " + s.time.ToString("0000") + " ST " + s.stage.ToString("00") + " ♥" + s.hearts
                  + " P" + s.lives + " HP " + s.playerHp + " E " + s.enemyHp + " roi " + s.whipLevel + " " + s.subWeapon + "×" + s.shotMultiplier);
    }

    public static string Report()
    {
        var sb = new StringBuilder();
        sb.Append(Mode).Append(Running ? " ĐANG CHẠY" : Finished ? " xong" : "").Append(" — tick ").Append(Tick).Append(", hash ").Append(Hash.ToString("x8"))
          .Append(", ảnh ").Append(ShotsTaken).Append(Error.Length > 0 ? ", LỖI: " + Error : "").Append('\n');
        sb.Append("trước STAGE 04: tick ").Append(Stage4Tick).Append(" hash ").Append(PreStage4Hash.ToString("x8")).Append(" (chuẩn Map 1: 8449 / 165b9aef)\n");
        sb.Append("STAGE 07 ở tick ").Append(Stage7Tick).Append('\n');
        sb.Append("checkpoints: ");
        foreach (var c in Checkpoints)
            sb.Append(c.ToString("x8")).Append(' ');
        sb.Append("\nGameObject sau mỗi lần: ");
        foreach (var c in ObjectCounts)
            sb.Append(c).Append(' ');
        sb.Append('\n');
        foreach (var m in Marks)
            sb.Append(m).Append('\n');
        return sb.ToString();
    }

    public static string SaveReport(string path)
    {
        var sb = new StringBuilder(Report());
        sb.Append("\n# sự kiện (tick lộ trình \\t nơi \\t tick thế giới \\t cx \\t sự kiện)\n");
        foreach (var e in Events)
            sb.Append(e).Append('\n');
        sb.Append("\n# vết autopilot\n");
        foreach (var e in Trace)
            sb.Append(e).Append('\n');
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        return path;
    }

    // ---------------------------------------------------------------- autopilot (chỉ dùng khi GHI / THỬ)

    /// <summary>
    /// Lái Simon qua Map 2 theo đường đi video (map2_simon.csv) bằng chuỗi bước: đi tới x, nhảy (40 tick, cao 36 — ConfigSimon; lên bậc 32 px khi
    /// cách mép 19–37 px), leo thang, cưỡi bệ di động (chờ bệ chạm bờ), đánh Queen Medusa phản xạ. Chỉ đọc trạng thái công khai; mọi quyết định
    /// thành bitmask nút ⇒ NesPad ghi ⇒ replay tái hiện y hệt.
    /// </summary>
    private class Pilot
    {
        const int U = 1 << (int)eNesButton.Up, D = 1 << (int)eNesButton.Down, L = 1 << (int)eNesButton.Left, R = 1 << (int)eNesButton.Right,
                  A = 1 << (int)eNesButton.A, B = 1 << (int)eNesButton.B;

        private delegate int StepFn(int age);   // trả mask; −1 = bước xong

        private readonly List<KeyValuePair<string, StepFn>> steps = new List<KeyValuePair<string, StepFn>>();
        private int index, age, lastWhip = -100, ridePhase;
        public string Failed;
        public int Age => age;

        public Pilot(int startStage)
        {
            // ---- STAGE 04: TEA — thang ↗ chân (112,176) lên bục y 112 ⇒ nhảy trái lên sàn y 80 ⇒ thang ↗ (80,80) cắt đỉnh ⇒ TEB
            Add("S4", a => -1);
            Add("chờ TEA", a => Ready("TowerEntranceA") ? -1 : 0);
            Add("tới chân thang 1 TEA", a => WalkTo(StairFoot("TowerEntranceA_stair1")));
            Add("leo thang 1 TEA", a => S.Y == 112 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("tới mép bục TEA", a => WalkTo(186));
            Add("quay trái", a => a == 0 ? L : -1);
            Add("nhảy lên sàn y 80", a => Jump(a, L));
            Add("tới chân thang 0 TEA", a => WalkTo(StairFoot("TowerEntranceA_stair0")));
            Add("leo thang 0 ⇒ TEB", a => Sec == "TowerEntranceB" ? -1 : U);
            // ---- TEB: leo tiếp lên bục y 112 ⇒ nhảy trái lên y 80 ⇒ trái: nhảy xuống khối y 112 ⇒ nhảy lên y 80 ⇒ nhảy khe ⇒ bệ ⇒ cửa trái
            Add("chờ TEB", a => Ready("TowerEntranceB") ? -1 : 0);
            Add("leo hết thang TEB", a => S.Y == 112 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("tới mép bục TEB", a => WalkTo(470));
            Add("quay trái TEB", a => a == 0 ? L : -1);
            Add("nhảy lên y 80 TEB", a => Jump(a, L));
            Add("tới khối X TEB", a => WalkTo(360));
            Add("nhảy khe rộng TEB", a => Jump(a, L));
            Add("tới mép khối y 112", a => WalkTo(310));
            Add("nhảy lên y 80 (cột 17)", a => Jump(a, L));
            Add("tới mép khe 2 TEB", a => WalkTo(196));
            Add("nhảy khe 2 TEB", a => Jump(a, L));
            Add("tới bờ bệ TEB", a => WalkTo(100));
            Add("cưỡi bệ TEB", a => RideLeft(a, 64, 32));
            Add("cửa trái ⇒ Hall A", a => Sec == "TowerHallA" ? -1 : (Flow.Busy ? 0 : WalkHold(L)));
            // ---- STAGE 05: Hall A — trái: nhảy lên 2 khối nhô (hố dưới) ⇒ xuống ⇒ nhảy lên gờ trái y 48 ⇒ thang ↗ cắt đỉnh ⇒ Hall B
            Add("S5", a => -1);
            Add("chờ Hall A", a => Ready("TowerHallA") ? -1 : 0);
            Add("tới trước khối nhô", a => WalkTo(342));
            Add("nhảy lên khối nhô", a => Jump(a, L));
            Add("đi khỏi khối nhô", a => WalkTo(276));
            Add("tới trước gờ trái", a => WalkTo(84));
            Add("nhảy lên gờ y 48", a => Jump(a, L));
            Add("tới chân thang Hall A", a => WalkTo(StairFoot("TowerHallA_stair0")));
            Add("leo ⇒ Hall B", a => Sec == "TowerHallB" ? -1 : U);
            // ---- Hall B: lối dưới sang phải (4 khe) ⇒ khối y 112 ⇒ lối trên y 80 sang trái (bệ 2 nhảy, bệ 1 cưỡi) ⇒ rơi xuống gờ y 112 ⇒ cửa trái
            Add("chờ Hall B", a => Ready("TowerHallB") ? -1 : 0);
            Add("leo hết thang Hall B", a => S.Y == 160 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("tới khe 1 Hall B", a => WalkTo(124));
            Add("nhảy khe 1", a => Jump(a, R));
            Add("tới khe 2 Hall B", a => WalkTo(190));
            Add("nhảy khe 2 (lên y 144)", a => Jump(a, R));
            Add("tới khe 3 Hall B", a => WalkTo(284));
            Add("nhảy khe 3", a => Jump(a, R));
            Add("tới khe 4 Hall B", a => WalkTo(380));
            Add("nhảy khe 4", a => Jump(a, R));
            Add("tới trước khối y 112", a => WalkTo(567));   // video f3358 x 565–567: đứng sát khối (đầu đã dưới mái) rồi nhảy
            Add("nhảy lên khối y 112", a => Jump(a, R));
            Add("tới mép phải khối", a => WalkTo(601));
            Add("quay trái Hall B", a => a == 0 ? L : -1);
            Add("nhảy lên lối trên y 80", a => Jump(a, L));
            Add("tới bờ bệ 2", a => WalkTo(486));
            Add("chờ bệ 2 về phải", a => PlatformLeft(432) >= 439 ? -1 : 0);
            Add("nhảy lên bệ 2", a => Jump(a, L));
            Add("cưỡi bệ 2 tới trái", a => PlatformLeft(432) <= 393 ? -1 : 0);
            Add("nhảy khỏi bệ 2", a => Jump(a, L));
            Add("tới bờ bệ 1", a => WalkTo(358));
            Add("cưỡi bệ 1", a => RideLeft(a, 304, 256));
            Add("cửa trái ⇒ Hall C", a => Sec == "TowerHallC" ? -1 : (Flow.Busy ? 0 : WalkHold(L)));
            // ---- STAGE 06: Hall C — trái: rơi y 128 ⇒ y 176 ⇒ nhảy lên y 144 ⇒ dưới 3 bẫy ép ⇒ khối y 128 ⇒ thang ngắn ⇒ y 80 ⇒ y 48 ⇒ phải ⇒ thang ⇒ Medusa
            Add("S6", a => -1);
            Add("chờ Hall C", a => Ready("TowerHallC") ? -1 : 0);
            Add("xuống đáy y 176", a => WalkTo(538));
            Add("nhảy lên sàn y 144", a => Jump(a, L));
            Add("tới trước khối y 128", a => WalkTo(176));
            Add("nhảy lên khối y 128", a => Jump(a, L));
            Add("tới chân thang ngắn", a => WalkTo(StairFoot("TowerHallC_stair0")));
            Add("leo thang ngắn", a => S.Y == 96 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("tới mép y 96", a => WalkTo(206));
            Add("nhảy lên y 80", a => Jump(a, R));
            Add("tới trước lối trên", a => WalkTo(230));
            Add("nhảy lên lối trên y 48", a => Jump(a, R));
            Add("tới chân thang Medusa", a => WalkTo(StairFoot("TowerHallC_stair1")));
            Add("leo ⇒ Medusa", a => Sec == "Medusa" ? -1 : U);
            // ---- sảnh Medusa: leo lên y 144 ⇒ nhảy trái lên y 128 ⇒ trái tới phòng boss (khoá cx 0) ⇒ đứng x 61 ⇒ đánh ⇒ quả cầu
            Add("chờ Medusa", a => Ready("Medusa") ? -1 : 0);
            Add("leo hết thang Medusa", a => S.Y == 144 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("quay trái Medusa", a => a == 0 ? L : -1);
            Add("nhảy lên y 128", a => Jump(a, L));
            Add("tới phòng boss", a => WalkTo(61));
            Add("chờ Medusa thức", a => BossAwake() ? -1 : 0);
            Add("đánh Queen Medusa", a => Fight());
            Add("nhặt quả cầu", a => StageRules.Current != null && StageRules.Current.Tally.Active ? -1 : Crystal());
            Add("chờ hết", a => 0);
            if (startStage > 4)
                index = steps.FindIndex(kv => kv.Key == "S" + startStage);
        }

        private void Add(string name, StepFn fn)
        {
            steps.Add(new KeyValuePair<string, StepFn>(name, fn));
        }

        public string Current => index < steps.Count ? steps[index].Key : "hết";

        public int Decide()
        {
            while (index < steps.Count)
            {
                int m;
                try { m = steps[index].Value(age); }
                catch (Exception e) { Failed = steps[index].Key + ": " + e.Message; return 0; }
                if (m >= 0)
                {
                    age++;
                    if (age > (steps[index].Key.StartsWith("đánh") ? 20000 : 6000))
                        Failed = "kẹt ở bước '" + steps[index].Key + "'";
                    if (S != null && S.IsDying)
                        Failed = "Simon chết ở bước '" + steps[index].Key + "' (" + S.X + "," + S.Y + ")";
                    return m;
                }
                Trace.Add(Tick + "\t✓ " + steps[index].Key + (S != null && W != null ? "\t" + Sec + " (" + S.X + "," + S.Y + ") t" + W.TickCount : ""));
                index++;
                age = 0;
                ridePhase = 0;
            }
            return 0;
        }

        private static bool Ready(string sec)
        {
            return InStage && Sec == sec && Flow != null && !Flow.Busy && S != null && S.Fsm != null;
        }

        /// <summary>x chân thang (điểm Simon đứng để bấm Lên): StairPath.bottom.x + lệch Simon.</summary>
        private static int StairFoot(string id)
        {
            var tiles = W.Tiles;
            for (int i = 0; i < tiles.StairCount; i++)
            {
                var p = tiles.GetStair(i);
                if (p.stair != null && p.stair.id == id)
                    return p.bottom.x + S.StairOffset(p);
            }
            throw new Exception("không có thang " + id);
        }

        /// <summary>Giữ hướng tới khi X đúng bằng x (1 px/tick ⇒ dừng đúng chỗ). Tune[tên bước] (nếu có) thay x.</summary>
        private int WalkTo(int x)
        {
            int tx;
            if (Tune.TryGetValue(Current, out tx))
                x = tx;
            var s = S;
            if (s == null || Flow.Busy)
                return 0;
            if (!s.OnGround() || s.Fsm.Is(eSimonState.Land) || s.Fsm.Is(eSimonState.Attack) || s.OnStairs)
                return s.X == x && s.OnGround() && !s.OnStairs ? -1 : 0;
            if (s.X == x)
                return -1;
            return s.X < x ? R : L;
        }

        private static int WalkHold(int dir)
        {
            var s = S;
            return s != null && s.OnGround() ? dir : 0;
        }

        private int jumpAt = -1;

        /// <summary>Nhảy theo hướng dir: chờ đứng vững (Idle/Walk) ⇒ A+dir; giữ dir tới khi đáp đất.</summary>
        private int Jump(int a, int dir)
        {
            var s = S;
            if (a == 0)
                jumpAt = -1;
            if (jumpAt < 0)
            {
                if (s.OnGround() && (s.Fsm.Is(eSimonState.Idle) || s.Fsm.Is(eSimonState.Walk)))
                {
                    jumpAt = a;
                    return A | dir;
                }
                return 0;
            }
            if (a - jumpAt < 4)
                return dir;
            return s.OnGround() && !s.Fsm.Is(eSimonState.Jump) && !s.Fsm.Is(eSimonState.Fall) ? -1 : dir;
        }

        private static MovingPlatform Platform(int centerX)
        {
            var list = W.Platforms;
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && list[i].Alive && list[i].CenterX == centerX)
                    return list[i];
            return null;
        }

        /// <summary>Mép trái hiện tại của bệ có tâm dao động centerX (không thấy ⇒ −9999).</summary>
        private static int PlatformLeft(int centerX)
        {
            var p = Platform(centerX);
            return p != null ? p.Left : -9999;
        }

        /// <summary>
        /// Cưỡi bệ chạm 2 bờ sang trái: chờ bệ ở cực phải (mép trái ≥ max − 1) ⇒ đi trái tới tâm bệ ⇒ đứng yên tới cực trái (mép trái ≤ min + 1)
        /// (bước sau đi tiếp sang bờ trái).
        /// </summary>
        private int RideLeft(int a, int centerX, int minLeft)
        {
            var p = Platform(centerX);
            if (p == null)
                return 0;
            int maxLeft = p.CenterX + p.Amplitude - p.Width / 2;
            var s = S;
            switch (ridePhase)
            {
                case 0:
                    if (p.Left >= maxLeft - 1)
                        ridePhase = 1;
                    return 0;
                case 1:
                    if (s.X <= p.X + 2)
                    {
                        ridePhase = 2;
                        return 0;
                    }
                    return L;
                default:
                    return p.Left <= minLeft + 1 ? -1 : 0;   // bệ chạm bờ trái ⇒ bước sau tự đi
            }
        }

        private static bool BossAwake()
        {
            var b = Boss();
            return b != null && !b.dormant;
        }

        /// <summary>Đánh Queen Medusa trên sàn phòng boss (bất tử): quay về phía Medusa + roi khi trong tầm; xa ⇒ đi theo trong [32, 200].</summary>
        private int Fight()
        {
            var s = S;
            var b = Boss();
            if (b == null || b.defeated || b.dying || !b.Alive)
                return -1;
            if (s.Fsm.Is(eSimonState.Attack) || !s.OnGround())
                return 0;
            int dx = b.X - s.X, dy = b.Y - s.Y;
            int dir = dx >= 0 ? R : L;
            bool band = dy >= -56 && dy <= 8;
            int t = W.TickCount;
            if (band && Math.Abs(dx) <= 60 && t - lastWhip > 24)
            {
                lastWhip = t;
                return B | ((dx >= 0) != (s.Facing > 0) ? dir : 0);
            }
            if (Math.Abs(dx) > 40)
            {
                if (dx > 0 && s.X < 200)
                    return R;
                if (dx < 0 && s.X > 32)
                    return L;
            }
            return 0;
        }

        private static int Crystal()
        {
            var s = S;
            foreach (var e in W.GroupList(eEntityGroup.Item))
            {
                var it = e as ItemPickup;
                if (it != null && it.Alive && it.id == eItemId.MagicCrystal)
                {
                    if (!s.OnGround())
                        return 0;
                    return it.X > s.X ? R : it.X < s.X ? L : 0;
                }
            }
            return 0;
        }
    }
}
#endif
