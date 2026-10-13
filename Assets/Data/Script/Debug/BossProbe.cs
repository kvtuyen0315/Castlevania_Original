#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness boss Phantom Bat (CVGP 14; routine 16/19/20/21 chạy lại) — CHỈ Editor, scene Arena (Player = Simon). Qua MCP khi Play:
///   BossProbe.SaveReplays()                       ⇒ Assets/Data/Tests/Replays/boss_phantombat_&lt;tên&gt;.txt
///   BossProbe.RunSuite(BossProbe.ScenarioNames()) ⇒ chạy (input ĐỌC TỪ FILE), ghi từng tick, TỰ CHẤM ⇒ SuiteReport()
///   BossProbe.ShotDir = thư mục ⇒ kịch bản wake_video / death_video chụp Game view đúng tick (Tools/cvgp/boss_pairs.py)
///   BossProbe.Traces[tên] / SaveTraces(dir) ⇒ vết dơi + Simon từng tick (Tools/cvgp/boss_compare.py so video)
/// Phòng boss = section PhantomBat có sẵn trong scene Arena (Warp) + BossArena tạo lúc chạy (KHÔNG lưu scene) — như cheat Warp Boss Room.
/// Kỳ vọng = hằng VIDEO_* / WIKI_* (đo video routine 14 — CVGP_MEMORY §2l, wiki) độc lập Config.
/// </summary>
public static class BossProbe
{
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";
    const string PREFAB = "Assets/Data/Prefabs/Boss/PhantomBat.prefab";

    // ---------------------------------------------------------------- kỳ vọng (CVGP_MEMORY §2l)
    public const int VIDEO_SLEEP_X = 623, VIDEO_SLEEP_Y = 41;          // tâm khung bay khi treo (màn (111,73) + cx 512)
    public const int VIDEO_WAKE_X_MIN = 693, VIDEO_WAKE_X_MAX = 696;   // Simon.X lúc dơi thức
    public const int VIDEO_WAKE_HOVER_MIN = 31, VIDEO_WAKE_HOVER_MAX = 35;
    public const int VIDEO_FIRST_END_Y = 82;                           // đáy cú lao đầu y màn 114 (2 video)
    public const int VIDEO_FIRST_END_X_MIN = 690, VIDEO_FIRST_END_X_MAX = 699;   // 693 (boss) / 690 (Map_1) ±… (Simon 696)
    public const int VIDEO_FIRST_T_MIN = 52, VIDEO_FIRST_T_MAX = 60;   // fitdive 54 / 60
    public const int VIDEO_GLIDE_SPEED = 192;                          // trục trội 0.75 px/tick (5 lần 0.72–0.77)
    public const int VIDEO_GLIDE_LOW_Y = 92, VIDEO_GLIDE_HIGH_Y_MIN = 121, VIDEO_GLIDE_HIGH_Y_MAX = 125;
    public const int VIDEO_GLIDE_MIN_DX = 64;                          // 5/5 lần ≥ 68
    public const int VIDEO_HOVER_MIN = 32, VIDEO_HOVER_MAX = 124;
    public const int VIDEO_RISE_TOP = 88;                              // y màn 120 (3 lần)
    public const int VIDEO_DIVE_MAX_Y = 156;                           // y màn 188
    public const int VIDEO_SCREEN_MAX = 238;
    public const int VIDEO_STUN_MIN = 8, VIDEO_STUN_MAX = 14;          // Map_1 3 lần khựng ≈ 10
    public const int VIDEO_DEATH_FX = 62, VIDEO_CRYSTAL_MIN = 150, VIDEO_CRYSTAL_MAX = 154;
    public const int VIDEO_CRYSTAL_X = 640, VIDEO_CRYSTAL_Y = 104;     // màn (128, 136) pivot
    public const int WIKI_SCORE = 3000, WIKI_HITS_WEAK = 12, WIKI_HITS_STRONG = 8, WIKI_DAMAGE_ST3 = 2;
    public const int WIKI_STOPWATCH = 300;

    private struct Scn
    {
        public string name, rle, note;
        public Scn(string n, string r, string note) { name = n; rle = r; this.note = note; }
    }

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("wake_video", "U*24 B -*451", "như video boss f50–160: Simon (680,144) trên bệ chân thang bấm Lên, leo thang ↗ tới x 696 rồi quất roi trên thang (video f61) và đứng ⇒ dơi thức, lượn 33, lao đầu, lượn chậm sang trái"),
        new Scn("cycle_floor", "-*1600", "Simon (739,176) đứng sàn bất tử (x màn 227 như video boss f315–360): ≥ 4 chu kỳ lượn/lơ lửng/bổ nhào/vọt lên — kiểm luật từng pha"),
        new Scn("cycle_seed_b", "-*1600", "như cycle_floor nhưng seed RNG khác ⇒ đích lượn / thời gian lơ lửng khác, luật như nhau (tất định theo seed)"),
        new Scn("touch", "-*900", "Simon (739,176) KHÔNG bất tử: cú bổ nhào từ đích cao (y 123) chạm thân ⇒ −2 vạch (Stage 3), Simon văng (video boss 11.87 s)"),
        new Scn("strong_kill", "B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 -*200 R*40 -*40", "roi cấp 2 (mạnh) × 8 vào dơi bị Stopwatch giữ: −2 vạch/đòn (video Map_1 rìu), +3000, lửa 62 tick, quả cầu sau 152 tick ở (640,104) rơi xuống sàn, Simon nhặt ⇒ TIME đứng + tally"),
        new Scn("weak_kill", "B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 B -*23 -*20", "roi cấp 1 (yếu) × 12 (wiki): vạch 15,14,12,11,10,8,7,6,4,3,2,0"),
        new Scn("stun", "B -*60", "roi trúng dơi đang bay ngang ⇒ đứng yên 10 tick (khung hurt) rồi bay tiếp; bất tử 16 tick"),
        new Scn("holy_hold", "-*200", "lửa nước thánh dưới dơi đang lơ lửng ⇒ dơi đứng yên suốt lúc lửa cháy (80 tick) + khựng, mất máu mỗi 16 tick (bất tử)"),
        new Scn("stopwatch", "-*420", "Stopwatch (wiki: có tác dụng với Phantom Bat): dơi đứng yên đúng 300 tick rồi bay tiếp"),
        new Scn("rosary", "-*60", "Rosary không diệt boss (wiki Items#Rosary)"),
        new Scn("walls", "L*220", "camera khoá cx 512 ⇒ tường vô hình mép màn: Simon đi trái dừng ở x 520 (cx + 8)"),
        new Scn("death_video", "-*240", "như Map_1 f6305: dơi lơ lửng (723,121), Simon (655,176) — hạ ở tick 10 ⇒ chụp lửa chết + quả cầu (so video)"),
    };

    // ---------------------------------------------------------------- ghi
    private struct Rec
    {
        public int bx, by, sx, sy, php, ebars, score, fx, items, cx, stun;
        public string state;
        public bool alive, dying;
    }

    public static string Label { get; private set; } = "";
    public static bool Running { get; private set; }
    public static int Fails { get; private set; }
    public static uint Hash { get; private set; }
    public static string ShotDir;
    private static readonly List<Rec> recs = new List<Rec>();
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> measures = new List<string>();
    private static readonly Queue<string> suite = new Queue<string>();
    public static readonly List<string> SuiteReports = new List<string>();
    public static readonly Dictionary<string, string> Traces = new Dictionary<string, string>();
    public static bool SuiteDone { get; private set; } = true;
    public static int SuiteFails { get; private set; }
    public static uint SuiteHash { get; private set; }
    private static StageWorld world;
    private static int target, recorded, startTick;
    private static Action finish;
    private static Action<int> onTick;
    private static readonly List<GameObject> temp = new List<GameObject>();
    private static readonly Dictionary<int, string> shots = new Dictionary<int, string>();
    private static string current;
    private static BossArena arena;
    private static StageRules rules;
    private static int defeatRel, crystalRel, pickRel, wakeRel;
    private static ItemPickup crystal;
    private static readonly List<Vector2Int> crystalPath = new List<Vector2Int>();

    public static SimonController Simon => world != null ? world.Player as SimonController : null;
    public static PhantomBat Bat => arena != null ? arena.Boss as PhantomBat : null;

    public static string ReplayPath(string name) { return REPLAY_DIR + "/boss_phantombat_" + name + ".txt"; }

    public static int SaveReplays()
    {
        Directory.CreateDirectory(REPLAY_DIR);
        foreach (var s in SCENARIOS)
        {
            var ticks = NesReplay.Decode(s.rle);
            var sb = new StringBuilder();
            sb.Append("# ").Append(ticks.Count).Append(" tick, hash ").Append(NesReplay.Hash(ticks).ToString("x8")).Append('\n');
            sb.Append("# CVGP 14 BossProbe kịch bản '").Append(s.name).Append("': ").Append(s.note).Append('\n');
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
        CvgpDebug.CheatInvincible = false;
        BackToArena();
    }

    /// <summary>Trả thế giới về section Arena + Simon (48,176) như lúc vào Play.</summary>
    public static void BackToArena()
    {
        world = StageWorld.Current;
        if (world == null || Simon == null)
            return;
        Cleanup();
        Go("Arena", 48, 176, 1);
        world.ResetRng();
        GameSession.Current.ResetForNewGame();
    }

    public static string SuiteReport()
    {
        var sb = new StringBuilder();
        sb.Append("BOSS SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
          .Append(SuiteFails).Append(", hash bộ ").Append(SuiteHash.ToString("x8")).Append('\n');
        foreach (var r in SuiteReports)
            sb.Append(r).Append('\n');
        return sb.ToString();
    }

    public static int SaveTraces(string dir)
    {
        Directory.CreateDirectory(dir);
        foreach (var kv in Traces)
            File.WriteAllText(Path.Combine(dir, "trace_" + kv.Key + ".csv"), kv.Value, new UTF8Encoding(false));
        return Traces.Count;
    }

    // ---------------------------------------------------------------- dựng

    static LevelSection Section(string name)
    {
        foreach (var s in world.Sections)
            if (s.data != null && s.data.name == name)
                return s;
        Debug.LogError("[BossProbe] không có section " + name + " trong scene");
        return null;
    }

    static void Go(string section, int x, int y, int facing)
    {
        var s = Section(section);
        world.Warp(s, x, y);
        Simon.Teleport(x, y, facing);
        if (world.CameraCtl != null)
            world.CameraCtl.SnapToTarget();
    }

    /// <summary>Phòng boss: PB + BossArena tạm; boss sinh ngay (không chờ bước 8) để tick 0 đã có dơi.</summary>
    static void Room(int sx, int sy, int facing)
    {
        Go("PhantomBat", sx, sy, facing);
        var go = new GameObject("BossProbe_Arena");
        go.transform.SetParent(world.transform, false);
        temp.Add(go);
        arena = go.AddComponent<BossArena>();
        arena.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<PhantomBat>(PREFAB);
        world.AddSpawner(arena);
        world.CameraCtl.Lock(512);
        world.ResetRng();
    }

    /// <summary>Đặt dơi đã thức tại (x, y) ở state s (bỏ qua ngủ/thức).</summary>
    static void Awake(int x, int y, ePhantomBatState s)
    {
        var b = Bat;
        b.SetPositionPx(x, y);
        b.Activate();
        b.Fsm.SetState(s);
        b.SyncView();
    }

    static void Shot(int rel)
    {
        if (!string.IsNullOrEmpty(ShotDir))
            shots[rel] = Path.Combine(ShotDir, current + "_r" + rel.ToString("000") + ".png");
    }

    // ---------------------------------------------------------------- 1 kịch bản

    public static int Run(string scenario)
    {
        world = StageWorld.Current;
        var s = Simon;
        if (world == null || s == null || world.Items == null)
        {
            Debug.LogError("[BossProbe] cần scene Arena với Player = SimonController + StageWorld.itemCatalog");
            return -1;
        }
        string p = ReplayPath(scenario);
        if (!File.Exists(p))
        {
            Debug.LogError("[BossProbe] thiếu " + p + " — gọi BossProbe.SaveReplays()");
            return -1;
        }
        var ticks = NesReplay.Decode(File.ReadAllText(p));
        Cleanup();
        current = scenario;
        var ses = GameSession.Current;
        ses.ResetForNewGame();
        ses.stage = 3;
        world.Stage = 3;
        finish = null;
        onTick = null;
        defeatRel = crystalRel = pickRel = wakeRel = -1;
        crystal = null;
        crystalPath.Clear();
        CvgpDebug.CheatInvincible = true;
        switch (scenario)
        {
            case "wake_video":
                {
                    Room(680, 144, 1);
                    arena.SpawnBoss(world);
                    Vector2Int firstGlide = new Vector2Int(-1, -1), firstEnd = new Vector2Int(-1, -1);
                    onTick = rel =>
                    {
                        if (firstGlide.x < 0 && Bat != null && Bat.Fsm.Is(ePhantomBatState.Glide)) { firstGlide = Bat.glideTarget; firstEnd = Bat.swoopEnd; }
                        if (wakeRel < 0 && Bat != null && !Bat.dormant) wakeRel = rel;
                        if (wakeRel >= 0)
                            foreach (int k in new[] { 0, 16, 34, 50, 66, 88, 120, 160, 200 })
                                if (rel == wakeRel + k - 1) Shot(rel + 1);
                    };
                    finish = () =>
                    {
                        Expect("dơi treo trần tại x 623 lúc đầu", VIDEO_SLEEP_X, recs[0].bx);
                        Expect("… y 41 (tâm khung bay; khung ngủ vẽ cao 8)", VIDEO_SLEEP_Y, recs[0].by);
                        ExpectRange("thức (tick rel)", 1, 400, wakeRel);
                        int sxw = wakeRel >= 0 ? recs[wakeRel].sx : -1;
                        ExpectRange("Simon.X lúc dơi thức (video 693–696)", VIDEO_WAKE_X_MIN, VIDEO_WAKE_X_MAX, sxw);
                        Expect("trước khi thức Simon.X < 693 (tick sai)", 0, CountRel(0, wakeRel, r => r.sx >= VIDEO_WAKE_X_MIN ? 1 : 0));
                        int sw = FirstRel(wakeRel, r => r.state == "Swoop");
                        ExpectRange("lượn tại chỗ sau khi thức (tick — video 31/35)", VIDEO_WAKE_HOVER_MIN, VIDEO_WAKE_HOVER_MAX, sw - wakeRel);
                        Expect("không dời chỗ khi lượn tại chỗ (tick sai)", 0, CountRel(wakeRel, sw, r => r.bx != VIDEO_SLEEP_X || r.by != VIDEO_SLEEP_Y ? 1 : 0));
                        int gl = FirstRel(sw, r => r.state == "Glide");
                        ExpectRange("cú lao đầu dài (tick — fitdive 54/60)", VIDEO_FIRST_T_MIN, VIDEO_FIRST_T_MAX, gl - sw);
                        int ex = firstEnd.x, ey = firstEnd.y;
                        Expect("đáy cú lao đầu y 82 (y màn 114 — 2 video)", VIDEO_FIRST_END_Y, ey);
                        ExpectRange("đáy cú lao đầu x (video 693 / 690)", VIDEO_FIRST_END_X_MIN, VIDEO_FIRST_END_X_MAX, ex);
                        Expect("lao đầu không qua Sweep/Rise (đáy cao hơn riseTop)", 0, CountRel(sw, gl, r => r.state == "Sweep" || r.state == "Rise" ? 1 : 0));
                        ExpectRange("đích lượn đầu bên TRÁI dơi (2 video)", 536, 632, firstGlide.x);
                        Expect("đích lượn đầu y 92 (y màn 124/125)", VIDEO_GLIDE_LOW_Y, firstGlide.y);
                        GlideSpeedCheck(gl, recs.Count);
                        measures.Add("thức rel " + wakeRel + " (Simon " + sxw + "), lao rel " + sw + "→" + gl + " đáy (" + ex + "," + ey + "), đích lượn đầu " + firstGlide
                            + ", Simon đứng ở " + recs[sw].sx + "," + recs[sw].sy);
                    };
                }
                break;
            case "cycle_floor":
            case "cycle_seed_b":
                {
                    Room(739, 176, -1);
                    if (scenario == "cycle_seed_b")
                        world.Rng.Reset(0x1234);
                    arena.SpawnBoss(world);
                    finish = () => CycleChecks();
                }
                break;
            case "touch":
                {
                    Room(739, 176, -1);
                    arena.SpawnBoss(world);
                    CvgpDebug.CheatInvincible = false;
                    finish = () =>
                    {
                        int hits = 0, bad = 0, first = -1;
                        for (int k = 1; k < recs.Count; k++)
                            if (recs[k].php < recs[k - 1].php)
                            {
                                hits++;
                                if (first < 0) first = k;
                                if (recs[k - 1].php - recs[k].php != WIKI_DAMAGE_ST3) bad++;
                            }
                        ExpectRange("Simon bị dơi chạm (lần)", 1, 20, hits);
                        Expect("mỗi lần chạm −2 vạch (Stage 3) — số lần sai", 0, bad);
                        string st = first > 0 && first < recs.Count ? recs[first].state : "-";
                        measures.Add("chạm lần đầu rel " + first + " khi dơi ở state " + st + " (" + (first > 0 ? recs[first].bx + "," + recs[first].by : "-") + ")");
                        Expect("dơi không bắn đạn (EnemyShot)", 0, shotsSeen);
                    };
                }
                break;
            case "strong_kill":
            case "weak_kill":
                {
                    bool strong = scenario == "strong_kill";
                    Room(620, 176, 1);
                    rules = world.gameObject.GetComponent<StageRules>();
                    if (rules == null && strong)
                    {
                        savedAutoRespawn = s.AutoRespawn;   // StageRules tắt autoRespawn của Simon và không trả lại
                        rules = world.gameObject.AddComponent<StageRules>();
                        addedRules = true;
                    }
                    ses.whipLevel = strong ? 2 : 1;
                    arena.SpawnBoss(world);
                    Awake(644, 152, ePhantomBatState.Hover);
                    Bat.DebugHoverTicks(99999);
                    world.FreezeEnemies(strong ? 2000 : 2000);
                    var bars = new List<int>();
                    onTick = rel =>
                    {
                        int eb = GameSession.Current.enemyHp;
                        if (bars.Count == 0 || bars[bars.Count - 1] != eb) bars.Add(eb);
                        if (defeatRel < 0 && Bat != null && Bat.dying) defeatRel = rel;
                    };
                    finish = () =>
                    {
                        var want = strong ? new[] { 16, 14, 12, 10, 8, 6, 4, 2, 0 } : new[] { 16, 15, 14, 12, 11, 10, 8, 7, 6, 4, 3, 2, 0 };
                        Expect("vạch ENEMY sau mỗi đòn " + string.Join(",", want) + " (" + string.Join(",", bars) + ") — khác", 0, Diff(want, bars));
                        ExpectRange("hạ ở đòn thứ (wiki " + (strong ? WIKI_HITS_STRONG : WIKI_HITS_WEAK) + ")", strong ? WIKI_HITS_STRONG : WIKI_HITS_WEAK,
                            strong ? WIKI_HITS_STRONG : WIKI_HITS_WEAK, lastHits);
                        Expect("+3000 điểm lúc hạ", WIKI_SCORE, defeatRel >= 0 ? recs[defeatRel].score - (defeatRel > 0 ? recs[defeatRel - 1].score : 0) : -1);
                        if (!strong)
                            return;
                        int fx0 = defeatRel, fxEnd = FirstRel(defeatRel + 1, r => r.fx == 0);
                        ExpectRange("lửa chết tồn tại (tick — video 60/62)", VIDEO_DEATH_FX - 2, VIDEO_DEATH_FX, fxEnd - fx0 - 1);
                        ExpectRange("quả cầu hiện sau khi hạ (tick — video 150/154)", VIDEO_CRYSTAL_MIN, VIDEO_CRYSTAL_MAX, crystalRel - defeatRel);
                        Expect("quả cầu sinh ở x 640 (x màn 128)", VIDEO_CRYSTAL_X, crystalPath.Count > 0 ? crystalPath[0].x : -1);
                        Expect("… y 104 (pivot; y màn 136)", VIDEO_CRYSTAL_Y, crystalPath.Count > 0 ? crystalPath[0].y : -1);
                        Expect("quả cầu rơi tới sàn y 176", 176, crystalPath.Count > 0 ? crystalPath[crystalPath.Count - 1].y : -1);
                        int fall = 0;
                        for (int k = 1; k < crystalPath.Count && crystalPath[k].y < 176; k++) fall = k + 1;
                        ExpectRange("rơi 72 px (tick — g 0.25: 24/25)", 23, 26, fall);
                        ExpectRange("Simon nhặt quả cầu (tick rel)", crystalRel + 1, 9999, pickRel);
                        Expect("máu Simon đầy sau khi nhặt", GameSession.MAX_HP, pickRel >= 0 ? recs[pickRel].php : -1);
                        Expect("TIME đứng sau khi nhặt (StageRules StageClear)", 1, rules != null && (rules.Holds & eClockHold.StageClear) != 0 ? 1 : 0);
                        Expect("tally đã bắt đầu (WaitStart)", 1, rules != null && rules.Tally.Active ? 1 : 0);
                        measures.Add("hạ rel " + defeatRel + ", lửa hết rel " + fxEnd + ", cầu rel " + crystalRel + " (" + (crystalPath.Count > 0 ? crystalPath[0].ToString() : "-") + "), chạm sàn sau " + fall + " tick, nhặt rel " + pickRel);
                    };
                }
                break;
            case "stun":
                {
                    Room(700, 176, -1);
                    ses.whipLevel = 2;
                    arena.SpawnBoss(world);
                    Awake(700, 140, ePhantomBatState.Hover);
                    Bat.DebugHoverTicks(99999);
                    // dơi bay ngang qua tầm roi: vào Sweep vx −1 px/tick
                    Bat.vx = -256;
                    Bat.Fsm.SetState(ePhantomBatState.Sweep);
                    Bat.SetPositionPx(682, 152);
                    Bat.DebugSweepTicks(9999);
                    finish = () =>
                    {
                        int hit = FirstRel(0, r => r.ebars < 16);
                        int stillEnd = hit;
                        while (stillEnd + 1 < recs.Count && recs[stillEnd + 1].bx == recs[hit].bx) stillEnd++;
                        ExpectRange("trúng roi (tick rel)", 1, 40, hit);
                        ExpectRange("đứng yên sau đòn (tick — video Map_1 ≈ 10)", VIDEO_STUN_MIN, VIDEO_STUN_MAX, stillEnd - hit);
                        Expect("khung hurt (phantombat_2) khi khựng", 1, hit >= 0 && recs[hit + 1].state == "Stun" ? 1 : 0);
                        Expect("bay tiếp sau khựng (x đổi)", 1, stillEnd + 2 < recs.Count && recs[stillEnd + 2].bx != recs[stillEnd].bx ? 1 : 0);
                        measures.Add("trúng rel " + hit + ", đứng tới rel " + stillEnd + " (" + (stillEnd - hit) + " tick)");
                    };
                }
                break;
            case "holy_hold":
                {
                    Room(740, 176, -1);
                    arena.SpawnBoss(world);
                    Awake(700, 160, ePhantomBatState.Hover);
                    Bat.DebugHoverTicks(150);
                    var firePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<HolyFire>("Assets/Data/Prefabs/Weapon/HolyFire.prefab");
                    onTick = rel => { if (rel == 4) world.Spawn(firePrefab, 700, 176, -1); };
                    finish = () =>
                    {
                        int fs = FirstRel(5, r => r.state == "Stun");
                        int fe = FirstRel(fs + 1, r => r.state != "Stun");
                        Expect("đứng yên suốt lúc chạm lửa (x,y đổi — tick)", 0, CountRel(fs, fe, r => r.bx != 700 || r.by != 160 ? 1 : 0));
                        ExpectRange("bị giữ (tick — lửa 80 + khựng 10; video boss ~65 tick tới chết)", 80, 96, fe - fs);
                        int drops = 0;
                        for (int k = 1; k < recs.Count; k++) if (recs[k].ebars < recs[k - 1].ebars) drops++;
                        ExpectRange("số lần mất máu trong lửa (bất tử 16 ⇒ 5–6)", 5, 6, drops);
                        measures.Add("giữ rel " + fs + "→" + fe + ", vạch còn " + recs[recs.Count - 1].ebars + " sau " + drops + " lần trúng lửa");
                    };
                }
                break;
            case "stopwatch":
                {
                    Room(740, 176, -1);
                    arena.SpawnBoss(world);
                    Awake(600, 92, ePhantomBatState.Swoop);
                    onTick = rel => { if (rel == 20) world.FreezeEnemies(WIKI_STOPWATCH); };
                    finish = () =>
                    {
                        int still = CountRel(21, 21 + WIKI_STOPWATCH, r => r.bx == recs[21].bx && r.by == recs[21].by ? 1 : 0);
                        Expect("đứng yên đủ 300 tick Stopwatch", WIKI_STOPWATCH, still);
                        Expect("bay tiếp sau Stopwatch", 1, recs[21 + WIKI_STOPWATCH + 3].bx != recs[21].bx || recs[21 + WIKI_STOPWATCH + 3].by != recs[21].by ? 1 : 0);
                    };
                }
                break;
            case "rosary":
                {
                    Room(740, 176, -1);
                    arena.SpawnBoss(world);
                    Awake(600, 92, ePhantomBatState.Hover);
                    onTick = rel => { if (rel == 10) world.RosaryClear(); };
                    finish = () => Expect("boss còn sống sau Rosary", 1, recs[recs.Count - 1].alive ? 1 : 0);
                }
                break;
            case "walls":
                {
                    Room(656, 176, -1);
                    arena.SpawnBoss(world);
                    finish = () =>
                    {
                        Expect("camera khoá 512", 512, recs[recs.Count - 1].cx);
                        Expect("Simon dừng ở x 520 (cx + 8)", 520, MinOf(r => r.sx));
                    };
                }
                break;
            case "death_video":
                {
                    Room(655, 176, 1);
                    arena.SpawnBoss(world);
                    Awake(723, 121, ePhantomBatState.Hover);
                    Bat.DebugHoverTicks(99999);
                    onTick = rel =>
                    {
                        if (rel == 9) Bat.Defeat();
                        if (defeatRel < 0 && Bat != null && Bat.dying) defeatRel = rel;
                        if (defeatRel >= 0)
                            foreach (int k in new[] { 2, 10, 20, 30, 48, 150, 156, 166 })
                                if (rel == defeatRel + k - 1) Shot(rel + 1);
                    };
                    finish = () =>
                    {
                        ExpectRange("quả cầu hiện sau khi hạ (tick)", VIDEO_CRYSTAL_MIN, VIDEO_CRYSTAL_MAX, crystalRel - defeatRel);
                        measures.Add("hạ rel " + defeatRel + ", cầu rel " + crystalRel);
                    };
                }
                break;
            default:
                Debug.LogError("[BossProbe] không có kịch bản " + scenario);
                return -1;
        }
        s.ResetStats();
        NesPad.I.PlayReplay(ticks);
        Begin(scenario, ticks.Count);
        return ticks.Count;
    }

    private static bool addedRules, savedAutoRespawn = true;
    private static int shotsSeen, lastHits;

    // ---------------------------------------------------------------- chu kỳ (cycle_floor)

    static void CycleChecks()
    {
        // tách pha theo state
        var seq = new List<string>();
        var start = new List<int>();
        for (int k = 0; k < recs.Count; k++)
            if (seq.Count == 0 || recs[k].state != seq[seq.Count - 1])
            {
                seq.Add(recs[k].state);
                start.Add(k);
            }
        var sb = new StringBuilder();
        for (int i = 0; i < seq.Count; i++)
            sb.Append(seq[i]).Append('@').Append(start[i]).Append(' ');
        measures.Add("pha: " + sb);
        int swoops = 0, badOrder = 0, badHover = 0, badRise = 0, badBottom = 0, badGlideY = 0, badDx = 0, hovers = 0;
        var cyc = new List<int>();
        int lastSwoop = -1;
        for (int i = 0; i < seq.Count; i++)
        {
            string st = seq[i];
            int len = (i + 1 < seq.Count ? start[i + 1] : recs.Count) - start[i];
            string next = i + 1 < seq.Count ? seq[i + 1] : "";
            if (st == "Swoop")
            {
                swoops++;
                if (lastSwoop >= 0) cyc.Add(start[i] - lastSwoop);
                lastSwoop = start[i];
                if (next != "" && next != "Sweep" && next != "Glide") badOrder++;
                int bot = recs[start[i] + len - 1].by;
                if (bot > VIDEO_DIVE_MAX_Y) badBottom++;
            }
            else if (st == "Sweep" && next != "" && next != "Rise") badOrder++;
            else if (st == "Rise")
            {
                if (next != "" && next != "Glide") badOrder++;
                // tick chuyển: vọt lên tới ≤ 88 rồi bước lượn đầu (≤ 1 px) cùng tick
                if (next == "Glide" && recs[start[i + 1]].by > VIDEO_RISE_TOP + 1) badRise++;
            }
            else if (st == "Glide" && next != "" && next != "Hover") badOrder++;
            else if (st == "Hover")
            {
                hovers++;
                // số tick ghi "Hover" = hoverLen (tick tới đích ghi Hover; tick sau tick cuối đã bắt đầu lao)
                if (next != "" && (len < VIDEO_HOVER_MIN || len > VIDEO_HOVER_MAX + 1)) badHover++;
                int gy = recs[start[i]].by;
                if (gy != VIDEO_GLIDE_LOW_Y && (gy < VIDEO_GLIDE_HIGH_Y_MIN || gy > VIDEO_GLIDE_HIGH_Y_MAX)) badGlideY++;
                if (Math.Abs(recs[start[i]].bx - recs[start[i]].sx) < VIDEO_GLIDE_MIN_DX) badDx++;
                if (next != "" && next != "Swoop") badOrder++;
            }
        }
        ExpectRange("số cú bổ nhào trong 1600 tick (≥ 4)", 4, 99, swoops);
        Expect("thứ tự pha Swoop→(Sweep→Rise)→Glide→Hover→Swoop — sai", 0, badOrder);
        Expect("lơ lửng 32–124 tick — sai", 0, badHover);
        Expect("vọt lên dừng ở y ≤ 88 — sai", 0, badRise);
        Expect("đáy bổ nhào ≤ y 156 — sai", 0, badBottom);
        Expect("điểm lơ lửng y 92 / 121–125 — sai", 0, badGlideY);
        Expect("điểm lơ lửng cách Simon ≥ 64 — sai", 0, badDx);
        int outScr = CountRel(0, recs.Count, r => r.bx - r.cx > VIDEO_SCREEN_MAX + 2 || r.bx - r.cx < 16 ? 1 : 0);
        Expect("dơi ra ngoài [cx+16, cx+240] (tick)", 0, outScr);
        Expect("dơi không bắn đạn (EnemyShot)", 0, shotsSeen);
        measures.Add("chu kỳ bổ nhào (tick): " + string.Join(",", cyc) + " (video boss 232, 252)");
        GlideSpeedCheck(0, recs.Count);
    }

    /// <summary>Lượn: mỗi đoạn Glide (≥ 20 tick, không khựng) đi trục trội 0.75 px/tick (video 0.72–0.77) — tổng dời / số tick.</summary>
    static void GlideSpeedCheck(int from, int to)
    {
        int bad = 0, n = 0;
        var sb = new StringBuilder();
        int k = Math.Max(1, from);
        while (k < to)
        {
            if (recs[k].state != "Glide") { k++; continue; }
            int a = k;
            while (k < to && recs[k].state == "Glide") k++;
            int len = k - a;
            if (len < 30)
                continue;
            // từ tick lượn đầu (đã gồm bước chuyển state) tới tick cuối: len − 1 tick
            int dx = Math.Abs(recs[k - 1].bx - recs[a].bx), dy = Math.Abs(recs[k - 1].by - recs[a].by);
            int v = Math.Max(dx, dy) * 1000 / (len - 1);
            n++;
            sb.Append(v).Append(' ');
            if (v < 700 || v > 790) bad++;
        }
        measures.Add("tốc độ lượn trục trội ×1000 từng đoạn: " + sb);
        ExpectRange("số đoạn lượn đã đo", 1, 99, n);
        Expect("đoạn lượn (≥ 30 tick) trục trội 0.70–0.79 px/tick (0.75 + làm tròn px) — sai", 0, bad);
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
        shotsSeen = 0;
        lastHits = 0;
        recs.Clear();
        checks.Clear();
        measures.Clear();
        startTick = world.TickCount;
        world.TickEnded += OnTickEnded;
        BossBase.CrystalSpawned += OnCrystal;
        ItemEffects.CrystalPicked += OnPicked;
        Running = true;
    }

    private static void Unhook()
    {
        if (world != null)
            world.TickEnded -= OnTickEnded;
        BossBase.CrystalSpawned -= OnCrystal;
        ItemEffects.CrystalPicked -= OnPicked;
        Running = false;
    }

    private static void OnCrystal(BossBase b, ItemPickup it)
    {
        crystal = it;
        crystalRel = world.TickCount - startTick - 1;
    }

    private static void OnPicked(StageWorld w)
    {
        if (pickRel < 0)
            pickRel = w.TickCount - startTick - 1;
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
        var ses = GameSession.Current;
        var b = Bat;
        int fx = 0, items = 0;
        foreach (var e in w.GroupList(eEntityGroup.Effect))
            if (e.Alive && e is FxSprite && e.DebugState.StartsWith("bossdeath")) fx++;
        foreach (var e in w.GroupList(eEntityGroup.Item))
            if (e.Alive) items++;
        foreach (var e in w.GroupList(eEntityGroup.EnemyShot))
            if (e.Alive) shotsSeen++;
        if (crystal != null && crystal.Alive && crystal.id == eItemId.MagicCrystal)
            crystalPath.Add(new Vector2Int(crystal.X, crystal.Y));
        if (b != null)
            lastHits = b.hitsTaken;
        var r = new Rec
        {
            bx = b != null ? b.X : 0, by = b != null ? b.Y : 0, sx = s.X, sy = s.Y, php = ses.playerHp, ebars = ses.enemyHp, score = ses.score,
            fx = fx, items = items, cx = w.CameraCtl != null ? w.CameraCtl.X : 0, stun = b != null ? b.stunTicks : 0,
            state = b != null ? b.DebugState.Split(' ')[0] : "-", alive = b != null && b.Alive, dying = b != null && b.dying,
        };
        recs.Add(r);
        Mix(rel); Mix(r.bx); Mix(r.by); Mix(b != null ? b.fx : 0); Mix(b != null ? b.fy : 0); Mix(r.state.GetHashCode()); Mix(s.fx); Mix(s.fy);
        Mix(r.php); Mix(r.ebars); Mix(r.score); Mix(w.Rng.State);
        string shot;
        if (shots.TryGetValue(rel, out shot))
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
        shots.Clear();
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
        Traces[Label] = TraceCsv();
        Cleanup();
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

    private static void Cleanup()
    {
        foreach (var go in temp)
            if (go != null)
            {
                var sp = go.GetComponent<EnemySpawner>();
                if (world != null && sp != null)
                    world.RemoveSpawner(sp);
                UnityEngine.Object.Destroy(go);
            }
        temp.Clear();
        if (addedRules && rules != null)
        {
            rules.enabled = false;
            UnityEngine.Object.Destroy(rules);
            if (Simon != null)
                Simon.AutoRespawn = savedAutoRespawn;
        }
        addedRules = false;
        rules = null;
        arena = null;
        if (world != null)
        {
            world.FreezeEnemies(0);   // bỏ Stopwatch còn lại của kịch bản trước
            world.ClearNonPlayer();
            if (world.Tiles != null)
            {
                world.Tiles.WallMinX = int.MinValue;
                world.Tiles.WallMaxX = int.MaxValue;
            }
            if (world.CameraCtl != null && world.Section != null)
                world.CameraCtl.Bind(world.Section.data);
        }
        if (Simon != null)
            Simon.InputLocked = false;
    }

    public static string Report()
    {
        var sb = new StringBuilder();
        sb.Append("[").Append(Fails == 0 ? "ĐẠT" : "FAIL").Append("] ").Append(Label).Append(" — ").Append(recs.Count).Append(" tick, hash ")
          .Append(Hash.ToString("x8")).Append('\n');
        foreach (var c in checks)
            sb.Append(c).Append('\n');
        foreach (var m in measures)
            sb.Append("    · ").Append(m).Append('\n');
        return sb.ToString().TrimEnd('\n');
    }

    /// <summary>Vết từng tick: rel,bx,by,state,stun,sx,sy,php,ebars,score,cx.</summary>
    public static string TraceCsv()
    {
        var sb = new StringBuilder("rel,bx,by,state,stun,sx,sy,php,ebars,score,cx\n");
        for (int k = 0; k < recs.Count; k++)
        {
            var r = recs[k];
            sb.Append(k).Append(',').Append(r.bx).Append(',').Append(r.by).Append(',').Append(r.state).Append(',').Append(r.stun).Append(',')
              .Append(r.sx).Append(',').Append(r.sy).Append(',').Append(r.php).Append(',').Append(r.ebars).Append(',').Append(r.score).Append(',')
              .Append(r.cx).Append('\n');
        }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- chấm

    static int FirstRel(int from, Func<Rec, bool> f)
    {
        for (int k = Math.Max(0, from); k < recs.Count; k++)
            if (f(recs[k]))
                return k;
        return -1;
    }

    static int CountRel(int from, int to, Func<Rec, int> f)
    {
        int n = 0;
        for (int k = Math.Max(0, from); k < Math.Min(to, recs.Count); k++)
            n += f(recs[k]);
        return n;
    }

    static int MinOf(Func<Rec, int> f)
    {
        int m = int.MaxValue;
        foreach (var r in recs)
            m = Math.Min(m, f(r));
        return m;
    }

    static int Diff(int[] want, List<int> got)
    {
        int n = Math.Abs(want.Length - got.Count);
        for (int i = 0; i < Math.Min(want.Length, got.Count); i++)
            if (want[i] != got[i]) n++;
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
