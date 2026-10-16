#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness CVM2 04 — lọ nước thánh bay XUYÊN enemy/boss, chỉ vỡ khi chạm sàn (CHỈ Editor, scene Arena, Player = Simon). Qua MCP khi Play:
///   HolyPierceProbe.RunSuite(HolyPierceProbe.ScenarioNames()) ⇒ chạy, ghi từng tick, TỰ CHẤM ⇒ SuiteReport()  (RegressionRunner bộ "holy")
///   HolyPierceProbe.ShotDir = "&lt;thư mục&gt;" trước khi chạy ⇒ chụp Game view lúc lọ vào thân / giữa thân / ló ra / lửa (so video)
/// Bằng chứng video (CVM2_MEMORY §2g): Boss_Map_1 f467–483 lọ vào thân Phantom Bat, bị thân che, ló ra dưới-trái vẫn bay rồi vỡ trên sàn;
/// ENEMY 16→15 đúng f471 (lọ vừa chạm, không có lửa, Simon đang tư thế ném) ⇒ lọ GÂY 1 đòn khi bay qua; Boss_Map_2 f134–149, f160–163,
/// f164–179, f383–402 lọ xuyên Medusa vỡ ở sàn phía sau, f393 ENEMY 10→9 lúc lọ trong mặt Medusa (không lửa, Simon đang ném).
/// Input giải mã thẳng từ RLE (không file replay). Kỳ vọng độc lập Config: vị trí/tick lửa so với kịch bản đối chứng không mục tiêu.
/// </summary>
public static class HolyPierceProbe
{
    const string BAT_PREFAB = "Assets/Data/Prefabs/Boss/PhantomBat.prefab";
    const string TARGET_PREFAB = "Assets/Data/Prefabs/Test/ArenaTarget.prefab";

    private struct Scn
    {
        public string name, rle, note;
        public Scn(string n, string r, string note) { name = n; rle = r; this.note = note; }
    }

    private static readonly Scn[] SCENARIOS =
    {
        new Scn("control", "-*2 Sub -*60", "đối chứng: Simon (300,176) quay phải ném nước thánh, không mục tiêu ⇒ tick + chỗ vỡ chuẩn"),
        new Scn("enemy", "-*2 Sub -*60", "bia (Enemy) đứng ngay trước mặt x 322: lọ trúng 1 lần, bay tiếp, vỡ ở sàn PHÍA SAU bia đúng tick/chỗ như đối chứng"),
        new Scn("bat", "-*2 Sub -*150", "Phantom Bat lơ lửng thấp (732,160): lọ trúng 1 lần rồi bay xuyên thân (dơi khựng đứng suốt lúc chạm), vỡ ở sàn dưới dơi ⇒ lửa giữ chân + đốt"),
        new Scn("medusa", "-*130 Sub -*130", "Queen Medusa vỡ tượng bay sát sàn về Simon (32,176): lọ trúng 1 lần, xuyên thân, vỡ ở sàn phía sau"),
        new Scn("count3", "-*2 Sub -*22 Sub -*22 Sub -*100", "Shot III: 3 lọ qua bia A (x 322) — mỗi lọ trúng A 1 lần; bia B (x 345) đứng chỗ lửa; subWeaponHits = tổng đòn lên enemy (luật 10 lần ⇒ multiplier)"),
    };

    // ---------------------------------------------------------------- ghi
    private struct Rec
    {
        public int bx, by, ebars, hitsA, hitsB, swHits, hearts, bottles, fires;
        public bool stun, overlap;
        public Vector2Int bottle, fire;
        public string state;
    }

    public static string Label { get; private set; } = "";
    public static bool Running { get; private set; }
    public static int Fails { get; private set; }
    public static uint Hash { get; private set; }
    public static string ShotDir;
    private static readonly List<Rec> recs = new List<Rec>();
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> measures = new List<string>();
    private static readonly Queue<string> suite = new Queue<string>();
    public static readonly List<string> SuiteReports = new List<string>();
    public static bool SuiteDone { get; private set; } = true;
    public static int SuiteFails { get; private set; }
    public static uint SuiteHash { get; private set; }
    private static StageWorld world;
    private static int target, recorded, startTick;
    private static Action finish;
    private static readonly List<GameObject> temp = new List<GameObject>();
    private static readonly List<Entity> extras = new List<Entity>();
    private static ArenaTarget tA, tB;
    private static BossBase boss;
    private static string current;
    private static int ctrlFireRel = -1, ctrlSpawnRel = -1;
    private static Vector2Int ctrlFire;
    private static int shotState, shotOver;
    private static int savedStage = -1;

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

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
        world = StageWorld.Current;
        savedStage = world != null ? world.Stage : -1;   // bat/medusa đổi world.Stage ⇒ trả lại cho bộ chạy sau
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
        CvgpDebug.CheatInvincible = false;
        BackToArena();
    }

    public static string SuiteReport()
    {
        var sb = new StringBuilder();
        sb.Append("HOLY SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
          .Append(SuiteFails).Append(", hash bộ ").Append(SuiteHash.ToString("x8")).Append('\n');
        foreach (var r in SuiteReports)
            sb.Append(r).Append('\n');
        return sb.ToString();
    }

    /// <summary>Trả thế giới về section Arena + Simon (48,176) như lúc vào Play.</summary>
    public static void BackToArena()
    {
        world = StageWorld.Current;
        if (world == null || Simon == null)
            return;
        Cleanup();
        Go("Arena", 48, 176, 1);
        if (savedStage >= 0)
            world.Stage = savedStage;
        world.ResetRng();
        GameSession.Current.ResetForNewGame();
    }

    // ---------------------------------------------------------------- dựng

    static void Go(string section, int x, int y, int facing)
    {
        foreach (var s in world.Sections)
            if (s.data != null && s.data.name == section)
            {
                world.Warp(s, x, y);
                Simon.Teleport(x, y, facing);
                if (world.CameraCtl != null)
                    world.CameraCtl.SnapToTarget();
                return;
            }
        Debug.LogError("[HolyPierceProbe] không có section " + section);
    }

    static ArenaTarget Target(int x, int y)
    {
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<ArenaTarget>(TARGET_PREFAB);
        var t = (ArenaTarget)world.Spawn(prefab, x, y, -1);
        t.touchDamage = false;   // pool giữ cờ của probe trước (CVM2_MEMORY §6) ⇒ đặt tường minh
        t.Touch.active = false;
        extras.Add(t);
        return t;
    }

    static void Cleanup()
    {
        for (int i = 0; i < extras.Count; i++)
            if (extras[i] != null && extras[i].Alive)
                extras[i].Kill(eKillReason.Cleared);
        extras.Clear();
        foreach (var go in temp)
            if (go != null)
            {
                var sp = go.GetComponent<EnemySpawner>();
                if (world != null && sp != null)
                    world.RemoveSpawner(sp);
                UnityEngine.Object.DestroyImmediate(go);
            }
        temp.Clear();
        boss = null;
        tA = tB = null;
        if (world != null)
        {
            world.FreezeEnemies(0);
            world.ClearNonPlayer();
            if (world.Tiles != null)
            {
                world.Tiles.WallMinX = int.MinValue;
                world.Tiles.WallMaxX = int.MaxValue;
            }
            if (world.CameraCtl != null && world.Section != null)
                world.CameraCtl.Bind(world.Section.data);
        }
    }

    // ---------------------------------------------------------------- 1 kịch bản

    public static int Run(string scenario)
    {
        world = StageWorld.Current;
        var s = Simon;
        if (world == null || s == null)
        {
            Debug.LogError("[HolyPierceProbe] cần scene Arena với Player = SimonController");
            return -1;
        }
        string rle = null;
        foreach (var sc in SCENARIOS)
            if (sc.name == scenario)
                rle = sc.rle;
        if (rle == null)
        {
            Debug.LogError("[HolyPierceProbe] kịch bản lạ " + scenario);
            return -1;
        }
        var ticks = NesReplay.Decode(rle);
        Cleanup();
        current = scenario;
        var ses = GameSession.Current;
        ses.ResetForNewGame();
        finish = null;
        CvgpDebug.CheatInvincible = true;
        switch (scenario)
        {
            case "control":
                Go("Arena", 300, 176, 1);
                world.ResetRng();
                finish = ControlChecks;
                break;
            case "enemy":
                Go("Arena", 300, 176, 1);
                world.ResetRng();
                tA = Target(322, 176);
                finish = EnemyChecks;
                break;
            case "count3":
                Go("Arena", 300, 176, 1);
                world.ResetRng();
                ses.shotMultiplier = 3;
                tA = Target(322, 176);
                tB = Target(345, 176);
                finish = Count3Checks;
                break;
            case "bat":
                {
                    Go("PhantomBat", 700, 176, 1);
                    var go = new GameObject("HolyPierceProbe_Arena");
                    go.transform.SetParent(world.transform, false);
                    temp.Add(go);
                    var arena = go.AddComponent<BossArena>();
                    arena.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<PhantomBat>(BAT_PREFAB);
                    world.AddSpawner(arena);
                    world.CameraCtl.Lock(512);
                    world.ResetRng();
                    ses.stage = 3;
                    world.Stage = 3;
                    var b = arena.SpawnBoss(world) as PhantomBat;
                    b.SetPositionPx(732, 160);
                    b.Activate();
                    b.Fsm.SetState(ePhantomBatState.Hover);
                    b.DebugHoverTicks(9999);
                    b.SyncView();
                    boss = b;
                    finish = () => BossChecks(true);
                }
                break;
            case "medusa":
                {
                    GameObject host;
                    var arena = BossRoomSetup.Enter(world, BossRoomSetup.MEDUSA, true, out host);
                    if (host != null)
                        temp.Add(host);
                    world.ResetRng();
                    ses.stage = BossRoomSetup.MEDUSA.stage;
                    boss = arena != null ? arena.Boss : null;
                    finish = () => BossChecks(false);
                }
                break;
        }
        ses.subWeapon = eSubWeapon.HolyWater;
        ses.hearts = 20;
        ses.subWeaponHits = 0;
        ses.NotifyChanged();
        s.ResetStats();
        s.whipCount = s.throwCount = s.throwFailCount = 0;
        NesPad.I.PlayReplay(ticks);
        Begin(scenario, ticks.Count);
        return ticks.Count;
    }

    // ---------------------------------------------------------------- chấm

    static void ControlChecks()
    {
        int spawn = First(r => r.bottles > 0);
        int fire = First(r => r.fires > 0);
        Expect("lọ sinh + vỡ thành lửa", 1, spawn >= 0 && fire > spawn ? 1 : 0);
        if (fire < 0)
            return;
        ctrlSpawnRel = spawn;
        ctrlFireRel = fire;
        ctrlFire = recs[fire].fire;
        Expect("lửa trên mặt sàn y 176", 176, ctrlFire.y);
        measures.Add("đối chứng: lọ sinh rel " + spawn + " (" + recs[spawn].bottle + "), vỡ rel " + fire + " lửa " + ctrlFire);
    }

    static void EnemyChecks()
    {
        int hit = First(r => r.hitsA > 0);
        Expect("lọ trúng bia (có đòn khi bay — video boss1 f471 / boss2 f393)", 1, hit >= 0 ? 1 : 0);
        if (hit < 0)
            return;
        Expect("lúc trúng chưa có lửa (đòn của LỌ)", 0, recs[hit].fires);
        Expect("lọ còn bay tick sau khi trúng (không vỡ)", 1, hit + 1 < recs.Count && recs[hit + 1].bottles == 1 ? 1 : 0);
        int over = CountWhere(r => r.bottles > 0 && r.overlap);
        ExpectRange("lọ đi xuyên thân bia (tick chồng hộp)", 3, 99, over);
        int fire = First(r => r.fires > 0);
        Expect("vỡ cùng tick như đối chứng (quỹ đạo không đổi)", ctrlFireRel, fire);
        Expect("lửa cùng chỗ như đối chứng (x*1000+y)", ctrlFire.x * 1000 + ctrlFire.y, fire >= 0 ? recs[fire].fire.x * 1000 + recs[fire].fire.y : -1);
        var hb = tA.Hurt.SectionRect;
        ExpectRange("lửa ở PHÍA SAU bia (x lửa − mép phải hộp bia)", 0, 99, fire >= 0 ? recs[fire].fire.x - hb.xMax : -1);
        Expect("bia trúng đúng 1 lần (lọ singleHit, lửa không chạm)", 1, recs[recs.Count - 1].hitsA);
        Expect("subWeaponHits = số đòn lên enemy", 1, recs[recs.Count - 1].swHits);
        measures.Add("bia hộp " + hb + ": lọ trúng rel " + hit + " tại " + recs[hit].bottle + ", chồng hộp " + over + " tick, vỡ rel " + fire + " lửa " + (fire >= 0 ? recs[fire].fire.ToString() : "-"));
    }

    static void Count3Checks()
    {
        var last = recs[recs.Count - 1];
        Expect("ném đủ 3 lọ (Shot III)", 3, Simon.throwCount);
        Expect("bia A trúng 3 lần (mỗi lọ 1 lần, không vỡ)", 3, last.hitsA);
        int fires = 0;
        for (int i = 1; i < recs.Count; i++)
            if (recs[i].fires > recs[i - 1].fires)
                fires++;
        Expect("3 lọ đều vỡ thành lửa ở sàn (lần sinh lửa)", 3, fires);
        ExpectRange("bia B đứng trong lửa bị đốt", 1, 99, last.hitsB);
        Expect("subWeaponHits = tổng đòn lên enemy (A + B)", last.hitsA + last.hitsB, last.swHits);
        measures.Add("III: A " + last.hitsA + " đòn, B " + last.hitsB + " đòn, subWeaponHits " + last.swHits + ", tim " + last.hearts);
    }

    static void BossChecks(bool bat)
    {
        Expect("có boss", 1, boss != null ? 1 : 0);
        if (boss == null)
            return;
        int hit = -1;
        for (int i = 1; i < recs.Count; i++)
            if (recs[i].ebars < recs[i - 1].ebars || (i == 1 && recs[0].ebars < GameSession.MAX_HP))
            {
                hit = i;
                break;
            }
        Expect("lọ làm boss mất máu khi bay qua (video boss1 f471, boss2 f393)", 1, hit >= 0 ? 1 : 0);
        if (hit < 0)
            return;
        Expect("lúc mất máu lần đầu chưa có lửa (đòn của LỌ)", 0, recs[hit].fires);
        Expect("lọ còn bay tick sau khi trúng (không vỡ trên thân)", 1, hit + 1 < recs.Count && recs[hit + 1].bottles == 1 ? 1 : 0);
        int over = 0, moved = 0, notStun = 0, lastOver = -1;
        for (int i = hit; i < recs.Count; i++)
            if (recs[i].bottles > 0 && recs[i].overlap)
            {
                over++;
                lastOver = i;
                if (recs[i].bx != recs[hit].bx || recs[i].by != recs[hit].by) moved++;
                if (!recs[i].stun) notStun++;
            }
        ExpectRange("lọ đi xuyên thân boss (tick chồng hộp)", 3, 99, over);
        Expect("boss đứng yên suốt lúc lọ chạm (khựng — video f471–482 dơi đứng)", 0, moved);
        Expect("boss khựng suốt lúc lọ chạm (tick không khựng)", 0, notStun);
        int fire = First(r => r.fires > 0);
        Expect("lọ vỡ thành lửa trên sàn y 176 SAU khi ra khỏi thân", 1, fire > lastOver && recs[fire].fire.y == 176 ? 1 : 0);
        int dropsBottle = 0;
        for (int i = 1; i < recs.Count; i++)
            if (recs[i].ebars < recs[i - 1].ebars && recs[i].fires == 0)
                dropsBottle++;
        Expect("1 lọ chỉ trúng boss 1 lần (singleHit)", 1, dropsBottle);
        string extra = "";
        if (bat)
        {
            int fireEnd = fire >= 0 ? First(fire, r => r.fires == 0) : -1;
            if (fireEnd < 0) fireEnd = recs.Count;
            int stunIn = 0, movedIn = 0, drops = 0;
            for (int i = fire; i >= 0 && i < fireEnd; i++)
            {
                if (recs[i].stun) stunIn++;
                if (recs[i].bx != recs[fire].bx || recs[i].by != recs[fire].by) movedIn++;
                if (i > 0 && recs[i].ebars < recs[i - 1].ebars) drops++;
            }
            Expect("lửa giữ chân dơi suốt lúc cháy (tick không khựng)", 0, (fireEnd - fire) - stunIn);
            Expect("dơi không dời chỗ khi trong lửa", 0, movedIn);
            ExpectRange("lửa đốt dơi nhiều lần (bất tử 16 ⇒ 5–6)", 5, 6, drops);
            extra = ", lửa rel " + fire + "→" + fireEnd + " đốt " + drops + " lần";
        }
        measures.Add((bat ? "Phantom Bat" : "Queen Medusa") + " (" + recs[hit].bx + "," + recs[hit].by + "): lọ trúng rel " + hit + " tại " + recs[hit].bottle
                     + " (E " + recs[hit - 1].ebars + "→" + recs[hit].ebars + "), chồng hộp " + over + " tick tới rel " + lastOver + ", vỡ rel " + fire
                     + " lửa " + (fire >= 0 ? recs[fire].fire.ToString() : "-") + extra + ", E cuối " + recs[recs.Count - 1].ebars);
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
        shotState = shotOver = 0;
        recs.Clear();
        checks.Clear();
        measures.Clear();
        startTick = world.TickCount;
        world.TickEnded += OnTickEnded;
        Running = true;
    }

    private static void Unhook()
    {
        if (world != null)
            world.TickEnded -= OnTickEnded;
        Running = false;
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
        var ses = GameSession.Current;
        int rel = w.TickCount - startTick - 1;
        var r = new Rec();
        r.ebars = ses.enemyHp;
        r.swHits = ses.subWeaponHits;
        r.hearts = ses.hearts;
        r.hitsA = tA != null ? tA.HitsTaken : 0;
        r.hitsB = tB != null ? tB.HitsTaken : 0;
        RectInt victim = new RectInt();
        bool hasVictim = false;
        if (boss != null)
        {
            r.bx = boss.X;
            r.by = boss.Y;
            r.stun = boss.stunTicks > 0;
            r.state = boss.DebugState;
            hasVictim = boss.Hurt.active;
            victim = boss.Hurt.SectionRect;
        }
        else if (tA != null)
        {
            hasVictim = tA.Alive;
            victim = tA.Hurt.SectionRect;
        }
        var list = w.GroupList(eEntityGroup.PlayerWeapon);
        for (int i = 0; i < list.Count; i++)
        {
            var p = list[i] as SubWeaponProjectile;
            if (p == null || !p.Alive)
                continue;
            if (p is HolyFire)
            {
                r.fires++;
                r.fire = new Vector2Int(p.X, p.Y);
            }
            else if (p is HolyWater)
            {
                r.bottles++;
                r.bottle = new Vector2Int(p.X, p.Y);
                if (hasVictim && p.Hit.active && CombatResolver.Overlaps(p.Hit.SectionRect, victim))
                    r.overlap = true;
            }
            Mix(p.fx);
            Mix(p.fy);
        }
        recs.Add(r);
        Mix(rel); Mix(s.fx); Mix(s.fy); Mix(r.bx); Mix(r.by); Mix(r.stun ? 1 : 0); Mix(r.ebars); Mix(r.hitsA); Mix(r.hitsB); Mix(r.swHits); Mix(r.hearts);
        Shoot(r, rel);
        if (++recorded >= target)
            Finish();
    }

    /// <summary>Chụp 4 khung so video: lọ vào thân (0), 6 tick sau (1), lọ vừa ra khỏi thân (2), lửa +2 tick (3).</summary>
    private static void Shoot(Rec r, int rel)
    {
        if (string.IsNullOrEmpty(ShotDir) || current == "control" || current == "count3")
            return;
        string name = null;
        int n = recs.Count;
        if (r.overlap)
            shotOver++;
        if (shotState == 0 && shotOver == 1) { name = "a_vao"; shotState = 1; }
        else if (shotState == 1 && shotOver == 7) { name = "b_giua"; shotState = 2; }
        else if (shotState >= 1 && shotState < 3 && !r.overlap && r.bottles > 0 && n >= 2 && recs[n - 2].overlap) { name = "c_ra"; shotState = 3; }
        else if (shotState >= 1 && shotState < 4 && r.fires > 0 && n >= 3 && recs[n - 3].fires == 0 && recs[n - 2].fires > 0) { name = "d_lua"; shotState = 4; }
        if (name == null)
            return;
        Directory.CreateDirectory(ShotDir);
        ScreenCapture.CaptureScreenshot(Path.Combine(ShotDir, current + "_" + name + "_r" + rel.ToString("000") + ".png"));
    }

    private static void Finish()
    {
        Unhook();
        NesPad.I.StopReplay();
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
        sb.Append("== ").Append(Label).Append(": ").Append(recorded).Append(" tick, hash ").Append(Hash.ToString("x8")).Append(", FAIL ").Append(Fails).Append('\n');
        foreach (var m in measures)
            sb.Append("  đo: ").Append(m).Append('\n');
        foreach (var c in checks)
            sb.Append(c).Append('\n');
        return sb.ToString();
    }

    /// <summary>Vết từng tick (debug): rel boss stun E lọ lửa A B swHits.</summary>
    public static string Trace(int from, int to)
    {
        var sb = new StringBuilder();
        for (int i = Math.Max(0, from); i <= to && i < recs.Count; i++)
        {
            var r = recs[i];
            sb.Append(i).Append(" boss(").Append(r.bx).Append(',').Append(r.by).Append(") ").Append(r.state).Append(r.stun ? " STUN" : "").Append(" E").Append(r.ebars)
              .Append(r.bottles > 0 ? " lọ" + r.bottle + (r.overlap ? "*" : "") : "").Append(r.fires > 0 ? " lửa" + r.fire : "")
              .Append(" A").Append(r.hitsA).Append(" B").Append(r.hitsB).Append(" sw").Append(r.swHits).Append('\n');
        }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- tiện ích

    static int First(Func<Rec, bool> f) { return First(0, f); }

    static int First(int from, Func<Rec, bool> f)
    {
        for (int i = Math.Max(0, from); i < recs.Count; i++)
            if (f(recs[i]))
                return i;
        return -1;
    }

    static int CountWhere(Func<Rec, bool> f)
    {
        int n = 0;
        foreach (var r in recs)
            if (f(r))
                n++;
        return n;
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
