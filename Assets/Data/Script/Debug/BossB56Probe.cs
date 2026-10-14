#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness boss Block 5–6 (CVGP 20; routine 21 chạy lại) — CHỈ Editor, scene Arena (Player = Simon, StageWorld.itemCatalog). Qua MCP khi Play:
///   BossB56Probe.SaveReplays()                         ⇒ Assets/Data/Tests/Replays/boss_b56_&lt;tên&gt;.txt (kịch bản viết tay)
///   BossB56Probe.Record(BossB56Probe.PilotNames())     ⇒ AUTOPILOT (chỉ đọc trạng thái, bấm nút qua NesPad.Inject): Death bằng RÌU (wiki khuyên
///                                                        Axe), Dracula 2 dạng bằng roi cấp 3 (wiki: roi vào đầu) tới khi nhặt quả cầu ⇒ GHI replay
///   BossB56Probe.RunSuite(BossB56Probe.ScenarioNames()) ⇒ chạy (input ĐỌC TỪ FILE), ghi từng tick, TỰ CHẤM ⇒ SuiteReport()
///   BossB56Probe.ShotDir = thư mục ⇒ chụp Game view ở các tick cố định (≥ 6 ảnh/boss/pha)
/// Phòng thử: BossRoomSetup.DEATH / DRACULA (Arena boss rooms, camera khoá cx 0). Không có video Block 5–6 ⇒ kỳ vọng = hằng WIKI_* (đòn, điểm,
/// sát thương theo stage, 3 lưỡi hái, 3 cầu lửa) + EST_* (ước lượng routine 20 chép ĐỘC LẬP Config — đổi Config mà quên đổi đây thì bộ kiểm báo).
/// </summary>
public static class BossB56Probe
{
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";

    // ---------------------------------------------------------------- kỳ vọng
    public const int WIKI_DEATH_W = 24, WIKI_DEATH_S = 16, WIKI_SCORE_DEATH = 7000, WIKI_MAX_SCYTHES = 3;
    public const int WIKI_DRAC_W = 24, WIKI_DRAC_S = 16, WIKI_DRAC2_S = 32, WIKI_SCORE_DRAC = 50000, WIKI_VOLLEY = 3, WIKI_PROJ_SCORE = 100;
    public const int WIKI_DMG_ST15 = 4, WIKI_DMG_ST18 = 4;   // wiki Gameplay#Damage (Stage 13–18 = 4 vạch)
    public const int EST_DRAC2_W = 48;                       // tỉ lệ 3:2 (§4)
    public const int EST_DEATH_WAKE = 32, EST_DEATH_V = 128, EST_HOVER_MIN = 96, EST_HOVER_MAX = 160, EST_SUMMON_FIRST = 16, EST_SUMMON_GAP = 32;
    public const int EST_CORNER_XL = 40, EST_CORNER_XR = 216, EST_CORNER_YH = 88, EST_CORNER_YL = 152;
    public const int EST_SCYTHE_IDLE = 60, EST_SCYTHE_ACCEL = 4, EST_SCYTHE_CAP0 = 128, EST_SCYTHE_GROW = 1, EST_SCYTHE_CAPMAX = 384;
    public const int EST_D1_FIRST = 60, EST_D1_APPEAR = 32, EST_D1_STAND = 24, EST_D1_FIRE_AT = 8, EST_D1_CAPE = 40, EST_D1_CLOSE = 16;
    public const int EST_D1_VANISH = 24, EST_D1_HIDDEN_MIN = 48, EST_D1_HIDDEN_MAX = 96, EST_D1_FIRE_UP = 40, EST_BALL_V = 384, EST_SPREAD_DEG = 14;
    public const int EST_D1_TRANSFORM = 96, EST_HEAD_VY = -384, EST_HEAD_VX = 128, EST_D2_EMERGE = 48, EST_D2_CROUCH = 12, EST_D2_LAND = 12;
    public const int EST_D2_HIGH_RISE = 58, EST_D2_LOW_RISE = 17, EST_D2_HIGH_VX = 512, EST_D2_LOW_VX = 384, EST_D2_MOUTH_UP = 39, EST_D2_MOUTH_DX = 8;
    public const int EST_D2_MINX = 32, EST_D2_MAXX = 224, EST_HIT_STUN = 10;
    public const int STOPWATCH_TICKS = 300, PILOT_MAX_TICKS = 9000;
    public static readonly RectInt D1_HEAD = new RectInt(-6, -58, 8, 14), D1_BODY = new RectInt(-12, -58, 24, 58);

    private struct Scn
    {
        public string name, rle, note;
        public bool pilot;
        public Scn(string n, string r, string note, bool pilot = false) { name = n; rle = r; this.note = note; this.pilot = pilot; }
    }

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("death_pattern", "-*1100", "Death: hiện giữa-trên (128,48) 32 tick không va chạm ⇒ bay 0.5 px/tick vào góc trên-phải (216,88) ⇒ dừng 96–160 gọi lưỡi hái (tick 16, rồi mỗi 32, ≤ 3 sống) ⇒ 75 % sang góc khác; lưỡi hái đứng 60 tick rồi đuổi Simon, mỗi trục ±1/64 px/tick², trần 0.5 → 1.5"),
        new Scn("death_touch", "-*900", "Stage 15, Simon không bất tử đứng góc dưới-trái: lưỡi hái / thân Death chạm ⇒ −4 vạch/lần (wiki: mọi đòn boss = mức stage)"),
        new Scn("death_scythe_whip", "-*4 B -*30", "Arena: lưỡi hái đứng trước mặt Simon 28 px ngang tầm roi ⇒ roi da 1 đòn phá, +100 (đạn phá được — suy luận 17)"),
        new Scn("death_stopwatch", "-*100 Sub -*420", "Stopwatch tick 101: Death + lưỡi hái KHÔNG bị giữ (wiki: chỉ Phantom Bat + Medusa)"),
        new Scn("death_kill_axe", "", "AUTOPILOT rìu (wiki khuyên Axe): 16 đòn MẠNH (wiki 24/16), +7000, lưỡi hái VẪN CÒN sau khi Death chết (wiki), quả cầu sau 152, nhặt ⇒ lưỡi hái biến mất + tally", true),
        new Scn("drac1_pattern", "-*1000", "Dracula dạng 1, Simon (128,176) đứng giữa sàn: ẩn 60 ⇒ hiện dần 32 (không va chạm) ⇒ đứng 24 ⇒ mở áo, tick 8 bắn 3 cầu lửa (giữa nhắm Simon, ±14°, 1.5 px/tick) ⇒ áo mở 40 ⇒ khép 16 ⇒ biến mất 24 ⇒ ẩn 48–96 ⇒ dịch chuyển (50 % đúng x Simon), đứng trên mặt sàn dưới cột"),
        new Scn("drac1_head_body", "-*91 B -*32 A -*1 B -*80", "Dracula hiện ép ở x 158 (Simon (128,176) quay phải): roi da ĐỨNG trúng THÂN (tick 92) ⇒ KHÔNG mất máu; NHẢY quất (A 124, B 126) trúng ĐẦU ⇒ −1 đòn yếu (wiki: chỉ đầu bị thương)"),
        new Scn("drac1_touch", "-*800", "Stage 18, Simon không bất tử đứng (128,176): cầu lửa / thân Dracula ⇒ −4 vạch/lần"),
        new Scn("drac_fireball_whip", "-*4 B -*35 -*4 B -*35 -*4 B -*35 -*4 B -*35 -*4 B -*35 -*4 B -*35 -*4 B -*35 -*4 B -*35", "Arena: 8 cầu lửa Dracula bay về Simon (mỗi 40 tick) ⇒ roi da phá cả 8 (+100 mỗi quả), phá ⇒ 25 % rơi tim nhỏ (wiki 'đôi khi sinh tim')"),
        new Scn("drac_transform", "-*400", "Dracula dạng 1 bị hạ ở tick 100 (cheat Defeat): 0 điểm, ENEMY 0, đầu bay lên 1.5 px/tick (lùi 0.5), thân không đầu 96 tick ⇒ lửa + dạng 2 sinh TẠI CHỖ, đứng trong lửa 48 tick (không va chạm) ⇒ ENEMY về 16"),
        new Scn("drac2_pattern", "-*1400", "Dracula dạng 2 (quỷ) giữa phòng, Simon (24,160) trên bậc trái: đứng 32–64 ⇒ 33 % phun 3 cầu lửa từ miệng (chân − 39) / ngồi 12 ⇒ nhảy CAO (vy0 −5.5, vx 2, đỉnh 58) hoặc THẤP (vy0 −3, vx 1.5, đỉnh 17), g 0.25, về phía Simon, ngang trong [32, 224] ⇒ đáp 12; hộp nhận = MẶT theo khung"),
        new Scn("drac2_holy", "-*54 U+B -*45 R*80 -*220", "Dracula dạng 2 (128,176), Simon (90,176) KHÔNG bất tử ném nước thánh vào chân quỷ: lửa ⇒ quỷ đứng im (choáng) + KHÔNG mất máu (wiki: chân không nhận đòn) + hộp chạm TẮT ⇒ Simon đi xuyên qua không bị thương (wiki)"),
        new Scn("drac2_stopwatch", "-*60 Sub -*420", "Stopwatch tick 61: quỷ KHÔNG bị giữ (wiki: boss sau Medusa miễn nhiễm)"),
        new Scn("drac_kill_l3", "", "AUTOPILOT roi cấp 3 (mạnh): dạng 1 16 đòn vào đầu (nhảy quất) ⇒ biến hình ⇒ dạng 2 32 đòn (wiki) ⇒ +50 000, quả cầu CUỐI sau 152, nhặt ⇒ GameRules.OnGameCleared + tally", true),
    };

    // ---------------------------------------------------------------- ghi
    public class Track
    {
        public Entity e;
        public string kind;
        public int spawnTick, spawnRel = -1, endRel = -1, hits;
        public bool defeated;
        public readonly List<Vector2Int> fpath = new List<Vector2Int>();
        public readonly List<Vector2Int> vel = new List<Vector2Int>();
        public readonly List<string> states = new List<string>();
        public readonly List<bool> active = new List<bool>();
        public readonly List<bool> touch = new List<bool>();
        public readonly List<bool> visible = new List<bool>();
        public readonly List<string> anims = new List<string>();
        public readonly List<int> hp = new List<int>();
    }

    private struct Rec
    {
        public int sx, sy, hp, score, ebar, cx, fx, shots, hearts;
        public eSimonState state;
        public bool frozenE;
    }

    public static string Label { get; private set; } = "";
    public static bool Running { get; private set; }
    public static bool Recording { get; private set; }
    public static int Fails { get; private set; }
    public static uint Hash { get; private set; }
    public static string ShotDir;
    public static readonly Dictionary<string, uint> RecordHashes = new Dictionary<string, uint>();
    private static readonly List<Rec> recs = new List<Rec>();
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> measures = new List<string>();
    private static readonly List<Track> tracks = new List<Track>();
    private static readonly List<string> playerHits = new List<string>();
    private static readonly List<int> playerDmg = new List<int>();
    private static readonly List<int> bars = new List<int>();
    private static readonly List<string> bossHits = new List<string>();
    private static readonly Queue<string> suite = new Queue<string>();
    public static readonly List<string> SuiteReports = new List<string>();
    public static bool SuiteDone { get; private set; } = true;
    public static int SuiteFails { get; private set; }
    public static uint SuiteHash { get; private set; }
    private static StageWorld world;
    private static int target, recorded, startTick, savedStage, defeatRel, crystalRel, pickRel, brokenShots, clearedRel, clearedCount, phase1EndRel;
    private static Action finish;
    private static Action<int> onTick;
    private static readonly List<GameObject> temp = new List<GameObject>();
    private static readonly Dictionary<int, string> shots = new Dictionary<int, string>();
    private static string current;
    private static BossArena arena;
    private static StageRules rules;
    private static bool addedRules, savedAutoRespawn = true, pilotOn;
    private static ItemPickup crystal;
    private static int pilotPrev, pilotEnd, pilotPendingB, heartDrops;
    private static readonly List<Vector2Int> headPath = new List<Vector2Int>();
    private static int headBoxBad, headBoxChecked;

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string ReplayPath(string name) { return REPLAY_DIR + "/boss_b56_" + name + ".txt"; }

    static Scn Find(string name)
    {
        foreach (var s in SCENARIOS) if (s.name == name) return s;
        return new Scn(null, null, null);
    }

    /// <summary>Ghi file replay cho kịch bản viết tay (kịch bản autopilot ghi bằng Record).</summary>
    public static int SaveReplays()
    {
        Directory.CreateDirectory(REPLAY_DIR);
        int n = 0;
        foreach (var s in SCENARIOS)
        {
            if (s.pilot) continue;
            WriteReplay(s.name, NesReplay.Decode(s.rle), s.note);
            n++;
        }
        UnityEditor.AssetDatabase.Refresh();
        return n;
    }

    static void WriteReplay(string name, List<int> ticks, string note)
    {
        var sb = new StringBuilder();
        sb.Append("# ").Append(ticks.Count).Append(" tick, hash ").Append(NesReplay.Hash(ticks).ToString("x8")).Append('\n');
        sb.Append("# CVGP 20 BossB56Probe kịch bản '").Append(name).Append("': ").Append(note).Append('\n');
        sb.Append(NesReplay.Encode(ticks)).Append('\n');
        File.WriteAllText(ReplayPath(name), sb.ToString(), new UTF8Encoding(false));
    }

    public static string ScenarioNames() { return Names(false, true); }
    public static string PilotNames() { return Names(true, false); }

    static string Names(bool pilotOnly, bool all)
    {
        var sb = new StringBuilder();
        foreach (var s in SCENARIOS)
            if (all || s.pilot == pilotOnly)
                sb.Append(sb.Length > 0 ? "," : "").Append(s.name);
        return sb.ToString();
    }

    public static int RunSuite(string names) { return StartQueue(names, false); }

    /// <summary>Chạy autopilot các kịch bản *_kill_* và GHI replay (dừng khi nhặt quả cầu + 60 tick).</summary>
    public static int Record(string names) { return StartQueue(names, true); }

    static int StartQueue(string names, bool record)
    {
        Unhook();
        suite.Clear();
        SuiteReports.Clear();
        SuiteFails = 0;
        SuiteHash = 2166136261u;
        Recording = record;
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
        bool wasRec = Recording;
        Recording = false;
        CvgpDebug.CheatInvincible = false;
        BackToArena();
        if (wasRec)
            UnityEditor.AssetDatabase.Refresh();
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
        sb.Append("BOSS B5-6 SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(Recording ? " (GHI autopilot)" : "").Append(", ").Append(SuiteReports.Count)
          .Append(" kịch bản, FAIL ").Append(SuiteFails).Append(", hash bộ ").Append(SuiteHash.ToString("x8")).Append('\n');
        foreach (var r in SuiteReports)
            sb.Append(r).Append('\n');
        return sb.ToString();
    }

    // ---------------------------------------------------------------- dựng

    static T Prefab<T>(string name) where T : Entity
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(BossRoomSetup.BOSS_DIR + name + ".prefab");
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

    static void Room(BossRoomSetup.Room r)
    {
        GameObject host;
        arena = BossRoomSetup.Enter(world, r, true, out host);
        if (host != null)
            temp.Add(host);
        world.ResetRng();
    }

    static BossRoomSetup.Room Demon()
    {
        var r = BossRoomSetup.DRACULA;
        r.boss = "DraculaPhase2";
        return r;
    }

    static void PlaceSimon(int x, int y, int facing)
    {
        Simon.Teleport(x, y, facing);
        if (world.CameraCtl != null)
            world.CameraCtl.SnapToTarget();
    }

    static void AddRules()
    {
        rules = world.gameObject.GetComponent<StageRules>();
        if (rules == null)
        {
            savedAutoRespawn = Simon.AutoRespawn;   // StageRules tắt autoRespawn của Simon và không trả lại
            rules = world.gameObject.AddComponent<StageRules>();
            addedRules = true;
        }
    }

    static void Photo(params int[] rels)
    {
        if (string.IsNullOrEmpty(ShotDir))
            return;
        foreach (var rel in rels)
            shots[rel] = Path.Combine(ShotDir, current + "_r" + rel.ToString("0000") + ".png");
    }

    static T Boss<T>() where T : BossBase
    {
        foreach (var e in world.GroupList(eEntityGroup.Enemy))
        {
            var b = e as T;
            if (b != null && b.Alive) return b;
        }
        return null;
    }

    // ---------------------------------------------------------------- 1 kịch bản

    public static int Run(string scenario)
    {
        world = StageWorld.Current;
        var s = Simon;
        if (world == null || s == null || world.Items == null)
        {
            Debug.LogError("[BossB56Probe] cần scene Arena với Player = SimonController + StageWorld.itemCatalog");
            return -1;
        }
        var sc = Find(scenario);
        if (sc.name == null)
        {
            Debug.LogError("[BossB56Probe] không có kịch bản " + scenario);
            return -1;
        }
        if (Recording && !sc.pilot)
            return -1;
        bool pilot = sc.pilot && Recording;
        List<int> ticks = null;
        if (!pilot)
        {
            string p = ReplayPath(scenario);
            if (!File.Exists(p))
            {
                Debug.LogError("[BossB56Probe] thiếu " + p + (sc.pilot ? " — gọi BossB56Probe.Record(\"" + scenario + "\")" : " — gọi BossB56Probe.SaveReplays()"));
                return -1;
            }
            ticks = NesReplay.Decode(File.ReadAllText(p));
        }
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
        ses.enemyHp = GameSession.MAX_HP;
        ses.NotifyChanged();
        finish = null;
        onTick = null;
        pilotOn = false;
        headPath.Clear();
        headBoxBad = headBoxChecked = 0;
        CvgpDebug.CheatInvincible = true;
        world.FreezeEnemies(0);
        world.ResetRng();
        switch (scenario)
        {
            // ======================================================= Death
            case "death_pattern":
                Room(BossRoomSetup.DEATH);
                Photo(10, 60, 150, 250, 330, 420, 520, 700, 900, 1080);
                finish = DeathPatternChecks;
                break;
            case "death_touch":
                Room(BossRoomSetup.DEATH);
                CvgpDebug.CheatInvincible = false;
                PlaceSimon(24, 176, 1);
                Photo(400, 600);
                finish = () => DamageChecks(WIKI_DMG_ST15, 1);
                break;
            case "death_scythe_whip":
                {
                    Go("Arena", 100, 176, 1);
                    world.Stage = 15;
                    world.Spawn(Prefab<DeathScythe>("DeathScythe"), 128, 156, -1);
                    Photo(2, 18, 24);
                    finish = () =>
                    {
                        var t = First("DeathScythe");
                        if (t == null) return;
                        var sy = t.e as DeathScythe;
                        Expect("lưỡi hái bị phá (1 đòn roi — wiki)", 1, sy != null && sy.broken && t.endRel >= 0 ? 1 : 0);
                        Expect("lưỡi hái đứng yên trước khi bị phá (tick di chuyển — đứng 60)", 0, CountK(t, k => k > 0 && t.fpath[k] != t.fpath[k - 1]));
                        Expect("+100 (đạn phá được)", WIKI_PROJ_SCORE, Last.score);
                        measures.Add("lưỡi hái vỡ ở tick " + t.endRel);
                    };
                }
                break;
            case "death_stopwatch":
                Room(BossRoomSetup.DEATH);
                ses.subWeapon = eSubWeapon.Stopwatch;
                ses.hearts = 10;
                Photo(200, 350);
                finish = () =>
                {
                    int f0 = FirstRel(0, r => r.frozenE);
                    ExpectRange("Stopwatch bắt đầu (tick — bấm 101 + ném)", 100, 120, f0);
                    var d = First("GrimReaper");
                    if (d == null || f0 < 0) return;
                    int dm = 0, sm = 0;
                    for (int rel = f0 + 1; rel < f0 + STOPWATCH_TICKS && rel < recs.Count; rel++)
                        if (PosAt(d, rel) != PosAt(d, rel - 1)) dm++;
                    foreach (var t in Tracks("DeathScythe"))
                        for (int rel = Math.Max(f0 + 1, t.spawnRel + 1); rel < f0 + STOPWATCH_TICKS && rel < recs.Count; rel++)
                            if (PosAt(t, rel) != PosAt(t, rel - 1) && PosAt(t, rel).x > -9999) sm++;
                    ExpectRange("Death vẫn bay khi Stopwatch (tick di chuyển)", 50, 9999, dm);
                    ExpectRange("lưỡi hái vẫn đuổi khi Stopwatch (tick-con di chuyển)", 10, 99999, sm);
                    measures.Add("Stopwatch tick " + f0 + ": Death di chuyển " + dm + " tick, lưỡi hái " + sm + " tick-con");
                };
                break;
            case "death_kill_axe":
                Room(BossRoomSetup.DEATH);
                AddRules();
                ses.subWeapon = eSubWeapon.Axe;
                ses.hearts = 99;
                ses.NotifyChanged();
                onTick = rel => { if (GameSession.Current.hearts < 90) { GameSession.Current.hearts = 99; GameSession.Current.NotifyChanged(); } };
                Photo(100, 400, 900);
                finish = DeathKillChecks;
                break;

            // ======================================================= Dracula dạng 1
            case "drac1_pattern":
                Room(BossRoomSetup.DRACULA);
                PlaceSimon(128, 176, 1);
                Photo(60, 75, 95, 125, 140, 160, 180, 300, 500, 700);
                finish = Drac1PatternChecks;
                break;
            case "drac1_head_body":
                {
                    Room(BossRoomSetup.DRACULA);
                    PlaceSimon(128, 176, 1);
                    var d = Boss<DraculaPhase1>();
                    if (d != null) d.debugForceX = 158;
                    Photo(100, 108, 140, 145);
                    finish = () =>
                    {
                        var t = First("DraculaPhase1");
                        if (t == null) return;
                        var dd = t.e as DraculaPhase1;
                        int full = t.hp[0];
                        // roi đứng: tick đòn duỗi 105–113 (bấm 92); trước khi nhảy (tick 123) máu phải còn đủ
                        Expect("Dracula hiện đúng x ép 158", 158, Fixed.ToPx(t.fpath[t.fpath.Count - 1].x));
                        Expect("roi ĐỨNG chạm THÂN (tick 92–123): máu không đổi", full, t.hp[Math.Min(123, t.hp.Count - 1)]);
                        int bodyOverlap = 0;
                        foreach (var w in whipRects) if (w.rel >= 92 && w.rel <= 123 && CombatResolver.Overlaps(w.r, w.body)) bodyOverlap++;
                        ExpectRange("hộp roi đứng THỰC SỰ chồng hộp thân (tick)", 1, 99, bodyOverlap);
                        Expect("số đòn trúng Dracula (chỉ cú nhảy quất vào đầu)", 1, t.hits);
                        Expect("máu sau cú quất đầu = HP − 1 đòn yếu (CombatHealth 24/16 ⇒ −16)", full - 16, t.hp[t.hp.Count - 1]);
                        Expect("hộp nhận đòn = đầu (−6,−58,8,14)", 1, dd != null && dd.Hurt.local.Equals(D1_HEAD) ? 1 : 0);
                        measures.Add("đòn vào Dracula: " + string.Join(" ", bossHits) + "; chồng thân " + bodyOverlap + " tick");
                    };
                }
                break;
            case "drac1_touch":
                Room(BossRoomSetup.DRACULA);
                CvgpDebug.CheatInvincible = false;
                PlaceSimon(128, 176, 1);
                Photo(122, 300);
                finish = () => DamageChecks(WIKI_DMG_ST18, 1);
                break;
            case "drac_fireball_whip":
                {
                    Go("Arena", 100, 176, 1);
                    world.Stage = 18;
                    var prefab = Prefab<DraculaFireball>("DraculaFireball");
                    onTick = rel =>
                    {
                        if (rel % 40 == 39 && rel < 320)
                        {
                            var f = world.Spawn(prefab, 160, 156, -1) as DraculaFireball;
                            if (f != null) f.vx = -EST_BALL_V;
                        }
                    };
                    // quả đầu sinh trước tick 1
                    var f0 = world.Spawn(prefab, 160, 156, -1) as DraculaFireball;
                    if (f0 != null) f0.vx = -EST_BALL_V;
                    Photo(14, 20, 60);
                    finish = () =>
                    {
                        var balls = Tracks("DraculaFireball");
                        int hearts = heartDrops;
                        Expect("cầu lửa sinh", 8, balls.Count);
                        Expect("roi da phá cả 8 quả (đòn phá ghi lúc trúng — thực thể tái dùng)", 8, brokenShots);
                        Expect("+100 mỗi quả phá", 8 * WIKI_PROJ_SCORE, Last.score);
                        ExpectRange("phá ⇒ đôi khi rơi tim nhỏ (25 % — số quả rơi)", 1, 7, hearts);
                        Expect("Simon không trúng cầu lửa", 0, playerDmg.Count);
                        measures.Add(brokenShots + " cầu lửa phá, " + hearts + " tim rơi (RNG thế giới)");
                    };
                }
                break;
            case "drac_transform":
                Room(BossRoomSetup.DRACULA);
                PlaceSimon(128, 176, 1);
                onTick = rel =>
                {
                    var d = Boss<DraculaPhase1>();
                    if (rel == 100 && d != null) d.Defeat();
                    if (d != null && d.headFlying) headPath.Add(new Vector2Int(d.headFx, d.headFy));
                };
                Photo(99, 102, 120, 150, 180, 196, 200, 230, 260, 320);
                finish = TransformChecks;
                break;

            // ======================================================= Dracula dạng 2
            case "drac2_pattern":
                Room(Demon());
                PlaceSimon(24, 160, 1);
                onTick = rel => CheckHeadBox();
                Photo(20, 60, 100, 160, 220, 300, 420, 560, 700, 900, 1100, 1300);
                finish = Drac2PatternChecks;
                break;
            case "drac2_holy":
                Room(Demon());
                CvgpDebug.CheatInvincible = false;
                PlaceSimon(90, 176, 1);
                ses.subWeapon = eSubWeapon.HolyWater;
                ses.hearts = 20;
                ses.NotifyChanged();
                Photo(80, 100, 120, 140, 160);
                finish = HolyChecks;
                break;
            case "drac2_stopwatch":
                Room(Demon());
                PlaceSimon(24, 160, 1);
                ses.subWeapon = eSubWeapon.Stopwatch;
                ses.hearts = 10;
                Photo(200);
                finish = () =>
                {
                    int f0 = FirstRel(0, r => r.frozenE);
                    ExpectRange("Stopwatch bắt đầu (tick — bấm 61 + ném)", 60, 80, f0);
                    var d = First("DraculaPhase2");
                    if (d == null || f0 < 0) return;
                    int dm = 0, sc2 = 0;
                    for (int rel = f0 + 1; rel < f0 + STOPWATCH_TICKS && rel < recs.Count; rel++)
                    {
                        if (PosAt(d, rel) != PosAt(d, rel - 1)) dm++;
                        int k = rel - d.spawnRel;
                        if (k > 0 && k < d.states.Count && d.states[k] != d.states[k - 1]) sc2++;
                    }
                    ExpectRange("quỷ vẫn hành động khi Stopwatch (đổi state)", 3, 9999, sc2);
                    ExpectRange("quỷ vẫn di chuyển khi Stopwatch (tick)", 10, 9999, dm);
                    measures.Add("Stopwatch tick " + f0 + ": quỷ di chuyển " + dm + " tick, đổi state " + sc2 + " lần");
                };
                break;
            case "drac_kill_l3":
                Room(BossRoomSetup.DRACULA);
                AddRules();
                PlaceSimon(128, 176, 1);
                ses.whipLevel = 3;
                ses.NotifyChanged();
                Photo(100, 600, 1500);
                finish = DraculaKillChecks;
                break;
            default:
                Debug.LogError("[BossB56Probe] không có kịch bản " + scenario);
                return -1;
        }
        s.ResetStats();

        if (pilot)
        {
            pilotOn = true;
            pilotPrev = 0;
            pilotEnd = -1;
            pilotPendingB = 0;
            NesPad.I.StopReplay();
            NesPad.I.StartRecord();
            Begin(scenario, PILOT_MAX_TICKS);
            return PILOT_MAX_TICKS;
        }
        NesPad.I.PlayReplay(ticks);
        Begin(scenario, ticks.Count);
        return ticks.Count;
    }

    // ---------------------------------------------------------------- chấm từng kịch bản

    static void DeathPatternChecks()
    {
        var d = First("GrimReaper");
        if (d == null) return;
        int wake = FirstK(d, k => d.states[k] != "Enter");
        Expect("hiện giữa-trên (128,48) — vị trí sinh", 1, PosAt(d, d.spawnRel) == new Vector2Int(128, 48) ? 1 : 0);
        Expect("đứng chờ tới tick (wakeDelay 32)", EST_DEATH_WAKE - 1, wake);
        Expect("không va chạm trước khi thức (tick hộp bật)", 0, CountK(d, k => k < wake && d.active[k]));
        Expect("va chạm từ lúc thức (tick hộp tắt)", 0, CountK(d, k => k >= wake && !d.active[k]));
        int h0 = FirstK(d, k => d.states[k] == "Hover");
        Expect("điểm dừng đầu = góc trên-phải (216,88)", 1, h0 >= 0 && Fixed.ToPx(d.fpath[h0].x) == EST_CORNER_XR && Fixed.ToPx(d.fpath[h0].y) == EST_CORNER_YH ? 1 : 0);
        int badV = 0, badStop = 0, stops = 0, corners = 0, badHover = 0, run = 0;
        var hovers = new List<int>();
        for (int k = 1; k < d.fpath.Count; k++)
        {
            int dx = Math.Abs(d.fpath[k].x - d.fpath[k - 1].x), dy = Math.Abs(d.fpath[k].y - d.fpath[k - 1].y);
            if (d.states[k] == "Move" && Math.Max(dx, dy) > EST_DEATH_V) badV++;
            if (d.states[k] == "Hover")
            {
                run++;
                if (d.states[k - 1] != "Hover")
                {
                    stops++;
                    int x = Fixed.ToPx(d.fpath[k].x), y = Fixed.ToPx(d.fpath[k].y);
                    if ((x == EST_CORNER_XL || x == EST_CORNER_XR) && (y == EST_CORNER_YH || y == EST_CORNER_YL)) corners++;
                    if (x < EST_CORNER_XL || x > EST_CORNER_XR || y < 64 || y > EST_CORNER_YL) badStop++;
                }
                else if (dx != 0 || dy != 0) badStop++;   // tick vào Hover = tick bước cuối của Move (state nối cùng tick)
            }
            else if (run > 0)
            {
                hovers.Add(run);
                if (run < EST_HOVER_MIN - 1 || run > EST_HOVER_MAX - 1) badHover++;
                run = 0;
            }
        }
        Expect("bay ≤ 0.5 px/tick trục trội (tick sai)", 0, badV);
        Expect("điểm dừng trong vùng [40,216]×[64,152] + đứng yên khi dừng (lỗi)", 0, badStop);
        Expect("dừng 96–160 tick (đếm tick ở state Hover = len − 1) (lần sai)", 0, badHover);
        ExpectRange("số lần dừng", 3, 99, stops);
        ExpectRange("ưu tiên góc: % lần dừng ở góc", 50, 100, stops > 0 ? corners * 100 / stops : 0);
        var sc = Tracks("DeathScythe");
        ExpectRange("số lưỡi hái gọi", 3, 99, sc.Count);
        if (sc.Count > 0) Expect("lưỡi hái đầu ở tick dừng thứ 16 (rel dừng + 15)", h0 + EST_SUMMON_FIRST - 1, sc[0].spawnRel);
        if (sc.Count > 1) Expect("lưỡi hái 2 sau 32 tick", EST_SUMMON_GAP, sc[1].spawnRel - sc[0].spawnRel);
        int maxAlive = 0;
        for (int rel = 0; rel < recs.Count; rel++)
        {
            int n = 0;
            foreach (var t in sc) if (t.spawnRel <= rel && (t.endRel < 0 || rel < t.endRel)) n++;
            maxAlive = Math.Max(maxAlive, n);
        }
        Expect("lưỡi hái sống cùng lúc tối đa (wiki 3)", WIKI_MAX_SCYTHES, maxAlive);
        int idleMoved = 0, firstMoveBad = 0, accelBad = 0, capBad = 0, homed = 0;
        int fastEarly = 0, fastLate = 0;
        foreach (var t in sc)
        {
            int k0 = FirstK(t, k => k > 0 && t.fpath[k] != t.fpath[k - 1]);
            idleMoved += CountK(t, k => k > 0 && k < EST_SCYTHE_IDLE && t.fpath[k] != t.fpath[k - 1]);
            if (k0 < 0) continue;
            homed++;
            if (k0 != EST_SCYTHE_IDLE) firstMoveBad++;
            for (int k = k0; k < t.fpath.Count; k++)
            {
                int j = k - EST_SCYTHE_IDLE;   // tick đuổi thứ j (0 = đầu)
                int vx = t.fpath[k].x - t.fpath[k - 1].x, vy = t.fpath[k].y - t.fpath[k - 1].y;
                int cap = Math.Min(EST_SCYTHE_CAPMAX, EST_SCYTHE_CAP0 + j * EST_SCYTHE_GROW);
                if (Math.Abs(vx) > cap || Math.Abs(vy) > cap) capBad++;
                if (k > k0)
                {
                    int pvx = t.fpath[k - 1].x - t.fpath[k - 2].x, pvy = t.fpath[k - 1].y - t.fpath[k - 2].y;
                    if (Math.Abs(vx - pvx) > EST_SCYTHE_ACCEL || Math.Abs(vy - pvy) > EST_SCYTHE_ACCEL) accelBad++;
                }
                int sp = Math.Max(Math.Abs(vx), Math.Abs(vy));
                if (j < 40) fastEarly = Math.Max(fastEarly, sp);
                else if (j > 200) fastLate = Math.Max(fastLate, sp);
            }
        }
        Expect("lưỡi hái đứng yên 60 tick sau khi sinh (tick di chuyển)", 0, idleMoved);
        Expect("lưỡi hái bắt đầu đuổi đúng tick 60 (cái sai)", 0, firstMoveBad);
        ExpectRange("lưỡi hái đã đuổi", 2, 99, homed);
        Expect("gia tốc mỗi trục ≤ 1/64 px/tick² (tick sai)", 0, accelBad);
        Expect("tốc độ mỗi trục ≤ trần 0.5 + t/256 ≤ 1.5 (tick sai)", 0, capBad);
        ExpectRange("nhanh dần: tốc độ tối đa sau 200 tick đuổi > 40 tick đầu (fixed)", fastEarly + 1, 9999, fastLate);
        var g = d.e as GrimReaper;
        measures.Add("thức rel " + wake + ", dừng " + stops + " lần (" + corners + " ở góc), dừng dài " + string.Join(",", hovers) + "; lưỡi hái "
                     + sc.Count + " (tối đa sống " + maxAlive + "), tốc độ tối đa 40 tick đầu " + fastEarly + "/256 → sau 200 tick " + fastLate + "/256"
                     + (g != null ? "; Death gọi " + g.summoned : ""));
    }

    static void DeathKillChecks()
    {
        KillChecks("GrimReaper", WIKI_DEATH_S, WIKI_SCORE_DEATH, 1);
        var sc = Tracks("DeathScythe");
        int aliveAtDefeat = 0, killedAtDefeat = 0, aliveAfterPick = 0;
        foreach (var t in sc)
        {
            if (defeatRel >= 0 && t.spawnRel <= defeatRel && (t.endRel < 0 || t.endRel > defeatRel)) aliveAtDefeat++;
            if (defeatRel >= 0 && t.endRel == defeatRel + 1 && !((DeathScythe)t.e).broken) killedAtDefeat++;
            if (pickRel >= 0 && (t.endRel < 0 || t.endRel > pickRel + 1)) aliveAfterPick++;
        }
        ExpectRange("lưỡi hái còn sống lúc Death bị hạ (wiki: vẫn còn)", 1, 3, aliveAtDefeat);
        Expect("lưỡi hái bị gỡ cùng lúc Death chết", 0, killedAtDefeat);
        Expect("lưỡi hái biến mất khi nhặt quả cầu (còn sau nhặt)", 0, aliveAfterPick);
        measures.Add("lưỡi hái: gọi " + sc.Count + ", sống lúc hạ " + aliveAtDefeat + ", phá " + brokenShots);
    }

    static void Drac1PatternChecks()
    {
        var d = First("DraculaPhase1");
        if (d == null) return;
        var runs = Runs(d);
        int firstAppear = FirstK(d, k => d.states[k] == "Appear");
        Expect("ẩn tới tick (lần đầu 60)", EST_D1_FIRST - 1, firstAppear);
        int badLen = 0, cycles = 0, badHidden = 0;
        var seq = new StringBuilder();
        for (int i = 0; i < runs.Count; i++)
        {
            string st = runs[i].Key;
            int len = runs[i].Value;
            if (seq.Length < 200) seq.Append(st).Append(len).Append(' ');
            if (i == runs.Count - 1) break;   // đoạn cuối bị cắt
            int want = st == "Appear" ? EST_D1_APPEAR : st == "Stand" ? EST_D1_STAND : st == "Cape" ? EST_D1_CAPE : st == "Close" ? EST_D1_CLOSE
                     : st == "Vanish" ? EST_D1_VANISH : -1;
            if (want > 0 && len != want - 1) badLen++;
            if (st == "Hidden" && i > 0 && (len < EST_D1_HIDDEN_MIN - 1 || len > EST_D1_HIDDEN_MAX - 1)) badHidden++;
            if (st == "Vanish") cycles++;
        }
        Expect("độ dài Appear/Stand/Cape/Close/Vanish = 32/24/40/16/24 (đếm len − 1) (đoạn sai)", 0, badLen);
        Expect("ẩn 48–96 giữa 2 lần hiện (lần sai)", 0, badHidden);
        ExpectRange("số chu kỳ hiện → biến mất", 3, 99, cycles);
        int badAct = CountK(d, k => (d.states[k] == "Stand" || d.states[k] == "Cape" || d.states[k] == "Close") != d.active[k]);
        Expect("chỉ nhận/gây đòn khi Stand/Cape/Close (tick sai)", 0, badAct);
        int visHidden = CountK(d, k => d.states[k] == "Hidden" && d.visible[k]);
        Expect("ẩn hẳn khi Hidden (tick vẽ)", 0, visHidden);
        int flick = CountK(d, k => d.states[k] == "Appear" && !d.visible[k]);
        ExpectRange("nhấp nháy khi hiện dần (tick ẩn trong Appear)", 8, 9999, flick);
        // vị trí hiện: trong màn, trên mặt sàn dưới cột
        int badPos = 0, onSimon = 0, appears = 0;
        for (int k = 1; k < d.fpath.Count; k++)
            if (d.states[k] == "Appear" && d.states[k - 1] == "Hidden")
            {
                appears++;
                int x = Fixed.ToPx(d.fpath[k].x), y = Fixed.ToPx(d.fpath[k].y);
                int floor = x < 48 || x >= 208 ? 160 : 176;
                if (x < 32 || x > 224 || y != floor) badPos++;
                int rel = d.spawnRel + k;
                if (Math.Abs(x - Math.Max(32, Math.Min(224, recs[rel].sx))) <= 1) onSimon++;
            }
        Expect("chỗ hiện trong [32, 224] và đứng trên mặt sàn dưới cột (lần sai)", 0, badPos);
        ExpectRange("số lần hiện", 4, 99, appears);
        ExpectRange("hiện ĐÚNG x Simon (wiki 'ngay trên đầu nếu đứng yên') — lần", 1, appears, onSimon);
        // cầu lửa: 3 quả / loạt ở tick 8 của áo mở, giữa nhắm Simon, ±14°, 1.5 px/tick, từ ngực
        var balls = Tracks("DraculaFireball");
        var volleys = new Dictionary<int, List<Track>>();
        foreach (var b in balls)
        {
            List<Track> l;
            if (!volleys.TryGetValue(b.spawnRel, out l)) volleys[b.spawnRel] = l = new List<Track>();
            l.Add(b);
        }
        int badCount = 0, badSpeed = 0, badSpread = 0, badAim = 0, badOrigin = 0, badTime = 0;
        foreach (var kv in volleys)
        {
            if (kv.Value.Count != WIKI_VOLLEY) badCount++;
            int k = kv.Key - d.spawnRel;
            int capeStart = k;
            while (capeStart > 0 && d.states[capeStart - 1] == "Cape") capeStart--;
            if (k - capeStart != EST_D1_FIRE_AT - 1) badTime++;
            var dirs = new List<double>();
            foreach (var b in kv.Value)
            {
                if (b.fpath.Count < 2) continue;
                double vx = b.fpath[1].x - b.fpath[0].x, vy = b.fpath[1].y - b.fpath[0].y;
                if (Math.Abs(Math.Sqrt(vx * vx + vy * vy) - EST_BALL_V) > 3) badSpeed++;
                dirs.Add(Math.Atan2(vy, vx) * 180 / Math.PI);
                int ox = Fixed.ToPx(b.fpath[0].x), oy = Fixed.ToPx(b.fpath[0].y);   // tick sinh: chưa bay (thực thể sinh trong tick bị bỏ qua)
                if (Math.Abs(ox - Fixed.ToPx(d.fpath[k].x)) > 1 || Math.Abs(oy - (Fixed.ToPx(d.fpath[k].y) - EST_D1_FIRE_UP)) > 1) badOrigin++;
            }
            dirs.Sort();
            if (dirs.Count == 3)
            {
                if (Math.Abs(dirs[1] - dirs[0] - EST_SPREAD_DEG) > 1 || Math.Abs(dirs[2] - dirs[1] - EST_SPREAD_DEG) > 1) badSpread++;
                // quả giữa nhắm tâm thân Simon lúc bắn
                var r = recs[kv.Key];
                double aim = Math.Atan2((r.sy - 16) - (Fixed.ToPx(d.fpath[k].y) - EST_D1_FIRE_UP), r.sx - Fixed.ToPx(d.fpath[k].x)) * 180 / Math.PI;
                if (Math.Abs(dirs[1] - aim) > 3 && Math.Abs(Math.Abs(dirs[1] - aim) - 360) > 3) badAim++;
            }
        }
        ExpectRange("số loạt bắn", 3, 99, volleys.Count);
        Expect("3 cầu lửa mỗi loạt (wiki) (loạt sai)", 0, badCount);
        Expect("bắn ở tick 8 của áo mở (loạt sai)", 0, badTime);
        Expect("|v| = 1.5 px/tick (quả sai)", 0, badSpeed);
        Expect("xoè ±14° (loạt sai)", 0, badSpread);
        Expect("quả giữa nhắm tâm thân Simon ±3° (loạt sai)", 0, badAim);
        Expect("sinh ở ngực (chân − 40) (quả sai)", 0, badOrigin);
        measures.Add("chuỗi: " + seq + "; hiện " + appears + " lần (" + onSimon + " đúng x Simon), loạt bắn " + volleys.Count);
    }

    static void TransformChecks()
    {
        var p1 = First("DraculaPhase1");
        var p2s = Tracks("DraculaPhase2");
        if (p1 == null) return;
        int t0 = FirstK(p1, k => p1.states[k] == "Transform");
        Expect("biến hình bắt đầu tick sau cú hạ (rel 101)", 101, p1.spawnRel + t0);
        Expect("0 điểm khi hết máu dạng 1", 0, Last.score);
        Expect("ENEMY 0 khi hết máu dạng 1", 0, t0 >= 0 ? recs[p1.spawnRel + t0].ebar : -1);
        Expect("không va chạm khi biến hình (tick hộp bật)", 0, CountK(p1, k => k >= t0 && p1.active[k]));
        Expect("có dạng 2", 1, p2s.Count);
        if (p2s.Count == 0) return;
        var p2 = p2s[0];
        Expect("dạng 2 sinh sau 96 tick biến hình (rel)", p1.spawnRel + t0 + EST_D1_TRANSFORM - 1, p2.spawnRel);
        Expect("dạng 1 bị gỡ cùng tick dạng 2 sinh (rel kết thúc)", p2.spawnRel, p1.endRel);
        Expect("dạng 2 sinh TẠI CHỖ dạng 1 (x)", Fixed.ToPx(p1.fpath[p1.fpath.Count - 1].x), Fixed.ToPx(p2.fpath[0].x));
        Expect("dạng 2 sinh TẠI CHỖ dạng 1 (y)", Fixed.ToPx(p1.fpath[p1.fpath.Count - 1].y), Fixed.ToPx(p2.fpath[0].y));
        int emerge = FirstK(p2, k => p2.states[k] != "Emerge");
        Expect("quỷ đứng trong lửa (tick — 48; sinh giữa tick ⇒ k 0 chưa chạy)", EST_D2_EMERGE, emerge);
        Expect("quỷ không va chạm khi Emerge (tick hộp bật)", 0, CountK(p2, k => k < emerge && p2.active[k]));
        int dec = 0, refill = 0;
        for (int i = 1; i < bars.Count; i++) { if (bars[i] < bars[i - 1]) dec++; else refill++; }
        Expect("thanh ENEMY 16 → 0 → 16 (chuỗi)", 1, bars.Count == 3 && bars[0] == 16 && bars[1] == 0 && bars[2] == 16 ? 1 : 0);
        int badHead = 0;
        for (int i = 1; i < headPath.Count; i++)
            if (headPath[i].y - headPath[i - 1].y != EST_HEAD_VY || Math.Abs(headPath[i].x - headPath[i - 1].x) != EST_HEAD_VX) badHead++;
        ExpectRange("đầu bay (tick)", 40, 200, headPath.Count);
        Expect("đầu bay lên 1.5 px/tick, lùi 0.5 (tick sai)", 0, badHead);
        measures.Add("biến hình rel " + (p1.spawnRel + t0) + " → dạng 2 rel " + p2.spawnRel + " ở (" + Fixed.ToPx(p2.fpath[0].x) + "," + Fixed.ToPx(p2.fpath[0].y)
                     + "); đầu bay " + headPath.Count + " tick; vạch ENEMY " + string.Join(",", bars));
    }

    static void Drac2PatternChecks()
    {
        var d = First("DraculaPhase2");
        if (d == null) return;
        var dd = d.e as DraculaPhase2;
        int emerge = FirstK(d, k => d.states[k] != "Emerge");
        Expect("đứng trong lửa (Emerge) tới tick", EST_D2_EMERGE - 1, emerge);
        var runs = Runs(d);
        int badCrouch = 0, badLand = 0, highs = 0, lows = 0, badRise = 0, badVx = 0, badX = 0, badLandY = 0;
        var rises = new StringBuilder();
        int k = 0;
        for (int i = 0; i < runs.Count; i++)
        {
            string st = runs[i].Key;
            int len = runs[i].Value;
            bool last = i == runs.Count - 1;
            if (!last && st == "Crouch" && len != EST_D2_CROUCH - 1) badCrouch++;
            if (!last && st == "Land" && len != EST_D2_LAND - 1) badLand++;
            if ((st == "JumpH" || st == "JumpL") && !last)
            {
                bool high = st == "JumpH";
                if (high) highs++; else lows++;
                int y0 = Fixed.ToPx(d.fpath[k - 1].y), top = y0;
                for (int j = k; j < k + len; j++)
                {
                    top = Math.Min(top, Fixed.ToPx(d.fpath[j].y));
                    int vx = Math.Abs(d.fpath[j].x - d.fpath[j - 1].x);
                    if (vx > (high ? EST_D2_HIGH_VX : EST_D2_LOW_VX)) badVx++;
                }
                int rise = y0 - top;
                if (rise != (high ? EST_D2_HIGH_RISE : EST_D2_LOW_RISE)) badRise++;
                if (rises.Length < 120) rises.Append(high ? "H" : "L").Append(rise).Append(' ');
                int ly = Fixed.ToPx(d.fpath[k + len].y), lx = Fixed.ToPx(d.fpath[k + len].x);
                if (ly != (lx < 48 || lx >= 208 ? 160 : 176)) badLandY++;
            }
            k += len;
        }
        for (int j = 0; j < d.fpath.Count; j++)
        {
            int x = Fixed.ToPx(d.fpath[j].x);
            if (x < EST_D2_MINX || x > EST_D2_MAXX) badX++;
        }
        ExpectRange("nhảy CAO", 1, 99, highs);
        ExpectRange("nhảy THẤP", 1, 99, lows);
        Expect("đỉnh cú nhảy cao 58 / thấp 17 px (cú sai)", 0, badRise);
        Expect("ngang ≤ 2 (cao) / 1.5 (thấp) px/tick (tick sai)", 0, badVx);
        Expect("trong [32, 224] (tick ra ngoài)", 0, badX);
        Expect("ngồi lấy đà 12 / đáp 12 (đoạn sai)", 0, badCrouch + badLand);
        Expect("đáp đúng mặt sàn dưới cột (cú sai)", 0, badLandY);
        var balls = Tracks("DraculaFireball");
        ExpectRange("phun lửa (số loạt)", 1, 99, dd != null ? dd.volleys : 0);
        Expect("3 cầu lửa mỗi loạt", (dd != null ? dd.volleys : 0) * WIKI_VOLLEY, balls.Count);
        int badMouth = 0;
        foreach (var b in balls)
        {
            int rel = b.spawnRel, kk = rel - d.spawnRel;
            if (b.fpath.Count < 2 || kk < 0) continue;
            int ox = Fixed.ToPx(b.fpath[0].x), oy = Fixed.ToPx(b.fpath[0].y);
            int face = recs[rel].sx > Fixed.ToPx(d.fpath[kk].x) ? 1 : -1;
            if (Math.Abs(ox - (Fixed.ToPx(d.fpath[kk].x) + face * EST_D2_MOUTH_DX)) > 1 || Math.Abs(oy - (Fixed.ToPx(d.fpath[kk].y) - EST_D2_MOUTH_UP)) > 1) badMouth++;
        }
        Expect("cầu lửa từ miệng (chân − 39, trước 8) (quả sai)", 0, badMouth);
        ExpectRange("hộp mặt đổi theo khung (tick kiểm)", 500, 99999, headBoxChecked);
        Expect("hộp mặt khớp khung (tick sai)", 0, headBoxBad);
        Expect("va chạm từ khi hết Emerge (tick hộp tắt)", 0, CountK(d, kk => kk >= emerge && !d.active[kk]));
        measures.Add("nhảy cao " + highs + " / thấp " + lows + " (đỉnh: " + rises + "), phun " + (dd != null ? dd.volleys : 0) + " loạt; hộp mặt kiểm " + headBoxChecked + " tick");
    }

    /// <summary>Hộp mặt = khung A (0,1,2,5): cột 0–23 hàng 0–31; khung B (3,4,6): cột 4–31 hàng 16–47 (pivot đáy-giữa).</summary>
    static void CheckHeadBox()
    {
        var d = Boss<DraculaPhase2>();
        if (d == null || d.Anim == null || d.Anim.Current == null || d.Anim.Current.name != "p2_all") return;
        int f = d.Anim.FrameIndex;
        int h = d.Anim.Current.frames[f].texture != null ? Mathf.RoundToInt(d.Anim.Current.frames[f].rect.height) : 64;
        bool b = f == 3 || f == 4 || f == 6;
        var want = b ? new RectInt(-20, 16 - h, 28, 32) : new RectInt(-24, -h, 24, 32);
        headBoxChecked++;
        if (!d.HeadLocal.Equals(want) || f != d.frame) headBoxBad++;
    }

    static void HolyChecks()
    {
        var d = First("DraculaPhase2");
        if (d == null) return;
        var dd = d.e as DraculaPhase2;
        ExpectRange("lửa chạm quỷ (tick)", 20, 9999, dd != null ? dd.holyTicks : 0);
        Expect("quỷ KHÔNG mất máu vì nước thánh (wiki)", d.hp[0], d.hp[d.hp.Count - 1]);
        Expect("số đòn vào quỷ", 0, d.hits);
        // khi lửa chạm: đứng im; hộp chạm tắt; Simon đi xuyên
        int stunK = FirstK(d, k => d.states[k] == "Stun");
        int moveInStun = 0, touchOnInHoly = 0, stunTicks = 0;
        for (int k = 1; k < d.fpath.Count; k++)
            if (d.states[k] == "Stun")
            {
                stunTicks++;
                if (d.fpath[k] != d.fpath[k - 1]) moveInStun++;
            }
        int crossed = 0, stunEnd = stunK;
        for (int rel = 1; rel < recs.Count; rel++)
        {
            int k = rel - d.spawnRel;
            if (k < 0 || k >= d.fpath.Count) continue;
            int dx = Fixed.ToPx(d.fpath[k].x);
            if (d.states[k] == "Stun")
            {
                stunEnd = k;
                if (d.touch[k]) touchOnInHoly++;
                if (Math.Abs(recs[rel].sx - dx) < 4) crossed++;
            }
        }
        int hitsInStun = 0;
        foreach (var h in playerHits)
        {
            int at = int.Parse(h.Substring(h.IndexOf('@') + 1).Split('−')[0]);
            if (at >= d.spawnRel + stunK && at <= d.spawnRel + stunEnd) hitsInStun++;
        }
        int x0 = stunK >= 0 ? recs[d.spawnRel + stunK].sx : 0, x1 = recs[Math.Min(recs.Count - 1, d.spawnRel + stunEnd)].sx, demonX = Fixed.ToPx(d.fpath[Math.Max(0, stunK)].x);
        ExpectRange("quỷ choáng (tick)", 40, 9999, stunTicks);
        Expect("quỷ đứng im khi choáng (tick di chuyển)", 0, moveInStun);
        Expect("hộp chạm TẮT khi bị lửa giữ (tick bật)", 0, touchOnInHoly);
        Expect("Simon đi XUYÊN qua quỷ trong lúc choáng (trái thân → phải thân)", 1, x0 < demonX - 24 && x1 > demonX + 24 ? 1 : 0);
        Expect("Simon không bị thương trong lúc quỷ bị lửa giữ (đòn)", 0, hitsInStun);
        measures.Add("lửa chạm " + (dd != null ? dd.holyTicks : 0) + " tick, choáng k " + stunK + "–" + stunEnd + " (" + stunTicks + " tick); Simon x " + x0 + " → " + x1
                     + " qua quỷ x " + demonX + " (trùng ±3: " + crossed + " tick); đòn sau đó: " + string.Join(" ", playerHits));
    }

    static void DraculaKillChecks()
    {
        var p1 = First("DraculaPhase1");
        var p2s = Tracks("DraculaPhase2");
        if (p1 == null) return;
        Expect("dạng 1: số đòn MẠNH (roi cấp 3) tới khi biến hình (wiki 24/16)", WIKI_DRAC_S, p1.hits);
        Expect("có dạng 2", 1, p2s.Count);
        if (p2s.Count == 0) return;
        var p2 = p2s[0];
        Expect("dạng 2: số đòn MẠNH (wiki '32 lần')", WIKI_DRAC2_S, p2.hits);
        Expect("dạng 2 bị hạ", 1, p2.defeated ? 1 : 0);
        ExpectRange("hạ (tick rel)", 1, PILOT_MAX_TICKS, defeatRel);
        Expect("điểm: dạng 1 = 0, dạng 2 = +50 000 (+100 × cầu lửa phá " + brokenShots + ")", WIKI_SCORE_DRAC + brokenShots * WIKI_PROJ_SCORE, defeatRel >= 0 ? recs[defeatRel].score : -1);
        Expect("điểm khi hết dạng 1 (chỉ cầu lửa phá)", phase1EndRel >= 0 ? recs[phase1EndRel - 1].score : -1, phase1EndRel >= 0 ? recs[phase1EndRel].score : -2);
        Expect("ENEMY 0 lúc hạ", 0, defeatRel >= 0 ? recs[defeatRel].ebar : -1);
        int zeros = 0;
        for (int i = 0; i < bars.Count; i++) if (bars[i] == 0) zeros++;
        Expect("thanh ENEMY về 0 hai lần (hết dạng 1, hết dạng 2)", 2, zeros);
        ExpectRange("quả cầu sau khi hạ (tick — 152)", 151, 153, crystalRel - defeatRel);
        ExpectRange("Simon nhặt quả cầu", crystalRel + 1, 99999, pickRel);
        Expect("GameRules.OnGameCleared đúng 1 lần", 1, clearedCount);
        Expect("OnGameCleared cùng tick nhặt quả cầu", pickRel, clearedRel);
        Expect("máu Simon đầy sau khi nhặt", GameSession.MAX_HP, pickRel >= 0 && pickRel < recs.Count ? recs[pickRel].hp : -1);
        Expect("tally đã bắt đầu (StageRules)", 1, rules != null && rules.Tally.Active ? 1 : 0);
        measures.Add("dạng 1: " + p1.hits + " đòn, hết ở tick " + phase1EndRel + "; dạng 2: " + p2.hits + " đòn, hạ ở tick " + defeatRel + " ("
                     + (defeatRel / 60.0).ToString("0.0") + " s), cầu " + crystalRel + ", nhặt " + pickRel + ", OnGameCleared " + clearedRel
                     + "; vạch ENEMY: " + string.Join(",", bars));
    }

    // ---------------------------------------------------------------- autopilot (chỉ khi GHI)

    struct WhipSample { public int rel; public RectInt r, body; }
    private static readonly List<WhipSample> whipRects = new List<WhipSample>();

    static int PilotDecide()
    {
        var s = Simon;
        if (s == null || !s.Alive)
            return 0;
        if (crystal != null && crystal.Alive)
            return ToCrystal(s);
        if (pilotPendingB > 0 && --pilotPendingB == 0)
            return 1 << (int)eNesButton.B;
        if (!s.Fsm.Is(eSimonState.Idle) && !s.Fsm.Is(eSimonState.Walk))
            return 0;
        if (current == "death_kill_axe")
            return PilotAxe(s);
        return PilotWhip(s);
    }

    /// <summary>Đi nhặt quả cầu; cầu nằm trên bục (phòng Death: bục giữa y 144) ⇒ tới cách ≤ 28 px rồi nhảy về phía cầu (A + hướng).</summary>
    static int ToCrystal(SimonController s)
    {
        int dx = crystal.X - s.X;
        if (crystal.Y >= s.Y - 8)
            return Dir(dx);
        if (!s.Fsm.Is(eSimonState.Idle) && !s.Fsm.Is(eSimonState.Walk))
            return 0;
        if (Math.Abs(dx) > 28 && Math.Abs(dx) < 48)
            return Dir(dx) | ((pilotPrev & (1 << (int)eNesButton.A)) == 0 ? 1 << (int)eNesButton.A : 0);
        return Math.Abs(dx) >= 48 ? Dir(dx) : Dir(-dx);   // quá sát mép bục ⇒ lùi ra lấy đà
    }

    /// <summary>Death: đứng cách Death ~40 px quay mặt về nó, ném rìu (đỉnh ~50 px trên chỗ ném ở +40) khi chưa có rìu trên màn.</summary>
    static int PilotAxe(SimonController s)
    {
        var d = Boss<GrimReaper>();
        if (d == null || d.dormant || d.dying)
            return 0;
        int dx = d.X - s.X;
        int face = s.Facing;
        int want = dx >= 0 ? 1 : -1;
        if (want != face)
            return Dir(want);
        int axes = 0;
        foreach (var e in world.GroupList(eEntityGroup.PlayerWeapon))
        {
            var w = e as SubWeaponProjectile;
            if (w != null && w.Alive && w.Kind == eSubWeapon.Axe) axes++;
        }
        int adx = Math.Abs(dx);
        if (axes == 0 && adx >= 16 && adx <= 72 && (pilotPrev & (1 << (int)eNesButton.SubWeapon)) == 0)
            return 1 << (int)eNesButton.SubWeapon;
        int idealX = Mathf.Clamp(d.X - want * 40, 16, 240);
        return Math.Abs(idealX - s.X) > 4 ? Dir(idealX - s.X) : 0;
    }

    /// <summary>Dracula: roi cấp 3 (đầu roi tới pivot + 52). Đầu trong tầm roi đứng ⇒ quất; trong tầm nhảy quất (đầu roi cao hơn ~34 px) ⇒
    /// nhảy rồi quất 2 tick sau; không thì đi tới chỗ đầu cách 32 px trước mặt. Bỏ qua khi boss ẩn/đang nhảy/không va chạm.</summary>
    static int PilotWhip(SimonController s)
    {
        BossBase b = Boss<DraculaPhase2>();
        if (b == null) b = Boss<DraculaPhase1>();
        var p1 = b as DraculaPhase1;
        var p2 = b as DraculaPhase2;
        // đã hiện chỗ mới (kể cả đang hiện dần — nhấp nháy) ⇒ đón vị trí; ẩn hẳn / biến hình ⇒ chờ giữa sàn (ngoài bậc)
        bool seen = b != null && !b.dying && (p1 == null || p1.Fsm.Is(eDraculaState.Appear) || p1.Fsm.Is(eDraculaState.Stand)
                                              || p1.Fsm.Is(eDraculaState.Cape) || p1.Fsm.Is(eDraculaState.Close));
        if (!seen)
            return Math.Abs(128 - s.X) > 24 ? Dir(128 - s.X) : 0;
        if (p2 != null && p2.Fsm.Is(eDemonState.Jump))
            return 0;
        var head = b.ToSection(b.Hurt.local);
        if (b.dormant)
        {
            int hx = head.x + head.width / 2, w0 = hx >= s.X ? 1 : -1, ix = Mathf.Clamp(hx - w0 * 32, 16, 240);
            return Math.Abs(ix - s.X) > 2 ? Dir(ix - s.X) : (w0 != s.Facing ? Dir(w0) : 0);
        }
        int hc = head.x + head.width / 2;
        int want = hc >= s.X ? 1 : -1;
        if (want != s.Facing)
            return Dir(want);
        var stand = WhipBox(s, 0);
        var jump = WhipBox(s, 34);
        if (CombatResolver.Overlaps(stand, head) && (pilotPrev & (1 << (int)eNesButton.B)) == 0)
            return 1 << (int)eNesButton.B;
        if (CombatResolver.Overlaps(jump, head) && (pilotPrev & (1 << (int)eNesButton.A)) == 0)
        {
            pilotPendingB = 2;
            return 1 << (int)eNesButton.A;
        }
        int idealX = Mathf.Clamp(hc - want * 32, 16, 240);
        if (s.Y != b.Y && Math.Abs(idealX - s.X) <= 4)
            return 0;   // khác mặt sàn (bậc) — chờ
        return Math.Abs(idealX - s.X) > 2 ? Dir(idealX - s.X) : 0;
    }

    static RectInt WhipBox(SimonController s, int up)
    {
        return new RectInt(s.Facing > 0 ? s.X + 14 : s.X - 50, s.Y - 24 - up, 36, 8);
    }

    static int Dir(int dx)
    {
        return dx == 0 ? 0 : 1 << (int)(dx > 0 ? eNesButton.Right : eNesButton.Left);
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
        playerHits.Clear();
        playerDmg.Clear();
        bars.Clear();
        bossHits.Clear();
        whipRects.Clear();
        defeatRel = crystalRel = pickRel = clearedRel = phase1EndRel = -1;
        clearedCount = 0;
        brokenShots = heartDrops = 0;
        crystal = null;
        startTick = world.TickCount;
        foreach (var e in world.GroupList(eEntityGroup.Enemy))
            TrackOf(e, 0);
        foreach (var e in world.GroupList(eEntityGroup.EnemyShot))
            TrackOf(e, 0);
        world.TickEnded += OnTickEnded;
        world.Hit += OnHit;
        EnemyBase.Defeated += OnDefeated;
        BossBase.BossDefeated += OnBossDefeated;
        BossBase.CrystalSpawned += OnCrystal;
        ItemEffects.CrystalPicked += OnPicked;
        GameRules.OnGameCleared += OnCleared;
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
        BossBase.BossDefeated -= OnBossDefeated;
        BossBase.CrystalSpawned -= OnCrystal;
        ItemEffects.CrystalPicked -= OnPicked;
        GameRules.OnGameCleared -= OnCleared;
        Running = false;
    }

    private static int Rel => world.TickCount - startTick - 1;

    private static void OnHit(HitInfo h)
    {
        var t = TrackOf(h.victim, Rel);
        if (t != null) t.hits++;
        if (h.victim == world.Player)
        {
            playerHits.Add(h.hitbox.layer + ":" + (h.attacker != null ? h.attacker.GetType().Name : "?") + "@" + Rel + "−" + h.damage);
            playerDmg.Add(h.damage);
        }
        if (h.victim is BossBase)
            bossHits.Add(h.victim.GetType().Name + "@" + Rel + ":" + h.strength);
        var f = h.victim as BoneTowerFireball;
        if (f != null && f.broken) brokenShots++;
        var sy = h.victim as DeathScythe;
        if (sy != null && sy.broken) brokenShots++;
        var df = h.victim as DraculaFireball;
        if (df != null && df.droppedHeart) heartDrops++;
    }

    private static void OnDefeated(EnemyBase e)
    {
        if (e is DraculaPhase1 && phase1EndRel < 0) phase1EndRel = Rel;
        var t = TrackOf(e, Rel);
        if (t != null) t.defeated = true;
    }

    private static void OnBossDefeated(BossBase b)
    {
        if (defeatRel < 0) defeatRel = Rel;
    }

    private static void OnCrystal(BossBase b, ItemPickup it)
    {
        crystal = it;
        crystalRel = Rel;
    }

    private static void OnPicked(StageWorld w)
    {
        if (pickRel < 0) pickRel = Rel;
    }

    private static void OnCleared()
    {
        clearedCount++;
        if (clearedRel < 0) clearedRel = Rel;
    }

    private static Track TrackOf(Entity e, int rel)
    {
        if (e == null || (e.Group != eEntityGroup.Enemy && e.Group != eEntityGroup.EnemyShot))
            return null;
        foreach (var t in tracks)
            if (t.e == e && t.spawnTick == e.SpawnTick)
                return t;
        var n = new Track { e = e, spawnTick = e.SpawnTick, spawnRel = rel, kind = e.GetType().Name };
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
        int rel = Rel;
        var ses = GameSession.Current;
        int fx = 0, shotsN = 0;
        for (int g = (int)eEntityGroup.Enemy; g <= (int)eEntityGroup.EnemyShot; g++)
            foreach (var e in w.GroupList(g))
            {
                if (!e.Alive) continue;
                var t = TrackOf(e, rel);
                t.fpath.Add(new Vector2Int(e.fx, e.fy));
                t.vel.Add(new Vector2Int(e.vx, e.vy));
                t.states.Add(e.DebugState.Split(' ')[0]);
                var en = e as EnemyBase;
                t.active.Add(en != null && en.Hurt != null && en.Hurt.active);
                t.touch.Add(en != null && en.Touch != null && en.Touch.active);
                t.visible.Add(e.Anim == null || e.Anim.Visible);
                t.anims.Add(e.Anim != null && e.Anim.Current != null ? e.Anim.Current.name + ":" + e.Anim.FrameIndex : "");
                t.hp.Add(en != null ? en.Health.Current : 0);
                if (g == (int)eEntityGroup.EnemyShot) shotsN++;
                Mix(e.fx); Mix(e.fy); Mix(e.Facing);
            }
        foreach (var t in tracks)
            if (t.endRel < 0 && (!t.e.Alive || t.e.SpawnTick != t.spawnTick))
                t.endRel = rel;
        foreach (var e in w.GroupList(eEntityGroup.Effect))
            if (e.Alive) fx++;
        if (bars.Count == 0 || bars[bars.Count - 1] != ses.enemyHp) bars.Add(ses.enemyHp);
        if (s.WhipHit != null && s.WhipHit.active)
        {
            var d1 = Boss<DraculaPhase1>();
            var body = d1 != null ? d1.ToSection(D1_BODY) : new RectInt();
            whipRects.Add(new WhipSample { rel = rel, r = s.WhipHit.SectionRect, body = body });
        }
        var r = new Rec
        {
            sx = s.X, sy = s.Y, hp = ses.playerHp, score = ses.score, state = s.Fsm.CurState, cx = w.CameraCtl != null ? w.CameraCtl.X : 0,
            fx = fx, frozenE = w.IsEnemyFrozen, shots = shotsN, ebar = ses.enemyHp, hearts = ses.hearts,
        };
        recs.Add(r);
        Mix(rel); Mix(s.fx); Mix(s.fy); Mix((int)r.state); Mix(r.hp); Mix(r.score); Mix(r.ebar); Mix(fx); Mix(shotsN); Mix(w.Rng.State);
        string shot;
        if (shots.TryGetValue(rel, out shot))
            ScreenCapture.CaptureScreenshot(shot);
        if (onTick != null)
            onTick(rel);
        if (!string.IsNullOrEmpty(ShotDir) && !pilotOn)
        {
            string tag = phase1EndRel >= 0 && rel == phase1EndRel + 20 ? "transform" : defeatRel >= 0 && rel == defeatRel + 8 ? "deathfx"
                : crystalRel >= 0 && rel == crystalRel + 12 ? "crystal" : pickRel >= 0 && rel == pickRel + 2 ? "picked" : null;
            if (tag != null)
                ScreenCapture.CaptureScreenshot(Path.Combine(ShotDir, current + "_" + tag + "_r" + rel.ToString("0000") + ".png"));
        }
        if (pilotOn)
        {
            if (pickRel >= 0 && pilotEnd < 0)
                pilotEnd = rel + 60;
            if (pilotEnd >= 0 && rel >= pilotEnd)
                target = recorded + 1;
            else
            {
                int mask = PilotDecide();
                pilotPrev = mask;
                for (int b = 0; b < NesPad.BUTTON_COUNT; b++)
                    if ((mask & (1 << b)) != 0)
                        NesPad.I.Inject((eNesButton)b, 1);
            }
        }
        if (++recorded >= target)
            Finish();
    }

    private static void Finish()
    {
        Unhook();
        if (pilotOn)
        {
            var rec = NesPad.I.StopRecord();
            WriteReplay(Label, rec, Find(Label).note + " — ghi bằng autopilot (CVGP 20), hash chạy ghi " + Hash.ToString("x8"));
            RecordHashes[Label] = Hash;
            measures.Add("GHI replay " + rec.Count + " tick (hash chạy ghi " + Hash.ToString("x8") + ")");
            pilotOn = false;
        }
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
                checks.Add("    FAIL ngoại lệ khi chấm: " + ex.Message + " @ " + ex.StackTrace.Split('\n')[0]);
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
        if (addedRules && rules != null)
        {
            rules.enabled = false;
            UnityEngine.Object.DestroyImmediate(rules);   // kịch bản kế chạy NGAY cùng frame (AddRules không được thấy bản sắp huỷ)
            if (Simon != null)
                Simon.AutoRespawn = savedAutoRespawn;
        }
        addedRules = false;
        rules = null;
        arena = null;
        if (world != null)
        {
            world.FreezeEnemies(0);
            world.ClearNonPlayer();
            if (world.Tiles != null)
            {
                world.Tiles.WallMinX = int.MinValue;
                world.Tiles.WallMaxX = int.MaxValue;
            }
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

    /// <summary>Vết thực thể kịch bản vừa chạy: "kind,spawnRel,k,rel,x,y,state,active,visible,simonX,simonY,php,ebar" (px).</summary>
    public static string TraceCsv()
    {
        var sb = new StringBuilder("kind,spawnRel,k,rel,x,y,state,active,visible,simonX,simonY,php,ebar\n");
        foreach (var t in tracks)
            for (int k = 0; k < t.fpath.Count; k++)
            {
                int rel = t.spawnRel + k;
                bool ok = rel >= 0 && rel < recs.Count;
                sb.Append(t.kind).Append(',').Append(t.spawnRel).Append(',').Append(k).Append(',').Append(rel).Append(',')
                  .Append(Fixed.ToPx(t.fpath[k].x)).Append(',').Append(Fixed.ToPx(t.fpath[k].y)).Append(',').Append(t.states[k]).Append(',')
                  .Append(t.active[k] ? 1 : 0).Append(',').Append(t.visible[k] ? 1 : 0).Append(',')
                  .Append(ok ? recs[rel].sx : 0).Append(',').Append(ok ? recs[rel].sy : 0).Append(',').Append(ok ? recs[rel].hp : 0).Append(',')
                  .Append(ok ? recs[rel].ebar : 0).Append('\n');
            }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- chấm chung

    static Rec Last => recs[recs.Count - 1];

    static List<Track> Tracks(string kind)
    {
        var l = new List<Track>();
        foreach (var t in tracks)
            if (t.kind == kind)
                l.Add(t);
        l.Sort((a, b) => a.spawnRel.CompareTo(b.spawnRel));
        return l;
    }

    static Track First(string kind)
    {
        var l = Tracks(kind);
        if (l.Count == 0)
        {
            Expect("có " + kind, 1, 0);
            return null;
        }
        return l[0];
    }

    /// <summary>Các đoạn state liên tiếp (tên, số tick).</summary>
    static List<KeyValuePair<string, int>> Runs(Track t)
    {
        var l = new List<KeyValuePair<string, int>>();
        for (int k = 0; k < t.states.Count; k++)
        {
            if (l.Count > 0 && l[l.Count - 1].Key == t.states[k])
                l[l.Count - 1] = new KeyValuePair<string, int>(t.states[k], l[l.Count - 1].Value + 1);
            else
                l.Add(new KeyValuePair<string, int>(t.states[k], 1));
        }
        return l;
    }

    static int FirstK(Track t, Func<int, bool> f)
    {
        for (int k = 0; k < t.fpath.Count; k++) if (f(k)) return k;
        return -1;
    }

    static int CountK(Track t, Func<int, bool> f)
    {
        int n = 0;
        for (int k = 0; k < t.fpath.Count; k++) if (f(k)) n++;
        return n;
    }

    static int FirstRel(int from, Func<Rec, bool> f)
    {
        for (int i = Math.Max(0, from); i < recs.Count; i++) if (f(recs[i])) return i;
        return -1;
    }

    static Vector2Int PosAt(Track t, int rel)
    {
        int k = rel - t.spawnRel;
        if (k < 0 || k >= t.fpath.Count) return new Vector2Int(-9999, -9999);
        return new Vector2Int(Fixed.ToPx(t.fpath[k].x), Fixed.ToPx(t.fpath[k].y));
    }

    static void DamageChecks(int dmg, int minHits)
    {
        ExpectRange("số lần Simon trúng đòn", minHits, 999, playerDmg.Count);
        int bad = 0, sum = 0;
        foreach (var d in playerDmg) { if (d != dmg) bad++; sum += d; }
        Expect("sát thương mỗi đòn = " + dmg + " vạch (đòn sai)", 0, bad);
        if (sum < GameSession.MAX_HP)
            Expect("máu Simon = 16 − tổng sát thương", GameSession.MAX_HP - sum, Last.hp);
        measures.Add(string.Join(" ", playerHits));
    }

    static void KillChecks(string kind, int hits, int score, int entities)
    {
        var ts = Tracks(kind);
        Expect("số thực thể boss", entities, ts.Count);
        int n = 0, defeated = 0;
        foreach (var t in ts) { n += t.hits; if (t.defeated) defeated++; }
        Expect(kind + " số đòn tới khi hạ (wiki " + hits + ")", hits, n);
        Expect("bị hạ (thực thể)", entities, defeated);
        ExpectRange("hạ (tick rel)", 1, PILOT_MAX_TICKS, defeatRel);
        Expect("+" + score + " (+100 × đạn phá " + brokenShots + ")", score + brokenShots * WIKI_PROJ_SCORE, defeatRel >= 0 ? recs[defeatRel].score : -1);
        Expect("ENEMY 0 lúc hạ", 0, defeatRel >= 0 ? recs[defeatRel].ebar : -1);
        int inc = 0;
        for (int i = 1; i < bars.Count; i++) if (bars[i] > bars[i - 1]) inc++;
        Expect("thanh ENEMY chỉ giảm (lần tăng)", 0, inc);
        ExpectRange("quả cầu sau khi hạ (tick — 152)", 151, 153, crystalRel - defeatRel);
        ExpectRange("Simon nhặt quả cầu", crystalRel + 1, 99999, pickRel);
        Expect("máu Simon đầy sau khi nhặt", GameSession.MAX_HP, pickRel >= 0 && pickRel < recs.Count ? recs[pickRel].hp : -1);
        Expect("tally đã bắt đầu (StageRules)", 1, rules != null && rules.Tally.Active ? 1 : 0);
        Expect("OnGameCleared KHÔNG bắn (không phải quả cầu cuối)", 0, clearedCount);
        measures.Add(kind + ": " + n + " đòn, hạ ở tick " + defeatRel + " (" + (defeatRel / 60.0).ToString("0.0") + " s), cầu " + crystalRel
                     + ", nhặt " + pickRel + "; vạch ENEMY: " + string.Join(",", bars));
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
