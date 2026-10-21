#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness CVM3 10 — lớp vật thể Block 3 (CHỈ Editor, scene Arena, Player = Simon). Qua MCP khi Play:
///   Map3ObjectsProbe.Start("candles,blocks,secrets,spawns,zones,boss", shotDir, "&lt;Section:cx:tên;…&gt;") ⇒ chạy theo NesClock, TỰ CHẤM ⇒ Report()
///     candles : mỗi nến của Map3Objects.json: khung hiện (x màn 7 chưa / 8 sinh), vị trí (x, Y), phá ⇒ item = content (video) qua ConfigDrop;
///               nến WEB_C08 ⇒ rìu rơi qua khe xuống đáy section và MẤT (video f2292–2308, wiki "Axe rơi mất nếu không nhặt ngay")
///     blocks  : khối phá WEA (18,8) + MummyMan (83,9) ⇒ thịt nướng
///     secrets : St7 ngồi mép phải bục thấp nhất ⇒ túi trồi bục cao trái (tick ngồi → hàng đầu hiện / nghỉ, vị trí, tuổi thọ từ lúc nghỉ, nhặt
///               +1000, ngồi sát mép ⇒ túi ngoài màn vẫn còn, ngồi ngoài vùng / đứng ⇒ không); St8 ngồi trên 2 khối lơ lửng ⇒ túi mút trái
///               cầu (ngoài màn vẫn còn ⇒ đi lùi nhặt được); St9 (chỉ wiki) đi trái trên khối (82,8) 120 tick ⇒ túi, khoá camera ⇒ không
///     spawns  : mọi SpawnPoint enemy sinh đúng loại/chỗ (Raven Perch, Fleaman Wait)
///     zones   : Ghost WEA (sau lưng, chân − 31, 1 con), Medusa Head WalkwayA / MummyMan (trong vùng có, ngoài không)
///     boss    : BossArena MummyMen ⇒ 2 Mummy chờ ở 1503 / 1312
///   shotDir != "" ⇒ chụp Game view (ẩn Simon) ở các khung "Section:cx:tên;…" (cx Unity) — so video bằng Tools/cvm3/pairs10.py.
/// Section Block 3 dựng lúc chạy (Cvm3TerrainProbe.Setup) + instance &lt;Section&gt;_Objects.prefab làm con — KHÔNG đổi Arena.unity; gỡ cuối bộ
/// (Cvm3TerrainProbe.Cleanup — trả camera bám Simon, Stage cũ). Không thuộc bộ hồi quy (kết quả ClaudeNotes/cvm3/_cache/10/).
/// </summary>
public static class Map3ObjectsProbe
{
    public const string TABLE = "Assets/Data/Config/Level/Block3/Map3Objects.json";
    public const string OBJ_DIR = "Assets/Data/Prefabs/Level/Block3/Objects/";

    static readonly Dictionary<string, int> STAGE = new Dictionary<string, int>
    {
        { "WalkwayEntranceA", 7 }, { "WalkwayEntranceB", 7 }, { "WalkwayA", 8 }, { "WalkwayB", 8 }, { "MummyMan", 9 },
    };
    // nội dung khối phá — video:Map_3 (MAP3_PLACEMENT §2)
    static readonly string[,] VIDEO_BLOCKS = { { "WalkwayEntranceA", "18", "8", "PotRoast" }, { "MummyMan", "83", "9", "PotRoast" } };
    // chỗ Simon đứng an toàn khi kiểm (ngoài vùng bộ sinh / bí mật, trên sàn đặc)
    static readonly Dictionary<string, Vector2Int> SAFE = new Dictionary<string, Vector2Int>
    {
        { "WalkwayEntranceA", new Vector2Int(720, 160) }, { "WalkwayEntranceB", new Vector2Int(240, 160) }, { "WalkwayA", new Vector2Int(48, 128) },
        { "WalkwayB", new Vector2Int(330, 128) }, { "MummyMan", new Vector2Int(48, 96) },
    };
    // video:Map_3 (MAP3_PLACEMENT §3): túi bí mật
    const int VIDEO_BAG7_X = 424, VIDEO_BAG8_X = 128, BAG_Y = 129, WIKI_BAG9_X = 1264, WIKI_BAG9_Y = 97;
    const int VIDEO_FIRST_ROW_TICKS = 6;   // khung ngồi đầu → hàng túi đầu tiên hiện (St7 f1958 → f1961, St8 f3411 → ≈ f3413) ±2
    const int VIDEO_REST_TICKS = 59;       // khung ngồi đầu → túi nghỉ (St7 f1957.5 → f1987; St8 f3410.5 → f3440) ±2
    const int LIFETIME = 188;              // ConfigItem lifetimeTicks — tính từ lúc nghỉ (lifetimeFromRest)
    const int WIKI_WALK_TICKS = 120;       // St9 "vài giây" (suy luận theo rương Stage 5)
    // chỗ vật nến nghỉ đo video (dò sàn cột tâm — ConfigRule.ItemFootCenterFromBlock): tim lớn WEB_C01 f1334 (160,161), túi WEB_C09 f2357 (671,161)
    static readonly Dictionary<string, int> VIDEO_REST_Y = new Dictionary<string, int> { { "WEB_C01", 160 }, { "WEB_C09", 160 } };

    [Serializable] class Candle { public string id, content; public int x, y; }
    [Serializable] class SPoint { public string name, prefab; public int x, y, facing; }
    [Serializable] class Section { public string id; public Candle[] candles; public SPoint[] spawnPoints; }
    [Serializable] class Table { public Section[] sections; }

    public static bool Running { get; private set; }
    public static string Status { get; private set; } = "";
    static readonly StringBuilder log = new StringBuilder();
    static int pass, fail, savedStage;
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
        string r = Cvm3TerrainProbe.Setup();
        int attached = AttachObjects();
        savedStage = W.Stage;
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
        log.Append("# Map3ObjectsProbe ").Append(what).Append(" — ").Append(r).Append("; Objects gắn ").Append(attached).Append('\n');
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
        Cvm3TerrainProbe.Cleanup();   // Warp Arena + gỡ section Block 3 + camera bám Simon
        W.Stage = savedStage;
        CvgpDebug.CheatInvincible = false;
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
        if (set.Contains("zones")) foreach (var t in Zones()) yield return t;
        if (set.Contains("boss")) foreach (var t in Boss()) yield return t;
        foreach (var t in Shots()) yield return t;
    }

    // ================================================================ dựng

    static int AttachObjects()
    {
        int n = 0;
        foreach (var id in Cvm3TerrainProbe.SECTION_IDS)
        {
            var s = Cvm3TerrainProbe.Find(id);
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
        ses.whipLevel = 3;                       // video Map_3: roi cấp 3 suốt Block 3 ⇒ nến tim nhỏ không đổi thành roi
        ses.subWeapon = eSubWeapon.HolyWater;    // video Map_3 cầm nước thánh từ f491; bộ đếm trúng = 0 ⇒ tim nhỏ không đổi thành II
        ses.hearts = 20;
        ses.NotifyChanged();
        CvgpDebug.CheatInvincible = true;        // enemy không làm Simon lùi/rơi hố khi đang kiểm vật thể
    }

    static void Go(string section, int x, int y, int facing)
    {
        var s = Cvm3TerrainProbe.Find(section);
        NesPad.I.StopReplay();
        W.Paused = false;
        W.Warp(s, x, y);
        W.Stage = STAGE[section];
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

    static Table Load() => JsonUtility.FromJson<Table>(File.ReadAllText(TABLE, Encoding.UTF8));

    // ================================================================ nến

    static IEnumerable<int> Candles()
    {
        foreach (var sec in Load().sections)
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
                Cam(Mathf.Clamp(c.x - 128, W.CameraCtl.MinX, W.CameraCtl.MaxX));
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
                var it = cp.spawnedItem;
                var got = it != null ? it.id : cp.dropped;
                Check(posOk && got == want, "nến " + sec.id + " " + c.id + " (" + cp.X + "," + cp.Y + ") id " + cp.objectId + " ⇒ " + got
                      + (got == want ? "" : " ≠ " + want) + " sau " + t + " tick");
                if (c.id == "WEB_C08" && it != null)
                {
                    // rìu rơi qua khe x 592–623 (không có sàn tới đáy) ⇒ ra khỏi đáy section ⇒ mất, Simon không chạm
                    int k = 0, lastY = it.Y;
                    while (it.Alive && k++ < 200)
                    {
                        lastY = it.Y;
                        yield return 0;
                    }
                    Check(!it.Alive && !it.picked && it.KillReason == eKillReason.Offscreen && lastY > W.Tiles.Height,
                          "WEB_C08 rìu rơi xuống hố ⇒ " + (it.Alive ? "CÒN" : "mất (" + it.KillReason + ")") + " sau " + k + " tick, Y cuối " + lastY
                          + " (đáy section " + W.Tiles.Height + "; video f2292–2308 ≈ 32 tick)");
                }
                else if (it != null && VIDEO_REST_Y.ContainsKey(c.id))
                {
                    int k = 0;
                    while (it.Alive && it.motion != ItemPickup.eMotion.Rest && k++ < 120)
                        yield return 0;
                    Check(it.Alive && it.motion == ItemPickup.eMotion.Rest && it.Y == VIDEO_REST_Y[c.id], c.id + " " + it.id + " rơi tới ("
                          + it.X + "," + it.Y + ") sau " + k + " tick (video Y " + VIDEO_REST_Y[c.id] + " — dò sàn cột tâm; cả thân 16 px sẽ đỡ sớm hơn)");
                    if (it.Alive)
                        it.Kill(eKillReason.Cleared);
                }
                else if (it != null && it.Alive)
                    it.Kill(eKillReason.Cleared);
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
            Cam(Mathf.Clamp(col * 16 - 120, W.CameraCtl.MinX, W.CameraCtl.MaxX));
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
            var it = b.spawnedItem;
            if (it != null)
            {
                int t = 0;
                while (it.Alive && it.motion != ItemPickup.eMotion.Rest && t++ < 120)
                    yield return 0;
                Check(it.Alive && it.motion == ItemPickup.eMotion.Rest, "thịt nướng " + oid + " nghỉ ở (" + it.X + "," + it.Y + ") sau " + t + " tick"
                      + (sec == "WalkwayEntranceA" ? " (video f803: x 296 đáy đo 177 ⇒ Y 176)" : ""));
                if (it.Alive)
                    it.Kill(eKillReason.Cleared);
            }
        }
    }

    // ================================================================ bí mật

    static SecretTrigger SecretOf(string name)
    {
        var sp = Point(name);
        return sp != null ? sp.Live as SecretTrigger : null;
    }

    /// <summary>Ngồi (giữ ↓) tại (x, y) — đo tick: Simon ngồi đầu, bí mật kích hoạt, hàng túi đầu tiên hiện (còn ≤ 13 px dưới chỗ nghỉ), nghỉ.</summary>
    static IEnumerable<int> CrouchTrial(string sec, string secret, int x, int y, int facing, int[] res, ItemPickup[] bagOut)
    {
        Prep();
        Go(sec, x, y, facing);
        yield return 0;
        var tr = SecretOf(secret);
        res[0] = res[1] = res[2] = res[3] = -1;
        bagOut[0] = null;
        if (tr == null)
        {
            Fail(sec + " không có " + secret);
            yield break;
        }
        NesPad.I.PlayReplay("D*90 -*10");
        for (int k = 0; k < 100; k++)
        {
            if (res[0] < 0 && S.Fsm.Is(eSimonState.Crouch)) res[0] = k;
            if (res[1] < 0 && tr.fired) { res[1] = k; bagOut[0] = tr.spawnedItem; }
            var b = bagOut[0];
            if (b != null && b.Alive)
            {
                int d = b.motion == ItemPickup.eMotion.Rise ? Fixed.ToPx(b.riseLeft) : 0;
                if (res[2] < 0 && d <= 13) res[2] = k;
                if (res[3] < 0 && b.motion == ItemPickup.eMotion.Rest) res[3] = k;
            }
            yield return 0;
        }
    }

    static IEnumerable<int> Secrets()
    {
        var r = new int[4];
        var bagRef = new ItemPickup[1];

        // ---- St7: ngồi mép phải bục thấp nhất (x 537 = video) ⇒ túi trồi trên bục cao trái (424, 129)
        foreach (var t in CrouchTrial("WalkwayEntranceB", "Secret_St7Bag", 537, 176, 1, r, bagRef)) yield return t;
        var bag = bagRef[0];
        Check(bag != null && bag.id == eItemId.FlashingMoneyBag && bag.X == VIDEO_BAG7_X && bag.Y == BAG_Y,
              "St7 ngồi x 537 ⇒ túi " + (bag != null ? bag.id + " (" + bag.X + "," + bag.Y + ")" : "KHÔNG") + " (video (424, 129))");
        Check(r[1] >= 0 && Math.Abs((r[2] - r[0]) - VIDEO_FIRST_ROW_TICKS) <= 2 && Math.Abs((r[3] - r[0]) - VIDEO_REST_TICKS) <= 2,
              "St7 nhịp: ngồi tick " + r[0] + ", kích hoạt +" + (r[1] - r[0]) + ", hàng đầu hiện +" + (r[2] - r[0]) + " (video " + VIDEO_FIRST_ROW_TICKS
              + " ±2), nghỉ +" + (r[3] - r[0]) + " (video " + VIDEO_REST_TICKS + " ±2)");
        if (bag != null && bag.Alive && shotDir.Length > 0)
            foreach (var x in Shot("bag7_rest", 380, false)) yield return x;
        // không nhặt: tuổi thọ tính từ lúc nghỉ (188) — còn ở ≥ 197 tick sau khi sinh (video nhặt f2054)
        if (bag != null)
        {
            int k = 0;
            S.Teleport(500, 176, 1);   // tránh xa túi, đứng (không ngồi) trên bục thấp nhất x 480–543 ngoài vùng
            while (bag.Alive && k++ < 400)
                yield return 0;
            int life = bag.age - bag.riseTicks;
            Check(!bag.Alive && !bag.picked && Math.Abs(life - LIFETIME) <= 1 && bag.age >= 197,
                  "St7 túi không nhặt ⇒ biến mất ở tuổi " + bag.age + " (trồi " + bag.riseTicks + " + nghỉ " + life + "; tuổi thọ " + LIFETIME
                  + " từ lúc nghỉ; video còn ở ≥ 197 tick sau khi sinh)");
        }
        // nhặt ⇒ +1000
        foreach (var t in CrouchTrial("WalkwayEntranceB", "Secret_St7Bag", 537, 176, 1, r, bagRef)) yield return t;
        bag = bagRef[0];
        if (bag != null)
        {
            int sc0 = GameSession.Current.score;
            S.Teleport(bag.X, 128, -1);
            int k = 0;
            while (bag.Alive && k++ < 10)
                yield return 0;
            Check(bag.picked && GameSession.Current.score - sc0 == 1000, "St7 nhặt túi ⇒ +" + (GameSession.Current.score - sc0) + " (video +1000 f2054)");
        }
        // ngồi sát mép phải (x 549) ⇒ túi hiện NGOÀI màn trái (cx 421 ⇒ x màn 3) nhưng vẫn còn (keepOffscreen)
        foreach (var t in CrouchTrial("WalkwayEntranceB", "Secret_St7Bag", 549, 176, 1, r, bagRef)) yield return t;
        bag = bagRef[0];
        Check(bag != null && bag.Alive && bag.X - W.CameraCtl.X < 8, "St7 ngồi x 549 ⇒ túi " + (bag != null && bag.Alive ? "còn" : "KHÔNG/mất")
              + " ở x màn " + (bag != null ? bag.X - W.CameraCtl.X : -999) + " (ngoài khung hiện 8 — wiki 'chạy tới nhặt')");
        // ngồi ngoài vùng (x 520, cùng bục) / đứng yên ở 537 ⇒ không
        foreach (var t in CrouchTrial("WalkwayEntranceB", "Secret_St7Bag", 520, 176, 1, r, bagRef)) yield return t;
        Check(r[1] < 0, "St7 ngồi x 520 (ngoài vùng 528–549) 90 tick ⇒ " + (r[1] < 0 ? "không có túi" : "CÓ túi"));
        Prep();
        Go("WalkwayEntranceB", 537, 176, 1);
        yield return 0;
        var t7 = SecretOf("Secret_St7Bag");
        for (int i = 0; i < 150; i++)
            yield return 0;
        Check(t7 != null && !t7.fired, "St7 đứng yên x 537 150 tick (không ngồi) ⇒ " + (t7 != null && !t7.fired ? "không có túi" : "CÓ túi"));

        // ---- St8: ngồi trên 2 khối lơ lửng (x 246 = video) ⇒ túi mút trái cầu (128, 129)
        foreach (var t in CrouchTrial("WalkwayB", "Secret_St8Bag", 246, 128, 1, r, bagRef)) yield return t;
        bag = bagRef[0];
        Check(bag != null && bag.id == eItemId.FlashingMoneyBag && bag.X == VIDEO_BAG8_X && bag.Y == BAG_Y,
              "St8 ngồi x 246 ⇒ túi " + (bag != null ? bag.id + " (" + bag.X + "," + bag.Y + ")" : "KHÔNG") + " (video (128, 129) = sheet 16)");
        Check(r[1] >= 0 && Math.Abs((r[2] - r[0]) - VIDEO_FIRST_ROW_TICKS) <= 3 && Math.Abs((r[3] - r[0]) - VIDEO_REST_TICKS) <= 2,
              "St8 nhịp: ngồi tick " + r[0] + ", kích hoạt +" + (r[1] - r[0]) + ", hàng đầu hiện +" + (r[2] - r[0]) + " (video ≈ " + VIDEO_FIRST_ROW_TICKS
              + "), nghỉ +" + (r[3] - r[0]) + " (video f3410.5 → f3440 ≈ " + VIDEO_REST_TICKS + ")");
        if (bag != null && bag.Alive && shotDir.Length > 0)
            foreach (var x in Shot("bag8_rest", 77, false)) yield return x;
        // ngồi ở đầu phải vùng (x 271) ⇒ túi ngoài màn trái (x màn −15) vẫn còn ⇒ đi lùi trái nhặt được
        foreach (var t in CrouchTrial("WalkwayB", "Secret_St8Bag", 271, 128, 1, r, bagRef)) yield return t;
        bag = bagRef[0];
        if (bag != null)
        {
            int sx = bag.X - W.CameraCtl.X;
            int sc0 = GameSession.Current.score;
            W.CameraCtl.Target = W.Player;
            NesPad.I.PlayReplay("L*200");
            int k = 0;
            while (bag.Alive && k++ < 200)
                yield return 0;
            Check(bag.picked && GameSession.Current.score - sc0 == 1000, "St8 ngồi x 271 ⇒ túi ở x màn " + sx + " (ngoài màn) vẫn còn; đi trái "
                  + k + " tick nhặt ⇒ +" + (GameSession.Current.score - sc0) + " (wiki 'có thể hiện ngoài màn — đi lùi lại')");
            NesPad.I.StopReplay();
        }
        else
            Fail("St8 ngồi x 271 ⇒ không có túi");
        foreach (var t in CrouchTrial("WalkwayB", "Secret_St8Bag", 300, 128, 1, r, bagRef)) yield return t;
        Check(r[1] < 0, "St8 ngồi x 300 (ngoài 2 khối) 90 tick ⇒ " + (r[1] < 0 ? "không có túi" : "CÓ túi"));

        // ---- St9 (CHỈ WIKI): đứng trên khối (82,8) đi trái 120 tick (đẩy tường) trước khi khoá ⇒ túi (1264, 97)
        Prep();
        Go("MummyMan", 1320, 128, -1);
        yield return 0;
        var t9 = SecretOf("Secret_St9Bag");
        NesPad.I.PlayReplay("L*140");
        int fireAt = -1, minX = S.X;
        for (int k = 0; k < 140; k++)
        {
            if (fireAt < 0 && t9 != null && t9.fired) fireAt = k;
            minX = Mathf.Min(minX, S.X);
            yield return 0;
        }
        bag = t9 != null ? t9.spawnedItem : null;
        Check(bag != null && Math.Abs(fireAt - WIKI_WALK_TICKS) <= 2 && minX == 1320 && !W.CameraCtl.Locked,
              "St9 đi trái trên khối (82,8) ⇒ " + (bag != null ? "túi sau " + fireAt + " tick" : "KHÔNG có túi") + ", Simon kẹt tường x " + minX
              + " (wiki 'vài giây' — 120 suy luận)");
        if (bag != null)
        {
            int k = 0;
            while (bag.Alive && bag.motion == ItemPickup.eMotion.Rise && k++ < 100)
                yield return 0;
            Check(bag.X == WIKI_BAG9_X && bag.Y == WIKI_BAG9_Y, "St9 túi nghỉ ở (" + bag.X + "," + bag.Y + ") (SUY LUẬN (1264, 97) — wiki không nói chỗ)");
        }
        NesPad.I.StopReplay();
        // camera đã khoá (Simon vào phòng x ≥ 1408) ⇒ không
        Prep();
        Go("MummyMan", 1420, 176, 1);
        for (int i = 0; i < 4; i++)
            yield return 0;
        bool locked = W.CameraCtl.Locked;
        S.Teleport(1320, 128, -1);
        t9 = SecretOf("Secret_St9Bag");
        NesPad.I.PlayReplay("L*160");
        for (int i = 0; i < 160; i++)
            yield return 0;
        Check(locked && t9 != null && !t9.fired, "St9 sau khi khoá camera (cx " + W.CameraCtl.X + ", khoá " + locked + ") đi trái 160 tick ⇒ "
              + (t9 != null && !t9.fired ? "không có túi" : "CÓ túi/không có trigger") + " (wiki 'before the screen gets locked')");
        NesPad.I.StopReplay();
        // đứng yên trên khối ⇒ không
        Prep();
        Go("MummyMan", 1320, 128, -1);
        yield return 0;
        t9 = SecretOf("Secret_St9Bag");
        for (int i = 0; i < 160; i++)
            yield return 0;
        Check(t9 != null && !t9.fired, "St9 đứng yên trên khối 160 tick ⇒ " + (t9 != null && !t9.fired ? "không có túi" : "CÓ túi"));
    }

    // ================================================================ enemy SpawnPoint

    static IEnumerable<int> Spawns()
    {
        foreach (var sec in Load().sections)
        {
            if (sec.spawnPoints == null)
                continue;
            foreach (var p in sec.spawnPoints)
            {
                Prep();
                var safe = SAFE[sec.id];
                Go(sec.id, safe.x, safe.y, -1);
                var sp = Point(p.name);
                if (sp == null)
                {
                    Fail(sec.id + " thiếu SpawnPoint " + p.name);
                    continue;
                }
                W.Paused = true;   // chỉ kiểm điểm sinh: không cho enemy hành động
                Cam(Mathf.Clamp(sp.position.x - 128, W.CameraCtl.MinX, W.CameraCtl.MaxX));
                W.RefreshSpawnPoints();
                yield return 0;
                var live = sp.Live;
                string want = Path.GetFileNameWithoutExtension(p.prefab);
                bool ok = live != null && live.GetType().Name == want && live.X == p.x && live.Y == p.y;
                string extra = "";
                if (live is Raven)
                {
                    var rv = (Raven)live;
                    ok &= rv.Fsm.CurState == eRavenState.Perch;
                    extra = " " + rv.Fsm.CurState;
                }
                if (live is Fleaman)
                {
                    var fl = (Fleaman)live;
                    ok &= fl.Fsm.CurState == eFleamanState.Wait;
                    extra = " " + fl.Fsm.CurState;
                }
                Check(ok, sec.id + " " + p.name + " ⇒ " + (live != null ? live.GetType().Name + " (" + live.X + "," + live.Y + ") hướng " + live.Facing : "không sinh")
                      + extra + " (điểm " + sp.position + ")");
                W.Paused = false;
            }
        }
    }

    // ================================================================ bộ sinh theo vùng

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
              + (expectSpawn ? "có" : "không") + "; vùng x [" + sp.zoneMinX + ", " + sp.zoneMaxX + "])");
    }

    static IEnumerable<int> Zones()
    {
        // Ghost WEA: Simon đi trái trong vùng ⇒ Ghost sinh 96 px SAU LƯNG, tâm = chân − 31 (video f416 (671,145) khi Simon 575/176)
        foreach (var t in ZoneTest("WalkwayEntranceA", "Ghost_WEA", 575, 176, -1, 10, true)) yield return t;
        {
            var g = Find<Ghost>(x => true);
            Check(g != null && g.X == S.X + 96 && g.Y == 145, "Ghost WEA hiện ở " + (g != null ? "(" + g.X + "," + g.Y + ")" : "-") + " — Simon ("
                  + S.X + "," + S.Y + ") (video (671,145) với Simon 575: +96, chân − 31)");
            var sp = Spawner<EdgeSpawner>("Ghost_WEA");
            if (g != null)
                g.Kill(eKillReason.Cleared);
            for (int i = 0; i < 240; i++)
                yield return 0;
            Check(sp != null && sp.SpawnCount == 1 && Count<Ghost>() == 0, "Ghost WEA: hạ rồi 240 tick không sinh lại (SpawnCount " + (sp != null ? sp.SpawnCount : -1) + ")");
        }
        foreach (var t in ZoneTest("WalkwayEntranceA", "Ghost_WEA", 650, 176, -1, 120, false)) yield return t;
        // Medusa Head WalkwayA [144, …] / MummyMan [976, 1259]
        foreach (var t in ZoneTest("WalkwayA", "MedusaHead_WA", 300, 128, 1, 80, true)) yield return t;
        foreach (var t in ZoneTest("WalkwayA", "MedusaHead_WA", 100, 128, 1, 200, false)) yield return t;
        foreach (var t in ZoneTest("MummyMan", "MedusaHead_MM", 1100, 96, 1, 80, true)) yield return t;
        foreach (var t in ZoneTest("MummyMan", "MedusaHead_MM", 940, 96, 1, 200, false)) yield return t;
        foreach (var t in ZoneTest("MummyMan", "MedusaHead_MM", 1270, 96, 1, 200, false)) yield return t;
    }

    // ================================================================ boss

    static IEnumerable<int> Boss()
    {
        Prep();
        Go("MummyMan", 1100, 96, 1);
        yield return 0;
        yield return 0;
        var xs = new List<int>();
        for (int g = 0; g < 6; g++)
        {
            List<Entity> list;
            try { list = W.GroupList(g); } catch { break; }
            if (list == null) continue;
            foreach (var e in list)
                if (e is MummyMan && e.Alive)
                    xs.Add(e.X * 1000 + e.Y);
        }
        xs.Sort();
        Check(xs.Count == 2 && xs[0] == 1312 * 1000 + 176 && xs[1] == 1503 * 1000 + 176,
              "BossArena MummyMen ⇒ " + xs.Count + " Mummy " + string.Join(" ", xs.ConvertAll(v => "(" + v / 1000 + "," + v % 1000 + ")").ToArray()) + " (video 1312 / 1503 chờ)");
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
