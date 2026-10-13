#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness enemy Block 2–3 (CVGP 17; routine 21 chạy lại) — CHỈ Editor, scene Arena (Player = Simon). Khuôn EnemyProbe (13). Qua MCP khi Play:
///   EnemyB23Probe.SaveReplays()                          ⇒ Assets/Data/Tests/Replays/enemy_b23_&lt;tên&gt;.txt
///   EnemyB23Probe.RunSuite(EnemyB23Probe.ScenarioNames()) ⇒ chạy (input ĐỌC TỪ FILE), ghi từng tick, TỰ CHẤM ⇒ SuiteReport()
///   EnemyB23Probe.ShotDir = thư mục ⇒ kịch bản chụp Game view ở các tick cố định (≥ 4 ảnh/enemy)
/// Không có video Block 2–3 ⇒ kỳ vọng = hằng WIKI_* (số đòn, điểm, sát thương theo stage) + EST_* (giá trị ƯỚC LƯỢNG routine 17 chép
/// độc lập Config — đổi Config mà quên đổi đây thì bộ kiểm báo). Geometry Arena giữ nguyên (sàn y 176 x 0–415, tường thấp 416–431 đỉnh
/// y 144, hố 480–511) — đủ cho bục/khoảng trống, không đụng bộ kiểm 07–16.
/// </summary>
public static class EnemyB23Probe
{
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";

    // ---------------------------------------------------------------- kỳ vọng
    public const int WIKI_SCORE_KNIGHT = 400, WIKI_SCORE_MEDUSA = 300, WIKI_SCORE_TOWER = 400, WIKI_SCORE_GHOST = 300;
    public const int WIKI_SCORE_RAVEN = 200, WIKI_SCORE_SKEL = 300, WIKI_SCORE_FLEA = 500, WIKI_PROJ_SCORE = 100;
    public const int WIKI_KNIGHT_W = 3, WIKI_KNIGHT_S = 2, WIKI_TOWER_W = 8, WIKI_TOWER_S = 6, WIKI_GHOST_W = 3, WIKI_GHOST_S = 2;
    public const int WIKI_DMG_B2 = 2, WIKI_DMG_B3 = 3;     // wiki Gameplay#Damage: Stage 4–6 = 2, 7–9 = 3
    public const int WIKI_STOPWATCH = 300;
    public const int EST_KNIGHT_VX = 128;                  // 0.5 px/tick
    public const int EST_KNIGHT_LEG_MAX = 192;
    public const int EST_MEDUSA_VX = 256, EST_MEDUSA_A = 24, EST_MEDUSA_P = 96, EST_MEDUSA_ABOVE = 24;
    public const int EST_MEDUSA_GAP_MIN = 64, EST_MEDUSA_GAP_MAX = 128, EST_MEDUSA_FIRST = 32;
    public const int EST_TOWER_FIRST = 48, EST_TOWER_WARN = 32, EST_TOWER_GAP = 16, EST_TOWER_IDLE_MIN = 96, EST_TOWER_IDLE_MAX = 160;
    public const int EST_TOWER_BALL_VX = 384, EST_TOWER_TOP = 16, EST_TOWER_BOTTOM = 0;
    public const int EST_GHOST_VX = 160, EST_GHOST_VY = 128;
    public const int EST_RAVEN_TRIGGER = 96, EST_RAVEN_RISE = 16, EST_RAVEN_HOVER1 = 24, EST_RAVEN_HOVER = 12, EST_RAVEN_SPEED = 512;
    public const int EST_SKEL_GAP = 10, EST_SKEL_BONE_VX = 256, EST_SKEL_BONE_VX_STEP = 96, EST_SKEL_BONE_VY = -896, EST_SKEL_BONE_VY_STEP = 128;
    public const int EST_FLEA_TRIGGER = 112, EST_FLEA_SHORT_VY = -512, EST_FLEA_LONG_VY = -1152, EST_FLEA_G = 64, EST_FLEA_PAUSE_MIN = 4, EST_FLEA_PAUSE_MAX = 16;
    public const int EST_BAT_SWOOP_VY = 384, EST_BAT_SWOOP_ABOVE = 14, EST_BAT_PERCH = 64;

    private struct Scn
    {
        public string name, rle, note;
        public Scn(string n, string r, string note) { name = n; rle = r; this.note = note; }
    }

    const string WHIP5 = "B -*23 B -*23 B -*23 B -*23 B -*23";
    const string WHIP10 = WHIP5 + " " + WHIP5;

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("knight_patrol", "-*600", "Arena: Simon (60,176) đứng; Spear Knight (250,176) tuần tra 0.5 px/tick, quay đầu ngẫu nhiên 64–192 tick, không rời sàn"),
        new Scn("knight_ledge", "-*400", "Spear Knight trên đỉnh tường thấp (424,144) — quay đầu ở mép, không rơi"),
        new Scn("knight_touch", "-*90", "Stage 4: Spear Knight đi vào Simon (không bất tử): −2 vạch (chỉ va chạm, không đâm giáo)"),
        new Scn("knight_whip_l1", WHIP5 + " -*40", "roi da (yếu) hạ Spear Knight: đúng 3 đòn, +400, tia lửa ở đòn chưa hạ"),
        new Scn("knight_whip_l2", WHIP5 + " -*40", "roi xích (mạnh): đúng 2 đòn"),
        new Scn("medusa_front", "-*900", "Simon (200,176) quay phải đứng: EdgeSpawner Medusa Head trước mặt — 1.0 px/tick, sóng A 24 P 96, ≤ 2 con, cách 64–128"),
        new Scn("medusa_back", "-*240", "Simon quay TRÁI giữa màn ⇒ Medusa Head ra từ mép trái, bay sang phải"),
        new Scn("medusa_kill", "-*30 " + WHIP5 + " -*40", "roi cấp 3 hạ Medusa Head bay ngang tầm: 1 đòn, +300"),
        new Scn("medusa_touch", "-*160", "Stage 5: Medusa Head chạm Simon: −2 vạch"),
        new Scn("tower_fire", "-*420", "Bone Tower (260,176), Simon (150,176): nghỉ 48 ⇒ nháy tím/đỏ 32 ⇒ 2 cầu lửa (đầu trên rồi dưới cách 16) bay 1.5 px/tick ⇒ nghỉ 96–160"),
        new Scn("tower_fire_hit", "-*220", "Stage 6: cầu lửa Bone Tower trúng Simon: −2 vạch"),
        new Scn("tower_whip_ball", "-*88 " + WHIP5 + " -*60", "quất vỡ cầu lửa (wiki): cầu biến mất + 100 đ (suy luận), Simon không mất máu"),
        new Scn("tower_kill_l1", WHIP10 + " -*30", "roi da hạ Bone Tower: đúng 8 đòn, +400"),
        new Scn("tower_kill_l2", WHIP10 + " -*30", "roi xích: đúng 6 đòn"),
        new Scn("ghost_chase", "-*600", "Ghost (260,100), Simon (150,176) đứng: bám theo, tăng tốc dần, trần 0.625/0.5 px/tick, vượt quá rồi vòng lại"),
        new Scn("ghost_kill_l1", WHIP10 + " -*30", "roi da hạ Ghost: đúng 3 đòn, +300"),
        new Scn("ghost_kill_l2", WHIP10 + " -*30", "roi xích: đúng 2 đòn"),
        new Scn("ghost_touch", "-*200", "Stage 7: Ghost chạm Simon: −3 vạch"),
        new Scn("raven_dive", "R*60 -*360", "Raven đậu (260,112); Simon đi phải từ 120: cất cánh khi |dx| < 96, lên 16, khựng 24, lao thẳng 2 px/tick tới tâm thân Simon, khựng 12, lao tiếp"),
        new Scn("raven_touch", "-*160", "Stage 7: Raven bổ nhào trúng Simon: −3 vạch"),
        new Scn("raven_kill", "-*36 " + WHIP5 + " -*40", "roi da hạ Raven đang lao: 1 đòn, +200"),
        new Scn("skel_idle", "-*900", "Skeleton (300,176), Simon (200,176) đứng: giữ khoảng 48–96, nhảy, ném loạt 2–3 xương vòng cung khác nhau cách 10 tick"),
        new Scn("skel_bone_hit", "-*300", "Stage 7: xương trúng Simon: −3 vạch/đòn"),
        new Scn("skel_kill", "-*10 Sub -*80", "dao hạ Skeleton: 1 đòn, +300"),
        new Scn("flea_hops", "R*60 -*540", "Fleaman (300,176) chờ; Simon đi phải từ 150: bật khi |dx| < 112, chu kỳ nhảy ngắn-ngắn-vọt, nghỉ 4–16"),
        new Scn("flea_touch", "-*120", "Stage 7: Fleaman nhảy vào Simon: −3 vạch"),
        new Scn("flea_kill", "-*2 " + WHIP5 + " -*20", "roi da hạ Fleaman: 1 đòn, +500"),
        new Scn("bat_black", "R*100 -*200", "Black Bat treo (300,96); Simon đi phải từ 150: |dx| < 64 ⇒ sà xuống 1.5 px/tick tới chân Simon − 14 rồi bay sóng như dơi đỏ"),
        new Scn("freeze_b23", "-*60 Sub -*400", "Stopwatch: mọi enemy Block 2–3 + đạn + bộ sinh Medusa Head dừng 300 tick rồi chạy tiếp"),
        new Scn("freeze_whip", "-*2 Sub -*10 " + WHIP5 + " -*40", "Stopwatch + roi da: Spear Knight đứng yên vẫn ăn đủ 3 đòn (bộ đếm bất tử chạy khi đóng băng — wiki)"),
        new Scn("rosary_b23", "-*200", "Rosary (RosaryClear) tick 150: mọi enemy + đạn Block 2–3 trên màn biến mất, 0 điểm"),
    };

    // ---------------------------------------------------------------- ghi
    public class Track
    {
        public EnemyBase e;
        public int spawnTick, spawnRel = -1, endRel = -1, x0, y0, facing0, hits;
        public eEnemyId id;
        public bool defeated, fromSpawner;
        public eKillReason reason;
        public readonly List<Vector2Int> path = new List<Vector2Int>();
        public readonly List<Vector2Int> fpath = new List<Vector2Int>();   // fixed
        public readonly List<string> states = new List<string>();
        public readonly List<string> anims = new List<string>();
        public readonly List<int> facing = new List<int>();
    }

    public class Shot
    {
        public Entity e;
        public int spawnTick, spawnRel, endRel = -1;
        public string type;
        public bool broken;
        public readonly List<Vector2Int> fpath = new List<Vector2Int>();
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
    public static string ShotDir;
    private static readonly List<Rec> recs = new List<Rec>();
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> measures = new List<string>();
    private static readonly List<Track> tracks = new List<Track>();
    private static readonly List<Shot> shotsT = new List<Shot>();
    private static readonly List<int> spawnRels = new List<int>();
    private static readonly List<string> playerHits = new List<string>();
    private static readonly Queue<string> suite = new Queue<string>();
    public static readonly List<string> SuiteReports = new List<string>();
    public static bool SuiteDone { get; private set; } = true;
    public static int SuiteFails { get; private set; }
    public static uint SuiteHash { get; private set; }
    private static StageWorld world;
    private static int target, recorded, startTick, savedStage;
    private static Action finish;
    private static Action<int> onTick;
    private static readonly List<GameObject> temp = new List<GameObject>();
    private static readonly Dictionary<int, string> shots = new Dictionary<int, string>();
    private static string current;

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string ReplayPath(string name) { return REPLAY_DIR + "/enemy_b23_" + name + ".txt"; }

    public static int SaveReplays()
    {
        Directory.CreateDirectory(REPLAY_DIR);
        foreach (var s in SCENARIOS)
        {
            var ticks = NesReplay.Decode(s.rle);
            var sb = new StringBuilder();
            sb.Append("# ").Append(ticks.Count).Append(" tick, hash ").Append(NesReplay.Hash(ticks).ToString("x8")).Append('\n');
            sb.Append("# CVGP 17 EnemyB23Probe kịch bản '").Append(s.name).Append("': ").Append(s.note).Append('\n');
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
        sb.Append("ENEMY B2-3 SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
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

    static void Go(string section, int x, int y, int facing)
    {
        LevelSection sec = null;
        foreach (var s in world.Sections)
            if (s.data != null && s.data.name == section)
                sec = s;
        world.Warp(sec, x, y);
        Simon.Teleport(x, y, facing);
        if (world.CameraCtl != null)
            world.CameraCtl.SnapToTarget();
    }

    static T AddSpawner<T>(EnemyBase prefab) where T : EnemySpawner
    {
        var go = new GameObject("EnemyB23Probe_" + typeof(T).Name);
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

    static void Photo(params int[] rels)
    {
        if (string.IsNullOrEmpty(ShotDir))
            return;
        foreach (var rel in rels)
            shots[rel] = Path.Combine(ShotDir, current + "_r" + rel.ToString("000") + ".png");
    }

    // ---------------------------------------------------------------- 1 kịch bản

    public static int Run(string scenario)
    {
        world = StageWorld.Current;
        var s = Simon;
        if (world == null || s == null || world.Items == null)
        {
            Debug.LogError("[EnemyB23Probe] cần scene Arena với Player = SimonController + StageWorld.itemCatalog");
            return -1;
        }
        string p = ReplayPath(scenario);
        if (!File.Exists(p))
        {
            Debug.LogError("[EnemyB23Probe] thiếu " + p + " — gọi EnemyB23Probe.SaveReplays()");
            return -1;
        }
        var ticks = NesReplay.Decode(File.ReadAllText(p));
        Cleanup();
        current = scenario;
        savedStage = world.Stage;
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
        world.FreezeEnemies(0);   // bỏ Stopwatch còn lại của kịch bản trước
        world.ResetRng();
        var knight = Prefab<SpearKnight>("SpearKnight");
        var medusa = Prefab<MedusaHead>("MedusaHead");
        var tower = Prefab<BoneTower>("BoneTower");
        var ghost = Prefab<Ghost>("Ghost");
        var raven = Prefab<Raven>("Raven");
        var skel = Prefab<Skeleton>("Skeleton");
        var flea = Prefab<Fleaman>("Fleaman");
        var bat = Prefab<Bat>("Bat");
        switch (scenario)
        {
            case "knight_patrol":
                {
                    Go("Arena", 60, 176, 1);
                    Put(knight, 250, 176, -1);
                    Photo(30, 150, 300, 450);
                    finish = () =>
                    {
                        var t = First(eEnemyId.SpearKnight);
                        if (t == null) return;
                        Expect("không rời sàn y 176 (tick sai)", 0, CountWhere(t, k => t.path[k].y != 176));
                        LegChecks(t, EST_KNIGHT_VX, EST_KNIGHT_LEG_MAX, 2);
                    };
                }
                break;
            case "knight_ledge":
                {
                    Go("Arena", 300, 176, 1);
                    Put(knight, 424, 144, -1);
                    Photo(20, 100, 200, 300);
                    finish = () =>
                    {
                        var t = First(eEnemyId.SpearKnight);
                        if (t == null) return;
                        Expect("đứng trên đỉnh tường y 144 suốt (tick sai)", 0, CountWhere(t, k => t.path[k].y != 144));
                        Expect("x trong [416, 431] (không bước khỏi mép)", 0, CountWhere(t, k => t.path[k].x < 416 || t.path[k].x > 431));
                        ExpectRange("quay đầu ở mép (lần)", 2, 999, t.e is SpearKnight ? ((SpearKnight)t.e).turns : 0);
                    };
                }
                break;
            case "knight_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 4;
                    Go("Arena", 200, 176, 1);
                    var k = Put(knight, 240, 176, -1) as SpearKnight;
                    k.legTimer = 9999;
                    finish = () =>
                    {
                        Expect("chạm Spear Knight: −2 vạch (wiki Stage 4)", GameSession.MAX_HP - WIKI_DMG_B2, Last.hp);
                        ExpectRange("Simon văng (Hurt)", 1, 999, CountRecs(r => r.state == eSimonState.Hurt));
                        Expect("chỉ thân enemy gây đòn (không đạn/giáo)", 0, CountPlayerHits("EnemyAttack"));
                    };
                }
                break;
            case "knight_whip_l1":
            case "knight_whip_l2":
                {
                    bool strong = scenario.EndsWith("l2");
                    ses.whipLevel = strong ? 2 : 1;
                    Go("Arena", 200, 176, 1);
                    var k = Put(knight, 236, 176, -1) as SpearKnight;
                    k.legTimer = 9999;
                    Photo(20, 44);
                    finish = () => KillChecks(eEnemyId.SpearKnight, strong ? WIKI_KNIGHT_S : WIKI_KNIGHT_W, WIKI_SCORE_KNIGHT, true);
                }
                break;
            case "medusa_front":
            case "medusa_back":
                {
                    bool back = scenario == "medusa_back";
                    Go("Arena", 200, 176, back ? -1 : 1);
                    var sp = AddSpawner<EdgeSpawner>(medusa);
                    sp.side = eSpawnSide.FrontOfPlayer;
                    sp.playerRelativeY = true;
                    if (!back) Photo(60, 84, 108, 132, 400);
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.MedusaHead);
                        int cx = recs.Count > 0 ? recs[0].cx : 0;
                        ExpectRange("số lần sinh", back ? 1 : 6, 99, ts.Count);
                        if (ts.Count == 0) return;
                        Expect("lần sinh đầu (tick)", EST_MEDUSA_FIRST - 1, ts[0].spawnRel);
                        int badX = 0, badF = 0, badY = 0;
                        foreach (var t in ts)
                        {
                            if (t.x0 != (back ? cx + 8 : cx + NesCoord.SCREEN_W - 8)) badX++;
                            if (t.facing0 != (back ? 1 : -1)) badF++;
                            if (t.y0 != 176 - EST_MEDUSA_ABOVE) badY++;
                        }
                        Expect(back ? "Simon quay trái ⇒ sinh mép trái (cx + 8) — số lần sai" : "sinh mép trước mặt (cx + 248) — số lần sai", 0, badX);
                        Expect("bay vào màn — số lần sai", 0, badF);
                        Expect("tâm sóng = chân Simon − 24 — số lần sai", 0, badY);
                        var t0 = ts[0];
                        int badV = 0, n = 0, minY = 999, maxY = -999;
                        for (int k = 1; k < t0.fpath.Count; k++, n++)
                            if (t0.fpath[k].x - t0.fpath[k - 1].x != t0.facing0 * EST_MEDUSA_VX) badV++;
                        for (int k = 0; k < t0.path.Count; k++) { minY = Math.Min(minY, t0.path[k].y); maxY = Math.Max(maxY, t0.path[k].y); }
                        Expect("ngang 1.0 px/tick (tick sai / " + n + ")", 0, badV);
                        ExpectRange("biên độ đỉnh–đáy (px) — ước lượng 2·24", 2 * EST_MEDUSA_A - 1, 2 * EST_MEDUSA_A + 1, maxY - minY);
                        int badW = 0;
                        for (int k = 0; k < t0.fpath.Count; k++)
                        {
                            int want = t0.y0 * 256 - (int)Math.Round(EST_MEDUSA_A * Math.Sin(2 * Math.PI * k / EST_MEDUSA_P) * 256);
                            if (Math.Abs(t0.fpath[k].y - want) > 1) badW++;
                        }
                        Expect("y = y0 − 24·sin(2πt/96) đúng từng tick (tick sai)", 0, badW);
                        if (!back)
                        {
                            ExpectRange("tối đa sống cùng lúc", 1, 2, MaxOf(r => r.enemies));
                            int badGap = 0;
                            var sb = new StringBuilder("khoảng sinh: ");
                            for (int i = 1; i < ts.Count; i++)
                            {
                                int d = ts[i].spawnRel - ts[i - 1].spawnRel;
                                bool cap = ts[i].spawnRel - 1 >= 0 && recs[ts[i].spawnRel - 1].enemies >= 2;
                                sb.Append(d).Append(cap ? "* " : " ");
                                if (d < EST_MEDUSA_GAP_MIN || (d > EST_MEDUSA_GAP_MAX + 1 && !cap)) badGap++;
                            }
                            measures.Add(sb.ToString());
                            Expect("khoảng sinh 64–128 tick — số lần sai", 0, badGap);
                        }
                    };
                }
                break;
            case "medusa_kill":
                {
                    ses.whipLevel = 3;
                    Go("Arena", 200, 176, 1);
                    Put(medusa, 300, 176 - EST_MEDUSA_ABOVE, -1);
                    Photo(30, 46);
                    finish = () => KillChecks(eEnemyId.MedusaHead, 1, WIKI_SCORE_MEDUSA, false);
                }
                break;
            case "medusa_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 5;
                    Go("Arena", 200, 176, 1);
                    Put(medusa, 300, 176 - EST_MEDUSA_ABOVE, -1);
                    finish = () =>
                    {
                        Expect("Medusa Head chạm Simon: −2 vạch (Stage 5)", GameSession.MAX_HP - WIKI_DMG_B2, Last.hp);
                    };
                }
                break;
            case "tower_fire":
                {
                    Go("Arena", 150, 176, 1);
                    Put(tower, 260, 176, -1);
                    Photo(52, 56, 82, 110, 200);
                    finish = () =>
                    {
                        var t = First(eEnemyId.BoneTower);
                        if (t == null) return;
                        Expect("bất động (tick x/y đổi)", 0, CountWhere(t, k => t.path[k] != t.path[0]));
                        int kW = t.states.IndexOf("Warn"), kF = t.states.IndexOf("Fire");
                        Expect("đứng nghỉ đầu (tick trước khi nháy)", EST_TOWER_FIRST, kW);
                        Expect("nháy kéo dài (tick)", EST_TOWER_WARN, kF - kW);
                        int red = 0, flips = 0;
                        for (int k = kW; k < kF; k++)
                        {
                            if (t.anims[k] == "red") red++;
                            if (k > kW && t.anims[k] != t.anims[k - 1]) flips++;
                        }
                        Expect("nháy tím/đỏ mỗi 4 tick: số lần đổi màu trong 32 tick (+ đổi về tím lúc bắn)", EST_TOWER_WARN / 4, flips);
                        ExpectRange("tick đỏ khi nháy", 12, 20, red);
                        var balls = ShotsOf("BoneTowerFireball");
                        ExpectRange("số cầu lửa (≥ 2 lượt)", 4, 99, balls.Count);
                        int badPair = 0, badVx = 0, badY = 0;
                        for (int i = 0; i + 1 < balls.Count; i += 2)
                        {
                            if (balls[i + 1].spawnRel - balls[i].spawnRel != EST_TOWER_GAP) badPair++;
                            if (balls[i].fpath.Count > 0 && Fixed.ToPx(balls[i].fpath[0].y) != 176 - EST_TOWER_TOP) badY++;
                            if (balls[i + 1].fpath.Count > 0 && Fixed.ToPx(balls[i + 1].fpath[0].y) != 176 - EST_TOWER_BOTTOM) badY++;
                        }
                        foreach (var b in balls)
                            for (int k = 1; k < b.fpath.Count; k++)
                                if (b.fpath[k].x - b.fpath[k - 1].x != -EST_TOWER_BALL_VX || b.fpath[k].y != b.fpath[0].y) badVx++;
                        Expect("2 cầu/lượt cách 16 tick — số lượt sai", 0, badPair);
                        Expect("đầu trên y − 16, đầu dưới y − 0 — số cầu sai", 0, badY);
                        Expect("bay ngang −1.5 px/tick về phía Simon (tick sai)", 0, badVx);
                        int gap = balls.Count >= 3 ? balls[2].spawnRel - balls[1].spawnRel : -1;
                        ExpectRange("nghỉ giữa 2 lượt (cầu dưới → cầu trên kế, tick) = nghỉ 96–160 + nháy 32", EST_TOWER_IDLE_MIN + EST_TOWER_WARN, EST_TOWER_IDLE_MAX + EST_TOWER_WARN + 1, gap);
                        measures.Add("cầu lửa sinh ở tick: " + string.Join(" ", balls.ConvertAll(b => b.spawnRel.ToString()).ToArray()));
                    };
                }
                break;
            case "tower_fire_hit":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 6;
                    Go("Arena", 150, 176, 1);
                    Put(tower, 260, 176, -1);
                    finish = () =>
                    {
                        Expect("cầu lửa trúng Simon: −2 vạch (Stage 6)", GameSession.MAX_HP - WIKI_DMG_B2, Last.hp);
                        Expect("đòn đầu do cầu lửa (EnemyAttack)", 1, playerHits.Count > 0 && playerHits[0].StartsWith("EnemyAttack") ? 1 : 0);
                    };
                }
                break;
            case "tower_whip_ball":
                {
                    ses.whipLevel = 2;
                    Go("Arena", 200, 176, 1);
                    Put(tower, 300, 176, -1);
                    Photo(80, 104, 106);
                    finish = () =>
                    {
                        var balls = ShotsOf("BoneTowerFireball");
                        int broken = 0;
                        foreach (var b in balls) if (b.broken) broken++;
                        ExpectRange("cầu lửa bị roi phá", 1, 2, broken);
                        Expect("+100 / cầu phá (wiki: một số đạn 100 đ — suy luận)", broken * WIKI_PROJ_SCORE, Last.score);
                        Expect("Simon không mất máu", GameSession.MAX_HP, Last.hp);
                        Expect("Bone Tower không trúng (ngoài tầm roi)", 0, First(eEnemyId.BoneTower) != null ? First(eEnemyId.BoneTower).hits : -1);
                    };
                }
                break;
            case "tower_kill_l1":
            case "tower_kill_l2":
                {
                    bool strong = scenario.EndsWith("l2");
                    ses.whipLevel = strong ? 2 : 1;
                    Go("Arena", 200, 176, 1);
                    Put(tower, 234, 176, -1);
                    finish = () => KillChecks(eEnemyId.BoneTower, strong ? WIKI_TOWER_S : WIKI_TOWER_W, WIKI_SCORE_TOWER, true);
                }
                break;
            case "ghost_chase":
                {
                    Go("Arena", 150, 176, 1);
                    Put(ghost, 260, 100, -1);
                    Photo(40, 120, 200, 320);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Ghost);
                        if (t == null) return;
                        int badV = 0, crosses = 0, minD = 9999, kMin = -1;
                        for (int k = 1; k < t.fpath.Count; k++)
                        {
                            int dx = Math.Abs(t.fpath[k].x - t.fpath[k - 1].x), dy = Math.Abs(t.fpath[k].y - t.fpath[k - 1].y);
                            if (dx > EST_GHOST_VX || dy > EST_GHOST_VY) badV++;
                            int rel = t.spawnRel + k;
                            int sx = rel < recs.Count ? recs[rel].sx : 150;
                            if (Math.Sign(t.path[k].x - sx) != Math.Sign(t.path[k - 1].x - sx) && t.path[k].x != sx) crosses++;
                            int d = Math.Abs(t.path[k].x - sx) + Math.Abs(t.path[k].y - (176 - 16));
                            if (d < minD) { minD = d; kMin = k; }
                        }
                        Expect("tốc độ ≤ 0.625 ngang / 0.5 dọc (tick vượt)", 0, badV);
                        ExpectRange("tới sát tâm thân Simon (khoảng cách |dx|+|dy| nhỏ nhất)", 0, 12, minD);
                        ExpectRange("vượt qua Simon rồi vòng lại (số lần cắt ngang x Simon)", 2, 999, crosses);
                        int badF = CountWhere(t, k => k > 0 && t.fpath[k].x != t.fpath[k - 1].x && t.facing[k] != Math.Sign(t.fpath[k].x - t.fpath[k - 1].x));
                        Expect("quay mặt theo hướng bay ngang (tick sai)", 0, badF);
                        measures.Add("gần nhất ở tick " + kMin + " (d " + minD + "), cắt ngang " + crosses + " lần");
                    };
                }
                break;
            case "ghost_kill_l1":
            case "ghost_kill_l2":
                {
                    bool strong = scenario.EndsWith("l2");
                    ses.whipLevel = strong ? 2 : 1;
                    Go("Arena", 150, 176, 1);
                    Put(ghost, 196, 156, -1);
                    finish = () => KillChecks(eEnemyId.Ghost, strong ? WIKI_GHOST_S : WIKI_GHOST_W, WIKI_SCORE_GHOST, true);
                }
                break;
            case "ghost_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 7;
                    Go("Arena", 150, 176, 1);
                    Put(ghost, 220, 140, -1);
                    finish = () => Expect("Ghost chạm Simon: −3 vạch (Stage 7)", GameSession.MAX_HP - WIKI_DMG_B3, Last.hp);
                }
                break;
            case "raven_dive":
                {
                    Go("Arena", 120, 176, 1);
                    Put(raven, 260, 112, -1);
                    Photo(30, 56, 80, 96, 120, 160);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Raven);
                        if (t == null) return;
                        int kR = t.states.IndexOf("Rise"), kH = t.states.IndexOf("Hover"), kD = t.states.IndexOf("Dive");
                        int simX = kR >= 0 ? recs[t.spawnRel + kR].sx : -1;
                        Expect("cất cánh khi |Simon − Raven| < 96: Simon X lúc cất cánh = 260 − 95", 260 - (EST_RAVEN_TRIGGER - 1), simX);
                        Expect("đậu yên trước khi cất cánh (tick đổi vị trí)", 0, CountWhere(t, k => k < kR && t.path[k] != t.path[0]));
                        Expect("lên 16 px thẳng đứng", EST_RAVEN_RISE, kH >= 0 ? t.path[kR].y - t.path[kH].y + 1 : -1);
                        Expect("khựng lần đầu (tick)", EST_RAVEN_HOVER1, kD - kH);
                        int badS = 0, n = 0;
                        for (int k = kD + 1; k < t.fpath.Count && t.states[k] == "Dive"; k++, n++)
                        {
                            double v = Math.Sqrt(Math.Pow(t.fpath[k].x - t.fpath[k - 1].x, 2) + Math.Pow(t.fpath[k].y - t.fpath[k - 1].y, 2));
                            if (v < EST_RAVEN_SPEED - 8 || v > EST_RAVEN_SPEED + 2) badS++;
                        }
                        Expect("lao thẳng 2 px/tick (|v| sai / " + n + ")", 0, badS);
                        int dives = t.e is Raven ? ((Raven)t.e).dives : 0;
                        ExpectRange("lao lại sau khi trượt (số cú lao)", 2, 99, dives);
                        int kH2 = -1;
                        for (int k = kD + 1; k < t.states.Count; k++) if (t.states[k] == "Hover") { kH2 = k; break; }
                        int kD2 = kH2 >= 0 ? t.states.IndexOf("Dive", kH2) : -1;
                        Expect("khựng giữa 2 cú lao (tick) — 'quay đầu rất nhanh'", EST_RAVEN_HOVER, kD2 - kH2);
                        int maxY = 0; foreach (var v in t.path) maxY = Math.Max(maxY, v.y);
                        ExpectRange("không lao xuyên sàn: tâm ≤ chân Simon − 8 (+ 1 tick lao 2 px)", 0, 176 - 8 + 2, maxY);
                        measures.Add("cất cánh tick " + kR + ", khựng " + kH + ", lao " + kD + ", lao 2 " + kD2 + "; số cú lao " + dives);
                    };
                }
                break;
            case "raven_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 7;
                    Go("Arena", 200, 176, 1);
                    Put(raven, 270, 140, -1);
                    finish = () => Expect("Raven bổ nhào trúng Simon: −3 vạch (Stage 7)", GameSession.MAX_HP - WIKI_DMG_B3, Last.hp);
                }
                break;
            case "raven_kill":
                {
                    Go("Arena", 200, 176, 1);
                    Put(raven, 280, 150, -1);
                    Photo(40, 56);
                    finish = () => KillChecks(eEnemyId.Raven, 1, WIKI_SCORE_RAVEN, false);
                }
                break;
            case "skel_idle":
                {
                    Go("Arena", 200, 176, 1);
                    Put(skel, 300, 176, -1);
                    Photo(52, 64, 80, 300, 600);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Skeleton);
                        if (t == null) return;
                        var bs = ShotsOf("SkeletonBone");
                        var sk = t.e as Skeleton;
                        ExpectRange("số loạt ném (≥ 3 trong 900 tick)", 3, 99, sk != null ? sk.bursts : 0);
                        // ghép loạt: xương cách nhau đúng 10 tick thuộc cùng loạt
                        int badBurst = 0, badPath = 0, bursts = 0, k0 = 0;
                        var sizes = new StringBuilder();
                        for (int i = 0; i <= bs.Count; i++)
                        {
                            if (i < bs.Count && i > k0 && bs[i].spawnRel - bs[i - 1].spawnRel == EST_SKEL_GAP)
                                continue;
                            if (i > k0)
                            {
                                int size = i - k0;
                                bursts++;
                                sizes.Append(size).Append(' ');
                                if (size < 2 || size > 3) badBurst++;
                                for (int j = k0; j < i; j++)
                                {
                                    var b = bs[j];
                                    if (b.fpath.Count < 3) continue;
                                    int idx = j - k0;
                                    int vx = b.fpath[1].x - b.fpath[0].x, vy1 = b.fpath[1].y - b.fpath[0].y;
                                    if (Math.Abs(vx) != EST_SKEL_BONE_VX + idx * EST_SKEL_BONE_VX_STEP) badPath++;
                                    if (vy1 != EST_SKEL_BONE_VY - idx * EST_SKEL_BONE_VY_STEP + 32) badPath++;   // vy đã cộng g 0.125 trước khi dời
                                }
                            }
                            k0 = i;
                        }
                        Expect("mỗi loạt 2–3 xương (wiki) — số loạt sai", 0, badBurst);
                        Expect("xương k: vx 1 + 0.375k, vy0 −3.5 − 0.5k (đường khác nhau) — số sai", 0, badPath);
                        int inBand = 0, n = 0;
                        for (int k = 120; k < t.path.Count; k++, n++)
                        {
                            int rel = t.spawnRel + k;
                            int d = Math.Abs(t.path[k].x - recs[rel].sx);
                            if (d >= 16 && d <= 160) inBand++;
                        }
                        ExpectRange("bám quanh Simon: khoảng cách 16–160 px (% tick sau 120 — dải chọn 48–96, cú nhảy kéo ra ngoài)", 85, 100, n > 0 ? inBand * 100 / n : 0);
                        ExpectRange("có nhảy (wiki 'nhảy nhiều')", 1, 999, sk != null ? sk.jumps : 0);
                        Expect("quay mặt về Simon (tick sai, bỏ tick trùng x)", 0, CountWhere(t, k => recs[t.spawnRel + k].sx != t.path[k].x && t.facing[k] != (recs[t.spawnRel + k].sx > t.path[k].x ? 1 : -1)));
                        int maxV = 0;
                        for (int k = 1; k < t.fpath.Count; k++) maxV = Math.Max(maxV, Math.Abs(t.fpath[k].x - t.fpath[k - 1].x));
                        ExpectRange("ngang ≤ 1 px/tick (fixed)", 1, 256, maxV);
                        int minY = 999; foreach (var v in t.path) minY = Math.Min(minY, v.y);
                        measures.Add("loạt: " + sizes + "; số xương " + bs.Count + "; nhảy " + (sk != null ? sk.jumps : 0) + "; y cao nhất " + minY);
                    };
                }
                break;
            case "skel_bone_hit":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 7;
                    Go("Arena", 200, 176, 1);
                    Put(skel, 290, 176, -1);
                    finish = () =>
                    {
                        ExpectRange("Simon trúng ≥ 1 đòn", 1, 99, playerHits.Count);
                        ExpectRange("có đòn do xương (EnemyAttack SkeletonBone)", 1, 99, CountPlayerHits("EnemyAttack:SkeletonBone"));
                        Expect("máu mất = 3 × số đòn (Stage 7)", WIKI_DMG_B3 * playerHits.Count, GameSession.MAX_HP - Last.hp);
                        measures.Add("đòn: " + string.Join(", ", playerHits.ToArray()));
                    };
                }
                break;
            case "skel_kill":
                {
                    ses.subWeapon = eSubWeapon.Dagger;
                    Go("Arena", 200, 176, 1);
                    Put(skel, 280, 176, -1);
                    finish = () => KillChecks(eEnemyId.Skeleton, 1, WIKI_SCORE_SKEL, false);
                }
                break;
            case "flea_hops":
                {
                    Go("Arena", 150, 176, 1);
                    Put(flea, 300, 176, -1);
                    Photo(60, 70, 90, 120, 160);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Fleaman);
                        if (t == null) return;
                        int kT = -1;
                        for (int k = 0; k < t.states.Count && kT < 0; k++) if (t.states[k] != "Wait") kT = k;
                        int simX = kT >= 0 ? recs[t.spawnRel + kT].sx : -1;
                        Expect("bật khi |Simon − Fleaman| < 112: Simon X = 300 − 111", 300 - (EST_FLEA_TRIGGER - 1), simX);
                        // tách từng cú nhảy
                        var kinds = new StringBuilder();
                        int badApex = 0, badPause = 0, hops = 0, badSeq = 0, kStart = -1, lastLand = -1, crossed = 0;
                        int shortApex = Apex(EST_FLEA_SHORT_VY, EST_FLEA_G), longApex = Apex(EST_FLEA_LONG_VY, EST_FLEA_G);
                        for (int k = 1; k < t.states.Count; k++)
                        {
                            bool air = t.states[k].StartsWith("Hop"), wasAir = t.states[k - 1].StartsWith("Hop");
                            if (air && !wasAir)
                            {
                                kStart = k;
                                if (lastLand >= 0)
                                {
                                    int pause = k - lastLand;
                                    if (pause < EST_FLEA_PAUSE_MIN || pause > EST_FLEA_PAUSE_MAX) badPause++;
                                }
                            }
                            if (!air && wasAir && kStart >= 0)
                            {
                                lastLand = k;
                                bool big = t.states[kStart] == "HopL";
                                int y0 = t.path[kStart - 1].y, minY = 999;
                                for (int j = kStart; j < k; j++) minY = Math.Min(minY, t.path[j].y);
                                int ap = y0 - minY;
                                if (ap != (big ? longApex : shortApex)) badApex++;
                                if (big != (hops % 3 == 2)) badSeq++;
                                int sx0 = recs[t.spawnRel + kStart].sx;
                                if (big && Math.Sign(t.path[kStart - 1].x - sx0) != Math.Sign(t.path[k].x - sx0)) crossed++;
                                kinds.Append(big ? "L" : "S").Append(ap).Append(' ');
                                hops++;
                            }
                        }
                        ExpectRange("số cú nhảy", 6, 999, hops);
                        Expect("chu kỳ ngắn-ngắn-vọt (cú sai)", 0, badSeq);
                        Expect("đỉnh ngắn " + shortApex + " / vọt " + longApex + " px (cú sai)", 0, badApex);
                        Expect("nghỉ 4–16 tick giữa 2 cú (lần sai)", 0, badPause);
                        ExpectRange("nhảy vọt qua đầu Simon (lần)", 1, 999, crossed);
                        measures.Add("cú nhảy: " + kinds);
                    };
                }
                break;
            case "flea_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 7;
                    Go("Arena", 200, 176, 1);
                    Put(flea, 250, 176, -1);
                    finish = () => Expect("Fleaman chạm Simon: −3 vạch (Stage 7)", GameSession.MAX_HP - WIKI_DMG_B3, Last.hp);
                }
                break;
            case "flea_kill":
                {
                    Go("Arena", 200, 176, 1);
                    Put(flea, 250, 176, -1);
                    Photo(10, 16);
                    finish = () => KillChecks(eEnemyId.Fleaman, 1, WIKI_SCORE_FLEA, false);
                }
                break;
            case "bat_black":
                {
                    Go("Arena", 150, 176, 1);
                    var b = Put(bat, 300, 96, -1) as Bat;
                    b.ForcePerch();
                    Photo(60, 100, 130, 160, 220);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Bat);
                        if (t == null) return;
                        int kS = t.states.IndexOf("Swoop"), kF = t.states.IndexOf("Fly");
                        int simX = kS >= 0 ? recs[t.spawnRel + kS].sx : -1;
                        Expect("rời chỗ treo khi |dx| < 64: Simon X = 300 − 63", 300 - (EST_BAT_PERCH - 1), simX);
                        Expect("treo yên trước đó (tick đổi vị trí)", 0, CountWhere(t, k => k < kS && t.path[k] != t.path[0]));
                        int bad = 0;
                        for (int k = kS + 1; k < kF; k++)
                            if (t.fpath[k].y - t.fpath[k - 1].y != EST_BAT_SWOOP_VY || t.fpath[k].x - t.fpath[k - 1].x != -288) bad++;
                        Expect("sà xuống 1.5 px/tick + ngang 1.125 (tick sai)", 0, bad);
                        Expect("chuyển bay sóng ở tick đầu tâm ≥ chân Simon − 14", 1, kF > 1 && t.path[kF].y >= 176 - EST_BAT_SWOOP_ABOVE && t.path[kF - 1].y < 176 - EST_BAT_SWOOP_ABOVE ? 1 : 0);
                        measures.Add("treo tới tick " + kS + ", sà tới tick " + kF + " (y " + (kF > 0 ? t.path[kF - 1].y : -1) + ")");
                    };
                }
                break;
            case "freeze_b23":
                {
                    ses.subWeapon = eSubWeapon.Stopwatch;
                    ses.hearts = 10;
                    Go("Arena", 200, 176, 1);
                    Put(knight, 300, 176, -1);
                    Put(tower, 380, 176, -1);
                    Put(skel, 120, 176, 1);
                    Put(ghost, 260, 90, -1);
                    Put(flea, 160, 176, 1);
                    var sp = AddSpawner<EdgeSpawner>(medusa);
                    sp.side = eSpawnSide.FrontOfPlayer;
                    sp.playerRelativeY = true;
                    Photo(100, 200);
                    finish = () =>
                    {
                        int f0 = -1, f1 = -1;
                        for (int i = 0; i < recs.Count; i++)
                        {
                            if (f0 < 0 && recs[i].frozenE) f0 = i;
                            if (f0 >= 0 && f1 < 0 && !recs[i].frozenE) f1 = i;
                        }
                        Expect("Stopwatch đóng băng (tick) — wiki 5 s", WIKI_STOPWATCH, f1 - f0);
                        int moved = 0, spawned = 0, shotMoved = 0, shotSpawned = 0, movedAfter = 0;
                        foreach (var t in tracks)
                        {
                            for (int k = 1; k < t.fpath.Count; k++)
                            {
                                int rel = t.spawnRel + k;
                                if (rel > f0 && rel < f1 && t.fpath[k] != t.fpath[k - 1]) moved++;
                                if (rel > f1 && t.fpath[k] != t.fpath[k - 1]) movedAfter++;
                            }
                            if (t.spawnRel > f0 && t.spawnRel < f1) spawned++;
                        }
                        foreach (var b in shotsT)
                        {
                            for (int k = 1; k < b.fpath.Count; k++)
                            {
                                int rel = b.spawnRel + k;
                                if (rel > f0 && rel < f1 && b.fpath[k] != b.fpath[k - 1]) shotMoved++;
                            }
                            if (b.spawnRel > f0 && b.spawnRel < f1) shotSpawned++;
                        }
                        Expect("enemy đứng yên khi đóng băng (tick dời)", 0, moved);
                        Expect("đạn đứng yên khi đóng băng (tick dời)", 0, shotMoved);
                        Expect("không sinh enemy / đạn mới khi đóng băng", 0, spawned + shotSpawned);
                        ExpectRange("chạy lại sau khi hết đóng băng (tick dời)", 1, 99999, movedAfter);
                        ExpectRange("có đạn trên màn lúc bấm đồng hồ (cầu lửa/xương)", 1, 99, recs[f0].shots);
                    };
                }
                break;
            case "freeze_whip":
                {
                    ses.subWeapon = eSubWeapon.Stopwatch;
                    ses.hearts = 10;
                    Go("Arena", 200, 176, 1);
                    var k = Put(knight, 234, 176, -1) as SpearKnight;
                    k.legTimer = 9999;
                    finish = () =>
                    {
                        KillChecks(eEnemyId.SpearKnight, WIKI_KNIGHT_W, WIKI_SCORE_KNIGHT, true);
                        var t = First(eEnemyId.SpearKnight);
                        Expect("bị hạ khi vẫn đang đóng băng", 1, t != null && t.endRel >= 0 && recs[t.endRel].frozenE ? 1 : 0);
                    };
                }
                break;
            case "rosary_b23":
                {
                    Go("Arena", 200, 176, 1);
                    Put(knight, 300, 176, -1);
                    Put(tower, 330, 176, -1);
                    Put(skel, 100, 176, 1);
                    Put(ghost, 250, 80, -1);
                    Put(raven, 240, 120, -1);
                    var sp = AddSpawner<EdgeSpawner>(medusa);
                    sp.side = eSpawnSide.FrontOfPlayer;
                    sp.playerRelativeY = true;
                    int killed = -1, before = -1, shotsBefore = -1, after = -1;
                    onTick = rel =>
                    {
                        if (rel == 150) { before = OnScreen(eEntityGroup.Enemy); shotsBefore = OnScreen(eEntityGroup.EnemyShot); killed = world.RosaryClear(); }
                        if (rel == 151) after = OnScreen(eEntityGroup.Enemy, startTick + 150) + OnScreen(eEntityGroup.EnemyShot, startTick + 150);
                    };
                    Photo(149, 152);
                    finish = () =>
                    {
                        ExpectRange("enemy trên màn trước Rosary", 4, 99, before);
                        ExpectRange("có đạn trên màn trước Rosary", 1, 99, shotsBefore);
                        Expect("Rosary gỡ hết enemy + đạn trên màn", before + shotsBefore, killed);
                        Expect("enemy + đạn trên màn (đã có lúc Rosary) còn lại tick sau", 0, after);
                        Expect("Rosary 0 điểm", 0, recs[151].score);
                    };
                }
                break;
            default:
                Debug.LogError("[EnemyB23Probe] không có kịch bản " + scenario);
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
        playerHits.Clear();
        startTick = world.TickCount;
        foreach (var e in world.GroupList(eEntityGroup.Enemy))
            TrackOf(e as EnemyBase, 0);
        world.TickEnded += OnTickEnded;
        world.Hit += OnHit;
        EnemyBase.Defeated += OnDefeated;
        EnemySpawner.Spawned += OnSpawned;
        Running = true;
    }

    private static void Unhook()
    {
        if (world != null)
        {
            world.TickEnded -= OnTickEnded;
            world.Hit -= OnHit;
        }
        EnemyBase.Defeated -= OnDefeated;
        EnemySpawner.Spawned -= OnSpawned;
        Running = false;
    }

    private static void OnHit(HitInfo h)
    {
        int rel = world.TickCount - startTick - 1;
        var en = h.victim as EnemyBase;
        if (en != null)
        {
            var t = TrackOf(en, rel);
            if (t != null) t.hits++;
        }
        if (h.victim == world.Player)
            playerHits.Add(h.hitbox.layer + ":" + (h.attacker != null ? h.attacker.GetType().Name : "?") + "@" + rel + " −" + h.damage);
        var b = ShotOf(h.victim, rel);
        if (b != null && h.victim is BoneTowerFireball)
            b.broken = ((BoneTowerFireball)h.victim).broken;
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

    private static Shot ShotOf(Entity e, int rel)
    {
        if (e == null || e.Group != eEntityGroup.EnemyShot)
            return null;
        foreach (var b in shotsT)
            if (b.e == e && b.spawnTick == e.SpawnTick)
                return b;
        var n = new Shot { e = e, spawnTick = e.SpawnTick, spawnRel = rel, type = e.GetType().Name };
        shotsT.Add(n);
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
            t.fpath.Add(new Vector2Int(en.fx, en.fy));
            t.states.Add(en.DebugState.Split(' ')[0]);
            t.anims.Add(en.Anim != null && en.Anim.Current != null ? en.Anim.Current.name : "");
            t.facing.Add(en.Facing);
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
            if (e.Alive)
            {
                shotsN++;
                var b = ShotOf(e, rel);
                b.fpath.Add(new Vector2Int(e.fx, e.fy));
                Mix(e.fx); Mix(e.fy);
            }
        foreach (var b in shotsT)
            if (b.endRel < 0 && (!b.e.Alive || b.e.SpawnTick != b.spawnTick))
            {
                b.endRel = rel;
                var bf = b.e as BoneTowerFireball;
                if (bf != null && bf.broken) b.broken = true;
            }
        var r = new Rec
        {
            sx = s.X, sy = s.Y, hp = ses.playerHp, score = ses.score, state = s.Fsm.CurState, cx = w.CameraCtl != null ? w.CameraCtl.X : 0,
            enemies = enemies, fx = fx, frozenE = w.IsEnemyFrozen, shots = shotsN,
        };
        recs.Add(r);
        Mix(rel); Mix(s.fx); Mix(s.fy); Mix((int)r.state); Mix(r.hp); Mix(r.score); Mix(enemies); Mix(fx); Mix(shotsN); Mix(w.Rng.State);
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
        shotsT.Clear();
        if (world != null)
        {
            world.ClearNonPlayer();
            if (savedStage > 0)
                world.Stage = savedStage;
        }
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

    /// <summary>Vết enemy + đạn kịch bản vừa chạy: "kind,id,spawnRel,k,rel,x,y,state" (px).</summary>
    public static string TraceCsv()
    {
        var sb = new StringBuilder("kind,id,spawnRel,k,rel,x,y,state,simonX\n");
        foreach (var t in tracks)
            for (int k = 0; k < t.path.Count; k++)
            {
                int rel = t.spawnRel + k;
                sb.Append("enemy,").Append(t.id).Append(',').Append(t.spawnRel).Append(',').Append(k).Append(',').Append(rel).Append(',')
                  .Append(t.path[k].x).Append(',').Append(t.path[k].y).Append(',').Append(t.states[k]).Append(',')
                  .Append(rel >= 0 && rel < recs.Count ? recs[rel].sx : 0).Append('\n');
            }
        foreach (var b in shotsT)
            for (int k = 0; k < b.fpath.Count; k++)
            {
                int rel = b.spawnRel + k;
                sb.Append("shot,").Append(b.type).Append(',').Append(b.spawnRel).Append(',').Append(k).Append(',').Append(rel).Append(',')
                  .Append(Fixed.ToPx(b.fpath[k].x)).Append(',').Append(Fixed.ToPx(b.fpath[k].y)).Append(",-,")
                  .Append(rel >= 0 && rel < recs.Count ? recs[rel].sx : 0).Append('\n');
            }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- chấm

    static Rec Last => recs[recs.Count - 1];

    static List<Track> Tracks(eEnemyId id)
    {
        var l = new List<Track>();
        foreach (var t in tracks)
            if (t.id == id)
                l.Add(t);
        l.Sort((a, b) => a.spawnRel.CompareTo(b.spawnRel));
        return l;
    }

    static Track First(eEnemyId id)
    {
        var l = Tracks(id);
        if (l.Count == 0)
        {
            Expect("có " + id, 1, 0);
            return null;
        }
        return l[0];
    }

    static List<Shot> ShotsOf(string type)
    {
        var l = new List<Shot>();
        foreach (var b in shotsT)
            if (b.type == type)
                l.Add(b);
        l.Sort((a, b) => a.spawnRel.CompareTo(b.spawnRel));
        return l;
    }

    static int CountWhere(Track t, Func<int, bool> f)
    {
        int n = 0;
        for (int k = 0; k < t.path.Count; k++)
            if (f(k)) n++;
        return n;
    }

    static int CountRecs(Func<Rec, bool> f)
    {
        int n = 0;
        foreach (var r in recs) if (f(r)) n++;
        return n;
    }

    /// <summary>Số thực thể sống nhóm g có X trong khung camera [cx, cx + 256) (như StageWorld.RosaryClear).</summary>
    static int OnScreen(eEntityGroup g, int maxSpawnTick = int.MaxValue)
    {
        int cx = world.CameraCtl != null ? world.CameraCtl.X : 0, n = 0;
        foreach (var e in world.GroupList(g))
            if (e.Alive && e.SpawnTick <= maxSpawnTick && e.X >= cx && e.X < cx + NesCoord.SCREEN_W) n++;
        return n;
    }

    static int CountPlayerHits(string layer)
    {
        int n = 0;
        foreach (var h in playerHits) if (h.StartsWith(layer)) n++;
        return n;
    }

    /// <summary>Độ cao đỉnh (px) của cú nhảy vy0/g fixed cập nhật kiểu "vy += g rồi dời" (như StepFall các enemy).</summary>
    static int Apex(int vy0, int g)
    {
        int vy = vy0, y = 0, min = 0;
        for (int i = 0; i < 200 && (vy < 0 || i == 0); i++)
        {
            vy += g;
            y += vy;
            min = Math.Min(min, y);
        }
        return -(int)Math.Floor(min / 256.0);   // y px = floor(fy/256), xuất phát ở px nguyên
    }

    /// <summary>Tuần tra: mỗi đoạn đi thẳng (giữa 2 lần quay) có tốc độ TB = v (±2 %) nếu dài ≥ 20 tick; đoạn ≤ legMax + 1 tick; ≥ minTurns lần quay.</summary>
    static void LegChecks(Track t, int v, int legMax, int minTurns)
    {
        int turns = 0, badV = 0, badLeg = 0, k0 = 0;
        var sb = new StringBuilder("đoạn (tick): ");
        for (int k = 1; k <= t.facing.Count; k++)
        {
            if (k < t.facing.Count && t.facing[k] == t.facing[k - 1])
                continue;
            int len = k - k0;
            sb.Append(len).Append(' ');
            if (k < t.facing.Count)
            {
                turns++;
                if (len > legMax + 1) badLeg++;
            }
            if (len >= 20)
            {
                int d = Math.Abs(t.fpath[k - 1].x - t.fpath[k0].x), want = v * (len - 1);
                if (Math.Abs(d - want) > want / 50 + 256) badV++;
            }
            k0 = k;
        }
        measures.Add(sb.ToString());
        ExpectRange("số lần quay đầu", minTurns, 999, turns);
        Expect("đoạn tuần tra ≤ 192 tick (đoạn sai)", 0, badLeg);
        Expect("tốc độ 0.5 px/tick (đoạn sai)", 0, badV);
    }

    /// <summary>Hạ bằng đúng `hits` đòn, đúng điểm wiki; nhiều đòn ⇒ có tia lửa ở đòn chưa hạ.</summary>
    static void KillChecks(eEnemyId id, int hits, int score, bool spark)
    {
        var t = First(id);
        if (t == null) return;
        Expect(id + " bị hạ", 1, t.defeated ? 1 : 0);
        Expect(id + " số đòn tới khi hạ (wiki)", hits, t.hits);
        int proj = 0;
        foreach (var b in shotsT) if (b.broken) proj++;
        Expect("điểm (wiki " + score + (proj > 0 ? " + " + proj + " cầu lửa phá" : "") + ")", score + proj * WIKI_PROJ_SCORE, Last.score);
        if (spark && t.endRel > 0)
        {
            int sp = 0;
            for (int i = 0; i < t.endRel && i < recs.Count; i++) if (recs[i].fx > 0) sp++;
            ExpectRange("tia lửa ở đòn chưa hạ (tick có FX trước khi chết)", 1, 9999, sp);
        }
        measures.Add(id + ": " + t.hits + " đòn, hạ ở tick " + t.endRel);
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
