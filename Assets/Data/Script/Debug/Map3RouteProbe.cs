#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Harness lộ trình Map 3 (CVM3 11; khuôn Map2RouteProbe) — CHỈ Editor, scene Main.unity khi Play. Qua MCP:
///   Map3RouteProbe.Record(tpu)            ⇒ phát map1_route → map2_route (tới tick màn STAGE 07 bật), rồi AUTOPILOT Map 3 (Pilot dưới — chỉ đọc trạng
///                                            thái, bấm nút qua NesPad.Inject) tới STAGE 10 stub, ghi input từ tick STAGE 07 ⇒ Assets/Data/Tests/Replays/map3_route.txt
///   Map3RouteProbe.Replay(shotDir, tpu)   ⇒ map1_route → map2_route → map3_route (NesPad.PlayReplay), hash trạng thái từng tick CÙNG công thức
///                                            Map1RouteProbe (từ tick 1 Stage 01 ⇒ hash cuối = hash luồng thật Title → STAGE 10 của Map1AcceptProbe.StartFlow),
///                                            "trước STAGE 04" = PRE4, "trước STAGE 07" = PRE7, mốc luồng, vết từng tick Map 3 (đo ±2 tick), ảnh mốc
///   Map3RouteProbe.Try(stage, tpu)        ⇒ THỬ autopilot từ đầu stage 7/8/9 (session giả = cuối Map 2, KHÔNG ghi file) để chỉnh lộ trình
///   Map3RouteProbe.Report() / SaveReport(path) / SaveTrace(path)
/// Chạy: NesClock TẮT, tự gọi NesClock.Step tpu tick mỗi vòng EditorApplication.update.
/// BẤT TỬ: Map 1 + Map 2 bật suốt (như map1/map2_route); Map 3 CHỈ ở đoạn khó (InvincibleSections: WEB — 4 hố + Raven + Skeleton; WalkwayA —
/// Medusa Head bay sóng + Skeleton ném xương trên cầu (lần thử không bất tử: autopilot chết ở x 508, HP 13 → 0); WalkwayB — 2 khe cầu + Bone Tower +
/// Raven; MummyMan — cầu hỏng 3 khe + Raven/Medusa Head/Bone Tower + 2 Mummy) — WEA (Fleaman, Ghost, Skeleton) KHÔNG bất tử (Simon trúng đòn 3 vạch
/// + knockback CVM3 04; autopilot quất phản xạ). Luật này (ApplyCheat) chạy CÙNG chỗ (trước bước thế giới mỗi tick) ở Replay và ở
/// Map1AcceptProbe.StartFlow ⇒ cùng hash.
/// </summary>
public static class Map3RouteProbe
{
    public const string REPLAY = "Assets/Data/Tests/Replays/map3_route.txt";
    public const int MAX_TICKS = 80000;
    /// <summary>Số tick chạy sau khi STAGE 10 stub hiện rồi dừng.</summary>
    public const int TAIL_TICKS = 60;
    /// <summary>Section Map 3 có bật bất tử khi chạy lộ trình (đoạn khó — hố/cầu hỏng/phòng boss).</summary>
    public static readonly string[] InvincibleSections = { "WalkwayEntranceB", "WalkwayA", "WalkwayB", "MummyMan" };

    public static bool Running { get; private set; }
    public static bool Finished { get; private set; }
    public static string Mode { get; private set; } = "";
    public static string Error { get; private set; } = "";
    public static int Tick { get; private set; }
    public static uint Hash { get; private set; }
    public static uint PreStage4Hash { get; private set; }
    public static uint PreStage7Hash { get; private set; }
    public static int Stage4Tick { get; private set; } = -1;
    public static int Stage7Tick { get; private set; } = -1;
    public static int Stage10Tick { get; private set; } = -1;
    public static int ShotsTaken { get; private set; }
    public static int TicksPerUpdate = 40;
    public static int TraceEvery = 30;
    public static string TraceStep;
    public static readonly Dictionary<string, int> Tune = new Dictionary<string, int>();
    public static readonly List<string> Marks = new List<string>();
    public static readonly List<uint> Checkpoints = new List<uint>();
    public static readonly List<int> ObjectCounts = new List<int>();
    public static readonly List<string> Trace = new List<string>();
    public static readonly List<string> Events = new List<string>();
    /// <summary>Vết TỪNG tick phần Map 3 (tick lộ trình, màn, section, tick thế giới, cx, Simon x/y/state, TIME, STAGE, pha flow) — đo chuyển cảnh.</summary>
    public static readonly List<string> TickLog = new List<string>();

    private static MethodInfo stepMethod;
    private static Pilot pilot;
    private static bool prevInvincible;
    private static eGameScreen lastScreen;
    private static string lastSection = "";
    private static List<int> map2, map3;
    private static bool switched2, switched3, tryMode;
    private static int tryStage;
    private static List<Shot> shots;
    private static string shotDir, waitFile;
    private static int waited;
    private static int lockTick = -1, wakeTick = -1, mapEnterFrame = -1, stage10Frame = -1, crystalTick = -1, crystalSpawnTick = -1, tallyTimeTick = -1;
    private static readonly Dictionary<string, int> milestoneTick = new Dictionary<string, int>();

    private static StageWorld W => StageWorld.Current;
    private static SimonController S => W != null ? W.Player as SimonController : null;
    private static int Cx => W != null && W.CameraCtl != null ? W.CameraCtl.X : -1;
    private static string Sec => W != null && W.Section != null ? W.Section.name : "";
    private static bool InStage => GameManager.I != null && GameManager.I.State == eGameScreen.Stage1 && !GameManager.I.IsTransitioning && W != null;
    private static StageFlow Flow => W != null ? W.GetComponent<StageFlow>() : null;
    private static bool InMap3 => tryMode || Stage7Tick >= 0;

    // ---------------------------------------------------------------- bất tử theo đoạn (dùng chung với Map1AcceptProbe.StartFlow)

    /// <summary>
    /// Đặt cờ bất tử cho tick sắp chạy: trước Map 3 (map3Started false) ⇒ bật (như lộ trình Map 1/2); trong Map 3 ⇒ bật khi đang ở màn chơi trong
    /// section thuộc InvincibleSections, tắt ở WEA/WalkwayA; ngoài màn chơi (đen/bản đồ/STAGE 10) ⇒ bật. Ghi cả EditorPrefs khi đổi (StageWorld
    /// OnEnable đọc lại EditorPrefs — CvgpDebug.LoadPrefs).
    /// </summary>
    public static void ApplyCheat(bool map3Started)
    {
        bool want = true;
        if (map3Started && InStage)
            want = Array.IndexOf(InvincibleSections, Sec) >= 0;
        if (CvgpDebug.CheatInvincible != want || EditorPrefs.GetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, false) != want)
        {
            CvgpDebug.CheatInvincible = want;
            EditorPrefs.SetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, want);
        }
    }

    // ---------------------------------------------------------------- điểm chụp (mốc luồng — video Map_3 ghép bằng Tools/cvm3/pairs11m3.py)

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

    private static List<Shot> BuildShots()
    {
        var l = new List<Shot>
        {
            new Shot("map2_r100", () => mapEnterFrame >= 0 && !InMap3 && GameSession.Current.stage == 7 && GameManager.I.State == eGameScreen.CastleMap && NesClock.Frame - mapEnterFrame == 100),
            new Shot("map2_r211", () => mapEnterFrame >= 0 && !InMap3 && GameSession.Current.stage == 7 && GameManager.I.State == eGameScreen.CastleMap && NesClock.Frame - mapEnterFrame == 211),
            new Shot("map2_r300", () => mapEnterFrame >= 0 && !InMap3 && GameSession.Current.stage == 7 && GameManager.I.State == eGameScreen.CastleMap && NesClock.Frame - mapEnterFrame == 300),
            new Shot("st07_r001", () => InStage && GameSession.Current.stage == 7 && Sec == "WalkwayEntranceA" && Stage7Tick >= 0 && Tick == Stage7Tick + 1),
            new Shot("st07_r040", () => InStage && GameSession.Current.stage == 7 && Sec == "WalkwayEntranceA" && Stage7Tick >= 0 && Tick == Stage7Tick + 40),
            new Shot("web_r001", () => AtMs("arr:WalkwayEntranceB", 1)),
            new Shot("web_r010", () => AtMs("arr:WalkwayEntranceB", 10)),
            new Shot("wb_r001", () => AtMs("arr:WalkwayB", 1)),
            new Shot("wb_r010", () => AtMs("arr:WalkwayB", 10)),
        };
        foreach (var d in new[] { "1", "2" })
        {
            string k = d;
            l.Add(new Shot("door" + k + "_scroll1_r064", () => AtMs("door" + k, 64)));
            foreach (int r in new[] { 4, 10, 40, 80, 104, 109, 130 })
            {
                int rr = r;
                l.Add(new Shot("door" + k + "_stop_r" + rr.ToString("000"), () => AtMs("door_stop" + k, rr)));
            }
        }
        l.Add(new Shot("door1_scroll2_r064", () => AtMs("door_stop1", Map3FlowTiming.DOOR_TO_WALKWAY_A.scroll2 + 64)));
        l.Add(new Shot("door2_scroll2_r064", () => AtMs("door_stop2", Map3FlowTiming.DOOR_TO_MUMMY_MAN.scroll2 + 64)));
        l.Add(new Shot("wa_r002", () => AtMs("arr:WalkwayA", 2)));
        l.Add(new Shot("mm_r002", () => AtMs("arr:MummyMan", 2)));
        l.Add(new Shot("lock_r000", () => lockTick >= 0 && InStage && W.TickCount == lockTick));
        l.Add(new Shot("wake_r002", () => wakeTick >= 0 && InStage && W.TickCount == wakeTick + 2));
        l.Add(new Shot("crystal_r010", () => crystalSpawnTick >= 0 && InStage && W.TickCount == crystalSpawnTick + 10));
        l.Add(new Shot("pickup_r030", () => crystalTick >= 0 && InStage && W.TickCount == crystalTick + 30));
        l.Add(new Shot("tally_r040", () => tallyTimeTick >= 0 && InStage && W.TickCount == tallyTimeTick + 40));
        l.Add(new Shot("map3_r060", () => mapEnterFrame >= 0 && InMap3 && GameManager.I.State == eGameScreen.CastleMap && NesClock.Frame - mapEnterFrame == 60));
        l.Add(new Shot("map3_r160", () => mapEnterFrame >= 0 && InMap3 && GameManager.I.State == eGameScreen.CastleMap && NesClock.Frame - mapEnterFrame == 160));
        l.Add(new Shot("map3_r300", () => mapEnterFrame >= 0 && InMap3 && GameManager.I.State == eGameScreen.CastleMap && NesClock.Frame - mapEnterFrame == 300));
        l.Add(new Shot("st10_r006", () => stage10Frame >= 0 && GameManager.I.State == eGameScreen.Stage10Stub && NesClock.Frame - stage10Frame == 6));
        return l;
    }

    private static List<MummyMan> Mummies()
    {
        var l = new List<MummyMan>();
        if (W == null)
            return l;
        var list = W.GroupList(eEntityGroup.Enemy);
        for (int i = 0; i < list.Count; i++)
        {
            var m = list[i] as MummyMan;
            if (m != null && m.Alive)
                l.Add(m);
        }
        return l;
    }

    // ---------------------------------------------------------------- chạy

    public static string Record(int tpu)
    {
        return Start(false, tpu, null, 0);
    }

    public static string Replay(string dir, int tpu)
    {
        if (!File.Exists(REPLAY))
            return "thiếu " + REPLAY + " — chạy Record trước";
        map3 = NesReplay.Decode(File.ReadAllText(REPLAY));
        return Start(true, tpu, dir, 0);
    }

    public static string Try(int stage, int tpu)
    {
        return Start(false, tpu, null, Mathf.Clamp(stage, 7, 9));
    }

    private static string Start(bool replay, int tpu, string dir, int devStage)
    {
        if (!EditorApplication.isPlaying || GameManager.I == null || NesClock.I == null || NesPad.I == null)
            return "cần Play Main.unity";
        if (Running || Map1RouteProbe.Running || Map2RouteProbe.Running || Map1AcceptProbe.Running)
            return "đang chạy";
        if (!File.Exists(Map1RouteProbe.REPLAY) || !File.Exists(Map2RouteProbe.REPLAY))
            return "thiếu map1_route / map2_route";
        TicksPerUpdate = Mathf.Max(1, tpu);
        tryMode = devStage > 0;
        tryStage = devStage;
        Mode = replay ? "replay" : tryMode ? "try" + devStage : "record";
        Error = "";
        Finished = false;
        Tick = 0;
        Hash = 2166136261u;
        PreStage4Hash = PreStage7Hash = 0;
        Stage4Tick = Stage7Tick = Stage10Tick = -1;
        ShotsTaken = 0;
        Marks.Clear();
        Checkpoints.Clear();
        Trace.Clear();
        Events.Clear();
        TickLog.Clear();
        milestoneTick.Clear();
        lastSection = "";
        switched2 = switched3 = false;
        lockTick = wakeTick = mapEnterFrame = stage10Frame = crystalTick = crystalSpawnTick = tallyTimeTick = -1;
        shotDir = dir;
        waitFile = null;
        shots = dir != null ? BuildShots() : new List<Shot>();
        if (dir != null)
            Directory.CreateDirectory(dir);
        if (!replay)
            map3 = null;
        map2 = NesReplay.Decode(File.ReadAllText(Map2RouteProbe.REPLAY));
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
            // session giả = trạng thái lúc STAGE 07 của lộ trình Map 1 + 2 (Map2RouteProbe tick 16663: SCORE 13510, P3, roi 3, dao ×1, tim 5)
            var s = GameSession.Current;
            s.score = 13510;
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
        {
            switched2 = true;
            SwitchToMap3();
        }
        else
            NesPad.I.PlayReplay(NesReplay.Decode(File.ReadAllText(Map1RouteProbe.REPLAY)));
        Running = true;
        EditorApplication.update += Pump;
        Mark("start " + Mode);
        return Mode + " bắt đầu";
    }

    private static void SwitchToMap3()
    {
        switched3 = true;
        if (map3 != null)
        {
            NesPad.I.PlayReplay(map3);
            Mark("phát map3_route (" + map3.Count + " tick)");
            return;
        }
        NesPad.I.StopReplay();
        pilot = new Pilot(tryMode ? tryStage : 7);
        if (!tryMode)
            NesPad.I.StartRecord();
        Mark("autopilot Map 3");
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
        sb.Append("# CVM3 11 map3_route: lộ trình Map 3 từ tick màn STAGE 07 bật (sau map1_route + map2_route — Map3RouteProbe phát 2 lộ trình trước) tới ")
          .Append("STAGE 10 stub — ghi bằng autopilot Map3RouteProbe.Pilot; BẤT TỬ chỉ ở WEB / WalkwayA / WalkwayB / MummyMan (Map3RouteProbe.InvincibleSections), ")
          .Append("WEA không bất tử — phát lại: Map3RouteProbe.Replay\n");
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
            // tick màn đen cuối trước STAGE 04 / STAGE 07 (bản đồ lâu đài → StageScreen): NesPad.Tick của tick này đọc phần tử 0 của lộ trình kế
            if (!switched2 && gm.IsTransitioning && PendingNext() == eGameScreen.Stage1 && BlackLeft() == 1 && GameSession.Current.stage == 4)
            {
                switched2 = true;
                NesPad.I.PlayReplay(map2);
                Mark("phát map2_route (" + map2.Count + " tick)");
            }
            if (switched2 && !switched3 && gm.IsTransitioning && PendingNext() == eGameScreen.Stage1 && BlackLeft() == 1 && GameSession.Current.stage == 7)
                SwitchToMap3();
            ApplyCheat(switched3);
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
            else if (switched3 && Mode == "replay" && !NesPad.I.IsReplaying && Stage10Tick < 0)
            {
                Stop("hết map3_route trước STAGE 10");
                return;
            }
            else if (!switched3 && !NesPad.I.IsReplaying)
            {
                Stop(switched2 ? "hết map2_route trước STAGE 07" : "hết map1_route trước STAGE 04");
                return;
            }
            stepMethod.Invoke(NesClock.I, null);
            Tick++;
            Observe();
            if (Stage10Tick >= 0 && Tick - Stage10Tick >= TAIL_TICKS)
            {
                Stop(null);
                return;
            }
            if (TakeShots())
                return;
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
            if (Map1RouteProbe.IsStage7Screen(gm.State) && Stage7Tick < 0)
            {
                Stage7Tick = Tick;
                PreStage7Hash = Hash;
            }
            if (gm.State == eGameScreen.CastleMap)
                mapEnterFrame = NesClock.Frame;
            if (gm.State == eGameScreen.Stage10Stub && Stage10Tick < 0)
            {
                Stage10Tick = Tick;
                stage10Frame = NesClock.Frame;
            }
        }
        var s = GameSession.Current;
        if (InStage)
        {
            if (Sec != lastSection)
            {
                lastSection = Sec;
                Mark("section " + Sec);
            }
            var ms = Mummies();
            if (wakeTick < 0 && InMap3)
                foreach (var m in ms)
                    if (!m.dormant)
                    {
                        wakeTick = W.TickCount;
                        Mark("Mummy thức");
                        Ev("Mummy thức");
                        break;
                    }
            if (lockTick < 0 && Sec == "MummyMan" && W.CameraCtl != null && W.CameraCtl.Locked)
            {
                lockTick = W.TickCount;
                Mark("khoá camera cx " + Cx);
                Ev("khoá camera");
            }
            var r = StageRules.Current;
            if (tallyTimeTick < 0 && InMap3 && r != null && r.Tally.Phase == eTallyPhase.CountTime)
            {
                tallyTimeTick = W.TickCount;
                Ev("tally đếm TIME");
            }
            if (pilot != null && (Tick % TraceEvery == 0 || (TraceStep != null && pilot.Current.Contains(TraceStep))))
                Trace.Add(Tick + "\t" + pilot.Current + "\t" + Sec + " cx " + Cx + " (" + S.X + "," + S.Y + ") " + S.Fsm.CurState + " f" + S.Facing + " HP " + s.playerHp);
        }
        if (InMap3 && switched3)
        {
            var f = Flow;
            var simon = S;
            TickLog.Add(Tick + "\t" + gm.State + (gm.IsTransitioning ? "*" : "") + "\t" + (InStage ? Sec : "-") + "\t" + (InStage ? W.TickCount : -1) + "\t" + (InStage ? Cx : -1)
                        + "\t" + (InStage && simon != null ? simon.X + "\t" + simon.Y + "\t" + simon.Fsm.CurState : "-\t-\t-")
                        + "\t" + s.time + "\t" + s.stage + "\t" + (f != null && InStage ? f.Phase.ToString() : "-") + "\t" + s.hearts + "\t" + s.score + "\t" + s.playerHp
                        + "\t" + (StageRules.Current != null && InStage ? StageRules.Current.Tally.Phase.ToString() : "-"));
        }
        // hash trạng thái — NGUYÊN công thức Map1RouteProbe.Observe
        uint h = Hash;
        h = Mix(h, (int)gm.State);
        h = Mix(h, gm.IsTransitioning ? 1 : 0);
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
            Ev("ảnh " + sh.name);
            return true;
        }
        return false;
    }

    private static void OnMilestone(StageFlow f, string m)
    {
        int t = f.World.TickCount;
        string key = m;
        string door = Sec == "WalkwayEntranceB" ? "1" : Sec == "WalkwayB" ? "2" : "_x";
        if (m == "door")
            key = "door" + door;
        else if (m == "door_stop")
            key = "door_stop" + door;
        else if (m == "door_open")
            key = "door_open" + door;
        else if (m == "door_closed")
            key = "door_closed" + door;
        else if (m == "ca_from_cb" || m == "cb_from_ca" || m.StartsWith("stage"))
            key = "arr:" + Sec;
        if (InMap3)
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
        if (InMap3 && W != null)
            crystalSpawnTick = W.TickCount;
        Mark("quả cầu hiện (" + (p != null ? p.X + "," + p.Y : "?") + ")");
        Ev("quả cầu hiện");
    }

    private static void OnCrystalPicked(StageWorld w)
    {
        if (InMap3)
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
        sb.Append("trước STAGE 04: tick ").Append(Stage4Tick).Append(" hash ").Append(PreStage4Hash.ToString("x8")).Append(" (chuẩn ").Append(RegressionRunner.PRE4).Append(")\n");
        sb.Append("trước STAGE 07: tick ").Append(Stage7Tick).Append(" hash ").Append(PreStage7Hash.ToString("x8")).Append(" (chuẩn PRE7 ").Append(RegressionRunner.PRE7).Append(")\n");
        sb.Append("STAGE 10 ở tick ").Append(Stage10Tick).Append('\n');
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

    /// <summary>Vết từng tick phần Map 3 (TSV) — Tools/cvm3/flow11_compare.py so mốc chuyển cảnh với video.</summary>
    public static string SaveTrace(string path)
    {
        var sb = new StringBuilder("tick\tscreen\tsection\twtick\tcx\tx\ty\tstate\ttime\tstage\tflow\thearts\tscore\thp\ttally\n");
        foreach (var e in TickLog)
            sb.Append(e).Append('\n');
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        return path;
    }

    // ---------------------------------------------------------------- autopilot (chỉ dùng khi GHI / THỬ)

    /// <summary>
    /// Lái Simon qua Map 3 theo đường đi video (map3_simon.csv) + lưới va chạm Block 3 (CVM3 05 — Cvm3TerrainProbe): đi tới x, nhảy (lên bậc
    /// 16–32 px / qua khe 32 px), leo thang (zig-zag WEB + WalkwayA, ↖ cắt đỉnh ⇒ đổi tầng), xuống thang WEB stair4, quất phản xạ ở đoạn KHÔNG bất tử
    /// (WEA, WalkwayA), đánh 2 Mummy (bất tử), nhặt quả cầu. Chỉ đọc trạng thái công khai; mọi quyết định thành bitmask nút ⇒ NesPad ghi ⇒ replay tái hiện.
    /// </summary>
    private class Pilot
    {
        const int U = 1 << (int)eNesButton.Up, D = 1 << (int)eNesButton.Down, L = 1 << (int)eNesButton.Left, R = 1 << (int)eNesButton.Right,
                  A = 1 << (int)eNesButton.A, B = 1 << (int)eNesButton.B;

        private delegate int StepFn(int age);

        private readonly List<KeyValuePair<string, StepFn>> steps = new List<KeyValuePair<string, StepFn>>();
        private int index, age, lastWhip = -100;
        public string Failed;
        public int Age => age;

        public Pilot(int startStage)
        {
            // ---- STAGE 07: WEA — trái xuống sàn y 176 ⇒ bậc y 160 ⇒ bờ y 128 ⇒ quay phải nhảy lên y 96 ⇒ thang ↖ 12 bậc cắt đỉnh ⇒ WEB (KHÔNG bất tử)
            Add("S7", a => -1);
            Add("chờ WEA", a => Ready("WalkwayEntranceA") ? -1 : 0);
            Add("đi trái tới trước bậc y 160", a => Rx(WalkTo(140)));
            Add("nhảy lên bậc y 160", a => Jump(a, L));
            Add("tới trước bờ y 128", a => Rx(WalkTo(72)));
            Add("nhảy lên bờ y 128", a => Jump(a, L));
            Add("tới mép bờ x 60", a => Rx(WalkTo(60)));
            Add("quay phải WEA", a => a == 0 ? R : -1);
            Add("nhảy lên y 96", a => Jump(a, R));
            Add("tới chân thang WEA", a => Rx(WalkTo(StairFoot("WalkwayEntranceA_stair0"))));
            Add("leo ⇒ WEB", a => Sec == "WalkwayEntranceB" ? -1 : U);
            // ---- WEB (bất tử): thang ↖ cắt đáy → zig-zag ↗ lên y 96 ⇒ phải rơi xuống y 160 ⇒ nhảy khe 320 ⇒ thang ↗ stair2 lên y 128 ⇒ khối y 112
            //      ⇒ khối y 96 ⇒ xuống thang ↖ stair4 tới y 160 ⇒ khối y 144 ⇒ khối y 112 ⇒ rơi xuống gờ cửa y 128 ⇒ cửa PHẢI #1
            Add("chờ WEB", a => Ready("WalkwayEntranceB") ? -1 : 0);
            Add("leo hết thang WEB", a => S.Y == 96 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("phải rơi xuống y 160", a => WalkTo(180));
            Add("tới mép khe 1 WEB", a => WalkTo(314));
            Add("nhảy khe 1 WEB", a => Jump(a, R));
            Add("tới chân thang 2 WEB", a => WalkTo(StairFoot("WalkwayEntranceB_stair2")));
            Add("leo thang 2 WEB", a => S.Y == 128 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("tới mép khối y 128", a => WalkTo(450));
            Add("nhảy lên khối y 112", a => Jump(a, R));
            Add("tới mép khối y 112", a => WalkTo(514));
            Add("nhảy lên khối y 96", a => Jump(a, R));
            Add("tới đỉnh thang 4 WEB", a => WalkTo(StairTop("WalkwayEntranceB_stair4")));
            Add("xuống thang 4 WEB", a => S.Y == 160 && !S.OnStairs && S.OnGround() ? -1 : D);
            Add("tới mép y 160 WEB", a => WalkTo(664));   // thân chạm khối (42-43,9) y 144 ở x 664
            Add("nhảy lên khối y 144", a => Jump(a, R));
            Add("tới mép khối y 144", a => WalkTo(690));
            Add("nhảy lên khối y 112 cửa", a => Jump(a, R));
            Add("cửa phải ⇒ WalkwayA", a => Sec == "WalkwayA" ? -1 : (Flow.Busy ? 0 : WalkHold(R)));
            // ---- STAGE 08: WalkwayA (bất tử) — cầu y 128 ⇒ thang ↗ stair0 ⇒ chiếu nghỉ ⇒ ↖ stair1 cắt đỉnh ⇒ WalkwayB
            Add("S8", a => -1);
            Add("chờ WalkwayA", a => Ready("WalkwayA") ? -1 : 0);
            Add("tới chân thang WalkwayA", a => Rx(WalkTo(StairFoot("WalkwayA_stair0"))));
            Add("leo ⇒ WalkwayB", a => Sec == "WalkwayB" ? -1 : U);
            // ---- WalkwayB (bất tử): thang ↖ cắt đáy lên cầu y 128 ⇒ 2 khe cầu ⇒ khối y 112 ⇒ khối y 96 ⇒ cửa PHẢI #2
            Add("chờ WalkwayB", a => Ready("WalkwayB") ? -1 : 0);
            Add("leo hết thang WalkwayB", a => S.Y == 128 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("tới khe 1 WalkwayB", a => WalkTo(490));
            Add("nhảy khe 1 WalkwayB", a => Jump(a, R));
            Add("tới khe 2 WalkwayB", a => WalkTo(618));
            Add("nhảy khe 2 WalkwayB", a => Jump(a, R));
            Add("tới trước khối y 112 WB", a => WalkTo(770));
            Add("nhảy lên khối y 112 WB", a => Jump(a, R));
            Add("tới mép khối y 112 WB", a => WalkTo(800));
            Add("nhảy lên khối y 96 WB", a => Jump(a, R));
            Add("cửa phải ⇒ MummyMan", a => Sec == "MummyMan" ? -1 : (Flow.Busy ? 0 : WalkHold(R)));
            // ---- STAGE 09: MummyMan (bất tử) — cầu y 96 ⇒ cầu hỏng 3 khe ⇒ bậc xuống phòng ⇒ khoá cx 1280 ⇒ 2 Mummy ⇒ quả cầu
            Add("S9", a => -1);
            Add("chờ MummyMan", a => Ready("MummyMan") ? -1 : 0);
            Add("tới khe cầu 1", a => WalkTo(762));
            Add("nhảy khe cầu 1", a => Jump(a, R));
            Add("tới khe cầu 2", a => WalkTo(826));
            Add("nhảy khe cầu 2", a => Jump(a, R));
            Add("tới khe cầu 3", a => WalkTo(890));
            Add("nhảy khe cầu 3", a => Jump(a, R));
            Add("tới phòng Mummy", a => WalkTo(1408));
            Add("chờ Mummy thức", a => MummyAwake() ? -1 : 0);
            Add("đánh Mummy Men", a => FightMummies());
            Add("nhặt quả cầu", a => StageRules.Current != null && StageRules.Current.Tally.Active ? -1 : Crystal());
            Add("chờ hết", a => 0);
            if (startStage > 7)
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
                Trace.Add(Tick + "\t✓ " + steps[index].Key + (S != null && W != null ? "\t" + Sec + " (" + S.X + "," + S.Y + ") t" + W.TickCount + " HP " + GameSession.Current.playerHp : ""));
                index++;
                age = 0;
            }
            return 0;
        }

        private static bool Ready(string sec)
        {
            return InStage && Sec == sec && Flow != null && !Flow.Busy && S != null && S.Fsm != null;
        }

        private static StairPath Stair(string id)
        {
            var tiles = W.Tiles;
            for (int i = 0; i < tiles.StairCount; i++)
            {
                var p = tiles.GetStair(i);
                if (p.stair != null && p.stair.id == id)
                    return p;
            }
            throw new Exception("không có thang " + id);
        }

        private static int StairFoot(string id)
        {
            var p = Stair(id);
            return p.bottom.x + S.StairOffset(p);
        }

        private static int StairTop(string id)
        {
            var p = Stair(id);
            return p.top.x + S.StairOffset(p);
        }

        private int WalkTo(int x)
        {
            int tx;
            if (Tune.TryGetValue(Current, out tx))
                x = tx;
            var s = S;
            if (s == null || Flow.Busy)
                return 0;
            if (!s.OnGround() || s.Fsm.Is(eSimonState.Land) || s.Fsm.Is(eSimonState.Attack) || s.OnStairs || s.Fsm.Is(eSimonState.Hurt))
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

        /// <summary>Quất phản xạ (đoạn không bất tử): kẻ địch trong tầm roi (|dx| ≤ 40, chân trong [Simon − 40, Simon + 12]) ⇒ quất (quay về phía nó;
        /// Fleaman sát đất ⇒ ngồi quất); không thì trả mask của bước. m &lt; 0 (bước xong) giữ nguyên.</summary>
        private int Rx(int m)
        {
            if (m < 0)
                return m;
            var s = S;
            if (s == null || !s.OnGround() || s.OnStairs || s.Fsm.Is(eSimonState.Attack) || s.Fsm.Is(eSimonState.Hurt))
                return m;
            int t = W.TickCount;
            if (t - lastWhip <= 20)
                return m;
            var list = W.GroupList(eEntityGroup.Enemy);
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i] as EnemyBase;
                if (e == null || !e.Alive || e.defeated || e is BossBase)
                    continue;
                int dx = e.X - s.X, dy = e.Y - s.Y;
                if (Math.Abs(dx) > 40 || dy < -40 || dy > 12)
                    continue;
                lastWhip = t;
                int dir = dx >= 0 ? R : L;
                int turn = (dx >= 0) != (s.Facing > 0) ? dir : 0;
                bool low = e is Fleaman && dy >= -4;
                return B | turn | (low ? D : 0);
            }
            return m;
        }

        private static bool MummyAwake()
        {
            foreach (var m in Mummies())
                if (!m.dormant)
                    return true;
            return false;
        }

        /// <summary>Đánh 2 Mummy (bất tử): con gần nhất — quay về phía nó + roi khi |dx| ≤ 44; xa ⇒ đi theo trong [cx + 20, cx + 236].</summary>
        private int FightMummies()
        {
            var s = S;
            var ms = Mummies();
            MummyMan best = null;
            foreach (var m in ms)
                if (!m.dying && (best == null || Math.Abs(m.X - s.X) < Math.Abs(best.X - s.X)))
                    best = m;
            if (best == null)
                return ms.Count == 0 ? -1 : 0;
            if (s.Fsm.Is(eSimonState.Attack) || !s.OnGround())
                return 0;
            int dx = best.X - s.X;
            int dir = dx >= 0 ? R : L;
            int t = W.TickCount;
            if (Math.Abs(dx) <= 44 && t - lastWhip > 24)
            {
                lastWhip = t;
                return B | ((dx >= 0) != (s.Facing > 0) ? dir : 0);
            }
            if (Math.Abs(dx) > 36)
            {
                int cx = Cx;
                if (dx > 0 && s.X < cx + 236)
                    return R;
                if (dx < 0 && s.X > cx + 20)
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
