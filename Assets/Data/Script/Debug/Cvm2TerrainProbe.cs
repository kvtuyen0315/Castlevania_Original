#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Harness kiểm địa hình Map 2 (CVM2 05) — CHỈ Editor, scene Arena (Player = Simon), gọi qua MCP execute_code khi Play:
///   Cvm2TerrainProbe.Setup()                    ⇒ dựng 6 prefab Assets/Data/Prefabs/Level/Block2/*.prefab làm con (tắt) của StageWorld + AddSection
///                                                 (chỉ lúc chạy — KHÔNG đổi Arena.unity; như phòng boss thử CVGP 19).
///   Cvm2TerrainProbe.StartShots(dir, "Section:cx,cx;…", hideHud) ⇒ thế giới dừng, ẩn sprite động (+ HUD nếu hideHud), chụp Game view
///                                                 terrain2_&lt;Section&gt;_cx&lt;N&gt;.png (so sheet bằng Tools/cvm2/accept5.py terrain).
///   Cvm2TerrainProbe.RunMoves("all"|"a,b")      ⇒ kịch bản Simon (đi / nhảy / leo thang / rơi hố / roi khối) theo replay RLE, tự chấm ⇒ Report().
///   Cvm2TerrainProbe.Cleanup()                  ⇒ gỡ section Block 2, warp về Arena (48,176).
/// Chỉ đọc gameplay (không đổi lõi) — không thuộc bộ hồi quy (kết quả ghi ClaudeNotes/cvm2/_cache/05/).
/// </summary>
public static class Cvm2TerrainProbe
{
    public const string PREFAB_DIR = "Assets/Data/Prefabs/Level/Block2/";
    public static readonly string[] SECTION_IDS = { "TowerEntranceA", "TowerEntranceB", "TowerHallA", "TowerHallB", "TowerHallC", "Medusa" };

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
        return "đã thêm " + n + " section Block 2 (tổng " + W.Sections.Count + ")";
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
                W.RemoveSection(s);   // CVM2 06: không để tham chiếu chết trong StageWorld (Warp sau đó sẽ lỗi)
                UnityEngine.Object.DestroyImmediate(s.gameObject);
            }
        added.Clear();
        return "đã gỡ section Block 2";
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
                shots.Add(new Shot { name = "terrain2_" + kv[0].Trim() + "_cx" + c.Trim(), section = kv[0].Trim(), cx = int.Parse(c.Trim()) });
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

    // forceRenderingOff (SyncView không bật lại) — như Map1AcceptProbe.HideSprites; HUD: tắt GameObject "HUD" (gồm nền đen NES y 0–47)
    private static void HideAll()
    {
        ShowAll();
        foreach (var r in UnityEngine.Object.FindObjectsOfType<SpriteRenderer>())
        {
            string l = r.sortingLayerName;
            if (!r.forceRenderingOff && (l == "BackgroundFX" || l == "Character" || l == "Overlay" || (shotHideHud && l == "HUD")))
            {
                r.forceRenderingOff = true;   // HUD: gồm HudBackdrop (nền đen NES y 0–47, con StageWorld)
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
        public int ticks, minY = int.MaxValue, maxY = int.MinValue, deaths;
        public eTileCollision diedKind;
        public int lastX, lastY;
        public eSimonState lastState;
        public bool everStair;
        public readonly List<string> trace = new List<string>();
    }

    private class Scn
    {
        public string name, section, rle, note;
        public int x, y, facing;
        public string stairId;      // != null ⇒ PlaceOnStair(stairId, stairT, +1) thay cho Teleport (đến từ section khác — StageFlow.ArriveStair)
        public int stairT;
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
            new Scn { name = "tea_floor", section = "TowerEntranceA", x = 64, y = 176, facing = 1, rle = "R*40 -*4",
                note = "điểm vào St04 (64,176) quay phải, đi phải 40 px trên sàn hàng 11", check = r => Eq("Y", r.lastY, 176) ?? Eq("X", r.lastX, 104) ?? NoDeath(r) },
            new Scn { name = "tea_stair1_up", section = "TowerEntranceA", x = 111, y = 176, facing = 1, rle = "U*140 -*4 R*30 -*4",
                note = "chân thang thấp ↗ (đường x 108 + 3) — leo 64 px lên gờ hàng 7 (y 112) rồi đi phải lên gờ", check = r => Eq("Y", r.lastY, 112) ?? Ge("X", r.lastX, 190) ?? NoDeath(r) },
            new Scn { name = "tea_stair0_up", section = "TowerEntranceA", x = 79, y = 80, facing = 1, rle = "U*110 -*2",
                note = "bờ hàng 5 chân thang trên ↗ (đường x 76 + 3) — leo cắt mép trên (StageFlow đổi tầng khi Y ≤ 31; Arena không có StageFlow)", check = r => Le("minY", r.minY, 31) ?? Stair(r) ?? NoDeath(r) },
            new Scn { name = "tea_break_wall", section = "TowerEntranceA", x = 214, y = 112, facing = 1, rle = "-*2 B -*40",
                note = "đứng gờ hàng 7 x 214 quay phải, roi vào tường phải (video 0:15.63 vỡ ô (14,5))", check = r => W.Tiles.IsBroken(14, 5) ? NoDeath(r) : "ô (14,5) chưa vỡ" },
            new Scn { name = "teb_arrive_up", section = "TowerEntranceB", stairId = "TowerEntranceB_stair0", stairT = 4, rle = "U*160 -*4",
                note = "đến từ TEA: PlaceOnStair t 4 (Y 188 = STAIR_UP_ARRIVE_Y) rồi leo tới đỉnh (460,112)", check = r => Eq("Y", r.lastY, 112) ?? NoDeath(r) },
            new Scn { name = "teb_jump_ledge", section = "TowerEntranceB", x = 462, y = 112, facing = -1, rle = "-*2 L+A L*45 -*20",
                note = "đỉnh thang (462,112) nhảy trái lên gờ hàng 5 y 80 (video 0:32.7-0:33.1, 0:36.5-0:36.9)", check = r => Eq("Y", r.lastY, 80) ?? NoDeath(r) },
            new Scn { name = "teb_floor", section = "TowerEntranceB", x = 440, y = 80, facing = -1, rle = "L*80 -*4",
                note = "đi trái trên bờ cột 22-27 tới mép khe 18-21", check = r => Eq("Y", r.lastY, 80) ?? Eq("X", r.lastX, 360) ?? NoDeath(r) },
            new Scn { name = "teb_pit", section = "TowerEntranceB", x = 200, y = 80, facing = -1, rle = "L*40 -*170",
                note = "đi trái khỏi bờ cột 12-17 vào khe 10-11 (hố tới đáy) ⇒ rơi ra đáy = FallDeath Pit", check = r => r.deaths >= 1 && r.diedKind == eTileCollision.Pit ? null : "không chết Pit (deaths " + r.deaths + " " + r.diedKind + ")" },
            new Scn { name = "teb_door", section = "TowerEntranceB", x = 24, y = 80, facing = -1, rle = "L*40 -*4",
                note = "go cửa trái cột 0-1: cột cửa (ô Door) KHÔNG đặc ⇒ tới mép trái section x 8 (Arena không có StageFlow)", check = r => Eq("X", r.lastX, 8) ?? Eq("Y", r.lastY, 80) ?? NoDeath(r) },
            new Scn { name = "halla_stair_up", section = "TowerHallA", x = 47, y = 48, facing = 1, rle = "U*80 -*2",
                note = "bờ hàng 3 cột 2-3 chân thang ↗ (đường x 44 + 3) — leo cắt mép trên sang Hall B", check = r => Le("minY", r.minY, 31) ?? Stair(r) ?? NoDeath(r) },
            new Scn { name = "halla_floor", section = "TowerHallA", x = 720, y = 80, facing = -1, rle = "L*420 -*4",
                note = "điểm vào St05 (720,80) quay trái, đi trái 392 px trên sàn hàng 5 ⇒ chặn ở x 328 bởi 2 khối nhô cao hàng 3 cột 18-19 (ngang đầu, trên hố) — video 1:22.4 Simon NHẢY lên khối", check = r => Eq("Y", r.lastY, 80) ?? Eq("X", r.lastX, 328) ?? NoDeath(r) },
            new Scn { name = "halla_blocks", section = "TowerHallA", x = 348, y = 80, facing = -1, rle = "-*2 L+A L*90 -*20",
                note = "nhảy trái từ x 348 lên 2 khối nhô cao (y 48) rồi đi trái xuống sàn cột 17 (video 1:22.47-1:23.67: 348 → khối 321 → 281 → sàn 277)", check = r => (r.trace.Exists(t => t.EndsWith(",48,Walk")) ? null : "không đi trên khối y 48") ?? Eq("Y", r.lastY, 80) ?? Le("X", r.lastX, 271) ?? NoDeath(r) },
            new Scn { name = "halla_pit", section = "TowerHallA", x = 304, y = 120, facing = -1, rle = "-*170",
                note = "thả Simon giữa hố cột 18-19 dưới mặt sàn ⇒ rơi ra đáy section = FallDeath Pit (hố wiki Stage 5)", check = r => r.deaths >= 1 && r.diedKind == eTileCollision.Pit ? null : "không chết Pit" },
            new Scn { name = "hallb_arrive_up", section = "TowerHallB", stairId = "TowerHallB_stair0", stairT = 4, rle = "U*80 -*2 R*10 -*2",
                note = "đến từ Hall A: PlaceOnStair t 4 rồi leo tới đỉnh (92,160), bước phải lên khối (6,10)", check = r => Eq("Y", r.lastY, 160) ?? NoDeath(r) },
            new Scn { name = "hallb_pit", section = "TowerHallB", x = 112, y = 160, facing = 1, rle = "R*40 -*170",
                note = "khối hàng 10 cột 6-7, đi phải vào hố cột 8-9 ⇒ FallDeath Pit", check = r => r.deaths >= 1 && r.diedKind == eTileCollision.Pit ? null : "không chết Pit" },
            new Scn { name = "hallb_floor", section = "TowerHallB", x = 520, y = 144, facing = -1, rle = "L*100 -*4",
                note = "sàn hàng 9 cột 26-47 (dưới khối hàng 7 cột 36-37): đi trái 100 px tới x 420, vẫn trên cột 26 (mép hố 24-25 ở x 416)", check = r => Eq("Y", r.lastY, 144) ?? Eq("X", r.lastX, 420) ?? NoDeath(r) },
            new Scn { name = "hallc_entry_drop", section = "TowerHallC", x = 720, y = 112, facing = -1, rle = "L*40 -*20",
                note = "điểm vào St06 (720,112) quay trái, đi trái xuống bậc hàng 8 y 128 (video 2:30.1)", check = r => Eq("Y", r.lastY, 128) ?? NoDeath(r) },
            new Scn { name = "hallc_jump_step", section = "TowerHallC", x = 521, y = 176, facing = -1, rle = "-*2 L+A L*45 -*20",
                note = "sàn thấp y 176 nhảy trái lên bậc 32 px cột 30-31 y 144 (video 2:38.6-2:39.1)", check = r => Eq("Y", r.lastY, 144) ?? NoDeath(r) },
            new Scn { name = "hallc_crushers", section = "TowerHallC", x = 505, y = 144, facing = -1, rle = "L*170 -*4",
                note = "đi trái dưới 3 bẫy ép (ô đã vá đen, KHÔNG Hazard trên lưới) x 505 → 335", check = r => Eq("Y", r.lastY, 144) ?? Eq("X", r.lastX, 335) ?? NoDeath(r) },
            new Scn { name = "hallc_stair0_up", section = "TowerHallC", x = 159, y = 128, facing = 1, rle = "U*70 -*4",
                note = "khối chân thang ngắn (đường x 156 + 3) — leo 32 px tới đỉnh (188,96)", check = r => Eq("Y", r.lastY, 96) ?? NoDeath(r) },
            new Scn { name = "hallc_stair1_up", section = "TowerHallC", x = 655, y = 48, facing = 1, rle = "U*80 -*2",
                note = "sàn trên hàng 3 chân thang ↗ (đường x 652 + 3) — leo cắt mép trên sang sảnh Medusa", check = r => Le("minY", r.minY, 31) ?? Stair(r) ?? NoDeath(r) },
            new Scn { name = "medusa_arrive_up", section = "Medusa", stairId = "Medusa_stair0", stairT = 4, rle = "U*100 -*2 R*6 -*2 L+A L*40 -*20",
                note = "đến từ Hall C: PlaceOnStair t 4 → đỉnh (716,144) → bước phải → nhảy trái lên sàn sảnh y 128 (video 3:19.0-3:19.8)", check = r => Eq("Y", r.lastY, 128) ?? NoDeath(r) },
            new Scn { name = "medusa_boss_floor", section = "Medusa", x = 120, y = 144, facing = -1, rle = "L*130 -*4",
                note = "sàn phòng boss y 144: đi trái qua tượng (nền) tới cột cửa trái ⇒ x 8", check = r => Eq("Y", r.lastY, 144) ?? Eq("X", r.lastX, 8) ?? NoDeath(r) },
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
        rec = new Rec();
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
        if (s.OnStairs)
            rec.everStair = true;
        if (!s.IsDying)
        {
            rec.minY = Mathf.Min(rec.minY, s.Y);
            rec.maxY = Mathf.Max(rec.maxY, s.Y);
        }
        if (rec.ticks % 10 == 1 || tickLeft == 1)
            rec.trace.Add(rec.ticks + ":" + s.X + "," + s.Y + "," + s.Fsm.CurState);
        if (--tickLeft > 0)
            return;
        string why = null;
        try
        {
            why = cur.check(rec);
        }
        catch (Exception ex)
        {
            why = "ngoại lệ: " + ex.Message;
        }
        if (why == null) pass++; else fail++;
        report.Append(why == null ? "PASS " : "FAIL ").Append(cur.name).Append(" [").Append(cur.section).Append("] — ").Append(cur.note).Append('\n')
              .Append("     cuối X ").Append(rec.lastX).Append(" Y ").Append(rec.lastY).Append(" ").Append(rec.lastState)
              .Append(" | Y min ").Append(rec.minY).Append(" max ").Append(rec.maxY).Append(" | thang ").Append(rec.everStair ? "có" : "không")
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
