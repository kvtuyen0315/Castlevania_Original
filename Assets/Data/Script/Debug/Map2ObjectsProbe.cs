#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness CVM2 10 — lớp vật thể Block 2 (CHỈ Editor, scene Arena, Player = Simon). Qua MCP khi Play:
///   Map2ObjectsProbe.Start("candles,blocks,secrets,spawns,mech,boss", shotDir, "&lt;danh sách ảnh&gt;") ⇒ chạy theo NesClock, TỰ CHẤM ⇒ Report()
///     candles : mỗi nến của Map2Objects.json: điểm sinh có, vị trí (x, Y) đúng, phá ⇒ item = cột content (video) qua ConfigDrop + ItemDrops
///     blocks  : 6 khối phá (lưới CVM2 05): Break ⇒ item = VIDEO_BLOCKS
///     secrets : vương miện TEA (phá 2 ô + đi vào khe), rương Hall A (đứng yên trên 2 khối), túi vòm 4 sảnh Medusa (chỉ wiki):
///               tick kích hoạt, x Simon lúc kích hoạt, thời gian trồi, vị trí nghỉ, điểm khi nhặt
///     spawns  : mọi SpawnPoint không phải nến sinh đúng loại/chỗ (dơi treo ở Perch); bộ sinh Medusa Head/Ghost theo vùng (x, Y, maxSpawns)
///     mech    : MechSpawner sinh bệ/bẫy đúng số ở tick đầu sau khi nạp section
///     boss    : BossArena Medusa sinh QueenMedusa ở chân tượng (112, 144)
///   shotDir != "" ⇒ chụp Game view (ẩn Simon) ở các khung "Section:cx:tên;…" — so video bằng Tools/cvm2/pairs10.py.
/// Section Block 2 dựng lúc chạy (Cvm2TerrainProbe.Setup) + instance &lt;Section&gt;_Objects.prefab làm con — KHÔNG đổi Arena.unity; gỡ cuối bộ
/// (Map2MechProbe.BackToArena). Không thuộc bộ hồi quy (kết quả ClaudeNotes/cvm2/_cache/10/).
/// </summary>
public static class Map2ObjectsProbe
{
    public const string TABLE = "Assets/Data/Config/Level/Block2/Map2Objects.json";
    public const string OBJ_DIR = "Assets/Data/Prefabs/Level/Block2/Objects/";

    // nội dung khối phá — video:Map_2 (MAP2_PLACEMENT §3; TEB (22,5) chỉ wiki)
    static readonly string[,] VIDEO_BLOCKS =
    {
        { "TowerEntranceA", "14", "5", "None" }, { "TowerEntranceA", "14", "6", "None" }, { "TowerEntranceB", "22", "5", "DoubleShot" },
        { "TowerHallB", "39", "3", "PotRoast" }, { "TowerHallC", "44", "7", "PotRoast" }, { "TowerHallC", "8", "8", "DoubleShot" },
    };
    // chỗ Simon đứng an toàn khi kiểm (ngoài vùng bộ sinh, trên sàn đặc)
    static readonly Dictionary<string, Vector2Int> SAFE = new Dictionary<string, Vector2Int>
    {
        { "TowerEntranceA", new Vector2Int(64, 176) }, { "TowerEntranceB", new Vector2Int(420, 80) }, { "TowerHallA", new Vector2Int(100, 80) },
        { "TowerHallB", new Vector2Int(700, 80) }, { "TowerHallC", new Vector2Int(740, 128) }, { "Medusa", new Vector2Int(650, 128) },
    };
    // video:Map_2 (§2 MAP2_PLACEMENT): bí mật
    const int VIDEO_CROWN_X = 136, VIDEO_CROWN_Y = 177, VIDEO_CROWN_TRIGGER_X = 217;
    const int VIDEO_CHEST_X = 400, VIDEO_CHEST_Y = 81, VIDEO_STAND_TICKS = 120;
    const int WIKI_BAG_X = 492, WIKI_BAG_Y = 129;
    const int VIDEO_RISE_TICKS = 64;   // 16 px @ 0.25 px/tick

    [Serializable] class Candle { public string id, content; public int x, y; }
    [Serializable] class Section { public string id; public Candle[] candles; }
    [Serializable] class Table { public Section[] sections; }

    public static bool Running { get; private set; }
    public static string Status { get; private set; } = "";
    static readonly StringBuilder log = new StringBuilder();
    static int pass, fail;
    static IEnumerator<int> job;
    static StageWorld W;
    static SimonController S => W != null ? W.Player as SimonController : null;
    static string shotDir;
    static readonly List<string[]> shots = new List<string[]>();
    static readonly List<Renderer> hidden = new List<Renderer>();

    // ================================================================ chạy

    public static string Start(string what, string shotDirectory, string shotSpec)
    {
        W = StageWorld.Current;
        if (!Application.isPlaying || W == null || S == null)
            return "cần Play scene Arena (Player = Simon)";
        if (Running)
            return "đang chạy";
        string r = Cvm2TerrainProbe.Setup();
        int attached = AttachObjects();
        log.Length = 0;
        pass = fail = 0;
        shotDir = shotDirectory ?? "";
        shots.Clear();
        if (!string.IsNullOrEmpty(shotSpec))
            foreach (var part in shotSpec.Split(';'))
            {
                var kv = part.Trim().Split(':');
                if (kv.Length >= 3)
                    shots.Add(kv);
            }
        if (shotDir.Length > 0)
            Directory.CreateDirectory(shotDir);
        log.Append("# Map2ObjectsProbe ").Append(what).Append(" — ").Append(r).Append("; Objects gắn ").Append(attached).Append('\n');
        job = Main(what.Split(',')).GetEnumerator();
        W.TickEnded -= OnTick;
        W.TickEnded += OnTick;
        Application.runInBackground = true;
        Running = true;
        Status = "chạy";
        return "bắt đầu " + what + " (" + shots.Count + " ảnh)";
    }

    public static string Report() => log.ToString();

    static void OnTick(StageWorld w)
    {
        if (!Running)
            return;
        bool more;
        try
        {
            more = job.MoveNext();
        }
        catch (Exception e)
        {
            Fail("NGOẠI LỆ " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
            more = false;
        }
        if (!more)
            Finish();
    }

    static void Finish()
    {
        W.TickEnded -= OnTick;
        ShowSimon();
        W.Paused = false;
        NesPad.I.StopReplay();
        Map2MechProbe.BackToArena();
        CvgpDebug.CheatInvincible = false;
        // camera bám Simon lại (Cam() bỏ Target) — không để trạng thái lạ cho bộ hồi quy chạy sau trong cùng phiên Play
        if (W.CameraCtl != null)
        {
            W.CameraCtl.Target = W.Player;
            W.CameraCtl.SnapToTarget();
        }
        log.Append("== TỔNG ").Append(pass).Append(" đạt / ").Append(fail).Append(" trượt\n");
        if (shotDir.Length > 0)
            File.WriteAllText(Path.Combine(shotDir, "probe_report.txt"), log.ToString());
        Running = false;
        Status = "xong " + pass + "/" + (pass + fail);
    }

    static void Pass(string m) { pass++; log.Append("PASS ").Append(m).Append('\n'); }
    static void Fail(string m) { fail++; log.Append("FAIL ").Append(m).Append('\n'); }
    static void Check(bool ok, string m) { if (ok) Pass(m); else Fail(m); }

    static IEnumerable<int> Main(string[] parts)
    {
        var set = new HashSet<string>(parts);
        if (set.Contains("candles")) foreach (var t in Candles()) yield return t;
        if (set.Contains("blocks")) foreach (var t in Blocks()) yield return t;
        if (set.Contains("secrets")) foreach (var t in Secrets()) yield return t;
        if (set.Contains("spawns")) foreach (var t in Spawns()) yield return t;
        if (set.Contains("mech")) foreach (var t in Mech()) yield return t;
        if (set.Contains("boss")) foreach (var t in Boss()) yield return t;
        foreach (var t in Shots()) yield return t;
    }

    // ================================================================ dựng

    static int AttachObjects()
    {
        int n = 0;
        foreach (var id in Cvm2TerrainProbe.SECTION_IDS)
        {
            var s = Cvm2TerrainProbe.Find(id);
            if (s == null || s.transform.Find(id + "_Objects") != null)
                continue;
            var pf = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(OBJ_DIR + id + "_Objects.prefab");
            if (pf == null)
                continue;
            var go = UnityEngine.Object.Instantiate(pf, s.transform);
            go.name = id + "_Objects";
            n++;
        }
        return n;
    }

    static void Prep()
    {
        var ses = GameSession.Current;
        ses.ResetForNewGame();
        ses.whipLevel = 3;              // video Map_2: roi cấp 3 suốt Block 2 ⇒ nến tim nhỏ không đổi thành roi
        ses.subWeapon = eSubWeapon.Stopwatch;   // ⇒ không đổi tim nhỏ thành II (ItemDrops.Resolve); nhặt II ⇒ +700
        ses.hearts = 20;
        ses.NotifyChanged();
        CvgpDebug.CheatInvincible = true;   // enemy không làm Simon lùi/rơi hố khi đang kiểm vật thể
    }

    static void Go(string section, int x, int y, int facing)
    {
        var s = Cvm2TerrainProbe.Find(section);
        NesPad.I.StopReplay();
        W.Paused = false;
        W.Warp(s, x, y);
        S.Teleport(x, y, facing);
        W.ResetRng();
        W.CameraCtl.Target = W.Player;
        W.CameraCtl.SnapToTarget();
    }

    static void Cam(int cx)
    {
        W.CameraCtl.Target = null;
        W.CameraCtl.SnapTo(cx);
    }

    static T Find<T>(Func<T, bool> pred) where T : Entity
    {
        for (int g = 0; g < 6; g++)
        {
            List<Entity> list;
            try { list = W.GroupList(g); } catch { break; }
            if (list == null) continue;
            foreach (var e in list)
                if (e is T && e.Alive && pred((T)e))
                    return (T)e;
        }
        return null;
    }

    static int Count<T>() where T : Entity
    {
        int n = 0;
        for (int g = 0; g < 6; g++)
        {
            List<Entity> list;
            try { list = W.GroupList(g); } catch { break; }
            if (list == null) continue;
            foreach (var e in list)
                if (e is T && e.Alive)
                    n++;
        }
        return n;
    }

    static SpawnPoint Point(string name)
    {
        foreach (var p in W.SpawnPoints)
            if (p != null && p.name == name)
                return p;
        return null;
    }

    // ================================================================ nến

    static IEnumerable<int> Candles()
    {
        var table = JsonUtility.FromJson<Table>(File.ReadAllText(TABLE, Encoding.UTF8));
        foreach (var sec in table.sections)
        {
            Prep();
            var safe = SAFE[sec.id];
            Go(sec.id, safe.x, safe.y, -1);
            yield return 0;
            foreach (var c in sec.candles)
            {
                var sp = Point("Candle_" + c.id);
                // khung hiện (video: x màn 8–248): x màn 7 ⇒ chưa sinh, x màn 8 ⇒ sinh (chỉ khi camera tới được 2 vị trí đó)
                if (sp != null && sp.State == SpawnPoint.eState.Ready && c.x - 8 >= W.CameraCtl.MinX && c.x - 7 <= W.CameraCtl.MaxX)
                {
                    Cam(c.x - 7);
                    yield return 0;
                    bool none7 = sp.Live == null;
                    Cam(c.x - 8);
                    yield return 0;
                    Check(none7 && sp.Live != null, "nến " + c.id + " khung: x màn 7 ⇒ " + (none7 ? "chưa sinh" : "ĐÃ sinh") + ", x màn 8 ⇒ " + (sp.Live != null ? "sinh" : "CHƯA sinh"));
                }
                Cam(c.x - 128);
                yield return 0;
                yield return 0;
                var cp = sp != null ? sp.Live as CandleProp : null;
                if (cp == null)
                {
                    Fail("nến " + c.id + " không sinh (SpawnPoint " + (sp != null) + ")");
                    continue;
                }
                bool posOk = cp.X == c.x && cp.Y == c.y && cp.objectId == c.id;
                var hit = default(HitInfo);
                cp.OnHurt(ref hit);
                int t = 0;
                while (cp.spawnedItem == null && t++ < 60)
                    yield return 0;
                var want = (eItemId)Enum.Parse(typeof(eItemId), c.content);
                var got = cp.spawnedItem != null ? cp.spawnedItem.id : cp.dropped;
                Check(posOk && got == want, "nến " + sec.id + " " + c.id + " (" + cp.X + "," + cp.Y + ") id " + cp.objectId + " ⇒ " + got
                      + (got == want ? "" : " ≠ " + want) + " sau " + t + " tick");
                if (cp.spawnedItem != null && cp.spawnedItem.Alive)
                    cp.spawnedItem.Kill(eKillReason.Cleared);
            }
        }
    }

    // ================================================================ khối phá

    static IEnumerable<int> Blocks()
    {
        for (int i = 0; i < VIDEO_BLOCKS.GetLength(0); i++)
        {
            string sec = VIDEO_BLOCKS[i, 0];
            int col = int.Parse(VIDEO_BLOCKS[i, 1]), row = int.Parse(VIDEO_BLOCKS[i, 2]);
            var want = (eItemId)Enum.Parse(typeof(eItemId), VIDEO_BLOCKS[i, 3]);
            Prep();
            var safe = SAFE[sec];
            Go(sec, safe.x, safe.y, -1);
            Cam(col * 16 - 120);
            yield return 0;
            var b = Find<BreakableBlock>(x => x.col == col && x.row == row);
            if (b == null)
            {
                Fail("khối " + sec + " (" + col + "," + row + ") không có thực thể");
                continue;
            }
            string oid = b.ObjectId;
            b.Break();
            var got = b.spawnedItem != null ? b.spawnedItem.id : eItemId.None;
            bool cell = W.Tiles.IsBroken(col, row);
            Check(got == want && cell, "khối " + oid + " ⇒ " + got + (got == want ? "" : " ≠ " + want) + (cell ? "" : " (ô chưa vỡ)"));
            if (got == eItemId.DoubleShot && b.spawnedItem != null)
            {
                // nhặt khi cầm đồng hồ ⇒ +700 (video Hall C f5148 +700)
                int sc0 = GameSession.Current.score;
                var it = b.spawnedItem;
                int t = 0;
                while (it.Alive && it.motion != ItemPickup.eMotion.Rest && t++ < 120)
                    yield return 0;
                S.Teleport(it.X, it.Y, -1);
                t = 0;
                while (it.Alive && t++ < 10)
                    yield return 0;
                Check(GameSession.Current.score - sc0 == 700, "nhặt II khi cầm đồng hồ ⇒ +" + (GameSession.Current.score - sc0) + " (video +700)");
            }
            if (b.spawnedItem != null && b.spawnedItem.Alive)
                b.spawnedItem.Kill(eKillReason.Cleared);
        }
    }

    // ================================================================ bí mật

    static SecretTrigger SecretOf(string name)
    {
        var sp = Point(name);
        return sp != null ? sp.Live as SecretTrigger : null;
    }

    static IEnumerable<int> Secrets()
    {
        // (1) vương miện TEA: phá (14,5)/(14,6), đi phải vào khe
        Prep();
        Go("TowerEntranceA", 200, 112, 1);
        yield return 0;
        var tr = SecretOf("Secret_Crown");
        Check(tr != null, "TEA Secret_Crown sinh");
        // chưa phá: đi phải 40 tick ⇒ tường chặn ở x ≤ 216, không kích hoạt
        NesPad.I.PlayReplay("R*40");
        for (int i = 0; i < 42; i++)
            yield return 0;
        Check(tr != null && !tr.fired && S.X <= 216, "chưa phá tường: Simon dừng ở x " + S.X + " (≤ 216), vương miện chưa hiện");
        NesPad.I.StopReplay();
        S.Teleport(200, 112, 1);
        foreach (var t in Blocks2(new[] { 14, 14 }, new[] { 5, 6 })) yield return t;
        NesPad.I.PlayReplay("R*40 -*200");
        int k = 0, fireAt = -1, fireX = -1;
        while (k++ < 60 && tr != null && !tr.fired)
            yield return 0;
        if (tr != null && tr.fired)
        {
            fireAt = k;
            fireX = S.X;
        }
        var crown = tr != null ? tr.spawnedItem : null;
        Check(crown != null && crown.id == eItemId.Crown && fireX >= VIDEO_CROWN_TRIGGER_X && fireX <= VIDEO_CROWN_TRIGGER_X + 1,
              "vương miện kích hoạt khi Simon x " + fireX + " (video: thân chồng khe = x " + VIDEO_CROWN_TRIGGER_X + ")");
        foreach (var t in RiseAndPick(crown, VIDEO_CROWN_X, VIDEO_CROWN_Y, 2000, 0, "crown_rest")) yield return t;

        // (2) rương Hall A: đứng yên trên 2 khối nhô
        Prep();
        Go("TowerHallA", 306, 48, -1);
        int t0 = W.TickCount;
        yield return 0;
        var tc = SecretOf("Secret_TreasureChest");
        k = 0;
        while (k++ < 200 && tc != null && !tc.fired)
            yield return 0;
        int held = W.TickCount - t0;
        var chest = tc != null ? tc.spawnedItem : null;
        Check(chest != null && chest.id == eItemId.TreasureChest && Math.Abs(held - VIDEO_STAND_TICKS) <= 2,
              "rương kích hoạt sau " + held + " tick đứng yên (video ≈ 121 ⇒ " + VIDEO_STAND_TICKS + ")");
        foreach (var t in RiseAndPick(chest, VIDEO_CHEST_X, VIDEO_CHEST_Y, 2000, 218, "chest_rest")) yield return t;
        // đứng yên NGOÀI 2 khối ⇒ không có
        Prep();
        Go("TowerHallA", 360, 80, -1);
        yield return 0;
        tc = SecretOf("Secret_TreasureChest");
        for (int i = 0; i < 160; i++)
            yield return 0;
        Check(tc == null || !tc.fired, "đứng yên ở sàn x 360 (ngoài 2 khối) 160 tick ⇒ không có rương");

        // (3) túi tiền vòm thứ 4 sảnh Medusa (chỉ wiki) — Simon đứng mép trái vòm (x 474): túi trồi giữa vòm (492) không chạm thân
        //     (đứng giữa vòm ⇒ thân chồng ô túi ⇒ nhặt ngay lúc trồi — chạy thử lần 1)
        Prep();
        Go("Medusa", 474, 128, -1);
        t0 = W.TickCount;
        yield return 0;
        var tb = SecretOf("Secret_ArchBag");
        k = 0;
        while (k++ < 200 && tb != null && !tb.fired)
            yield return 0;
        held = W.TickCount - t0;
        var bag = tb != null ? tb.spawnedItem : null;
        Check(bag != null && bag.id == eItemId.FlashingMoneyBag && Math.Abs(held - VIDEO_STAND_TICKS) <= 2,
              "túi vòm 4 kích hoạt sau " + held + " tick (wiki — holdTicks theo rương)");
        foreach (var t in RiseAndPick(bag, WIKI_BAG_X, WIKI_BAG_Y, 1000, -1, null)) yield return t;
    }

    static IEnumerable<int> Blocks2(int[] cols, int[] rows)
    {
        for (int i = 0; i < cols.Length; i++)
        {
            int c = cols[i], r = rows[i];
            var b = Find<BreakableBlock>(x => x.col == c && x.row == r);
            if (b != null)
                b.Break();
            else
                Fail("không có khối (" + c + "," + r + ")");
        }
        yield return 0;
    }

    static IEnumerable<int> RiseAndPick(ItemPickup it, int wx, int wy, int points, int shotCx, string shotName)
    {
        if (it == null)
        {
            Fail("không có item để kiểm trồi/nhặt");
            yield break;
        }
        int t = 0;
        while (it.Alive && it.motion == ItemPickup.eMotion.Rise && t < 200)
        {
            t++;
            yield return 0;
        }
        Check(it.X == wx && it.Y == wy && Math.Abs(t - VIDEO_RISE_TICKS) <= 1,
              it.id + " trồi " + t + " tick (video 16 px @ 0.25 = " + VIDEO_RISE_TICKS + ") nghỉ ở (" + it.X + "," + it.Y + ") (video (" + wx + "," + wy + "))");
        if (shotCx >= 0 && shotName != null && shotDir.Length > 0)
            foreach (var x in Shot(shotName, shotCx, false)) yield return x;
        int sc0 = GameSession.Current.score;
        S.Teleport(it.X, FloorUnder(it.X, it.Y), -1);
        t = 0;
        while (it.Alive && t++ < 20)
            yield return 0;
        Check(!it.Alive && GameSession.Current.score - sc0 == points, "nhặt " + it.id + " ⇒ +" + (GameSession.Current.score - sc0) + " (wiki/video +" + points + ")");
    }

    static int FloorUnder(int x, int y)
    {
        // mặt sàn đặc gần nhất ≤ y (item nghỉ thấp hơn mặt sàn 1 px — NES)
        for (int yy = y; yy >= y - 4; yy--)
            if (W.Tiles != null && W.Tiles.RowLandable(yy, x - 1, x))
                return yy;
        return y - 1;
    }

    // ================================================================ enemy + bộ sinh

    static IEnumerable<int> Spawns()
    {
        var expect = new[]
        {
            new object[] { "TowerEntranceA", "SpearKnight_TEA", typeof(SpearKnight) }, new object[] { "TowerEntranceA", "Bat_TEA", typeof(Bat) },
            new object[] { "TowerEntranceB", "SpearKnight_TEB_gap", typeof(SpearKnight) }, new object[] { "TowerEntranceB", "SpearKnight_TEB_door", typeof(SpearKnight) },
            new object[] { "TowerEntranceB", "Bat_TEB", typeof(Bat) }, new object[] { "TowerHallA", "SpearKnight_HA", typeof(SpearKnight) },
            new object[] { "TowerHallB", "SpearKnight_HB_low", typeof(SpearKnight) }, new object[] { "TowerHallB", "SpearKnight_HB_up1", typeof(SpearKnight) },
            new object[] { "TowerHallB", "SpearKnight_HB_up2", typeof(SpearKnight) }, new object[] { "TowerHallC", "Bat_HC", typeof(Bat) },
            new object[] { "TowerHallC", "BoneTower_HC_low", typeof(BoneTower) }, new object[] { "TowerHallC", "BoneTower_HC_up", typeof(BoneTower) },
        };
        foreach (var e in expect)
        {
            string sec = (string)e[0], name = (string)e[1];
            var type = (Type)e[2];
            Prep();
            var safe = SAFE[sec];
            Go(sec, safe.x, safe.y, -1);
            var sp = Point(name);
            if (sp == null)
            {
                Fail(sec + " thiếu SpawnPoint " + name);
                continue;
            }
            Cam(sp.position.x - 128);
            yield return 0;
            var live = sp.Live;
            bool ok = live != null && live.GetType() == type;
            string extra = "";
            if (live is Bat)
            {
                var bat = (Bat)live;
                ok &= bat.Fsm.CurState == eBatState.Perch;
                extra = " " + bat.Fsm.CurState;
            }
            Check(ok, sec + " " + name + " ⇒ " + (live != null ? live.GetType().Name + " (" + live.X + "," + live.Y + ") hướng " + live.Facing : "không sinh")
                  + extra + " (điểm " + sp.position + ")");
        }
        // Medusa Head Hall A: trong vùng (x 720) có sinh, ngoài vùng (x 400) không
        foreach (var t in ZoneTest("TowerHallA", "MedusaHead_HA", 720, 80, -1, 80, true)) yield return t;
        foreach (var t in ZoneTest("TowerHallA", "MedusaHead_HA", 400, 80, -1, 200, false)) yield return t;
        foreach (var t in ZoneTest("TowerHallB", "MedusaHead_HB", 200, 80, -1, 80, true)) yield return t;
        foreach (var t in ZoneTest("TowerHallB", "MedusaHead_HB", 400, 80, -1, 200, false)) yield return t;
        foreach (var t in ZoneTest("Medusa", "MedusaHead_MD", 500, 128, -1, 80, true)) yield return t;
        foreach (var t in ZoneTest("Medusa", "MedusaHead_MD", 650, 128, -1, 200, false)) yield return t;
        // Ghost: tầng dưới x ≤ 256 sinh 1 con, hạ rồi không sinh lại; tầng trên x ≥ 576 sinh; bậc y 128 x 740 không sinh (khoá Y)
        foreach (var t in ZoneTest("TowerHallC", "Ghost_HC_low", 250, 144, -1, 10, true)) yield return t;
        {
            var sp = Spawner<EdgeSpawner>("Ghost_HC_low");
            var g = Find<Ghost>(x => true);
            if (g != null)
                g.Kill(eKillReason.Cleared);
            for (int i = 0; i < 240; i++)
                yield return 0;
            Check(sp != null && sp.SpawnCount == 1 && Count<Ghost>() == 0, "Ghost tầng dưới: hạ rồi 240 tick không sinh lại (SpawnCount " + (sp != null ? sp.SpawnCount : -1) + ")");
        }
        foreach (var t in ZoneTest("TowerHallC", "Ghost_HC_up", 600, 48, 1, 10, true)) yield return t;
        foreach (var t in ZoneTest("TowerHallC", "Ghost_HC_up", 740, 128, -1, 120, false)) yield return t;
    }

    static T Spawner<T>(string name) where T : EnemySpawner
    {
        foreach (var s in W.Spawners)
            if (s != null && s.name == name && s is T)
                return (T)s;
        return null;
    }

    static IEnumerable<int> ZoneTest(string sec, string spawner, int x, int y, int facing, int ticks, bool expectSpawn)
    {
        Prep();
        Go(sec, x, y, facing);
        var sp = Spawner<EdgeSpawner>(spawner);
        if (sp == null)
        {
            Fail(sec + " thiếu bộ sinh " + spawner);
            yield break;
        }
        int t = 0;
        while (t < ticks && sp.SpawnCount == 0)
        {
            t++;
            yield return 0;
        }
        Check((sp.SpawnCount > 0) == expectSpawn, sec + " " + spawner + " Simon (" + x + "," + y + ") ⇒ sinh " + sp.SpawnCount + " sau " + t + " tick (kỳ vọng "
              + (expectSpawn ? "có" : "không") + "; vùng x [" + sp.zoneMinX + ", " + sp.zoneMaxX + "] y [" + sp.zoneMinY + ", " + sp.zoneMaxY + "])");
    }

    // ================================================================ cơ chế + boss

    static IEnumerable<int> Mech()
    {
        var want = new[] { new object[] { "TowerEntranceB", 1, 0 }, new object[] { "TowerHallB", 2, 0 }, new object[] { "TowerHallC", 0, 3 } };
        foreach (var w in want)
        {
            string sec = (string)w[0];
            Prep();
            var safe = SAFE[sec];
            Go(sec, safe.x, safe.y, -1);
            int p0 = Count<MovingPlatform>(), c0 = Count<Crusher>();
            yield return 0;
            int p1 = Count<MovingPlatform>(), c1 = Count<Crusher>();
            Check(p0 == 0 && c0 == 0 && p1 == (int)w[1] && c1 == (int)w[2], sec + " MechSpawner: sau Warp " + p0 + "/" + c0 + ", tick đầu " + p1 + " bệ / " + c1 + " bẫy");
        }
    }

    static IEnumerable<int> Boss()
    {
        Prep();
        Go("Medusa", 650, 128, -1);
        yield return 0;
        var m = Find<QueenMedusa>(x => true);
        Check(m != null && m.X == 112 && m.Y == 144, "BossArena Medusa ⇒ " + (m != null ? "QueenMedusa (" + m.X + "," + m.Y + ")" : "không có"));
    }

    // ================================================================ ảnh

    static IEnumerable<int> Shots()
    {
        foreach (var s in shots)
        {
            string sec = s[0];
            int cx = int.Parse(s[1]);
            Prep();
            var safe = SAFE[sec];
            Go(sec, safe.x, safe.y, -1);
            Cam(cx);
            W.RefreshSpawnPoints();
            foreach (var x in Shot(s[2], cx, true)) yield return x;
        }
    }

    static IEnumerable<int> Shot(string name, int cx, bool pause)
    {
        if (shotDir.Length == 0)
            yield break;
        bool wasPaused = W.Paused;
        W.Paused = true;
        Cam(cx);
        HideSimon();
        W.RefreshSpawnPoints();   // + SyncViews
        yield return 0;
        yield return 0;
        string f = Path.Combine(shotDir, name + ".png");
        if (File.Exists(f))
            File.Delete(f);
        ScreenCapture.CaptureScreenshot(f);
        int t = 0;
        while ((!File.Exists(f) || new FileInfo(f).Length == 0) && t++ < 120)
            yield return 0;
        for (int i = 0; i < 3; i++)
            yield return 0;
        log.Append("SHOT ").Append(name).Append(" section ").Append(W.Section.name).Append(" cx ").Append(W.CameraCtl.X).Append('\n');
        ShowSimon();
        W.Paused = wasPaused;
    }

    static void HideSimon()
    {
        ShowSimon();
        foreach (var r in S.GetComponentsInChildren<Renderer>())
            if (!r.forceRenderingOff)
            {
                r.forceRenderingOff = true;
                hidden.Add(r);
            }
    }

    static void ShowSimon()
    {
        foreach (var r in hidden)
            if (r != null)
                r.forceRenderingOff = false;
        hidden.Clear();
    }
}
#endif
