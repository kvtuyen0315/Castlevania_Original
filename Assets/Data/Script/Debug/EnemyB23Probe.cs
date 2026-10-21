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
/// Kỳ vọng = hằng WIKI_* (số đòn, điểm, sát thương theo stage) + VIDEO_* (CVM2 08: số ĐO video Map_2 — Spear Knight, Medusa Head, Bone
/// Tower, Ghost, dơi treo; CVM3 07: video Map_3 — Skeleton, xương, Fleaman; CVM3 08: Raven) + EST_* (giá trị ƯỚC LƯỢNG routine 17 còn lại —
/// phần video không thấy), chép độc lập Config — đổi
/// Config mà quên đổi đây thì bộ kiểm báo. Geometry Arena giữ nguyên (sàn y 176 x 0–415, tường thấp 416–431 đỉnh
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
    public const int VIDEO_STOPWATCH = 186;   // video:Map_2 (đo:cvm3-03 — CVM3_MEMORY §2d; wiki 5 s = 300 sai)
    // CVM2 08 — số đo video Map_2 (CVM2_MEMORY §2j)
    public const int VIDEO_KNIGHT_VX = 128;                // 0.5 px/tick (1 px / 2 tick)
    public const int VIDEO_KNIGHT_LEG_MAX = 192;           // quay ngẫu nhiên đều [16, 192]
    public const int VIDEO_KNIGHT_WALL = 16, VIDEO_KNIGHT_EDGE = 4, VIDEO_KNIGHT_STUN = 16, VIDEO_KNIGHT_FRAME = 17;
    public const int VIDEO_MEDUSA_VX = 320, VIDEO_MEDUSA_P = 100, VIDEO_MEDUSA_ABOVE = 49, VIDEO_MEDUSA_GAP = 128;
    public const double VIDEO_MEDUSA_A = 33.75;
    public const int VIDEO_MEDUSA_FIRST_MIN = 17, VIDEO_MEDUSA_FIRST_MAX = 56;
    public const int VIDEO_TOWER_FIRST_SHOT = 40, VIDEO_TOWER_WARN = 12, VIDEO_TOWER_GAP = 40, VIDEO_TOWER_CYCLE_MIN = 185, VIDEO_TOWER_CYCLE_MAX = 314;
    public const int VIDEO_TOWER_BALL_VX = 512, VIDEO_TOWER_ABOVE = 15;
    public const int VIDEO_GHOST_VX = 192, VIDEO_GHOST_VY = 32, VIDEO_GHOST_STUN = 16;
    public const int VIDEO_BAT_PERCH = 96, VIDEO_BAT_PERCH_DY = 80, VIDEO_BAT_VX = 384, VIDEO_BAT_DROP = 31, VIDEO_BAT_SWOOP = 42;
    // CVM3 08 — Raven đo video Map_3 (CVM3_MEMORY §2h); fixed 1/256
    public const int VIDEO_RAVEN_PERCH = 60, VIDEO_RAVEN_VX = 192, VIDEO_RAVEN_DECEL = 9, VIDEO_RAVEN_HOVER = 25, VIDEO_RAVEN_TURN_HOVER = 14, VIDEO_RAVEN_ABOVE = 16;
    public const int VIDEO_RAVEN_DASH_V0 = 32, VIDEO_RAVEN_DASH_ACC = 8, VIDEO_RAVEN_DASH_MAX = 768, VIDEO_RAVEN_BRAKE = 48;
    public static readonly int[] VIDEO_RAVEN_VY0 = { 320, 448, 512 };
    // CVM3 07 — số đo video Map_3 (CVM3_MEMORY §2g); fixed 1/256
    public const int VIDEO_SKEL_VX = 368, VIDEO_SKEL_LEG = 16, VIDEO_SKEL_PAUSE = 4, VIDEO_SKEL_THROW_TICKS = 18, VIDEO_SKEL_KEEP = 72, VIDEO_SKEL_THROW_AT = 4, VIDEO_SKEL_FRAME = 17;
    public const int VIDEO_SKEL_JUMP_VY = -640, VIDEO_SKEL_JUMP_VX = 320, VIDEO_SKEL_G = 32;
    public const int VIDEO_SKEL_BONE_VX = 192, VIDEO_SKEL_BONE_VY_LOW = -896, VIDEO_SKEL_BONE_VY_HIGH = -1152, VIDEO_SKEL_BONE_G = 32, VIDEO_SKEL_HAND = 13;
    public const int VIDEO_FLEA_TRIGGER = 128, VIDEO_FLEA_DELAY_MIN = 62, VIDEO_FLEA_DELAY_MAX = 136, VIDEO_FLEA_CROUCH = 13, VIDEO_FLEA_WAIT_FRAME = 9;
    public const int VIDEO_FLEA_SHORT_VX = 576, VIDEO_FLEA_SHORT_VY = -256, VIDEO_FLEA_G = 32;
    public const int EST_FLEA_LONG_VX = 576, EST_FLEA_LONG_VY = -832, EST_FLEA_PAUSE_MIN = 4, EST_FLEA_PAUSE_MAX = 16;


    private struct Scn
    {
        public string name, rle, note;
        public Scn(string n, string r, string note) { name = n; rle = r; this.note = note; }
    }

    const string WHIP5 = "B -*23 B -*23 B -*23 B -*23 B -*23";
    const string WHIP10 = WHIP5 + " " + WHIP5;

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("knight_patrol", "-*600", "Arena: Simon (60,176) đứng; Spear Knight (250,176) tuần tra 0.5 px/tick, quay đầu ngẫu nhiên 16–192 tick (video Map_2), không rời sàn"),
        new Scn("knight_ledge", "-*400", "Spear Knight trên đỉnh tường thấp (424,144) — quay đầu ở mép (dò sàn pivot ± 4 — video Map_2), không rơi"),
        new Scn("knight_wall", "-*120", "CVM2 08: Spear Knight (380,176) đi phải về tường thấp x 416: quay ở x 400 = tường − 16 (video Map_2 TEB khe: quay ở 336, tường 352)"),
        new Scn("knight_touch", "-*90", "Stage 4: Spear Knight đi vào Simon (không bất tử): −2 vạch (chỉ va chạm, không đâm giáo)"),
        new Scn("knight_whip_l1", WHIP5 + " -*40", "roi da (yếu) hạ Spear Knight: đúng 3 đòn, +400, tia lửa + khựng 16 tick ở đòn chưa hạ (video Map_2)"),
        new Scn("knight_whip_l2", WHIP5 + " -*40", "roi xích (mạnh): đúng 2 đòn"),
        new Scn("medusa_front", "-*900", "Simon (200,176) quay phải đứng: EdgeSpawner Medusa Head trước mặt — 1.25 px/tick, sinh ở đỉnh chân − 49, sóng 1−cos A 33.75 P 100, ≤ 2 con, đều 128 tick (video Map_2)"),
        new Scn("medusa_back", "-*240", "Simon quay TRÁI giữa màn ⇒ Medusa Head ra từ mép trái, bay sang phải"),
        new Scn("medusa_kill", "-*30 " + WHIP5 + " -*40", "roi cấp 3 hạ Medusa Head bay ngang tầm: 1 đòn, +300"),
        new Scn("medusa_touch", "-*160", "Stage 5: Medusa Head chạm Simon: −2 vạch"),
        new Scn("tower_fire", "-*720", "Bone Tower (260,176), Simon (150,176): phát 1 ở tick 40 (đỏ 12 tick trước) ⇒ 2 cầu lửa CÙNG miệng đầu trên cách 40 tick, bay 2.0 px/tick ⇒ lượt kế sau 185–314 tick (video Map_2)"),
        new Scn("tower_face", "R*120 -*60", "CVM2 08: Simon đi phải ngang qua Bone Tower (180,176) ⇒ tháp quay mặt theo Simon NGAY (video Map_2 f4982/4992/5034), cầu lửa bay về hướng đang quay"),
        new Scn("tower_fire_hit", "-*220", "Stage 6: cầu lửa Bone Tower trúng Simon: −2 vạch"),
        new Scn("tower_whip_ball", "-*44 " + WHIP5 + " -*60", "quất vỡ cầu lửa (wiki): cầu biến mất + 100 đ (suy luận), Simon không mất máu (video Map_2 không có lần phá)"),
        new Scn("tower_kill_l1", WHIP10 + " -*30", "roi da hạ Bone Tower: đúng 8 đòn, +400"),
        new Scn("tower_kill_l2", WHIP10 + " -*30", "roi xích: đúng 6 đòn"),
        new Scn("ghost_chase", "-*900", "Ghost (260,100), Simon (150,176) đứng: sinh đủ tốc 0.75 về phía Simon, trần 0.75/0.125 px/tick (video Map_2), vượt quá rồi vòng lại (wiki)"),
        new Scn("ghost_spawn", "-*120", "CVM2 08: EdgeSpawner BehindPlayer Ghost: Simon (200,176) quay phải ⇒ Ghost hiện ở cx + 32 (96 px sau lưng), tâm = chân Simon (video Map_2 2/2)"),
        new Scn("ghost_kill_l1", WHIP10 + " -*30", "roi da hạ Ghost: đúng 3 đòn, +300"),
        new Scn("ghost_kill_l2", WHIP10 + " -*30", "roi xích: đúng 2 đòn"),
        new Scn("ghost_touch", "-*200", "Stage 7: Ghost chạm Simon: −3 vạch"),
        new Scn("raven_dive", "R*60 -*420", "Raven đậu (300,100); Simon đi phải từ 140: cất cánh đúng 60 tick sau khi sinh (không theo khoảng cách) ⇒ lướt vx 0.75 về Simon, vy0 1.25/1.75/2.0 về TÂM THÂN Simon giảm 9/256 mỗi tick ⇒ treo 25 ⇒ cao hơn tâm ⇒ lao ngang v0 0.125 + 1/32 mỗi tick ⇒ vượt Simon ⇒ phanh 0.1875 ⇒ treo 14 ⇒ lao lại ⇒ vượt lần 2 bay luôn (video Map_3)"),
        new Scn("raven_glide_up", "-*260", "CVM3 08: Raven đậu (300,176) thấp hơn tâm thân Simon (200,176): chặng lướt đầu đi LÊN-trái (video Map_3 #5/#6 y 121 với chân Simon 128 ⇒ lên); treo xong vẫn thấp hơn tâm ⇒ lướt tiếp (video #3)"),
        new Scn("raven_touch", "-*300", "Stage 7: Raven lao ngang trúng Simon: −3 vạch"),
        new Scn("raven_kill", "-*36 " + WHIP5 + " -*40", "roi da hạ Raven đang ĐẬU ngang tầm roi (236,156): 1 đòn, +200 (video: 11/11 lần hạ +200)"),
        new Scn("skel_idle", "-*900", "Skeleton (300,176), Simon (200,176) đứng: lượt đi 20 tick = đi 16 (1.4375 px/tick tới nếu |dx| ≥ 72, lùi nếu gần hơn) + đứng 4; lượt ném 18 tick (25 %, 1 xương ở tick 4) — xương 0.75 px/tick, vy0 −3.5 / −4.5, sinh Y − 13; không nhảy trên sàn phẳng; khung đi 17 tick liên tục (video Map_3)"),
        new Scn("skel_pit_jump", "-*120", "CVM3 07: Skeleton (530,176) đi trái về Simon (440,176): tới mép sàn x 512 ⇒ nhảy (vy0 −2.5, g 0.125, vx 1.25 — đỉnh 23 px, 40 tick) qua hố 480–511, đáp x 462 (video Map_3 WEB 0:50.9 / MummyMan 2:53.0)"),
        new Scn("skel_bone_hit", "-*300", "Stage 7: xương trúng Simon: −3 vạch/đòn"),
        new Scn("skel_kill", "-*10 Sub -*80", "dao hạ Skeleton: 1 đòn, +300"),
        new Scn("flea_hops", "R*60 -*540", "Fleaman (300,176) ngồi chờ (khung 0↔1 mỗi 9 tick); Simon đi phải từ 150: |dx| < 128 ⇒ đếm 62–136 tick ⇒ ngồi thấp 13 tick ⇒ cú ngắn 2.25 / −1 / g 0.125 (video Map_3), chu kỳ ngắn-ngắn-vọt (wiki), nghỉ 4–16"),
        new Scn("flea_touch", "-*240", "Stage 7: Fleaman nhảy vào Simon: −3 vạch (CVM3 07: chờ 62–136 + ngồi thấp 13 tick rồi mới nhảy ⇒ 240 tick)"),
        new Scn("flea_kill", "D*4 D+B D*23 -*40", "roi da NGỒI hạ Fleaman đang ngồi chờ (228,176): 1 đòn, +500 (CVM3 07: Fleaman nay chờ 62–136 tick mới bật; roi đứng hàng Y−24..−17 cao hơn Fleaman 16 px đang ở đất — video chỉ hạ lúc sắp đáp, wiki St11 'quất ngay trước khi chạm đất')"),
        new Scn("bat_black", "R*100 -*200", "Dơi treo (300,100) — chân Simon thấp hơn 76 < 80; Simon đi phải từ 150: |dx| < 96 ⇒ sà 31 px theo 1/4 sin trong 42 tick, ngang 1.5 px/tick, rồi bay ngang thẳng (video Map_2 Hall C)"),
        new Scn("bat_black_high", "-*200", "CVM2 08: dơi treo (210,88) — chân Simon (200,176) thấp hơn 88 ≥ 80 ⇒ KHÔNG rời chỗ dù |dx| 10 (video Map_2 Hall C đứng dy 87 chưa rời; TEA dy 107 không)"),
        new Scn("freeze_b23", "-*60 Sub -*400", "Stopwatch: mọi enemy Block 2–3 + đạn + bộ sinh Medusa Head dừng 186 tick rồi chạy tiếp"),
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
        public readonly List<int> frames = new List<int>();     // CVM2 08: chỉ số khung anim từng tick
        public readonly List<int> hitRels = new List<int>();    // CVM2 08: tick (rel) nhận đòn
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
                        LegChecks(t, VIDEO_KNIGHT_VX, VIDEO_KNIGHT_LEG_MAX, 2);
                        int badA = 0, runA = 1;
                        for (int k = 1; k < t.anims.Count; k++)
                        {
                            if (t.frames[k] == t.frames[k - 1]) { runA++; continue; }
                            if (runA != VIDEO_KNIGHT_FRAME && k - runA > 0) badA++;
                            runA = 1;
                        }
                        Expect("khung walk 0→1→2→1 đổi mỗi 17 tick (lần sai)", 0, badA);
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
                        int mn = 999, mx = -999;
                        foreach (var v in t.path) { mn = Math.Min(mn, v.x); mx = Math.Max(mx, v.x); }
                        Expect("quay ở mép trái: x nhỏ nhất = 416 + 4 − 1 (dò sàn pivot − 4)", 416 + VIDEO_KNIGHT_EDGE - 1, mn);
                        Expect("quay ở mép phải: x lớn nhất = 431 − 4 + 1", 431 - VIDEO_KNIGHT_EDGE + 1, mx);
                    };
                }
                break;
            case "knight_wall":
                {
                    Go("Arena", 60, 176, 1);
                    var k = Put(knight, 380, 176, 1) as SpearKnight;
                    k.legTimer = 9999;
                    Photo(20, 40);
                    finish = () =>
                    {
                        var t = First(eEnemyId.SpearKnight);
                        if (t == null) return;
                        int mx = -999;
                        foreach (var v in t.path) mx = Math.Max(mx, v.x);
                        Expect("quay trước tường thấp x 416: x lớn nhất = 416 − 16", 416 - VIDEO_KNIGHT_WALL, mx);
                        ExpectRange("đã quay đầu (lần)", 1, 1, t.e is SpearKnight ? ((SpearKnight)t.e).turns : 0);
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
                    finish = () =>
                    {
                        KillChecks(eEnemyId.SpearKnight, strong ? WIKI_KNIGHT_S : WIKI_KNIGHT_W, WIKI_SCORE_KNIGHT, true);
                        StunChecks(eEnemyId.SpearKnight, VIDEO_KNIGHT_STUN);
                    };
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
                        ExpectRange("lần sinh đầu (tick) — video 17 / 56", VIDEO_MEDUSA_FIRST_MIN - 1, VIDEO_MEDUSA_FIRST_MAX - 1, ts[0].spawnRel);
                        int badX = 0, badF = 0, badY = 0;
                        foreach (var t in ts)
                        {
                            if (t.x0 != (back ? cx + 8 : cx + NesCoord.SCREEN_W - 8)) badX++;
                            if (t.facing0 != (back ? 1 : -1)) badF++;
                            if (t.y0 != 176 - VIDEO_MEDUSA_ABOVE) badY++;
                        }
                        Expect(back ? "Simon quay trái ⇒ sinh mép trái (cx + 8) — số lần sai" : "sinh mép trước mặt (cx + 248) — số lần sai", 0, badX);
                        Expect("bay vào màn — số lần sai", 0, badF);
                        Expect("sinh ở ĐỈNH sóng = chân Simon − 49 — số lần sai", 0, badY);
                        var t0 = ts[0];
                        int badV = 0, n = 0, minY = 999, maxY = -999;
                        for (int k = 1; k < t0.fpath.Count; k++, n++)
                            if (t0.fpath[k].x - t0.fpath[k - 1].x != t0.facing0 * VIDEO_MEDUSA_VX) badV++;
                        for (int k = 0; k < t0.path.Count; k++) { minY = Math.Min(minY, t0.path[k].y); maxY = Math.Max(maxY, t0.path[k].y); }
                        Expect("ngang 1.25 px/tick (tick sai / " + n + ")", 0, badV);
                        ExpectRange("đỉnh–đáy (px) — video 67–68", 66, 68, maxY - minY);
                        int badW = 0;
                        for (int k = 0; k < t0.fpath.Count; k++)
                        {
                            int want = t0.y0 * 256 + (int)Math.Round(VIDEO_MEDUSA_A * (1 - Math.Cos(2 * Math.PI * k / VIDEO_MEDUSA_P)) * 256);
                            if (Math.Abs(t0.fpath[k].y - want) > 1) badW++;
                        }
                        Expect("y = y0 + 33.75·(1 − cos 2πt/100) đúng từng tick (tick sai)", 0, badW);
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
                                if (d != VIDEO_MEDUSA_GAP && !cap) badGap++;
                            }
                            measures.Add(sb.ToString());
                            Expect("khoảng sinh đều 128 tick (video 126.8–129.0) — số lần sai", 0, badGap);
                        }
                    };
                }
                break;
            case "medusa_kill":
                {
                    ses.whipLevel = 3;
                    Go("Arena", 200, 176, 1);
                    Put(medusa, 300, 112, -1);   // CVM2 08: sinh ở đỉnh 112 ⇒ tick 30–40 ngang tầm roi
                    Photo(30, 46);
                    finish = () => KillChecks(eEnemyId.MedusaHead, 1, WIKI_SCORE_MEDUSA, false);
                }
                break;
            case "medusa_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 5;
                    Go("Arena", 200, 176, 1);
                    Put(medusa, 300, 176 - VIDEO_MEDUSA_ABOVE, -1);
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
                        Expect("cảnh báo đỏ kéo dài (tick) — video 8–14", VIDEO_TOWER_WARN, kF - kW);
                        int red = 0, flips = 0;
                        for (int k = kW; k < kF; k++)
                        {
                            if (t.anims[k] == "red") red++;
                            if (k > kW && t.anims[k] != t.anims[k - 1]) flips++;
                        }
                        Expect("đỏ LIỀN khi cảnh báo (số lần đổi màu)", 0, flips);
                        Expect("tick đỏ khi cảnh báo", kF - kW, red);
                        var balls = ShotsOf("BoneTowerFireball");
                        ExpectRange("số cầu lửa (≥ 2 lượt)", 4, 99, balls.Count);
                        if (balls.Count == 0) return;
                        Expect("phát 1 lượt đầu ở tick (video: 40 tick sau khi hết đóng băng)", VIDEO_TOWER_FIRST_SHOT, balls[0].spawnRel);
                        int badPair = 0, badVx = 0, badY = 0;
                        for (int i = 0; i + 1 < balls.Count; i += 2)
                            if (balls[i + 1].spawnRel - balls[i].spawnRel != VIDEO_TOWER_GAP) badPair++;
                        foreach (var b in balls)
                            if (b.fpath.Count > 0 && Fixed.ToPx(b.fpath[0].y) != 176 - VIDEO_TOWER_ABOVE) badY++;
                        foreach (var b in balls)
                            for (int k = 1; k < b.fpath.Count; k++)
                                if (b.fpath[k].x - b.fpath[k - 1].x != -VIDEO_TOWER_BALL_VX || b.fpath[k].y != b.fpath[0].y) badVx++;
                        Expect("2 cầu/lượt cách 40 tick — số lượt sai", 0, badPair);
                        Expect("cả 2 cầu từ miệng đầu TRÊN (chân − 15) — số cầu sai", 0, badY);
                        Expect("bay ngang −2.0 px/tick về phía Simon (tick sai)", 0, badVx);
                        int badCycle = 0;
                        var sbc = new StringBuilder("phát 1 → phát 1 (tick): ");
                        for (int i = 2; i < balls.Count; i += 2)
                        {
                            int c = balls[i].spawnRel - balls[i - 2].spawnRel;
                            sbc.Append(c).Append(' ');
                            if (c < VIDEO_TOWER_CYCLE_MIN || c > VIDEO_TOWER_CYCLE_MAX) badCycle++;
                        }
                        measures.Add(sbc.ToString());
                        Expect("khoảng 2 lượt trong 185–314 tick (video 6 mẫu) — số lần sai", 0, badCycle);
                        measures.Add("cầu lửa sinh ở tick: " + string.Join(" ", balls.ConvertAll(b => b.spawnRel.ToString()).ToArray()));
                    };
                }
                break;
            case "tower_face":
                {
                    Go("Arena", 120, 176, 1);
                    Put(tower, 180, 176, -1);
                    Photo(30, 80);
                    finish = () =>
                    {
                        var t = First(eEnemyId.BoneTower);
                        if (t == null) return;
                        int bad = 0;
                        for (int k = 0; k < t.facing.Count; k++)
                        {
                            int rel = t.spawnRel + k;
                            int sx = rel < recs.Count ? recs[rel].sx : 0;
                            if (sx != 180 && t.facing[k] != Math.Sign(sx - 180) && k > 0) bad++;
                        }
                        Expect("tháp luôn quay về Simon (tick sai — trễ 1 tick cho phép ở tick đổi)", 0, Math.Max(0, bad - 2));
                        var balls = ShotsOf("BoneTowerFireball");
                        int badDir = 0;
                        foreach (var b in balls)
                        {
                            int sx = b.spawnRel < recs.Count ? recs[b.spawnRel].sx : 0;
                            if (b.fpath.Count > 1 && Math.Sign(b.fpath[1].x - b.fpath[0].x) != Math.Sign(sx - 180)) badDir++;
                        }
                        ExpectRange("có cầu lửa", 1, 99, balls.Count);
                        Expect("cầu lửa bay về phía Simon lúc bắn (số cầu sai)", 0, badDir);
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
                            if (dx > VIDEO_GHOST_VX || dy > VIDEO_GHOST_VY) badV++;
                            int rel = t.spawnRel + k;
                            int sx = rel < recs.Count ? recs[rel].sx : 150;
                            if (Math.Sign(t.path[k].x - sx) != Math.Sign(t.path[k - 1].x - sx) && t.path[k].x != sx) crosses++;
                            int d = Math.Abs(t.path[k].x - sx) + Math.Abs(t.path[k].y - (176 - 16));
                            if (d < minD) { minD = d; kMin = k; }
                        }
                        Expect("tốc độ ≤ 0.75 ngang / 0.125 dọc (tick vượt)", 0, badV);
                        Expect("sinh đủ tốc: tick đầu vx = −0.75 về phía Simon", -VIDEO_GHOST_VX, t.fpath.Count > 1 ? t.fpath[1].x - t.fpath[0].x : 0);
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
                    finish = () =>
                    {
                        KillChecks(eEnemyId.Ghost, strong ? WIKI_GHOST_S : WIKI_GHOST_W, WIKI_SCORE_GHOST, true);
                        StunChecks(eEnemyId.Ghost, VIDEO_GHOST_STUN);
                    };
                }
                break;
            case "ghost_spawn":
                {
                    Go("Arena", 200, 176, 1);
                    var sp = AddSpawner<EdgeSpawner>(ghost);
                    sp.side = eSpawnSide.BehindPlayer;
                    sp.playerRelativeY = true;
                    Photo(4, 40);
                    finish = () =>
                    {
                        var ts = Tracks(eEnemyId.Ghost);
                        Expect("1 Ghost (maxAlive 1)", 1, ts.Count);
                        if (ts.Count == 0) return;
                        int cx = recs.Count > 0 ? recs[0].cx : 0;
                        Expect("hiện 96 px SAU lưng Simon: x = cx + 32", cx + 32, ts[0].x0);
                        Expect("tâm = chân Simon (y 176)", 176, ts[0].y0);
                        Expect("bay về phía Simon (quay phải)", 1, ts[0].facing0);
                    };
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
                    Go("Arena", 140, 176, 1);
                    Put(raven, 300, 100, -1);   // trong màn lúc cất cánh (cx 72) và đủ xa Simon ⇒ cú lao đầu nhanh ⇒ phanh 10–11 tick (mô phỏng Tools/cvm3/enemy8b_raven_model.py)
                    Photo(30, 64, 90, 120, 160, 200, 240);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Raven);
                        if (t == null) return;
                        var rv = t.e as Raven;
                        Expect("cất cánh ở ageTicks 60 (video 58–62 tick sau khi hiện)", VIDEO_RAVEN_PERCH, rv != null ? rv.takeoffAge : -1);
                        int kG = t.states.IndexOf("Glide");
                        Expect("đậu yên trước khi cất cánh (tick đổi vị trí)", 0, CountWhere(t, k => kG >= 0 && k < kG && t.path[k] != t.path[0]));
                        RavenGlideChecks(t);
                        Expect("treo sau chặng lướt đầu (tick)", VIDEO_RAVEN_HOVER, RunLen(t, "Hover", 0));
                        int badA = 0, badB = 0, nA = 0, nB = 0;
                        for (int k = 1; k < t.fpath.Count; k++)
                        {
                            int d = Math.Abs(t.fpath[k].x - t.fpath[k - 1].x), d0 = k >= 2 ? Math.Abs(t.fpath[k - 1].x - t.fpath[k - 2].x) : -1;
                            if (t.states[k] == "Dash" && k >= 2 && t.states[k - 1] == "Dash") { nA++; if (d != Math.Min(d0 + VIDEO_RAVEN_DASH_ACC, VIDEO_RAVEN_DASH_MAX)) badA++; }
                            // tick đầu Brake chạy nối Dash cùng tick (StateMachine) ⇒ chỉ kiểm từ tick Brake thứ 2
                            if (t.states[k] == "Brake" && k >= 2 && t.states[k - 1] == "Brake" && t.states[k - 2] == "Brake") { nB++; if (d != Math.Max(0, d0 - VIDEO_RAVEN_BRAKE)) badB++; }
                            if (t.states[k] == "Dash" && t.fpath[k].y != t.fpath[k - 1].y) badA++;
                        }
                        Expect("lao ngang: |vx| +1/32 mỗi tick, y giữ (tick sai / " + nA + ")", 0, badA);
                        Expect("phanh: |vx| −0.1875 mỗi tick (tick sai / " + nB + ")", 0, badB);
                        ExpectRange("phanh kéo dài (tick Brake liền nhau, bỏ tick đầu)", 4, 40, nB);
                        ExpectRange("có lao ngang", 1, 99, rv != null ? rv.dashes : 0);
                        Expect("số lần phanh quay đầu (video 1 — lần vượt thứ 2 bay luôn)", 1, Runs(t, "Brake"));
                        Expect("treo sau phanh (tick)", VIDEO_RAVEN_TURN_HOVER, RunLen(t, "Hover", Runs(t, "Hover") - 1));
                        Expect("hướng mặt luôn về Simon (tick sai, bỏ tick trùng x)", 0, CountWhere(t, k => recs[t.spawnRel + k].sx != t.path[k].x && t.facing[k] != (recs[t.spawnRel + k].sx > t.path[k].x ? 1 : -1)));
                        ExpectRange("bay ra khỏi màn sau cú lao thứ 2 (gỡ Offscreen)", 1, 1, t.endRel >= 0 && t.reason == eKillReason.Offscreen ? 1 : 0);
                        Expect("khung bay raven_1→2→3→2 mỗi 9 tick (anim fly)", "fly", t.anims.Count > kG + 1 && kG >= 0 ? t.anims[kG + 1] : "");
                        measures.Add("cất cánh age " + (rv != null ? rv.takeoffAge : -1) + ", lướt " + (rv != null ? rv.glides : 0) + ", lao " + (rv != null ? rv.dashes : 0)
                                     + ", treo " + RunLen(t, "Hover", 0) + "/" + RunLen(t, "Hover", Runs(t, "Hover") - 1) + ", gỡ rel " + t.endRel);
                    };
                }
                break;
            case "raven_glide_up":
                {
                    Go("Arena", 200, 176, 1);
                    Put(raven, 300, 176, -1);
                    Photo(70, 110, 160);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Raven);
                        if (t == null) return;
                        int kG = t.states.IndexOf("Glide");
                        Expect("chặng lướt đầu đi LÊN (vy0 < 0 — Raven thấp hơn tâm thân Simon)", 1, kG >= 1 && t.fpath[kG].y < t.fpath[kG - 1].y ? 1 : 0);
                        Expect("chặng lướt đầu đi về phía Simon (trái)", 1, kG >= 1 && t.fpath[kG].x < t.fpath[kG - 1].x ? 1 : 0);
                        RavenGlideChecks(t);
                        var rv = t.e as Raven;
                        int kH = t.states.IndexOf("Hover"), kNext = -1;
                        for (int k = kH; k >= 0 && k < t.states.Count; k++) if (t.states[k] != "Hover") { kNext = k; break; }
                        bool below = kH >= 0 && t.path[kH].y >= 176 - VIDEO_RAVEN_ABOVE;
                        Expect("treo xong: thấp hơn tâm ⇒ lướt tiếp / cao hơn ⇒ lao ngang", below ? "Glide" : "Dash", kNext >= 0 ? t.states[kNext] : "");
                        measures.Add("lướt " + (rv != null ? rv.glides : 0) + " (y sau chặng 1: " + (kH >= 0 ? t.path[kH].y : -1) + "), state sau treo " + (kNext >= 0 ? t.states[kNext] : "-"));
                    };
                }
                break;
            case "raven_touch":
                {
                    CvgpDebug.CheatInvincible = false;
                    world.Stage = 7;
                    Go("Arena", 200, 176, 1);
                    Put(raven, 250, 128, -1);
                    finish = () =>
                    {
                        int lost = GameSession.MAX_HP - Last.hp;
                        ExpectRange("Raven lao trúng Simon (số lần, 300 tick — Raven quay lại được 1 lần)", 1, 3, lost / WIKI_DMG_B3);
                        Expect("mỗi đòn −3 vạch (Stage 7): phần dư", 0, lost % WIKI_DMG_B3);
                    };
                }
                break;
            case "raven_kill":
                {
                    Go("Arena", 200, 176, 1);
                    Put(raven, 236, 156, -1);
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
                        ExpectRange("số lượt (900 tick ⇒ 45–50)", 44, 51, sk != null ? sk.units : 0);
                        // chặng đi: tick state ActL/ActR liên tục — đúng 16 tick, dời đúng 1.4375 px/tick; giữa 2 chặng ≥ 4 tick đứng
                        int badV = 0, badLeg = 0, badGap = 0, badDir = 0, legs = 0, runStart = -1, lastEnd = -1;
                        for (int k = 1; k <= t.states.Count; k++)
                        {
                            bool walk = k < t.states.Count && (t.states[k] == "ActL" || t.states[k] == "ActR");
                            if (walk && Math.Abs(t.fpath[k].x - t.fpath[k - 1].x) != VIDEO_SKEL_VX) badV++;
                            if (walk && runStart < 0)
                            {
                                runStart = k;
                                if (lastEnd >= 0 && k - lastEnd < VIDEO_SKEL_PAUSE) badGap++;
                                int sx = recs[t.spawnRel + k].sx, x0 = t.path[k - 1].x;
                                int toward = sx > x0 ? 1 : -1, dir = t.fpath[k].x > t.fpath[k - 1].x ? 1 : -1;
                                if (dir != (Math.Abs(sx - x0) >= VIDEO_SKEL_KEEP ? toward : -toward)) badDir++;
                            }
                            if (!walk && runStart >= 0)
                            {
                                legs++;
                                if (k - runStart != VIDEO_SKEL_LEG && k < t.states.Count) badLeg++;
                                lastEnd = k;
                                runStart = -1;
                            }
                        }
                        ExpectRange("số chặng đi", 10, 99, legs);
                        int badT = 0, tRun = 0;
                        for (int k = 0; k <= t.states.Count; k++)
                        {
                            if (k < t.states.Count && t.states[k] == "ActT") { tRun++; continue; }
                            if (tRun > 0 && k < t.states.Count && tRun % VIDEO_SKEL_THROW_TICKS != 0) badT++;
                            tRun = 0;
                        }
                        Expect("lượt ném đứng đúng 18 tick (liền nhau ⇒ bội 18) (lần sai)", 0, badT);
                        Expect("chặng đi dời đúng 1.4375 px/tick (tick sai)", 0, badV);
                        Expect("chặng đi dài 16 tick (chặng sai)", 0, badLeg);
                        Expect("đứng ≥ 4 tick giữa 2 chặng (lần sai)", 0, badGap);
                        Expect("tới khi |dx| ≥ 72, lùi khi gần hơn (chặng sai)", 0, badDir);
                        // xương: 1 xương / lượt ném, sinh (X, Y − 13), 0.75 px/tick về phía Simon, vy0 −3.5 / −4.5, g 0.125
                        int badPath = 0, low = 0, high = 0, badSpawn = 0;
                        foreach (var b in bs)
                        {
                            if (b.fpath.Count < 3) continue;
                            int k = b.spawnRel - t.spawnRel;
                            if (k < 0 || k >= t.path.Count || t.states[k] != "ActT") badSpawn++;
                            else if (b.fpath[0].x != t.path[k].x * 256 || b.fpath[0].y != (t.path[k].y - VIDEO_SKEL_HAND) * 256) badSpawn++;
                            int vx = b.fpath[1].x - b.fpath[0].x, vy1 = b.fpath[1].y - b.fpath[0].y;
                            int sx = recs[b.spawnRel].sx;
                            if (vx != (sx > b.fpath[0].x / 256 ? 1 : -1) * VIDEO_SKEL_BONE_VX) badPath++;
                            if (vy1 == VIDEO_SKEL_BONE_VY_LOW + VIDEO_SKEL_BONE_G) low++;
                            else if (vy1 == VIDEO_SKEL_BONE_VY_HIGH + VIDEO_SKEL_BONE_G) high++;
                            else badPath++;
                            if (b.fpath[2].y - b.fpath[1].y - vy1 != VIDEO_SKEL_BONE_G) badPath++;
                        }
                        ExpectRange("số xương (25 % × ~45 lượt)", 5, 20, bs.Count);
                        Expect("số xương = số lượt ném", sk != null ? sk.throws : -1, bs.Count);
                        Expect("xương sinh ở (X, Y − 13) đúng lượt ném (xương sai)", 0, badSpawn);
                        Expect("xương 0.75 px/tick về Simon, vy0 −3.5 / −4.5, g 0.125 (số sai)", 0, badPath);
                        ExpectRange("có xương thấp (vy0 −3.5)", 1, 99, low);
                        ExpectRange("có xương cao (vy0 −4.5)", 1, 99, high);
                        Expect("không nhảy trên sàn phẳng (video: chỉ nhảy ở mép, 6/6)", 0, sk != null ? sk.jumps : -1);
                        Expect("quay mặt về Simon (tick sai, bỏ tick trùng x)", 0, CountWhere(t, k => recs[t.spawnRel + k].sx != t.path[k].x && t.facing[k] != (recs[t.spawnRel + k].sx > t.path[k].x ? 1 : -1)));
                        int badF = 0, f0 = 0;
                        for (int k = 1; k < t.frames.Count; k++)
                            if (t.frames[k] != t.frames[k - 1])
                            {
                                if (f0 > 0 && k - f0 != VIDEO_SKEL_FRAME) badF++;
                                f0 = k;
                            }
                        Expect("khung đi đổi đều mỗi 17 tick cả lúc đứng/ném (lần sai)", 0, badF);
                        measures.Add("lượt " + (sk != null ? sk.units : 0) + ", chặng " + legs + ", xương " + bs.Count + " (thấp " + low + " / cao " + high + "), nhảy " + (sk != null ? sk.jumps : 0));
                    };
                }
                break;
            case "skel_pit_jump":
                {
                    Go("Arena", 440, 176, 1);
                    var sk = Put(skel, 530, 176, -1) as Skeleton;
                    if (sk != null) sk.forcedPlan = new[] { Skeleton.PLAN_TOWARD, Skeleton.PLAN_STAND, Skeleton.PLAN_STAND, Skeleton.PLAN_STAND };
                    Photo(10, 20, 40);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Skeleton);
                        if (t == null) return;
                        int kJ = t.states.IndexOf("Jump"), kL = kJ >= 0 ? t.states.IndexOf("Act", kJ) : -1;
                        ExpectRange("có nhảy", 1, 9999, kJ);
                        if (kJ < 1 || kL < 0) return;
                        int xT = t.path[kJ - 1].x, minY = 999;
                        for (int k = kJ; k < kL; k++) minY = Math.Min(minY, t.path[k].y);
                        ExpectRange("bật ở mép sàn x 512 (bước tới sẽ hụt sàn)", 512, 513, xT);
                        Expect("đỉnh cú nhảy (px)", Apex(VIDEO_SKEL_JUMP_VY, VIDEO_SKEL_G), 176 - minY);
                        Expect("trên không 40 tick (tick bật … tick đáp)", 40, kL - kJ + 1);
                        Expect("đáp ở mặt sàn y 176", 176, t.path[kL].y);
                        int want = t.fpath[kJ - 1].x - 40 * VIDEO_SKEL_JUMP_VX;
                        ExpectRange("đáp ở x = bật − 50 (vx 1.25)", want / 256 - 1, want / 256 + 1, t.path[kL].x);
                        Expect("1 cú nhảy", 1, (t.e as Skeleton) != null ? (t.e as Skeleton).jumps : -1);
                        measures.Add("bật x " + xT + " tick " + kJ + ", đỉnh " + (176 - minY) + ", đáp x " + t.path[kL].x + " tick " + kL);
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
                        int kIn = -1, kG = t.states.IndexOf("Ground"), kH = -1;
                        for (int k = 0; k < t.path.Count && kIn < 0; k++) if (Math.Abs(recs[t.spawnRel + k].sx - 300) < VIDEO_FLEA_TRIGGER) kIn = k;
                        for (int k = 0; k < t.states.Count && kH < 0; k++) if (t.states[k].StartsWith("Hop")) kH = k;
                        Expect("vào tầm khi |Simon − Fleaman| < 128: Simon X = 300 − 127", 300 - (VIDEO_FLEA_TRIGGER - 1), kIn >= 0 ? recs[t.spawnRel + kIn].sx : -1);
                        ExpectRange("đếm sau khi vào tầm 62–136 tick (video 62 / 62 / 136)", VIDEO_FLEA_DELAY_MIN, VIDEO_FLEA_DELAY_MAX, kG - kIn);
                        Expect("ngồi thấp fleaman_0 13 tick rồi bật", VIDEO_FLEA_CROUCH, kH - kG);
                        int badW = 0, w0 = 0;
                        for (int k = 1; k < kG && k < t.frames.Count; k++)
                            if (t.frames[k] != t.frames[k - 1])
                            {
                                if (w0 > 0 && k - w0 != VIDEO_FLEA_WAIT_FRAME) badW++;
                                w0 = k;
                            }
                        Expect("ngồi chờ đổi khung 0↔1 mỗi 9 tick (lần sai)", 0, badW);
                        Expect("ngồi thấp ở khung 0 (tick sai)", 0, CountWhere(t, k => k >= kG && k < kH && t.frames[k] != 0));
                        // tách từng cú nhảy
                        var kinds = new StringBuilder();
                        int badApex = 0, badPause = 0, hops = 0, badSeq = 0, kStart = -1, lastLand = -1, crossed = 0, badVx = 0;
                        int shortApex = Apex(VIDEO_FLEA_SHORT_VY, VIDEO_FLEA_G), longApex = Apex(EST_FLEA_LONG_VY, VIDEO_FLEA_G);
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
                            if (air && Math.Abs(t.fpath[k].x - t.fpath[k - 1].x) != (t.states[k] == "HopL" ? EST_FLEA_LONG_VX : VIDEO_FLEA_SHORT_VX)) badVx++;
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
                        Expect("ngang 2.25 px/tick mọi cú (tick sai)", 0, badVx);
                        Expect("nghỉ 4–16 tick giữa 2 cú (lần sai)", 0, badPause);
                        ExpectRange("nhảy vọt qua đầu Simon (lần)", 1, 999, crossed);
                        measures.Add("vào tầm tick " + kIn + ", ngồi thấp " + kG + ", bật " + kH + "; cú nhảy: " + kinds);
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
                    Put(flea, 228, 176, -1);
                    Photo(10, 16);
                    finish = () => KillChecks(eEnemyId.Fleaman, 1, WIKI_SCORE_FLEA, false);
                }
                break;
            case "bat_black":
                {
                    Go("Arena", 150, 176, 1);
                    var b = Put(bat, 300, 100, -1) as Bat;
                    b.ForcePerch();
                    Photo(60, 100, 130, 160, 220);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Bat);
                        if (t == null) return;
                        int kS = t.states.IndexOf("Swoop"), kF = t.states.IndexOf("Glide");
                        int simX = kS >= 0 ? recs[t.spawnRel + kS].sx : -1;
                        Expect("rời chỗ treo khi |dx| < 96: Simon X = 300 − 95", 300 - (VIDEO_BAT_PERCH - 1), simX);
                        Expect("treo yên trước đó (tick đổi vị trí)", 0, CountWhere(t, k => k < kS && t.path[k] != t.path[0]));
                        Expect("treo bằng bat_purple_0 (anim purple_hang)", "purple_hang", kS > 0 ? t.anims[kS - 1] : "");
                        int bad = 0, y0 = kS > 0 ? t.fpath[kS - 1].y : 0;
                        for (int k = kS; k < kF && k > 0; k++)
                        {
                            int i = k - kS + 1;
                            int want = y0 + (int)Math.Round(VIDEO_BAT_DROP * Math.Sin(0.5 * Math.PI * Math.Min(i, VIDEO_BAT_SWOOP) / VIDEO_BAT_SWOOP) * 256);
                            if (t.fpath[k].y != want || t.fpath[k].x - t.fpath[k - 1].x != -VIDEO_BAT_VX) bad++;
                        }
                        Expect("sà 31 px theo 1/4 sin + ngang 1.5 px/tick (tick sai)", 0, bad);
                        Expect("sà dài 42 tick rồi bay ngang", VIDEO_BAT_SWOOP, kF - kS);
                        int badG = 0;
                        for (int k = kF + 1; k < t.fpath.Count && kF > 0; k++)
                            if (t.fpath[k].y != t.fpath[kF].y || t.fpath[k].x - t.fpath[k - 1].x != -VIDEO_BAT_VX) badG++;
                        Expect("bay ngang thẳng 1.5 px/tick sau khi sà (tick sai)", 0, badG);
                        Expect("bay bằng anim purple_fly", "purple_fly", kF > 0 ? t.anims[kF] : "");
                        measures.Add("treo tới tick " + kS + ", sà tới tick " + kF + " (y " + (kF > 0 ? t.path[kF - 1].y : -1) + ")");
                    };
                }
                break;
            case "bat_black_high":
                {
                    Go("Arena", 200, 176, 1);
                    var b = Put(bat, 210, 88, -1) as Bat;
                    b.ForcePerch();
                    Photo(100);
                    finish = () =>
                    {
                        var t = First(eEnemyId.Bat);
                        if (t == null) return;
                        Expect("chân Simon thấp hơn dơi 88 ≥ 80 px ⇒ không rời chỗ (tick ở state khác Perch)", 0, CountWhere(t, k => t.states[k] != "Perch"));
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
                        Expect("Stopwatch đóng băng (tick) — video Map_2", VIDEO_STOPWATCH, f1 - f0);
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
            if (t != null) { t.hits++; t.hitRels.Add(rel); }
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
            t.frames.Add(en.Anim != null ? en.Anim.FrameIndex : 0);
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

    /// <summary>CVM3 08: mỗi tick Glide dời ngang đúng ±0.75; tick Glide đầu |Δy| ∈ {1.25, 1.75, 2.0}; tick Glide liền nhau |Δy| giảm đúng 9/256.</summary>
    static void RavenGlideChecks(Track t)
    {
        int badX = 0, badD = 0, badV = 0, n = 0;
        for (int k = 1; k < t.fpath.Count; k++)
        {
            if (t.states[k] != "Glide") continue;
            n++;
            int dx = Math.Abs(t.fpath[k].x - t.fpath[k - 1].x), dy = Math.Abs(t.fpath[k].y - t.fpath[k - 1].y);
            if (dx != VIDEO_RAVEN_VX) badX++;
            if (t.states[k - 1] != "Glide")
            {
                if (Array.IndexOf(VIDEO_RAVEN_VY0, dy) < 0) badV++;
            }
            else if (k >= 2 && Math.Abs(t.fpath[k - 1].y - t.fpath[k - 2].y) - dy != VIDEO_RAVEN_DECEL) badD++;
        }
        Expect("lướt: ngang đúng 0.75 px/tick (tick sai / " + n + ")", 0, badX);
        Expect("lướt: vy0 thuộc {1.25, 1.75, 2.0} (chặng sai)", 0, badV);
        Expect("lướt: |vy| giảm đúng 9/256 mỗi tick (tick sai)", 0, badD);
    }

    /// <summary>Số đoạn liền mạch của state.</summary>
    static int Runs(Track t, string st)
    {
        int n = 0;
        for (int k = 0; k < t.states.Count; k++)
            if (t.states[k] == st && (k == 0 || t.states[k - 1] != st)) n++;
        return n;
    }

    /// <summary>Độ dài (tick) đoạn liền mạch thứ i của state (−1 nếu không có).</summary>
    static int RunLen(Track t, string st, int i)
    {
        int r = -1;
        for (int k = 0; k < t.states.Count; k++)
            if (t.states[k] == st && (k == 0 || t.states[k - 1] != st))
            {
                r++;
                if (r == i)
                {
                    int len = 0;
                    while (k + len < t.states.Count && t.states[k + len] == st) len++;
                    return len;
                }
            }
        return -1;
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

    /// <summary>CVM2 08: sau mỗi đòn chưa hạ enemy đứng yên + anim dừng đúng `stun` tick (video Map_2 Spear Knight / Ghost).</summary>
    static void StunChecks(eEnemyId id, int stun)
    {
        var t = First(id);
        if (t == null || t.hitRels.Count == 0) return;
        int bad = 0;
        var sb = new StringBuilder("khựng sau đòn (tick đứng yên): ");
        for (int h = 0; h < t.hitRels.Count; h++)
        {
            if (t.defeated && h == t.hitRels.Count - 1) break;   // đòn hạ: không khựng
            int k0 = t.hitRels[h] - t.spawnRel;
            int still = 0;
            for (int k = k0 + 1; k < t.fpath.Count && t.fpath[k] == t.fpath[k0] && t.frames[k] == t.frames[k0]; k++) still++;
            sb.Append(still).Append(' ');
            if (still < stun) bad++;
        }
        measures.Add(sb.ToString());
        Expect("đứng yên + anim dừng ≥ " + stun + " tick sau đòn chưa hạ (đòn sai)", 0, bad);
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

    static void Expect(string what, string want, string got)
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
