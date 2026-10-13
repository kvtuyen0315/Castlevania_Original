#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness item / vật chứa / bí mật (CVGP 11; routine 16/21 chạy lại) — CHỈ Editor, scene Arena (Player = Simon). Qua MCP khi Play:
///   ItemProbe.SaveReplays()                      ⇒ Assets/Data/Tests/Replays/items_&lt;tên&gt;.txt
///   ItemProbe.RunSuite(ItemProbe.ScenarioNames()) ⇒ chạy (input ĐỌC TỪ FILE), ghi từng tick, TỰ CHẤM ⇒ SuiteReport()
///   ItemProbe.Shots[rel] = đường dẫn ⇒ chụp Game view đúng tick (ảnh so video)
/// Kỳ vọng là hằng VIDEO_* / WIKI_* (đo video routine 11, wiki) — độc lập với Config. Vật thử sinh bằng code (không sửa scene),
/// trừ khối phá được Arena (63,7) do StageWorld tự sinh từ lưới 06.
/// </summary>
public static class ItemProbe
{
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";

    // ---------------------------------------------------------------- kỳ vọng (CVGP_MEMORY §2i)
    public const int VIDEO_BREAK_ITEM_DELAY = 20;     // 5 chậu CE: lửa vỡ → item hiện 9–12 frame
    public const int VIDEO_GRAVITY_FIXED = 64;        // 0.25 px/tick² (y = g·t(t−1)/2 — 5 lần rơi)
    public const int VIDEO_LIFETIME = 188;            // đồng hồ f874–968
    public const int VIDEO_SMALL_FALL_FIXED = 128;    // tim nhỏ 0.5 px/tick (f4046–4069)
    public const int VIDEO_SWAY_MAX_PX = 13;          // lệch phải tối đa (video +14 ±1, mô hình 13.5)
    public const int VIDEO_FREEZE = 46;               // nhặt roi (routine 09 đo f195–217)
    public const int VIDEO_POPUP = 36;                // chữ điểm 16–18 frame
    public const int VIDEO_POPUP_FLASH = 48;          // chữ 1000 24 frame
    public const int VIDEO_ROSARY_FLASH = 26;         // 22–28 tick
    public const int VIDEO_POTION = 232;              // nhặt → hiện lại 116 frame
    public const int VIDEO_POTION_VISIBLE = 84;       // pha vẽ thường
    public const int VIDEO_RISE_TICKS = 52;           // 16 px / 0.3125 (f4200–4226 ~26 frame)
    public const int WIKI_POT_ROAST = 6;
    public const int WIKI_MULT_PICKUP_BONUS = 700;

    private struct Scn
    {
        public string name, rle, note;
        public Scn(string n, string r, string note) { name = n; rle = r; this.note = note; }
    }

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("torch_heart", "-*2 B -*40 R*30 -*20", "chậu lửa (Torch) x 336 chứa Large Heart: roi cấp 1 ⇒ lửa ⇒ item sau 20 tick ⇒ rơi g 0.25 ⇒ đi tới nhặt (+5 tim)"),
        new Scn("whip_upgrade", "-*2 B -*40 R*90 -*4 B -*40 R*90 -*10", "2 chậu CE_T2/CE_T3 (ConfigDrop = Morning Star): nhặt ⇒ roi 1→2→3, đóng băng 46 tick, Simon nháy 4 bảng màu"),
        new Scn("small_heart", "-*2 B -*230", "nến (Candle) chứa tim nhỏ (roi cấp 3): rơi 0.5 px/tick đung đưa (lần đầu sang phải), chạm sàn, biến mất sau 188 tick"),
        new Scn("star_rule", "-*2 B -*40 B -*45", "luật Morning Star: nến tim nhỏ, roi 1 — tim 3 ⇒ tim nhỏ; tim 4 ⇒ Morning Star (ConfigWhip.upgradeHearts)"),
        new Scn("mult_rule", "-*2 B -*30 R*40 -*4", "luật multiplier: cầm Dagger đã trúng 10 lần ⇒ nến tim nhỏ ra Double Shot ⇒ nhặt ⇒ Shot II"),
        new Scn("bags", "R*120 -*40", "túi đỏ/tím/trắng (+100/+400/+700) + chữ điểm 36 tick; vương miện/rương/Moai +2000/+2000/+4000"),
        new Scn("subweapons", "R*110 -*10", "nhặt Dagger (đang Shot II ⇒ đổi vũ khí mất multiplier), Axe, Holy Water, Cross, Stopwatch; rồi Double khi cầm Stopwatch ⇒ +700"),
        new Scn("rosary", "R*20 -*40", "Rosary: diệt 2 bia trên màn (0 điểm), bia ngoài màn còn; nháy màn 26 tick (xám/thường mỗi 2 tick)"),
        new Scn("potion", "R*20 -*260", "bình tàng hình: 232 tick không nhận đòn (bia chạm thân), 84 tick đầu vẽ thường rồi ẩn + lóe white"),
        new Scn("heal_life", "R*60 -*20", "Pot Roast +6 vạch (6→12, 14→16), 1UP +1 mạng, Magic Crystal hồi đầy + sự kiện"),
        new Scn("block", "-*2 A -*9 B -*80 L*8 -*20", "khối phá được Arena (63,7) (StageWorld tự sinh): nhảy + roi cấp 3 ⇒ vỡ, ô hết đặc, tile xoá, 4 mảnh, item ra NGAY tại ô rồi rơi"),
        new Scn("secret_crouch", "R*20 D*60 -*60 R*40 -*60", "kho báu bí mật kiểu ngồi: ngồi 4 tick trong vùng ⇒ túi 1000 trồi 16 px (52 tick) ⇒ nhặt +1000, chữ 48 tick"),
        new Scn("secret_enter", "-*2 R+A R*40 -*80", "kho báu bí mật kiểu đi vào vùng (nhảy vượt cổng Stage 1): vùng trên cao chỉ chạm khi nhảy"),
        new Scn("despawn", "-*20 R*12 -*10", "item nằm sàn gần mép trái màn: camera cuộn ⇒ gỡ khi tâm cách mép < 8 px"),
    };

    // ---------------------------------------------------------------- ghi
    private struct Rec
    {
        public int sx, sy, hearts, score, whip, mult, hp, lives, items, fx, popups, invis;
        public eSimonState state;
        public eSubWeapon sub;
        public bool frozen, flash, visible;
        public string palette, itemStr;
    }

    public static string Label { get; private set; } = "";
    public static bool Running { get; private set; }
    public static int Fails { get; private set; }
    public static uint Hash { get; private set; }
    private static readonly List<Rec> recs = new List<Rec>();
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> measures = new List<string>();
    private static readonly List<string> events = new List<string>();
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
    private static readonly List<Entity> extras = new List<Entity>();
    // theo dõi item theo id thực thể (pool tái dùng ⇒ ghi theo lần sinh)
    private static readonly List<Track> tracks = new List<Track>();

    private class Track
    {
        public ItemPickup item;
        public eItemId id;
        public int spawnRel = -1, endRel = -1, landRel = -1, x0, y0;
        public bool picked;
        public readonly List<Vector2Int> path = new List<Vector2Int>();
    }

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string ReplayPath(string name) { return REPLAY_DIR + "/items_" + name + ".txt"; }

    public static int SaveReplays()
    {
        Directory.CreateDirectory(REPLAY_DIR);
        foreach (var s in SCENARIOS)
        {
            var ticks = NesReplay.Decode(s.rle);
            var sb = new StringBuilder();
            sb.Append("# ").Append(ticks.Count).Append(" tick, hash ").Append(NesReplay.Hash(ticks).ToString("x8")).Append('\n');
            sb.Append("# CVGP 11 ItemProbe kịch bản '").Append(s.name).Append("': ").Append(s.note).Append('\n');
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
        sb.Append("ITEM SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
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
        if (world == null || s == null || world.Items == null)
        {
            Debug.LogError("[ItemProbe] cần scene Arena với Player = SimonController + StageWorld.itemCatalog");
            return -1;
        }
        string p = ReplayPath(scenario);
        if (!File.Exists(p))
        {
            Debug.LogError("[ItemProbe] thiếu " + p + " — gọi ItemProbe.SaveReplays()");
            return -1;
        }
        var ticks = NesReplay.Decode(File.ReadAllText(p));
        ClearExtras();
        // đặt lại section (khối gạch nguyên vẹn, tile trả lại) + thực thể
        world.Warp(world.Section, 300, 176);
        world.ResetRng();
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
        tracks.Clear();
        events.Clear();
        var cat = world.Items;
        switch (scenario)
        {
            case "torch_heart":
                {
                    s.Teleport(300, 176, 1);
                    var t = Prop(cat.torchPrefab, 336, 176, eItemId.LargeHeart, null);
                    finish = () =>
                    {
                        var tr = FirstTrack(eItemId.LargeHeart);
                        int brk = FirstEvent("break");
                        Expect("chậu vỡ (roi cấp 1 chạm)", 1, brk >= 0 ? 1 : 0);
                        Expect("item hiện sau lửa vỡ (tick)", VIDEO_BREAK_ITEM_DELAY, tr != null ? tr.spawnRel - brk : -1);
                        Expect("item sinh tại tâm x chậu", 336, tr != null ? tr.x0 : -1);
                        Expect("item sinh: đỉnh ô = đỉnh chậu + 8 (Y = sàn − 8)", 168, tr != null ? tr.y0 : -1);
                        GravityChecks(tr, 176);
                        Expect("Large Heart +5 tim", 10, Last().hearts);
                        Expect("đã nhặt", 1, tr != null && tr.picked ? 1 : 0);
                        Expect("lửa FX + tia lửa đã sinh (Effect tối đa ≥ 2)", 1, MaxOf(r => r.fx) >= 2 ? 1 : 0);
                    };
                }
                break;
            case "whip_upgrade":
                {
                    s.Teleport(300, 176, 1);
                    Prop(cat.torchPrefab, 336, 176, eItemId.None, "CE_T2");
                    Prop(cat.torchPrefab, 380, 176, eItemId.None, "CE_T3");
                    finish = () =>
                    {
                        Expect("ConfigDrop CE_T2/CE_T3 ⇒ 2 Morning Star sinh ra", 2, CountTracks(eItemId.MorningStar));
                        Expect("roi cuối = 3", 3, Last().whip);
                        // đóng băng: từ tick sau nhặt, Simon đứng yên đúng 46 tick dù giữ →
                        var t1 = FirstTrack(eItemId.MorningStar);
                        int pick = t1 != null ? t1.endRel : -1;
                        int frozen = 0;
                        for (int i = pick + 1; i < recs.Count && recs[i].frozen; i++)
                            frozen++;
                        Expect("đóng băng thế giới sau nhặt roi (tick)", VIDEO_FREEZE, frozen);
                        int moved = 0;
                        for (int i = pick + 1; i <= pick + VIDEO_FREEZE && i < recs.Count; i++)
                            if (recs[i].sx != recs[pick].sx)
                                moved++;
                        Expect("Simon đứng yên khi đóng băng (tick đổi x)", 0, moved);
                        var pals = new HashSet<string>();
                        for (int i = pick + 1; i <= pick + VIDEO_FREEZE && i < recs.Count; i++)
                            pals.Add(recs[i].palette);
                        Expect("nháy đủ 4 bảng (thường/đỏ/tím/xanh)", 4, pals.Count);
                        Expect("hết đóng băng ⇒ bảng gốc", 1, pick + VIDEO_FREEZE + 1 < recs.Count && recs[pick + VIDEO_FREEZE + 1].palette == "-" ? 1 : 0);
                        var seq = new StringBuilder();
                        for (int i = pick + 1; i <= pick + 10 && i < recs.Count; i++)
                            seq.Append(recs[i].palette).Append(' ');
                        measures.Add("bảng màu 10 tick đầu đóng băng: " + seq);
                    };
                }
                break;
            case "small_heart":
                {
                    ses.whipLevel = 3;
                    s.Teleport(300, 176, 1);
                    Prop(cat.candlePrefab, 340, 168, eItemId.SmallHeart, null);
                    finish = () =>
                    {
                        var tr = FirstTrack(eItemId.SmallHeart);
                        Expect("tim nhỏ sinh ra", 1, tr != null ? 1 : 0);
                        if (tr == null)
                            return;
                        Expect("sinh: ô trùng ô nến (Y = Y nến)", 168, tr.y0);
                        int bad = 0, maxDx = -99, firstDir = 0, landY = -1;
                        for (int k = 1; k < tr.path.Count; k++)
                        {
                            int dy = tr.path[k].y - tr.path[0].y;
                            int want = Math.Min((VIDEO_SMALL_FALL_FIXED * k) >> 8, 15);
                            if (tr.landRel < 0 || tr.spawnRel + k < tr.landRel)
                                if (dy != want) bad++;
                            int dx = tr.path[k].x - tr.x0;
                            maxDx = Math.Max(maxDx, dx);
                            if (firstDir == 0 && dx != 0) firstDir = Math.Sign(dx);
                            landY = tr.path[k].y;
                        }
                        Expect("rơi đều 0.5 px/tick (tick sai)", 0, bad);
                        Expect("lần đầu đung đưa sang PHẢI", 1, firstDir);
                        ExpectRange("lệch ngang tối đa (px)", VIDEO_SWAY_MAX_PX, VIDEO_SWAY_MAX_PX + 1, maxDx);
                        Expect("chạm sàn: hàng tim cuối ngay trên sàn (Y = 176 + 7)", 183, landY);
                        Expect("tồn tại (tick) rồi biến mất, không nhặt", VIDEO_LIFETIME, tr.endRel - tr.spawnRel);
                        Expect("không nhặt", 0, tr.picked ? 1 : 0);
                    };
                }
                break;
            case "star_rule":
                {
                    s.Teleport(300, 176, 1);
                    ses.hearts = 3;
                    var c1 = Prop(cat.candlePrefab, 336, 168, eItemId.SmallHeart, null);
                    onTick = rel => { if (rel == 40) { GameSession.Current.hearts = 4; Prop(world.Items.candlePrefab, 336, 168, eItemId.SmallHeart, null); } };
                    finish = () =>
                    {
                        Expect("tim 3 (< 4) ⇒ tim nhỏ", 1, CountTracks(eItemId.SmallHeart));
                        Expect("tim 4 ⇒ Morning Star", 1, CountTracks(eItemId.MorningStar));
                    };
                }
                break;
            case "mult_rule":
                {
                    ses.whipLevel = 3;
                    ses.subWeapon = eSubWeapon.Dagger;
                    ses.subWeaponHits = 10;
                    s.Teleport(300, 176, 1);
                    Prop(cat.candlePrefab, 340, 168, eItemId.SmallHeart, null);
                    finish = () =>
                    {
                        Expect("10 lần trúng ⇒ nến tim nhỏ ra Double Shot", 1, CountTracks(eItemId.DoubleShot));
                        Expect("nhặt ⇒ Shot II", 2, Last().mult);
                        Expect("vẫn cầm Dagger", (int)eSubWeapon.Dagger, (int)Last().sub);
                        Expect("điểm không đổi", 0, Last().score);
                    };
                }
                break;
            case "bags":
                {
                    s.Teleport(300, 176, 1);
                    Drop(eItemId.RedMoneyBag, 320);
                    Drop(eItemId.PurpleMoneyBag, 340);
                    Drop(eItemId.WhiteMoneyBag, 360);
                    Drop(eItemId.Crown, 380);
                    Drop(eItemId.TreasureChest, 400);
                    Drop(eItemId.Moai, 420);
                    finish = () =>
                    {
                        Expect("tổng điểm 100+400+700+2000+2000+4000", 9200, Last().score);
                        int pop = PopupLife(LastEvent("popup"));
                        Expect("chữ điểm (cái cuối) tồn tại (tick)", VIDEO_POPUP, pop);
                        Expect("6 chữ điểm đã hiện", 6, CountEvents("popup"));
                        foreach (var e in events)
                            if (e.Contains("popup"))
                                measures.Add(e);
                    };
                }
                break;
            case "subweapons":
                {
                    ses.subWeapon = eSubWeapon.Axe;
                    ses.shotMultiplier = 2;
                    s.Teleport(300, 176, 1);
                    Drop(eItemId.Dagger, 318);   // Arena: tường thấp x 416–431 ⇒ mọi item < 400
                    Drop(eItemId.Axe, 334);
                    Drop(eItemId.HolyWater, 350);
                    Drop(eItemId.Cross, 366);
                    Drop(eItemId.Stopwatch, 382);
                    Drop(eItemId.DoubleShot, 398);
                    finish = () =>
                    {
                        int iDag = RelOfPick(eItemId.Dagger);
                        Expect("nhặt Dagger khi cầm Axe + Shot II ⇒ mất multiplier", 1, iDag >= 0 ? recs[iDag].mult : -1);
                        Expect("cuối cầm Stopwatch", (int)eSubWeapon.Stopwatch, (int)Last().sub);
                        Expect("Double khi cầm Stopwatch ⇒ +700 điểm, Shot không đổi", WIKI_MULT_PICKUP_BONUS, Last().score);
                        Expect("Shot giữ 1", 1, Last().mult);
                        foreach (var e in events)
                            if (e.Contains("nhặt"))
                                measures.Add(e);
                    };
                }
                break;
            case "rosary":
                {
                    s.Teleport(300, 176, 1);
                    var a = Target(380, 176);
                    var b = Target(420, 176);
                    var c = Target(700, 176);   // ngoài màn (cx 172 ⇒ màn 172..427)
                    Drop(eItemId.Rosary, 320);
                    finish = () =>
                    {
                        Expect("2 bia trên màn bị diệt", 0, (a.Alive ? 1 : 0) + (b.Alive ? 1 : 0));
                        Expect("bia ngoài màn còn", 1, c.Alive ? 1 : 0);
                        Expect("Rosary không cho điểm", 0, Last().score);
                        int pick = RelOfPick(eItemId.Rosary), on = 0, first = -1, last = -1;
                        var pat = new StringBuilder();
                        for (int i = pick; i < recs.Count; i++)
                            if (recs[i].flash) { on++; if (first < 0) first = i; last = i; }
                        for (int i = pick + 1; i <= pick + 12 && i < recs.Count; i++)
                            pat.Append(recs[i].flash ? '#' : '.');
                        Expect("nháy màn dài (tick đầu → cuối)", VIDEO_ROSARY_FLASH, last - first + 1);
                        Expect("bắt đầu tick sau khi nhặt", pick + 1, first);
                        measures.Add("mẫu nháy 12 tick đầu: " + pat + " (2 xám / 2 thường)");
                    };
                }
                break;
            case "potion":
                {
                    s.Teleport(300, 176, 1);
                    Drop(eItemId.InvisibilityPotion, 320);
                    var t = Target(330, 176);
                    t.touchDamage = true;
                    t.Touch.active = true;
                    finish = () =>
                    {
                        int pick = RelOfPick(eItemId.InvisibilityPotion);
                        int hiddenFrom = -1, end = -1;
                        for (int i = pick + 1; i < recs.Count; i++)
                        {
                            if (hiddenFrom < 0 && !recs[i].visible) hiddenFrom = i;
                            if (end < 0 && recs[i].invis == 0) end = i;
                        }
                        Expect("hết tàng hình sau (tick)", VIDEO_POTION, end - pick);
                        Expect("máu giữ 16 suốt lúc tàng hình dù chạm bia", 16, end > 0 ? recs[end - 1].hp : -1);
                        Expect("hết tàng hình ⇒ vẽ thường", 1, end > 0 && recs[end].visible && recs[end].palette == "-" ? 1 : 0);
                        Expect("hết tàng hình ⇒ chạm bia lại mất máu", 1, Last().hp < 16 ? 1 : 0);
                        Expect("pha vẽ thường rồi mới ẩn (tick sau nhặt)", VIDEO_POTION_VISIBLE, hiddenFrom - pick);
                        int glints = 0;
                        for (int i = pick + 1; i <= pick + VIDEO_POTION && i < recs.Count; i++)
                            if (recs[i].visible && recs[i].palette == "white") glints++;
                        measures.Add("số tick lóe white trong pha ẩn: " + glints);
                        Expect("có lóe white", 1, glints > 0 ? 1 : 0);
                    };
                }
                break;
            case "heal_life":
                {
                    ses.playerHp = 6;
                    s.Teleport(300, 176, 1);
                    Drop(eItemId.PotRoast, 320);
                    Drop(eItemId.PotRoast, 335);
                    Drop(eItemId.OneUp, 350);
                    Drop(eItemId.MagicCrystal, 365);
                    int crystal = 0;
                    Action<StageWorld> h = w => crystal++;
                    ItemEffects.CrystalPicked += h;
                    var set14 = new bool[1];
                    onTick = rel =>
                    {
                        if (!set14[0] && GameSession.Current.playerHp == 12)
                        {
                            set14[0] = true;
                            GameSession.Current.playerHp = 14;   // lần 2: 14 + 6 ⇒ kẹp 16
                        }
                    };
                    finish = () =>
                    {
                        ItemEffects.CrystalPicked -= h;
                        bool saw12 = false;
                        foreach (var e in events) if (e.Contains("máu 6→12")) saw12 = true;
                        Expect("Pot Roast lần 1: 6 → 12 (+6 wiki)", 1, saw12 ? 1 : 0);
                        bool cap = false;
                        foreach (var e in events) if (e.Contains("máu 14→16")) cap = true;
                        Expect("Pot Roast lần 2: 14 → 16 (kẹp 16)", 1, cap ? 1 : 0);
                        Expect("1UP: mạng 3 → 4", 4, Last().lives);
                        Expect("Magic Crystal: sự kiện CrystalPicked", 1, crystal);
                        Expect("máu cuối 16", 16, Last().hp);
                    };
                }
                break;
            case "block":
                {
                    ses.whipLevel = 3;
                    s.Teleport(1032, 176, -1);   // phải Bục C (không bị trần chặn khi nhảy), roi quay trái chạm khối
                    world.CameraCtl.SnapToTarget();
                    BreakableBlock blk = null;
                    foreach (var e in world.GroupList(eEntityGroup.Prop))
                    {
                        var bb = e as BreakableBlock;
                        if (bb != null && bb.col == 63 && bb.row == 7) blk = bb;
                    }
                    if (blk != null)
                        blk.content = eItemId.PotRoast;
                    int solidBefore = world.Tiles.IsSolid(1015, 120) ? 1 : 0;
                    finish = () =>
                    {
                        Expect("StageWorld tự sinh BreakableBlock cho ô (63,7)", 1, blk != null ? 1 : 0);
                        Expect("ô (63,7) đặc trước khi phá", 1, solidBefore);
                        Expect("khối vỡ (roi cấp 3 khi nhảy)", 1, blk != null && blk.broken ? 1 : 0);
                        Expect("ô (63,7) hết đặc", 0, world.Tiles.IsSolid(1015, 120) ? 1 : 0);
                        Expect("tile BG ô (63,7) đã xoá (4 tile)", 1, world.Section.bg.GetTile(new Vector3Int(126, -15, 0)) == null ? 1 : 0);
                        var tr = FirstTrack(eItemId.PotRoast);
                        int brk = FirstEvent("blockbreak");
                        Expect("item ra NGAY tick vỡ (video 1:39.6)", 0, tr != null ? tr.spawnRel - brk : -1);
                        Expect("item tại ô khối: Y = 16·7 + 15", 127, tr != null ? tr.y0 : -1);
                        Expect("4 mảnh gạch (Effect tối đa ≥ 4)", 1, MaxOf(r => r.fx) >= 4 ? 1 : 0);
                        Expect("item rơi xuống sàn 176", 176, tr != null && tr.path.Count > 0 ? tr.path[tr.path.Count - 1].y : -1);
                    };
                }
                break;
            case "secret_crouch":
                {
                    s.Teleport(300, 176, 1);
                    var st = (SecretTrigger)world.Spawn(world.Items.secretPrefab, 0, 0, -1);
                    extras.Add(st);
                    st.area = new RectInt(318, 170, 8, 12);
                    st.condition = eSecretCondition.Crouch;
                    st.holdTicks = 0;
                    st.item = eItemId.FlashingMoneyBag;
                    st.spawnAt = new Vector2Int(360, 176);
                    st.rise = true;
                    finish = () =>
                    {
                        var tr = FirstTrack(eItemId.FlashingMoneyBag);
                        int crouch = -1;
                        for (int i = 0; i < recs.Count; i++) if (recs[i].state == eSimonState.Crouch && recs[i].sx >= 318) { crouch = i; break; }
                        Expect("bí mật kích hoạt sau ngồi (tick)", 4, tr != null ? tr.spawnRel - crouch + 1 : -1);
                        int risen = tr != null && tr.landRel >= 0 ? tr.landRel - tr.spawnRel : -1;
                        ExpectRange("trồi hết 16 px (tick, kể cả phần lẻ)", VIDEO_RISE_TICKS - 1, VIDEO_RISE_TICKS, risen);
                        Expect("+1000", 1000, Last().score);
                        int pop = PopupLife(FirstEvent("popup"));
                        Expect("chữ 1000 tồn tại (tick)", VIDEO_POPUP_FLASH, pop);
                    };
                }
                break;
            case "secret_enter":
                {
                    s.Teleport(300, 176, 1);
                    var st = (SecretTrigger)world.Spawn(world.Items.secretPrefab, 0, 0, -1);
                    extras.Add(st);
                    st.area = new RectInt(310, 140, 40, 8);   // chỉ chạm khi pivot lên cao (nhảy) — y 140..147
                    st.condition = eSecretCondition.Enter;
                    st.item = eItemId.FlashingMoneyBag;
                    st.spawnAt = new Vector2Int(260, 176);
                    finish = () =>
                    {
                        Expect("nhảy qua vùng cao ⇒ túi 1000 sinh", 1, CountTracks(eItemId.FlashingMoneyBag));
                        int fire = FirstTrack(eItemId.FlashingMoneyBag) != null ? FirstTrack(eItemId.FlashingMoneyBag).spawnRel : -1;
                        Expect("Simon đang trên không khi kích hoạt", 1, fire >= 0 && recs[fire].sy < 176 ? 1 : 0);
                    };
                }
                break;
            case "despawn":
                {
                    s.Teleport(700, 176, 1);   // tránh hố Arena x 480–511
                    world.CameraCtl.SnapToTarget();   // cx 572
                    Drop(eItemId.WhiteMoneyBag, 588);   // màn x 16
                    finish = () =>
                    {
                        var tr = FirstTrack(eItemId.WhiteMoneyBag);
                        int end = tr != null ? tr.endRel : -1;
                        Expect("gỡ khi tâm cách mép màn < 8 px: tâm − cx lúc gỡ", 7, end >= 0 ? 588 - (recs[end].sx - 128) : -1);
                        Expect("không nhặt", 0, tr != null && tr.picked ? 1 : 0);
                    };
                }
                break;
            default:
                Debug.LogError("[ItemProbe] kịch bản lạ " + scenario);
                return -1;
        }
        s.ResetStats();
        // item đặt sẵn lúc setup (Drop/Spawn) cũng phải được theo dõi
        foreach (var e in world.GroupList(eEntityGroup.Item))
            TrackItem(e as ItemPickup, -1);
        NesPad.I.PlayReplay(ticks);
        Begin(scenario, ticks.Count);
        return ticks.Count;
    }

    // ---------------------------------------------------------------- tiện ích dựng

    static CandleProp Prop(CandleProp prefab, int x, int y, eItemId content, string objectId)
    {
        var c = (CandleProp)world.Spawn(prefab, x, y, -1);
        c.content = content;
        c.objectId = objectId;
        extras.Add(c);
        return c;
    }

    /// <summary>Item nằm sẵn trên sàn y 176 (để nhặt).</summary>
    static void Drop(eItemId id, int x)
    {
        var it = ItemDrops.Spawn(world, id, x, 176);
        if (it != null)
            it.motion = ItemPickup.eMotion.Rest;
    }

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
        startTick = world.TickCount;
        world.TickEnded += OnTickEnded;
        ItemEffects.Picked += OnPicked;
        Running = true;
    }

    private static void Unhook()
    {
        if (world != null)
            world.TickEnded -= OnTickEnded;
        ItemEffects.Picked -= OnPicked;
        Running = false;
    }

    private static void OnPicked(eItemId id, string log)
    {
        int rel = world.TickCount - startTick - 1;
        events.Add(rel + " nhặt " + id + ": " + log);
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

    private static Track TrackItem(ItemPickup it, int rel)
    {
        if (it == null)
            return null;
        foreach (var t in tracks)
            if (t.item == it && t.endRel < 0)
                return t;
        var n = new Track { item = it, id = it.id, spawnRel = rel, x0 = it.X, y0 = it.Y };
        tracks.Add(n);
        return n;
    }

    private static void OnTickEnded(StageWorld w)
    {
        if (!Running)
            return;
        var s = Simon;
        int rel = w.TickCount - startTick - 1;
        var ses = GameSession.Current;
        var r = new Rec
        {
            sx = s.X, sy = s.Y, state = s.Fsm.CurState, hearts = ses.hearts, score = ses.score, whip = ses.whipLevel,
            mult = ses.shotMultiplier, hp = ses.playerHp, lives = ses.lives, sub = ses.subWeapon,
            frozen = w.IsWorldFrozen, flash = w.IsScreenFlashOn, visible = s.View != null && s.View.enabled,
            palette = s.CurrentPalette != null ? s.CurrentPalette.name : "-", invis = s.InvisibleLeft,
        };
        // sự kiện vật chứa / khối / chữ điểm
        foreach (var e in w.GroupList(eEntityGroup.Prop))
        {
            var c = e as CandleProp;
            if (c != null && c.broken && c.breakAge == 0)
                events.Add(rel + " break " + c.X + "," + c.Y);
        }
        int fx = 0, pop = 0;
        foreach (var e in w.GroupList(eEntityGroup.Effect))
        {
            if (!e.Alive) continue;
            var sp = e as ScorePopup;
            if (sp != null)
            {
                pop++;
                if (sp.age == 0)
                    events.Add(rel + " popup " + sp.points + " @" + sp.X + "," + sp.Y + " life " + sp.life);
            }
            else
                fx++;
        }
        r.fx = fx;
        r.popups = pop;
        // item
        var sb = new StringBuilder();
        int items = 0;
        foreach (var e in w.GroupList(eEntityGroup.Item))
        {
            var it = e as ItemPickup;
            if (it == null || !it.Alive) continue;
            items++;
            var t = TrackItem(it, rel);
            if (t.spawnRel < 0) t.spawnRel = rel;
            t.path.Add(new Vector2Int(it.X, it.Y));
            if (t.landRel < 0 && it.motion == ItemPickup.eMotion.Rest) t.landRel = rel;
            sb.Append((int)it.id).Append(':').Append(it.X).Append(',').Append(it.Y).Append(';');
            Mix((int)it.id); Mix(it.fx); Mix(it.fy); Mix((int)it.motion);
        }
        foreach (var t in tracks)
            if (t.endRel < 0 && t.item != null && !t.item.Alive)
            {
                t.endRel = rel;
                t.picked = t.item.picked;
            }
        r.items = items;
        r.itemStr = sb.ToString();
        recs.Add(r);
        Mix(rel); Mix(s.fx); Mix(s.fy); Mix((int)r.state); Mix(r.hearts); Mix(r.score); Mix(r.whip); Mix(r.mult); Mix(r.hp); Mix(r.lives);
        Mix((int)r.sub); Mix(r.frozen ? 1 : 0); Mix(r.flash ? 1 : 0); Mix(r.visible ? 1 : 0); Mix(r.palette.Length > 0 ? r.palette[0] : 0); Mix(fx); Mix(pop);
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
        sb.Append("[").Append(Fails == 0 ? "ĐẠT" : "FAIL").Append("] ").Append(Label).Append(" — ").Append(recs.Count).Append(" tick, hash ")
          .Append(Hash.ToString("x8")).Append('\n');
        foreach (var c in checks)
            sb.Append(c).Append('\n');
        foreach (var m in measures)
            sb.Append("    · ").Append(m).Append('\n');
        return sb.ToString().TrimEnd('\n');
    }

    /// <summary>Nhật ký sự kiện của kịch bản vừa chạy (vỡ, nhặt, chữ điểm).</summary>
    public static string Events()
    {
        return string.Join("\n", events.ToArray());
    }

    /// <summary>Vết item kịch bản vừa chạy: id, tick sinh/đáp/hết, đường đi.</summary>
    public static string Tracks()
    {
        var sb = new StringBuilder();
        foreach (var t in tracks)
        {
            sb.Append(t.id).Append(" sinh ").Append(t.spawnRel).Append(" @").Append(t.x0).Append(',').Append(t.y0).Append(" đáp ").Append(t.landRel)
              .Append(" hết ").Append(t.endRel).Append(t.picked ? " (nhặt)" : "").Append(" :");
            for (int i = 0; i < t.path.Count && i < 60; i++)
                sb.Append(' ').Append(t.path[i].x).Append(',').Append(t.path[i].y);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- chấm

    static void GravityChecks(Track tr, int floorY)
    {
        if (tr == null)
        {
            Expect("item có vết", 1, 0);
            return;
        }
        int bad = 0, land = -1;
        for (int k = 1; k < tr.path.Count; k++)
        {
            int want = Math.Min(tr.y0 + ((VIDEO_GRAVITY_FIXED * k * (k - 1) / 2) >> 8), floorY);
            if (tr.path[k].y != want) bad++;
            if (land < 0 && tr.path[k].y == floorY) land = k;
        }
        Expect("rơi y = y0 + 0.25·t(t−1)/2 (tick sai)", 0, bad);
        measures.Add("rơi " + (floorY - tr.y0) + " px chạm sàn sau " + land + " tick (video: chậu CE 8 px ~8–12 tick)");
    }

    static Rec Last() { return recs[recs.Count - 1]; }

    static Track FirstTrack(eItemId id)
    {
        foreach (var t in tracks)
            if (t.id == id)
                return t;
        return null;
    }

    static int CountTracks(eItemId id)
    {
        int n = 0;
        foreach (var t in tracks)
            if (t.id == id)
                n++;
        return n;
    }

    static int RelOfPick(eItemId id)
    {
        foreach (var t in tracks)
            if (t.id == id && t.picked)
                return t.endRel;
        return -1;
    }

    static int FirstEvent(string key)
    {
        foreach (var e in events)
        {
            var parts = e.Split(' ');
            if (parts.Length > 1 && parts[1] == key)
                return int.Parse(parts[0]);
        }
        if (key == "blockbreak")
        {
            // khối: tick đầu tiên ô (63,7) không còn đặc = tick có mảnh gạch
            for (int i = 0; i < recs.Count; i++)
                if (recs[i].fx >= 4)
                    return i;
        }
        return -1;
    }

    static int LastEvent(string key)
    {
        int r = -1;
        foreach (var e in events)
        {
            var parts = e.Split(' ');
            if (parts.Length > 1 && parts[1] == key)
                r = int.Parse(parts[0]);
        }
        return r;
    }

    static int CountEvents(string key)
    {
        int n = 0;
        foreach (var e in events)
        {
            var parts = e.Split(' ');
            if (parts.Length > 1 && parts[1] == key)
                n++;
        }
        return n;
    }

    /// <summary>Số tick chữ điểm còn trên màn kể từ tick hiện (ghi theo số popup trên màn).</summary>
    static int PopupLife(int from)
    {
        if (from < 0)
            return -1;
        int n = 0;
        for (int i = from; i < recs.Count && recs[i].popups > 0; i++)
            n++;
        return n;
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
