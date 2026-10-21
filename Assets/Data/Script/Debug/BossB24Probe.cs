#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness boss Block 2–4 (CVGP 19; routine 21 chạy lại) — CHỈ Editor, scene Arena (Player = Simon, StageWorld.itemCatalog). Qua MCP khi Play:
///   BossB24Probe.SaveReplays()                         ⇒ Assets/Data/Tests/Replays/boss_b24_&lt;tên&gt;.txt (kịch bản viết tay)
///   BossB24Probe.Record(BossB24Probe.PilotNames())     ⇒ AUTOPILOT (chỉ đọc trạng thái, bấm nút qua NesPad.Inject) đánh boss bằng roi cấp 1 tới
///                                                        khi nhặt quả cầu ⇒ GHI replay boss_b24_*_kill_l1.txt (chạy lại khi đổi gameplay)
///   BossB24Probe.RunSuite(BossB24Probe.ScenarioNames()) ⇒ chạy (input ĐỌC TỪ FILE), ghi từng tick, TỰ CHẤM ⇒ SuiteReport()
///   BossB24Probe.ShotDir = thư mục ⇒ chụp Game view ở các tick cố định (≥ 6 ảnh/boss)
/// Phòng thử: BossRoomSetup (Arena boss rooms, camera khoá cx 0). Không có video Block 3–4 ⇒ kỳ vọng = hằng WIKI_* (đòn, điểm, sát thương theo
/// stage) + EST_* (ước lượng routine 19 chép ĐỘC LẬP Config — đổi Config mà quên đổi đây thì bộ kiểm báo).
/// CVM2 09: Queen Medusa + rắn ĐO VIDEO (Boss_Map_2 + Map_2 — CVM2_MEMORY §2k) ⇒ hằng VIDEO_*; thêm kịch bản PHÒNG THẬT medusa_room_*
/// (section Block 2 "Medusa" + BossArena từ Tools/cvm2/map2_boss.json — Map2BossRoom): khoá camera cx 0 khi cuộn trái, đầu tượng tile nền
/// xoá lúc thức, bay / rắn / va Simon ⇒ nảy / nước thánh / hạ bằng roi cấp 3 ⇒ quả cầu.
/// CVM3 09: Mummy Men + băng gạc ĐO VIDEO (Map_3 + Boss_Map_3 — CVM3_MEMORY §2i) ⇒ hằng VIDEO_MUMMY_* / VIDEO_BANDAGE_*; thêm kịch bản PHÒNG
/// THẬT mummy_room_* (section Block 3 "MummyMan" + BossArena từ Tools/cvm3/map3_boss.json — Map3BossRoom): 2 Mummy đứng chờ, khoá cx 1280,
/// thức khoá + 120, nước thánh làm đứng, hạ bằng roi cấp 3 ⇒ mỗi con +3000 ⇒ quả cầu đứng rồi rơi; nến phải ⇒ II.
/// </summary>
public static class BossB24Probe
{
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";

    // ---------------------------------------------------------------- kỳ vọng
    public const int WIKI_MEDUSA_W = 12, WIKI_MUMMY_W = 48, WIKI_FRANK_W = 24;
    public const int WIKI_SCORE_MEDUSA = 3000, WIKI_SCORE_MUMMY = 3000, WIKI_SCORE_FRANK = 5000, WIKI_PROJ_SCORE = 100;
    public const int WIKI_DMG_ST6 = 2, WIKI_DMG_ST9 = 3, WIKI_DMG_ST12 = 3;   // wiki Gameplay#Damage
    // CVM2 09 — Queen Medusa + rắn ĐO VIDEO (chép ĐỘC LẬP Config; nguồn CVM2_MEMORY §2k)
    public const int VIDEO_MEDUSA_W = 16, VIDEO_MEDUSA_WAKE = 180, VIDEO_MEDUSA_EMERGE_DX = 2, VIDEO_MEDUSA_HEAD_UP = 31, VIDEO_MEDUSA_V = 224;
    public const int VIDEO_MEDUSA_DIP = 13, VIDEO_MEDUSA_T = 68, VIDEO_MEDUSA_FIRST = 64, VIDEO_MEDUSA_HOVER = 64, VIDEO_MEDUSA_UNIT = 16;
    public const int VIDEO_MEDUSA_UNITS = 10, VIDEO_MEDUSA_MUSIC = 57, VIDEO_MEDUSA_FLASH = 9, VIDEO_MEDUSA_CLEAR = 8, VIDEO_MEDUSA_RISE = 18;
    public const int VIDEO_SNAKE_V = 352, VIDEO_SNAKE_G = 32, VIDEO_SNAKE_DROP = 5, VIDEO_SNAKE_SCORE = 100, VIDEO_SNAKE_EDGE = 8, EST_SNAKE_ALIVE = 2;
    public const int VIDEO_ROOM_STATUE_X = 112, VIDEO_ROOM_FLOOR = 144, VIDEO_ROOM_LOCK_SIMON_X = 128;
    // CVM3 09 — Mummy Men + băng gạc ĐO VIDEO (chép ĐỘC LẬP Config; nguồn CVM3_MEMORY §2i, ClaudeNotes/cvm3/ref/data/boss9m/model.txt)
    public const int VIDEO_MUMMY_W = 16, VIDEO_MUMMY_SCORE = 3000, VIDEO_MUMMY_V = 192, VIDEO_MUMMY_DX = 191, VIDEO_MUMMY_WAKE = 120, VIDEO_MUMMY_MUSIC = 60;
    public const int VIDEO_MUMMY_NEAR = 46, VIDEO_MUMMY_ANIM = 16, VIDEO_MUMMY_THROW_FIRST = 64, VIDEO_MUMMY_THROW_FIRST2 = 120, VIDEO_MUMMY_POSE = 48;
    public const int VIDEO_MUMMY_RELEASE = 40, VIDEO_MUMMY_FLASH = 2, VIDEO_MUMMY_INVINC = 32, VIDEO_MUMMY_HOLY_STUN = 12, VIDEO_MUMMY_CRYSTAL = 117, VIDEO_MUMMY_HOVER = 34;
    public const int VIDEO_BANDAGE_V = 384, VIDEO_BANDAGE_HIGH = 31, VIDEO_BANDAGE_LOW = 15, VIDEO_BANDAGE_AMP = 24, VIDEO_BANDAGE_T = 102, VIDEO_BANDAGE_INSET = 8;
    public const int VIDEO_ROOM_MUMMY_R = 1503, VIDEO_ROOM_MUMMY_L = 1312, VIDEO_ROOM_LOCK_CX = 1280, VIDEO_ROOM_LOCK_SIMON_X3 = 1408;
    public const int EST_FRANK_V = 64, EST_FRANK_WAKE = 16, EST_FRANK_ZONE_MIN = 128, EST_FRANK_ZONE_MAX = 216, EST_IGOR_START = 96;
    public const int EST_IGOR_RIDE = 40, EST_IGOR_SHORT_VX = 320, EST_IGOR_LONG_VX = 384, EST_IGOR_FIRE_EVERY = 2, EST_IGOR_BALL_V = 384;
    public const int EST_IGOR_STUN = 120, STOPWATCH_TICKS = 186;   // Stopwatch video:Map_2 (đo:cvm3-03 — CVM3_MEMORY §2d; wiki 5 s = 300 sai)
    public const int PILOT_MAX_TICKS = 7200;

    private struct Scn
    {
        public string name, rle, note;
        public bool pilot;
        public Scn(string n, string r, string note, bool pilot = false) { name = n; rle = r; this.note = note; this.pilot = pilot; }
    }

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("medusa_pattern", "-*760", "CVM2 09 (video): phòng thử, chân tượng (128,176), Simon (32,176) đứng; camera khoá tick đầu ⇒ thức sau 180 tick: đầu hiện ở (130,145), nhận/gây đòn ngay, thả rắn; bay ±0.875 px/tick về Simon, dọc nhào y0 + 13(1 − cos 2πτ/68)/2; đoạn đầu 64 tick, dừng 64 ⇒ thả rắn ⇒ đoạn mới bội 16 (16–160); nháy xanh/tím 9 tick; rắn rơi g 0.125 từ (x, tâm + 5) rồi trườn 1.375 về Simon, ra mép màn (x < 8) thì mất; nhạc boss thức + 57"),
        new Scn("medusa_touch", "-*420", "Stage 6, Simon không bất tử đứng yên (64,176): Medusa chạm ⇒ −2 vạch/lần (wiki) + Medusa QUAY ngang ngay tick sau và nảy lên cao hơn đầu đoạn (video Boss_Map_2 f196)"),
        new Scn("medusa_snake_kill", "D*20 D+B D*40", "Arena: rắn (150,168) rơi rồi trườn 1.375 về Simon (100,176) ⇒ roi da NGỒI 1 đòn là chết (wiki), +100 (video Map_2 3:48.20)"),
        new Scn("medusa_snake_whip3", "-*20 B -*40", "Arena: rắn (150,168) trườn về Simon (100,176) ⇒ roi cấp 3 ĐỨNG hạ rắn (video Map_2 f6843–6849: roi ngang hàng NES 156–159 trúng rắn bò) 1 đòn, +100"),
        new Scn("medusa_stopwatch", "-*200 Sub -*500", "Stopwatch ở tick 201 (đang bay; wiki: boss CUỐI Stopwatch còn tác dụng): Medusa + rắn đứng yên 186 tick rồi chạy tiếp; nhạc boss vẫn đúng thức + 57 (video Map_2: đồng hồ không giữ nhạc)"),
        new Scn("medusa_rosary", "-*360", "Rosary (RosaryClear) tick 330 (rắn 2 vừa thả lúc rời chỗ dừng 1): rắn bị gỡ, Medusa (boss) còn, 0 điểm"),
        new Scn("medusa_kill_l1", "", "AUTOPILOT roi da (cấp 1): quay về Medusa, quất khi đoán trúng; 16 đòn (video: mọi đòn −1 vạch — CVM2 09), +3000, lửa, quả cầu sau 152, nhặt ⇒ tally", true),
        new Scn("medusa_room_wake", "L*139 -*600", "PHÒNG THẬT (section Medusa, BossArena map2_boss.json): Simon (200,144) đi TRÁI tới x 61 — camera khoá cx 0 lúc Simon x 128 (video Map_2 3:44.50), tường mép màn; 180 tick sau: xoá tile đầu tượng (marker statueHead), thân tượng giữ, đầu hiện (114,113), thả rắn; bay về Simon, dừng, rắn trườn trên sàn y 144 rồi ra mép trái; nhạc thức + 57"),
        new Scn("medusa_room_touch", "-*480", "PHÒNG THẬT: Simon (61,144) quay phải, KHÔNG bất tử: Medusa thức, bay trái chạm Simon ⇒ −2 vạch, Medusa quay ngang ngay + nảy (sóng lên trước) như video Boss_Map_2 f196–f292"),
        new Scn("medusa_room_holy", "-*205 Sub -*240", "PHÒNG THẬT: Simon (61,144) quay phải cầm nước thánh, ném ở tick 206 khi Medusa bay tới: lọ trúng 1 lần (−1 vạch), Medusa đứng yên lúc chạm lọ/lửa và giữ khung tím (video Boss_Map_2 f135–196)"),
        new Scn("medusa_room_kill_l3", "", "AUTOPILOT PHÒNG THẬT roi cấp 3 (Strong): 16 nhát (video Map_2 3:48.30–3:55.77), +3000, lửa ở tâm đầu, quả cầu sau 152 ở (128,104) rơi xuống sàn y 144, nhặt ⇒ tally", true),
        new Scn("mummy_pattern", "R*1 -*899", "CVM3 09 (video): phòng thử, Simon (128,176) đứng quay phải; con đầu (216,176) + con 2 cách 191 về trái đứng chờ khung 0 quay trái; camera khoá tick đầu ⇒ nhạc boss khoá + 60, CẢ HAI thức khoá + 120 (bước đầu về Simon); đi 0.75 px/tick, khung [0,1,2,1] × 16; mỗi 16 tick quyết định: Simon quay lưng ⇒ tới, nhìn về ⇒ |dx| < 46 đi xa / ≥ 56 đi tới; ném: con đầu sau 64 tick đi, con 2 sau 120 (sau đó mỗi 66 tick đi, 50 %): đứng 48 tick, nháy đỏ↔tím mỗi 2 tick, tick 40 băng ở X Mummy, tâm chân − 31 / − 15, ±1.5 px/tick, y + 24·sin(2πt/102), mất cách mép màn 8"),
        new Scn("mummy_whip_bandage", "-*200 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24 B -*24", "CVM3 09: Simon (128,176) roi cấp 3 quất đều: băng gạc bay tới bị roi phá ⇒ +100 + lửa (video Boss_Map_3 f505–507, f637–640, f835–837, f900–903)"),
        new Scn("mummy_stand", "-*700", "Stage 9: Simon ĐỨNG ở (80,176) không bất tử: băng gạc trúng ⇒ −3 vạch (EnemyAttack; video Boss_Map_3 f542 / f702 / f798)"),
        new Scn("mummy_holy", "-*129 Sub -*400", "CVM3 09: Simon (128,176) ném nước thánh tick 130: con đầu đi vào lửa ⇒ đứng yên, khung đứng, suốt lúc chạm + 12 tick (video Map_3 f6101–6149), mỗi đòn −1 vạch, cách nhau ≥ 32 tick (bất tử 32)"),
        new Scn("mummy_stopwatch", "-*100 Sub -*320", "Stopwatch tick 101: Mummy Men + băng KHÔNG bị giữ (wiki 'Stopwatch vô dụng')"),
        new Scn("mummy_kill_l1", "", "AUTOPILOT roi da: mỗi con 8 đòn (video: 16 vạch chung 2 con), MỖI con hạ +3000 + lửa (video Map_3 3:42.87 / 3:45.70); con cuối ⇒ quả cầu sau 117 ở (cx + 128, 104), đứng 34 tick rồi rơi, nhặt ⇒ tally", true),
        new Scn("mummy_room_wake", "R*150 -*450", "PHÒNG THẬT (section MummyMan, BossArena map3_boss.json): Simon (1260,96) đi PHẢI xuống bậc khối — 2 Mummy đứng chờ (1312 / 1503, khung 0, không va chạm); camera khoá cx 1280 lúc Simon x 1408 (video Map_3 3:17.70), tường mép màn; nhạc khoá + 60, thức khoá + 120, ném đầu + 64 / + 120; khối Pot Roast (83,9)"),
        new Scn("mummy_room_holy", "-*286 Sub -*146 Sub -*165 Sub -*163 Sub -*164 Sub -*161 Sub -*161 Sub -*155 Sub -*166 Sub -*300", "PHÒNG THẬT: Simon trên khối trái (1302,112) quay phải ném 9 lọ nước thánh đúng mốc video Map_3 (f6074 … f6717 tính từ khoá f5931), không bất tử: Mummy chạm lọ/lửa đứng yên, −1 vạch mỗi đòn (cách ≥ 32 tick), băng gạc bay dưới khối không trúng Simon"),
        new Scn("mummy_room_kill_l3", "", "AUTOPILOT PHÒNG THẬT roi cấp 3: mỗi con 8 nhát (video Boss_Map_3 16 vạch), mỗi con +3000, quả cầu (1408,104) sau 117, đứng 34 rồi rơi xuống sàn y 176, nhặt ⇒ tally", true),
        new Scn("mummy_room_ii", "-*2 A A*3 B -*50 R*40 -*20", "PHÒNG THẬT: vũ khí phụ đã trúng 10 lần (roi cấp 3), Simon (1478,176) nhảy quất nến phải (1503,128) ⇒ tim nhỏ đổi thành Double Shot (ItemDrops.Resolve — video Map_3 f6838–6864), nhặt ⇒ Shot ×2"),
        new Scn("frank_pattern", "-*960", "Frankenstein (176,176) + Igor trên vai (Y − 40); Simon (16,144) trên bục góc trái: Monster thức tick 16, đi 0.25 px/tick trong nửa phải [128, 216]; Igor nhảy khỏi vai sau 96, nhảy tiến (vx 1.25, vy0 −3) / nhảy cao gần góc (vx 1.5, vy0 −5) qua lại 2 phía, cầu lửa ở đỉnh mỗi cú thứ 2 nhắm Simon 1.5 px/tick"),
        new Scn("frank_igor_freeze", "D*4 D+B D*23 -*170", "Arena: Igor đứng (124,176), Simon (100,176) NGỒI quất roi da: Igor đứng hình 120 tick, không mất máu, không chết, 0 điểm (wiki: bất tử, quất ⇒ đóng băng)"),
        new Scn("frank_touch", "-*600", "Stage 12, Simon không bất tử trên bục trái: cầu lửa Igor / chạm Igor/Monster ⇒ −3 vạch/lần"),
        new Scn("frank_stopwatch", "-*130 Sub -*320", "Stopwatch tick 131: Monster + Igor KHÔNG bị giữ (wiki: chỉ Phantom Bat + Medusa)"),
        new Scn("frank_kill_l1", "", "AUTOPILOT roi da: 24 đòn vào Monster (wiki — Igor bất tử), +5000, Igor biến mất, quả cầu, nhặt ⇒ tally", true),
    };

    // ---------------------------------------------------------------- ghi
    public class Track
    {
        public Entity e;
        public string kind;
        public int spawnTick, spawnRel = -1, endRel = -1, hits;
        public bool defeated;
        public readonly List<Vector2Int> fpath = new List<Vector2Int>();
        public readonly List<string> states = new List<string>();
        public readonly List<bool> active = new List<bool>();
        public readonly List<string> anims = new List<string>();
        public readonly List<int> hp = new List<int>();
    }

    private struct Rec
    {
        public int sx, sy, hp, score, ebar, cx, fx, shots;
        public eSimonState state;
        public bool frozenE, locked;
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
    private static readonly Queue<string> suite = new Queue<string>();
    public static readonly List<string> SuiteReports = new List<string>();
    public static bool SuiteDone { get; private set; } = true;
    public static int SuiteFails { get; private set; }
    public static uint SuiteHash { get; private set; }
    private static StageWorld world;
    private static int target, recorded, startTick, savedStage, defeatRel, crystalRel, pickRel, brokenShots;
    private static Action finish;
    private static Action<int> onTick;
    private static readonly List<GameObject> temp = new List<GameObject>();
    private static readonly Dictionary<int, string> shots = new Dictionary<int, string>();
    private static string current;
    private static BossArena arena;
    private static StageRules rules;
    private static bool addedRules, savedAutoRespawn = true, pilotOn;
    private static ItemPickup crystal;
    private static int pilotPrev, pilotEnd, firstDefeatRel = -1;
    // CVM2 09: phòng thật Map 2 + mốc riêng Medusa (nhạc boss, đòn thân lên Simon, ô nền tượng)
    private static bool realRoom, musicSeen;
    private static int musicRel = -1, statueHeadBefore = -1, statueHeadAfter = -1, statueBodyBefore = -1, statueBodyAfter = -1, wallMin = int.MinValue, wallMax = int.MaxValue;
    private static readonly List<int> medusaHitRels = new List<int>();
    private static int crystalX = -1, crystalY = -1;
    // CVM3 09: phòng thật Map 3 + mốc riêng Mummy (lửa nước thánh cuối, II, khối Pot Roast, quả cầu đứng)
    private static bool realRoom3;
    private static int fireLastRel = -1, doubleShotSeen, doubleShotRel = -1, potRoastContent = -1;
    private static readonly List<int> crystalYs = new List<int>();

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string ReplayPath(string name) { return REPLAY_DIR + "/boss_b24_" + name + ".txt"; }

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
        sb.Append("# CVGP 19 BossB24Probe kịch bản '").Append(name).Append("': ").Append(note).Append('\n');
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

    /// <summary>Chạy autopilot các kịch bản *_kill_l1 và GHI replay (dừng khi nhặt quả cầu + 60 tick).</summary>
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
        if (realRoom)
        {
            Cvm2TerrainProbe.Cleanup();   // CVM2 09: gỡ section Block 2 dựng lúc chạy (bộ chạy sau thấy StageWorld như cũ)
            realRoom = false;
        }
        if (realRoom3)
        {
            Cvm3TerrainProbe.Cleanup();   // CVM3 09: gỡ section Block 3
            realRoom3 = false;
        }
        Go("Arena", 48, 176, 1);
        world.ResetRng();
        GameSession.Current.ResetForNewGame();
    }

    public static string SuiteReport()
    {
        var sb = new StringBuilder();
        sb.Append("BOSS B2-4 SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(Recording ? " (GHI autopilot)" : "").Append(", ").Append(SuiteReports.Count)
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

    /// <summary>CVM2 09: phòng boss thật (section Medusa + BossArena map2_boss.json), Medusa sinh ngay (ẩn trong tượng).</summary>
    static void RealRoom(int x, int y, int facing)
    {
        GameObject host;
        realRoom = true;
        arena = Map2BossRoom.Enter(world, x, y, facing, true, out host);
        if (host != null)
            temp.Add(host);
        world.ResetRng();
    }

    /// <summary>CVM3 09: phòng boss thật Map 3 (section MummyMan + BossArena map3_boss.json), 2 Mummy sinh ngay (đứng chờ); candles ⇒ nến có content.</summary>
    static void RealRoom3(int x, int y, int facing, bool candles)
    {
        GameObject host;
        realRoom3 = true;
        arena = Map3BossRoom.Enter(world, x, y, facing, true, candles, out host);
        if (host != null)
            temp.Add(host);
        world.ResetRng();
        potRoastContent = -1;
        foreach (eEntityGroup g in Enum.GetValues(typeof(eEntityGroup)))
            foreach (var e in world.GroupList(g))
            {
                var b = e as BreakableBlock;
                if (b != null && b.Alive && b.col == 83 && b.row == 9)
                    potRoastContent = (int)b.content;
            }
    }

    static bool AnyHolyFire()
    {
        foreach (var e in world.GroupList(eEntityGroup.PlayerWeapon))
            if (e.Alive && e is HolyFire)
                return true;
        return false;
    }

    static QueenMedusa Med()
    {
        foreach (var e in world.GroupList(eEntityGroup.Enemy))
            if (e.Alive && e is QueenMedusa)
                return (QueenMedusa)e;
        return null;
    }

    /// <summary>Theo dõi mỗi tick: lúc nhạc boss bắt đầu (musicAt ≥ 0 → −1), ô nền tượng trước/sau thức, tường mép màn.</summary>
    static void WatchMedusa(int rel)
    {
        if (crystal != null && crystal.Alive)
        {
            crystalX = crystal.X;
            crystalY = crystal.Y;
        }
        var q = Med();
        if (q == null)
            return;
        if (q.musicAt >= 0)
            musicSeen = true;
        else if (musicSeen && musicRel < 0)
            musicRel = rel;
        if (realRoom && world.Section != null && world.Section.data != null)
        {
            var head = QueenMedusa.MarkerOfKind(world.Section.data, "statueHead");
            var st = QueenMedusa.MarkerOfKind(world.Section.data, "statue");
            if (head != null && st != null)
            {
                var body = new RectInt(st.rect.x, head.rect.yMax, st.rect.width, st.rect.yMax - head.rect.yMax);
                if (!q.statueCleared)
                {
                    statueHeadBefore = Map2BossRoom.TilesIn(world, head.rect);
                    statueBodyBefore = Map2BossRoom.TilesIn(world, body);
                }
                else if (statueHeadAfter < 0)
                {
                    statueHeadAfter = Map2BossRoom.TilesIn(world, head.rect);
                    statueBodyAfter = Map2BossRoom.TilesIn(world, body);
                }
            }
        }
        if (world.Tiles != null && world.Tiles.WallMinX != int.MinValue && wallMin == int.MinValue)
        {
            wallMin = world.Tiles.WallMinX;
            wallMax = world.Tiles.WallMaxX;
        }
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

    // ---------------------------------------------------------------- 1 kịch bản

    public static int Run(string scenario)
    {
        world = StageWorld.Current;
        var s = Simon;
        if (world == null || s == null || world.Items == null)
        {
            Debug.LogError("[BossB24Probe] cần scene Arena với Player = SimonController + StageWorld.itemCatalog");
            return -1;
        }
        var sc = Find(scenario);
        if (sc.name == null)
        {
            Debug.LogError("[BossB24Probe] không có kịch bản " + scenario);
            return -1;
        }
        if (Recording && !sc.pilot)
            return -1;   // Record chỉ chạy kịch bản autopilot
        bool pilot = sc.pilot && Recording;
        List<int> ticks = null;
        if (!pilot)
        {
            string p = ReplayPath(scenario);
            if (!File.Exists(p))
            {
                Debug.LogError("[BossB24Probe] thiếu " + p + (sc.pilot ? " — gọi BossB24Probe.Record(\"" + scenario + "\")" : " — gọi BossB24Probe.SaveReplays()"));
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
        musicSeen = false;
        musicRel = statueHeadBefore = statueHeadAfter = statueBodyBefore = statueBodyAfter = -1;
        wallMin = int.MinValue;
        wallMax = int.MaxValue;
        medusaHitRels.Clear();
        crystalX = crystalY = -1;
        fireLastRel = doubleShotRel = -1;
        doubleShotSeen = 0;
        crystalYs.Clear();
        CvgpDebug.CheatInvincible = true;
        world.FreezeEnemies(0);
        world.ResetRng();
        switch (scenario)
        {
            // ======================================================= Queen Medusa
            case "medusa_pattern":
                Room(BossRoomSetup.MEDUSA);
                Photo(30, 179, 181, 190, 200, 230, 260, 300, 330, 420, 520);
                finish = () =>
                {
                    var m = First("QueenMedusa");
                    if (m == null) return;
                    int wake = MedusaWakeChecks(m, 128, 176, 0);
                    if (wake < 0) return;
                    int badV, badY, badLen, badHover, segs = MedusaSegments(m, 128 + VIDEO_MEDUSA_EMERGE_DX, 176 - VIDEO_MEDUSA_HEAD_UP, 176, -1, out badV, out badY, out badLen, out badHover);
                    Expect("ngang ±0.875 px/tick mọi tick bay (tick sai)", 0, badV);
                    Expect("dọc nhào y0 + 13(1 − cos 2πτ/68)/2 từ đầu mỗi đoạn (tick sai)", 0, badY);
                    Expect("độ dài đoạn: đầu 64, sau bội 16 trong [16, 160] (đoạn sai)", 0, badLen);
                    Expect("dừng 63 tick ghi state Hover (64 theo Config — chuỗi state) không di chuyển (tick sai)", 0, badHover);
                    ExpectRange("số đoạn bay", 3, 99, segs);
                    MedusaFlashChecks(m, wake);
                    SnakeChecks(m, Tracks("Snake"), 176, 0, true);
                    Expect("nhạc boss: thức + 57 (tick)", VIDEO_MEDUSA_MUSIC, musicRel - wake);
                };
                break;
            case "medusa_touch":
                Room(BossRoomSetup.MEDUSA);
                CvgpDebug.CheatInvincible = false;
                PlaceSimon(64, 176, 1);
                Photo(200, 230, 260);
                finish = () =>
                {
                    DamageChecks(WIKI_DMG_ST6, 1);
                    var m = First("QueenMedusa");
                    if (m != null) BounceChecks(m);
                };
                break;
            case "medusa_snake_kill":
            case "medusa_snake_whip3":
                {
                    Go("Arena", 100, 176, 1);
                    world.Stage = 6;
                    if (scenario == "medusa_snake_whip3") ses.whipLevel = 3;
                    world.Spawn(Prefab<Snake>("Snake"), 150, 168, -1);
                    Photo(6, 24, 34, 40);
                    finish = () =>
                    {
                        var t = First("Snake");
                        if (t == null) return;
                        Expect("rắn bị hạ", 1, t.defeated ? 1 : 0);
                        Expect("số đòn (wiki: 1 đòn roi)", 1, t.hits);
                        Expect("điểm (video Map_2 3:48.20: +100)", VIDEO_SNAKE_SCORE, Last.score);
                        int k0 = FirstK(t, k => t.states[k] == "Crawl");
                        Expect("rắn chạm sàn y 176 (tick có)", 1, k0 >= 0 && Fixed.ToPx(t.fpath[k0].y) == 176 ? 1 : 0);
                        int bad = 0;
                        for (int k = k0 + 1; k0 >= 0 && k < t.fpath.Count; k++)
                            if (t.states[k] == "Crawl" && t.states[k - 1] == "Crawl" && t.fpath[k].x - t.fpath[k - 1].x != -VIDEO_SNAKE_V) bad++;
                        Expect("trườn về Simon (trái) 1.375 px/tick (tick sai)", 0, bad);
                        measures.Add("rắn chạm sàn rel " + k0 + ", hạ ở tick " + t.endRel + " tại x " + (t.fpath.Count > 0 ? Fixed.ToPx(t.fpath[t.fpath.Count - 1].x) : -1));
                    };
                }
                break;
            case "medusa_stopwatch":
                Room(BossRoomSetup.MEDUSA);
                ses.subWeapon = eSubWeapon.Stopwatch;
                ses.hearts = 10;
                PlaceSimon(40, 176, 1);
                Photo(190, 300, 520);
                finish = () =>
                {
                    var m = First("QueenMedusa");
                    if (m == null) return;
                    int wake = FirstK(m, k => m.states[k] != "Statue");
                    int f0 = FirstRel(0, r => r.frozenE);
                    ExpectRange("Stopwatch bắt đầu (tick — bấm 201 + ném)", 200, 220, f0);
                    if (f0 < 0) return;
                    int moved = 0, movedAfter = 0;
                    for (int rel = f0 + 1; rel < f0 + STOPWATCH_TICKS && rel < recs.Count; rel++)
                        if (PosAt(m, rel) != PosAt(m, rel - 1)) moved++;
                    for (int rel = f0 + STOPWATCH_TICKS + 1; rel < recs.Count; rel++)
                        if (PosAt(m, rel) != PosAt(m, rel - 1)) movedAfter++;
                    Expect("Medusa đứng yên khi Stopwatch (tick di chuyển)", 0, moved);
                    ExpectRange("Medusa bay tiếp sau 300 tick (tick di chuyển)", 30, 9999, movedAfter);
                    int snakeMoved = 0, snakeSeen = 0;
                    foreach (var t in Tracks("Snake"))
                        for (int rel = Math.Max(f0 + 1, t.spawnRel + 1); rel < f0 + STOPWATCH_TICKS && (t.endRel < 0 || rel < t.endRel) && rel < recs.Count; rel++)
                        {
                            snakeSeen++;
                            if (PosAt(t, rel) != PosAt(t, rel - 1)) snakeMoved++;
                        }
                    Expect("rắn đứng yên khi Stopwatch (tick di chuyển)", 0, snakeMoved);
                    ExpectRange("rắn bị giữ trong lúc Stopwatch (tick-con)", 1, 99999, snakeSeen);
                    Expect("nhạc boss vẫn đúng thức + 57 lúc bị Stopwatch (tick)", VIDEO_MEDUSA_MUSIC, musicRel - wake);
                    measures.Add("thức rel " + wake + ", Stopwatch tick " + f0 + "; rắn quan sát " + snakeSeen + " tick-con trong lúc giữ");
                };
                break;
            case "medusa_rosary":
                {
                    Room(BossRoomSetup.MEDUSA);
                    int before = -1, killed = -1;
                    Photo(328, 332);
                    onTick = rel =>
                    {
                        if (rel == 329) { before = OnScreen(eEntityGroup.Enemy); killed = world.RosaryClear(); }
                    };
                    finish = () =>
                    {
                        ExpectRange("trước Rosary: Medusa + ≥ 1 rắn", 2, 99, before);
                        Expect("Medusa (boss) còn sau Rosary", 1, First("QueenMedusa") != null && First("QueenMedusa").endRel < 0 ? 1 : 0);
                        int snakesAfter = 0;
                        foreach (var t in Tracks("Snake")) if (t.endRel < 0) snakesAfter++;
                        Expect("rắn còn sau Rosary", 0, snakesAfter);
                        Expect("0 điểm", 0, Last.score);
                        measures.Add("Rosary gỡ " + killed + " thực thể");
                    };
                }
                break;
            case "medusa_kill_l1":
                Room(BossRoomSetup.MEDUSA);
                AddRules();
                Photo(100, 400, 800);
                finish = () => KillChecks("QueenMedusa", VIDEO_MEDUSA_W, WIKI_SCORE_MEDUSA, 1);
                break;

            // ----------------------------------------------- CVM2 09: phòng boss THẬT (section Medusa + BossArena map2_boss.json)
            case "medusa_room_wake":
                RealRoom(200, 144, -1);
                Photo(60, 71, 251, 252, 253, 258, 270, 300, 330, 380, 460);
                finish = () =>
                {
                    var m = First("QueenMedusa");
                    if (m == null) return;
                    int lockRel = FirstRel(0, r => r.locked);
                    Expect("camera khoá ở cx 0 (phòng boss trái)", 0, lockRel >= 0 ? recs[lockRel].cx : -1);
                    Expect("khoá lúc Simon x 128 (video Map_2 f6735)", VIDEO_ROOM_LOCK_SIMON_X, lockRel >= 0 ? recs[lockRel].sx : -1);
                    Expect("không khoá trước khi tới cx 0 (tick khoá mà cx > 0)", 0, CountRel(r => r.locked && r.cx != 0));
                    Expect("tường mép màn trái (WallMinX)", 0, wallMin);
                    Expect("tường mép màn phải (WallMaxX)", 256, wallMax);
                    int wake = MedusaWakeChecks(m, VIDEO_ROOM_STATUE_X, VIDEO_ROOM_FLOOR, lockRel);
                    ExpectRange("ô nền đầu tượng có trước khi thức", 1, 12, statueHeadBefore);
                    Expect("ô nền đầu tượng xoá hết lúc thức", 0, statueHeadAfter);
                    Expect("thân tượng giữ nguyên (ô nền trước = sau)", statueBodyBefore, statueBodyAfter);
                    ExpectRange("thân tượng có ô nền", 1, 8, statueBodyBefore);
                    int badV, badY, badLen, badHover, segs = MedusaSegments(m, VIDEO_ROOM_STATUE_X + VIDEO_MEDUSA_EMERGE_DX, VIDEO_ROOM_FLOOR - VIDEO_MEDUSA_HEAD_UP, VIDEO_ROOM_FLOOR, -1, out badV, out badY, out badLen, out badHover);
                    Expect("ngang ±0.875 px/tick (tick sai)", 0, badV);
                    Expect("dọc nhào (tick sai)", 0, badY);
                    Expect("độ dài đoạn (đoạn sai)", 0, badLen);
                    Expect("dừng không di chuyển (tick sai)", 0, badHover);
                    ExpectRange("số đoạn bay", 2, 99, segs);
                    if (wake >= 0) Expect("đoạn đầu bay TRÁI về Simon (dx tick thức)", -VIDEO_MEDUSA_V, m.fpath[wake].x - Fixed.FromPx(VIDEO_ROOM_STATUE_X + VIDEO_MEDUSA_EMERGE_DX));
                    MedusaFlashChecks(m, wake);
                    SnakeChecks(m, Tracks("Snake"), VIDEO_ROOM_FLOOR, 0, true);
                    Expect("nhạc boss: thức + 57 (tick)", VIDEO_MEDUSA_MUSIC, musicRel - wake);
                    measures.Add("khoá rel " + lockRel + " (Simon x " + (lockRel >= 0 ? recs[lockRel].sx : -1) + "), thức rel " + wake + ", nhạc rel " + musicRel
                                 + ", ô nền đầu tượng " + statueHeadBefore + " → " + statueHeadAfter + ", thân " + statueBodyBefore + " → " + statueBodyAfter);
                };
                break;
            case "medusa_room_touch":
                RealRoom(61, 144, 1);
                CvgpDebug.CheatInvincible = false;
                Photo(181, 205, 215, 230, 260, 300);
                finish = () =>
                {
                    DamageChecks(WIKI_DMG_ST6, 1);
                    var m = First("QueenMedusa");
                    if (m != null) BounceChecks(m);
                };
                break;
            case "medusa_room_holy":
                RealRoom(61, 144, 1);
                ses.subWeapon = eSubWeapon.HolyWater;
                ses.hearts = 10;
                Photo(214, 220, 230, 250);
                finish = () =>
                {
                    var m = First("QueenMedusa");
                    if (m == null) return;
                    ExpectRange("Medusa trúng nước thánh (lọ/lửa)", 1, 99, m.hits);
                    int dec = 0, bad = 0;
                    for (int i = 1; i < bars.Count; i++) { if (bars[i] < bars[i - 1]) dec++; if (bars[i] != bars[i - 1] - 1) bad++; }
                    Expect("mỗi lần ENEMY đổi đúng −1 vạch (video: lọ / lửa 1 vạch)", 0, bad);
                    int stunTicks = 0, stunMoved = 0, stunAnim = 0;
                    for (int k = 1; k < m.fpath.Count; k++)
                        if (m.states[k] == "Stun")
                        {
                            stunTicks++;
                            if (m.states[k - 1] == "Stun" && m.fpath[k] != m.fpath[k - 1]) stunMoved++;
                            if (!m.anims[k].StartsWith("hurt")) stunAnim++;
                        }
                    ExpectRange("Medusa khựng (tick)", 8, 9999, stunTicks);
                    Expect("không di chuyển lúc khựng (tick)", 0, stunMoved);
                    Expect("khung tím (hurt = medusa_stone) lúc khựng (tick sai)", 0, stunAnim);
                    measures.Add("nước thánh: " + m.hits + " lần trúng, khựng " + stunTicks + " tick; vạch ENEMY: " + string.Join(",", bars));
                };
                break;
            case "medusa_room_kill_l3":
                RealRoom(61, 144, 1);
                ses.whipLevel = 3;
                AddRules();
                Photo(300, 600);
                finish = () =>
                {
                    KillChecks("QueenMedusa", VIDEO_MEDUSA_W, WIKI_SCORE_MEDUSA, 1);
                    Expect("quả cầu ở x 128 (cx 0 + 128 — video 128)", 128, crystalX);
                    Expect("quả cầu nằm trên sàn phòng y 144 (trước khi nhặt)", VIDEO_ROOM_FLOOR, crystalY);
                };
                break;

            // ======================================================= Mummy Men (CVM3 09 — đo video Map_3 + Boss_Map_3, CVM3_MEMORY §2i)
            case "mummy_pattern":
                Room(BossRoomSetup.MUMMY);
                PlaceSimon(128, 176, 1);
                Photo(2, 60, 119, 125, 183, 186, 222, 240, 300, 500, 700, 880);
                finish = () =>
                {
                    Track tr, tl;
                    if (!MummyPair(out tr, out tl)) return;
                    Expect("con 2 sinh cách con đầu 191 px về trái", -VIDEO_MUMMY_DX, Fixed.ToPx(tl.fpath[0].x) - Fixed.ToPx(tr.fpath[0].x));
                    int wR = FirstK(tr, k => tr.states[k] != "Dormant"), wL = FirstK(tl, k => tl.states[k] != "Dormant");
                    Expect("thức (tick — camera khoá từ trước tick 0, đếm từ tick 1 ⇒ 120)", VIDEO_MUMMY_WAKE, tr.spawnRel + wR);
                    Expect("2 con thức cùng tick", tr.spawnRel + wR, tl.spawnRel + wL);
                    var mR = tr.e as MummyMan;
                    Expect("nhạc boss: khoá + 60 (tick)", VIDEO_MUMMY_MUSIC, mR != null && mR.musicAtTick >= 0 ? mR.musicAtTick - startTick - 1 : -1);
                    DormantChecks(tr, wR);
                    DormantChecks(tl, wL);
                    MummyWalkChecks(tr, wR, "con đầu");
                    MummyWalkChecks(tl, wL, "con 2");
                    ThrowChecks(tr, wR, VIDEO_MUMMY_THROW_FIRST, "con đầu");
                    ThrowChecks(tl, wL, VIDEO_MUMMY_THROW_FIRST2, "con 2");
                    var bs = ShotTracks("MummyBandage");
                    ExpectRange("số băng ném", 3, 99, bs.Count);
                    BandageChecks(bs, 176);
                    // con đầu ở TRƯỚC mặt Simon (Simon quay phải): giữ cự ly — không vào gần hơn near − 12 (lưới quyết định 16 tick × 0.75)
                    int close = 0;
                    for (int k = wR; k < tr.fpath.Count; k++)
                        if (tr.states[k] == "Walk" && Math.Abs(Fixed.ToPx(tr.fpath[k].x) - recs[tr.spawnRel + k].sx) < VIDEO_MUMMY_NEAR - 13) close++;
                    Expect("con đầu trước mặt Simon không vào gần hơn 33 px (tick)", 0, close);
                    int dxL0 = Math.Abs(Fixed.ToPx(tl.fpath[wL].x) - recs[tl.spawnRel + wL].sx);
                    int dxL1 = wL + 100 < tl.fpath.Count ? Math.Abs(Fixed.ToPx(tl.fpath[wL + 100].x) - recs[tl.spawnRel + wL + 100].sx) : 9999;
                    ExpectRange("con 2 sau lưng Simon tiến lại gần (|dx| sau 100 tick nhỏ hơn lúc thức)", 0, dxL0 - 1, dxL1);
                    measures.Add("thức rel " + (tr.spawnRel + wR) + "; con đầu đổi hướng " + (mR != null ? mR.turns : -1) + " lần, ném " + (mR != null ? mR.throws : -1)
                                 + "; con 2 ném " + (tl.e is MummyMan ? ((MummyMan)tl.e).throws : -1) + "; băng " + bs.Count);
                };
                break;
            case "mummy_whip_bandage":
                Room(BossRoomSetup.MUMMY);
                PlaceSimon(128, 176, 1);
                ses.whipLevel = 3;
                Photo(220, 240, 300);
                finish = () =>
                {
                    ExpectRange("băng gạc bị roi phá (video Boss_Map_3 f505–507, f637–640)", 1, 99, brokenShots);
                    int deaths = 0;
                    foreach (var t in Tracks("MummyMan")) if (t.defeated) deaths++;
                    Expect("+100 mỗi băng phá (+3000 mỗi Mummy hạ)", brokenShots * WIKI_PROJ_SCORE + deaths * VIDEO_MUMMY_SCORE, Last.score);
                    measures.Add("phá " + brokenShots + " băng, Mummy hạ " + deaths + ", điểm " + Last.score);
                };
                break;
            case "mummy_stand":
                Room(BossRoomSetup.MUMMY);
                CvgpDebug.CheatInvincible = false;
                PlaceSimon(80, 176, 1);
                Photo(260, 330);
                finish = () =>
                {
                    int bandHits = 0;
                    foreach (var h in playerHits) if (h.StartsWith("EnemyAttack:MummyBandage")) bandHits++;
                    ExpectRange("băng gạc trúng Simon ĐỨNG", 1, 99, bandHits);
                    DamageChecks(WIKI_DMG_ST9, 1);
                    measures.Add("băng trúng " + bandHits + "; " + string.Join(" ", playerHits));
                };
                break;
            case "mummy_holy":
                Room(BossRoomSetup.MUMMY);
                PlaceSimon(128, 176, 1);
                ses.subWeapon = eSubWeapon.HolyWater;
                ses.hearts = 10;
                Photo(150, 180, 200, 240, 270);
                onTick = rel => { if (AnyHolyFire()) fireLastRel = rel; };
                finish = () => HolyChecks(true);
                break;
            case "mummy_stopwatch":
                Room(BossRoomSetup.MUMMY);
                PlaceSimon(128, 176, 1);
                ses.subWeapon = eSubWeapon.Stopwatch;
                ses.hearts = 10;
                Photo(200);
                finish = () =>
                {
                    int f0 = FirstRel(0, r => r.frozenE);
                    ExpectRange("Stopwatch bắt đầu (tick — bấm 101 + ném)", 100, 120, f0);
                    int moved = 0, bandMoved = 0;
                    foreach (var t in Tracks("MummyMan"))
                        for (int rel = f0 + 1; rel < f0 + 200 && rel < recs.Count; rel++)
                            if (PosAt(t, rel) != PosAt(t, rel - 1)) moved++;
                    foreach (var b in ShotTracks("MummyBandage"))
                        for (int k = 1; k < b.fpath.Count; k++)
                            if (b.spawnRel + k > f0 && b.spawnRel + k < f0 + 200 && b.fpath[k] != b.fpath[k - 1]) bandMoved++;
                    ExpectRange("Mummy vẫn đi khi Stopwatch (tick-con di chuyển)", 20, 9999, moved);
                    ExpectRange("băng vẫn bay khi Stopwatch (tick)", 1, 9999, bandMoved);
                    measures.Add("Stopwatch tick " + f0 + ": Mummy di chuyển " + moved + " tick-con, băng " + bandMoved);
                };
                break;
            case "mummy_kill_l1":
                Room(BossRoomSetup.MUMMY);
                AddRules();
                Photo(100, 600, 1200);
                finish = () => MummyKillChecks();
                break;

            // ----------------------------------------------- CVM3 09: phòng boss THẬT (section MummyMan + BossArena map3_boss.json)
            case "mummy_room_wake":
                RealRoom3(1260, 96, 1, false);
                Photo(2, 60, 140, 149, 210, 269, 300, 333, 380, 420, 500);
                finish = () =>
                {
                    Track tr, tl;
                    if (!MummyPair(out tr, out tl)) return;
                    Expect("con đầu chờ ở x 1503 (video)", VIDEO_ROOM_MUMMY_R, Fixed.ToPx(tr.fpath[0].x));
                    Expect("con 2 chờ ở x 1312 (video)", VIDEO_ROOM_MUMMY_L, Fixed.ToPx(tl.fpath[0].x));
                    int lockRel = FirstRel(0, r => r.locked);
                    Expect("camera khoá ở cx 1280 (phòng boss phải)", VIDEO_ROOM_LOCK_CX, lockRel >= 0 ? recs[lockRel].cx : -1);
                    Expect("khoá lúc Simon x 1408 (video Map_3 f5931)", VIDEO_ROOM_LOCK_SIMON_X3, lockRel >= 0 ? recs[lockRel].sx : -1);
                    Expect("không khoá trước khi tới cx 1280 (tick khoá mà cx ≠ 1280)", 0, CountRel(r => r.locked && r.cx != VIDEO_ROOM_LOCK_CX));
                    Expect("tường mép màn trái (WallMinX)", VIDEO_ROOM_LOCK_CX, wallMin);
                    Expect("tường mép màn phải (WallMaxX)", VIDEO_ROOM_LOCK_CX + 256, wallMax);
                    int wR = FirstK(tr, k => tr.states[k] != "Dormant"), wL = FirstK(tl, k => tl.states[k] != "Dormant");
                    Expect("thức = khoá + 120 (tick — video 2 lần 119.5–120.9)", VIDEO_MUMMY_WAKE, tr.spawnRel + wR - lockRel);
                    Expect("2 con thức cùng tick", tr.spawnRel + wR, tl.spawnRel + wL);
                    var mR = tr.e as MummyMan;
                    Expect("nhạc boss = khoá + 60 (tick)", VIDEO_MUMMY_MUSIC, mR != null && mR.musicAtTick >= 0 ? mR.musicAtTick - startTick - 1 - lockRel : -1);
                    DormantChecks(tr, wR);
                    DormantChecks(tl, wL);
                    ThrowChecks(tr, wR, VIDEO_MUMMY_THROW_FIRST, "con đầu");
                    ThrowChecks(tl, wL, VIDEO_MUMMY_THROW_FIRST2, "con 2");
                    BandageChecks(ShotTracks("MummyBandage"), 176);
                    Expect("khối Pot Roast (83,9) còn nguyên (ConfigDrop MummyMan_X83_9)", (int)eItemId.PotRoast, potRoastContent);
                    measures.Add("khoá rel " + lockRel + " (Simon x " + (lockRel >= 0 ? recs[lockRel].sx : -1) + "), thức rel " + (tr.spawnRel + wR)
                                 + ", nhạc rel " + (mR != null ? mR.musicAtTick - startTick - 1 : -1) + ", tường [" + wallMin + ", " + wallMax + ")");
                };
                break;
            case "mummy_room_holy":
                RealRoom3(1302, 112, 1, false);
                if (world.CameraCtl != null) world.CameraCtl.Lock(VIDEO_ROOM_LOCK_CX);   // video: Simon đã qua mốc khoá rồi lùi lên khối trái
                CvgpDebug.CheatInvincible = false;
                ses.subWeapon = eSubWeapon.HolyWater;
                ses.hearts = 55;
                Photo(300, 340, 470, 620, 900, 1200, 1500);
                onTick = rel => { if (AnyHolyFire()) fireLastRel = rel; };
                finish = () =>
                {
                    HolyChecks(false);
                    Expect("Simon trên khối trái không trúng đòn (video Map_3 0 lần)", GameSession.MAX_HP, Last.hp);
                    measures.Add("ENEMY theo thời gian: " + string.Join(",", bars) + "; con đầu hạ rel " + firstDefeatRel + ", con cuối rel " + defeatRel
                                 + " (video Map_3: 9 lọ ⇒ 16 vạch, Mummy 1 chết rel ≈ 1510, Mummy 2 ≈ 1683)");
                };
                break;
            case "mummy_room_kill_l3":
                RealRoom3(1409, 176, -1, false);
                ses.whipLevel = 3;
                AddRules();
                Photo(300, 700, 1100);
                finish = () =>
                {
                    MummyKillChecks();
                    Expect("quả cầu ở x 1408 (cx 1280 + 128 — video)", VIDEO_ROOM_LOCK_CX + 128, crystalX);
                    int up = 0, maxY = 0;
                    for (int i = 1; i < crystalYs.Count; i++) { if (crystalYs[i] < crystalYs[i - 1]) up++; maxY = Math.Max(maxY, crystalYs[i]); }
                    Expect("quả cầu chỉ rơi xuống (lần đi lên)", 0, up);
                    ExpectRange("quả cầu rơi tới ≤ sàn phòng y 176 (video chạm sàn f6859) trước khi nhặt", 105, 176, maxY);
                };
                break;
            case "mummy_room_ii":
                RealRoom3(1478, 176, 1, true);
                ses.whipLevel = 3;
                ses.subWeapon = eSubWeapon.HolyWater;
                ses.subWeaponHits = 10;
                ses.hearts = 46;
                Photo(10, 20, 30, 45, 70);
                finish = () =>
                {
                    Expect("nến phải (1503, 128) vỡ ⇒ II (tim nhỏ đổi Double Shot — vũ khí phụ đã trúng ≥ 10 lần; video Map_3 f6841–6864)", 1, doubleShotSeen);
                    Expect("nhặt II ⇒ Shot ×2", 2, GameSession.Current.shotMultiplier);
                    measures.Add("II hiện rel " + doubleShotRel + ", hệ số " + GameSession.Current.shotMultiplier);
                };
                onTick = rel =>
                {
                    foreach (var e in world.GroupList(eEntityGroup.Item))
                    {
                        var it = e as ItemPickup;
                        if (it != null && it.Alive && it.id == eItemId.DoubleShot && doubleShotSeen == 0) { doubleShotSeen = 1; doubleShotRel = rel; }
                    }
                };
                break;

            // ======================================================= Frankenstein & Igor
            case "frank_pattern":
                Room(BossRoomSetup.FRANK);
                Photo(8, 60, 116, 160, 250, 330, 420, 600, 800);
                finish = () =>
                {
                    var m = First("Frankenstein");
                    var ig = First("Igor");
                    if (m == null || ig == null) return;
                    int wake = FirstK(m, k => m.states[k] != "Dormant");
                    Expect("Monster thức (tick — 16)", EST_FRANK_WAKE - 1, wake);
                    Expect("Monster nhận đòn từ lúc thức (tick hộp tắt sau đó)", 0, CountK(m, k => k >= wake && !m.active[k]));
                    int leave = FirstK(ig, k => ig.states[k] != "Ride");
                    ExpectRange("Igor rời vai (tick sau khi Monster thức — 96)", EST_IGOR_START - 1, EST_IGOR_START + 1, leave - wake);
                    int badRide = 0;
                    for (int k = 0; k < leave && k < ig.fpath.Count; k++)
                        if (ig.fpath[k].x != m.fpath[k].x || Fixed.ToPx(ig.fpath[k].y) != Fixed.ToPx(m.fpath[k].y) - EST_IGOR_RIDE) badRide++;
                    Expect("Igor trên vai Monster (Y − 40) (tick sai)", 0, badRide);
                    int badMV = 0, outZone = 0, inZone = -1;
                    for (int k = wake + 1; k < m.fpath.Count; k++)
                    {
                        int d = Math.Abs(m.fpath[k].x - m.fpath[k - 1].x);
                        if (d != 0 && d != EST_FRANK_V) badMV++;
                        int x = Fixed.ToPx(m.fpath[k].x);
                        bool inside = x >= EST_FRANK_ZONE_MIN - 4 && x <= EST_FRANK_ZONE_MAX + 4;
                        if (inside && inZone < 0) inZone = k;
                        if (inZone >= 0 && !inside) outZone++;
                    }
                    Expect("Monster đi 0.25 px/tick (tick sai)", 0, badMV);
                    Expect("Monster ở nửa phải [128, 216] ±4 (tick ra ngoài)", 0, outZone);
                    var igor = ig.e as Igor;
                    ExpectRange("Igor nhảy tiến", 3, 999, igor != null ? igor.shortHops : 0);
                    ExpectRange("Igor nhảy cao (gần góc)", 1, 999, igor != null ? igor.longHops : 0);
                    ExpectRange("Igor đổi phía", 2, 999, igor != null ? igor.sideFlips : 0);
                    var balls = ShotTracks("BoneTowerFireball");
                    ExpectRange("cầu lửa Igor", 2, 999, balls.Count);
                    int hops = igor != null ? igor.shortHops + igor.longHops : 0;
                    ExpectRange("cầu lửa = 1/2 số cú nhảy (đỉnh cú thứ 2, 4, …)", hops / EST_IGOR_FIRE_EVERY - 1, hops / EST_IGOR_FIRE_EVERY, balls.Count);
                    int badBall = 0;
                    foreach (var b in balls)
                    {
                        if (b.fpath.Count < 3) continue;
                        long vx = b.fpath[1].x - b.fpath[0].x, vy = b.fpath[1].y - b.fpath[0].y;
                        double sp = Math.Sqrt(vx * vx + vy * vy);
                        if (Math.Abs(sp - EST_IGOR_BALL_V) > 2) badBall++;
                    }
                    Expect("cầu lửa 1.5 px/tick (quả sai)", 0, badBall);
                    int onPlat = 0;
                    for (int k = 0; k < ig.fpath.Count; k++) if (ig.states[k] == "Ground" && Fixed.ToPx(ig.fpath[k].y) == 144) onPlat++;
                    ExpectRange("Igor đứng trên bục góc (tick — nhảy cao lên bục)", 1, 9999, onPlat);
                    measures.Add("Monster thức " + wake + ", Igor rời vai " + leave + "; Igor nhảy tiến " + (igor != null ? igor.shortHops : 0) + " / cao "
                                 + (igor != null ? igor.longHops : 0) + ", đổi phía " + (igor != null ? igor.sideFlips : 0) + ", cầu lửa " + balls.Count);
                };
                break;
            case "frank_igor_freeze":
                {
                    Go("Arena", 100, 176, 1);
                    world.Stage = 12;
                    var ig = world.Spawn(Prefab<Igor>("Igor"), 124, 176, -1) as Igor;
                    ig.pauseLen = 99999;   // đứng yên (không có Monster) để roi ngồi trúng
                    Photo(20, 40, 100, 150);
                    finish = () =>
                    {
                        var t = First("Igor");
                        if (t == null) return;
                        Expect("roi trúng Igor (lần)", 1, t.hits);
                        Expect("Igor không chết (bất tử)", 0, t.defeated || t.endRel >= 0 ? 1 : 0);
                        Expect("máu Igor không đổi", t.hp[0], t.hp[t.hp.Count - 1]);
                        Expect("0 điểm", 0, Last.score);
                        int k0 = FirstK(t, k => t.states[k] == "Stun"), k1 = FirstK(t, k => k > k0 && t.states[k] != "Stun");
                        Expect("đứng hình (tick — 120; đòn sau kéo dài)", EST_IGOR_STUN, k1 - k0);
                        measures.Add("Igor đứng hình từ tick " + k0 + " tới " + k1 + ", trúng " + t.hits + " lần");
                    };
                }
                break;
            case "frank_touch":
                Room(BossRoomSetup.FRANK);
                CvgpDebug.CheatInvincible = false;
                Photo(300);
                finish = () => DamageChecks(WIKI_DMG_ST12, 1);
                break;
            case "frank_stopwatch":
                Room(BossRoomSetup.FRANK);
                ses.subWeapon = eSubWeapon.Stopwatch;
                ses.hearts = 10;
                finish = () =>
                {
                    int f0 = FirstRel(0, r => r.frozenE);
                    ExpectRange("Stopwatch bắt đầu (tick — bấm 131 + ném)", 130, 150, f0);
                    int mm = 0, im = 0;
                    var m = First("Frankenstein");
                    var ig = First("Igor");
                    if (m == null || ig == null) return;
                    for (int rel = f0 + 1; rel < f0 + 200 && rel < recs.Count; rel++)
                    {
                        if (PosAt(m, rel) != PosAt(m, rel - 1)) mm++;
                        if (PosAt(ig, rel) != PosAt(ig, rel - 1)) im++;
                    }
                    ExpectRange("Monster vẫn đi khi Stopwatch (tick)", 10, 9999, mm);
                    ExpectRange("Igor vẫn nhảy khi Stopwatch (tick)", 10, 9999, im);
                    measures.Add("Stopwatch tick " + f0 + ": Monster " + mm + " tick di chuyển, Igor " + im);
                };
                break;
            case "frank_kill_l1":
                Room(BossRoomSetup.FRANK);
                AddRules();
                Photo(100, 600, 1200);
                finish = () =>
                {
                    KillChecks("Frankenstein", WIKI_FRANK_W, WIKI_SCORE_FRANK, 1);
                    var ig = First("Igor");
                    Expect("Igor biến mất khi Monster bị hạ", 1, ig != null && ig.endRel >= 0 && defeatRel >= 0 && Math.Abs(ig.endRel - defeatRel) <= 1 ? 1 : 0);
                };
                break;
            default:
                Debug.LogError("[BossB24Probe] không có kịch bản " + scenario);
                return -1;
        }
        s.ResetStats();
        if (pilot)
        {
            pilotOn = true;
            pilotPrev = 0;
            pilotEnd = -1;
            NesPad.I.StopReplay();
            NesPad.I.StartRecord();
            Begin(scenario, PILOT_MAX_TICKS);
            return PILOT_MAX_TICKS;
        }
        NesPad.I.PlayReplay(ticks);
        Begin(scenario, ticks.Count);
        return ticks.Count;
    }

    // ---------------------------------------------------------------- autopilot (chỉ khi GHI)

    /// <summary>Quyết định nút cho tick KẾ TIẾP (gọi cuối tick): quay về mục tiêu, quất khi đoán trúng (tầm roi da: 12–36 px trước mặt,
    /// hàng chân −24..−17 — hộp WhipRect), không thì đi tới; có quả cầu thì đi nhặt. Không đọc RNG thế giới (giữ tất định).</summary>
    static int PilotDecide()
    {
        var s = Simon;
        if (s == null || !s.Alive)
            return 0;
        if (crystal != null && crystal.Alive)
            return ToCrystal(s);
        if (!s.Fsm.Is(eSimonState.Idle) && !s.Fsm.Is(eSimonState.Walk))
            return 0;
        EnemyBase tgt = null;
        int best = int.MaxValue;
        foreach (var e in world.GroupList(eEntityGroup.Enemy))
        {
            var b = e as BossBase;
            if (b == null || !b.Alive || b.dying || b.dormant || b.hidden || b.defeated) continue;
            int d = Math.Abs(b.X - s.X);
            if (d < best) { best = d; tgt = b; }
        }
        if (tgt == null)
            return 0;
        int dx = tgt.X - s.X;
        // vị trí đoán sau ~16 tick (roi duỗi ở tick 13–21 sau khi bấm)
        var tr = Tracks(tgt.GetType().Name).Find(t => t.e == tgt);
        int vx = 0;
        if (tr != null && tr.fpath.Count >= 2) vx = tr.fpath[tr.fpath.Count - 1].x - tr.fpath[tr.fpath.Count - 2].x;
        int px = Fixed.ToPx(tgt.fx + vx * 16);
        int face = s.Facing;
        if (Hits(s, tgt, px, face) && (pilotPrev & (1 << (int)eNesButton.B)) == 0)
            return 1 << (int)eNesButton.B;
        int want = px != s.X ? Math.Sign(px - s.X) : face;
        if (want != face)
            return Dir(want);
        if (tgt is QueenMedusa)
            return Math.Abs(dx) > 120 ? Dir(dx) : 0;   // Medusa tự lao tới: đứng chờ
        return Math.Abs(px - s.X) > 30 ? Dir(px - s.X) : 0;
    }

    /// <summary>Đi nhặt quả cầu; cầu nằm tầng trên (gờ phòng Mummy) ⇒ tới chân thang của tầng đó rồi giữ ↑ leo lên.</summary>
    static int ToCrystal(SimonController s)
    {
        int up = 1 << (int)eNesButton.Up;
        if (s.OnStairs)
            return up;
        if (crystal.Y >= s.Y - 8 || world.Tiles == null)
            return Dir(crystal.X - s.X);
        for (int i = 0; i < world.Tiles.StairCount; i++)
        {
            var st = world.Tiles.GetStair(i);
            if (st.bottom.y != s.Y || st.top.y > crystal.Y + 8)
                continue;
            int dx = st.bottom.x - s.X;
            // CVM3 09: chân thang phòng Mummy thử (244) ngoài mép màn khoá cx + 239 (CVM3 04) ⇒ đứng sát mép (lệch 5) rồi giữ ↑
            return Math.Abs(dx) > 5 ? Dir(dx) : up;
        }
        return Dir(crystal.X - s.X);
    }

    static bool Hits(SimonController s, EnemyBase tgt, int px, int face)
    {
        var whip = new RectInt(face > 0 ? s.X + 12 : s.X - 36, s.Y - 24, 24, 8);
        var hb = tgt.Hurt.SectionRect;
        hb.x += px - tgt.X;
        return CombatResolver.Overlaps(whip, hb);
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
        defeatRel = crystalRel = pickRel = firstDefeatRel = -1;
        brokenShots = 0;
        crystal = null;
        startTick = world.TickCount;
        foreach (var e in world.GroupList(eEntityGroup.Enemy))
            TrackOf(e, 0);
        world.TickEnded += OnTickEnded;
        world.Hit += OnHit;
        EnemyBase.Defeated += OnDefeated;
        BossBase.BossDefeated += OnBossDefeated;
        BossBase.CrystalSpawned += OnCrystal;
        ItemEffects.CrystalPicked += OnPicked;
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
        Running = false;
    }

    private static int Rel => world.TickCount - startTick - 1;

    private static void OnHit(HitInfo h)
    {
        var t = TrackOf(h.victim, Rel);
        if (t != null) t.hits++;
        if (h.victim == world.Player)
        {
            if (h.attacker is QueenMedusa)
                medusaHitRels.Add(Rel);
            playerHits.Add(h.hitbox.layer + ":" + (h.attacker != null ? h.attacker.GetType().Name : "?") + "@" + Rel + "−" + h.damage);
            playerDmg.Add(h.damage);
        }
        var f = h.victim as BoneTowerFireball;
        if (f != null && f.broken) brokenShots++;
        var bd = h.victim as MummyBandage;   // CVM3 09: băng gạc Mummy phá được (+100)
        if (bd != null && bd.broken) brokenShots++;
    }

    private static void OnDefeated(EnemyBase e)
    {
        if (firstDefeatRel < 0 && e is BossBase) firstDefeatRel = Rel;
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
                t.states.Add(e.DebugState.Split(' ')[0]);
                var en = e as EnemyBase;
                t.active.Add(en != null && en.Hurt != null && en.Hurt.active);
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
        var r = new Rec
        {
            sx = s.X, sy = s.Y, hp = ses.playerHp, score = ses.score, state = s.Fsm.CurState, cx = w.CameraCtl != null ? w.CameraCtl.X : 0,
            fx = fx, frozenE = w.IsEnemyFrozen, shots = shotsN, ebar = ses.enemyHp, locked = w.CameraCtl != null && w.CameraCtl.Locked,
        };
        recs.Add(r);
        Mix(rel); Mix(s.fx); Mix(s.fy); Mix((int)r.state); Mix(r.hp); Mix(r.score); Mix(r.ebar); Mix(fx); Mix(shotsN); Mix(w.Rng.State);
        string shot;
        if (shots.TryGetValue(rel, out shot))
            ScreenCapture.CaptureScreenshot(shot);
        WatchMedusa(rel);
        if (crystal != null && crystal.Alive)
            crystalYs.Add(crystal.Y);
        if (realRoom3 && w.Tiles != null && w.Tiles.WallMinX != int.MinValue && wallMin == int.MinValue)
        {
            wallMin = w.Tiles.WallMinX;   // CVM3 09: WatchMedusa chỉ ghi khi có Medusa
            wallMax = w.Tiles.WallMaxX;
        }
        if (onTick != null)
            onTick(rel);
        // ảnh theo sự kiện (kịch bản hạ boss): con đầu bị hạ, lửa chết, quả cầu rơi, sau khi nhặt
        if (!string.IsNullOrEmpty(ShotDir) && !pilotOn)
        {
            string tag = firstDefeatRel >= 0 && rel == firstDefeatRel + 6 ? "defeat1" : defeatRel >= 0 && rel == defeatRel + 8 ? "deathfx"
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
            // tick đầu đã ghi = tick 1 của kịch bản (bắt đầu ghi trước tick 1) — số tick ghi = số tick chạy
            WriteReplay(Label, rec, Find(Label).note + " — ghi bằng autopilot (CVGP 19), hash chạy ghi " + Hash.ToString("x8"));
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
        if (addedRules && rules != null)
        {
            rules.enabled = false;
            UnityEngine.Object.DestroyImmediate(rules);   // kịch bản kế chạy NGAY cùng frame (như BossB56Probe; CVM2 00)
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

    /// <summary>Vết thực thể kịch bản vừa chạy: "kind,spawnRel,k,rel,x,y,state,simonX,simonY,php,ebar" (px).</summary>
    public static string TraceCsv()
    {
        var sb = new StringBuilder("kind,spawnRel,k,rel,x,y,state,simonX,simonY,php,ebar\n");
        foreach (var t in tracks)
            for (int k = 0; k < t.fpath.Count; k++)
            {
                int rel = t.spawnRel + k;
                bool ok = rel >= 0 && rel < recs.Count;
                sb.Append(t.kind).Append(',').Append(t.spawnRel).Append(',').Append(k).Append(',').Append(rel).Append(',')
                  .Append(Fixed.ToPx(t.fpath[k].x)).Append(',').Append(Fixed.ToPx(t.fpath[k].y)).Append(',').Append(t.states[k]).Append(',')
                  .Append(ok ? recs[rel].sx : 0).Append(',').Append(ok ? recs[rel].sy : 0).Append(',').Append(ok ? recs[rel].hp : 0).Append(',')
                  .Append(ok ? recs[rel].ebar : 0).Append('\n');
            }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- chấm

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

    static List<Track> ShotTracks(string kind) { return Tracks(kind); }

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

    /// <summary>Vị trí px của thực thể ở tick rel (ngoài vết ⇒ (−9999, −9999)).</summary>
    static Vector2Int PosAt(Track t, int rel)
    {
        int k = rel - t.spawnRel;
        if (k < 0 || k >= t.fpath.Count) return new Vector2Int(-9999, -9999);
        return new Vector2Int(Fixed.ToPx(t.fpath[k].x), Fixed.ToPx(t.fpath[k].y));
    }

    static int[] Wave(int amp, int period)
    {
        var w = new int[period];
        for (int i = 0; i < period; i++)
            w[i] = -(int)Math.Round(amp * Math.Sin(2.0 * Math.PI * i / period) * Fixed.ONE);
        return w;
    }

    static int OnScreen(eEntityGroup g)
    {
        int cx = world.CameraCtl != null ? world.CameraCtl.X : 0, n = 0;
        foreach (var e in world.GroupList(g))
            if (e.Alive && e.X >= cx && e.X < cx + NesCoord.SCREEN_W) n++;
        return n;
    }

    /// <summary>Mọi đòn lên Simon = mức stage (wiki); máu giảm đúng tổng.</summary>
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

    /// <summary>Hạ boss bằng roi da (autopilot): đúng số đòn yếu wiki, thanh ENEMY giảm theo đòn, điểm, lửa, quả cầu 152, nhặt ⇒ tally.</summary>
    static void KillChecks(string kind, int hitsWeak, int score, int entities, int crystalDelay = 152)
    {
        var ts = Tracks(kind);
        Expect("số thực thể boss", entities, ts.Count);
        int hits = 0, defeated = 0;
        foreach (var t in ts) { hits += t.hits; if (t.defeated) defeated++; }
        Expect(kind + " số đòn roi da tới khi hạ hết (wiki " + hitsWeak + ")", hitsWeak, hits);
        if (entities > 1)
            foreach (var t in ts) Expect(kind + " con (sinh rel " + t.spawnRel + ") số đòn (" + hitsWeak / entities + ")", hitsWeak / entities, t.hits);
        Expect("bị hạ (thực thể)", entities, defeated);
        ExpectRange("hạ (tick rel)", 1, PILOT_MAX_TICKS, defeatRel);
        int snakeKills = 0;   // CVM2 09: rắn Medusa +100 (video Map_2 3:48.20) — roi hạ rắn trên đường quất Medusa
        foreach (var t in Tracks("Snake")) if (t.defeated && t.endRel >= 0 && defeatRel >= 0 && t.endRel <= defeatRel) snakeKills++;
        Expect("+" + score + " (+100 × cầu lửa phá " + brokenShots + ", +100 × rắn hạ " + snakeKills + ")", score + brokenShots * WIKI_PROJ_SCORE + snakeKills * VIDEO_SNAKE_SCORE,
               defeatRel >= 0 ? recs[defeatRel].score : -1);
        Expect("ENEMY 0 lúc hạ", 0, defeatRel >= 0 ? recs[defeatRel].ebar : -1);
        int dec = 0;
        for (int i = 1; i < bars.Count; i++) if (bars[i] > bars[i - 1]) dec++;
        Expect("thanh ENEMY chỉ giảm (lần tăng)", 0, dec);
        ExpectRange("quả cầu sau khi hạ (tick — " + crystalDelay + ")", crystalDelay - 1, crystalDelay + 1, crystalRel - defeatRel);
        ExpectRange("Simon nhặt quả cầu", crystalRel + 1, 99999, pickRel);
        Expect("máu Simon đầy sau khi nhặt", GameSession.MAX_HP, pickRel >= 0 && pickRel < recs.Count ? recs[pickRel].hp : -1);
        Expect("tally đã bắt đầu (StageRules)", 1, rules != null && rules.Tally.Active ? 1 : 0);
        measures.Add(kind + ": " + hits + " đòn roi da, hạ ở tick " + defeatRel + " (" + (defeatRel / 60.0).ToString("0.0") + " s), cầu " + crystalRel
                     + ", nhặt " + pickRel + "; vạch ENEMY: " + string.Join(",", bars));
    }

    // ---------------------------------------------------------------- CVM3 09: chấm Mummy Men / băng gạc theo số đo video

    /// <summary>Vết 2 Mummy: tr = con đầu (phải), tl = con 2 (trái).</summary>
    static bool MummyPair(out Track tr, out Track tl)
    {
        tr = tl = null;
        foreach (var t in Tracks("MummyMan"))
        {
            var m = t.e as MummyMan;
            if (m != null && m.second) { if (tl == null) tl = t; }
            else if (tr == null) tr = t;
        }
        Expect("số Mummy (con đầu + con 2)", 2, (tr != null ? 1 : 0) + (tl != null ? 1 : 0));
        return tr != null && tl != null;
    }

    static int AnimIdx(Track t, int k)
    {
        var a = t.anims[k];
        int c = a.LastIndexOf(':');
        return c >= 0 ? int.Parse(a.Substring(c + 1)) : -1;
    }

    /// <summary>Trước khi thức: đứng yên, khung walk_purple 0, không nhận/gây đòn.</summary>
    static void DormantChecks(Track t, int wake)
    {
        int bad = 0;
        for (int k = 0; k < wake && k < t.fpath.Count; k++)
            if (t.fpath[k] != t.fpath[0] || t.anims[k] != "walk_purple:0" || t.active[k]) bad++;
        Expect("đứng chờ: yên, khung tím 0, không va chạm (tick sai)", 0, bad);
    }

    /// <summary>Sau khi thức: đi 0.75 px/tick; khung [0,1,2,1] mỗi 16 tick chạy liên tục (cả lúc ném).</summary>
    static void MummyWalkChecks(Track t, int wake, string who)
    {
        int badV = 0, moves = 0, partial = 0;
        for (int k = wake + 1; k < t.fpath.Count; k++)
        {
            int d = Math.Abs(t.fpath[k].x - t.fpath[k - 1].x);
            if (d > VIDEO_MUMMY_V) badV++;
            else if (d != 0 && d != VIDEO_MUMMY_V) partial++;   // chạm ô đặc ngang chân (phòng thử: khối (6,10) trên sàn) ⇒ bước ngắn
            if (d != 0) moves++;
        }
        Expect(who + " đi 0.75 px/tick (tick nhanh hơn)", 0, badV);
        ExpectRange(who + " bước ngắn (chạm tường) ≤ 3 lần", 0, 3, partial);
        ExpectRange(who + " có đi (tick)", 30, 99999, moves);
        var runs = new List<int[]>();   // (khung, độ dài)
        for (int k = wake; k < t.fpath.Count; k++)
        {
            if (t.states[k] == "Stun") break;
            int i = AnimIdx(t, k);
            if (runs.Count > 0 && runs[runs.Count - 1][0] == i) runs[runs.Count - 1][1]++;
            else runs.Add(new[] { i, 1 });
        }
        int badLen = 0, badSeq = 0;
        for (int r = 1; r < runs.Count; r++)
        {
            if (r < runs.Count - 1 && runs[r][1] != VIDEO_MUMMY_ANIM) badLen++;
            if (runs[r][0] != (runs[r - 1][0] + 1) % 4) badSeq++;
        }
        Expect(who + " khung walk mỗi 16 tick (đoạn sai)", 0, badLen);
        Expect(who + " thứ tự khung 0→1→2→1(3)→0 (lần sai)", 0, badSeq);
    }

    /// <summary>Lần ném đầu: sau first tick đi, đứng 48 (47 tick Throw), nháy đỏ/tím mỗi 2 tick tới tick 40 ⇒ băng ở X Mummy.</summary>
    static void ThrowChecks(Track t, int wake, int first, string who)
    {
        int k0 = FirstK(t, k => k > wake && t.states[k] == "Throw");
        Expect(who + " lần ném đầu sau " + first + " tick đi (tick)", first - 1, k0 - wake);
        if (k0 < 0) return;
        int len = 0, moved = 0, badFlash = 0;
        for (int k = k0; k < t.fpath.Count && t.states[k] == "Throw"; k++)
        {
            len++;
            if (t.fpath[k].x != t.fpath[k0].x) moved++;
            int j = k - k0 + 1;
            bool red = t.anims[k].StartsWith("walk_red");
            bool want = j < VIDEO_MUMMY_RELEASE && ((j - 1) / VIDEO_MUMMY_FLASH) % 2 == 0;
            if (red != want) badFlash++;
        }
        Expect(who + " đứng ném (tick Throw — 48 − 1)", VIDEO_MUMMY_POSE - 1, len);
        Expect(who + " không di chuyển lúc ném (tick)", 0, moved);
        Expect(who + " nháy đỏ↔tím mỗi 2 tick tới lúc ném (tick sai)", 0, badFlash);
        int rel = t.spawnRel + k0 + VIDEO_MUMMY_RELEASE - 1, found = 0;
        foreach (var b in ShotTracks("MummyBandage"))
            if (b.spawnRel == rel) found++;
        Expect(who + " băng sinh ở tick 40 của tư thế (băng)", 1, found);
    }

    /// <summary>Băng: ±1.5 px/tick đều; y = y0 + 24·sin(2πt/102) (đi xuống trước); y0 = chân − 31 / − 15; mất cách mép màn 8.</summary>
    static void BandageChecks(List<Track> bs, int floorY)
    {
        var wave = new int[VIDEO_BANDAGE_T];
        for (int i = 0; i < VIDEO_BANDAGE_T; i++)
            wave[i] = (int)Math.Round(Fixed.FromPx(VIDEO_BANDAGE_AMP) * Math.Sin(2.0 * Math.PI * i / VIDEO_BANDAGE_T));
        int badV = 0, badY = 0, badY0 = 0, badEnd = 0, high = 0, low = 0;
        foreach (var b in bs)
        {
            if (b.fpath.Count < 2) continue;
            int best = int.MaxValue, age0 = 0;
            for (int a = 0; a <= 1; a++)
            {
                int bad = 0;
                for (int k = 1; k < b.fpath.Count; k++)
                    if (b.fpath[k].y - b.fpath[0].y != wave[(a + k) % VIDEO_BANDAGE_T] - wave[a % VIDEO_BANDAGE_T]) bad++;
                if (bad < best) { best = bad; age0 = a; }
            }
            badY += best;
            for (int k = 1; k < b.fpath.Count; k++)
                if (Math.Abs(b.fpath[k].x - b.fpath[k - 1].x) != VIDEO_BANDAGE_V) badV++;
            int y0 = b.fpath[0].y - wave[age0];
            if (y0 == Fixed.FromPx(floorY - VIDEO_BANDAGE_HIGH)) high++;
            else if (y0 == Fixed.FromPx(floorY - VIDEO_BANDAGE_LOW)) low++;
            else badY0++;
            var be = b.e as MummyBandage;
            if (b.endRel >= 0 && be != null && !be.broken && b.spawnRel + b.fpath.Count - 1 < recs.Count)
            {
                int last = b.spawnRel + b.fpath.Count - 1, cx = recs[last].cx;
                int x = Fixed.ToPx(b.fpath[b.fpath.Count - 1].x), dir = b.fpath[1].x > b.fpath[0].x ? 1 : -1;
                bool hitSimon = Math.Abs(x - recs[last].sx) <= 16;   // trúng Simon (kể cả lúc bất tử) ⇒ gỡ trước mép màn
                if (!hitSimon && (dir < 0 ? x - 2 >= cx + VIDEO_BANDAGE_INSET : x + 2 < cx + NesCoord.SCREEN_W - VIDEO_BANDAGE_INSET))   // tick sau (±1.5) vẫn trong khung ⇒ sai
                {
                    badEnd++;
                    measures.Add("băng sinh rel " + b.spawnRel + " mất ở x " + x + " (cx " + cx + ", hướng " + dir + ", Simon " + recs[last].sx + "," + recs[last].sy + ", y " + Fixed.ToPx(b.fpath[b.fpath.Count - 1].y) + ")");
                }
            }
        }
        Expect("băng bay ngang 1.5 px/tick (tick sai)", 0, badV);
        Expect("băng sóng dọc 24·sin(2πt/102) (tick sai)", 0, badY);
        Expect("băng sinh ở tâm chân − 31 (cao) / − 15 (thấp) (băng sai)", 0, badY0);
        Expect("băng mất cách mép màn 8 px (băng sai)", 0, badEnd);
        measures.Add("băng: " + bs.Count + " (cao " + high + ", thấp " + low + ")");
    }

    /// <summary>Nước thánh: Mummy đứng yên (khung đứng) lúc khựng, −1 vạch mỗi đòn, cách ≥ 32 tick; (phòng thử) hết khựng 12 tick sau lửa cuối.</summary>
    static void HolyChecks(bool tail)
    {
        int stun = 0, stunMoved = 0, stunAnim = 0, badGap = 0, hits = 0, stunEnd = -1;
        foreach (var t in Tracks("MummyMan"))
        {
            int lastHit = -9999;
            for (int k = 1; k < t.fpath.Count; k++)
            {
                if (t.states[k] == "Stun")
                {
                    stun++;
                    if (t.states[k - 1] == "Stun" && t.fpath[k] != t.fpath[k - 1]) stunMoved++;
                    if (t.states[k - 1] == "Stun" && t.anims[k] != t.anims[k - 1]) stunAnim++;
                    if (k + 1 >= t.fpath.Count || t.states[k + 1] != "Stun") stunEnd = Math.Max(stunEnd, t.spawnRel + k);
                }
                if (t.hp[k] < t.hp[k - 1])
                {
                    hits++;
                    if (k - lastHit < VIDEO_MUMMY_INVINC) badGap++;
                    lastHit = k;
                }
            }
        }
        ExpectRange("Mummy khựng vì nước thánh (tick)", 1, 99999, stun);
        Expect("không di chuyển lúc khựng (tick)", 0, stunMoved);
        Expect("khung đứng yên lúc khựng (tick)", 0, stunAnim);
        ExpectRange("Mummy trúng nước thánh (đòn)", 1, 999, hits);
        Expect("2 đòn liền cùng con cách ≥ 32 tick (lần sai)", 0, badGap);
        int bad = 0;
        for (int i = 1; i < bars.Count; i++) if (bars[i] != bars[i - 1] - 1) bad++;
        Expect("mỗi lần ENEMY đổi đúng −1 vạch (video: lọ / lửa 1 vạch)", 0, bad);
        if (tail)   // video Map_3: Mummy đi lại 13 / 11 tick sau khi lửa tắt (f6142.5 → 6149, f6462.5 → 6468)
            ExpectRange("Mummy đi lại sau khi lửa tắt (tick — video 11 / 13)", VIDEO_MUMMY_HOLY_STUN - 1, VIDEO_MUMMY_HOLY_STUN + 1, stunEnd + 1 - fireLastRel);
        measures.Add("nước thánh: " + hits + " đòn, khựng " + stun + " tick-con, lửa cuối rel " + fireLastRel + ", hết khựng rel " + stunEnd + "; vạch: " + string.Join(",", bars));
    }

    /// <summary>Hạ 2 Mummy: mỗi con 8 đòn, MỖI con +3000 (con đầu lúc chết), quả cầu sau 117 đứng 34 tick rồi rơi.</summary>
    static void MummyKillChecks()
    {
        KillChecks("MummyMan", VIDEO_MUMMY_W, 2 * VIDEO_MUMMY_SCORE, 2, VIDEO_MUMMY_CRYSTAL);
        int jump = firstDefeatRel > 0 && firstDefeatRel < recs.Count ? recs[firstDefeatRel].score - recs[firstDefeatRel - 1].score : -1;
        ExpectRange("con hạ đầu tiên +3000 ngay (video Map_3 3:42.87; +100 nếu cùng nhát phá băng)", VIDEO_MUMMY_SCORE, VIDEO_MUMMY_SCORE + WIKI_PROJ_SCORE, jump);
        ExpectRange("2 con chết ở 2 tick khác nhau (bất tử dùng chung — video cách 85 frame / 157 frame)", 1, 99999, defeatRel - firstDefeatRel);
        int hold = 0;
        foreach (var y in crystalYs) { if (y == crystalYs[0]) hold++; else break; }
        // đứng 34 tick rồi rơi g 0.25 từ 0 ⇒ dời 1 px đầu ở tick rơi thứ 4 (video: dời 1 px đầu 35 / 37 tick sau khi hiện)
        Expect("quả cầu: tick tới lần dời 1 px đầu (đứng 34 + 4)", VIDEO_MUMMY_HOVER + 4, hold);
        measures.Add("con đầu hạ rel " + firstDefeatRel + " (+" + jump + "), con cuối rel " + defeatRel + ", cầu đứng " + hold + " tick");
    }

    // ---------------------------------------------------------------- CVM2 09: chấm Queen Medusa / rắn theo số đo video

    static int CountRel(Func<Rec, bool> f)
    {
        int n = 0;
        foreach (var r in recs) if (f(r)) n++;
        return n;
    }

    /// <summary>Bảng sóng nhào (fixed) y hệt QueenMedusa: P/2 · (1 − cos 2πi/T) với P = Fixed.FromFloat(px).</summary>
    static int[] DipWave(int px, int period)
    {
        int p = Fixed.FromFloat(px);
        var w = new int[period];
        for (int i = 0; i < period; i++)
            w[i] = (int)Math.Round(p * 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / period)));
        return w;
    }

    /// <summary>Thức (video): ẩn + không va chạm khi trong tượng, thức đúng 180 tick sau khi camera khoá, đầu ở (chân + 2, sàn − 31) — tick
    /// thức đã bay 1 bước (chuỗi state Statue → Fly cùng tick), va chạm từ lúc thức. Trả rel thức (−1 nếu không thức).</summary>
    static int MedusaWakeChecks(Track m, int statueX, int floorY, int lockRel)
    {
        int kw = FirstK(m, k => m.states[k] != "Statue");
        if (kw < 0)
        {
            Expect("Medusa thức", 1, 0);
            return -1;
        }
        int wake = m.spawnRel + kw;
        Expect("thức sau khi camera khoá (tick — video Map_2 f6735 → f6825 = 180)", VIDEO_MEDUSA_WAKE, wake - lockRel);
        Expect("không va chạm khi trong tượng (tick hộp bật)", 0, CountK(m, k => k < kw && m.active[k]));
        Expect("nhận/gây đòn từ lúc thức (tick hộp tắt)", 0, CountK(m, k => k >= kw && !m.active[k] && m.states[k] != "Dying"));
        int ex = Fixed.FromPx(statueX + VIDEO_MEDUSA_EMERGE_DX), ey = Fixed.FromPx(floorY - VIDEO_MEDUSA_HEAD_UP);
        var dip = DipWave(VIDEO_MEDUSA_DIP, VIDEO_MEDUSA_T);
        Expect("tick thức: y = đỉnh tượng (sàn − 31) + 1 bước nhào (1/256 px)", ey + dip[1], m.fpath[kw].y);
        Expect("tick thức: |x − (chân + 2)| = 1 bước 0.875 (1/256 px)", VIDEO_MEDUSA_V, Math.Abs(m.fpath[kw].x - ex));
        return wake;
    }

    /// <summary>Các đoạn bay (chuỗi state "Fly" liên tiếp; tick ghi "Hover" đầu tiên còn bước cuối): ngang ±224/tick, dọc nhào từ y đầu đoạn
    /// (kẹp sàn − 8), độ dài (đầu 64, sau bội 16 trong [16, 160]), dừng ghi Hover 63 tick không di chuyển. Trả số đoạn.</summary>
    static int MedusaSegments(Track m, int ex, int ey, int floorY, int bounceRel, out int badV, out int badY, out int badLen, out int badHover)
    {
        var dip = DipWave(VIDEO_MEDUSA_DIP, VIDEO_MEDUSA_T);
        int lowest = Fixed.FromPx(floorY - VIDEO_MEDUSA_CLEAR);
        badV = badY = badLen = badHover = 0;
        int n = m.fpath.Count, k = 0, runs = 0;
        while (k < n)
        {
            if (m.states[k] != "Fly") { k++; continue; }
            int k0 = k;
            while (k < n && m.states[k] == "Fly") k++;
            int moves = k - k0 + 1;
            bool complete = k < n && m.states[k] == "Hover";
            runs++;
            int y0 = runs == 1 ? Fixed.FromPx(ey) : m.fpath[k0 - 1].y;
            int prevX = runs == 1 ? Fixed.FromPx(ex) : m.fpath[k0 - 1].x;
            if (complete)
            {
                if (runs == 1) { if (moves != VIDEO_MEDUSA_FIRST) badLen++; }
                else if (moves % VIDEO_MEDUSA_UNIT != 0 || moves < VIDEO_MEDUSA_UNIT || moves > VIDEO_MEDUSA_UNIT * VIDEO_MEDUSA_UNITS) badLen++;
            }
            for (int j = k0; j < Math.Min(n, k0 + moves); j++)
            {
                if (bounceRel >= 0 && m.spawnRel + j >= bounceRel) break;
                if (Math.Abs(m.fpath[j].x - prevX) != VIDEO_MEDUSA_V) badV++;
                prevX = m.fpath[j].x;
                int want = y0 + dip[(j - k0 + 1) % VIDEO_MEDUSA_T];
                if (want > lowest) want = lowest;
                if (m.fpath[j].y != want) badY++;
            }
            if (complete)
            {
                int h = k;
                while (h < n && m.states[h] == "Hover") h++;
                if (h < n && h - k != VIDEO_MEDUSA_HOVER - 1) badHover++;
                for (int j = k + 1; j < h; j++) if (m.fpath[j] != m.fpath[k]) badHover++;
            }
        }
        return runs;
    }

    /// <summary>Nháy xanh ↔ tím: lúc bay/dừng anim "fly" đổi khung đúng mỗi 9 tick (bỏ chuỗi đầu).</summary>
    static void MedusaFlashChecks(Track m, int wake)
    {
        if (wake < 0) return;
        int kw = wake - m.spawnRel, bad = 0, run = 0;
        string prev = null;
        var lens = new List<int>();
        for (int k = kw; k < m.fpath.Count && (m.states[k] == "Fly" || m.states[k] == "Hover"); k++)
        {
            string a = m.anims[k];
            if (!a.StartsWith("fly:")) { bad++; continue; }
            if (a != prev) { if (prev != null) lens.Add(run); run = 0; prev = a; }
            run++;
        }
        for (int i = 1; i < lens.Count; i++) if (lens[i] != VIDEO_MEDUSA_FLASH) bad++;
        ExpectRange("số lần đổi xanh ↔ tím", 4, 9999, lens.Count);
        Expect("nháy xanh ↔ tím mỗi 9 tick (chuỗi sai)", 0, bad);
    }

    /// <summary>Rắn (video): sinh ĐÚNG lúc thức / rời chỗ dừng tại (x Medusa, tâm đầu + 5), rơi g 0.125 từ đứng yên, chạm sàn rồi trườn
    /// ±1.375 về phía Simon (lúc chạm sàn), ra mép màn (pivot &lt; cx + 8) thì mất; ≤ 2 sống cùng lúc.</summary>
    static void SnakeChecks(Track m, List<Track> snakes, int floorY, int cx, bool drops)
    {
        ExpectRange("số rắn thả", 2, 99, snakes.Count);
        int badWhen = 0, badSpawn = 0, badFall = 0, badCrawl = 0, badDir = 0, badEdge = 0, landed = 0, edged = 0;
        var dip = DipWave(VIDEO_MEDUSA_DIP, VIDEO_MEDUSA_T);
        int floor = Fixed.FromPx(floorY);
        foreach (var t in snakes)
        {
            int km = t.spawnRel - m.spawnRel;
            if (km < 1 || km >= m.fpath.Count) { badWhen++; continue; }
            bool atWake = m.states[km - 1] == "Statue" && m.states[km] == "Fly";
            bool atHoverEnd = m.states[km - 1] == "Hover" && m.states[km] == "Fly";
            if (drops && !atWake && !atHoverEnd) badWhen++;
            int sxw, syw;
            if (atWake)
            {
                // điểm thức = vị trí tick thức lùi 1 bước (ngang theo hướng bay tick kế, dọc dip[1]) — rắn thả trước bước bay đầu
                int dir = km + 1 < m.fpath.Count ? Math.Sign(m.fpath[km + 1].x - m.fpath[km].x) : 0;
                sxw = Fixed.ToPx(m.fpath[km].x - dir * VIDEO_MEDUSA_V);
                syw = Fixed.ToPx(m.fpath[km].y - dip[1]) + VIDEO_SNAKE_DROP;
            }
            else
            {
                sxw = Fixed.ToPx(m.fpath[km - 1].x);
                syw = Fixed.ToPx(m.fpath[km - 1].y) + VIDEO_SNAKE_DROP;
            }
            if (Fixed.ToPx(t.fpath[0].x) != sxw || Fixed.ToPx(t.fpath[0].y) != syw) badSpawn++;
            int y0 = t.fpath[0].y, kl = -1;
            for (int k = 1; k < t.fpath.Count; k++)
            {
                if (t.states[k] == "Drop" && t.fpath[k].y != y0 + VIDEO_SNAKE_G * k * (k + 1) / 2) badFall++;
                if (t.states[k] == "Crawl")
                {
                    if (kl < 0)
                    {
                        kl = k;
                        landed++;
                        int rel = t.spawnRel + k;
                        int sim = rel < recs.Count ? recs[rel].sx : 0;
                        int want = sim > Fixed.ToPx(t.fpath[k - 1].x) ? 1 : -1;
                        if (Math.Sign(t.fpath[k].x - t.fpath[k - 1].x) != want) badDir++;
                    }
                    if (Math.Abs(t.fpath[k].x - t.fpath[k - 1].x) != VIDEO_SNAKE_V || t.fpath[k].y != floor) badCrawl++;
                }
            }
            if (t.endRel >= 0 && !t.defeated)
            {
                edged++;
                int lx = Fixed.ToPx(t.fpath[t.fpath.Count - 1].x);
                bool okL = lx >= cx + VIDEO_SNAKE_EDGE && lx < cx + VIDEO_SNAKE_EDGE + 2;
                bool okR = lx <= cx + NesCoord.SCREEN_W - VIDEO_SNAKE_EDGE && lx > cx + NesCoord.SCREEN_W - VIDEO_SNAKE_EDGE - 2;
                if (!okL && !okR) badEdge++;
            }
        }
        Expect("rắn sinh đúng lúc thức / rời chỗ dừng (con sai)", 0, badWhen);
        Expect("rắn sinh ở (x Medusa, tâm đầu + 5) (con sai)", 0, badSpawn);
        Expect("rắn rơi g 0.125 từ đứng yên (tick sai)", 0, badFall);
        ExpectRange("rắn chạm sàn rồi trườn (con)", 1, 99, landed);
        Expect("trườn ±1.375 px/tick trên sàn (tick sai)", 0, badCrawl);
        Expect("trườn về phía Simon lúc chạm sàn (con sai)", 0, badDir);
        Expect("ra mép màn (pivot < cx + 8) thì mất (con sai)", 0, badEdge);
        int maxAlive = 0;
        for (int rel = 0; rel < recs.Count; rel++)
        {
            int nAlive = 0;
            foreach (var t in snakes) if (t.spawnRel <= rel && (t.endRel < 0 || rel < t.endRel)) nAlive++;
            maxAlive = Math.Max(maxAlive, nAlive);
        }
        ExpectRange("rắn sống cùng lúc", 1, EST_SNAKE_ALIVE, maxAlive);
        var sb = new StringBuilder("rắn sinh ở tick: ");
        foreach (var t in snakes) sb.Append(t.spawnRel).Append(t.endRel >= 0 ? "→" + t.endRel : "").Append(' ');
        measures.Add(sb.Append("(ra mép " + edged + ")").ToString());
    }

    /// <summary>Va Simon (đòn thân được nhận) ⇒ Medusa quay ngang ngay tick sau (video Boss_Map_2 f196); đo độ nảy lên.</summary>
    static void BounceChecks(Track m)
    {
        ExpectRange("Medusa chạm Simon (lần)", 1, 99, medusaHitRels.Count);
        if (medusaHitRels.Count == 0) return;
        int h = medusaHitRels[0], k = h - m.spawnRel;
        if (k < 1 || k + 1 >= m.fpath.Count) return;
        int dxBefore = m.fpath[k].x - m.fpath[k - 1].x, dxAfter = m.fpath[k + 1].x - m.fpath[k].x;
        bool flying = m.states[k] == "Fly" && m.states[k + 1] == "Fly";
        Expect("đang bay lúc chạm (tick)", 1, flying ? 1 : 0);
        Expect("quay ngang NGAY tick sau khi chạm (dx đổi dấu)", 1, flying && dxBefore != 0 && Math.Sign(dxBefore) == -Math.Sign(dxAfter) ? 1 : 0);
        int yh = m.fpath[k].y, minY = yh, flyTicks = 0;
        for (int j = k + 1; j < m.fpath.Count && flyTicks < 70; j++)
            if (m.states[j] == "Fly") { flyTicks++; minY = Math.Min(minY, m.fpath[j].y); }
        measures.Add("chạm Simon rel " + h + " (dx " + dxBefore + " → " + dxAfter + "), sau đó lên cao nhất " + (Fixed.ToPx(yh) - Fixed.ToPx(minY))
                     + " px trên mức lúc chạm (70 tick bay)");
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
