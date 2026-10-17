#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// CVM2 09 — so Queen Medusa + rắn trong PHÒNG THẬT với 2 video (CHỈ Editor, scene Arena — Player = Simon, đang Play; KHÔNG thuộc bộ hồi
/// quy — đo). Mỗi đoạn dựng phòng (Map2BossRoom), Simon "RỐI" (AutoWalk noClip, mỗi tick ở TickBegan đặt vị trí/hướng — theo video boss
/// ref/data/boss2_simon.csv hoặc cố định), Medusa bắt đầu đúng trạng thái video (thức ngay / DebugStartFly), ghi vết mỗi tick ⇒
/// &lt;outDir&gt;/&lt;đoạn&gt;.csv (rel, loại, id, x, y, state, anim, khung, Simon x/y) và chụp Game view ở các tick cố định ⇒ &lt;shotDir&gt;/&lt;đoạn&gt;_rNNN.png.
/// So: python -P -E Tools/cvm2/boss9_compare.py &lt;outDir&gt; ; cặp ảnh: python -P -E Tools/cvm2/boss9_pairs.py &lt;shotDir&gt;.
///   Map2BossProbe.Run("wake_b2,wake_m2,seg_de,death", "&lt;outDir&gt;", "&lt;shotDir hoặc rỗng&gt;") ⇒ Done / Status
/// </summary>
public static class Map2BossProbe
{
    public const string SIMON_CSV = "ClaudeNotes/cvm2/ref/data/boss2_simon.csv";

    private class Seg
    {
        public string name;
        public int ticks;
        public int simonFrame = -1;           // ≥ 0: Simon rối theo boss2_simon.csv từ frame này (tick t ↔ frame f0 + t / 2.0033)
        public int sx = 30, sy = 144, sf = 1; // Simon cố định (simonFrame < 0)
        public Action<QueenMedusa> setup;
        public int[] shots = new int[0];
    }

    private static readonly Seg[] SEGS =
    {
        // video boss: thức f68 (tick 1 ≈ f68 — chuỗi state Statue → Fly cùng tick) → bay A → dừng 1 (f99–129) → rắn 2 (f130.7) → B (tới f135)
        new Seg { name = "wake_b2", ticks = 200, simonFrame = 67, setup = m => WakeIn(m, 1),
                  shots = new[] { 1, 13, 25, 37, 49, 62, 84, 104, 128, 146 } },
        // video Map_2 (bảng màu chuẩn): Simon đứng x 61 quay phải; tượng còn nguyên f6824 → đầu tượng đen + Medusa f6825/6826 → tím f6830 …
        new Seg { name = "wake_m2", ticks = 60, sx = 61, sf = 1, setup = m => WakeIn(m, 3),
                  shots = new[] { 1, 3, 6, 10, 16, 22, 24 } },
        // video boss: rời dừng 2 f322 (162,147) đoạn D 16 tick → dừng 3 (f330–361) → rắn 4 + đoạn E (ép 64) → (lửa f388); rắn 3 sinh f321.5
        new Seg { name = "seg_de", ticks = 240, sx = 30, sf = 1, setup = m => { m.forcedMoves.Enqueue(64); m.DebugStartFly(162, 147 - 32, -1, 16); SpawnSnake(162, 147 - 32 + 5); },
                  shots = new[] { 4, 16, 50, 80, 96, 110, 130, 150 } },
        // video Map_2: hạ ở (85,157) f7069 → lửa f7069–7100 → quả cầu f7146 (128,137) → chạm sàn f7155
        new Seg { name = "death", ticks = 200, sx = 61, sf = 1, setup = m => { m.DebugStartFly(85, 157 - 32, -1, 9999); pendingDefeat = true; },
                  shots = new[] { 7, 23, 41, 156, 162, 172, 190 } },
    };

    public static bool Done { get; private set; } = true;
    public static string Status { get; private set; } = "";
    private static readonly Queue<Seg> queue = new Queue<Seg>();
    private static StageWorld world;
    private static Seg cur;
    private static string outDir, shotDir;
    private static readonly List<int[]> simonPath = new List<int[]>();   // frame → (px, Y, flip) ; −1 = không thấy
    private static readonly List<GameObject> temp = new List<GameObject>();
    private static readonly Dictionary<long, int> ids = new Dictionary<long, int>();
    private static readonly StringBuilder trace = new StringBuilder();
    private static int startTick, nextId, savedStage = -1;
    private static bool savedCheat, pendingDefeat;
    private static QueenMedusa medusa;

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string Run(string names, string outDirectory, string shotDirectory = "")
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
        LoadSimon();
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

    static void LoadSimon()
    {
        simonPath.Clear();
        string p = Path.Combine(Path.GetDirectoryName(Application.dataPath), SIMON_CSV);
        if (!File.Exists(p))
            return;
        var lines = File.ReadAllLines(p);
        int[] last = { 82, 176, 1 };
        for (int i = 1; i < lines.Length; i++)
        {
            var f = lines[i].Split(',');
            // frame,t,dup,sprite,flip,px,Y,err,n,found
            if (f.Length >= 10 && f[9] == "1" && f[5].Length > 0 && f[6].Length > 0)
                last = new[] { int.Parse(f[5]), int.Parse(f[6]), f[4] == "1" ? 1 : -1 };
            simonPath.Add(last);
        }
    }

    /// <summary>Medusa thức ở tick thứ n của đoạn (n ≥ 1; tick 1 = rel 0).</summary>
    static void WakeIn(QueenMedusa m, int n)
    {
        m.lockTicks = m.wakeDelay - n;
    }

    static void SpawnSnake(int x, int y)
    {
        var pf = UnityEditor.AssetDatabase.LoadAssetAtPath<Snake>("Assets/Data/Prefabs/Boss/Snake.prefab");
        var p = world.Player;
        world.Spawn(pf, x, y, p != null && p.X < x ? -1 : 1);
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
        int sx = cur.sx, sy = cur.sy, sf = cur.sf;
        if (cur.simonFrame >= 0 && cur.simonFrame < simonPath.Count)
        {
            sx = simonPath[cur.simonFrame][0];
            sy = simonPath[cur.simonFrame][1] - 32;
            sf = simonPath[cur.simonFrame][2];
        }
        GameObject host;
        var arena = Map2BossRoom.Enter(world, sx, sy, sf, true, out host);
        if (host != null)
            temp.Add(host);
        Simon.StartAutoWalk(sx, 0, true);
        Simon.Facing = sf;
        CvgpDebug.CheatInvincible = true;
        world.ResetRng();
        world.FreezeEnemies(0);
        GameSession.Current.ResetForNewGame();
        GameSession.Current.stage = 6;
        medusa = arena != null ? arena.Boss as QueenMedusa : null;
        pendingDefeat = false;
        if (medusa != null)
        {
            cur.setup(medusa);
            medusa.SyncView();
        }
        trace.Length = 0;
        trace.Append("rel,kind,id,x,y,state,anim,frame,sx,sy,cx,fx\n");
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
        int x = cur.sx, y = cur.sy, f = cur.sf;
        if (cur.simonFrame >= 0)
        {
            int fr = Mathf.Min(simonPath.Count - 1, cur.simonFrame + Mathf.RoundToInt(rel / 2.0033f));
            if (fr >= 0)
            {
                x = simonPath[fr][0];
                y = simonPath[fr][1] - 32;
                f = simonPath[fr][2];
            }
        }
        s.fx = Fixed.FromPx(x);
        s.fy = Fixed.FromPx(y);
        s.autoWalkTarget = x;
        s.Facing = f;
        if (pendingDefeat && rel == 1 && medusa != null && medusa.Alive)
        {
            pendingDefeat = false;
            medusa.Defeat();
        }
    }

    private static void OnTickEnded(StageWorld w)
    {
        int rel = w.TickCount - startTick - 1;
        var s = Simon;
        int cx = w.CameraCtl != null ? w.CameraCtl.X : 0, fxN = 0;
        foreach (var e in w.GroupList(eEntityGroup.Effect))
            if (e.Alive) fxN++;
        for (int g = 0; g < 2; g++)
            foreach (var e in w.GroupList(g == 0 ? eEntityGroup.Enemy : eEntityGroup.Item))
            {
                if (!e.Alive) continue;
                int id;
                long key = ((long)e.GetInstanceID() << 32) ^ (uint)e.SpawnTick;
                if (!ids.TryGetValue(key, out id)) { id = nextId++; ids[key] = id; }
                var en = e as EnemyBase;
                string state = en != null ? en.DebugState.Split(' ')[0] : "-";
                string anim = e.Anim != null && e.Anim.Current != null ? e.Anim.Current.name : "";
                int frame = e.Anim != null ? e.Anim.FrameIndex : 0;
                trace.Append(rel).Append(',').Append(e.GetType().Name).Append(',').Append(id).Append(',').Append(e.X).Append(',').Append(e.Y)
                     .Append(',').Append(state).Append(',').Append(anim).Append(',').Append(frame).Append(',').Append(s.X).Append(',').Append(s.Y)
                     .Append(',').Append(cx).Append(',').Append(fxN).Append('\n');
            }
        if (!string.IsNullOrEmpty(shotDir) && Array.IndexOf(cur.shots, rel) >= 0)
            ScreenCapture.CaptureScreenshot(Path.Combine(shotDir, cur.name + "_r" + rel.ToString("000") + ".png"));
        if (rel + 1 >= cur.ticks)
        {
            File.WriteAllText(Path.Combine(outDir, cur.name + ".csv"), trace.ToString(), new UTF8Encoding(false));
            Status += cur.name + ": " + cur.ticks + " tick\n";
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
            }
        }
    }

    private static void Finish()
    {
        Unhook();
        Cleanup();
        Cvm2TerrainProbe.Cleanup();
        if (savedStage >= 0)
            world.Stage = savedStage;
        CvgpDebug.CheatInvincible = savedCheat;
        world.ResetRng();
        GameSession.Current.ResetForNewGame();
        Done = true;
    }
}
#endif
