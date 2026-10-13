#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Harness lộ trình Map 1 hoàn chỉnh (CVGP 15; routine 16/21 chạy lại) — CHỈ Editor, scene Main.unity khi Play. Qua MCP:
///   Map1RouteProbe.Record(tpu)            ⇒ AUTOPILOT (Pilot bên dưới — chỉ đọc trạng thái, bấm nút qua NesPad.Inject) chơi từ đầu Stage 01
///                                            tới STAGE 04 stub, ghi input từng tick ⇒ Assets/Data/Tests/Replays/map1_route.txt
///   Map1RouteProbe.Replay(shotDir, tpu)   ⇒ phát lại file (NesPad.PlayReplay — input thật bị bỏ qua), log mốc + hash trạng thái từng tick,
///                                            chụp Game view tại các điểm kiểm (camera x trùng frame video — Tools/cvgp/map1_pairs.py)
///   Map1RouteProbe.Report()               ⇒ mốc (vào section, cổng, cửa, đổi tầng, boss, quả cầu, tally, bản đồ, STAGE 04) + hash + số ảnh
/// Chạy: NesClock TẮT, tự gọi NesClock.Step tpu tick mỗi vòng EditorApplication.update (tua nhanh; tới điểm chụp thì 1 tick rồi chờ file ảnh).
/// Cheat BẤT TỬ bật suốt lộ trình (EditorPrefs CVGP.CheatInvincible — trả lại khi xong): đề bài cho phép ở đoạn khó; ghi rõ ở sổ.
/// Ván mới: GameRules.NewGame + GameManager.GoTo(Stage1, 0) — phần tử replay đầu = tick đầu tiên sau khi màn chơi bật.
/// </summary>
public static class Map1RouteProbe
{
    public const string REPLAY = "Assets/Data/Tests/Replays/map1_route.txt";
    public const int MAX_TICKS = 40000;
    /// <summary>Số tick chạy sau khi STAGE 04 stub hiện rồi dừng.</summary>
    public const int TAIL_TICKS = 60;

    public static bool Running { get; private set; }
    public static bool Finished { get; private set; }
    public static string Mode { get; private set; } = "";
    public static string Error { get; private set; } = "";
    public static int Tick { get; private set; }
    public static uint Hash { get; private set; }
    public static int ShotsTaken { get; private set; }
    public static int TicksPerUpdate = 40;
    public static readonly List<string> Marks = new List<string>();
    /// <summary>Hash trạng thái mỗi 500 tick (so 2 lần chạy).</summary>
    public static readonly List<uint> Checkpoints = new List<uint>();
    /// <summary>Số GameObject trong scene sau mỗi lần chạy (kiểm rò).</summary>
    public static readonly List<int> ObjectCounts = new List<int>();

    private static MethodInfo stepMethod;
    private static Pilot pilot;
    private static bool prevInvincible;
    private static eGameScreen lastScreen;
    private static string lastSection = "";
    private static int stage4Tick = -1;
    private static List<Shot> shots;
    private static string shotDir, waitFile;
    private static int waited;

    // ---------------------------------------------------------------- điểm chụp (camera x = cx frame video — planner work/map1_camera.csv)

    private class Shot
    {
        public string name;
        public Func<bool> when;
        public bool done;
        public Shot(string n, Func<bool> w) { name = n; when = w; }
    }

    private static StageWorld W => StageWorld.Current;
    private static SimonController S => W != null ? W.Player as SimonController : null;
    private static int Cx => W != null && W.CameraCtl != null ? W.CameraCtl.X : -1;
    private static string Sec => W != null && W.Section != null ? W.Section.name : "";
    private static bool InStage => GameManager.I != null && GameManager.I.State == eGameScreen.Stage1 && !GameManager.I.IsTransitioning && W != null;
    private static StageFlow Flow => W != null ? W.GetComponent<StageFlow>() : null;

    private static bool At(string sec, int cx)
    {
        return InStage && Sec == sec && Cx == cx && Flow != null && !Flow.Busy;
    }

    private static int doorOpenTick = -1, mapEnterFrame = -1;

    private static List<Shot> BuildShots()
    {
        return new List<Shot>
        {
            new Shot("ce_f70_cx0", () => At("CastleEntrance", 0) && W.TickCount >= 2),
            new Shot("ce_f180_cx63", () => At("CastleEntrance", 63)),
            new Shot("ce_f330_cx246", () => At("CastleEntrance", 246)),
            new Shot("ce_f470_cx482", () => At("CastleEntrance", 482)),
            new Shot("ce_f540_gate", () => InStage && Sec == "CastleEntrance" && Flow != null && Flow.Phase == StageFlow.ePhase.Gate && S.X >= 696),
            new Shot("eh_f606_cx0", () => At("EntranceHall", 0) && W.TickCount >= 2),
            new Shot("eh_f900_cx108", () => At("EntranceHall", 108)),
            new Shot("eh_f1700_cx711", () => At("EntranceHall", 711)),
            new Shot("eh_f2100_cx1030", () => At("EntranceHall", 1030)),
            new Shot("eh_f2300_cx1239", () => At("EntranceHall", 1239)),
            new Shot("door1_f2640", () => doorOpenTick >= 0 && Sec == "EntranceHall" && W.TickCount == doorOpenTick + 30),
            new Shot("ca_f2760_cx0", () => At("CryptA", 0)),
            new Shot("ca_f2900_cx29", () => At("CryptA", 29)),
            new Shot("cb_f3600_cx0", () => At("CryptB", 0) && S.OnStairs == false),
            new Shot("cb_f3800_cx88", () => At("CryptB", 88)),
            new Shot("cb_f4050_cx256", () => At("CryptB", 256)),
            new Shot("ca_f4445_cx253", () => At("CryptA", 253) && S.Y > 112),
            new Shot("door2_f5010", () => doorOpenTick >= 0 && Sec == "CryptA" && W.TickCount == doorOpenTick + 30),
            new Shot("pb_f5130_cx0", () => At("PhantomBat", 0)),
            new Shot("pb_f5600_cx118", () => At("PhantomBat", 118)),
            new Shot("pb_f5800_cx330", () => At("PhantomBat", 330)),
            new Shot("pb_f5940_boss_sleep", () => At("PhantomBat", 512) && BossAwake() == false),
            new Shot("pb_f6000_boss_awake", () => InStage && Sec == "PhantomBat" && BossAwake() == true && BossAge() == 30),
            new Shot("pb_f6390_crystal", () => InStage && CrystalOnFloor()),
            new Shot("tally_f6600", () => InStage && StageRules.Current != null && StageRules.Current.Tally.Active && GameSession.Current.time == 0
                                          && GameSession.Current.hearts > 0 && TallyGap()),
            new Shot("map_f7100", () => GameManager.I != null && GameManager.I.State == eGameScreen.CastleMap && mapEnterFrame >= 0
                                        && NesClock.Frame - mapEnterFrame == 171 + 97),
            new Shot("stage4_f7180", () => GameManager.I != null && GameManager.I.State == eGameScreen.Stage4Stub && stage4Tick >= 0
                                           && Tick - stage4Tick == 6),
        };
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

    private static bool? BossAwake()
    {
        var b = Boss();
        if (b == null)
            return null;
        return !b.dormant;
    }

    private static int bossWakeTick = -1;

    private static int BossAge()
    {
        return bossWakeTick >= 0 ? W.TickCount - bossWakeTick : -1;
    }

    private static bool CrystalOnFloor()
    {
        foreach (var e in W.GroupList(eEntityGroup.Item))
        {
            var it = e as ItemPickup;
            if (it != null && it.Alive && it.id == eItemId.MagicCrystal && it.Y >= 128)
                return true;
        }
        return false;
    }

    private static bool TallyGap()
    {
        var t = StageRules.Current.Tally;
        return t.Phase == eTallyPhase.Gap;
    }

    // ---------------------------------------------------------------- chạy

    /// <summary>Autopilot chơi + ghi replay. tpu = tick mỗi vòng update.</summary>
    public static string Record(int tpu)
    {
        return Start(null, null, tpu);
    }

    /// <summary>Phát lại map1_route.txt (shotDir null ⇒ không chụp).</summary>
    public static string Replay(string dir, int tpu)
    {
        if (!File.Exists(REPLAY))
            return "thiếu " + REPLAY + " — chạy Record trước";
        return Start(NesReplay.Decode(File.ReadAllText(REPLAY)), dir, tpu);
    }

    private static string Start(List<int> ticks, string dir, int tpu)
    {
        if (!EditorApplication.isPlaying || GameManager.I == null || NesClock.I == null || NesPad.I == null)
            return "cần Play Main.unity";
        if (Running)
            return "đang chạy";
        TicksPerUpdate = Mathf.Max(1, tpu);
        Mode = ticks == null ? "record" : "replay";
        Error = "";
        Finished = false;
        Tick = 0;
        Hash = 2166136261u;
        ShotsTaken = 0;
        Marks.Clear();
        Checkpoints.Clear();
        lastSection = "";
        stage4Tick = -1;
        doorOpenTick = -1;
        mapEnterFrame = -1;
        bossWakeTick = -1;
        shotDir = dir;
        waitFile = null;
        shots = dir != null ? BuildShots() : new List<Shot>();
        if (dir != null)
            Directory.CreateDirectory(dir);
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
        if (!GameManager.I.GoTo(eGameScreen.Stage1, 0))
        {
            Stop("GameManager đang chuyển cảnh");
            return Error;
        }
        lastScreen = GameManager.I.State;
        if (ticks != null)
            NesPad.I.PlayReplay(ticks);
        else
        {
            pilot = new Pilot();
            NesPad.I.StartRecord();
        }
        Running = true;
        EditorApplication.update += Pump;
        Mark("start");
        return Mode + " bắt đầu" + (ticks != null ? " (" + ticks.Count + " tick)" : "");
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
                if (string.IsNullOrEmpty(err))
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
        pilot = null;
        Checkpoints.Add(Hash);
        ObjectCounts.Add(UnityEngine.Object.FindObjectsOfType<Transform>(true).Length);
        Mark(string.IsNullOrEmpty(err) ? "end" : "LỖI " + err);
    }

    private static void SaveReplay(List<int> rec)
    {
        var sb = new StringBuilder();
        sb.Append("# ").Append(rec.Count).Append(" tick, hash ").Append(NesReplay.Hash(rec).ToString("x8")).Append('\n');
        sb.Append("# CVGP 15 map1_route: lộ trình Map 1 từ đầu Stage 01 (GameRules.NewGame + GoTo(Stage1,0)) tới STAGE 04 stub — ghi bằng autopilot ")
          .Append("Map1RouteProbe.Pilot, CHEAT BẤT TỬ bật suốt (CVGP.CheatInvincible) — phát lại: Map1RouteProbe.Replay\n");
        sb.Append(NesReplay.Encode(rec)).Append('\n');
        File.WriteAllText(REPLAY, sb.ToString(), new UTF8Encoding(false));
        AssetDatabase.ImportAsset(REPLAY);
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
            else if (!NesPad.I.IsReplaying && Mode == "replay" && stage4Tick < 0)
            {
                Stop("hết replay trước STAGE 04");
                return;
            }
            stepMethod.Invoke(NesClock.I, null);
            Tick++;
            Observe();
            if (stage4Tick >= 0 && Tick - stage4Tick >= TAIL_TICKS)
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
            if (gm.State == eGameScreen.CastleMap)
                mapEnterFrame = NesClock.Frame;
            if (gm.State == eGameScreen.Stage4Stub)
                stage4Tick = Tick;
        }
        if (InStage)
        {
            if (Sec != lastSection)
            {
                lastSection = Sec;
                Mark("section " + Sec);
            }
            var b = Boss();
            if (b != null && !b.dormant && bossWakeTick < 0)
            {
                bossWakeTick = W.TickCount;
                Mark("boss thức");
            }
        }
        // hash trạng thái
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

    private static void OnMilestone(StageFlow f, string m)
    {
        if (m == "door_open")
            doorOpenTick = f.World.TickCount;
        Mark("flow " + m);
    }

    private static void OnBossDefeated(BossBase b)
    {
        Mark("boss hạ (+" + b.Score + ")");
    }

    private static void OnCrystalSpawned(BossBase b, ItemPickup p)
    {
        Mark("quả cầu hiện (" + (p != null ? p.X + "," + p.Y : "?") + ")");
    }

    private static void OnCrystalPicked(StageWorld w)
    {
        Mark("nhặt quả cầu");
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

    // ---------------------------------------------------------------- autopilot (chỉ dùng khi GHI replay)

    /// <summary>
    /// Lái Simon qua Map 1 bằng chuỗi bước (đi tới x, bấm nút, giữ nút tới khi điều kiện đúng) + đánh boss phản xạ.
    /// Không đọc gì ngoài trạng thái công khai; mọi quyết định thành bitmask nút ⇒ NesPad ghi lại ⇒ replay tái hiện y hệt.
    /// </summary>
    private class Pilot
    {
        const int U = 1 << (int)eNesButton.Up, D = 1 << (int)eNesButton.Down, L = 1 << (int)eNesButton.Left, R = 1 << (int)eNesButton.Right,
                  A = 1 << (int)eNesButton.A, B = 1 << (int)eNesButton.B;

        private delegate int StepFn(int age);   // trả mask; −1 = bước xong

        private readonly List<KeyValuePair<string, StepFn>> steps = new List<KeyValuePair<string, StepFn>>();
        private int index, age, lastWhip = -100;
        public string Failed;

        public Pilot()
        {
            // Castle Entrance: phá 5 chậu lửa như video (tim lớn, roi, roi, tim lớn, dao) — đứng cách chậu 24 px
            Add("chờ CE", a => InStage && Sec == "CastleEntrance" ? -1 : 0);
            foreach (int tx in new[] { 96, 224, 352, 480, 608 })
            {
                int x = tx - 24;
                Add("tới chậu " + tx, a => Walk(x));
                Add("roi chậu " + tx, a => a == 0 ? B : a < 50 ? 0 : -1);
            }
            Add("vào cổng", a => InStage && Sec == "EntranceHall" ? -1 : R);
            // Entrance Hall: đi sàn tới dưới cầu thang dài (↗ 12 bậc, chân x 1292) ⇒ leo ⇒ đi phải tới cửa
            Add("chờ EH", a => Ready("EntranceHall") ? -1 : 0);
            Add("tới gầm thang dài", a => Walk(1298));
            Add("leo thang dài", a => S.Y == 80 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("tới cửa 1", a => Sec == "CryptA" ? -1 : (Flow.Busy ? 0 : R));
            // Crypt A (lần 1): đi phải rơi 2 gờ xuống sàn ⇒ trái tới miệng thang trái-dưới ⇒ xuống
            Add("chờ CA", a => Ready("CryptA") ? -1 : 0);
            Add("xuống sàn CA", a => S.Y == 176 && S.OnGround() && !S.Fsm.Is(eSimonState.Land) ? -1 : R);
            Add("tới miệng thang trái", a => Walk(56));
            Add("xuống thang ⇒ CB", a => Sec == "CryptB" ? -1 : D);
            // Crypt B: xuống hết thang ⇒ đi phải ⇒ nhảy 2 khe nước ⇒ thang phải lên ⇒ CA
            Add("chờ CB", a => Ready("CryptB") ? -1 : 0);
            Add("xuống hết thang CB", a => !S.OnStairs && S.OnGround() ? -1 : D);
            Add("tới mép khe 1", a => Walk(226));
            Add("nhảy khe 1", a => Jump(a, R));
            Add("tới mép khe 2", a => Walk(290));
            Add("nhảy khe 2", a => Jump(a, R));
            Add("tới chân thang phải", a => Walk(448));
            Add("leo thang ⇒ CA", a => Sec == "CryptA" ? -1 : U);
            // Crypt A (lần 2): leo hết thang ⇒ phải tới thang dài ⇒ leo ⇒ nhảy lên lối đi trên ⇒ cửa 2
            Add("chờ CA 2", a => Ready("CryptA") ? -1 : 0);
            Add("leo ra sàn CA", a => S.Y == 176 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("tới chân thang dài CA", a => Walk(464));
            Add("leo thang dài CA", a => S.Y == 112 && !S.OnStairs && S.OnGround() ? -1 : U);
            Add("nhảy lên lối đi trên", a => Jump(a, R));
            Add("tới cửa 2", a => Sec == "PhantomBat" ? -1 : (Flow.Busy ? 0 : R));
            // Phantom Bat: đi phải rơi 2 gờ xuống sàn ⇒ nhảy lên bệ chân thang ⇒ đi tới x 696 (dơi thức) ⇒ xuống sàn ⇒ đánh
            Add("chờ PB", a => Ready("PhantomBat") ? -1 : 0);
            Add("xuống sàn PB", a => S.Y == 176 && S.OnGround() && !S.Fsm.Is(eSimonState.Land) && S.X > 380 ? -1 : R);
            Add("tới trước bệ", a => Walk(648));
            Add("nhảy lên bệ", a => Jump(a, R));
            Add("đánh thức dơi", a => BossAwake() == true ? -1 : (S.X < 696 ? R : 0));
            Add("xuống sàn đánh", a => S.Y == 176 && S.OnGround() && S.X <= 640 ? -1 : L);
            Add("đánh dơi", a => Fight());
            Add("nhặt quả cầu", a => StageRules.Current != null && StageRules.Current.Tally.Active ? -1 : Crystal());
            Add("chờ hết", a => 0);
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
                    if (age > 6000)
                        Failed = "kẹt ở bước '" + steps[index].Key + "'";
                    return m;
                }
                index++;
                age = 0;
            }
            return 0;
        }

        private static bool Ready(string sec)
        {
            return InStage && Sec == sec && Flow != null && !Flow.Busy && S != null;
        }

        /// <summary>Giữ hướng tới khi X đúng bằng x (1 px/tick ⇒ dừng đúng chỗ).</summary>
        private static int Walk(int x)
        {
            var s = S;
            if (s == null || Flow.Busy)
                return 0;
            if (!s.OnGround() || s.Fsm.Is(eSimonState.Land) || s.Fsm.Is(eSimonState.Attack))
                return s.X == x ? -1 : 0;
            if (s.X == x)
                return -1;
            return s.X < x ? R : L;
        }

        /// <summary>Nhảy theo hướng dir: tick đầu A+dir; chờ tới khi đáp đất.</summary>
        private static int Jump(int a, int dir)
        {
            var s = S;
            if (a == 0)
                return A | dir;
            if (a < 4)
                return dir;
            return s.OnGround() && !s.Fsm.Is(eSimonState.Jump) && !s.Fsm.Is(eSimonState.Fall) ? -1 : dir;
        }

        /// <summary>Đánh Phantom Bat trên sàn (bất tử): quay về phía dơi + roi khi dơi trong tầm roi (cao ngang ngực/đầu); dơi xa ⇒ đi theo
        /// trong [528, 640] (không với tới khối II ở x 688 — giữ nguyên khối như video).</summary>
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
            bool band = dy >= -44 && dy <= 4;
            int t = W.TickCount;
            if (band && Math.Abs(dx) <= 60 && t - lastWhip > 24)
            {
                lastWhip = t;
                return B | ((dx >= 0) != (s.Facing > 0) ? dir : 0);
            }
            if (Math.Abs(dx) > 48)
            {
                if (dx > 0 && s.X < 640)
                    return R;
                if (dx < 0 && s.X > 528)
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
