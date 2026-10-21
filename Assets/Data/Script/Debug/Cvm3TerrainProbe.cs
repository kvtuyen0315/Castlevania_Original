#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Harness kiểm địa hình Map 3 (CVM3 05) — CHỈ Editor, scene Arena (Player = Simon), gọi qua MCP execute_code khi Play (khuôn Cvm2TerrainProbe):
///   Cvm3TerrainProbe.Setup()                    ⇒ dựng 5 prefab Assets/Data/Prefabs/Level/Block3/*.prefab làm con (tắt) của StageWorld + AddSection
///                                                 (chỉ lúc chạy — KHÔNG đổi Arena.unity).
///   Cvm3TerrainProbe.StartShots(dir, "Section:cx,cx;…", hideHud) ⇒ thế giới dừng, ẩn sprite động (+ HUD nếu hideHud), chụp Game view
///                                                 terrain3_&lt;Section&gt;_cx&lt;N&gt;.png (so sheet bằng Tools/cvm3/accept3.py terrain). WalkwayB: cx sau nới
///                                                 (cx video −112 ⇔ 0).
///   Cvm3TerrainProbe.RunMoves("all"|"a,b")      ⇒ kịch bản Simon (đi / nhảy / thang zig-zag / cầu hỏng / hố / roi khối) theo replay RLE, tự chấm ⇒ Report().
///                                                 Mọi kịch bản có di chuyển phải có X cuối ≠ X đầu (CVM2 05: probe "PASS" rỗng khi Simon kẹt tường).
///   Cvm3TerrainProbe.Cleanup()                  ⇒ gỡ section Block 3, warp về Arena (48,176), camera bám Simon.
/// Chỉ đọc gameplay (không đổi lõi) — không thuộc bộ hồi quy (kết quả ghi ClaudeNotes/cvm3/_cache/05/).
/// </summary>
public static class Cvm3TerrainProbe
{
    public const string PREFAB_DIR = "Assets/Data/Prefabs/Level/Block3/";
    public static readonly string[] SECTION_IDS = { "WalkwayEntranceA", "WalkwayEntranceB", "WalkwayA", "WalkwayB", "MummyMan" };

    private static StageWorld W => StageWorld.Current;
    private static SimonController S => W != null ? W.Player as SimonController : null;
    private static readonly List<LevelSection> added = new List<LevelSection>();

    public static bool Running { get; private set; }
    public static string Status { get; private set; } = "";
    private static readonly StringBuilder report = new StringBuilder();

    // ================================================================ dựng / gỡ

    public static string Setup()
    {
        if (!Application.isPlaying || W == null)
            return "cần Play scene Arena";
        int n = 0;
        foreach (var id in SECTION_IDS)
        {
            if (Find(id) != null)
                continue;
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_DIR + id + ".prefab");
            if (pf == null)
                return "không thấy prefab " + id;
            var go = UnityEngine.Object.Instantiate(pf, W.transform);
            go.name = id;
            go.SetActive(false);
            var s = go.GetComponent<LevelSection>();
            W.AddSection(s);
            added.Add(s);
            n++;
        }
        return "đã thêm " + n + " section Block 3 (tổng " + W.Sections.Count + ")";
    }

    public static LevelSection Find(string id)
    {
        if (W == null)
            return null;
        foreach (var s in W.Sections)
            if (s != null && s.name == id)
                return s;
        return null;
    }

    public static string Cleanup()
    {
        if (W == null)
            return "không có StageWorld";
        LevelSection arena = Find("Arena");
        if (arena != null)
        {
            W.Warp(arena, 48, 176);
            if (S != null)
                S.Teleport(48, 176, 1);
        }
        foreach (var s in added)
            if (s != null)
            {
                W.RemoveSection(s);
                UnityEngine.Object.DestroyImmediate(s.gameObject);
            }
        added.Clear();
        if (W.CameraCtl != null)
        {
            W.CameraCtl.Target = W.Player;
            W.CameraCtl.SnapToTarget();
        }
        return "đã gỡ section Block 3";
    }

    // ================================================================ ảnh nền

    private class Shot
    {
        public string name, section;
        public int cx;
    }

    private static List<Shot> shots;
    private static int shotIndex, shotWait;
    private static string shotDir, shotFile;
    private static bool shotHideHud, prevPaused;
    private static readonly List<SpriteRenderer> hidden = new List<SpriteRenderer>();
    private static readonly List<GameObject> hiddenGo = new List<GameObject>();

    public static string StartShots(string dir, string spec, bool hideHud)
    {
        if (W == null || Running)
            return W == null ? "cần Play" : "đang chạy";
        shots = new List<Shot>();
        foreach (var part in spec.Split(';'))
        {
            var kv = part.Split(':');
            if (kv.Length != 2)
                continue;
            foreach (var c in kv[1].Split(','))
                shots.Add(new Shot { name = "terrain3_" + kv[0].Trim() + "_cx" + c.Trim(), section = kv[0].Trim(), cx = int.Parse(c.Trim()) });
        }
        Directory.CreateDirectory(dir);
        shotDir = dir;
        shotIndex = 0;
        shotWait = 0;
        shotFile = null;
        shotHideHud = hideHud;
        prevPaused = W.Paused;
        W.Paused = true;
        Application.runInBackground = true;
        report.Length = 0;
        Running = true;
        Status = "chụp 0/" + shots.Count;
        EditorApplication.update -= PumpShots;
        EditorApplication.update += PumpShots;
        return "chụp " + shots.Count + " ảnh";
    }

    private static void PumpShots()
    {
        if (W == null)
        {
            EditorApplication.update -= PumpShots;
            Running = false;
            Status = "ABORT: mất StageWorld";
            return;
        }
        if (shotFile != null)
        {
            if (File.Exists(shotFile) && new FileInfo(shotFile).Length > 0 && ++shotWait >= 3)
            {
                shotFile = null;
                shotIndex++;
                shotWait = 0;
                Status = "chụp " + shotIndex + "/" + shots.Count;
            }
            return;
        }
        if (shotIndex >= shots.Count)
        {
            ShowAll();
            W.Paused = prevPaused;
            W.CameraCtl.Target = W.Player;
            EditorApplication.update -= PumpShots;
            File.WriteAllText(Path.Combine(shotDir, "shots.txt"), report.ToString());
            Running = false;
            Status = "xong " + shots.Count + " ảnh";
            return;
        }
        var j = shots[shotIndex];
        if (shotWait == 0)
        {
            var target = Find(j.section);
            if (target == null)
            {
                report.Append("THIẾU section ").Append(j.section).Append('\n');
                shotIndex++;
                return;
            }
            if (W.Section != target)
            {
                var p = target.data.hasSimonStart ? target.data.simonStart : new Vector2Int(target.data.width / 2, 0);
                W.Warp(target, p.x, p.y);
            }
            W.CameraCtl.Target = null;
            W.CameraCtl.SnapTo(j.cx);
            HideAll();
            shotWait = -3;
            return;
        }
        if (shotWait < 0)
        {
            shotWait++;
            if (shotWait < 0)
                return;
        }
        shotFile = Path.Combine(shotDir, j.name + ".png");
        if (File.Exists(shotFile))
            File.Delete(shotFile);
        ScreenCapture.CaptureScreenshot(shotFile);
        report.Append(j.name).Append(" section ").Append(W.Section.name).Append(" cx ").Append(W.CameraCtl.X).Append('\n');
        shotWait = 0;
    }

    // forceRenderingOff (như Cvm2TerrainProbe) — HUD: tắt GameObject "HUD" + HudBackdrop (lớp HUD)
    private static void HideAll()
    {
        ShowAll();
        foreach (var r in UnityEngine.Object.FindObjectsOfType<SpriteRenderer>())
        {
            string l = r.sortingLayerName;
            if (!r.forceRenderingOff && (l == "BackgroundFX" || l == "Character" || l == "Overlay" || (shotHideHud && l == "HUD")))
            {
                r.forceRenderingOff = true;
                hidden.Add(r);
            }
        }
        if (shotHideHud)
        {
            var hud = GameObject.Find("HUD");
            if (hud != null && hud.activeSelf)
            {
                hud.SetActive(false);
                hiddenGo.Add(hud);
            }
        }
    }

    private static void ShowAll()
    {
        foreach (var r in hidden)
            if (r != null)
                r.forceRenderingOff = false;
        hidden.Clear();
        foreach (var g in hiddenGo)
            if (g != null)
                g.SetActive(true);
        hiddenGo.Clear();
    }

    // ================================================================ kịch bản Simon

    private class Rec
    {
        public int ticks, minY = int.MaxValue, maxY = int.MinValue, deaths, startX, minCam = int.MaxValue;
        public eTileCollision diedKind;
        public int lastX, lastY;
        public eSimonState lastState;
        public bool everStair;
        public readonly List<string> trace = new List<string>();
        public readonly HashSet<int> groundYs = new HashSet<int>();
    }

    private class Scn
    {
        public string name, section, rle, note;
        public int x, y, facing;
        public string stairId;      // != null ⇒ PlaceOnStair(stairId, stairT, +1) thay cho Teleport (đến từ section khác — StageFlow.ArriveStair)
        public int stairT;
        public bool still;          // true = kịch bản không cần đổi X (vd roi tại chỗ)
        public Func<Rec, string> check;   // null = đạt, chuỗi = lý do trượt
    }

    private static List<Scn> queue;
    private static int qIndex, tickLeft;
    private static Scn cur;
    private static Rec rec;
    private static SimonController hooked;
    private static int pass, fail;

    private static Scn[] Scenarios()
    {
        return new[]
        {
            // ---- WalkwayEntranceA (St07 tầng dưới)
            new Scn { name = "wea_start", section = "WalkwayEntranceA", x = 720, y = 160, facing = -1, rle = "L*80 -*20",
                note = "điểm vào St07 (720,160) quay trái (video 0:06.90), đi trái khỏi khối hàng 10 (mép x 672) xuống sàn y 176", check = r => Eq("Y", r.lastY, 176) ?? Eq("X", r.lastX, 640) ?? NoDeath(r) },
            new Scn { name = "wea_floor", section = "WalkwayEntranceA", x = 500, y = 176, facing = -1, rle = "L*400 -*4",
                note = "sàn y 176 đi trái ⇒ chặn bởi bậc (4-7,10) y 160 ở x 136 (video Simon nhảy lên 0:34.2)", check = r => Eq("Y", r.lastY, 176) ?? Eq("X", r.lastX, 136) ?? NoDeath(r) },
            new Scn { name = "wea_jump_step", section = "WalkwayEntranceA", x = 140, y = 176, facing = -1, rle = "-*2 L+A L*30 -*20",
                note = "nhảy trái lên bậc (4-7,10) y 160 (video x 88-108 f1027)", check = r => Eq("Y", r.lastY, 160) ?? NoDeath(r) },
            new Scn { name = "wea_jump_ledge", section = "WalkwayEntranceA", x = 72, y = 160, facing = -1, rle = "-*2 L+A L*24 -*20",
                note = "nhảy trái lên bờ (2-3,8) y 128 (video x 58-65 f1053)", check = r => Eq("Y", r.lastY, 128) ?? NoDeath(r) },
            new Scn { name = "wea_jump_upper", section = "WalkwayEntranceA", x = 60, y = 128, facing = 1, rle = "-*2 R+A R*40 -*20",
                note = "nhảy phải lên bờ (6-11,6) y 96 chân thang (video f1059 → f1082 x 98)", check = r => Eq("Y", r.lastY, 96) ?? NoDeath(r) },
            new Scn { name = "wea_stair_up", section = "WalkwayEntranceA", x = 164, y = 96, facing = -1, rle = "U*220 -*2",
                note = "chân thang ↖ 12 bậc (152,88) (đường x 164) — leo cắt mép trên (StageFlow đổi tầng khi Y ≤ 31; Arena không có StageFlow)", check = r => Le("minY", r.minY, 31) ?? Stair(r) ?? NoDeath(r) },
            new Scn { name = "wea_break_pot", section = "WalkwayEntranceA", x = 318, y = 176, facing = -1, rle = "-*2 A -*8 B -*60", still = true,
                note = "nhảy tại chỗ + roi trái vào khối (18,8) y 128 (video 0:26.63 ô đen ⇒ thịt nướng)", check = r => W.Tiles.IsBroken(18, 8) ? NoDeath(r) : "ô (18,8) chưa vỡ" },
            new Scn { name = "wea_door_entry", section = "WalkwayEntranceA", x = 720, y = 160, facing = 1, rle = "R*60 -*4",
                note = "đi phải vào cột cửa trang trí (ô Door KHÔNG đặc) ⇒ tới mép phải section x 760", check = r => Eq("X", r.lastX, 760) ?? Eq("Y", r.lastY, 160) ?? NoDeath(r) },
            // ---- WalkwayEntranceB (St07 tầng trên)
            new Scn { name = "web_arrive_zigzag", section = "WalkwayEntranceB", stairId = "WalkwayEntranceB_stair1", stairT = 4, rle = "U*240 -*4",
                note = "đến từ WEA: PlaceOnStair ↖ t 4 → đỉnh (68,160) → giữ Lên ⇒ vào thang ↗ stair0 (video f1228-1233 x 66→71) → đỉnh (127,96)", check = r => Eq("Y", r.lastY, 96) ?? Ge("X", r.lastX, 124) ?? NoDeath(r) },
            new Scn { name = "web_drop_floor", section = "WalkwayEntranceB", x = 140, y = 96, facing = 1, rle = "R*40 -*30",
                note = "đi phải khỏi bờ (8-9,6) ⇒ rơi xuống sàn (10-19,10) y 160 (video f1341→1346 x 163→172)", check = r => Eq("Y", r.lastY, 160) ?? Ge("X", r.lastX, 166) ?? NoDeath(r) },
            new Scn { name = "web_landing_walk", section = "WalkwayEntranceB", x = 50, y = 160, facing = 1, rle = "R*30 -*4",
                note = "chiếu nghỉ y 160: đi phải qua miệng thang (4,10) StairFloor tới khối (5,10) ⇒ không rơi", check = r => Eq("Y", r.lastY, 160) ?? Ge("X", r.lastX, 80) ?? NoDeath(r) },
            new Scn { name = "web_pit", section = "WalkwayEntranceB", x = 300, y = 160, facing = 1, rle = "R*40 -*170",
                note = "đi phải khỏi sàn (19,10) vào khe cột 20-21 (hố tới đáy) ⇒ FallDeath Pit", check = r => r.deaths >= 1 && r.diedKind == eTileCollision.Pit ? null : "không chết Pit (deaths " + r.deaths + " " + r.diedKind + ")" },
            new Scn { name = "web_jump_gap", section = "WalkwayEntranceB", x = 314, y = 160, facing = 1, rle = "-*2 R+A R*40 -*20",
                note = "nhảy qua khe 32 px cột 20-21 (video f1601-1633 x 333→366)", check = r => Eq("Y", r.lastY, 160) ?? Ge("X", r.lastX, 352) ?? NoDeath(r) },
            new Scn { name = "web_stair2_up", section = "WalkwayEntranceB", x = 383, y = 160, facing = 1, rle = "U*50 -*2 R*10 -*4",
                note = "chân thang ↗ stair2 (384,152) (đường x 380 + 3) — leo 32 px lên bờ (26-27,8) y 128", check = r => Eq("Y", r.lastY, 128) ?? Stair(r) ?? NoDeath(r) },
            new Scn { name = "web_stair4_down", section = "WalkwayEntranceB", x = 580, y = 96, facing = 1, rle = "D*140 -*4",
                note = "đỉnh thang ↖ stair4 (580,96) — xuống 64 px tới chân (644,160)", check = r => Eq("Y", r.lastY, 160) ?? Stair(r) ?? NoDeath(r) },
            new Scn { name = "web_door", section = "WalkwayEntranceB", x = 724, y = 112, facing = 1, rle = "R*50 -*20",
                note = "khối (44-45,7) y 112 đi phải ⇒ rơi xuống gờ cửa y 128 (video f2436-2440) ⇒ cột cửa Door không đặc ⇒ mép phải x 760", check = r => Eq("Y", r.lastY, 128) ?? Eq("X", r.lastX, 760) ?? NoDeath(r) },
            // ---- WalkwayA (St08 tầng dưới)
            new Scn { name = "wa_floor", section = "WalkwayA", x = 48, y = 128, facing = 1, rle = "R*400 -*4",
                note = "điểm vào St08 (48,128) quay phải, đi phải 400 px trên cầu qua bóng gai (8-9,7) (video f2645-2991)", check = r => Eq("Y", r.lastY, 128) ?? Eq("X", r.lastX, 448) ?? NoDeath(r) },
            new Scn { name = "wa_stairs_zigzag", section = "WalkwayA", x = 575, y = 128, facing = 1, rle = "U*260 -*2",
                note = "chân thang ↗ stair0 (576,120) → chiếu nghỉ (40-43,4) y 64 → giữ Lên ⇒ ↖ stair1 cắt mép trên (video 1:44-1:48.7)", check = r => Le("minY", r.minY, 31) ?? Stair(r) ?? NoDeath(r) },
            // ---- WalkwayB (St08 tầng trên — nới trái 112 px)
            new Scn { name = "wb_arrive_left_end", section = "WalkwayB", stairId = "WalkwayB_stair0", stairT = 4, rle = "U*120 -*2 L*20 -*4",
                note = "đến từ WalkwayA: PlaceOnStair ↖ t 4 → đỉnh (148,128) → đi trái 20 px tới x 128 = mút trái cầu video x 16 ⇒ camera bám = cx 0 (video cx −112)", check = r => Eq("Y", r.lastY, 128) ?? Eq("X", r.lastX, 128) ?? Eq("camMin", r.minCam, 0) ?? NoDeath(r) },
            new Scn { name = "wb_bridge_cross_mouth", section = "WalkwayB", x = 130, y = 128, facing = 1, rle = "R*40 -*4",
                note = "đi phải qua miệng thang (9,8) StairFloor (video f3336-3346 x 146-165) ⇒ không rơi", check = r => Eq("Y", r.lastY, 128) ?? Eq("X", r.lastX, 170) ?? NoDeath(r) },
            new Scn { name = "wb_left_void", section = "WalkwayB", x = 120, y = 128, facing = -1, rle = "L*20 -*200",
                note = "đi trái khỏi mút cầu (x 112) vào trời phần nới ⇒ rơi ra đáy = Pit (video không làm — hình học)", check = r => r.deaths >= 1 && r.diedKind == eTileCollision.Pit ? null : "không chết Pit" },
            new Scn { name = "wb_gap_jump", section = "WalkwayB", x = 490, y = 128, facing = 1, rle = "-*2 R+A R*40 -*20",
                note = "nhảy qua khe cầu 32 px (cột 31-32 sau nới; video f3870-3894)", check = r => Eq("Y", r.lastY, 128) ?? Ge("X", r.lastX, 528) ?? NoDeath(r) },
            new Scn { name = "wb_gap_pit", section = "WalkwayB", x = 480, y = 128, facing = 1, rle = "R*30 -*170",
                note = "đi vào khe cầu cột 31-32 ⇒ FallDeath Pit", check = r => r.deaths >= 1 && r.diedKind == eTileCollision.Pit ? null : "không chết Pit" },
            new Scn { name = "wb_block1", section = "WalkwayB", x = 770, y = 128, facing = 1, rle = "-*2 R+A R*30 -*20",
                note = "nhảy phải lên khối (49-50,7) y 112 (video f4192 x 779)", check = r => Eq("Y", r.lastY, 112) ?? NoDeath(r) },
            new Scn { name = "wb_block2_door", section = "WalkwayB", x = 800, y = 112, facing = 1, rle = "-*2 R+A R*30 R*60 -*20",
                note = "nhảy phải lên khối (51-54,6) y 96 rồi đi vào cột cửa (Door không đặc) ⇒ mép phải x 872", check = r => Eq("Y", r.lastY, 96) ?? Eq("X", r.lastX, 872) ?? NoDeath(r) },
            // ---- MummyMan (St09)
            new Scn { name = "mm_floor", section = "MummyMan", x = 48, y = 96, facing = 1, rle = "R*400 -*4",
                note = "điểm vào St09 (48,96) quay phải, đi phải 400 px trên cầu (video f4566-4690)", check = r => Eq("Y", r.lastY, 96) ?? Eq("X", r.lastX, 448) ?? NoDeath(r) },
            new Scn { name = "mm_gap1", section = "MummyMan", x = 762, y = 96, facing = 1, rle = "-*2 R+A R*40 -*20",
                note = "cầu hỏng khe #1 cột 48-49: nhảy qua (video 2:55.0-2:55.5)", check = r => Eq("Y", r.lastY, 96) ?? Ge("X", r.lastX, 800) ?? NoDeath(r) },
            new Scn { name = "mm_gap2", section = "MummyMan", x = 826, y = 96, facing = 1, rle = "-*2 R+A R*40 -*20",
                note = "khe #2 cột 52-53", check = r => Eq("Y", r.lastY, 96) ?? Ge("X", r.lastX, 864) ?? NoDeath(r) },
            new Scn { name = "mm_gap3", section = "MummyMan", x = 890, y = 96, facing = 1, rle = "-*2 R+A R*40 -*20",
                note = "khe #3 cột 56-57", check = r => Eq("Y", r.lastY, 96) ?? Ge("X", r.lastX, 928) ?? NoDeath(r) },
            new Scn { name = "mm_gap_fall", section = "MummyMan", x = 760, y = 96, facing = 1, rle = "R*20 -*170",
                note = "đi vào khe #1 ⇒ rơi ra đáy = FallDeath Pit", check = r => r.deaths >= 1 && r.diedKind == eTileCollision.Pit ? null : "không chết Pit" },
            new Scn { name = "mm_room_steps", section = "MummyMan", x = 1260, y = 96, facing = 1, rle = "R*200 -*20",
                note = "vào phòng Mummy: bậc khối y 96 → 112 (80-81,7) → 128 (82,8) → 144 (83,9) → sàn 176 (video f5790-5856)",
                check = r => (r.groundYs.Contains(112) && r.groundYs.Contains(128) && r.groundYs.Contains(144) ? null : "không qua đủ bậc 112/128/144") ?? Eq("Y", r.lastY, 176) ?? Ge("X", r.lastX, 1440) ?? NoDeath(r) },
            new Scn { name = "mm_break_pot", section = "MummyMan", x = 1356, y = 176, facing = -1, rle = "-*2 B -*40", still = true,
                note = "sàn phòng y 176 quay trái quất khối thấp nhất (83,9) (Boss_Map_3 7.6 s ⇒ thịt nướng)", check = r => W.Tiles.IsBroken(83, 9) ? NoDeath(r) : "ô (83,9) chưa vỡ" },
        };
    }

    private static string Eq(string what, int got, int want) => got == want ? null : what + " " + got + " ≠ " + want;
    private static string Ge(string what, int got, int want) => got >= want ? null : what + " " + got + " < " + want;
    private static string Le(string what, int got, int want) => got <= want ? null : what + " " + got + " > " + want;
    private static string NoDeath(Rec r) => r.deaths == 0 ? null : "chết " + r.deaths + " lần (" + r.diedKind + ")";
    private static string Stair(Rec r) => r.everStair ? null : "không vào thang";

    public static string RunMoves(string names)
    {
        if (W == null || S == null || Running)
            return W == null || S == null ? "cần Play Arena (Player = Simon)" : "đang chạy";
        var all = Scenarios();
        queue = new List<Scn>();
        foreach (var s in all)
            if (names == "all" || ("," + names + ",").Contains("," + s.name + ","))
                queue.Add(s);
        qIndex = -1;
        pass = fail = 0;
        report.Length = 0;
        Running = true;
        W.Paused = false;
        Application.runInBackground = true;
        NextScenario();
        return "chạy " + queue.Count + " kịch bản";
    }

    private static void NextScenario()
    {
        Unhook();
        qIndex++;
        if (qIndex >= queue.Count)
        {
            NesPad.I.StopReplay();
            report.Append("TỔNG: ").Append(pass).Append(" đạt / ").Append(pass + fail).Append('\n');
            Running = false;
            Status = "xong " + pass + "/" + (pass + fail);
            return;
        }
        cur = queue[qIndex];
        var target = Find(cur.section);
        if (target == null)
        {
            report.Append("THIẾU section ").Append(cur.section).Append('\n');
            fail++;
            NextScenario();
            return;
        }
        GameSession.Current.playerHp = GameSession.Current.shownPlayerHp = 16;
        if (cur.stairId != null)
        {
            W.Warp(target, 0, 0);
            bool placed = false;
            for (int i = 0; i < W.Tiles.StairCount; i++)
            {
                var p = W.Tiles.GetStair(i);
                if (p.stair != null && p.stair.id == cur.stairId)
                {
                    S.PlaceOnStair(p, cur.stairT, 1);
                    placed = true;
                }
            }
            if (!placed)
                report.Append("  không thấy thang ").Append(cur.stairId).Append('\n');
        }
        else
        {
            W.Warp(target, cur.x, cur.y);
            S.Teleport(cur.x, cur.y, cur.facing);
        }
        W.CameraCtl.Target = W.Player;
        W.CameraCtl.SnapToTarget();
        rec = new Rec { startX = S.X };
        var ticks = NesReplay.Decode(cur.rle);
        tickLeft = ticks.Count;
        hooked = S;
        hooked.Died += OnDied;
        W.TickEnded += OnTickEnded;
        NesPad.I.PlayReplay(ticks);
        Status = cur.name;
    }

    private static void OnDied(SimonController s, eTileCollision kind)
    {
        rec.deaths++;
        rec.diedKind = kind;
    }

    private static void OnTickEnded(StageWorld w)
    {
        var s = S;
        rec.ticks++;
        rec.lastX = s.X;
        rec.lastY = s.Y;
        rec.lastState = s.Fsm.CurState;
        rec.minCam = Mathf.Min(rec.minCam, w.CameraCtl.X);
        if (s.OnStairs)
            rec.everStair = true;
        if (!s.IsDying)
        {
            rec.minY = Mathf.Min(rec.minY, s.Y);
            rec.maxY = Mathf.Max(rec.maxY, s.Y);
            if (!s.OnStairs && w.Tiles.IsOnGround(s))
                rec.groundYs.Add(s.Y);
        }
        if (rec.ticks % 10 == 1 || tickLeft == 1)
            rec.trace.Add(rec.ticks + ":" + s.X + "," + s.Y + "," + s.Fsm.CurState);
        if (--tickLeft > 0)
            return;
        string why = null;
        try
        {
            why = cur.check(rec);
            if (why == null && !cur.still && rec.deaths == 0 && rec.lastX == rec.startX)
                why = "X cuối = X đầu " + rec.startX + " (Simon không di chuyển — kẹt?)";
        }
        catch (Exception ex)
        {
            why = "ngoại lệ: " + ex.Message;
        }
        if (why == null) pass++; else fail++;
        report.Append(why == null ? "PASS " : "FAIL ").Append(cur.name).Append(" [").Append(cur.section).Append("] — ").Append(cur.note).Append('\n')
              .Append("     đầu X ").Append(rec.startX).Append(" | cuối X ").Append(rec.lastX).Append(" Y ").Append(rec.lastY).Append(" ").Append(rec.lastState)
              .Append(" | Y min ").Append(rec.minY).Append(" max ").Append(rec.maxY).Append(" | thang ").Append(rec.everStair ? "có" : "không")
              .Append(" | cx min ").Append(rec.minCam)
              .Append(" | chết ").Append(rec.deaths).Append(rec.deaths > 0 ? " " + rec.diedKind : "").Append(" | ").Append(rec.ticks).Append(" tick")
              .Append(why != null ? " | LÝ DO: " + why : "").Append('\n')
              .Append("     vết: ").Append(string.Join(" ", rec.trace)).Append('\n');
        NextScenario();
    }

    private static void Unhook()
    {
        if (W != null)
            W.TickEnded -= OnTickEnded;
        if (hooked != null)
        {
            hooked.Died -= OnDied;
            hooked = null;
        }
    }

    public static string Report()
    {
        return (Running ? "ĐANG CHẠY " + Status + "\n" : Status + "\n") + report;
    }

    public static void SaveReport(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, Report());
    }
}
#endif
