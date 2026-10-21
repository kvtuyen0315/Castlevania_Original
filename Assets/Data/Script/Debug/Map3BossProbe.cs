#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// CVM3 09 — so Mummy Men + băng gạc + chết/quả cầu trong PHÒNG THẬT (Map3BossRoom) với 2 video (CHỈ Editor, scene Arena — Player = Simon,
/// đang Play; KHÔNG thuộc bộ hồi quy — đo). Khuôn Map2BossProbe (CVM2 09) + Map3EnemyProbe (Simon "RỐI" theo puppet từng tick
/// ref/data/boss9m/puppet_&lt;đoạn&gt;.csv — Tools/cvm3/boss9m_puppet.py). Mỗi đoạn bắt đầu ĐÚNG lúc camera khoá cx 1280 (Simon x 1409 ⇒
/// camera kẹp 1280 ngay tick đầu) với 2 Mummy đứng chờ ở 1503 / 1312; phần NGẪU NHIÊN ép theo video: độ cao băng (forcedHeights).
/// Ghi vết mỗi tick ⇒ &lt;outDir&gt;/&lt;đoạn&gt;.csv (rel, loại, id, x, y, hướng, state, đỏ, khung, Simon x/y, cx) + chụp Game view ở tick đặt
/// ⇒ &lt;shotDir&gt;/&lt;đoạn&gt;_rNNN.png. So: python -P -E Tools/cvm3/boss9m_compare.py &lt;outDir&gt; ; cặp ảnh: Tools/cvm3/boss9m_pairs.py.
///   Map3BossProbe.Run("m3_open,b3_open,m3_death", "&lt;outDir&gt;", "&lt;shotDir hoặc rỗng&gt;", "đoạn:rel,rel;…") ⇒ Done / Status
/// </summary>
public static class Map3BossProbe
{
    public const string DATA = "ClaudeNotes/cvm3/ref/data/boss9m/";

    private class Seg
    {
        public string name;
        public string heightsR = "", heightsL = "";
        public string plan = "";             // ép theo video (harnessPlan): "L|R:rel:act" — act = 1 / −1 (quay), T (ném), Sn (khựng n tick)
        public int[] defeats = new int[0];   // rel hạ con TRÁI (partner), rel hạ con PHẢI — m3_death
        public int mummyL = -1, mummyR = -1; // m3_death: đặt sẵn x 2 Mummy (đã thức, đứng yên)
    }

    private static readonly Seg[] SEGS =
    {
        // video Map_3 f5931 (khoá) → f6160: thức f5992, phải ném B0 CAO f6043, trái ném B1 CAO f6072.6 + B2 THẤP f6128.3
        // ép (mốc frame video ⇒ rel = (f − 5931) × 2.0033): trái quay trái f5995.5, quay phải f6026.5, đứng ném f6051, quay phải f6090.5,
        // đứng ném f6108.5, quay phải f6139; phải đứng ném f6023.5 (rồi đi trái tới khi bị lửa nước thánh f6101 — Unity không có lửa)
        new Seg { name = "m3_open", heightsR = "H", heightsL = "HL", plan = "L:129:-1,L:191:1,L:240:T,L:320:1,L:356:T,L:419:1,R:185:T" },
        // video Boss_Map_3 f374 (khoá) → f540: thức f434, phải ném B0 CAO ~f496, trái ném B1 CAO f514.5 (sang phải)
        // ép (rel = (f − 374) × 2.0033): trái đứng ném f494; phải quay phải f447 + đứng yên f447–455 (không rõ nguyên nhân — §6), đứng ném f477
        new Seg { name = "b3_open", heightsR = "H", heightsL = "HL", plan = "L:240:T,R:146:1,R:147:S14,R:206:T" },
        // video Map_3 f6680 → f6930: Mummy 1 chết (1360) f6686, Mummy 2 (1366) f6771 ⇒ quả cầu f6829.5 (1408,104) đứng → rơi f6847 → nhặt f6920
        new Seg { name = "m3_death", mummyL = 1360, mummyR = 1366, defeats = new[] { 12, 182 } },
    };

    public static bool Done { get; private set; } = true;
    public static string Status { get; private set; } = "";
    private static readonly Queue<Seg> queue = new Queue<Seg>();
    private static readonly Dictionary<string, List<int>> photos = new Dictionary<string, List<int>>();
    private static readonly List<int[]> puppet = new List<int[]>();
    private static readonly List<GameObject> temp = new List<GameObject>();
    private static readonly Dictionary<long, int> ids = new Dictionary<long, int>();
    private static readonly StringBuilder trace = new StringBuilder();
    private static StageWorld world;
    private static Seg cur;
    private static string outDir, shotDir;
    private static int startTick, nextId, savedStage = -1;
    private static bool savedCheat;
    private static MummyMan right, left;

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string Run(string names, string outDirectory, string shotDirectory = "", string photoSpec = "")
    {
        world = StageWorld.Current;
        if (world == null || Simon == null)
            return "cần Play scene Arena (Player = Simon)";
        if (!Done)
            return "đang chạy";
        outDir = outDirectory;
        shotDir = shotDirectory;
        Directory.CreateDirectory(outDir);
        if (!string.IsNullOrEmpty(shotDir))
            Directory.CreateDirectory(shotDir);
        photos.Clear();
        foreach (var part in (photoSpec ?? "").Split(';'))
        {
            var kv = part.Split(':');
            if (kv.Length != 2) continue;
            var l = new List<int>();
            foreach (var t in kv[1].Split(','))
                if (t.Trim().Length > 0) l.Add(int.Parse(t.Trim()));
            photos[kv[0].Trim()] = l;
        }
        queue.Clear();
        foreach (var n in names.Split(','))
            foreach (var s in SEGS)
                if (s.name == n.Trim())
                    queue.Enqueue(s);
        savedStage = world.Stage;
        savedCheat = CvgpDebug.CheatInvincible;
        Status = "";
        Done = false;
        Next();
        return "bắt đầu " + (queue.Count + 1) + " đoạn";
    }

    static void LoadPuppet(string name)
    {
        puppet.Clear();
        string p = Path.Combine(Path.GetDirectoryName(Application.dataPath), DATA + "puppet_" + name + ".csv");
        if (!File.Exists(p))
            return;
        var lines = File.ReadAllLines(p);
        for (int i = 1; i < lines.Length; i++)
        {
            var f = lines[i].Split(',');
            if (f.Length < 4) continue;
            puppet.Add(new[] { int.Parse(f[1]), int.Parse(f[2]), int.Parse(f[3]) });
        }
    }

    private static void Next()
    {
        Unhook();
        Cleanup();
        if (queue.Count == 0)
        {
            Finish();
            return;
        }
        cur = queue.Dequeue();
        LoadPuppet(cur.name);
        if (puppet.Count == 0)
        {
            Status += cur.name + ": thiếu puppet\n";
            Next();
            return;
        }
        var p0 = puppet[0];
        GameObject host;
        var arena = Map3BossRoom.Enter(world, p0[0], p0[1], p0[2], true, false, out host);
        if (host != null)
            temp.Add(host);
        Simon.StartAutoWalk(p0[0], 0, true);
        Simon.Facing = p0[2];
        CvgpDebug.CheatInvincible = true;
        world.ResetRng();
        world.FreezeEnemies(0);
        GameSession.Current.ResetForNewGame();
        GameSession.Current.stage = 9;
        right = arena != null ? arena.Boss as MummyMan : null;
        left = right != null ? right.partner : null;
        if (right != null)
        {
            right.forcedHeights = cur.heightsR;
            right.harnessPlan = cur.plan.Length > 0;
            if (left != null)
            {
                left.forcedHeights = cur.heightsL;
                left.harnessPlan = cur.plan.Length > 0;
            }
            if (cur.mummyR >= 0)
            {
                if (world.CameraCtl != null)
                    world.CameraCtl.Lock(1280);          // giữa trận: camera đã khoá cx 1280 (Simon x 1302 < 1408)
                right.lockTicks = right.wakeDelay - 1;   // thức ở tick đầu
                right.fx = Fixed.FromPx(cur.mummyR);
                if (left != null)
                    left.fx = Fixed.FromPx(cur.mummyL);
            }
        }
        trace.Length = 0;
        trace.Append("rel,kind,id,x,y,facing,state,red,frame,sx,sy,cx\n");
        ids.Clear();
        nextId = 0;
        startTick = world.TickCount;
        world.TickBegan += OnTickBegan;
        world.TickEnded += OnTickEnded;
    }

    private static void OnTickBegan(StageWorld w)
    {
        int rel = w.TickCount - startTick;
        var s = Simon;
        if (s == null)
            return;
        var p = puppet[Mathf.Min(rel, puppet.Count - 1)];
        s.fx = Fixed.FromPx(p[0]);
        s.fy = Fixed.FromPx(p[1]);
        s.autoWalkTarget = p[0];
        s.Facing = p[2];
        foreach (var part in cur.plan.Split(','))
        {
            var a = part.Split(':');
            if (a.Length != 3 || int.Parse(a[1]) != rel) continue;
            var m = a[0] == "L" ? left : right;
            if (m == null || !m.Alive) continue;
            if (a[2] == "T") m.ForceThrow();
            else if (a[2].StartsWith("S")) m.stunTicks = Mathf.Max(m.stunTicks, int.Parse(a[2].Substring(1)));
            else m.ForceTurn(int.Parse(a[2]));
        }
        if (cur.mummyR >= 0)
        {
            // m3_death: 2 Mummy đứng yên (khựng) tới lúc hạ
            if (right != null && right.Alive && !right.dying) right.stunTicks = Mathf.Max(right.stunTicks, 2);
            if (left != null && left.Alive && !left.defeated) left.stunTicks = Mathf.Max(left.stunTicks, 2);
            if (cur.defeats.Length > 0 && rel == cur.defeats[0] && left != null && left.Alive) left.Defeat();
            if (cur.defeats.Length > 1 && rel == cur.defeats[1] && right != null && right.Alive) right.Defeat();
        }
    }

    private static void OnTickEnded(StageWorld w)
    {
        int rel = w.TickCount - startTick - 1;
        var s = Simon;
        int cx = w.CameraCtl != null ? w.CameraCtl.X : 0;
        for (int g = 0; g < 4; g++)
        {
            var grp = g == 0 ? eEntityGroup.Enemy : g == 1 ? eEntityGroup.EnemyShot : g == 2 ? eEntityGroup.Item : eEntityGroup.Effect;
            foreach (var e in w.GroupList(grp))
            {
                if (!e.Alive) continue;
                int id;
                long key = ((long)e.GetInstanceID() << 32) ^ (uint)e.SpawnTick;
                if (!ids.TryGetValue(key, out id)) { id = nextId++; ids[key] = id; }
                var m = e as MummyMan;
                string kind = m != null ? (m.second ? "MummyL" : "MummyR") : e.GetType().Name;
                string state = e is EnemyBase ? ((EnemyBase)e).DebugState.Split(' ')[0] : "-";
                string anim = e.Anim != null && e.Anim.Current != null ? e.Anim.Current.name : "";
                int frame = e.Anim != null ? e.Anim.FrameIndex : 0;
                trace.Append(rel).Append(',').Append(kind).Append(',').Append(id).Append(',').Append(e.X).Append(',').Append(e.Y)
                     .Append(',').Append(e.Facing).Append(',').Append(state).Append(',').Append(m != null && m.flashRed ? 1 : 0)
                     .Append(',').Append(anim).Append(':').Append(frame).Append(',').Append(s.X).Append(',').Append(s.Y).Append(',').Append(cx).Append('\n');
            }
        }
        List<int> ph;
        if (!string.IsNullOrEmpty(shotDir) && photos.TryGetValue(cur.name, out ph) && ph.Contains(rel))
            ScreenCapture.CaptureScreenshot(Path.Combine(shotDir, cur.name + "_r" + rel.ToString("000") + ".png"));
        if (rel + 1 >= puppet.Count)
        {
            File.WriteAllText(Path.Combine(outDir, cur.name + ".csv"), trace.ToString(), new UTF8Encoding(false));
            Status += cur.name + ": " + puppet.Count + " tick, điểm " + GameSession.Current.score + "\n";
            Next();
        }
    }

    private static void Unhook()
    {
        if (world != null)
        {
            world.TickBegan -= OnTickBegan;
            world.TickEnded -= OnTickEnded;
        }
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
        if (world != null)
        {
            world.ClearNonPlayer();
            if (world.Tiles != null)
            {
                world.Tiles.WallMinX = int.MinValue;
                world.Tiles.WallMaxX = int.MaxValue;
                world.Tiles.PlayerWallMaxX = int.MaxValue;
            }
        }
    }

    private static void Finish()
    {
        Unhook();
        Cleanup();
        Cvm3TerrainProbe.Cleanup();
        if (savedStage >= 0)
            world.Stage = savedStage;
        CvgpDebug.CheatInvincible = savedCheat;
        world.ResetRng();
        GameSession.Current.ResetForNewGame();
        Done = true;
    }
}
#endif
