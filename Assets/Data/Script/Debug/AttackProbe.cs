#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness kiểm tấn công của Simon (CVGP 09; routine 10/16 chạy lại) — CHỈ Editor, scene Arena (Player = Simon). Qua MCP khi Play:
///   AttackProbe.SaveReplays()              ⇒ Assets/Data/Tests/Replays/attack_&lt;tên&gt;.txt
///   AttackProbe.RunSuite(AttackProbe.ScenarioNames()) ⇒ chạy (input ĐỌC TỪ FILE), ghi từng tick, TỰ CHẤM ⇒ SuiteReport()
///   AttackProbe.TrajectoryCsv()            ⇒ vết vũ khí phụ kịch bản vừa chạy (so video bằng Tools/cvgp/attack_compare.py)
///   AttackProbe.RunPhoto(tên, thư mục)     ⇒ dựng lại khung video ở section Map 1 thật, chụp Game view (ghép bằng attack_pairs.py)
/// Kỳ vọng là hằng VIDEO_* / SHEET_* (đo video, đọc sheet) — độc lập với ConfigSimon/ConfigWhip/ConfigSubWeapon.
/// Arena: sàn y 176; stair0 ↗ chân (620,176) đỉnh (684,112) ⇒ đường Simon x = đường bậc + 3.
/// </summary>
public static class AttackProbe
{
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";

    // ---------------------------------------------------------------- kỳ vọng VIDEO / SHEET (CVGP_MEMORY §2h)
    /// <summary>Nhịp 3 khung roi/ném (video Map_1 52 cú roi: TB 5.85 / 7.0 / 8.73 ⇒ 6 / 7 / 9).</summary>
    public static readonly int[] VIDEO_PHASE = { 6, 7, 9 };
    public const int VIDEO_DAGGER_SPEED = 4;          // px/tick (1:33.0, 1:47.1)
    public const int VIDEO_DAGGER_SPAWN_H = 23;       // pivot dao = Y − 23 (góc trên sprite Y − 31: f2797, f3214)
    public const int VIDEO_SPAWN_TICK = 3;            // vũ khí sinh 2.5–5 tick sau khi khung duỗi bắt đầu
    public const int VIDEO_STOPWATCH_TICKS = 186;     // video:Map_2 (đo:cvm3-03 — CVM3_MEMORY §2d; wiki 5 s = 300 sai)
    public const int VIDEO_FIRE_TICKS = 76;           // CVM3 04: video Map_3 4 chuỗi lửa trọn 75–78 (CVGP 09: ≈ 80 — video Boss)
    // CVM3 04: lọ nước thánh — video Map_3 9 lọ trận Mummy + 1 nhảy-ném (46 điểm dư 0 px) + Boss_Map_1 3 lọ (rms ≤ 0.7): x = X0 ± floor(1.5 n),
    // vy += g rồi y += vy (vy0 −1.5, g 33/256), sinh ở (X Simon, Y chân − 19)
    public const int VIDEO_HOLY_SPAWN_H = 19;
    public const int VIDEO_HOLY_VY0_256 = -384;
    public const int VIDEO_HOLY_G_256 = 33;

    /// <summary>Vị trí lọ theo mô hình video sau n tick bay (n = 0 lúc sinh), toạ độ px (floor như fixed-point 1/256).</summary>
    public static Vector2Int VideoHolyPos(int x0, int y0, int dir, int n)
    {
        int fx = x0 * 256 + dir * 384 * n;
        int fy = y0 * 256 + VIDEO_HOLY_VY0_256 * n + VIDEO_HOLY_G_256 * n * (n + 1) / 2;
        return new Vector2Int(Mathf.FloorToInt(fx / 256f), Mathf.FloorToInt(fy / 256f));
    }
    /// <summary>Góc trên-trái mảnh roi duỗi so với pivot thân, quay TRÁI (sheet — CVGP_MEMORY §3 roi tách lớp): [lên thang?][cấp].</summary>
    static readonly Vector2Int[] SHEET_EXT_OFF = { new Vector2Int(0, 0), new Vector2Int(-36, -33), new Vector2Int(-36, -32), new Vector2Int(-52, -32) };
    static readonly Vector2Int[] SHEET_EXT_OFF_UP = { new Vector2Int(0, 0), new Vector2Int(-36, -31), new Vector2Int(-36, -30), new Vector2Int(-52, -30) };
    /// <summary>Bbox pixel đục mảnh roi duỗi (sheet whip_l1_ext / l2_ext / l3cN_ext): x, y, w, h.</summary>
    static readonly RectInt[] SHEET_EXT_BOX = { new RectInt(), new RectInt(0, 8, 24, 8), new RectInt(0, 10, 24, 6), new RectInt(0, 10, 40, 6) };

    private struct Scn
    {
        public string name, rle, note;
        public Scn(string n, string r, string note) { name = n; rle = r; this.note = note; }
    }

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("l1_stand", "-*2 B -*30", "roi cấp 1 đứng quay phải (300,176); bia A ở tầm đầu roi (chạm 1 px), bia B ngay ngoài tầm"),
        new Scn("l2_stand", "-*2 B -*30", "roi cấp 2 đứng"),
        new Scn("l3_stand", "-*2 B -*30", "roi cấp 3 đứng (đổi 4 màu mỗi tick)"),
        new Scn("l1_duck", "D*2 D+B D*30 -*4", "roi cấp 1 ngồi quay trái"),
        new Scn("l3_duck", "D*2 D+B D*30 -*4", "roi cấp 3 ngồi quay trái"),
        new Scn("l1_air", "-*2 A -*9 B -*40", "nhảy tại chỗ, roi cấp 1 ở tick nhảy 11"),
        new Scn("l3_air", "-*2 R+A R*9 R+B R*40", "nhảy kèm →, roi cấp 3 ở tick nhảy 11 (giữ quỹ đạo, sau đó co chân)"),
        new Scn("l2_stairs_up", "U*29 -*20 B -*30", "lên stair0, thả (đi nốt tới bậc), roi cấp 2 trên thang (lên)"),
        new Scn("l3_stairs_down", "U*29 -*20 D*2 -*20 B -*30", "lên stair0, xuống 1 tick, đứng, roi cấp 3 (tư thế xuống thang)"),
        new Scn("rewhip", "-*2 B -*10 B -*10 B -*30", "bấm B giữa đòn (bỏ qua) + ngay khi hết đòn (đòn 2 bắt đầu)"),
        new Scn("dagger", "-*2 Sub -*60", "dao (phím tắt) quay phải; bia ở x +60"),
        new Scn("dagger_upb", "-*2 U+B -*30", "dao bằng ↑+B"),
        new Scn("axe", "-*2 Sub -*70", "rìu quay phải"),
        new Scn("holy", "-*2 Sub -*130", "nước thánh quay phải; bia đứng chỗ lửa"),
        new Scn("cross", "-*2 Sub -*120", "thập tự quay phải; bia ở x +40 (trúng lượt đi + lượt về), bắt lại khi về"),
        new Scn("stopwatch", "-*2 Sub -*40 Sub -*300", "đồng hồ (5 tim): bia đóng băng 186 tick (video) + roi vẫn vung; dùng lần 2 (sau 41 tick) khi đang hiệu lực ⇒ −5 tim nữa, đóng băng tính lại (video Map_2 f6928)"),
        new Scn("limit1", "-*2 Sub -*22 Sub -*30", "Shot I: dao 2 khi dao 1 còn bay ⇒ vung roi"),
        new Scn("limit2", "-*2 Sub -*22 Sub -*22 Sub -*30", "Shot II: 2 dao, dao 3 ⇒ roi"),
        new Scn("limit3", "-*2 Sub -*22 Sub -*22 Sub -*22 Sub -*30", "Shot III: 3 dao, dao 4 ⇒ roi"),
        new Scn("nohearts", "-*2 Sub -*30", "0 tim ⇒ vung roi, tim giữ 0"),
        new Scn("substairs", "-*2 Sub -*30", "phím tắt ở chân thang ⇒ ném, KHÔNG leo thang"),
    };

    // ---------------------------------------------------------------- ghi
    private struct Rec
    {
        public int x, y, facing, hearts, ticksA, hitsA, hitsB, drawY, jumpTick;
        public eSimonState state;
        public string body, overlay;
        public bool whipOn;
        public RectInt whip;
        public string proj;
    }

    public static string Label { get; private set; } = "";
    public static bool Running { get; private set; }
    public static int Fails { get; private set; }
    public static uint Hash { get; private set; }
    private static readonly List<Rec> recs = new List<Rec>();
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> measures = new List<string>();
    private static readonly StringBuilder traj = new StringBuilder();
    public static readonly Dictionary<int, string> Shots = new Dictionary<int, string>();
    private static readonly Queue<string> suite = new Queue<string>();
    public static readonly List<string> SuiteReports = new List<string>();
    public static bool SuiteDone { get; private set; } = true;
    public static int SuiteFails { get; private set; }
    public static uint SuiteHash { get; private set; }
    private static StageWorld world;
    private static int target, recorded, startTick;
    private static Action finish;
    private static Action<int> onTick;
    private static ArenaTarget tA, tB;
    private static readonly List<Entity> extras = new List<Entity>();

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string ReplayPath(string name) { return REPLAY_DIR + "/attack_" + name + ".txt"; }

    public static int SaveReplays()
    {
        Directory.CreateDirectory(REPLAY_DIR);
        foreach (var s in SCENARIOS)
        {
            var ticks = NesReplay.Decode(s.rle);
            var sb = new StringBuilder();
            sb.Append("# ").Append(ticks.Count).Append(" tick, hash ").Append(NesReplay.Hash(ticks).ToString("x8")).Append('\n');
            sb.Append("# CVGP 09 AttackProbe kịch bản '").Append(s.name).Append("': ").Append(s.note).Append('\n');
            sb.Append(NesReplay.Encode(ticks)).Append('\n');
            File.WriteAllText(ReplayPath(s.name), sb.ToString(), new UTF8Encoding(false));
        }
        UnityEditor.AssetDatabase.Refresh();
        return SCENARIOS.Length;
    }

    public static string ScenarioNames()
    {
        var sb = new StringBuilder();
        foreach (var s in SCENARIOS)
            sb.Append(sb.Length > 0 ? "," : "").Append(s.name);
        return sb.ToString();
    }

    public static int RunSuite(string names)
    {
        Unhook();
        suite.Clear();
        SuiteReports.Clear();
        SuiteFails = 0;
        SuiteHash = 2166136261u;
        foreach (var n in names.Split(','))
            if (n.Trim().Length > 0)
                suite.Enqueue(n.Trim());
        SuiteDone = false;
        NextInSuite();
        return suite.Count;
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
        sb.Append("ATTACK SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
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
            Debug.LogError("[AttackProbe] cần scene Arena với Player = SimonController");
            return -1;
        }
        string p = ReplayPath(scenario);
        if (!File.Exists(p))
        {
            Debug.LogError("[AttackProbe] thiếu " + p + " — gọi AttackProbe.SaveReplays()");
            return -1;
        }
        var ticks = NesReplay.Decode(File.ReadAllText(p));
        ClearExtras();
        world.ClearNonPlayer();
        world.ResetRng();
        var ses = GameSession.Current;
        ses.playerHp = GameSession.MAX_HP;
        ses.hearts = 20;
        ses.whipLevel = 1;
        ses.shotMultiplier = 1;
        ses.subWeapon = eSubWeapon.None;
        ses.subWeaponHits = 0;
        ses.NotifyChanged();
        finish = null;
        onTick = null;
        tA = tB = null;
        switch (scenario)
        {
            case "l1_stand":
            case "l2_stand":
            case "l3_stand":
                {
                    int lv = scenario[1] - '0';
                    ses.whipLevel = lv;
                    s.Teleport(300, 176, 1);
                    int reach = -SHEET_EXT_OFF[lv].x;
                    tA = Target(300 + reach + 7, 176);   // hurtbox [x−8, x+8) chạm đầu roi 1 px
                    tB = Target(300 + reach + 8 + 40, 176); // xa hẳn — đặt lại ở tick 0 thành ngay ngoài tầm
                    onTick = rel => { if (rel == 0) tB.SetPositionPx(300 + reach + 8, 176); };
                    finish = () => WhipChecks(lv, 2, eAttackPose.Stand, 1, 0, true);
                }
                break;
            case "l1_duck":
            case "l3_duck":
                {
                    int lv = scenario[1] - '0';
                    ses.whipLevel = lv;
                    s.Teleport(300, 176, -1);
                    tA = Target(300 - (-SHEET_EXT_OFF[lv].x) - 7, 176);
                    finish = () => WhipChecks(lv, 2, eAttackPose.Duck, -1, 8, false);
                }
                break;
            case "l1_air":
            case "l3_air":
                {
                    int lv = scenario[1] - '0';
                    ses.whipLevel = lv;
                    s.Teleport(300, 176, 1);
                    finish = () => AirChecks(lv, lv == 3 ? 1 : 0);
                }
                break;
            case "l2_stairs_up":
                ses.whipLevel = 2;
                s.Teleport(626, 176, 1);
                finish = () => StairWhipChecks(2, 49, eAttackPose.StairsUp);
                break;
            case "l3_stairs_down":
                ses.whipLevel = 3;
                s.Teleport(626, 176, 1);
                finish = () => StairWhipChecks(3, 71, eAttackPose.StairsDown);
                break;
            case "rewhip":
                ses.whipLevel = 3;
                s.Teleport(300, 176, 1);
                finish = () =>
                {
                    // đòn 1 hết ở tick 24 (Execute đầu) ⇒ Idle chạy nối cùng tick thấy B ⇒ đòn 2 bắt đầu NGAY tick 24 (state không rời Attack)
                    int a = FirstState(eSimonState.Attack, 0);
                    int end = FirstNot(eSimonState.Attack, a);
                    Expect("bấm B giữa đòn (tick 13) bị bỏ qua: tick 13 vẫn khung vắt của đòn 1", 1, recs[13].body == "use_stand_1" ? 1 : 0);
                    Expect("tick 24 (= tick đòn 1 kết thúc) đã là khung chuẩn bị của đòn 2", 1, recs[24].body == "use_stand_0" && recs[23].body.StartsWith("use_stand_") && !recs[23].body.EndsWith("_0") && !recs[23].body.EndsWith("_1") ? 1 : 0);
                    Expect("2 đòn liền: Attack liên tục 44 tick", 44, end - a);
                    Expect("đếm đòn", 2, s.whipCount);
                    measures.Add("vung liên tục: đòn 2 bắt đầu " + (24 - a) + " tick sau đòn 1, không trễ thêm (video: bắt đầu→bắt đầu ngắn nhất 28 tick — do người chơi)");
                };
                break;
            case "dagger":
            case "dagger_upb":
                ses.subWeapon = eSubWeapon.Dagger;
                s.Teleport(300, 176, 1);
                tA = Target(360, 176);
                finish = () => DaggerChecks(scenario == "dagger");
                break;
            case "axe":
                ses.subWeapon = eSubWeapon.Axe;
                s.Teleport(300, 176, 1);
                finish = () => ProjChecks("Axe");
                break;
            case "holy":
                ses.subWeapon = eSubWeapon.HolyWater;
                s.Teleport(300, 176, 1);
                tA = Target(345, 176);
                finish = HolyChecks;
                break;
            case "cross":
                ses.subWeapon = eSubWeapon.Cross;
                s.Teleport(300, 176, 1);
                tA = Target(340, 176);
                finish = CrossChecks;
                break;
            case "stopwatch":
                ses.subWeapon = eSubWeapon.Stopwatch;
                s.Teleport(300, 176, 1);
                tA = Target(400, 176);
                finish = StopwatchChecks;
                break;
            case "limit1":
            case "limit2":
            case "limit3":
                ses.subWeapon = eSubWeapon.Dagger;
                ses.shotMultiplier = scenario[5] - '0';
                s.Teleport(20, 176, 1);   // sát mép trái ⇒ dao bay ~61 tick mới ra khỏi màn
                finish = () => LimitChecks(scenario[5] - '0');
                break;
            case "nohearts":
                ses.subWeapon = eSubWeapon.Dagger;
                ses.hearts = 0;
                s.Teleport(300, 176, 1);
                finish = () =>
                {
                    Expect("0 tim ⇒ không ném (vung roi)", 0, s.throwCount);
                    Expect("0 tim ⇒ roi", 1, s.whipCount);
                    Expect("tim giữ 0", 0, recs[recs.Count - 1].hearts);
                    Expect("không có vũ khí phụ nào trên màn", 0, CountProj());
                };
                break;
            case "substairs":
                ses.subWeapon = eSubWeapon.Dagger;
                s.Teleport(623, 176, 1);   // đúng chân stair0 (đường Simon = chân + 3)
                finish = () =>
                {
                    Expect("phím tắt ở chân thang ⇒ ném (throwCount)", 1, s.throwCount);
                    int up = FirstState(eSimonState.StairUp, 0);
                    Expect("không leo thang (không có tick StairUp)", -1, up);
                    Expect("Y giữ 176", 176, recs[recs.Count - 1].y);
                };
                break;
            default:
                Debug.LogError("[AttackProbe] kịch bản lạ " + scenario);
                return -1;
        }
        s.ResetStats();
        s.whipCount = s.throwCount = s.throwFailCount = 0;
        NesPad.I.PlayReplay(ticks);
        Begin(scenario, ticks.Count);
        return ticks.Count;
    }

    // ---------------------------------------------------------------- chấm

    /// <summary>Hộp roi kỳ vọng (px section) theo SHEET khi Simon ở (x,y) quay facing, vẽ thêm drawY.</summary>
    static RectInt ExpectedWhip(int lv, bool up, int x, int y, int facing, int drawX, int drawY)
    {
        var off = up ? SHEET_EXT_OFF_UP[lv] : SHEET_EXT_OFF[lv];
        var b = SHEET_EXT_BOX[lv];
        int lx = off.x + b.x;                               // quay trái
        int sx = facing > 0 ? x + drawX - (lx + b.width) : x + drawX + lx;
        int sy = y + 1 + drawY + off.y + b.y;               // pivot sprite vẽ ở Y + 1 (viewOffset)
        return new RectInt(sx, sy, b.width, b.height);
    }

    static void PhaseChecks(int pressRel, string prefix)
    {
        // khung theo pha: index (pressRel .. pressRel + 21)
        int[] cnt = new int[3];
        int bad = 0;
        for (int i = 0; i < 22; i++)
        {
            int ph = i < VIDEO_PHASE[0] ? 0 : i < VIDEO_PHASE[0] + VIDEO_PHASE[1] ? 1 : 2;
            var r = recs[pressRel + i];
            if (r.state != eSimonState.Attack)
                bad++;
            string want = "_" + ph;
            bool ok = ph < 2 ? r.body.EndsWith(want) : (r.body.EndsWith("_2") || r.body.EndsWith("_3") || r.body.EndsWith("_4") || r.body.EndsWith("_5"));
            if (!ok || !r.body.StartsWith(prefix))
                bad++;
            else
                cnt[ph]++;
        }
        Expect("khung thân " + prefix + "* đúng pha 6/7/9 suốt 22 tick (số tick sai)", 0, bad);
        Expect("hết đòn ở tick 22 sau khi bấm", 1, recs[pressRel + 22].state != eSimonState.Attack ? 1 : 0);
        measures.Add("khung: chuẩn bị " + cnt[0] + " / vắt " + cnt[1] + " / duỗi " + cnt[2] + " tick (video 6/7/9)");
    }

    static void WhipChecks(int lv, int press, eAttackPose pose, int facing, int drawY, bool edge)
    {
        var s = Simon;
        string prefix = pose == eAttackPose.Duck ? "use_duck_" : "use_stand_";
        PhaseChecks(press, prefix);
        int on = 0, first = -1, wrong = 0;
        var want = ExpectedWhip(lv, false, 300, 176, facing, 0, drawY);
        for (int i = 0; i < recs.Count; i++)
            if (recs[i].whipOn)
            {
                on++;
                if (first < 0)
                    first = i;
                if (!recs[i].whip.Equals(want))
                    wrong++;
            }
        Expect("hitbox roi bật đúng khung duỗi (9 tick)", VIDEO_PHASE[2], on);
        Expect("hitbox bật từ tick đầu khung duỗi", press + VIDEO_PHASE[0] + VIDEO_PHASE[1], first);
        Expect("hộp roi = bbox mảnh roi duỗi sheet (số tick lệch)", 0, wrong);
        var w = first >= 0 ? recs[first].whip : new RectInt();
        measures.Add("roi L" + lv + " " + pose + ": hộp x " + w.xMin + "–" + (w.xMax - 1) + " y " + w.yMin + "–" + (w.yMax - 1) + " (Simon x 300; tầm đầu roi "
                     + (facing > 0 ? w.xMax - 300 : 300 - w.xMin) + " px — sheet " + (-SHEET_EXT_OFF[lv].x) + ")");
        if (tA != null)
            Expect("bia chạm đầu roi 1 px ⇒ trúng đúng 1 lần/đòn", 1, recs[recs.Count - 1].hitsA);
        if (edge && tB != null)
            Expect("bia ngay ngoài tầm (cách 0 px) ⇒ không trúng", 0, recs[recs.Count - 1].hitsB);
        int moved = 0;
        for (int i = press; i < press + 22; i++)
            if (recs[i].x != 300 || recs[i].y != 176)
                moved++;
        Expect("đứng yên khi quất trên đất (video 2:04.5)", 0, moved);
        Expect("vẽ thêm theo tư thế (ngồi +8)", drawY, recs[press + 1].drawY);
        if (lv == 3)
            ColorChecks(press);
        Expect("hướng mặt", facing, recs[press].facing);
    }

    static void ColorChecks(int press)
    {
        int bad = 0;
        for (int i = 0; i < 22; i++)
        {
            string want = "l3c" + (i % 4 + 1) + "_";
            if (recs[press + i].overlay.IndexOf(want, StringComparison.Ordinal) < 0)
                bad++;
        }
        Expect("roi cấp 3 đổi màu c1→c2→c3→c4 mỗi tick (video: 2 frame cách 2 tick lệch 2 màu) — số tick sai", 0, bad);
    }

    static void AirChecks(int lv, int dir)
    {
        var s = Simon;
        int press = 2;
        int atk = FirstState(eSimonState.Attack, 0);
        Expect("đòn trên không bắt đầu ở tick nhảy 11 (B ở tick 12 của replay)", press + 10, atk);
        PhaseChecks(atk, "use_stand_");
        // quỹ đạo nhảy giữ nguyên: độ cao = bảng video, x = 1 px/tick
        int bad = 0;
        for (int t = 1; t <= 40; t++)
        {
            int i = press + t - 1;
            if (176 - recs[i].y != SimonProbe.VIDEO_JUMP_H[t] || recs[i].x != 300 + dir * t)
                bad++;
        }
        Expect("quỹ đạo nhảy không đổi khi quất (40 tick, số tick lệch)", 0, bad);
        int after = atk + 22;
        Expect("sau đòn quay về Jump", (int)eSimonState.Jump, (int)recs[after].state);
        Expect("sau đòn co chân (jumpduck_0, video 2:14.6)", 1, recs[after].body == "jumpduck_0" ? 1 : 0);
        Expect("đáp đất ở tick nhảy 40", (int)eSimonState.Jump, (int)recs[press + 38].state);
        var want = ExpectedWhip(lv, false, recs[atk + 13].x, recs[atk + 13].y, 1, 0, 0);
        Expect("hộp roi trên không bám Simon (tick duỗi đầu)", 1, recs[atk + 13].whip.Equals(want) ? 1 : 0);
        measures.Add("roi trên không L" + lv + ": bắt đầu tick nhảy " + (atk - press + 1) + ", về Jump ở tick " + (after - press + 1) + " (co chân), đáp tick 40");
    }

    static void StairWhipChecks(int lv, int press, eAttackPose pose)
    {
        string prefix = pose == eAttackPose.StairsUp ? "use_stairs_up_" : "use_stairs_down_";
        int atk = FirstState(eSimonState.Attack, 0);
        Expect("đòn trên thang bắt đầu đúng tick bấm", press, atk);
        PhaseChecks(atk, prefix);
        int moved = 0;
        for (int i = atk; i < atk + 22; i++)
            if (recs[i].x != recs[atk - 1].x || recs[i].y != recs[atk - 1].y)
                moved++;
        Expect("đứng nguyên bậc khi quất (số tick di chuyển)", 0, moved);
        Expect("sau đòn về StairIdle", (int)eSimonState.StairIdle, (int)recs[atk + 23].state);
        int drawX = pose == eAttackPose.StairsDown ? Simon.stairDownBack * 1 : 0;   // vẽ lùi khi xuống thang ↗ (upDx +1 — routine 08)
        var want = ExpectedWhip(lv, pose == eAttackPose.StairsUp, recs[atk].x, recs[atk].y, recs[atk].facing, drawX, 0);
        Expect("hộp roi trên thang = sheet (offset lên thang y −30/−31)", 1, recs[atk + 13].whip.Equals(want) ? 1 : 0);
        if (lv == 3)
            ColorChecks(atk);
        measures.Add("roi trên thang " + pose + " L" + lv + " ở (" + recs[atk].x + "," + recs[atk].y + ") quay " + recs[atk].facing + ": hộp "
                     + recs[atk + 13].whip + " — video KHÔNG có roi trên thang (ước lượng = khung sheet + nhịp roi đứng)");
    }

    static void DaggerChecks(bool shortcut)
    {
        var s = Simon;
        int press = 2;
        Expect("tim −1 ngay tick bấm", 19, recs[press].hearts);
        Expect("ném (throwCount)", 1, s.throwCount);
        int spawn = FirstProj(0);
        Expect("dao sinh ở tick duỗi + " + VIDEO_SPAWN_TICK, press + VIDEO_PHASE[0] + VIDEO_PHASE[1] + VIDEO_SPAWN_TICK, spawn);
        if (spawn < 0)
            return;
        var p0 = ProjPos(spawn);
        Expect("dao sinh tại pivot x", 300, p0.x);
        Expect("pivot dao = Y − 23 (video f2797)", 176 - VIDEO_DAGGER_SPAWN_H, p0.y);
        int bad = 0;
        for (int i = spawn + 1; i < recs.Count && i < spawn + 14 && recs[i].proj.Length > 0; i++)
        {
            var p = ProjPos(i);
            if (p.x != 300 + VIDEO_DAGGER_SPEED * (i - spawn) || p.y != p0.y)
                bad++;
        }
        Expect("bay 4 px/tick thẳng ngang (video 1:33.0) — số tick lệch", 0, bad);
        if (shortcut)
        {
            Expect("dao trúng bia 1 lần", 1, recs[recs.Count - 1].hitsA);
            int gone = -1;
            for (int i = spawn; i < recs.Count; i++)
                if (recs[i].proj.Length == 0)
                {
                    gone = i;
                    break;
                }
            Expect("dao biến mất ngay khi trúng (không xuyên — wiki)", 1, gone > spawn && gone < spawn + 16 ? 1 : 0);
            measures.Add("dao: sinh tick " + spawn + " tại (" + p0.x + "," + p0.y + "), trúng bia x 360 rồi biến mất ở tick " + gone);
        }
        Expect("thân dùng khung ném use_stand_* (không lớp roi)", 1, recs[press + 14].body.StartsWith("use_stand_") && recs[press + 14].overlay.Length == 0 ? 1 : 0);
    }

    static void ProjChecks(string kind)
    {
        int spawn = FirstProj(0);
        Expect(kind + " sinh ở tick duỗi + " + VIDEO_SPAWN_TICK, 2 + VIDEO_PHASE[0] + VIDEO_PHASE[1] + VIDEO_SPAWN_TICK, spawn);
        if (spawn < 0)
            return;
        var p0 = ProjPos(spawn);
        int apex = p0.y, apexT = 0, n = 0;
        for (int i = spawn; i < recs.Count && recs[i].proj.Length > 0; i++)
        {
            var p = ProjPos(i);
            if (p.y < apex)
            {
                apex = p.y;
                apexT = i - spawn;
            }
            n++;
        }
        measures.Add(kind + ": sinh (" + p0.x + "," + p0.y + "), đỉnh y " + apex + " (cao " + (p0.y - apex) + " px) sau " + apexT + " tick, sống " + n
                     + " tick — so quỹ đạo video bằng Tools/cvgp/attack_compare.py");
        Expect("rìu sinh tại pivot, pivot Y − 26 (tâm bbox Y − 27.5 — khớp parabol video)", 300 * 1000 + 150, p0.x * 1000 + p0.y);
        ExpectRange("đỉnh rìu cao 52–53 px trên điểm sinh (video: tâm đỉnh Y−80.5, sinh Y−28 ⇒ 52.5 ±2)", 50, 55, p0.y - apex);
    }

    static void HolyChecks()
    {
        int spawn = FirstProj(0);
        Expect("lọ sinh ở tick duỗi + " + VIDEO_SPAWN_TICK, 2 + VIDEO_PHASE[0] + VIDEO_PHASE[1] + VIDEO_SPAWN_TICK, spawn);
        int fire = -1, fireEnd = -1;
        for (int i = 0; i < recs.Count; i++)
        {
            if (fire < 0 && recs[i].proj.StartsWith("HolyFire"))
                fire = i;
            if (fire >= 0 && fireEnd < 0 && i > fire && recs[i].proj.Length == 0)
                fireEnd = i;
        }
        Expect("lọ vỡ thành lửa", 1, fire > 0 ? 1 : 0);
        if (fire < 0)
            return;
        var pf = ProjPos(fire);
        var p0 = ProjPos(spawn);
        Expect("lửa đứng trên mặt sàn y 176", 176, pf.y);
        ExpectRange("lửa sống 76 tick (video Map_3 75–78)", VIDEO_FIRE_TICKS - 1, VIDEO_FIRE_TICKS + 1, fireEnd - fire);
        // CVM3 04: quỹ đạo từng tick = mô hình video; vỡ khi tâm lọ chạm mặt sàn (n 32 — lửa ở x lọ tick đó)
        Expect("lọ sinh tại (X Simon, Y − " + VIDEO_HOLY_SPAWN_H + ") (video Map_3)", 300 * 1000 + 176 - VIDEO_HOLY_SPAWN_H, p0.x * 1000 + p0.y);
        int bad = 0, firstBad = -1;
        for (int i = spawn; i < fire; i++)
        {
            var want = VideoHolyPos(300, 176 - VIDEO_HOLY_SPAWN_H, 1, i - spawn);
            if (ProjPos(i) != want)
            {
                bad++;
                if (firstBad < 0) firstBad = i - spawn;
            }
        }
        Expect("quỹ đạo lọ từng tick = mô hình video (số tick lệch)", 0, bad);
        int nBreak = 0;
        while (VideoHolyPos(300, 176 - VIDEO_HOLY_SPAWN_H, 1, nBreak).y < 176)
            nBreak++;
        Expect("vỡ ở tick bay n đầu tiên tâm lọ ≥ mặt sàn (" + nBreak + ")", nBreak, fire - spawn);
        Expect("lửa ở x lọ tick vỡ", VideoHolyPos(300, 176 - VIDEO_HOLY_SPAWN_H, 1, nBreak).x, pf.x);
        measures.Add("nước thánh: sinh (" + p0.x + "," + p0.y + ") tick " + spawn + ", chạm sàn sau " + (fire - spawn) + " tick cách " + (pf.x - p0.x)
                     + " px (mô hình video Map_3: " + nBreak + " tick; video Boss_Map_1 lửa hiện ~rel 24 ⇒ n ≈ 32), lệch quỹ đạo " + bad + " tick"
                     + (firstBad >= 0 ? " (đầu n" + firstBad + ")" : "") + ", lửa " + (fireEnd - fire) + " tick, bia trúng " + recs[recs.Count - 1].hitsA + " lần");
        ExpectRange("bia đứng trong lửa trúng lặp mỗi 8 tick (≥ 9 lần)", 9, 11, recs[recs.Count - 1].hitsA);
        Expect("ném tốn 1 tim", 19, recs[recs.Count - 1].hearts);
    }

    static void CrossChecks()
    {
        var s = Simon;
        int spawn = FirstProj(0);
        int turn = -1, far = 0, caught = -1;
        for (int i = spawn + 1; i < recs.Count; i++)
        {
            if (recs[i].proj.Length == 0)
            {
                caught = i;
                break;
            }
            var p = ProjPos(i);
            far = Math.Max(far, p.x - 300);
            if (turn < 0 && p.x < ProjPos(i - 1).x)
                turn = i;
        }
        Expect("thập tự quay đầu", 1, turn > 0 ? 1 : 0);
        Expect("bắt lại được (biến mất khi về chạm Simon)", 1, caught > 0 ? 1 : 0);
        Expect("bia trúng lượt đi + lượt về", 2, recs[recs.Count - 1].hitsA);
        measures.Add("thập tự: đi xa " + far + " px, quay đầu tick " + (turn - spawn) + ", bắt lại tick " + (caught - spawn) + " (CHỈ WIKI — tốc độ/quãng là quy ước)");
    }

    static void StopwatchChecks()
    {
        var s = Simon;
        int spawn = FirstProj(0);
        Expect("tốn 5 tim", 15, recs[spawn].hearts);
        int frozen = 0;
        int last = recs.Count - 1;
        // bia đóng băng: số tick không chạy = tổng tick − tick đã chạy (từ sau khi sinh)
        int span = 0;
        for (int i = spawn + 1; i < recs.Count; i++)
        {
            span++;
            if (recs[i].ticksA == recs[i - 1].ticksA)
                frozen++;
        }
        int second = -1;
        for (int i = spawn + 1; i < recs.Count && second < 0; i++)
            if (recs[i].hearts == 10)
                second = i;
        // CVM3 03: dùng lại khi còn hiệu lực ⇒ đóng băng tính lại từ lần 2 (video Map_2 f6928 → Medusa chạy lại f7021)
        Expect("bia đóng băng = (lần 2 − lần 1) + 186 tick (video Map_2)", second - spawn + VIDEO_STOPWATCH_TICKS, frozen);
        Expect("dùng lần 2 khi đồng hồ còn hiệu lực ⇒ dùng được (video f6928)", 2, s.throwCount);
        Expect("đồng hồ = roi vẫn vung (không đếm roi riêng)", 0, s.whipCount);
        Expect("tim trừ 2 lần", 10, recs[last].hearts);
        measures.Add("đồng hồ: lần 2 sau " + (second - spawn) + " tick, bia dừng " + frozen + " tick / " + span + " tick theo dõi");
    }

    static void LimitChecks(int m)
    {
        var s = Simon;
        Expect("ném được đúng " + m + " dao (Shot " + m + ")", m, s.throwCount);
        Expect("lần bấm thừa ⇒ vung roi", 1, s.whipCount);
        int maxOn = 0;
        for (int i = 0; i < recs.Count; i++)
            maxOn = Math.Max(maxOn, Count(recs[i].proj, ';'));
        Expect("tối đa trên màn", m, maxOn);
        Expect("tim trừ đúng số lần ném", 20 - m, recs[recs.Count - 1].hearts);
    }

    // ---------------------------------------------------------------- ghi từng tick

    private static void Begin(string label, int ticks)
    {
        Unhook();
        Label = label;
        target = ticks;
        recorded = 0;
        Hash = 2166136261u;
        Fails = 0;
        recs.Clear();
        checks.Clear();
        measures.Clear();
        traj.Length = 0;
        traj.Append("rel,kind,x,y\n");
        startTick = world.TickCount;
        world.TickEnded += OnTickEnded;
        Running = true;
    }

    private static void Unhook()
    {
        if (world != null)
            world.TickEnded -= OnTickEnded;
        Running = false;
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
        int rel = w.TickCount - startTick - 1;
        var r = new Rec();
        r.x = s.X;
        r.y = s.Y;
        r.facing = s.Facing;
        r.state = s.Fsm.CurState;
        r.hearts = GameSession.Current.hearts;
        r.drawY = s.drawOffset.y;
        r.jumpTick = s.jumpTick;
        var a = s.Anim != null ? s.Anim.Current : null;
        int fi = s.Anim != null ? s.Anim.FrameIndex : 0;
        r.body = a != null && a.FrameCount > fi ? a.frames[fi].name : "";
        r.overlay = a != null && a.HasOverlay && a.overlays.Length > fi && a.overlays[fi] != null ? a.overlays[fi].name : "";
        r.whipOn = s.WhipHit.active;
        r.whip = s.WhipHit.active ? s.WhipHit.SectionRect : new RectInt();
        r.ticksA = tA != null ? tA.TicksRun : 0;
        r.hitsA = tA != null ? tA.HitsTaken : 0;
        r.hitsB = tB != null ? tB.HitsTaken : 0;
        var sb = new StringBuilder();
        var list = w.GroupList(eEntityGroup.PlayerWeapon);
        for (int i = 0; i < list.Count; i++)
        {
            var p = list[i] as SubWeaponProjectile;
            if (p == null || !p.Alive)
                continue;
            sb.Append(p.GetType().Name).Append(' ').Append(p.X).Append(' ').Append(p.Y).Append(';');
            traj.Append(rel).Append(',').Append(p.GetType().Name).Append(',').Append(p.X).Append(',').Append(p.Y).Append('\n');
            Mix(p.fx);
            Mix(p.fy);
        }
        r.proj = sb.ToString();
        recs.Add(r);
        Mix(rel); Mix(s.fx); Mix(s.fy); Mix(s.Facing); Mix((int)r.state); Mix(fi); Mix(r.hearts); Mix(r.whipOn ? r.whip.x * 7 + r.whip.y : -1);
        Mix(r.body.Length > 0 ? r.body[r.body.Length - 1] : 0); Mix(r.overlay.Length > 2 ? r.overlay[r.overlay.Length - 6] : 0); Mix(r.hitsA); Mix(r.ticksA);
        string shot;
        if (Shots.TryGetValue(rel, out shot))
            ScreenCapture.CaptureScreenshot(shot);
        if (onTick != null)
            onTick(rel);
        if (++recorded >= target)
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
                checks.Add("    FAIL ngoại lệ khi chấm: " + ex.Message);
                Fails++;
            }
        }
        lastTraj = traj.ToString();
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

    private static string lastTraj = "";

    /// <summary>Vết vũ khí phụ của kịch bản vừa chạy (CSV rel,kind,x,y — tâm, px section).</summary>
    public static string TrajectoryCsv() { return lastTraj; }

    public static string Report()
    {
        var sb = new StringBuilder();
        sb.Append("== ").Append(Label).Append(": ").Append(recorded).Append(" tick, hash ").Append(Hash.ToString("x8")).Append(", FAIL ").Append(Fails).Append('\n');
        foreach (var m in measures)
            sb.Append("  đo: ").Append(m).Append('\n');
        foreach (var c in checks)
            sb.Append(c).Append('\n');
        return sb.ToString();
    }

    /// <summary>Vết từng tick (debug).</summary>
    public static string Trace(int from, int to)
    {
        var sb = new StringBuilder();
        for (int i = Math.Max(0, from); i <= to && i < recs.Count; i++)
        {
            var r = recs[i];
            sb.Append(i).Append(' ').Append(r.x).Append(',').Append(r.y).Append(' ').Append(r.state).Append(' ').Append(r.body).Append('/').Append(r.overlay)
              .Append(" f").Append(r.facing).Append(" hp").Append(r.hearts).Append(r.whipOn ? " W" + r.whip : "").Append(" A").Append(r.hitsA).Append('/').Append(r.ticksA)
              .Append(" B").Append(r.hitsB).Append(' ').Append(r.proj).Append('\n');
        }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- ảnh so video (section Map 1 thật)
    private struct Photo
    {
        public string name, section, rle;
        public int x, y, facing, level;
        public eSubWeapon sub;
        public int[] shots;
        public Photo(string n, string sec, int x, int y, int f, int lv, eSubWeapon sub, string rle, params int[] shots)
        {
            name = n; section = sec; this.x = x; this.y = y; facing = f; level = lv; this.sub = sub; this.rle = rle; this.shots = shots;
        }
    }

    /// <summary>Khung video cùng pha (CVGP_MEMORY §7 routine 09): vị trí Simon lấy từ khớp mẫu sprite (fit_w09_*.csv).</summary>
    private static readonly Photo[] PHOTOS =
    {
        // f92–102 (0:03.07): CE cx 0, Simon x 66 Y 160 quay phải, roi cấp 1; f95 = vắt (tick 7), f98 = duỗi đầu (tick 13)
        new Photo("l1", "CastleEntrance", 66, 160, 1, 1, eSubWeapon.None, "B -*24", 7, 13),
        // f264–273 (0:08.80): CE cx 179, x màn 128 ⇒ section 307, roi cấp 2; f269 = duỗi đầu
        new Photo("l2", "CastleEntrance", 307, 160, 1, 2, eSubWeapon.None, "B -*24", 13),
        // f927–936 (0:30.9): EH cx 146, x màn 128 ⇒ section 274; f933 roi xanh (c3 ⇒ tick 14), f936 cam (c1 ⇒ tick 16)
        new Photo("l3", "EntranceHall", 274, 176, 1, 3, eSubWeapon.None, "B -*24", 14, 16),
        // f2770–2781 (1:32.33): CA cx 0, x 48 Y 80; f2777 roi đỏ (c4 ⇒ tick 15)
        new Photo("l3ca", "CryptA", 48, 80, 1, 3, eSubWeapon.None, "B -*24", 15),
        // f722–731 (0:24.1): EH cx 0, x 75, nhảy tại chỗ, roi cấp 3 ở đỉnh; f727 = duỗi đầu ⇒ A ở tick 0, B tick 5 ⇒ duỗi từ tick 18
        new Photo("air", "EntranceHall", 75, 176, 1, 3, eSubWeapon.None, "A -*4 B -*40", 18),
        // f2786–2800 (1:33.0): CA x 48 Y 80, ném dao; f2798 dao góc trên-trái x 60 = pivot − 8 + 20 ⇒ tick 16 + 5
        new Photo("dagger", "CryptA", 48, 80, 1, 3, eSubWeapon.Dagger, "Sub -*30", 13, 21),
        // f6237–6252 (3:27.9): PB cx 512, x màn 143 ⇒ section 655, rìu; f6242 tâm +16 px (tick 24), f6246 +31 px (tick 31)
        new Photo("axe", "PhantomBat", 655, 176, 1, 3, eSubWeapon.Axe, "Sub -*40", 24, 31),
    };

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
        var ses = GameSession.Current;
        ses.playerHp = GameSession.MAX_HP;
        ses.whipLevel = ph.level;
        ses.subWeapon = ph.sub;
        ses.hearts = 20;
        ses.shotMultiplier = 1;
        ses.NotifyChanged();
        s.Teleport(ph.x, ph.y, ph.facing);
        if (world.CameraCtl != null)
            world.CameraCtl.SnapToTarget();
        finish = null;
        onTick = null;
        tA = tB = null;
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

    public static void BackToArena()
    {
        world = StageWorld.Current;
        foreach (var l in world.Sections)
            if (l.name == "Arena")
            {
                world.Warp(l, 48, 176);
                Simon.Teleport(48, 176, 1);
            }
    }

    // ---------------------------------------------------------------- tiện ích

    static ArenaTarget Target(int x, int y)
    {
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<ArenaTarget>("Assets/Data/Prefabs/Test/ArenaTarget.prefab");
        var t = (ArenaTarget)world.Spawn(prefab, x, y, -1);
        t.touchDamage = false;
        t.Touch.active = false;
        extras.Add(t);
        return t;
    }

    static void ClearExtras()
    {
        for (int i = 0; i < extras.Count; i++)
            if (extras[i] != null && extras[i].Alive)
                extras[i].Kill(eKillReason.Cleared);
        extras.Clear();
    }

    static int FirstState(eSimonState st, int from)
    {
        for (int i = Math.Max(0, from); i < recs.Count; i++)
            if (recs[i].state == st)
                return i;
        return -1;
    }

    static int FirstNot(eSimonState st, int from)
    {
        for (int i = Math.Max(0, from); i < recs.Count; i++)
            if (recs[i].state != st)
                return i;
        return -1;
    }

    static int FirstProj(int from)
    {
        for (int i = from; i < recs.Count; i++)
            if (recs[i].proj.Length > 0)
                return i;
        return -1;
    }

    static int CountProj()
    {
        int n = 0;
        foreach (var r in recs)
            n = Math.Max(n, Count(r.proj, ';'));
        return n;
    }

    static Vector2Int ProjPos(int i)
    {
        var parts = recs[i].proj.Split(';')[0].Split(' ');
        return parts.Length >= 3 ? new Vector2Int(int.Parse(parts[1]), int.Parse(parts[2])) : new Vector2Int(-9999, -9999);
    }

    static int Count(string s, char c)
    {
        int n = 0;
        foreach (var ch in s)
            if (ch == c)
                n++;
        return n;
    }

    static void Expect(string what, int want, int got)
    {
        bool ok = want == got;
        if (!ok)
            Fails++;
        checks.Add("    " + (ok ? "ĐẠT " : "FAIL ") + what + ": kỳ vọng " + want + ", được " + got);
    }

    static void ExpectRange(string what, int lo, int hi, int got)
    {
        bool ok = got >= lo && got <= hi;
        if (!ok)
            Fails++;
        checks.Add("    " + (ok ? "ĐẠT " : "FAIL ") + what + ": kỳ vọng " + lo + "–" + hi + ", được " + got);
    }
}
#endif
