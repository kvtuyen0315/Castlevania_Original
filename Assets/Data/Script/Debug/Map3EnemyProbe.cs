#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// CVM3 07 — so Skeleton / xương / Fleaman (CVM3 08: + Raven, Ghost, Medusa Head, Bone Tower + cầu lửa) trong Unity với video Map_3 (CHỈ Editor, scene Arena — Player = Simon, đang Play; không thuộc
/// bộ hồi quy). Khuôn Map2EnemyProbe (CVM2 08): mỗi đoạn dựng section Block 3 (Cvm3TerrainProbe.Setup), Simon "RỐI" theo đường đi video
/// (ClaudeNotes/cvm3/ref/data/enemy7/puppet_&lt;đoạn&gt;.csv — AutoWalk noClip đã tới đích, mỗi tick ở TickBegan đặt fx/fy/hướng = video;
/// cheat bất tử), sinh enemy ĐÚNG chỗ/lúc như video, phần NGẪU NHIÊN ép theo video: việc từng lượt của Skeleton (Skeleton.forcedPlan —
/// tới / lùi / ném xương thấp / cao như video), số tick đếm của Fleaman (Fleaman.forceTriggerDelay), vy0 từng chặng lướt của Raven
/// (Raven.forcedVy0 — Tools/cvm3/enemy8b_raven_model.py), nghỉ từng lượt của Bone Tower (idleLen). Ghi vết mỗi tick ⇒ &lt;outDir&gt;/&lt;đoạn&gt;.csv
/// (rel, loại, số thứ tự, kiểu, X, Y, hướng, state, anim, khung, Simon x/y, camera cx). So: python -P -E Tools/cvm3/enemy7_compare.py
/// (đoạn 07) / Tools/cvm3/enemy8b_compare.py (đoạn 08 — puppet ở ref/data/enemy8b/, WalkwayB x section = sheet + 112).
///   Map3EnemyProbe.Run("sk_wea,sk_web1,…", "&lt;outDir tuyệt đối&gt;", "&lt;shotDir hoặc rỗng&gt;", "sk_wea:40,90;fl_wea0:80")  ⇒ Done / Status
/// </summary>
public static class Map3EnemyProbe
{
    public const string DATA = "ClaudeNotes/cvm3/ref/data/enemy7/";
    public const string DATA8 = "ClaudeNotes/cvm3/ref/data/enemy8b/";
    const int T = Skeleton.PLAN_TOWARD, A = Skeleton.PLAN_AWAY, TL = Skeleton.PLAN_THROW_LOW, TH = Skeleton.PLAN_THROW_HIGH, S = Skeleton.PLAN_STAND;

    private class Spawn
    {
        public int rel, x, y, facing, delay = -1;
        public string prefab;
        public int[] plan, vy0, idles;
    }

    private class Seg
    {
        public string name, section, data = DATA;
        public Spawn[] spawns;
    }

    static Spawn Skel(int rel, int x, int y, params int[] plan) { return new Spawn { rel = rel, prefab = "Skeleton", x = x, y = y, facing = -1, plan = plan }; }
    static Spawn Flea(int rel, int x, int y, int delay) { return new Spawn { rel = rel, prefab = "Fleaman", x = x, y = y, facing = -1, delay = delay }; }
    /// <summary>Raven: vy0 từng chặng lướt (px/tick × 256) theo mô phỏng khớp video.</summary>
    static Spawn Rv(int rel, int x, int y, params int[] vy0) { return new Spawn { rel = rel, prefab = "Raven", x = x, y = y, facing = -1, vy0 = vy0 }; }
    static Spawn En(int rel, string prefab, int x, int y, int facing) { return new Spawn { rel = rel, prefab = prefab, x = x, y = y, facing = facing }; }
    /// <summary>Bone Tower: idles[0] = nghỉ đầu (thay firstIdle), idles[k] = nghỉ sau lượt k (thay Roll) — theo mốc cầu lửa video.</summary>
    static Spawn Bt(int rel, int x, int y, params int[] idles) { return new Spawn { rel = rel, prefab = "BoneTower", x = x, y = y, facing = -1, idles = idles }; }

    // tick rel ↔ frame f0 + rel / 2.0033 (Tools/cvm3/enemy7_puppet.py); vị trí = video (CVM3_MEMORY §2g)
    private static readonly Seg[] SEGS =
    {
        // WEA 0:33.5–0:35.9 (f1004–1077): hiện ở mép trái (33,128), tới ⇒ mép bờ x 63 ⇒ nhảy lọt mép bục (96–191, y 96) ⇒ đứng lên mặt bục;
        // rồi lùi, lùi, tới, ném (xương thấp f1068), lùi
        new Seg { name = "sk_wea", section = "WalkwayEntranceA", spawns = new[] { Skel(4, 33, 128, T, T, A, A, T, TL, A, A) } },
        // WEB 0:50.5–0:51.8 (f1516–1553): đứng ở mép trái bục (352,160) ném xương cao (f1520.7) rồi tới ⇒ nhảy qua hố 320–351
        new Seg { name = "sk_web1", section = "WalkwayEntranceB", spawns = new[] { Skel(0, 352, 160, TH, T, S, S) } },
        // WEB 1:10.7–1:14.1 (f2122–2222): (671,160) tới, tới, ném thấp, ném cao, lùi, tới, ném thấp, tới ⇒ mép 608 nhảy ⇒ rơi hố 544–607
        new Seg { name = "sk_web2", section = "WalkwayEntranceB", spawns = new[] { Skel(3, 671, 160, T, T, TL, TH, A, T, TL, T) } },
        // WalkwayA 1:39.5–1:42.4 (f2984–3072): (671,64) tới, tới ⇒ mép khối 640 nhảy xuống sàn y 128 ⇒ lùi, lùi, ném thấp (f3052.9), tới
        new Seg { name = "sk_wa", section = "WalkwayA", spawns = new[] { Skel(3, 671, 64, T, T, A, A, TL, T) } },
        // MummyMan 2:50.7–2:54.1 (f5120–5222): cầu hỏng St9 — S5 (815,96) ném cao rồi tới ⇒ nhảy qua khe 768–799; S6 (879,96) tới ⇒ nhảy
        // qua khe 832–863 đáp bục 800–831 rồi đứng
        new Seg { name = "sk_mm", section = "MummyMan", spawns = new[] { Skel(3, 815, 96, TH, T, S, S, S), Skel(128, 879, 96, T, S, S, S) } },
        // Fleaman WEA (3 con ngồi trên bục): đếm sau khi |dx| < 128 = 62 / 62 / 136 tick (video)
        new Seg { name = "fl_wea0", section = "WalkwayEntranceA", spawns = new[] { Flea(0, 433, 128, 62) } },
        new Seg { name = "fl_wea1", section = "WalkwayEntranceA", spawns = new[] { Flea(0, 305, 128, 62) } },
        new Seg { name = "fl_wea2", section = "WalkwayEntranceA", spawns = new[] { Flea(0, 177, 96, 136) } },
        // ---- CVM3 08 (puppet ref/data/enemy8b/; rel = (frame hiện − frame đầu đoạn) × 2.0033; vị trí = vết video lúc hiện, WalkwayB + 112)
        // Raven WEB #0 f1340 (280,41): lướt xuống 1.75 ⇒ treo ⇒ lao ngang ⇒ vượt Simon ⇒ phanh ⇒ treo 14 ⇒ lao lại ⇒ ra màn (f1490)
        new Seg { name = "rv_web0", section = "WalkwayEntranceB", data = DATA8, spawns = new[] { Rv(4, 280, 41, 448) } },
        // Raven WEB #1 f1574 (423,89) 2.0 · #2 f1634 (487,57) 1.25 · #3 f1654 (527,169) 1.25 rồi lướt lần 2 1.75 (bị hạ f1626 / f1682 / f1732)
        new Seg { name = "rv_web1", section = "WalkwayEntranceB", data = DATA8, spawns = new[] { Rv(4, 423, 89, 512), Rv(124, 487, 57, 320), Rv(164, 527, 169, 320, 448) } },
        // Raven WalkwayB #4 f3594 (271+112,57) 2.0 (bị hạ f3644)
        new Seg { name = "rv_wb0", section = "WalkwayB", data = DATA8, spawns = new[] { Rv(4, 383, 57, 512) } },
        // Raven WalkwayB #5 f3865 (487+112,121) 1.75 · #6 f3898 (551+112,121) 2.0 (bị hạ f3917 / f3957)
        new Seg { name = "rv_wb1", section = "WalkwayB", data = DATA8, spawns = new[] { Rv(4, 599, 121, 448), Rv(70, 663, 121, 512) } },
        // Raven MummyMan #11 f5086 (747,41) 1.25 (bị hạ f5123)
        new Seg { name = "rv_mm", section = "MummyMan", data = DATA8, spawns = new[] { Rv(4, 747, 41, 320) } },
        // Ghost WEA f416: hiện (671,145) sau lưng Simon (x màn 224), bay trái — số CVM2 08 (không đổi)
        new Seg { name = "gh_wea", section = "WalkwayEntranceA", data = DATA8, spawns = new[] { En(4, "Ghost", 671, 145, -1) } },
        // Medusa Head WalkwayA: 5 đỉnh sóng (= lúc sinh, Tools/cvm3/enemy8b_mh.py) f2790.9 / 2855.0 / 2930.2 / 2993.5 / 3057.8 — số CVM2 08
        new Seg { name = "mh_wa", section = "WalkwayA", data = DATA8, spawns = new[] { En(6, "MedusaHead", 306, 79, -1), En(134, "MedusaHead", 410, 79, -1),
            En(285, "MedusaHead", 560, 80, -1), En(412, "MedusaHead", 683, 79, -1), En(540, "MedusaHead", 759, 79, -1) } },
        // Medusa Head MummyMan: đỉnh f5371.0 / 5434.8 / 5498.6 / 5562.3 / 5626.5 (sóng đều 128) — số CVM2 08
        new Seg { name = "mh_mm", section = "MummyMan", data = DATA8, spawns = new[] { En(6, "MedusaHead", 1136, 47, -1), En(134, "MedusaHead", 1239, 47, -1),
            En(262, "MedusaHead", 1241, 47, -1), En(389, "MedusaHead", 1274, 48, -1), En(518, "MedusaHead", 1338, 48, -1) } },
        // Bone Tower MummyMan (1180, chân 96) hiện f5394: cầu lửa video f5419.3 / 5439 / 5481.5 / 5501.5 ⇒ nghỉ đầu 38 (51 tick sau khi hiện = 38 + 1 + 12),
        // nghỉ sau lượt 1 = 73 (phát 1 → phát 1 = 124.6 tick = nghỉ + 52 — NGẮN hơn khoảng Unity 185–314: §6)
        new Seg { name = "bt_mm", section = "MummyMan", data = DATA8, spawns = new[] { Bt(4, 1180, 96, 38, 73) } },
    };

    public static bool Done { get; private set; } = true;
    public static string Status { get; private set; } = "";
    private static readonly Queue<Seg> queue = new Queue<Seg>();
    private static StageWorld world;
    private static Seg cur;
    private static string outDir, shotDir;
    private static readonly Dictionary<string, List<int>> photos = new Dictionary<string, List<int>>();
    private static readonly List<int[]> puppet = new List<int[]>();
    private static readonly Dictionary<long, int> ids = new Dictionary<long, int>();   // (thực thể, tick sinh) — pool dùng lại object
    private static readonly Dictionary<BoneTower, int[]> towerIdles = new Dictionary<BoneTower, int[]>();
    private static readonly Dictionary<BoneTower, int> towerVolley = new Dictionary<BoneTower, int>();
    private static readonly StringBuilder trace = new StringBuilder();
    private static int startTick, nextId, savedStage = -1;
    private static bool savedCheat, savedBossLock = true;
    private static readonly System.Reflection.FieldInfo BOSS_LOCK = typeof(NesCameraController).GetField("autoBossLock",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string SegNames()
    {
        var sb = new StringBuilder();
        foreach (var s in SEGS) sb.Append(sb.Length > 0 ? "," : "").Append(s.name);
        return sb.ToString();
    }

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
            BOSS_LOCK.SetValue(world.CameraCtl, false);
        }
        Status = "";
        Cvm3TerrainProbe.Setup();
        Done = false;
        Next();
        return "bắt đầu " + (queue.Count + 1) + " đoạn";
    }

    private static void Next()
    {
        Unhook();
        if (world != null)
            world.ClearNonPlayer();
        if (queue.Count == 0)
        {
            Finish();
            return;
        }
        cur = queue.Dequeue();
        LoadPuppet(cur);
        if (puppet.Count == 0)
        {
            Status += cur.name + ": thiếu puppet\n";
            Next();
            return;
        }
        var sec = Cvm3TerrainProbe.Find(cur.section);
        var p0 = puppet[0];
        world.Warp(sec, p0[0], p0[1]);
        world.Stage = sec != null && sec.data != null ? sec.data.stage : 7;
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
        towerIdles.Clear();
        towerVolley.Clear();
        nextId = 0;
        startTick = world.TickCount;
        world.TickBegan += OnTickBegan;
        world.TickEnded += OnTickEnded;
    }

    private static void LoadPuppet(Seg seg)
    {
        puppet.Clear();
        string p = Path.Combine(Path.GetDirectoryName(Application.dataPath), seg.data + "puppet_" + seg.name + ".csv");
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

    static T Prefab<T>(string name) where T : Entity
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>("Assets/Data/Prefabs/Enemy/" + name + ".prefab");
    }

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
            if (sp.rel == rel)
            {
                var e = w.Spawn(Prefab<EnemyBase>(sp.prefab), sp.x, sp.y, sp.facing) as EnemyBase;
                var sk = e as Skeleton;
                if (sk != null && sp.plan != null) { sk.forcedPlan = sp.plan; sk.planIndex = 0; }
                var fl = e as Fleaman;
                if (fl != null) fl.forceTriggerDelay = sp.delay;
                var rv = e as Raven;
                if (rv != null && sp.vy0 != null) rv.forcedVy0 = sp.vy0;
                var bt = e as BoneTower;
                if (bt != null && sp.idles != null && sp.idles.Length > 0)
                {
                    bt.idleLen = sp.idles[0];
                    towerIdles[bt] = sp.idles;
                }
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
        // Bone Tower: nghỉ sau lượt k theo video (ghi đè Roll ngay tick vào Idle) — khuôn Map2EnemyProbe
        foreach (var kv in towerIdles)
        {
            var t = kv.Key;
            if (t == null || !t.Alive || t.Fsm.CurState != eBoneTowerState.Idle || t.volleys == 0) continue;
            int done;
            towerVolley.TryGetValue(t, out done);
            if (done == t.volleys) continue;
            towerVolley[t] = t.volleys;
            if (t.volleys < kv.Value.Length)
                t.idleLen = kv.Value[t.volleys];
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

    private static void Finish()
    {
        if (world != null)
        {
            world.FreezeEnemies(0);
            world.ClearNonPlayer();
        }
        Cvm3TerrainProbe.Cleanup();
        if (world != null && savedStage >= 0)
            world.Stage = savedStage;
        if (Simon != null)
            Simon.Teleport(48, 176, 1);
        world.ResetRng();
        GameSession.Current.ResetForNewGame();
        CvgpDebug.CheatInvincible = savedCheat;
        if (world.CameraCtl != null)
        {
            if (BOSS_LOCK != null)
                BOSS_LOCK.SetValue(world.CameraCtl, savedBossLock);
            world.CameraCtl.Target = world.Player;
        }
        Done = true;
        Status += "xong\n";
    }
}
#endif
