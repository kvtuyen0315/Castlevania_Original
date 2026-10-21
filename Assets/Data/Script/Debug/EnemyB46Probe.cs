#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness enemy Block 4–6 (CVGP 18; routine 21 chạy lại) — CHỈ Editor, scene Arena (Player = Simon). Khuôn EnemyB46Probe (17). Qua MCP khi Play:
///   EnemyB46Probe.SaveReplays()                          ⇒ Assets/Data/Tests/Replays/enemy_b46_&lt;tên&gt;.txt
///   EnemyB46Probe.RunSuite(EnemyB46Probe.ScenarioNames()) ⇒ chạy (input ĐỌC TỪ FILE), ghi từng tick, TỰ CHẤM ⇒ SuiteReport()
///   EnemyB46Probe.ShotDir = thư mục ⇒ kịch bản chụp Game view ở các tick cố định (≥ 4 ảnh/enemy)
/// Không có video Block 4–6 ⇒ kỳ vọng = hằng WIKI_* (số đòn, điểm, sát thương theo stage) + EST_* (giá trị ƯỚC LƯỢNG routine 18 chép
/// độc lập Config — đổi Config mà quên đổi đây thì bộ kiểm báo). Geometry Arena giữ nguyên (sàn y 176 x 0–415, tường thấp 416–431 đỉnh
/// y 144) — không đụng bộ kiểm 07–17.
/// </summary>
public static class EnemyB46Probe
{
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";

    // ---------------------------------------------------------------- kỳ vọng
    public const int WIKI_SCORE_EAGLE = 300, WIKI_SCORE_FLEA = 500, WIKI_SCORE_DRAGON = 1000, WIKI_SCORE_RED = 400, WIKI_SCORE_AXE = 1400, WIKI_PROJ_SCORE = 100;
    public const int EST_SCORE_PBAT = 200;
    public const int WIKI_DRAGON_W = 8, WIKI_DRAGON_S = 7, WIKI_AXE_W = 10, WIKI_AXE_S = 9, EST_PBAT_W = 3, EST_PBAT_S = 2;
    public const int WIKI_DMG_B4 = 3, WIKI_DMG_B5 = 4, WIKI_DMG_B6 = 4;   // wiki Gameplay#Damage: Stage 10–12 = 3, 13–18 = 4
    public const int WIKI_DROP_MIN = 5, WIKI_DROP_MAX = 8;
    public const int EST_EAGLE_VX = 384, EST_EAGLE_ABOVE = 96, EST_EAGLE_CARRY = 28, EST_EAGLE_DROP = 28, EST_EAGLE_FIRST = 32;
    public const int EST_EAGLE_GAP_MIN = 96, EST_EAGLE_GAP_MAX = 160;
    public const int EST_DRAGON_HEAD_DX = 6 * 8 + 10, EST_DRAGON_AMP = 24, EST_DRAGON_P = 128, EST_DRAGON_FIRST = 64, EST_DRAGON_WARN = 16;
    public const int EST_DRAGON_BURST_GAP = 20, EST_DRAGON_BALL_V = 384, EST_DRAGON_AIM = 16;
    public const int EST_RED_VX = 128, EST_RED_COLLAPSE = 8, EST_RED_REVIVE = 90, EST_RED_RISE = 8;
    public const int EST_AXE_VX = 128, EST_AXE_KEEP_MIN = 64, EST_AXE_KEEP_MAX = 112, EST_AXE_FIRST = 48, EST_AXE_POSE = 16, EST_AXE_THROW_MIN = 96;
    public const int EST_AXE_V = 512, EST_AXE_ACC = 6, EST_AXE_HIGH = 20, EST_AXE_LOW = 10;
    public const int EST_PBAT_WAKE = 96, EST_PBAT_HOVER = 16, EST_PBAT_SWOOP_ABOVE = 24, EST_PBAT_FIRE = 16, EST_PBAT_BALL_V = 384;

    private struct Scn
    {
        public string name, rle, note;
        public Scn(string n, string r, string note) { name = n; rle = r; this.note = note; }
    }

    const string WHIP5 = "B -*23 B -*23 B -*23 B -*23 B -*23";
    const string WHIP10 = WHIP5 + " " + WHIP5;
    const string DWHIP5 = "D*4 D+B D*23 D+B D*23 D+B D*23 D+B D*23 D+B D*23";

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("eagle_drop", "-*420", "Simon (150,176) quay phải đứng; EdgeSpawner Eagle trước mặt: chim (cx+248, chân − 96) bay −1.5 px/tick mang Fleaman dưới chân +28; |dx| ≤ 28 ⇒ thả, Fleaman rơi thẳng rồi nhảy; chim bay ra khỏi màn"),
        new Scn("eagle_whip", "-*1 " + WHIP5 + " -*40", "roi cấp 3 hạ Eagle đang mang Fleaman: cả hai chết (wiki), +300 +500"),
        new Scn("eagle_touch", "-*80", "Stage 11: Eagle/Fleaman bay thấp chạm Simon: −3 vạch"),
        new Scn("dragon_wave", "-*400", "Bone Dragon chân cổ (300,136) mặt trái, Simon (150,176): thân bất động, đầu ±24 chu kỳ 128; nghỉ 64 ⇒ há miệng 16 ⇒ 2 cầu lửa cách 20 nhắm tâm thân Simon 1.5 px/tick"),
        new Scn("dragon_fire_hit", "-*200", "Stage 11: cầu lửa Bone Dragon trúng Simon: −3 vạch (EnemyAttack)"),
        new Scn("dragon_whip_ball", "-*80 " + WHIP10, "roi xích quất vỡ cầu lửa Bone Dragon (wiki): +100/quả (suy luận)"),
        new Scn("dragon_kill_l1", "-*57 Sub -*30 " + WHIP10 + " -*60", "rồng (300,136): Stopwatch đúng lúc đầu hạ thấp (~tick 74) + roi da: đúng 8 đòn, +1000, rơi 5–8 túi tiền/tim nhỏ xuống sàn"),
        new Scn("dragon_kill_l2", "-*57 Sub -*30 " + WHIP10 + " -*60", "Stopwatch đúng lúc đầu thấp + roi xích: đúng 7 đòn, +1000, rơi 5–8 vật"),
        new Scn("dragon_holy", "-*2 Sub -*150", "nước thánh đốt đầu rồng rồi hạ (Defeat ở tick 100): rơi thêm Double Shot (wiki)"),
        new Scn("red_walk", "-*500", "Red Skeleton (300,176), Simon (100,176): đi 0.5 px/tick về phía Simon, không rời sàn"),
        new Scn("red_collapse", WHIP10 + " " + WHIP5, "roi da: Red Skeleton đổ 8 ⇒ đống xương 90 ⇒ ráp 8 ⇒ đi tiếp (không chết), +400 mỗi lần đổ"),
        new Scn("red_touch", "-*120", "Stage 13: Red Skeleton chạm Simon: −4 vạch"),
        new Scn("axe_keep", "-*700", "Axe Knight (250,176), Simon (130,176) đứng: tiến tới dải 64–112 rồi đứng; ném rìu (tick 48, rồi 96–160 + tư thế 16) cao/thấp bay −2 px/tick giảm 6/256 rồi quay về"),
        new Scn("axe_push", "R*260", "camera khoá cx 100; Simon tiến: Axe Knight lùi, bị đẩy khỏi mép phải màn ⇒ gỡ, không quay lại (wiki)"),
        new Scn("axe_whip_high", "-*56 " + WHIP10, "Stage 14: rìu CAO bị roi đứng quất vỡ (+100), Simon không mất máu"),
        new Scn("axe_whip_low", "-*56 " + DWHIP5 + " " + DWHIP5, "Stage 14: rìu THẤP bị roi NGỒI quất vỡ (+100), Simon không mất máu"),
        new Scn("axe_kill_l1", WHIP10 + " " + WHIP5 + " -*30", "roi da hạ Axe Knight (đứng yên): đúng 10 đòn, +1400"),
        new Scn("axe_kill_l2", WHIP10 + " " + WHIP5 + " -*30", "roi xích: đúng 9 đòn"),
        new Scn("axe_hit", "-*200", "Stage 14: rìu trúng Simon: −4 vạch (EnemyAttack)"),
        new Scn("pbat_wake", "R*30 -*570", "Phantom Bat thường ngủ (260,96); Simon đi phải từ 150: thức khi |dx| < 96, lượn 16, bổ nhào đáy ≤ chân − 24, lơ lửng 16 ⇒ phun cầu lửa; không thanh ENEMY, không quả cầu"),
        new Scn("pbat_kill_l1", "-*1 " + WHIP5 + " -*40", "roi da hạ Phantom Bat thường (khựng 32 mỗi đòn giữ chân): 3 đòn, +200 (ước lượng), không quả cầu, ENEMY không đổi"),
        new Scn("pbat_kill_l2", "-*1 " + WHIP5 + " -*40", "roi xích: 2 đòn"),
        new Scn("pbat_touch", "-*200", "Stage 16: Phantom Bat thường bổ nhào trúng Simon: −4 vạch"),
        new Scn("freeze_b46", "-*60 Sub -*400", "Stopwatch: Eagle + Fleaman, Bone Dragon, Red Skeleton, Axe Knight, Phantom Bat thường + đạn dừng 186 tick rồi chạy tiếp"),
        new Scn("freeze_whip_axe", "-*2 Sub -*30 " + WHIP5 + " Sub -*23 " + WHIP5 + " -*40", "Stopwatch + roi da: Axe Knight đứng yên vẫn ăn đủ 10 đòn (bộ đếm bất tử chạy khi đóng băng); CVM3 03: đồng hồ 186 tick < 10 roi ⇒ dùng lại đồng hồ giữa chừng (kéo dài — video Map_2 f6928)"),
        new Scn("rosary_b46", "-*200", "Rosary (RosaryClear) tick 150: mọi enemy + đạn Block 4–6 trên màn biến mất (Phantom Bat thường không phải boss), 0 điểm"),
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
        public readonly List<int> frames = new List<int>();
    }

    public class ItemT
    {
        public Entity e;
        public int spawnTick, spawnRel;
        public eItemId id;
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
        public int sx, sy, hp, score, cx, enemies, fx, shots, ehp;
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
    private static readonly List<ItemT> itemsT = new List<ItemT>();
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

    public static string ReplayPath(string name) { return REPLAY_DIR + "/enemy_b46_" + name + ".txt"; }

    public static int SaveReplays()
    {
        Directory.CreateDirectory(REPLAY_DIR);
        foreach (var s in SCENARIOS)
        {
            var ticks = NesReplay.Decode(s.rle);
            var sb = new StringBuilder();
            sb.Append("# ").Append(ticks.Count).Append(" tick, hash ").Append(NesReplay.Hash(ticks).ToString("x8")).Append('\n');
            sb.Append("# CVGP 18 EnemyB46Probe kịch bản '").Append(s.name).Append("': ").Append(s.note).Append('\n');
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
        sb.Append("ENEMY B4-6 SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
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
        var go = new GameObject("EnemyB46Probe_" + typeof(T).Name);
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
            Debug.LogError("[EnemyB46Probe] cần scene Arena với Player = SimonController + StageWorld.itemCatalog");
            return -1;
        }
        string p = ReplayPath(scenario);
        if (!File.Exists(p))
        {
            Debug.LogError("[EnemyB46Probe] thiếu " + p + " — gọi EnemyB46Probe.SaveReplays()");
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
        var eagle = Prefab<Eagle>("Eagle");
        var dragon = Prefab<BoneDragon>("BoneDragon");
        var red = Prefab<RedSkeleton>("RedSkeleton");
        var axeK = Prefab<AxeKnight>("AxeKnight");
        var pbat = Prefab<PhantomBatMinor>("PhantomBatMinor");
        int ehp0 = ses.enemyHp;
        switch (scenario)
        {
            case "eagle_drop":
                {
                    Go("Arena", 150, 176, 1);
                    var sp = AddSpawner<EdgeSpawner>(eagle);
                    sp.side = eSpawnSide.FrontOfPlayer;
                    sp.playerRelativeY = true;
                    Photo(40, 80, 104, 112, 130, 160);
                    finish = () =>
                    {
                        var es = Tracks(eEnemyId.Eagle);
                        var fs = Tracks(eEnemyId.Fleaman);
                        ExpectRange("số chim sinh (≥ 2 lượt)", 2, 99, es.Count);
                        if (es.Count == 0) return;
                        int cx = recs[0].cx;
                        Expect("chim đầu (tick)", EST_EAGLE_FIRST - 1, es[0].spawnRel);
                        int badX = 0, badF = 0, badY = 0;
                        foreach (var t in es)
                        {
                            if (t.x0 != recs[Math.Max(0, t.spawnRel)].cx + NesCoord.SCREEN_W - 8) badX++;
                            if (t.facing0 != -1) badF++;
                            if (t.y0 != 176 - EST_EAGLE_ABOVE) badY++;
                        }
                        Expect("sinh mép trước mặt (cx + 248) — số lần sai", 0, badX);
                        Expect("bay vào màn (sang trái) — số lần sai", 0, badF);
                        Expect("tâm = chân Simon − 96 — số lần sai", 0, badY);
                        var e0 = es[0];
                        int badV = 0;
                        for (int k = 1; k < e0.fpath.Count; k++)
                            if (e0.fpath[k].x - e0.fpath[k - 1].x != -EST_EAGLE_VX || e0.fpath[k].y != e0.fpath[0].y) badV++;
                        Expect("bay ngang −1.5 px/tick, giữ độ cao (tick sai / " + (e0.fpath.Count - 1) + ")", 0, badV);
                        Expect("mỗi chim mang 1 Fleaman (số Fleaman sinh = số chim)", es.Count, fs.Count);
                        if (fs.Count == 0) return;
                        var f0 = fs[0];
                        Expect("Fleaman sinh cùng tick với chim", e0.spawnRel, f0.spawnRel);
                        int kRel = -1, badHold = 0;
                        for (int k = 0; k < f0.states.Count; k++)
                        {
                            int rel = f0.spawnRel + k, ke = rel - e0.spawnRel;
                            if (f0.states[k] != "Carried") { kRel = rel; break; }
                            if (ke < 0 || ke >= e0.path.Count || f0.path[k] != e0.path[ke] + new Vector2Int(0, EST_EAGLE_CARRY)) badHold++;
                        }
                        Expect("Fleaman treo dưới chân chim (+28) khi mang (tick sai)", 0, badHold);
                        ExpectRange("có thả", 1, 99999, kRel);
                        if (kRel < 0) return;
                        int ke2 = kRel - e0.spawnRel;
                        int dxDrop = ke2 >= 0 && ke2 < e0.path.Count ? e0.path[ke2].x - recs[kRel].sx : 999;
                        ExpectRange("thả khi |chim − Simon| ≤ 28 (lần đầu, đang bay tới 1.5 px/tick)", EST_EAGLE_DROP - 1, EST_EAGLE_DROP, Math.Abs(dxDrop));
                        int kf = kRel - f0.spawnRel, badFall = 0, kLand = -1;
                        for (int k = kf; k < f0.states.Count; k++)
                        {
                            if (f0.states[k] == "Ground") { kLand = k; break; }
                            if (f0.path[k].x != f0.path[kf].x) badFall++;
                        }
                        Expect("rơi thẳng đứng sau khi thả (tick x đổi)", 0, badFall);
                        ExpectRange("đáp sàn", 1, 99999, kLand);
                        if (kLand > 0)
                        {
                            Expect("đáp ở mặt sàn y 176", 176, f0.path[kLand].y);
                            ExpectRange("đáp trong tầm roi: |Fleaman − Simon| ≤ 28", 0, EST_EAGLE_DROP, Math.Abs(f0.path[kLand].x - recs[f0.spawnRel + kLand].sx));
                            measures.Add("thả ở tick " + kRel + " (chim cách Simon " + dxDrop + "), đáp tick " + (f0.spawnRel + kLand) + " x " + f0.path[kLand].x);
                        }
                        Expect("chim bay tiếp ra khỏi màn (gỡ Offscreen)", 1, e0.endRel > 0 && e0.reason == eKillReason.Offscreen ? 1 : 0);
                        int maxE = 0;
                        for (int rel = 0; rel < recs.Count; rel++)
                        {
                            int n = 0;
                            foreach (var t in es) if (t.spawnRel <= rel && (t.endRel < 0 || rel < t.endRel)) n++;
                            maxE = Math.Max(maxE, n);
                        }
                        ExpectRange("tối đa chim cùng lúc", 1, 2, maxE);
                        if (es.Count >= 2)
                            ExpectRange("khoảng sinh chim 2 (tick) = nghỉ 96–160 (hoặc chờ vì đủ 2)", EST_EAGLE_GAP_MIN, 9999, es[1].spawnRel - es[0].spawnRel);
                        var sb = new StringBuilder("chim sinh ở tick: ");
                        foreach (var t in es) sb.Append(t.spawnRel).Append(' ');
                        measures.Add(sb.ToString());
                    };
                }
                break;
            case "eagle_whip":
                {
                    ses.whipLevel = 3;
                    Go("Arena", 150, 176, 1);
                    Put(eagle, 230, 152, -1);
                    Photo(4, 14, 20);
                    finish = () =>
                    {
                        var e = First(eEnemyId.Eagle);
                        var f = First(eEnemyId.Fleaman);
                        if (e == null || f == null) { Expect("có chim + Fleaman", 1, 0); return; }
                        Expect("Eagle bị hạ (1 đòn)", 1, e.defeated && e.hits == 1 ? 1 : 0);
                        Expect("Fleaman đang treo chết CÙNG tick (wiki)", e.endRel, f.endRel);
                        Expect("Fleaman bị hạ (không phải bị gỡ)", 1, f.defeated ? 1 : 0);
                        Expect("Fleaman không trúng roi (chỉ chim)", 0, f.hits);
                        Expect("điểm = 300 + 500 (suy luận: cả hai bị giết)", WIKI_SCORE_EAGLE + WIKI_SCORE_FLEA, Last.score);
                    };
                }
                break;
            case "eagle_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 11;
                    Go("Arena", 150, 176, 1);
                    Put(eagle, 230, 160, -1);
                    finish = () => Expect("Eagle/Fleaman chạm Simon: −3 vạch (Stage 11)", GameSession.MAX_HP - WIKI_DMG_B4, Last.hp);
                }
                break;
            case "dragon_wave":
                {
                    Go("Arena", 150, 176, 1);
                    var d = Put(dragon, 300, 136, -1) as BoneDragon;
                    var heads = new List<Vector2Int>();
                    onTick = rel => heads.Add(d.Alive ? d.HeadPos : new Vector2Int(-1, -1));
                    Photo(30, 64, 72, 84, 104, 160);
                    finish = () =>
                    {
                        var t = First(eEnemyId.BoneDragon);
                        if (t == null) return;
                        Expect("chân cổ bất động (tick đổi)", 0, CountWhere(t, k => t.path[k] != new Vector2Int(300, 136)));
                        int badHx = 0, minY = 999, maxY = -999, badP = 0;
                        foreach (var h in heads)
                        {
                            if (h.x != 300 - EST_DRAGON_HEAD_DX) badHx++;
                            minY = Math.Min(minY, h.y); maxY = Math.Max(maxY, h.y);
                        }
                        for (int k = 0; k + EST_DRAGON_P < heads.Count; k++)
                            if (heads[k].y != heads[k + EST_DRAGON_P].y) badP++;
                        Expect("đầu ở x = chân cổ − 58 (tick sai)", 0, badHx);
                        Expect("đầu cao nhất = 136 − 24", 136 - EST_DRAGON_AMP, minY);
                        Expect("đầu thấp nhất = 136 + 24", 136 + EST_DRAGON_AMP, maxY);
                        Expect("ngọ nguậy tuần hoàn 128 tick (tick lệch)", 0, badP);
                        int kW = t.states.IndexOf("Warn"), kF = t.states.IndexOf("Fire");
                        Expect("nghỉ đầu (tick trước khi há miệng)", EST_DRAGON_FIRST, kW);
                        Expect("há miệng (tick)", EST_DRAGON_WARN, kF - kW);
                        int badMouth = 0;
                        for (int k = kW; k < kF; k++) if (t.frames[k] != 1) badMouth++;
                        Expect("khung head_1 (há) suốt lúc báo (tick sai)", 0, badMouth);
                        var balls = ShotsOf("BoneTowerFireball");
                        ExpectRange("số cầu lửa (2 lượt × 2)", 4, 99, balls.Count);
                        int badPair = 0, badV = 0, badAim = 0;
                        for (int i = 0; i + 1 < balls.Count; i += 2)
                            if (balls[i + 1].spawnRel - balls[i].spawnRel != EST_DRAGON_BURST_GAP) badPair++;
                        var sb = new StringBuilder("góc (độ): ");
                        foreach (var b in balls)
                        {
                            if (b.fpath.Count < 3) continue;
                            int vx = b.fpath[2].x - b.fpath[1].x, vy = b.fpath[2].y - b.fpath[1].y;
                            double v = Math.Sqrt((double)vx * vx + (double)vy * vy);
                            if (v < EST_DRAGON_BALL_V - 4 || v > EST_DRAGON_BALL_V + 2) badV++;   // vx, vy cắt nguyên + ISqrt làm tròn xuống
                            double ax = 150 * 256.0 - b.fpath[0].x, ay = (176 - EST_DRAGON_AIM) * 256.0 - (b.fpath[0].y - 4 * 256.0);
                            double cross = (vx * ay - vy * ax) / (v * Math.Sqrt(ax * ax + ay * ay));
                            if (Math.Abs(cross) > 0.03) badAim++;
                            sb.Append((Math.Atan2(-vy, vx) * 180 / Math.PI).ToString("0")).Append(' ');
                        }
                        Expect("2 quả/lượt cách 20 tick — số lượt sai", 0, badPair);
                        Expect("|v| = 1.5 px/tick (quả sai)", 0, badV);
                        Expect("nhắm tâm thân Simon lúc phun (quả lệch > 1.7°)", 0, badAim);
                        measures.Add(sb.ToString() + "; sinh tick " + string.Join(" ", balls.ConvertAll(b => b.spawnRel.ToString()).ToArray()) + "; đầu y " + minY + "–" + maxY);
                    };
                }
                break;
            case "dragon_fire_hit":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 11;
                    Go("Arena", 150, 176, 1);
                    Put(dragon, 300, 136, -1);
                    finish = () =>
                    {
                        Expect("cầu lửa trúng Simon: −3 vạch (Stage 11)", GameSession.MAX_HP - WIKI_DMG_B4, Last.hp);
                        Expect("đòn đầu do cầu lửa (EnemyAttack)", 1, playerHits.Count > 0 && playerHits[0].StartsWith("EnemyAttack") ? 1 : 0);
                    };
                }
                break;
            case "dragon_whip_ball":
                {
                    ses.whipLevel = 2;
                    Go("Arena", 150, 176, 1);
                    Put(dragon, 300, 136, -1);
                    Photo(100, 112, 124);
                    finish = () =>
                    {
                        var balls = ShotsOf("BoneTowerFireball");
                        int broken = 0;
                        foreach (var b in balls) if (b.broken) broken++;
                        ExpectRange("cầu lửa bị roi phá (wiki: quất được)", 1, 99, broken);
                        Expect("+100 / quả phá (suy luận)", broken * WIKI_PROJ_SCORE, Last.score);
                        Expect("rồng không trúng (ngoài tầm roi)", 0, First(eEnemyId.BoneDragon) != null ? First(eEnemyId.BoneDragon).hits : -1);
                        measures.Add("phá " + broken + "/" + balls.Count + " quả");
                    };
                }
                break;
            case "dragon_kill_l1":
            case "dragon_kill_l2":
                {
                    bool strong = scenario.EndsWith("l2");
                    ses.whipLevel = strong ? 2 : 1;
                    ses.subWeapon = eSubWeapon.Stopwatch;
                    ses.hearts = 10;
                    Go("Arena", 300 - EST_DRAGON_HEAD_DX - 24, 176, 1);
                    Put(dragon, 300, 136, -1);
                    Photo(80, 140, 300, 330);
                    finish = () =>
                    {
                        var t = First(eEnemyId.BoneDragon);
                        if (t == null) return;
                        Expect("Bone Dragon bị hạ", 1, t.defeated ? 1 : 0);
                        Expect("số đòn tới khi hạ (wiki " + (strong ? "7" : "8") + ")", strong ? WIKI_DRAGON_S : WIKI_DRAGON_W, t.hits);
                        Expect("điểm lúc hạ (wiki 1000)", WIKI_SCORE_DRAGON, t.endRel > 0 && t.endRel < recs.Count ? recs[t.endRel].score : -1);
                        int n = 0, bad = 0;
                        foreach (var it in itemsT)
                        {
                            n++;
                            if (it.id != eItemId.RedMoneyBag && it.id != eItemId.SmallHeart) bad++;
                        }
                        ExpectRange("rơi 5–8 vật (wiki)", WIKI_DROP_MIN, WIKI_DROP_MAX, n);
                        int onFloor = 0;
                        foreach (var it in itemsT) if (it.e.Alive && it.e.SpawnTick == it.spawnTick && it.e.Y == 176) onFloor++;
                        measures.Add("vật nằm trên sàn y 176 cuối kịch bản: " + onFloor + " (còn lại Simon nhặt)");
                        Expect("chỉ túi tiền / tim nhỏ (không trúng nước thánh)", 0, bad);
                        int sp = 0;
                        for (int i = 0; i < t.endRel && i < recs.Count; i++) if (recs[i].fx > 0) sp++;
                        ExpectRange("tia lửa ở đòn chưa hạ", 1, 9999, sp);
                        measures.Add("hạ ở tick " + t.endRel + "; vật rơi: " + ItemList());
                    };
                }
                break;
            case "dragon_holy":
                {
                    ses.subWeapon = eSubWeapon.HolyWater;
                    ses.hearts = 10;
                    Go("Arena", 300 - EST_DRAGON_HEAD_DX - 54, 176, 1);
                    var d = Put(dragon, 300, 160, -1) as BoneDragon;   // đầu dao động 136–184 ⇒ chạm lửa trên sàn (y 160–176)
                    bool holy = false;
                    onTick = rel =>
                    {
                        if (rel == 100 && d.Alive) { holy = d.holyHit; d.Defeat(); }
                    };
                    Photo(40, 60, 130);
                    finish = () =>
                    {
                        var t = First(eEnemyId.BoneDragon);
                        if (t == null) return;
                        ExpectRange("lửa nước thánh trúng đầu rồng (đòn)", 1, 99, t.hits);
                        Expect("rồng ghi nhận trúng nước thánh", 1, holy ? 1 : 0);
                        int multi = 0;
                        foreach (var it in itemsT) if (it.id == eItemId.DoubleShot) multi++;
                        Expect("rơi Double Shot (wiki: dùng Holy Water ⇒ shot multiplier)", 1, multi);
                        measures.Add("vật rơi: " + ItemList());
                    };
                }
                break;
            case "red_walk":
                {
                    Go("Arena", 100, 176, 1);
                    Put(red, 300, 176, -1);
                    Photo(30, 200, 380, 480);
                    finish = () =>
                    {
                        var t = First(eEnemyId.RedSkeleton);
                        if (t == null) return;
                        Expect("không rời sàn y 176 (tick sai)", 0, CountWhere(t, k => t.path[k].y != 176));
                        int badV = 0, n = 0;
                        for (int k = 1; k < t.fpath.Count && t.path[k].x > 100 + 16; k++, n++)
                            if (t.fpath[k].x - t.fpath[k - 1].x != -EST_RED_VX) badV++;
                        Expect("đi −0.5 px/tick về phía Simon (tick sai / " + n + ")", 0, badV);
                        ExpectRange("tới chỗ Simon rồi bám quanh (|dx| cuối ≤ 24)", 0, 24, Math.Abs(t.path[t.path.Count - 1].x - 100));
                        Expect("không đổ khi không bị đánh", 0, CountWhere(t, k => t.states[k] != "Walk"));
                    };
                }
                break;
            case "red_collapse":
                {
                    ses.whipLevel = 1;
                    Go("Arena", 150, 176, 1);
                    Put(red, 186, 176, -1);
                    Photo(14, 20, 60, 110, 118);
                    finish = () =>
                    {
                        var t = First(eEnemyId.RedSkeleton);
                        if (t == null) return;
                        var rs = t.e as RedSkeleton;
                        Expect("KHÔNG chết (còn sống cuối kịch bản)", -1, t.endRel);
                        Expect("không bắn sự kiện Defeated", 0, t.defeated ? 1 : 0);
                        int coll = 0;
                        for (int k = 0; k < t.states.Count; k++) if (t.states[k] == "Collapse" && (k == 0 || t.states[k - 1] != "Collapse")) coll++;
                        ExpectRange("số lần bị đánh đổ", 2, 99, coll);
                        Expect("đòn được nhận = số lần đổ (nằm thì không nhận)", coll, t.hits);
                        Expect("+400 mỗi lần đổ (suy luận: wiki cho điểm)", coll * WIKI_SCORE_RED, Last.score);
                        int kC = t.states.IndexOf("Collapse"), kP = t.states.IndexOf("Pile"), kR = t.states.IndexOf("Rise"), kW = kR >= 0 ? t.states.IndexOf("Walk", kR) : -1;
                        Expect("đổ (khung _1) tick", EST_RED_COLLAPSE, kP - kC);
                        Expect("nằm đống xương tick (wiki 1–2 s)", EST_RED_REVIVE, kR - kP);
                        Expect("ráp lại tick", EST_RED_RISE, kW - kR);
                        int badF = 0;
                        for (int k = kC; k < kW; k++) if (t.anims[k] != "collapse" || t.frames[k] != (t.states[k] == "Pile" ? 0 : 1)) badF++;
                        Expect("khung đổ/ráp _1, nằm _0 (tick sai)", 0, badF);
                        measures.Add("đổ " + coll + " lần; lần đầu ở tick " + kC + ", đi lại ở " + kW);
                    };
                }
                break;
            case "red_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 13;
                    Go("Arena", 150, 176, 1);
                    Put(red, 190, 176, -1);
                    finish = () => Expect("Red Skeleton chạm Simon: −4 vạch (Stage 13)", GameSession.MAX_HP - WIKI_DMG_B5, Last.hp);
                }
                break;
            case "axe_keep":
                {
                    Go("Arena", 130, 176, 1);
                    Put(axeK, 250, 176, -1);
                    Photo(47, 60, 90, 120, 300);
                    finish = () =>
                    {
                        var t = First(eEnemyId.AxeKnight);
                        if (t == null) return;
                        var ak = t.e as AxeKnight;
                        int badV = 0, kIn = -1, badBand = 0, badFace = 0;
                        for (int k = 1; k < t.fpath.Count; k++)
                            if (Math.Abs(t.fpath[k].x - t.fpath[k - 1].x) > EST_AXE_VX) badV++;
                        for (int k = 0; k < t.path.Count; k++)
                        {
                            int d = Math.Abs(t.path[k].x - 130);
                            if (kIn < 0 && d <= EST_AXE_KEEP_MAX) kIn = k;
                            if (kIn >= 0 && (d < EST_AXE_KEEP_MIN - 1 || d > EST_AXE_KEEP_MAX + 16)) badBand++;
                            if (t.facing[k] != -1) badFace++;
                        }
                        Expect("tốc ≤ 0.5 px/tick (tick vượt)", 0, badV);
                        ExpectRange("vào dải giữ khoảng cách", 0, 99999, kIn);
                        Expect("sau đó giữ khoảng 63–128 px với Simon (tick sai)", 0, badBand);
                        Expect("luôn quay mặt về Simon (tick sai)", 0, badFace);
                        int kT = t.states.IndexOf("Throw");
                        Expect("rìu đầu ở tick thứ 48 (chỉ số)", EST_AXE_FIRST - 1, kT);
                        var axes = ShotsOf("AxeKnightAxe");
                        ExpectRange("số rìu ném", 3, 99, axes.Count);
                        int badY = 0, badVx = 0, hi = 0, lo = 0, badReach = 0, badGap = 0;
                        var sb = new StringBuilder("rìu: ");
                        for (int i = 0; i < axes.Count; i++)
                        {
                            var a = axes[i];
                            int y0 = Fixed.ToPx(a.fpath[0].y);
                            if (y0 == 176 - EST_AXE_HIGH) hi++; else if (y0 == 176 - EST_AXE_LOW) lo++; else badY++;
                            int x0 = a.fpath[0].x, minX = x0;
                            for (int k = 1; k < a.fpath.Count; k++)
                            {
                                int want = Math.Min(EST_AXE_V, -EST_AXE_V + EST_AXE_ACC * k);
                                if (a.fpath[k].x - a.fpath[k - 1].x != want || a.fpath[k].y != a.fpath[0].y) badVx++;
                                minX = Math.Min(minX, a.fpath[k].x);
                            }
                            int reach = (x0 - minX) / 256;
                            if (a.endRel < 0 || a.endRel - a.spawnRel > 90) { if (reach < 82 || reach > 86) badReach++; }
                            if (i > 0 && a.spawnRel - axes[i - 1].spawnRel < EST_AXE_THROW_MIN + EST_AXE_POSE) badGap++;
                            sb.Append(y0 == 176 - EST_AXE_HIGH ? "H" : "L").Append("@").Append(a.spawnRel).Append(" xa ").Append(reach)
                              .Append(a.e is AxeKnightAxe && ((AxeKnightAxe)a.e).caught ? " bắt" : "").Append("; ");
                        }
                        Expect("độ cao cao (chân − 20) / thấp (chân − 10) — rìu sai", 0, badY);
                        Expect("vx = −2 + 6/256·k (boomerang), giữ độ cao (tick sai)", 0, badVx);
                        Expect("bay xa 82–86 px rồi quay về (rìu sai)", 0, badReach);
                        Expect("nghỉ giữa 2 lần ném ≥ 96 + 16 tick (lần sai)", 0, badGap);
                        measures.Add(sb.ToString() + " cao " + (ak != null ? ak.highThrows : -1) + " / thấp " + (ak != null ? ak.lowThrows : -1));
                    };
                }
                break;
            case "axe_push":
                {
                    Go("Arena", 200, 176, 1);
                    world.CameraCtl.Lock(100);
                    var k = Put(axeK, 290, 176, -1) as AxeKnight;
                    k.throwTimer = 99999;
                    Photo(20, 120, 180);
                    finish = () =>
                    {
                        var t = First(eEnemyId.AxeKnight);
                        if (t == null) return;
                        Expect("bị đẩy ra khỏi mép phải màn ⇒ gỡ (Offscreen)", 1, t.endRel > 0 && t.reason == eKillReason.Offscreen ? 1 : 0);
                        Expect("không phải bị hạ", 0, t.defeated ? 1 : 0);
                        Expect("chỉ 1 Axe Knight (không quay lại)", 1, Tracks(eEnemyId.AxeKnight).Count);
                        int back = 0;
                        for (int i = 1; i < t.fpath.Count; i++) if (t.fpath[i].x < t.fpath[i - 1].x) back++;
                        ExpectRange("lùi khi Simon tiến (tick đi về phía Simon)", 0, 40, back);
                        ExpectRange("x lúc gỡ > cx + 252", 100 + 252, 999, t.path.Count > 0 ? t.path[t.path.Count - 1].x : -1);
                        measures.Add("gỡ ở tick " + t.endRel + ", Simon x " + (t.endRel > 0 ? recs[t.endRel].sx : -1));
                    };
                }
                break;
            case "axe_whip_high":
            case "axe_whip_low":
                {
                    bool low = scenario.EndsWith("low");
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 14;
                    ses.whipLevel = 2;
                    Go("Arena", 200, 176, 1);
                    var k = Put(axeK, 290, 176, -1) as AxeKnight;
                    k.keepMin = 0;
                    k.keepMax = 9999;
                    if (low) k.axeHigh = k.axeLow; else k.axeLow = k.axeHigh;
                    Photo(60, 70, 80);
                    finish = () =>
                    {
                        var axes = ShotsOf("AxeKnightAxe");
                        int broken = 0;
                        foreach (var a in axes) if (a.broken) broken++;
                        ExpectRange("rìu " + (low ? "thấp bị roi NGỒI" : "cao bị roi ĐỨNG") + " quất vỡ", 1, 99, broken);
                        Expect("+100 / rìu phá (suy luận)", broken * WIKI_PROJ_SCORE, Last.score);
                        Expect("Simon không mất máu", GameSession.MAX_HP, Last.hp);
                        measures.Add("phá " + broken + "/" + axes.Count + " rìu");
                    };
                }
                break;
            case "axe_kill_l1":
            case "axe_kill_l2":
                {
                    bool strong = scenario.EndsWith("l2");
                    ses.whipLevel = strong ? 2 : 1;
                    Go("Arena", 200, 176, 1);
                    var k = Put(axeK, 230, 176, -1) as AxeKnight;
                    k.keepMin = 0;
                    k.keepMax = 9999;
                    k.throwTimer = 99999;
                    Photo(14, 40);
                    finish = () => KillChecks(eEnemyId.AxeKnight, strong ? WIKI_AXE_S : WIKI_AXE_W, WIKI_SCORE_AXE, true);
                }
                break;
            case "axe_hit":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 14;
                    Go("Arena", 150, 176, 1);
                    Put(axeK, 250, 176, -1);
                    finish = () =>
                    {
                        Expect("rìu trúng Simon: −4 vạch (Stage 14)", GameSession.MAX_HP - WIKI_DMG_B5, Last.hp);
                        Expect("đòn đầu do rìu (EnemyAttack)", 1, playerHits.Count > 0 && playerHits[0].StartsWith("EnemyAttack") ? 1 : 0);
                    };
                }
                break;
            case "pbat_wake":
                {
                    Go("Arena", 150, 176, 1);
                    Put(pbat, 260, 96, -1);
                    Photo(10, 20, 40, 60, 120, 200);
                    finish = () =>
                    {
                        var t = First(eEnemyId.PhantomBatMinor);
                        if (t == null) return;
                        int kW = t.states.IndexOf("Wake"), kS = t.states.IndexOf("Swoop"), kH = t.states.IndexOf("Hover");
                        int simX = kW >= 0 ? recs[t.spawnRel + kW].sx : -1;
                        Expect("ngủ tới khi |Simon − dơi| < 96: Simon X lúc thức = 260 − 95", 260 - (EST_PBAT_WAKE - 1), simX);
                        Expect("ngủ yên trước khi thức (tick đổi vị trí)", 0, CountWhere(t, k => k < kW && t.path[k] != t.path[0]));
                        Expect("vỗ cánh tại chỗ (tick) trước khi bổ nhào", EST_PBAT_HOVER, kS - kW);
                        int maxY = 0; foreach (var v in t.path) maxY = Math.Max(maxY, v.y);
                        ExpectRange("đáy bổ nhào ≤ chân Simon − 24", 0, 176 - EST_PBAT_SWOOP_ABOVE, maxY);
                        var balls = ShotsOf("BoneTowerFireball");
                        ExpectRange("phun cầu lửa", 1, 99, balls.Count);
                        if (balls.Count > 0 && kH >= 0)
                        {
                            Expect("quả đầu ở tick lơ lửng thứ 16 (chỉ số lơ lửng + 15)", t.spawnRel + kH + EST_PBAT_FIRE - 1, balls[0].spawnRel);
                            var b = balls[0];
                            if (b.fpath.Count >= 3)
                            {
                                int vx = b.fpath[2].x - b.fpath[1].x, vy = b.fpath[2].y - b.fpath[1].y;
                                double v = Math.Sqrt((double)vx * vx + (double)vy * vy);
                                ExpectRange("|v| cầu lửa = 1.5 px/tick (×256, ±cắt nguyên)", EST_PBAT_BALL_V - 4, EST_PBAT_BALL_V + 2, (int)Math.Round(v));
                            }
                        }
                        Expect("không đổi thanh ENEMY (enemy thường)", 0, CountRecs(r => r.ehp != ehp0));
                        Expect("không có quả cầu", 0, itemsT.Count);
                        Expect("không phải boss (Rosary diệt được)", 0, t.e.IsBoss ? 1 : 0);
                        measures.Add("thức tick " + kW + ", bổ nhào " + kS + ", lơ lửng " + kH + "; cầu lửa " + balls.Count + "; y thấp nhất " + maxY);
                    };
                }
                break;
            case "pbat_kill_l1":
            case "pbat_kill_l2":
                {
                    bool strong = scenario.EndsWith("l2");
                    ses.whipLevel = strong ? 2 : 1;
                    Go("Arena", 160, 176, 1);
                    Put(pbat, 190, 152, -1);
                    Photo(14, 40);
                    finish = () =>
                    {
                        KillChecks(eEnemyId.PhantomBatMinor, strong ? EST_PBAT_S : EST_PBAT_W, EST_SCORE_PBAT, true);
                        Expect("không có quả cầu (không phải boss)", 0, itemsT.Count);
                        Expect("không đổi thanh ENEMY", 0, CountRecs(r => r.ehp != ehp0));
                    };
                }
                break;
            case "pbat_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 16;
                    Go("Arena", 150, 176, 1);
                    Put(pbat, 200, 150, -1);
                    finish = () => Expect("Phantom Bat thường chạm Simon: −4 vạch (Stage 16)", GameSession.MAX_HP - WIKI_DMG_B6, Last.hp);
                }
                break;
            case "freeze_b46":
                {
                    ses.subWeapon = eSubWeapon.Stopwatch;
                    ses.hearts = 10;
                    Go("Arena", 200, 176, 1);
                    Put(eagle, 320, 100, -1);
                    Put(dragon, 380, 136, -1);
                    Put(red, 100, 176, 1);
                    Put(axeK, 300, 176, -1);
                    Put(pbat, 250, 100, -1);
                    var sp = AddSpawner<EdgeSpawner>(eagle);
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
                        ExpectRange("Stopwatch giữ (tick) — video Map_2 186 (CVM3 03)", 185, 187, f1 - f0);
                        int moved = 0, spawned = 0, shotMoved = 0, movedAfter = 0;
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
                            for (int k = 1; k < b.fpath.Count; k++)
                            {
                                int rel = b.spawnRel + k;
                                if (rel > f0 && rel < f1 && b.fpath[k] != b.fpath[k - 1]) shotMoved++;
                            }
                        Expect("enemy đứng yên khi giữ (tick dời)", 0, moved);
                        Expect("đạn đứng yên khi giữ (tick dời)", 0, shotMoved);
                        Expect("bộ sinh Eagle không sinh khi giữ", 0, spawned);
                        ExpectRange("chạy lại sau khi hết giữ (tick dời)", 1, 999999, movedAfter);
                        measures.Add("giữ tick " + f0 + "–" + f1 + "; enemy " + tracks.Count + ", đạn " + shotsT.Count);
                    };
                }
                break;
            case "freeze_whip_axe":
                {
                    ses.subWeapon = eSubWeapon.Stopwatch;
                    ses.hearts = 10;
                    Go("Arena", 200, 176, 1);
                    var k = Put(axeK, 230, 176, -1) as AxeKnight;
                    k.throwTimer = 99999;
                    finish = () =>
                    {
                        KillChecks(eEnemyId.AxeKnight, WIKI_AXE_W, WIKI_SCORE_AXE, true);
                        var t = First(eEnemyId.AxeKnight);
                        int frozenHits = t != null && t.endRel > 0 && recs[t.endRel - 1].frozenE ? 1 : 0;
                        Expect("hạ trong lúc Stopwatch giữ", 1, frozenHits);
                    };
                }
                break;
            case "rosary_b46":
                {
                    Go("Arena", 200, 176, 1);
                    Put(eagle, 300, 100, -1);
                    Put(dragon, 320, 136, -1);
                    Put(red, 100, 176, 1);
                    Put(axeK, 280, 176, -1);
                    Put(pbat, 250, 90, -1);
                    int killed = -1, before = -1, shotsBefore = -1, after = -1;
                    onTick = rel =>
                    {
                        if (rel == 150) { before = OnScreen(eEntityGroup.Enemy); shotsBefore = OnScreen(eEntityGroup.EnemyShot); killed = world.RosaryClear(); }
                        if (rel == 151) after = OnScreen(eEntityGroup.Enemy, startTick + 150) + OnScreen(eEntityGroup.EnemyShot, startTick + 150);
                    };
                    Photo(149, 152);
                    finish = () =>
                    {
                        ExpectRange("enemy trên màn trước Rosary (5 + Fleaman nếu còn)", 5, 99, before);
                        Expect("Rosary gỡ mọi enemy + đạn trên màn", before + shotsBefore, killed);
                        Expect("sau Rosary còn trên màn", 0, after);
                        Expect("0 điểm", 0, Last.score);
                        Expect("Phantom Bat thường bị gỡ (không phải boss)", 1, First(eEnemyId.PhantomBatMinor) != null && First(eEnemyId.PhantomBatMinor).endRel > 0 ? 1 : 0);
                        measures.Add("trước: " + before + " enemy + " + shotsBefore + " đạn");
                    };
                }
                break;
            default:
                Debug.LogError("[EnemyB46Probe] không có kịch bản " + scenario);
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
        if (b != null && h.victim is AxeKnightAxe)
            b.broken = ((AxeKnightAxe)h.victim).broken;
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
            t.frames.Add(en.Anim != null ? en.Anim.FrameIndex : -1);
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
        foreach (var e in w.GroupList(eEntityGroup.Item))
        {
            var ip = e as ItemPickup;
            if (ip == null || !ip.Alive) continue;
            bool seen = false;
            foreach (var it in itemsT) if (it.e == e && it.spawnTick == e.SpawnTick) { seen = true; break; }
            if (!seen) itemsT.Add(new ItemT { e = e, spawnTick = e.SpawnTick, spawnRel = rel, id = ip.id });
        }
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
                var ax = b.e as AxeKnightAxe;
                if (ax != null && ax.broken) b.broken = true;
            }
        var r = new Rec
        {
            sx = s.X, sy = s.Y, hp = ses.playerHp, score = ses.score, state = s.Fsm.CurState, cx = w.CameraCtl != null ? w.CameraCtl.X : 0,
            enemies = enemies, fx = fx, frozenE = w.IsEnemyFrozen, shots = shotsN, ehp = ses.enemyHp,
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
        itemsT.Clear();
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

    static string ItemList()
    {
        var sb = new StringBuilder();
        foreach (var it in itemsT) sb.Append(it.id).Append('@').Append(it.spawnRel).Append(' ');
        return sb.Length > 0 ? sb.ToString() : "(không)";
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
