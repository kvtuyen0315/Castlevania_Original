#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness enemy Block 1 + bộ sinh (CVGP 13; routine 16/17/21 chạy lại) — CHỈ Editor, scene Arena (Player = Simon). Qua MCP khi Play:
///   EnemyProbe.SaveReplays()                        ⇒ Assets/Data/Tests/Replays/enemy_b1_&lt;tên&gt;.txt
///   EnemyProbe.RunSuite(EnemyProbe.ScenarioNames())  ⇒ chạy (input ĐỌC TỪ FILE), ghi từng tick, TỰ CHẤM ⇒ SuiteReport()
///   EnemyProbe.ShotDir = thư mục ⇒ kịch bản *_video chụp Game view đúng tick (ảnh so video, Tools/cvgp/enemy_pairs.py)
///   EnemyProbe.TraceCsv(tên) ⇒ vết enemy từng tick của kịch bản vừa chạy (so quỹ đạo video: Tools/cvgp/enemy_compare.py)
/// Section Map 1 dùng TẠM bằng Warp tới các section có sẵn trong scene Arena (EntranceHall, CryptA, CryptB, PhantomBat) — bộ sinh tạo
/// lúc chạy (con của StageWorld, KHÔNG lưu vào prefab/scene). Kỳ vọng = hằng VIDEO_* / WIKI_* (đo video routine 13, wiki) độc lập Config.
/// </summary>
public static class EnemyProbe
{
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";

    // ---------------------------------------------------------------- kỳ vọng (CVGP_MEMORY §2k)
    public const int VIDEO_GHOUL_VX = 224;            // 0.875 px/tick (19 vết)
    public const int VIDEO_GHOUL_FRAME = 17;          // khung đi
    public const int VIDEO_GROUP_GAP = 32;            // 29–33 (12 mẫu)
    public const int VIDEO_GROUP = 3;
    public const int VIDEO_WAVE_MIN = 115, VIDEO_WAVE_MAX = 288;
    public const int VIDEO_INSET = 8;                 // sinh: pivot = mép màn − 8
    public const int VIDEO_DESPAWN = 4;               // gỡ: pivot < cx + 4
    public const int VIDEO_G = 32;                    // 0.125 px/tick² (rơi Zombie, nhảy báo, Fish Man)
    public const int VIDEO_LEDGE_PB = 128;            // mép trái gờ PB (Zombie rơi ở x 127)
    public const int VIDEO_PANTHER_TRIGGER = 70;      // |dx| < 70
    public const int VIDEO_PANTHER_VX = 512;          // 2 px/tick
    public const int VIDEO_PANTHER_PEAK_MIN = 8, VIDEO_PANTHER_PEAK_MAX = 10;
    public const int VIDEO_BAT_VX = 288;              // 1.125 px/tick
    public const int VIDEO_BAT_PERIOD = 100;          // 94–106
    public const int VIDEO_BAT_PTP_MIN = 16, VIDEO_BAT_PTP_MAX = 19;   // đỉnh–đáy video 16–19
    public const int VIDEO_BAT_FRAME = 9;
    public const int VIDEO_BAT_RESPAWN_MIN = 194, VIDEO_BAT_RESPAWN_MAX = 330;
    public const int VIDEO_BAT_ABOVE = 14;
    public const int VIDEO_FISH_APEX_Y = 54;          // chân NES 85–88 ⇒ section 53–56
    public const int VIDEO_FISH_APEX_TOL = 3;
    public const int VIDEO_FISH_VX = 192;             // 0.75 px/tick
    public const int VIDEO_FISH_STAND = 8;            // 6–10
    public const int VIDEO_FISH_FRAME = 32;           // 32–34
    public const int VIDEO_FISH_STEP = 64, VIDEO_FISH_OFFSET = 32;
    public const int VIDEO_FISH_RESPAWN_MIN = 44, VIDEO_FISH_RESPAWN_MAX = 176;
    public const int VIDEO_DEATH_FX_TICKS = 20;       // lửa 8/9/3
    public const int WIKI_SCORE_GHOUL = 100, WIKI_SCORE_PANTHER = 200, WIKI_SCORE_BAT = 200, WIKI_SCORE_FISH = 300;
    public const int WIKI_DAMAGE_ST1 = 2;
    public const int VIDEO_STOPWATCH = 186;   // video:Map_2 (đo:cvm3-03 — CVM3_MEMORY §2d; wiki 5 s = 300 sai)

    private struct Scn
    {
        public string name, rle, note;
        public Scn(string n, string r, string note) { name = n; rle = r; this.note = note; }
    }

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("ghoul_spawner", "-*960", "EH cx 0, Simon (49,176) đứng (bất tử): EdgeSpawner Zombie mép phải — nhóm 3 cách 32, nghỉ 115–288, 0.875 px/tick, khung 17, gỡ ở mép trái"),
        new Scn("ghoul_pb_video", "-*240", "PB cx 0 (video f5127): Zombie gờ trên @0, sàn dưới @0, gờ trên @33 — rơi thẳng ở x 127, g 0.125"),
        new Scn("ghoul_pb_spawner", "-*960", "PB cx 0: EdgeSpawner hàng {112, 176} — chọn ngẫu nhiên hàng có sàn tại cột sinh, tối đa 3"),
        new Scn("panther_eh1", "R*120 -*100", "EH: báo #1 (699,112) nằm chờ, Simon đi phải từ 560 (video f1228–1300): bật dậy |dx| < 70, chạy 2, nhảy ở mép 688, đáp, quay về Simon"),
        new Scn("panther_eh3", "R*110 -*130", "EH: báo #3 (955,112) trên Bục C, Simon đi phải từ 820 (video f1746–1860)"),
        new Scn("bat_ca", "-*760", "CA cx 0, Simon (49,80) quay phải đứng (video f2756): EdgeSpawner dơi trước mặt — 1.125 px/tick, sóng A 8.75 P 100, 1 con/lần, hồi 194–330"),
        new Scn("bat_side_l", "-*60", "CA, Simon (200,176) quay TRÁI ở x màn 128 ⇒ dơi ra mép trái"),
        new Scn("bat_side_r", "-*60", "CA, Simon (49,80) quay TRÁI ở x màn 49 (< 64) ⇒ dơi ra mép phải (video f3174)"),
        new Scn("fishman_video", "-*220", "CB cx 0, Simon (86,96) (video f3514): 2 Fish Man trồi ở x 160 và 32 — thẳng đứng, đỉnh Y 54, đáp 96, đứng 8, đi 0.75 về Simon, bắn (quy ước)"),
        new Scn("fishman_gap", "-*160", "CB: Fish Man trồi ở x 224 (khe lối đi — video f3589–3627) ⇒ rơi lại xuống nước: bọt + gỡ, 0 điểm"),
        new Scn("fishman_spawner", "-*1200", "CB, Simon (120,96) bất tử: WaterSpawner — cột 32+64k có nước, cách Simon ≥ 32, ≤ 2 con, hồi 44–176/suất"),
        new Scn("kill_ghoul_l1", "-*2 B -*40", "Arena: roi cấp 1 hạ Zombie 1 đòn: +100, lửa chết 20 tick tại tâm, Simon không mất máu"),
        new Scn("kill_panther_l2", "-*2 D+B D*40", "roi cấp 2 KHI NGỒI hạ báo (thấp 16 px — video f1295 tia lửa ở chân −7): +200"),
        new Scn("kill_bat_l3", "-*2 B -*40", "roi cấp 3 hạ dơi: +200"),
        new Scn("kill_fish_dagger", "-*2 Sub -*50", "dao hạ Fish Man đứng trên sàn: +300, vũ khí phụ đếm trúng"),
        new Scn("touch", "-*60", "Zombie đi vào Simon (không bất tử): −2 vạch (Stage 1), Simon văng (Hurt)"),
        new Scn("fireball", "-*200", "Fish Man trên sàn bắn cầu lửa (quy ước 96 tick) trúng Simon: −2 vạch"),
        new Scn("freeze", "-*30 Sub -*420", "Stopwatch: Zombie + bộ sinh dừng 186 tick rồi chạy tiếp"),
        new Scn("rosary", "-*200", "Rosary (StageWorld.RosaryClear) tick 120: mọi Zombie trên màn biến mất, 0 điểm"),
        new Scn("pot_roast", "-*40", "CA: tường Pot Roast (16,10) vỡ ⇒ thả ngay 1 dơi (wiki Secrets) + thịt nướng"),
    };

    // ---------------------------------------------------------------- ghi
    public class Track
    {
        public EnemyBase e;
        public int spawnTick, spawnRel = -1, endRel = -1, x0, y0, facing0;
        public eEnemyId id;
        public bool defeated, fromSpawner;
        public eKillReason reason;
        public readonly List<Vector2Int> path = new List<Vector2Int>();
        public readonly List<int> frames = new List<int>();
        public readonly List<string> states = new List<string>();
        public readonly List<bool> hidden = new List<bool>();
        public int X(int k) { return k < path.Count ? path[k].x : int.MinValue; }
    }

    private struct Rec
    {
        public int sx, sy, hp, score, cx, enemies, fx, shots;
        public eSimonState state;
        public bool frozenE;
    }

    public static string Label { get; private set; } = "";
    public static bool Running { get; private set; }
    public static int Fails { get; private set; }
    public static uint Hash { get; private set; }
    /// <summary>Thư mục chụp ảnh (null = không chụp). Kịch bản *_video tự chụp ở các tick cố định.</summary>
    public static string ShotDir;
    private static readonly List<Rec> recs = new List<Rec>();
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> measures = new List<string>();
    private static readonly List<Track> tracks = new List<Track>();
    private static readonly List<int> spawnRels = new List<int>();
    private static readonly Queue<string> suite = new Queue<string>();
    public static readonly List<string> SuiteReports = new List<string>();
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

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string ReplayPath(string name) { return REPLAY_DIR + "/enemy_b1_" + name + ".txt"; }

    public static int SaveReplays()
    {
        Directory.CreateDirectory(REPLAY_DIR);
        foreach (var s in SCENARIOS)
        {
            var ticks = NesReplay.Decode(s.rle);
            var sb = new StringBuilder();
            sb.Append("# ").Append(ticks.Count).Append(" tick, hash ").Append(NesReplay.Hash(ticks).ToString("x8")).Append('\n');
            sb.Append("# CVGP 13 EnemyProbe kịch bản '").Append(s.name).Append("': ").Append(s.note).Append('\n');
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

    /// <summary>Trả thế giới về section Arena + Simon (48,176) như lúc vào Play (bộ kiểm khác chạy sau trong cùng phiên).</summary>
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
        sb.Append("ENEMY SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
          .Append(SuiteFails).Append(", hash bộ ").Append(SuiteHash.ToString("x8")).Append('\n');
        foreach (var r in SuiteReports)
            sb.Append(r).Append('\n');
        return sb.ToString();
    }

    // ---------------------------------------------------------------- dựng

    static T Prefab<T>(string name) where T : Entity
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>("Assets/Data/Prefabs/Enemy/" + name + ".prefab");
    }

    static LevelSection Section(string name)
    {
        foreach (var s in world.Sections)
            if (s.data != null && s.data.name == name)
                return s;
        Debug.LogError("[EnemyProbe] không có section " + name + " trong scene");
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

    static T AddSpawner<T>(EnemyBase prefab) where T : EnemySpawner
    {
        var go = new GameObject("EnemyProbe_" + typeof(T).Name);
        go.transform.SetParent(world.transform, false);
        temp.Add(go);
        var sp = go.AddComponent<T>();
        sp.prefab = prefab;
        world.AddSpawner(sp);
        return sp;
    }

    static EnemyBase Put(EnemyBase prefab, int x, int y, int facing)
    {
        return world.Spawn(prefab, x, y, facing) as EnemyBase;
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
            Debug.LogError("[EnemyProbe] cần scene Arena với Player = SimonController + StageWorld.itemCatalog");
            return -1;
        }
        string p = ReplayPath(scenario);
        if (!File.Exists(p))
        {
            Debug.LogError("[EnemyProbe] thiếu " + p + " — gọi EnemyProbe.SaveReplays()");
            return -1;
        }
        var ticks = NesReplay.Decode(File.ReadAllText(p));
        Cleanup();
        current = scenario;
        var ses = GameSession.Current;
        ses.playerHp = GameSession.MAX_HP;
        ses.hearts = 5;
        ses.score = 0;
        ses.lives = 3;
        ses.whipLevel = 1;
        ses.shotMultiplier = 1;
        ses.subWeapon = eSubWeapon.None;
        ses.subWeaponHits = 0;
        ses.NotifyChanged();
        finish = null;
        onTick = null;
        CvgpDebug.CheatInvincible = true;
        var ghoul = Prefab<Ghoul>("Ghoul");
        var panther = Prefab<Panther>("Panther");
        var bat = Prefab<Bat>("Bat");
        var fish = Prefab<Fishman>("Fishman");
        switch (scenario)
        {
            case "ghoul_spawner":
                {
                    Go("EntranceHall", 49, 176, 1);
                    world.ResetRng();
                    var sp = AddSpawner<EdgeSpawner>(ghoul);
                    sp.side = eSpawnSide.Right;
                    sp.rows = new[] { 176 };
                    onTick = rel => { if (rel == 39 || rel == 79 || rel == 119) Shot(rel + 1); };
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Ghoul);
                        ExpectRange("số lần sinh trong 960 tick (≥ 10)", 10, 99, ts.Count);
                        Expect("lần sinh đầu (tick rel — video: có mặt ngay frame đầu section)", 0, ts.Count > 0 ? ts[0].spawnRel : -1);
                        int badX = 0, badY = 0, badF = 0;
                        foreach (var t in ts)
                        {
                            if (t.x0 != NesCoord.SCREEN_W - VIDEO_INSET) badX++;
                            if (t.y0 != 176) badY++;
                            if (t.facing0 != -1) badF++;
                        }
                        Expect("sinh ở pivot màn 248 (mép phải − 8) — số lần sai", 0, badX);
                        Expect("sinh trên sàn y 176 — số lần sai", 0, badY);
                        Expect("hướng vào màn (trái) — số lần sai", 0, badF);
                        GroupChecks(ts);
                        SpeedChecks(ts, VIDEO_GHOUL_VX, 120);
                        FrameChecks(ts, VIDEO_GHOUL_FRAME);
                        int badEnd = 0, ended = 0;
                        foreach (var t in ts)
                            if (t.endRel >= 0 && t.reason == eKillReason.Offscreen)
                            {
                                ended++;
                                int last = t.path[t.path.Count - 1].x;
                                if (last < VIDEO_DESPAWN || last > VIDEO_DESPAWN + 1) badEnd++;
                            }
                        ExpectRange("Zombie đi hết ra mép trái (số con)", 3, 99, ended);
                        Expect("gỡ khi pivot < cx + 4 (x cuối còn thấy 4–5) — số lần sai", 0, badEnd);
                        ExpectRange("tối đa sống cùng lúc", 1, 3, MaxOf(r => r.enemies));
                    };
                }
                break;
            case "ghoul_pb_video":
                {
                    Go("PhantomBat", 49, 80, 1);
                    Put(ghoul, 248, 112, -1);
                    Put(ghoul, 248, 176, -1);
                    onTick = rel =>
                    {
                        if (rel == 32) Put(ghoul, 248, 112, -1);
                        if (rel == 150 || rel == 160 || rel == 176 || rel == 200) Shot(rel + 1);
                    };
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Ghoul);
                        Expect("3 Zombie", 3, ts.Count);
                        foreach (var t in ts)
                        {
                            if (t.y0 != 112)
                                continue;
                            int k0 = -1, kl = -1, xs = 0, xf = int.MinValue;
                            for (int k = 1; k < t.states.Count; k++)
                            {
                                if (k0 < 0 && t.states[k] == "Fall") { k0 = k; xf = t.path[k].x; }
                                else if (k0 >= 0 && kl < 0 && t.states[k] == "Walk") kl = k;
                                else if (k0 >= 0 && kl < 0 && t.path[k].x != xf) xs++;
                            }
                            Expect("gờ trên: bắt đầu rơi ở x 127 (mép 128 − 1)", VIDEO_LEDGE_PB - 1, xf);
                            Expect("rơi thẳng đứng (tick x đổi khi rơi)", 0, xs);
                            int dur = kl - k0 + 1;
                            Expect("đáp sàn y 176", 176, kl >= 0 ? t.path[kl].y : -1);
                            ExpectRange("rơi 64 px (tick) — g 0.125 từ 0 ⇒ 32 (video ~26–30, capture ±4)", 31, 33, dur);
                            int bad = 0;
                            for (int k = k0; k < kl && k < t.path.Count; k++)
                            {
                                int n = k - k0 + 1;
                                int want = 112 + ((VIDEO_G * n * (n + 1) / 2) >> 8);
                                if (t.path[k].y != want) bad++;
                            }
                            Expect("y rơi = 112 + g·n(n+1)/2 (tick sai)", 0, bad);
                            measures.Add("Zombie gờ trên #" + t.spawnRel + ": rơi tick " + k0 + "→" + kl + " (" + dur + " tick)");
                        }
                    };
                }
                break;
            case "ghoul_pb_spawner":
                {
                    Go("PhantomBat", 49, 80, 1);
                    world.ResetRng();
                    var sp = AddSpawner<EdgeSpawner>(ghoul);
                    sp.side = eSpawnSide.Right;
                    sp.rows = new[] { 112, 176 };
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Ghoul);
                        int up = 0, low = 0, fellAt127 = 0, upFell = 0;
                        foreach (var t in ts)
                        {
                            if (t.y0 == 112) up++; else if (t.y0 == 176) low++;
                            if (t.y0 == 112)
                                for (int k = 1; k < t.path.Count; k++)
                                    if (t.path[k].y > 112) { upFell++; if (t.path[k].x == VIDEO_LEDGE_PB - 1) fellAt127++; break; }
                        }
                        ExpectRange("sinh ở gờ trên (lần)", 1, 99, up);
                        ExpectRange("sinh ở sàn dưới (lần)", 1, 99, low);
                        Expect("mọi Zombie gờ trên rơi ở x 127", upFell, fellAt127);
                        GroupChecks(ts);
                        ExpectRange("tối đa sống cùng lúc", 1, 3, MaxOf(r => r.enemies));
                        measures.Add("hàng: gờ trên " + up + " / sàn dưới " + low + " (video PB 6 / 9)");
                    };
                }
                break;
            case "panther_eh1":
            case "panther_eh3":
                {
                    bool one = scenario == "panther_eh1";
                    int px = one ? 699 : 955, sx0 = one ? 560 : 820, edge = one ? 688 : 928;
                    Go("EntranceHall", sx0, 176, 1);
                    var pa = Put(panther, px, 112, -1) as Panther;
                    int trig = -1;
                    onTick = rel =>
                    {
                        if (trig < 0 && pa.triggerAge >= 0) trig = rel;
                        if (one && trig >= 0 && (rel == trig + 16 || rel == trig + 36 || rel == trig + 48)) Shot(rel + 1);
                    };
                    finish = () =>
                    {
                        var t = Tracks(eEnemyId.Panther).Count > 0 ? Tracks(eEnemyId.Panther)[0] : null;
                        if (t == null) { Expect("có báo", 1, 0); return; }
                        int kT = -1, kJ = -1, kL = -1, minY = 999;
                        for (int k = 0; k < t.states.Count; k++)
                        {
                            if (kT < 0 && t.states[k] == "Run") kT = k;
                            if (kJ < 0 && t.states[k] == "Jump") kJ = k;
                            if (kJ >= 0 && kL < 0 && t.states[k] == "Run") kL = k;
                            if (kJ >= 0 && kL < 0) minY = Math.Min(minY, t.path[k].y);
                        }
                        int simX = kT >= 0 && t.spawnRel + kT < recs.Count ? recs[t.spawnRel + kT].sx : -1;
                        Expect("bật dậy khi |Simon − báo| < 70: Simon X lúc bật = báo − 69", px - (VIDEO_PANTHER_TRIGGER - 1), simX);
                        int bad = 0;
                        for (int k = kT + 1; k < kJ && k < t.path.Count; k++)
                            if (t.path[k - 1].x - t.path[k].x != 2) bad++;
                        Expect("chạy trên gờ 2 px/tick (tick sai)", 0, bad);
                        Expect("cất cánh khi mất sàn ở x = mép gờ − 1 (rồi bay 2 px cùng tick ⇒ x ghi = mép − 3)", edge - 3, kJ >= 0 ? t.path[kJ].x : -1);
                        ExpectRange("đỉnh nhảy (px trên gờ) — video 8.5–8.8", VIDEO_PANTHER_PEAK_MIN, VIDEO_PANTHER_PEAK_MAX, 112 - minY);
                        Expect("đáp sàn y 176", 176, kL >= 0 ? t.path[kL].y : -1);
                        int landX = kL >= 0 ? t.path[kL].x : -1;
                        int simLand = kL >= 0 && t.spawnRel + kL < recs.Count ? recs[t.spawnRel + kL].sx : 0;
                        measures.Add("bật tick " + kT + ", cất cánh " + kJ + " (x " + (kJ >= 0 ? t.path[kJ].x : -1) + "), đáp " + kL + " x " + landX
                                     + "; bay " + (kL - kJ) + " tick; video #" + (one ? "1: 699→cất ~690→đáp 594 sau ~51 tick" : "3: 955→~926→834 sau ~61 tick"));
                        ExpectRange("đáp x (video " + (one ? "594" : "834") + " ±8)", one ? 586 : 826, one ? 602 : 842, landX);
                        Expect("đáp xong quay về phía Simon", simLand > landX ? 1 : -1, kL >= 0 && kL + 1 < t.path.Count ? Math.Sign(t.path[kL + 1].x - t.path[kL].x) : 0);
                        int bad2 = 0, n2 = 0;
                        for (int k = kL + 2; k < t.path.Count; k++, n2++)
                            if (Math.Abs(t.path[k].x - t.path[k - 1].x) != 2) bad2++;
                        Expect("chạy sau khi đáp 2 px/tick (tick sai)", 0, bad2);
                    };
                }
                break;
            case "bat_ca":
                {
                    Go("CryptA", 49, 80, 1);
                    world.ResetRng();
                    var sp = AddSpawner<EdgeSpawner>(bat);
                    sp.side = eSpawnSide.FrontOfPlayer;
                    sp.playerRelativeY = true;
                    int first = -1;
                    onTick = rel =>
                    {
                        if (first < 0 && spawnRels.Count > 0) first = spawnRels[0];
                        if (first >= 0 && (rel == first + 10 || rel == first + 30 || rel == first + 50 || rel == first + 70)) Shot(rel + 1);
                    };
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Bat);
                        ExpectRange("dơi đầu sinh sau (tick) — video 8 / 28", 8, 28, ts.Count > 0 ? ts[0].spawnRel : -1);
                        if (ts.Count == 0) return;
                        var t = ts[0];
                        Expect("sinh mép phải (trước mặt Simon): x 248", NesCoord.SCREEN_W - VIDEO_INSET, t.x0);
                        Expect("tâm dơi = chân Simon − 14 (video f2760: NES 98 = section 66)", 80 - VIDEO_BAT_ABOVE, t.y0);
                        SpeedChecks(ts, -VIDEO_BAT_VX, 150, true);
                        int maxD = 0, kFirst = -1, kLast = -1;
                        for (int k = 0; k < t.path.Count && k <= VIDEO_BAT_PERIOD; k++)
                        {
                            int d = t.path[k].y - t.y0;
                            if (d > maxD) { maxD = d; kFirst = k; kLast = k; }
                            else if (d == maxD) kLast = k;
                        }
                        ExpectRange("biên độ đỉnh–đáy sóng (px) — video 16–19", VIDEO_BAT_PTP_MIN, VIDEO_BAT_PTP_MAX, maxD);
                        Expect("đáy sóng ở giữa chu kỳ (tick, TB lần đầu/cuối chạm đáy)", VIDEO_BAT_PERIOD / 2, (kFirst + kLast) / 2);
                        Expect("về lại đỉnh sau đúng 1 chu kỳ (y tick 100 − y0)", 0, t.path.Count > VIDEO_BAT_PERIOD ? t.path[VIDEO_BAT_PERIOD].y - t.y0 : -99);
                        FrameChecks(ts, VIDEO_BAT_FRAME);
                        ExpectRange("tối đa 1 dơi cùng lúc", 1, 1, MaxOf(r => r.enemies));
                        int bad = 0, n = 0;
                        for (int i = 1; i < ts.Count; i++)
                            if (ts[i - 1].endRel >= 0)
                            {
                                n++;
                                int d = ts[i].spawnRel - ts[i - 1].endRel;
                                if (d < VIDEO_BAT_RESPAWN_MIN || d > VIDEO_BAT_RESPAWN_MAX + 1) bad++;
                                measures.Add("dơi " + i + ": sinh sau khi con trước bị gỡ " + d + " tick");
                            }
                        ExpectRange("có ≥ 1 lần hồi sinh", 1, 99, n);
                        Expect("hồi sinh trong 194–330 tick — số lần sai", 0, bad);
                    };
                }
                break;
            case "bat_side_l":
            case "bat_side_r":
                {
                    bool left = scenario == "bat_side_l";
                    if (left) Go("CryptA", 200, 176, -1); else Go("CryptA", 49, 80, -1);
                    world.ResetRng();
                    var sp = AddSpawner<EdgeSpawner>(bat);
                    sp.side = eSpawnSide.FrontOfPlayer;
                    sp.playerRelativeY = true;
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Bat);
                        int cx = recs.Count > 0 ? recs[0].cx : 0;
                        Expect(left ? "Simon quay trái giữa màn ⇒ dơi mép trái (cx + 8)" : "Simon quay trái sát mép trái ⇒ dơi mép phải (cx + 248)",
                            left ? cx + VIDEO_INSET : cx + NesCoord.SCREEN_W - VIDEO_INSET, ts.Count > 0 ? ts[0].x0 : -1);
                        Expect("bay vào màn", left ? 1 : -1, ts.Count > 0 ? ts[0].facing0 : 0);
                    };
                }
                break;
            case "fishman_video":
                {
                    Go("CryptB", 86, 96, 1);
                    var a = Put(fish, 160, 192, -1) as Fishman;
                    a.Emerge(160);
                    var b = Put(fish, 32, 192, 1) as Fishman;
                    b.Emerge(160);
                    onTick = rel => { if (rel == 12 || rel == 40 || rel == 74 || rel == 100) Shot(rel + 1); };
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Fishman);
                        Expect("2 Fish Man", 2, ts.Count);
                        foreach (var t in ts)
                        {
                            int minY = 999, kMin = -1, kLand = -1, xs = 0, kVis = -1;
                            for (int k = 0; k < t.path.Count; k++)
                            {
                                if (t.path[k].y < minY) { minY = t.path[k].y; kMin = k; }
                                if (kLand < 0 && t.states[k] == "Stand") kLand = k;
                                if (kLand < 0 && t.path[k].x != t.x0) xs++;
                                if (kVis < 0 && !t.hidden[k]) kVis = k;
                            }
                            ExpectRange("x" + t.x0 + ": đỉnh chân Y (video NES 85–88 ⇒ 53–56)", VIDEO_FISH_APEX_Y - 1, VIDEO_FISH_APEX_Y + 2, minY);
                            Expect("x" + t.x0 + ": nhảy thẳng đứng (tick x đổi trước khi đáp)", 0, xs);
                            Expect("x" + t.x0 + ": đáp lối đi y 96", 96, kLand >= 0 ? t.path[kLand].y : -1);
                            ExpectRange("x" + t.x0 + ": hiện khi chân < mặt nước + 16 (tick)", 2, 5, kVis);
                            measures.Add("x" + t.x0 + ": đỉnh Y " + minY + " ở tick " + kMin + ", đáp tick " + kLand + " (video trồi f3514→đáp f3550 ≈ 72 tick), hiện tick " + kVis);
                            int kWalk = -1;
                            for (int k = kLand; k >= 0 && k < t.states.Count; k++)
                                if (t.states[k] == "Walk") { kWalk = k; break; }
                            ExpectRange("đứng sau khi đáp (tick) — video 6–10", VIDEO_FISH_STAND, VIDEO_FISH_STAND + 1, kWalk - kLand);
                            int dir = t.x0 > 86 ? -1 : 1, bad = 0, kF = -1;
                            for (int k = kWalk + 1; k < t.path.Count && t.states[k] == "Walk"; k++)
                            {
                                int want = (int)Math.Floor((t.x0 * 256 + dir * VIDEO_FISH_VX * (k - kWalk + 1)) / 256.0);
                                if (Math.Abs(t.path[k].x - want) > 1) bad++;
                            }
                            for (int k = kLand + 1; k < t.frames.Count; k++)
                                if (t.frames[k] != t.frames[kLand]) { kF = k; break; }
                            Expect("x" + t.x0 + ": đi 0.75 px/tick về phía Simon (tick lệch > 1)", 0, bad);
                            Expect("x" + t.x0 + ": đổi khung đi sau 32 tick kể từ lúc đáp", VIDEO_FISH_FRAME, kF - kLand);
                        }
                        ExpectRange("bọt nước khi trồi (FX ≥ 6 = 3 bọt × 2)", 6, 99, recs.Count > 1 ? recs[0].fx : 0);
                        ExpectRange("bắn cầu lửa sau 96 tick trên đất (quy ước) — số phát", 1, 4, MaxOf(r => r.shots));
                    };
                }
                break;
            case "fishman_gap":
                {
                    Go("CryptB", 86, 96, 1);
                    var a = Put(fish, 224, 192, -1) as Fishman;
                    a.Emerge(160);
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Fishman);
                        var t = ts.Count > 0 ? ts[0] : null;
                        Expect("rơi lại xuống nước (bị gỡ, không bị hạ)", 1, t != null && t.endRel >= 0 && !t.defeated ? 1 : 0);
                        Expect("0 điểm", 0, recs[recs.Count - 1].score);
                        measures.Add("gỡ sau " + (t != null ? t.endRel - t.spawnRel : -1) + " tick (video f3589→3627+ ≈ 80+)");
                    };
                }
                break;
            case "fishman_spawner":
                {
                    Go("CryptB", 120, 96, 1);
                    world.ResetRng();
                    var sp = AddSpawner<WaterSpawner>(fish);
                    sp.surfaceY = 160;
                    // mô phỏng người chơi hạ Fish Man (video: bị hạ 13–25 frame sau khi đáp) ⇒ đo nhịp hồi sinh theo suất
                    onTick = rel =>
                    {
                        foreach (var e in world.GroupList(eEntityGroup.Enemy))
                        {
                            var f = e as Fishman;
                            if (f != null && f.Alive && f.landAge >= 0 && f.ageTicks - f.landAge >= 40) f.Defeat();
                        }
                    };
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Fishman);
                        ExpectRange("số lần trồi trong 1200 tick", 6, 99, ts.Count);
                        int badCol = 0, badDist = 0;
                        foreach (var t in ts)
                        {
                            if ((t.x0 - VIDEO_FISH_OFFSET) % VIDEO_FISH_STEP != 0) badCol++;
                            int simX = t.spawnRel >= 0 && t.spawnRel < recs.Count ? recs[t.spawnRel].sx : 120;
                            if (Math.Abs(t.x0 - simX) < 32) badDist++;
                        }
                        Expect("cột trồi = 32 + 64k — số lần sai", 0, badCol);
                        Expect("cách Simon ≥ 32 — số lần sai", 0, badDist);
                        ExpectRange("tối đa sống cùng lúc", 1, 2, MaxOf(r => r.enemies));
                        // hồi sinh theo suất: mỗi lần trồi (trừ 2 lần đầu) ghép với lần gỡ sớm nhất chưa ghép trước nó
                        var ends = new List<int>();
                        foreach (var t in ts) if (t.endRel >= 0) ends.Add(t.endRel);
                        ends.Sort();
                        int bad = 0, n = 0, used = 0;
                        for (int i = 2; i < ts.Count; i++)
                        {
                            if (used >= ends.Count || ends[used] > ts[i].spawnRel) { bad++; continue; }
                            int d = ts[i].spawnRel - ends[used++];
                            n++;
                            if (d < VIDEO_FISH_RESPAWN_MIN || d > VIDEO_FISH_RESPAWN_MAX + 1) bad++;
                        }
                        ExpectRange("số lần hồi sinh đã ghép", 3, 99, n);
                        Expect("hồi sinh 44–176 tick sau khi con cùng suất bị gỡ — số lần sai", 0, bad);
                        var sb = new StringBuilder("cột: ");
                        foreach (var t in ts) sb.Append(t.x0).Append('@').Append(t.spawnRel).Append(' ');
                        measures.Add(sb.ToString());
                    };
                }
                break;
            case "kill_ghoul_l1":
            case "kill_panther_l2":
            case "kill_bat_l3":
            case "kill_fish_dagger":
                {
                    CvgpDebug.CheatInvincible = false;
                    Go("Arena", 300, 176, 1);
                    eEnemyId id;
                    int want;
                    if (scenario == "kill_ghoul_l1") { ses.whipLevel = 1; id = eEnemyId.Ghoul; want = WIKI_SCORE_GHOUL; onTick = rel => { if (rel == 12) Put(ghoul, 330, 176, -1); }; }
                    else if (scenario == "kill_panther_l2") { ses.whipLevel = 2; id = eEnemyId.Panther; want = WIKI_SCORE_PANTHER; onTick = rel => { if (rel == 12) Put(panther, 335, 176, -1); }; }
                    else if (scenario == "kill_bat_l3") { ses.whipLevel = 3; id = eEnemyId.Bat; want = WIKI_SCORE_BAT; onTick = rel => { if (rel == 12) Put(bat, 335, 158, -1); }; }
                    else { ses.subWeapon = eSubWeapon.Dagger; id = eEnemyId.Fishman; want = WIKI_SCORE_FISH; Put(fish, 345, 176, -1); }
                    finish = () =>
                    {
                        var ts = Tracks(id);
                        var t = ts.Count > 0 ? ts[0] : null;
                        Expect("hạ bằng 1 đòn (wiki 1/1)", 1, t != null && t.defeated ? 1 : 0);
                        Expect("điểm (wiki)", want, recs[recs.Count - 1].score);
                        Expect("Simon không mất máu", GameSession.MAX_HP, recs[recs.Count - 1].hp);
                        int fxLife = 0;
                        if (t != null)
                            for (int i = t.endRel; i < recs.Count && recs[i].fx > 0; i++) fxLife++;
                        ExpectRange("lửa chết + tia (FX) tồn tại (tick) — video 20", VIDEO_DEATH_FX_TICKS - 1, VIDEO_DEATH_FX_TICKS, fxLife);
                        if (id == eEnemyId.Fishman)
                            Expect("vũ khí phụ đếm trúng (subWeaponHits)", 1, GameSession.Current.subWeaponHits);
                    };
                }
                break;
            case "touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    Go("Arena", 300, 176, 1);
                    Put(ghoul, 330, 176, -1);
                    finish = () =>
                    {
                        Expect("chạm Zombie: −2 vạch (wiki Stage 1–6)", GameSession.MAX_HP - WIKI_DAMAGE_ST1, recs[recs.Count - 1].hp);
                        int hurt = -1;
                        for (int i = 0; i < recs.Count; i++) if (recs[i].state == eSimonState.Hurt) { hurt = i; break; }
                        ExpectRange("Simon văng (state Hurt) khi chạm (tick)", 10, 25, hurt);
                        measures.Add("chạm ở tick " + hurt + ", Simon x " + recs[0].sx + " → " + recs[recs.Count - 1].sx);
                    };
                }
                break;
            case "fireball":
                {
                    CvgpDebug.CheatInvincible = false;
                    Go("Arena", 300, 176, 1);
                    Put(fish, 400, 176, -1);
                    finish = () =>
                    {
                        Expect("cầu lửa (quy ước) trúng Simon: −2 vạch", GameSession.MAX_HP - WIKI_DAMAGE_ST1, recs[recs.Count - 1].hp);
                        ExpectRange("Fish Man đã bắn", 1, 3, MaxOf(r => r.shots));
                    };
                }
                break;
            case "freeze":
                {
                    Go("Arena", 600, 176, 1);
                    ses.subWeapon = eSubWeapon.Stopwatch;
                    ses.hearts = 10;
                    world.ResetRng();
                    var sp = AddSpawner<EdgeSpawner>(ghoul);
                    sp.side = eSpawnSide.Right;
                    sp.rows = new[] { 176 };
                    finish = () =>
                    {
                        int f0 = -1, f1 = -1;
                        for (int i = 0; i < recs.Count; i++)
                        {
                            if (f0 < 0 && recs[i].frozenE) f0 = i;
                            if (f0 >= 0 && f1 < 0 && !recs[i].frozenE) f1 = i;
                        }
                        Expect("Stopwatch đóng băng enemy (tick) — video Map_2", VIDEO_STOPWATCH, f1 - f0);
                        int moved = 0, spawned = 0;
                        foreach (var t in Tracks(eEnemyId.Ghoul))
                        {
                            for (int k = 1; k < t.path.Count; k++)
                            {
                                int rel = t.spawnRel + k;
                                if (rel > f0 && rel < f1 && t.path[k].x != t.path[k - 1].x) moved++;
                            }
                            if (t.spawnRel > f0 && t.spawnRel < f1) spawned++;
                        }
                        Expect("Zombie đứng yên khi đóng băng (tick đổi x)", 0, moved);
                        Expect("bộ sinh dừng khi đóng băng (lần sinh)", 0, spawned);
                    };
                }
                break;
            case "rosary":
                {
                    Go("EntranceHall", 49, 176, 1);
                    world.ResetRng();
                    var sp = AddSpawner<EdgeSpawner>(ghoul);
                    sp.side = eSpawnSide.Right;
                    sp.rows = new[] { 176 };
                    int killed = -1;
                    onTick = rel => { if (rel == 120) killed = world.RosaryClear(); };
                    finish = () =>
                    {
                        ExpectRange("Rosary gỡ Zombie trên màn", 1, 3, killed);
                        Expect("enemy trên màn sau Rosary", 0, recs.Count > 121 ? recs[121].enemies : -1);
                        Expect("Rosary 0 điểm", 0, recs[recs.Count - 1].score);
                    };
                }
                break;
            case "pot_roast":
                {
                    Go("CryptA", 220, 176, 1);
                    var sp = AddSpawner<EdgeSpawner>(bat);
                    sp.side = eSpawnSide.FrontOfPlayer;
                    sp.playerRelativeY = true;
                    sp.zoneMinX = 9999;   // không sinh thường — chỉ khi tường vỡ
                    sp.releaseOnBlock = "CryptA_X16_10";
                    onTick = rel =>
                    {
                        if (rel != 10) return;
                        foreach (var e in world.GroupList(eEntityGroup.Prop))
                        {
                            var b = e as BreakableBlock;
                            if (b != null && b.col == 16 && b.row == 10) b.Break();
                        }
                    };
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Bat);
                        Expect("tường vỡ ⇒ thả 1 dơi (wiki Secrets)", 1, ts.Count);
                        ExpectRange("dơi sinh ngay (tick sau khi vỡ)", 10, 11, ts.Count > 0 ? ts[0].spawnRel : -1);
                    };
                }
                break;
            default:
                Debug.LogError("[EnemyProbe] không có kịch bản " + scenario);
                return -1;
        }
        s.ResetStats();
        NesPad.I.PlayReplay(ticks);
        Begin(scenario, ticks.Count);
        return ticks.Count;
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
        spawnRels.Clear();
        startTick = world.TickCount;
        // thực thể sinh lúc dựng (Put trước tick đầu)
        foreach (var e in world.GroupList(eEntityGroup.Enemy))
            TrackOf(e as EnemyBase, 0);
        world.TickEnded += OnTickEnded;
        EnemyBase.Defeated += OnDefeated;
        EnemySpawner.Spawned += OnSpawned;
        Running = true;
    }

    private static void Unhook()
    {
        if (world != null)
            world.TickEnded -= OnTickEnded;
        EnemyBase.Defeated -= OnDefeated;
        EnemySpawner.Spawned -= OnSpawned;
        Running = false;
    }

    private static void OnDefeated(EnemyBase e)
    {
        var t = TrackOf(e, world.TickCount - startTick - 1);
        if (t != null)
            t.defeated = true;
    }

    private static void OnSpawned(EnemySpawner sp, EnemyBase e)
    {
        var t = TrackOf(e, world.TickCount - startTick - 1);
        if (t != null)
            t.fromSpawner = true;
        spawnRels.Add(world.TickCount - startTick - 1);
    }

    private static Track TrackOf(EnemyBase e, int rel)
    {
        if (e == null)
            return null;
        foreach (var t in tracks)
            if (t.e == e && t.spawnTick == e.SpawnTick)
                return t;
        var n = new Track { e = e, spawnTick = e.SpawnTick, spawnRel = rel, id = e.EnemyId, x0 = e.X, y0 = e.Y, facing0 = e.Facing };
        tracks.Add(n);
        return n;
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
        int enemies = 0, fx = 0, shotsN = 0;
        foreach (var e in w.GroupList(eEntityGroup.Enemy))
        {
            var en = e as EnemyBase;
            if (en == null || !en.Alive) continue;
            enemies++;
            var t = TrackOf(en, rel);
            t.path.Add(new Vector2Int(en.X, en.Y));
            t.frames.Add(en.Anim != null ? en.Anim.FrameIndex + (en.Anim.Current != null ? en.Anim.Current.name.GetHashCode() & 0x7F00 : 0) : 0);
            t.states.Add(en.DebugState.Split(' ')[0]);
            t.hidden.Add(en.hidden);
            var fm = en as Fishman;
            if (fm != null) shotsN += fm.shots;
            Mix((int)en.EnemyId); Mix(en.fx); Mix(en.fy); Mix(en.Facing);
        }
        foreach (var t in tracks)
            if (t.endRel < 0 && (!t.e.Alive || t.e.SpawnTick != t.spawnTick))
            {
                t.endRel = rel;
                t.reason = t.e.KillReason;
            }
        foreach (var e in w.GroupList(eEntityGroup.Effect))
            if (e.Alive) fx++;
        foreach (var e in w.GroupList(eEntityGroup.EnemyShot))
            if (e.Alive) { Mix(e.fx); Mix(e.fy); }
        var r = new Rec
        {
            sx = s.X, sy = s.Y, hp = ses.playerHp, score = ses.score, state = s.Fsm.CurState, cx = w.CameraCtl != null ? w.CameraCtl.X : 0,
            enemies = enemies, fx = fx, frozenE = w.IsEnemyFrozen, shots = shotsN,
        };
        recs.Add(r);
        Mix(rel); Mix(s.fx); Mix(s.fy); Mix((int)r.state); Mix(r.hp); Mix(r.score); Mix(enemies); Mix(fx); Mix(w.Rng.State);
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
        lastTracks.Clear();
        lastTracks.AddRange(tracks);
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

    private static readonly List<Track> lastTracks = new List<Track>();
    /// <summary>Vết enemy (TraceCsv) của mọi kịch bản đã chạy trong phiên — SaveTraces(dir) ghi ra trace_&lt;tên&gt;.csv.</summary>
    public static readonly Dictionary<string, string> Traces = new Dictionary<string, string>();

    public static int SaveTraces(string dir)
    {
        Directory.CreateDirectory(dir);
        foreach (var kv in Traces)
            File.WriteAllText(Path.Combine(dir, "trace_" + kv.Key + ".csv"), kv.Value, new UTF8Encoding(false));
        return Traces.Count;
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
        tracks.Clear();
        if (world != null)
            world.ClearNonPlayer();
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

    /// <summary>Vết enemy kịch bản vừa chạy: "id,spawnRel,k,rel,x,y,state,hidden" mỗi dòng (so quỹ đạo video).</summary>
    public static string TraceCsv()
    {
        var sb = new StringBuilder("id,spawnRel,k,rel,x,y,state,hidden,cx\n");
        foreach (var t in lastTracks)
            for (int k = 0; k < t.path.Count; k++)
            {
                int rel = t.spawnRel + k;
                int cx = rel >= 0 && rel < recs.Count ? recs[rel].cx : 0;
                sb.Append(t.id).Append(',').Append(t.spawnRel).Append(',').Append(k).Append(',').Append(rel).Append(',').Append(t.path[k].x).Append(',')
                  .Append(t.path[k].y).Append(',').Append(t.states[k]).Append(',').Append(t.hidden[k] ? 1 : 0).Append(',').Append(cx).Append('\n');
            }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- chấm

    static List<Track> Tracks(eEnemyId id)
    {
        var l = new List<Track>();
        foreach (var t in tracks)
            if (t.id == id)
                l.Add(t);
        l.Sort((a, b) => a.spawnRel.CompareTo(b.spawnRel));
        return l;
    }

    /// <summary>Nhịp nhóm: trong nhóm cách 32; giữa nhóm 115–288 (từ con cuối nhóm trước); nhóm đủ 3. Lần sinh trễ hơn vì đang đủ
    /// maxAlive 3 con (tick trước còn 3 — video tối đa 3) là đúng luật ⇒ không tính sai, ghi "*".</summary>
    static void GroupChecks(List<Track> ts)
    {
        var sb = new StringBuilder("khoảng sinh (tick; * = chờ vì đủ 3 con): ");
        int badIn = 0, badWave = 0, inGroup = 1, waves = 0, capped = 0;
        for (int i = 1; i < ts.Count; i++)
        {
            int d = ts[i].spawnRel - ts[i - 1].spawnRel;
            int r = ts[i].spawnRel - 1;
            bool cap = r >= 0 && r < recs.Count && recs[r].enemies >= VIDEO_GROUP;
            sb.Append(d).Append(cap ? "* " : " ");
            if (cap) capped++;
            if (inGroup < VIDEO_GROUP)
            {
                inGroup++;
                if (d != VIDEO_GROUP_GAP && !(cap && d > VIDEO_GROUP_GAP)) badIn++;
            }
            else
            {
                waves++;
                inGroup = 1;
                if (d < VIDEO_WAVE_MIN || (d > VIDEO_WAVE_MAX + 1 && !cap)) badWave++;
            }
        }
        measures.Add("số lần chờ vì đủ 3 con: " + capped);
        measures.Add(sb.ToString());
        Expect("trong nhóm cách 32 tick (video 29–33) — số lần sai", 0, badIn);
        Expect("giữa 2 nhóm 115–288 tick — số lần sai", 0, badWave);
        ExpectRange("số nhóm đã nghỉ giữa (≥ 2)", 2, 99, waves);
    }

    /// <summary>Tốc độ ngang fixed đúng từng tick (x_k = floor((x0·256 + v·k)/256)) — k tick đầu của mỗi vết đi thẳng.</summary>
    static void SpeedChecks(List<Track> ts, int vFixed, int maxK, bool straightOnly = false)
    {
        int bad = 0, n = 0;
        foreach (var t in ts)
        {
            int v = vFixed < 0 ? -vFixed * (t.facing0 > 0 ? 1 : -1) : vFixed * (t.facing0 > 0 ? 1 : -1);
            for (int k = 1; k < t.path.Count && k <= maxK; k++)
            {
                if (!straightOnly && t.path[k].y != t.y0) break;
                int want = (int)Math.Floor((t.x0 * 256.0 + v * k) / 256.0);
                n++;
                if (t.path[k].x != want) bad++;
            }
        }
        Expect("tốc độ ngang " + (Math.Abs(vFixed) / 256.0).ToString("0.###") + " px/tick (tick sai / " + n + ")", 0, bad);
    }

    /// <summary>Đổi khung anim đều mỗi `period` tick (trừ lần đầu).</summary>
    static void FrameChecks(List<Track> ts, int period)
    {
        int bad = 0, n = 0;
        foreach (var t in ts)
        {
            int last = -1;
            for (int k = 1; k < t.frames.Count; k++)
                if (t.frames[k] != t.frames[k - 1])
                {
                    if (last >= 0) { n++; if (k - last != period) bad++; }
                    last = k;
                }
        }
        ExpectRange("số lần đổi khung đã đo", 3, 99999, n);
        Expect("đổi khung mỗi " + period + " tick — số lần sai", 0, bad);
    }

    static int MaxOf(Func<Rec, int> f)
    {
        int m = int.MinValue;
        foreach (var r in recs)
            m = Math.Max(m, f(r));
        return m;
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
