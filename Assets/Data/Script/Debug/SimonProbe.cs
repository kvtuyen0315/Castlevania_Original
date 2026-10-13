#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness kiểm Simon (CVGP 08; routine 10/16 chạy lại) — CHỈ Editor, scene Arena (Player = Simon). Gọi qua MCP execute_code khi Play:
///   SimonProbe.SaveReplays()            ⇒ ghi input mỗi kịch bản ra Assets/Data/Tests/Replays/simon_&lt;tên&gt;.txt (dòng đầu: số tick + hash)
///   SimonProbe.RunSuite("walk,jump,…")  ⇒ chạy lần lượt (input ĐỌC TỪ FILE replay), ghi từng tick (x, y, state, khung, vẽ, máu, bất tử),
///                                         TỰ CHẤM so với số đo VIDEO (hằng VIDEO_* bên dưới — độc lập với ConfigSimon) ⇒ SuiteReport().
///   SimonProbe.Shots[rel] = "đường dẫn tuyệt đối .png" trước khi Run ⇒ chụp Game view đúng tick.
/// Hash FNV-1a mỗi tick: tick tương đối, fx/fy/hướng/state/bất tử/khung/vẽ của Simon, máu, camera, mọi thực thể khác (fx, fy, sống).
/// Toạ độ Arena (Tools/cvgp/build_arena.py): sàn y 176; tường thấp x 416–431 mặt trên y 144; hố 480–511; nước 544–575;
/// bục y 112 mép phải x 735; stair0 ↗ chân (620,176) đỉnh (684,112) ⇒ Simon đi đường x = path + 3; stair2 ↖ chân (948,112) đỉnh (916,80).
/// </summary>
public static class SimonProbe
{
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";

    // ---------------------------------------------------------------- số đo VIDEO (CVGP_MEMORY §2 "Simon") — kỳ vọng độc lập với Config
    /// <summary>Độ cao chân (px) ở tick nhảy t = 0..40 (video Map_1 — đồng hồ x 2:03.7 / 3:23.6 + 14 cú nhảy ghép).</summary>
    public static readonly int[] VIDEO_JUMP_H =
    {
        0, 5, 10, 13, 16, 19, 22, 25, 27, 29, 31, 32, 33, 34, 35, 35, 36, 36, 36, 36, 36,
        36, 36, 36, 36, 35, 35, 34, 33, 32, 31, 29, 27, 25, 22, 19, 16, 13, 10, 5, 0,
    };
    public const int VIDEO_WALK_FRAME_TICKS = 7;
    public const int VIDEO_FALL_SPEED = 8;
    public const int VIDEO_LAND_CROUCH = 20;      // ±2
    public const int VIDEO_KB_DX = 12;            // 11–13
    public const int VIDEO_KB_PEAK = 15;          // 14–15
    public const int VIDEO_KB_AIR = 36;           // ±2 (trúng f362.5 → biến mất f381)
    public const int VIDEO_BLINK = 94;            // 92–96
    public const int VIDEO_TUCK_DRAW = 4;
    public const int VIDEO_CROUCH_DRAW = 8;

    // ---------------------------------------------------------------- kịch bản (input RLE — file replay sinh từ đây)
    private struct Scn
    {
        public string name, rle, note;
        public Scn(string n, string r, string note) { name = n; rle = r; this.note = note; }
    }

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("walk", "R*120 -*10", "Teleport (100,176) phải; đi 120 tick"),
        new Scn("jump", "-*2 A -*60", "Teleport (200,176) phải; nhảy tại chỗ ở tick 2"),
        new Scn("jumpright", "-*2 R+A R*39 -*20", "Teleport (200,176) phải; nhảy kèm → và giữ →"),
        new Scn("jumpnoctl", "-*2 R+A L*39 -*20", "Teleport (200,176) phải; nhảy → rồi bấm ← giữa không trung (không được đổi hướng)"),
        new Scn("jumpwall", "-*2 R+A R*50 -*10", "Teleport (380,176) phải; nhảy lên tường thấp 32 px (x 416–431)"),
        new Scn("ledge32", "R*20 -*10", "Teleport (424,144) phải trên tường thấp; bước khỏi mép phải (rơi 32 px)"),
        new Scn("ledge64", "L*30 -*40", "Teleport (940,112) trái trên Bục C; bước khỏi mép trái x 928 như video 1:02.8 (rơi 64 px ⇒ ngồi khựng; đầu chạm bục y 80 phía trên 1 tick)"),
        new Scn("stairs", "U*130 -*8 D*140 -*8", "Teleport (626,176) trái — gầm stair0 ↗ cách chân Simon 3 px như video 0:46.7; lên rồi xuống"),
        new Scn("stair2", "U*70 -*8 D*70 -*8", "Teleport (945,112) phải — gầm stair2 ↖ cách chân 3 px; lên rồi xuống"),
        new Scn("stairback", "U*40 -*4", "Teleport (616,176) phải — PHÍA SAU chân stair0 (7 px) ⇒ tự đi tới chân rồi leo"),
        new Scn("stairfar", "U*20 -*4", "Teleport (640,176) trái — cách chân 17 px > StairEnterRange ⇒ không vào thang"),
        new Scn("stairstop", "U*5 -*30", "Teleport (626,176) trái; leo 5 tick rồi thả ⇒ đi nốt tới bậc (bội 8 px)"),
        new Scn("hurtL", "R*10 -*170", "Teleport (300,176) phải, bia ở (316,176) ⇒ trúng khi chạm, văng TRÁI (ngược hướng mặt)"),
        new Scn("hurtR", "L*10 -*170", "Teleport (300,176) trái, bia ở (284,176) ⇒ văng PHẢI"),
        new Scn("hurtstair", "U*29 -*160", "lên stair0 rồi đứng; tick 60 đặt bia đè Simon ⇒ mất máu, không văng khỏi bậc"),
        new Scn("water", "R*40 -*140", "Teleport (530,176) phải; đi vào nước (544–575) ⇒ FallDeath Water ⇒ Died sau DeathTicks ⇒ hồi sinh"),
        new Scn("pit", "R*30 -*150", "Teleport (470,176) phải; đi vào hố (480–511) ⇒ rơi khỏi đáy ⇒ FallDeath Pit"),
        new Scn("death", "R*10 -*230", "HP 2, như hurtL ⇒ HP 0 ⇒ knockback ⇒ Dead ⇒ Died ⇒ hồi sinh đầy máu"),
        new Scn("crouch", "D*20 D+L*5 -*5", "Teleport (200,176) phải; ngồi, quay trái khi ngồi, đứng dậy"),
        new Scn("autowalk", "-*80", "Teleport (100,176) phải; StartAutoWalk(168) ở tick 0 (cửa cuối stage: tự đi 68 px)"),
    };

    // ---------------------------------------------------------------- dữ liệu ghi
    public static string Label { get; private set; } = "";
    public static bool Running { get; private set; }
    public static int Target { get; private set; }
    public static int Recorded { get; private set; }
    public static uint Hash { get; private set; }
    public static int Fails { get; private set; }
    public static readonly List<int> Xs = new List<int>();
    public static readonly List<int> Ys = new List<int>();
    public static readonly List<int> Fxs = new List<int>();
    public static readonly List<eSimonState> States = new List<eSimonState>();
    public static readonly List<string> Frames = new List<string>();
    public static readonly List<int> DrawYs = new List<int>();
    public static readonly List<int> DrawXs = new List<int>();
    public static readonly List<int> Facings = new List<int>();
    public static readonly List<int> Hps = new List<int>();
    public static readonly List<int> Invs = new List<int>();
    public static readonly List<bool> Visibles = new List<bool>();
    public static readonly List<int> HurtTops = new List<int>();
    public static readonly List<int> HurtHeights = new List<int>();
    public static readonly List<int> StairTs = new List<int>();
    public static readonly List<int> Cams = new List<int>();
    public static readonly List<string> Checks = new List<string>();
    public static readonly List<string> Measures = new List<string>();
    public static readonly List<uint> TickHashes = new List<uint>();
    public static readonly List<uint> PrevTickHashes = new List<uint>();
    /// <summary>Chụp ảnh: tick tương đối → đường dẫn tuyệt đối (xoá sau mỗi kịch bản).</summary>
    public static readonly Dictionary<int, string> Shots = new Dictionary<int, string>();
    public static int DiedCount;
    public static eTileCollision DiedKind;
    public static int DiedTick = -1;
    public static int AutoWalkDoneTick = -1;

    private static readonly Queue<string> suite = new Queue<string>();
    public static readonly List<string> SuiteReports = new List<string>();
    public static bool SuiteDone { get; private set; } = true;
    public static int SuiteFails { get; private set; }
    public static uint SuiteHash { get; private set; }

    private static StageWorld world;
    private static Action<int> onTickAction;
    private static Action finish;
    private static readonly List<Entity> extras = new List<Entity>();
    private static SimonController hooked;

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    // ---------------------------------------------------------------- replay file

    public static string ReplayPath(string name)
    {
        return REPLAY_DIR + "/simon_" + name + ".txt";
    }

    /// <summary>Ghi file replay cho mọi kịch bản (dòng 1: "# N tick, hash XXXXXXXX", dòng 2: mô tả). Trả số file.</summary>
    public static int SaveReplays()
    {
        Directory.CreateDirectory(REPLAY_DIR);
        int n = 0;
        foreach (var s in SCENARIOS)
        {
            var ticks = NesReplay.Decode(s.rle);
            var sb = new StringBuilder();
            sb.Append("# ").Append(ticks.Count).Append(" tick, hash ").Append(NesReplay.Hash(ticks).ToString("x8")).Append('\n');
            sb.Append("# CVGP 08 SimonProbe kịch bản '").Append(s.name).Append("': ").Append(s.note).Append('\n');
            sb.Append(NesReplay.Encode(ticks)).Append('\n');
            File.WriteAllText(ReplayPath(s.name), sb.ToString(), new UTF8Encoding(false));
            n++;
        }
        UnityEditor.AssetDatabase.Refresh();
        return n;
    }

    private static List<int> LoadReplay(string name)
    {
        string p = ReplayPath(name);
        if (!File.Exists(p))
        {
            Debug.LogError("[SimonProbe] thiếu " + p + " — gọi SimonProbe.SaveReplays()");
            return null;
        }
        return NesReplay.Decode(File.ReadAllText(p));
    }

    public static string ScenarioNames()
    {
        var sb = new StringBuilder();
        foreach (var s in SCENARIOS)
            sb.Append(sb.Length > 0 ? "," : "").Append(s.name);
        return sb.ToString();
    }

    // ---------------------------------------------------------------- bộ kịch bản

    public static int RunSuite(string names)
    {
        Unhook();
        suite.Clear();
        SuiteReports.Clear();
        SuiteFails = 0;
        SuiteHash = 2166136261u;
        int total = 0;
        foreach (var n in names.Split(','))
        {
            var t = n.Trim();
            if (t.Length == 0)
                continue;
            suite.Enqueue(t);
            var r = LoadReplay(t);
            total += r != null ? r.Count : 0;
        }
        SuiteDone = false;
        NextInSuite();
        return total;
    }

    private static void NextInSuite()
    {
        while (suite.Count > 0)
            if (Run(suite.Dequeue()) > 0)
                return;
        SuiteDone = true;
    }

    public static string SuiteReport()
    {
        var sb = new StringBuilder();
        sb.Append("SIMON SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
          .Append(SuiteFails).Append(", hash bộ ").Append(SuiteHash.ToString("x8")).Append('\n');
        foreach (var r in SuiteReports)
            sb.Append(r).Append('\n');
        return sb.ToString();
    }

    // ---------------------------------------------------------------- 1 kịch bản

    public static int Run(string scenario)
    {
        world = StageWorld.Current;
        var s = Simon;
        if (world == null || s == null)
        {
            Debug.LogError("[SimonProbe] cần StageWorld (scene Arena) với Player = SimonController");
            return -1;
        }
        var ticks = LoadReplay(scenario);
        if (ticks == null)
            return -1;
        ClearExtras();
        world.ClearNonPlayer();
        world.ResetRng();
        var session = GameSession.Current;
        session.playerHp = GameSession.MAX_HP;
        session.NotifyChanged();
        onTickAction = null;
        finish = null;
        DiedCount = 0;
        DiedTick = -1;
        AutoWalkDoneTick = -1;
        DiedKind = eTileCollision.Empty;
        switch (scenario)
        {
            case "walk":
                s.Teleport(100, 176, 1);
                finish = () =>
                {
                    int bad = 0;
                    for (int i = 0; i < 120; i++)
                        if (Xs[i] != 101 + i)
                            bad++;
                    Expect("X tăng đúng 1 px/tick (120 tick)", 0, bad);
                    Expect("X sau 120 tick", 220, Xs[119]);
                    Expect("Y giữ 176", 176, Max(Ys));
                    Expect("dừng ngay khi thả (X tick 121 = 220)", 220, Xs[121]);
                    Expect("khung đầu khi bắt đầu đi = walk_2 (video 3/4 lần)", 1, Frames[0] == "walk_2" ? 1 : 0);
                    // độ dài mỗi khung (bỏ khung đầu): đều 7 tick
                    var runs = RunLengths(Frames, 0, 119);
                    int badRun = 0;
                    for (int i = 1; i < runs.Count - 1; i++)
                        if (runs[i] != VIDEO_WALK_FRAME_TICKS)
                            badRun++;
                    Measures.Add("đi: 1.000 px/tick; khung walk " + Join(runs) + " tick (video 7)");
                    Expect("khung đi giữ 7 tick (video)", 0, badRun);
                    Expect("thứ tự khung walk_2,1,0,1", 1, FrameOrderOk(new[] { "walk_2", "walk_1", "walk_0", "walk_1" }) ? 1 : 0);
                    Expect("state cuối Idle", (int)eSimonState.Idle, (int)States[States.Count - 1]);
                };
                break;
            case "jump":
                s.Teleport(200, 176, 1);
                finish = () => JumpChecks(2, 200, 0);
                break;
            case "jumpright":
                s.Teleport(200, 176, 1);
                finish = () => JumpChecks(2, 200, 1);
                break;
            case "jumpnoctl":
                s.Teleport(200, 176, 1);
                finish = () =>
                {
                    JumpChecks(2, 200, 1);
                    int turned = 0;
                    for (int i = 2; i < 2 + 39; i++)
                        if (Facings[i] != 1)
                            turned++;
                    Expect("không quay mặt giữa không trung (tick bay)", 0, turned);
                };
                break;
            case "jumpwall":
                s.Teleport(380, 176, 1);
                finish = () =>
                {
                    int land = -1;
                    for (int i = 3; i < Ys.Count && land < 0; i++)
                        if (Ys[i] == 144 && States[i] != eSimonState.Jump)
                            land = i - 1;
                    Measures.Add("nhảy lên tường 32 px: đáp mặt tường y 144 ở tick nhảy " + land + " (video bục +32: tick 30 ±1, 2:03.7)");
                    Expect("đáp lên mặt tường y 144", 144, Ys[Ys.Count - 1]);
                    ExpectRange("tick đáp (video 2:03.7: 29–31)", 29, 31, land);
                };
                break;
            case "ledge32":
                s.Teleport(424, 144, 1);
                finish = () => FallChecks(144, 176, false);
                break;
            case "ledge64":
                s.Teleport(940, 112, -1);
                finish = () => FallChecks(112, 176, true);
                break;
            case "stairs":
                s.Teleport(626, 176, -1);
                finish = () => StairChecks(0, 3, 3);
                break;
            case "stair2":
                s.Teleport(945, 112, 1);
                finish = () => StairChecks(2, 0, 3);
                break;
            case "stairback":
                s.Teleport(616, 176, 1);
                finish = () =>
                {
                    int firstStair = Index(States, eSimonState.StairUp);
                    Expect("tự đi tới chân (x 623, 7 px @ 1 px/tick) rồi leo ngay tick đó", 6, firstStair);
                    Expect("x lúc bắt đầu leo", 623, firstStair >= 0 ? Xs[firstStair] : -1);
                    Expect("y lúc đó (t 0.5 ⇒ 176)", 176, firstStair >= 0 ? Ys[firstStair] : -1);
                };
                break;
            case "stairfar":
                s.Teleport(640, 176, -1);
                finish = () =>
                {
                    Expect("ngoài tầm vào thang ⇒ vẫn Idle", (int)eSimonState.Idle, (int)States[States.Count - 1]);
                    Expect("không di chuyển", 640, Xs[Xs.Count - 1]);
                };
                break;
            case "stairstop":
                s.Teleport(626, 176, -1);
                finish = () =>
                {
                    int last = StairTs[StairTs.Count - 1];
                    Expect("thả nút ⇒ đi nốt tới bậc (t = 8)", 8, last);
                    Expect("state StairIdle", (int)eSimonState.StairIdle, (int)States[States.Count - 1]);
                    Expect("tick tới t = 8 (t 3 + 0.5/tick ⇒ 10 tick)", 9, IndexWhere(i => StairTs[i] == 8));
                };
                break;
            case "hurtL":
            case "hurtR":
            case "death":
                {
                    int dir = scenario == "hurtR" ? -1 : 1;
                    s.Teleport(300, 176, dir);
                    if (scenario == "death")
                    {
                        session.playerHp = 2;
                        session.NotifyChanged();
                    }
                    var t = SpawnTarget(300 + dir * 16, 176);
                    onTickAction = rel =>
                    {
                        if (rel == 30 && t != null && t.Alive)
                            t.Kill(eKillReason.Cleared);
                    };
                    finish = () => HurtChecks(dir, scenario == "death");
                }
                break;
            case "hurtstair":
                s.Teleport(626, 176, -1);
                {
                    ArenaTarget t = null;
                    onTickAction = rel =>
                    {
                        if (rel == 60)
                            t = SpawnTarget(Simon.X, Simon.Y);
                        if (rel == 70 && t != null && t.Alive)
                            t.Kill(eKillReason.Cleared);
                    };
                    finish = () =>
                    {
                        int hit = IndexWhere(i => Hps[i] < GameSession.MAX_HP);
                        Expect("mất 2 vạch (Stage 1)", GameSession.MAX_HP - 2, Hps[Hps.Count - 1]);
                        Expect("vẫn trên thang (không văng — wiki Stage 17)", 1, IsStair(States[Math.Min(hit + 5, States.Count - 1)]) ? 1 : 0);
                        Expect("vị trí không đổi khi trúng", Xs[hit - 1] * 1000 + Ys[hit - 1], Xs[hit + 5] * 1000 + Ys[hit + 5]);
                        Measures.Add("trúng trên thang tick " + hit + " (t " + StairTs[hit] + "): HP 16→" + Hps[Hps.Count - 1] + ", bất tử " + Invs[hit] + " tick, đứng nguyên bậc");
                    };
                }
                break;
            case "water":
                s.Teleport(530, 176, 1);
                finish = () => DeathFallChecks(eTileCollision.Water, 530);
                break;
            case "pit":
                s.Teleport(470, 176, 1);
                finish = () => DeathFallChecks(eTileCollision.Pit, 470);
                break;
            case "crouch":
                s.Teleport(200, 176, 1);
                finish = () =>
                {
                    Expect("state Crouch khi giữ ↓", (int)eSimonState.Crouch, (int)States[5]);
                    Expect("khung ngồi vẽ +8 (video 1:02.8)", VIDEO_CROUCH_DRAW, DrawYs[5]);
                    Expect("hurtbox ngồi cao 23", 23, HurtHeights[5]);
                    Expect("hurtbox ngồi đỉnh = Y − 22", -22, HurtTops[5]);
                    Expect("không di chuyển khi ngồi + ←", 200, Xs[24]);
                    Expect("quay trái khi ngồi + ←", -1, Facings[24]);
                    Expect("thả ↓ ⇒ Idle", (int)eSimonState.Idle, (int)States[States.Count - 1]);
                    Expect("đứng dậy hết lệch vẽ", 0, DrawYs[DrawYs.Count - 1]);
                };
                break;
            case "autowalk":
                s.Teleport(100, 176, 1);
                onTickAction = rel =>
                {
                    if (rel == 0)
                        Simon.StartAutoWalk(168);
                };
                finish = () =>
                {
                    Expect("tự đi tới x 168", 168, Xs[Xs.Count - 1]);
                    Expect("AutoWalkDone ở tick (68 px @ 1 px/tick, bắt đầu tick 1)", 68, AutoWalkDoneTick);
                    Expect("state cuối Idle", (int)eSimonState.Idle, (int)States[States.Count - 1]);
                };
                break;
            default:
                Debug.LogError("[SimonProbe] kịch bản lạ " + scenario);
                return -1;
        }
        s.ResetStats();
        NesPad.I.PlayReplay(ticks);
        Begin(scenario, ticks.Count);
        return ticks.Count;
    }

    // ---------------------------------------------------------------- ảnh so video (section Map 1 thật, camera theo luật 07)
    private struct Photo
    {
        public string name, section, rle;
        public int x, y, facing, target;
        public int[] shots;
        public Photo(string n, string sec, int x, int y, int f, string rle, int target, params int[] shots)
        {
            name = n; section = sec; this.x = x; this.y = y; facing = f; this.rle = rle; this.target = target; this.shots = shots;
        }
    }

    /// <summary>Cặp ảnh so khung video cùng pha (CVGP_MEMORY §7 routine 08). target ≠ 0 ⇒ đặt bia chạm ở x target (gỡ sau tick 1).</summary>
    private static readonly Photo[] PHOTOS =
    {
        // video Map_1 f407 (0:13.57): CE x 509 walk_2 quay phải, cx 381 ⇒ bắt đầu đi ở 505, khung walk_2 tick 0–6
        new Photo("walk", "CastleEntrance", 505, 160, 1, "R*10", 0, 3),
        // video f1126–1144 (0:37.5): EH nhảy tại chỗ x 478 cx 350; pha video t 3/8/10/12/14/16/27/34 (độ cao đo) ⇒ tick 2 + t − 1
        new Photo("jump", "EntranceHall", 478, 176, 1, "-*2 A -*44", 0, 4, 9, 11, 13, 15, 17, 21, 28, 35),
        // video f1415 (0:47.17): EH stair0 x 641 (t 18) stairs_up_0, cx 513 ⇒ vào ở x 626 (t 3), t 18 ở tick 29
        new Photo("stairs", "EntranceHall", 626, 176, -1, "U*40", 0, 29),
        // video boss f366 (12.2 s): PB cx 512, trước khi trúng x 739 quay trái, bị đánh lùi phải: x +10, cao 13 ⇒ tick 10 sau đòn
        new Photo("knockback", "PhantomBat", 739, 176, -1, "-*40", 735, 10),
        // video f1890 (1:03.0): EH ngồi x 915 quay trái, cx 787 (ngồi khựng sau rơi) ⇒ giữ ↓
        new Photo("crouch", "EntranceHall", 915, 176, -1, "D*20", 0, 5),
        // video f1884 (1:02.8): EH đang rơi khỏi Bục C x 918 cao Y 136 (section), cx 790 ⇒ từ 940 đi trái, rơi tick 19, Y 136 ở tick 21
        new Photo("fall", "EntranceHall", 940, 112, -1, "L*30 -*30", 0, 21, 30),
        // không có video chết — chỉ ảnh Unity (Arena): HP 2 + bia ⇒ ngã tick 36, death_1 từ tick 52
        new Photo("death", "Arena", 300, 176, 1, "R*10 -*80", 316, 45, 70),
    };

    /// <summary>Chụp 1 bộ ảnh: warp section, đặt Simon, phát input, chụp Game view ở các tick (file outDir/photo_&lt;tên&gt;_&lt;tick&gt;.png).
    /// Cần targetFrameRate ≥ 120 + vSyncCount 0 để mỗi frame Unity ≤ 1 tick. Trả số tick.</summary>
    public static int RunPhoto(string name, string outDir)
    {
        world = StageWorld.Current;
        var s = Simon;
        if (world == null || s == null)
            return -1;
        Photo ph = default(Photo);
        bool found = false;
        foreach (var p in PHOTOS)
            if (p.name == name)
            {
                ph = p;
                found = true;
            }
        if (!found)
            return -1;
        LevelSection sec = null;
        foreach (var l in world.Sections)
            if (l.name == ph.section)
                sec = l;
        if (sec == null)
            return -1;
        ClearExtras();
        world.Warp(sec, ph.x, ph.y);
        world.ResetRng();
        GameSession.Current.playerHp = name == "death" ? 2 : GameSession.MAX_HP;
        GameSession.Current.NotifyChanged();
        s.Teleport(ph.x, ph.y, ph.facing);
        if (world.CameraCtl != null)
            world.CameraCtl.SnapToTarget();
        onTickAction = null;
        finish = null;
        ArenaTarget t = ph.target != 0 ? SpawnTarget(ph.target, ph.y) : null;
        onTickAction = rel =>
        {
            if (rel == 1 && t != null && t.Alive && name != "death")
                t.Kill(eKillReason.Cleared);
            if (rel == 20 && t != null && t.Alive)
                t.Kill(eKillReason.Cleared);
        };
        Directory.CreateDirectory(outDir);
        Shots.Clear();
        foreach (var k in ph.shots)
            Shots[k] = Path.Combine(outDir, "photo_" + name + "_" + k.ToString("000") + ".png");
        var ticks = NesReplay.Decode(ph.rle);
        s.ResetStats();
        NesPad.I.PlayReplay(ticks);
        Begin("photo_" + name, ticks.Count);
        return ticks.Count;
    }

    /// <summary>Về lại section Arena (sau khi chụp ảnh) — Simon ở điểm xuất phát.</summary>
    public static void BackToArena()
    {
        world = StageWorld.Current;
        foreach (var l in world.Sections)
            if (l.name == "Arena")
            {
                world.Warp(l, 48, 176);
                Simon.Teleport(48, 176, 1);
                GameSession.Current.playerHp = GameSession.MAX_HP;
                GameSession.Current.NotifyChanged();
            }
    }

    // ---------------------------------------------------------------- chấm

    private static void JumpChecks(int pressIdx, int x0, int dir)
    {
        int bad = 0, firstBad = -1;
        int land = -1;
        for (int t = 1; t <= 40; t++)
        {
            int i = pressIdx + t - 1;
            int h = 176 - Ys[i];
            if (h != VIDEO_JUMP_H[t])
            {
                bad++;
                if (firstBad < 0)
                    firstBad = t;
            }
            if (land < 0 && States[i] != eSimonState.Jump)
                land = t;
        }
        int apex = 0, hold = 0;
        for (int t = 1; t <= 40; t++)
        {
            int h = 176 - Ys[pressIdx + t - 1];
            if (h > apex)
            {
                apex = h;
                hold = 0;
            }
            if (h == apex)
                hold++;
        }
        int tuck = 0, tuckBad = 0;
        for (int t = 1; t <= 40; t++)
        {
            int i = pressIdx + t - 1;
            bool wantTuck = t >= 3 && t <= 29;
            bool isTuck = Frames[i] == "jumpduck_0";
            if (isTuck)
                tuck++;
            if (wantTuck != isTuck || (isTuck && DrawYs[i] != VIDEO_TUCK_DRAW))
                tuckBad++;
        }
        int dist = Xs[pressIdx + 39] - x0;
        Measures.Add("nhảy: cao " + apex + " px (video 36), đỉnh giữ " + hold + " tick (video 9), chạm đất tick " + land + " (video 40), xa " + dist
                     + " px (video " + (dir != 0 ? "40" : "0") + "); lệch bảng độ cao " + bad + "/40 tick" + (firstBad > 0 ? " (đầu tiên t" + firstBad + ")" : ""));
        Expect("độ cao từng tick = bảng video (số tick lệch)", 0, bad);
        Expect("chạm đất ở tick", 40, land);
        Expect("độ cao đỉnh", 36, apex);
        Expect("đỉnh giữ (tick ở 36 px)", 9, hold);
        Expect("tầm xa", dir * 40, dist);
        Expect("khung co chân tick 3–29 (+4 px) / đứng tick 1–2, 30–40 — số tick sai", 0, tuckBad);
    }

    private static void FallChecks(int fromY, int toY, bool crouch)
    {
        int start = IndexWhere(i => Ys[i] > fromY);
        int land = IndexWhere(i => i > start && Ys[i] == toY);
        int maxStep = 0;
        for (int i = Math.Max(1, start); i <= land && i > 0; i++)
            maxStep = Math.Max(maxStep, Ys[i] - Ys[i - 1]);
        int dx = land >= 0 && start >= 1 ? Xs[land] - Xs[start - 1] : -999;
        int crouchTicks = 0;
        for (int i = 0; i < States.Count; i++)
            if (States[i] == eSimonState.Land)
                crouchTicks++;
        Measures.Add("rơi " + (toY - fromY) + " px: bắt đầu tick " + start + " ở x " + (start >= 0 ? Xs[start] : -1) + ", " + (land - start + 1)
                     + " tick tới sàn, bước dọc lớn nhất " + maxStep + " px/tick (video 8), x đi thêm " + dx + " px khi rơi, ngồi khựng " + crouchTicks + " tick (video "
                     + (crouch ? "≈20" : "0") + ")");
        Expect("tốc độ rơi 8 px/tick", VIDEO_FALL_SPEED, maxStep);
        Expect("số tick rơi = cao / 8", (toY - fromY) / VIDEO_FALL_SPEED, land - start + 1);
        Expect("đáp đúng mặt sàn", toY, Ys[Ys.Count - 1]);
        // giữ đà ngang 1 px/tick suốt lúc rơi, trừ tick đầu bị tường chặn (mép trái Bục C: đầu chạm bục y 80–95 khi Y < 128 ⇒ 1 tick;
        // video 1:02.8: x 919→915 trong 8 tick rơi ±2 do capture)
        int blockedTicks = crouch ? 1 : 0;
        Expect("giữ đà ngang khi rơi (1 px/tick, trừ tick bị tường chặn)", Xs[start] > Xs[Math.Max(0, start - 1)] ? 1 : -1, Math.Sign(dx));
        Expect("số px ngang khi rơi", (toY - fromY) / VIDEO_FALL_SPEED - blockedTicks, Math.Abs(dx));
        if (crouch)
            ExpectRange("ngồi khựng (video 18–22 tick)", 18, 22, crouchTicks);
        else
            Expect("không ngồi khựng khi rơi 32 px (video 2:05.3)", 0, crouchTicks);
    }

    private static void StairChecks(int stairIndex, int off, int entryT)
    {
        var p = world.Tiles.GetStair(stairIndex);
        int bx = p.bottom.x + off * p.upDx;
        int offLine = 0, poseBad = 0, firstUp = -1, top = -1;
        for (int i = 0; i < Xs.Count; i++)
        {
            var st = States[i];
            if (top < 0 && StairTs[i] == p.Length && i > 0 && IsStair(States[i - 1]))
                top = i;
            if (!IsStair(st))
                continue;
            int t = StairTs[i];
            if (Xs[i] != bx + p.upDx * t || Ys[i] != p.bottom.y - t)
                offLine++;
            if (firstUp < 0 && st == eSimonState.StairUp)
                firstUp = i;
            // khung theo vị trí (video): lên — stairs_up_0 khi (t−1) mod 8 < 4, còn lại walk_1 vẽ +2; xuống — stairs_down_0 khi t mod 8 < 4
            if (st == eSimonState.StairUp || st == eSimonState.StairDown)
            {
                bool isUp = st == eSimonState.StairUp;
                bool a = isUp ? SimonController.Mod(t - 1, 8) < 4 : SimonController.Mod(t, 8) < 4;
                string want = isUp ? (a ? "stairs_up_0" : "stairs_up_1") : (a ? "stairs_down_0" : "stairs_down_1");
                int wantDy = isUp && !a ? 2 : 0;
                if (Frames[i] != want || DrawYs[i] != wantDy)
                    poseBad++;
            }
        }
        int dt = top - firstUp;
        Measures.Add("thang " + stairIndex + (p.upDx > 0 ? " ↗" : " ↖") + ": vào ở t " + (firstUp >= 0 ? StairTs[firstUp] : -1) + " (kẹp tại x hiện tại), tới đỉnh t "
                     + p.Length + " sau " + dt + " tick ⇒ " + (dt > 0 ? ((p.Length - entryT) / (float)(dt + 1)).ToString("0.000") : "?") + " px/tick (video 0.5); lệch đường thang "
                     + offLine + " tick; khung sai " + poseBad);
        Expect("tick đầu đã trên thang ở t = " + entryT + " (+0.5)", entryT, firstUp >= 0 ? StairTs[firstUp] : -1);
        Expect("tới đỉnh sau (len − t0)/0.5 − 1 tick", (p.Length - entryT) * 2 - 1, dt);
        Expect("mọi tick trên thang nằm trên đường x = path + lệch", 0, offLine);
        Expect("khung theo vị trí (video) — số tick sai", 0, poseBad);
        Expect("ra đỉnh đứng trên sàn trên (Y)", p.top.y, top >= 0 ? Ys[top] : -1);
        Expect("ra đỉnh ⇒ state đất (Idle)", (int)eSimonState.Idle, top >= 0 ? (int)States[top] : -1);
        Expect("ra đỉnh: x = đỉnh + lệch", p.top.x + off * p.upDx, top >= 0 ? Xs[top] : -1);
        Expect("xuống về chân: x = chân + lệch", bx, Xs[Xs.Count - 1]);
        Expect("xuống về chân: y", p.bottom.y, Ys[Ys.Count - 1]);
        Expect("cuối: đứng trên đất (Idle)", (int)eSimonState.Idle, (int)States[States.Count - 1]);
    }

    private static void HurtChecks(int facing, bool death)
    {
        int hit = IndexWhere(i => States[i] == eSimonState.Hurt);
        int land = IndexWhere(i => i > hit && States[i] != eSimonState.Hurt);
        int x0 = hit >= 0 ? Xs[hit] : -1;
        int minY = 999;
        int dxMax = 0;
        for (int i = hit; i < land && i >= 0; i++)
        {
            minY = Math.Min(minY, Ys[i]);
            dxMax = Math.Max(dxMax, Math.Abs(Xs[i] - x0));
        }
        int dxEnd = land > 0 ? Xs[land] - x0 : -999;
        int air = land - hit;
        int blinkStart = land;
        int blink = 0, hidden = 0, toggleBad = 0;
        for (int i = blinkStart; i >= 0 && i < Invs.Count && Invs[i] > 0; i++)
        {
            blink++;
            if (!Visibles[i])
                hidden++;
            if (i > blinkStart && Visibles[i] == Visibles[i - 1] && Invs[i] > 0)
                toggleBad++;
        }
        Measures.Add((death ? "chết" : "knockback") + " (mặt " + (facing > 0 ? "phải" : "trái") + "): trúng tick " + hit + ", lùi " + dxEnd + " px (video ∓12), cao "
                     + (176 - minY) + " px (video 14–15), bay " + air + " tick (video ≈36), nháy " + blink + " tick sau khi đáp (video 94), ẩn " + hidden + " tick, HP "
                     + Hps[Math.Max(0, hit)] + (death ? "" : "") + "; ma trận hướng: văng " + (dxEnd < 0 ? "trái" : "phải"));
        Expect("trúng đòn (Hurt) xảy ra", 1, hit >= 0 ? 1 : 0);
        Expect("văng NGƯỢC hướng mặt " + VIDEO_KB_DX + " px", -facing * VIDEO_KB_DX, dxEnd);
        ExpectRange("độ cao cung (video 14–15)", 14, 16, 176 - minY);
        ExpectRange("thời gian bay (video 36 ±2)", 34, 38, air);
        Expect("mất 2 vạch (Stage 1–6, wiki)", death ? 0 : GameSession.MAX_HP - 2, Hps[hit]);
        if (!death)
        {
            ExpectRange("nháy bất tử sau khi đáp (video 92–96)", 92, 96, blink);
            Expect("nháy 1 tick hiện / 1 tick ẩn (số lần không đổi)", 0, toggleBad);
            Expect("không nháy khi đang bay knockback (hiện suốt)", 1, AllVisible(hit, land) ? 1 : 0);
        }
        else
        {
            int dead = Index(States, eSimonState.Dead);
            Expect("hết máu ⇒ Dead ngay khi đáp", land, dead);
            Expect("khung chết: death_0 rồi death_1 sau DeathKneelTicks (16)", 1,
                dead >= 0 && dead + 16 < Frames.Count && Frames[dead] == "death_0" && Frames[dead + 16] == "death_1" ? 1 : 0);
            Expect("Died sau DeathTicks (124) kể từ khi ngã", dead + 124 - 1, DiedTick);
            Expect("hồi sinh đầy máu", GameSession.MAX_HP, Hps[Hps.Count - 1]);
            Measures.Add("chết: ngã tick " + dead + ", Died tick " + DiedTick + " (" + (DiedTick - dead + 1) + " tick — suy luận nhạc Death 2.07 s), hồi sinh ở (" + Xs[Xs.Count - 1] + "," + Ys[Ys.Count - 1] + ")");
        }
    }

    private static void DeathFallChecks(eTileCollision kind, int homeX)
    {
        int fd = Index(States, eSimonState.FallDeath);
        Measures.Add("chết " + kind + ": vào FallDeath tick " + fd + " ở x " + (fd >= 0 ? Xs[fd] : -1) + ", Died tick " + DiedTick + " loại " + DiedKind + ", hồi sinh x " + Xs[Xs.Count - 1]);
        Expect("vào FallDeath", 1, fd >= 0 ? 1 : 0);
        Expect("loại ô chết", (int)kind, (int)DiedKind);
        Expect("Died sau DeathTicks (state đổi từ bước kiểm ô chết ⇒ đếm từ tick sau)", fd + 124, DiedTick);
        Expect("hồi sinh về điểm xuất phát", homeX, Xs[Xs.Count - 1]);
        Expect("ẩn khi đã rơi ra khỏi đáy section", 0, Visibles[fd + 40] ? 1 : 0);
    }

    // ---------------------------------------------------------------- ghi

    private static void Begin(string label, int ticks)
    {
        Unhook();
        Label = label;
        Target = ticks;
        Recorded = 0;
        Hash = 2166136261u;
        Fails = 0;
        Xs.Clear(); Ys.Clear(); Fxs.Clear(); States.Clear(); Frames.Clear(); DrawYs.Clear(); DrawXs.Clear(); Facings.Clear(); Hps.Clear();
        Invs.Clear(); Visibles.Clear(); HurtTops.Clear(); HurtHeights.Clear(); StairTs.Clear(); Cams.Clear(); Checks.Clear(); Measures.Clear();
        PrevTickHashes.Clear();
        PrevTickHashes.AddRange(TickHashes);
        TickHashes.Clear();
        StartTick = world.TickCount;
        world.TickEnded += OnTickEnded;
        hooked = Simon;
        hooked.Died += OnDied;
        hooked.AutoWalkDone += OnAutoWalkDone;
        Running = true;
    }

    public static int StartTick { get; private set; }

    private static void Unhook()
    {
        if (world != null)
            world.TickEnded -= OnTickEnded;
        if (hooked != null)
        {
            hooked.Died -= OnDied;
            hooked.AutoWalkDone -= OnAutoWalkDone;
            hooked = null;
        }
        Running = false;
    }

    private static void OnDied(SimonController s, eTileCollision kind)
    {
        DiedCount++;
        DiedKind = kind;
        DiedTick = world.TickCount - StartTick - 1;
    }

    private static void OnAutoWalkDone(SimonController s)
    {
        AutoWalkDoneTick = world.TickCount - StartTick - 1;
    }

    private static void Mix(int v)
    {
        uint h = Hash;
        for (int i = 0; i < 4; i++)
        {
            h ^= (uint)(v & 0xFF);
            h *= 16777619u;
            v >>= 8;
        }
        Hash = h;
    }

    private static void OnTickEnded(StageWorld w)
    {
        if (!Running)
            return;
        var s = Simon;
        int rel = w.TickCount - StartTick - 1;
        Mix(rel);
        string frame = s.Anim != null && s.Anim.Current != null && s.Anim.Current.FrameCount > 0
            ? s.Anim.Current.frames[s.Anim.FrameIndex].name : "";
        Xs.Add(s.X);
        Ys.Add(s.Y);
        Fxs.Add(s.fx);
        States.Add(s.Fsm.CurState);
        Frames.Add(frame);
        DrawXs.Add(s.drawOffset.x);
        DrawYs.Add(s.drawOffset.y);
        Facings.Add(s.Facing);
        Hps.Add(GameSession.Current.playerHp);
        Invs.Add(s.InvincibleTicks);
        Visibles.Add(s.Anim == null || s.Anim.Visible);
        HurtTops.Add(s.Hurt.local.y);
        HurtHeights.Add(s.Hurt.local.height);
        StairTs.Add(s.StairTPx);
        int cam = w.CameraCtl != null ? w.CameraCtl.X : 0;
        Cams.Add(cam);
        Mix(s.fx); Mix(s.fy); Mix(s.Facing); Mix((int)s.Fsm.CurState); Mix(s.InvincibleTicks); Mix(s.Anim != null ? s.Anim.FrameIndex : 0);
        Mix(s.drawOffset.x); Mix(s.drawOffset.y); Mix(GameSession.Current.playerHp); Mix(s.StairTPx); Mix(cam); Mix(frame.Length > 0 ? frame[frame.Length - 1] : 0);
        for (int g = 1; g < Entity.GROUP_COUNT; g++)
        {
            var list = w.GroupList(g);
            for (int i = 0; i < list.Count; i++)
            {
                Mix(list[i].fx);
                Mix(list[i].fy);
                Mix(list[i].Alive ? 1 : 0);
            }
        }
        TickHashes.Add(Hash);
        string shot;
        if (Shots.TryGetValue(rel, out shot))
            ScreenCapture.CaptureScreenshot(shot);
        if (onTickAction != null)
            onTickAction(rel);
        if (++Recorded >= Target)
            Finish();
    }

    private static void Finish()
    {
        Unhook();
        NesPad.I.StopReplay();
        Shots.Clear();
        if (finish != null)
        {
            try
            {
                finish();
            }
            catch (Exception ex)
            {
                Checks.Add("    FAIL ngoại lệ khi chấm: " + ex.Message);
                Fails++;
            }
        }
        ClearExtras();
        if (!SuiteDone)
        {
            SuiteFails += Fails;
            uint h = SuiteHash;
            h ^= Hash;
            h *= 16777619u;
            SuiteHash = h;
            SuiteReports.Add(Report());
            NextInSuite();
        }
    }

    public static string Report()
    {
        var sb = new StringBuilder();
        sb.Append("== ").Append(Label).Append(": ").Append(Recorded).Append(" tick, hash ").Append(Hash.ToString("x8")).Append(", FAIL ").Append(Fails).Append('\n');
        foreach (var m in Measures)
            sb.Append("  đo: ").Append(m).Append('\n');
        foreach (var c in Checks)
            sb.Append(c).Append('\n');
        return sb.ToString();
    }

    /// <summary>Vết từng tick (debug): "rel x,y state khung dx,dy hp inv vis t cam".</summary>
    public static string Trace(int from, int to)
    {
        var sb = new StringBuilder();
        for (int i = Math.Max(0, from); i <= to && i < Xs.Count; i++)
            sb.Append(i).Append(' ').Append(Xs[i]).Append(',').Append(Ys[i]).Append(' ').Append(States[i]).Append(' ').Append(Frames[i])
              .Append(' ').Append(DrawXs[i]).Append(',').Append(DrawYs[i]).Append(" f").Append(Facings[i]).Append(" hp").Append(Hps[i])
              .Append(" inv").Append(Invs[i]).Append(Visibles[i] ? " V" : " _").Append(" t").Append(StairTs[i]).Append(" cam").Append(Cams[i]).Append('\n');
        return sb.ToString();
    }

    public static string FirstDiff()
    {
        int n = Math.Min(TickHashes.Count, PrevTickHashes.Count);
        for (int i = 0; i < n; i++)
            if (TickHashes[i] != PrevTickHashes[i])
                return "tick " + i;
        return TickHashes.Count == PrevTickHashes.Count ? "trùng" : "khác độ dài";
    }

    // ---------------------------------------------------------------- tiện ích

    private static ArenaTarget SpawnTarget(int x, int y)
    {
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<ArenaTarget>("Assets/Data/Prefabs/Test/ArenaTarget.prefab");
        if (prefab == null)
        {
            Debug.LogError("[SimonProbe] không tìm thấy prefab ArenaTarget (SpawnPoint của Arena)");
            return null;
        }
        var t = (ArenaTarget)world.Spawn(prefab, x, y, -1);
        t.touchDamage = true;
        extras.Add(t);
        return t;
    }

    private static void ClearExtras()
    {
        for (int i = 0; i < extras.Count; i++)
            if (extras[i] != null && extras[i].Alive)
                extras[i].Kill(eKillReason.Cleared);
        extras.Clear();
    }

    private static void Expect(string what, int want, int got)
    {
        bool ok = want == got;
        if (!ok)
            Fails++;
        Checks.Add("    " + (ok ? "ĐẠT " : "FAIL ") + what + ": kỳ vọng " + want + ", được " + got);
    }

    private static void ExpectRange(string what, int lo, int hi, int got)
    {
        bool ok = got >= lo && got <= hi;
        if (!ok)
            Fails++;
        Checks.Add("    " + (ok ? "ĐẠT " : "FAIL ") + what + ": kỳ vọng " + lo + "–" + hi + ", được " + got);
    }

    private static bool IsStair(eSimonState s)
    {
        return s == eSimonState.StairIdle || s == eSimonState.StairUp || s == eSimonState.StairDown;
    }

    private static bool AllVisible(int a, int b)
    {
        for (int i = Math.Max(0, a); i < b && i < Visibles.Count; i++)
            if (!Visibles[i])
                return false;
        return true;
    }

    private static int Index(List<eSimonState> l, eSimonState s)
    {
        for (int i = 0; i < l.Count; i++)
            if (l[i] == s)
                return i;
        return -1;
    }

    private static int IndexWhere(Func<int, bool> f)
    {
        for (int i = 0; i < Xs.Count; i++)
            if (f(i))
                return i;
        return -1;
    }

    private static int Max(List<int> l)
    {
        int m = int.MinValue;
        foreach (var v in l)
            m = Math.Max(m, v);
        return m;
    }

    private static List<int> RunLengths(List<string> l, int a, int b)
    {
        var r = new List<int>();
        int n = 0;
        for (int i = a; i <= b && i < l.Count; i++)
        {
            if (i > a && l[i] != l[i - 1])
            {
                r.Add(n);
                n = 0;
            }
            n++;
        }
        r.Add(n);
        return r;
    }

    private static bool FrameOrderOk(string[] cycle)
    {
        var seq = new List<string>();
        for (int i = 0; i < 120 && i < Frames.Count; i++)
            if (seq.Count == 0 || seq[seq.Count - 1] != Frames[i])
                seq.Add(Frames[i]);
        for (int i = 0; i < seq.Count; i++)
            if (seq[i] != cycle[i % cycle.Length])
                return false;
        return true;
    }

    private static string Join(List<int> l)
    {
        var sb = new StringBuilder();
        foreach (var v in l)
            sb.Append(sb.Length > 0 ? "," : "").Append(v);
        return sb.ToString();
    }
}
#endif
