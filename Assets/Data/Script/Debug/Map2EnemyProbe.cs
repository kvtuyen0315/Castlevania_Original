#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// CVM2 08 — so enemy Block 2 trong Unity với video Map_2 (CHỈ Editor, scene Arena — Player = Simon, đang Play; không thuộc bộ hồi quy).
/// Mỗi đoạn: dựng section Block 2 (Cvm2TerrainProbe.Setup), Simon "RỐI" theo đường đi video (ClaudeNotes/cvm2/ref/data/enemy8/
/// puppet_&lt;đoạn&gt;.csv — Simon ở AutoWalk noClip đã tới đích, mỗi tick ở TickBegan đặt fx/fy/hướng = video ⇒ không tự di chuyển,
/// không rơi; cheat bất tử), sinh enemy ĐÚNG chỗ/lúc như video (bộ sinh thật với độ trễ con đầu ép theo video; quay ngẫu nhiên Spear
/// Knight / nghỉ Bone Tower ép theo mốc video — phần ngẫu nhiên không so được), ghi vết mỗi tick ⇒ &lt;outDir&gt;/&lt;đoạn&gt;.csv
/// (rel, loại, số thứ tự, X, Y, hướng, state, anim, khung, Simon x/y, camera cx — Y là pivot logic; viewOffset theo prefab ở python). So: python -P -E Tools/cvm2/enemy8_compare.py.
///   Map2EnemyProbe.Run("mh_a,mh_b,…", "&lt;outDir tuyệt đối&gt;", "&lt;shotDir hoặc rỗng&gt;", "mh_a:40,90;sk_teb:30")  ⇒ Done / Status
/// </summary>
public static class Map2EnemyProbe
{
    public const string DATA = "ClaudeNotes/cvm2/ref/data/enemy8/";

    private class Seg
    {
        public string name, section;
        public Action<Seg> setup;
        public int[][] turns;                     // Spear Knight thứ k (theo thứ tự sinh): tick quay NGẪU NHIÊN theo video (null = để Config)
        public object[][] spawns = new object[0][]; // sinh hẹn giờ: { rel, prefab, x, y, hướng }
        public int[][] idles = new int[0][];      // Bone Tower: nghỉ sau lượt 1, 2, … (mỗi tháp) theo video
    }

    private static readonly Seg[] SEGS =
    {
        // Medusa Head — bộ sinh thật FrontOfPlayer + playerRelativeY; con đầu ép đúng tick đỉnh sóng đầu ở video (17 / 56 / 20)
        new Seg { name = "mh_a", section = "TowerHallA", setup = s => MedusaSpawner(17) },
        new Seg { name = "mh_b", section = "TowerHallB", setup = s => MedusaSpawner(56) },
        new Seg { name = "mh_m", section = "Medusa", setup = s => MedusaSpawner(20) },
        // Spear Knight — TEB: video lúc section hiện (f866) đã đang rơi (v 0.76) ⇒ đoạn bắt đầu sớm 6 tick (f863), sinh lơ lửng (336, 77)
        // quay phải (về Simon) rơi xuống khe; Hall B tầng trên (526, 80) quay trái
        new Seg { name = "sk_teb", section = "TowerEntranceB", setup = s => Put("SpearKnight", 336, 77, 1), turns = new[] { new int[0] } },
        new Seg { name = "sk_hb", section = "TowerHallB", setup = s => Put("SpearKnight", 526, 80, -1),
                  turns = new[] { new[] { 117, 310, 402 }, new int[0] }, spawns = new[] { new object[] { 343, "SpearKnight", 718, 80, -1 } } },
        // Ghost — bộ sinh BehindPlayer + playerRelativeY, hiện ở tick của frame đầu video (f4979 / f5813)
        new Seg { name = "gh_1", section = "TowerHallC", setup = s => GhostSpawner(6) },
        new Seg { name = "gh_2", section = "TowerHallC", setup = s => GhostSpawner(6) },
        // dơi treo — Hall C (145, 57) rời chỗ ở f5042; TEA (48, 33) không rời chỗ suốt f187–776
        new Seg { name = "bat_c", section = "TowerHallC", setup = s => Perch(145, 57) },
        new Seg { name = "bat_tea", section = "TowerEntranceA", setup = s => Perch(48, 33) },
        // Bone Tower ×2 — sinh ở tick 0 = f4922 (hết đồng hồ); nghỉ sau mỗi lượt = khoảng phát 1 → phát 1 video − 52
        new Seg { name = "bt_c", section = "TowerHallC", setup = s => { Put("BoneTower", 249, 80, -1); Put("BoneTower", 313, 48, -1); },
                  idles = new[] { new[] { 254, 246, 262, 133 }, new[] { 254 } } },
    };

    public static bool Done { get; private set; } = true;
    public static string Status { get; private set; } = "";
    private static readonly Queue<Seg> queue = new Queue<Seg>();
    private static StageWorld world;
    private static Seg cur;
    private static string outDir, shotDir;
    private static readonly Dictionary<string, List<int>> photos = new Dictionary<string, List<int>>();
    private static readonly List<int[]> puppet = new List<int[]>();
    private static readonly List<GameObject> temp = new List<GameObject>();
    private static readonly Dictionary<long, int> ids = new Dictionary<long, int>();   // (thực thể, tick sinh) — pool dùng lại object
    private static readonly StringBuilder trace = new StringBuilder();
    private static readonly Dictionary<BoneTower, int> towerVolley = new Dictionary<BoneTower, int>();
    private static readonly List<BoneTower> towers = new List<BoneTower>();
    private static readonly List<SpearKnight> knights = new List<SpearKnight>();
    private static int startTick, nextId, savedStage = -1;
    private static bool savedCheat, savedBossLock = true;
    private static readonly System.Reflection.FieldInfo BOSS_LOCK = typeof(NesCameraController).GetField("autoBossLock",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

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
        if (world.CameraCtl != null && BOSS_LOCK != null)
        {
            savedBossLock = (bool)BOSS_LOCK.GetValue(world.CameraCtl);
            BOSS_LOCK.SetValue(world.CameraCtl, false);   // sảnh Medusa bossLockCx 0 ⇒ khoá cx 0 ngay khi vào (§6 CVM2 05) — đoạn so cần camera bám Simon
        }
        Cvm2TerrainProbe.Setup();
        Done = false;
        Next();
        return "bắt đầu " + (queue.Count + 1) + " đoạn";
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
        var sec = Cvm2TerrainProbe.Find(cur.section);
        var p0 = puppet[0];
        world.Warp(sec, p0[0], p0[1]);
        Simon.Teleport(p0[0], p0[1], p0[2]);
        Simon.StartAutoWalk(p0[0], 0, true);
        Simon.Facing = p0[2];
        CvgpDebug.CheatInvincible = true;
        world.ResetRng();
        world.FreezeEnemies(0);
        GameSession.Current.ResetForNewGame();
        if (world.CameraCtl != null)
        {
            world.CameraCtl.Target = world.Player;
            world.CameraCtl.SnapToTarget();
        }
        trace.Length = 0;
        trace.Append("rel,kind,id,type,x,y,facing,state,anim,frame,sx,sy,cx\n");
        ids.Clear();
        nextId = 0;
        towers.Clear();
        towerVolley.Clear();
        knights.Clear();
        cur.setup(cur);
        foreach (var e in world.GroupList(eEntityGroup.Enemy))
        {
            if (e is BoneTower) towers.Add((BoneTower)e);
            if (e is SpearKnight) knights.Add((SpearKnight)e);
        }
        startTick = world.TickCount;
        world.TickBegan += OnTickBegan;
        world.TickEnded += OnTickEnded;
    }

    private static void LoadPuppet(string name)
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

    // ---------------------------------------------------------------- dựng enemy

    static T Prefab<T>(string name) where T : Entity
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>("Assets/Data/Prefabs/Enemy/" + name + ".prefab");
    }

    static EnemyBase Put(string prefab, int x, int y, int facing)
    {
        return world.Spawn(Prefab<EnemyBase>(prefab), x, y, facing) as EnemyBase;
    }

    static void Perch(int x, int y)
    {
        var b = Put("Bat", x, y, -1) as Bat;
        if (b != null)
            b.ForcePerch();
    }

    static EdgeSpawner Spawner(string prefab, eSpawnSide side, int firstDelay)
    {
        var go = new GameObject("Map2EnemyProbe_" + prefab);
        go.transform.SetParent(world.transform, false);
        temp.Add(go);
        var sp = go.AddComponent<EdgeSpawner>();
        sp.prefab = Prefab<EnemyBase>(prefab);
        sp.side = side;
        sp.playerRelativeY = true;
        sp.forceFirstDelay = firstDelay;
        world.AddSpawner(sp);
        return sp;
    }

    static void MedusaSpawner(int firstDelay) { Spawner("MedusaHead", eSpawnSide.FrontOfPlayer, firstDelay); }
    static void GhostSpawner(int firstDelay) { Spawner("Ghost", eSpawnSide.BehindPlayer, firstDelay); }

    // ---------------------------------------------------------------- mỗi tick

    private static void OnTickBegan(StageWorld w)
    {
        int rel = w.TickCount - startTick;   // tick sắp chạy (0-based)
        var s = Simon;
        if (s == null)
            return;
        var p = puppet[Mathf.Min(rel, puppet.Count - 1)];
        s.fx = Fixed.FromPx(p[0]);
        s.fy = Fixed.FromPx(p[1]);
        s.autoWalkTarget = p[0];
        s.Facing = p[2];
        foreach (var sp in cur.spawns)
            if ((int)sp[0] == rel)
            {
                var e = Put((string)sp[1], (int)sp[2], (int)sp[3], (int)sp[4]);
                if (e is SpearKnight) knights.Add((SpearKnight)e);
            }
        // Spear Knight: quay ngẫu nhiên đúng tick video (ngoài mốc ⇒ bộ đếm 9999 — chỉ còn quay ở mép/tường)
        if (cur.turns != null)
            for (int i = 0; i < knights.Count; i++)
            {
                var k = knights[i];
                if (k == null || !k.Alive) continue;
                k.legTimer = i < cur.turns.Length && Array.IndexOf(cur.turns[i], rel) >= 0 ? 1 : 9999;
            }
    }

    private static void OnTickEnded(StageWorld w)
    {
        int rel = w.TickCount - startTick - 1;
        var s = Simon;
        int cx = w.CameraCtl != null ? w.CameraCtl.X : 0;
        for (int g = 0; g < 2; g++)
            foreach (var e in w.GroupList(g == 0 ? eEntityGroup.Enemy : eEntityGroup.EnemyShot))
            {
                if (!e.Alive) continue;
                int id;
                long key = ((long)e.GetInstanceID() << 32) ^ (uint)e.SpawnTick;
                if (!ids.TryGetValue(key, out id)) { id = nextId++; ids[key] = id; }
                var en = e as EnemyBase;
                string state = en != null ? en.DebugState.Split(' ')[0] : "-";
                string anim = e.Anim != null && e.Anim.Current != null ? e.Anim.Current.name : "";
                int frame = e.Anim != null ? e.Anim.FrameIndex : 0;
                trace.Append(rel).Append(',').Append(g == 0 ? "enemy" : "shot").Append(',').Append(id).Append(',').Append(e.GetType().Name)
                     .Append(',').Append(e.X).Append(',').Append(e.Y).Append(',').Append(e.Facing)
                     .Append(',').Append(state).Append(',').Append(anim).Append(',').Append(frame)
                     .Append(',').Append(s.X).Append(',').Append(s.Y).Append(',').Append(cx).Append('\n');
            }
        // Bone Tower: nghỉ sau lượt k theo video (ghi đè Roll ngay tick vào Idle)
        for (int i = 0; i < towers.Count && i < cur.idles.Length; i++)
        {
            var t = towers[i];
            if (t == null || !t.Alive || t.Fsm.CurState != eBoneTowerState.Idle || t.volleys == 0) continue;
            int done;
            towerVolley.TryGetValue(t, out done);
            if (done == t.volleys) continue;
            towerVolley[t] = t.volleys;
            if (t.volleys - 1 < cur.idles[i].Length)
                t.idleLen = cur.idles[i][t.volleys - 1];
        }
        List<int> ph;
        if (!string.IsNullOrEmpty(shotDir) && photos.TryGetValue(cur.name, out ph) && ph.Contains(rel))
            ScreenCapture.CaptureScreenshot(Path.Combine(shotDir, cur.name + "_r" + rel.ToString("000") + ".png"));
        if (rel + 1 >= puppet.Count)
        {
            File.WriteAllText(Path.Combine(outDir, cur.name + ".csv"), trace.ToString(), new UTF8Encoding(false));
            Status += cur.name + ": " + puppet.Count + " tick, " + nextId + " thực thể\n";
            Next();
        }
    }

    // ---------------------------------------------------------------- gỡ

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
                UnityEngine.Object.DestroyImmediate(go);
            }
        temp.Clear();
        towers.Clear();
        if (world != null)
        {
            world.FreezeEnemies(0);
            world.ClearNonPlayer();
        }
    }

    private static void Finish()
    {
        Cvm2TerrainProbe.Cleanup();
        if (world != null && savedStage >= 0)
            world.Stage = savedStage;
        if (Simon != null)
            Simon.Teleport(48, 176, 1);
        world.ResetRng();
        GameSession.Current.ResetForNewGame();
        CvgpDebug.CheatInvincible = savedCheat;
        if (world.CameraCtl != null && BOSS_LOCK != null)
            BOSS_LOCK.SetValue(world.CameraCtl, savedBossLock);
        Done = true;
        Status += "xong\n";
    }
}
#endif
