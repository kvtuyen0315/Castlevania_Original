#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness kiểm lõi gameplay trong scene Arena (CVGP 07; routine 10 dùng lại) — CHỈ Editor. Gọi qua MCP execute_code khi Play:
///   CvgpCoreProbe.RunSuite("walk300,wallR,…")  ⇒ chạy lần lượt từng kịch bản (đặt Dummy, phát replay NesPad, hành động theo tick),
///   ghi từng tick (toạ độ, state, camera, đòn, hash) và TỰ CHẤM theo kỳ vọng ⇒ CvgpCoreProbe.SuiteReport() khi SuiteDone.
/// Hash FNV-1a mọi tick: tick tương đối, mọi thực thể (fx, fy, hướng, sống, bất tử), camera X, Rng.State, tổng đòn, state Dummy
/// — so tất định giữa các lần chạy / targetFrameRate.
/// Toạ độ kỳ vọng theo section Arena (Tools/cvgp/build_arena.py): sàn y 176; tường thấp x 416–431; hố 480–511; nước 544–575;
/// stair0 ↗ chân (620,176) đỉnh (684,112); stair2 ↖ chân (948,112) đỉnh (916,80); rộng 1040 ⇒ camera 0..784; bia x 860.
/// </summary>
public static class CvgpCoreProbe
{
    public static string Label { get; private set; } = "";
    public static bool Running { get; private set; }
    public static int Target { get; private set; }
    public static int Recorded { get; private set; }
    public static uint Hash { get; private set; }
    public static readonly List<int> Xs = new List<int>();
    public static readonly List<int> Ys = new List<int>();
    public static readonly List<int> Cams = new List<int>();
    public static readonly List<int> States = new List<int>();
    public static readonly List<int> HitTicks = new List<int>();
    public static readonly List<string> HitText = new List<string>();
    public static readonly List<string> Checks = new List<string>();
    /// <summary>Hash luỹ kế sau từng tick của lần chạy hiện tại / lần trước (tìm tick lệch đầu tiên khi so tất định).</summary>
    public static readonly List<uint> TickHashes = new List<uint>();
    public static readonly List<uint> PrevTickHashes = new List<uint>();
    public static readonly List<string> TickDumps = new List<string>();
    public static readonly List<string> PrevTickDumps = new List<string>();
    /// <summary>Ghi chuỗi trạng thái mọi thực thể từng tick (chậm — chỉ bật khi dò lệch).</summary>
    public static bool DumpEntities;
    public static int CameraBad;
    public static string CameraFirstBad = "";
    public static int StartTick;
    private static int startHits;
    public static int Fails { get; private set; }

    private static StageWorld world;
    private static readonly Queue<string> suite = new Queue<string>();
    public static readonly List<string> SuiteReports = new List<string>();
    public static bool SuiteDone { get; private set; } = true;
    public static int SuiteFails { get; private set; }
    public static uint SuiteHash { get; private set; }
    private static Action<int> onTickAction;
    private static Action finish;
    private static readonly List<Entity> extras = new List<Entity>();

    public static ArenaDummy Dummy => world != null ? world.Player as ArenaDummy : null;

    // ---------------------------------------------------------------- bộ kịch bản

    /// <summary>Chạy lần lượt các kịch bản (tên cách nhau dấu phẩy). Trả tổng số tick dự kiến.</summary>
    public static int RunSuite(string names)
    {
        StopAll();
        suite.Clear();
        SuiteReports.Clear();
        SuiteFails = 0;
        SuiteHash = 2166136261u;
        foreach (var n in names.Split(','))
            if (n.Trim().Length > 0)
                suite.Enqueue(n.Trim());
        SuiteDone = suite.Count == 0;
        return NextInSuite();
    }

    /// <summary>Dừng mọi thứ đang chạy: bỏ hàng đợi, ngừng ghi, ngừng replay NesPad, dọn bia phụ.</summary>
    public static void StopAll()
    {
        suite.Clear();
        SuiteDone = true;
        Unhook();
        onTickAction = null;
        finish = null;
        if (NesPad.I != null)
            NesPad.I.StopReplay();
        ClearExtras();
    }

    /// <summary>
    /// Đo hiệu năng (không gắn hook ghi — chỉ đếm trong StageWorld.Step): Dummy ở x 800 cạnh bia, phát input trộn n tick.
    /// Đọc kết quả bằng PerfReport() sau khi chạy xong.
    /// </summary>
    public static void RunPerf(int ticks)
    {
        StopAll();
        world = StageWorld.Current;
        world.ClearNonPlayer();
        Dummy.walkSpeedOverride = 0f;
        Dummy.Teleport(800, 176, 1);
        var src = NesReplay.Decode("R*40 B -*20 L*30 A -*40 R*60 B -*20 L*60 R+A R*50 -*30 B -*15 L*20");
        var list = new List<int>(ticks);
        for (int i = 0; i < ticks; i++)
            list.Add(src[i % src.Count]);
        NesPad.I.PlayReplay(list);
        world.ResetPerf();
        StageWorld.MeasurePerf = true;
    }

    public static string PerfReport()
    {
        var w = StageWorld.Current;
        int n = Math.Max(1, w.MeasuredSteps);
        return "perf: " + w.MeasuredSteps + " Step, alloc tổng " + w.TotalStepAlloc + " B, Step có alloc " + w.AllocSteps
               + ", max " + w.MaxStepAlloc + " B; thời gian TB " + (w.TotalStepMs / n).ToString("0.0000") + " ms, max "
               + w.MaxStepMs.ToString("0.000") + " ms; thực thể " + CountEntities(w) + ", pool đã tạo " + w.Pool.CreatedCount;
    }

    private static int CountEntities(StageWorld w)
    {
        int c = 0;
        for (int g = 0; g < Entity.GROUP_COUNT; g++)
            c += w.GroupList(g).Count;
        return c;
    }

    private static int NextInSuite()
    {
        if (suite.Count == 0)
        {
            SuiteDone = true;
            return 0;
        }
        return Run(suite.Dequeue());
    }

    public static string SuiteReport()
    {
        var sb = new StringBuilder();
        sb.Append("SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
          .Append(SuiteFails).Append(", hash gộp ").Append(SuiteHash.ToString("x8"));
        foreach (var r in SuiteReports)
            sb.Append('\n').Append(r);
        return sb.ToString();
    }

    // ---------------------------------------------------------------- 1 kịch bản

    /// <summary>Kịch bản có sẵn — trả số tick sẽ ghi. Tên lạ ⇒ −1.</summary>
    public static int Run(string scenario)
    {
        world = StageWorld.Current;
        if (world == null || Dummy == null)
        {
            Debug.LogError("[CvgpCoreProbe] cần StageWorld đang chạy với Player = ArenaDummy");
            return -1;
        }
        ClearExtras();
        world.ClearNonPlayer();   // mọi kịch bản bắt đầu từ thế giới sạch (bia/spawn đặt lại, RNG về seed) ⇒ hash không phụ thuộc thứ tự chạy
        world.ResetRng();
        var d = Dummy;
        d.walkSpeedOverride = 0f;
        onTickAction = null;
        finish = null;
        string rle;
        switch (scenario)
        {
            case "walk300":
                d.Teleport(48, 176, 1);
                rle = "R*300 -*4";
                finish = () =>
                {
                    int bad = 0;
                    for (int i = 0; i < 300; i++)
                        if (Xs[i] - (i == 0 ? 48 : Xs[i - 1]) != 1)
                            bad++;
                    Expect("X sau 300 tick R", 348, Xs[299]);
                    Expect("bước x ≠ 1 px/tick", 0, bad);
                    Expect("Y min", 176, Min(Ys));
                    Expect("Y max", 176, Max(Ys));
                    Expect("X sau khi nhả (4 tick)", 348, Last(Xs));
                };
                break;
            case "wallR":
            case "wallR3":
            case "wallR24":
                d.walkSpeedOverride = scenario == "wallR3" ? 3f : scenario == "wallR24" ? 24f : 0f;
                d.Teleport(300, 176, 1);
                rle = scenario == "wallR" ? "R*160" : scenario == "wallR3" ? "R*60" : "R*10";
                finish = () =>
                {
                    Expect("dừng sát tường trái của tường x 416 (X+8 = 416)", 408, Last(Xs));
                    Expect("X không vượt 408", 408, Max(Xs));
                    Expect("Y giữ 176", 176, Max(Ys));
                };
                break;
            case "wallL":
            case "wallL3":
                d.walkSpeedOverride = scenario == "wallL3" ? 3f : 0f;
                d.Teleport(470, 176, -1);
                rle = scenario == "wallL" ? "L*60" : "L*30";
                finish = () =>
                {
                    Expect("dừng sát mặt phải tường (X−8 = 432)", 440, Last(Xs));
                    Expect("X không dưới 440", 440, Min(Xs));
                };
                break;
            case "ledge":
                d.Teleport(720, 112, 1);
                rle = "R*30 -*30";
                finish = () =>
                {
                    int fall = -1;
                    for (int i = 0; i < Xs.Count && fall < 0; i++)
                        if (Ys[i] > 112)
                            fall = i;
                    Expect("X lúc bắt đầu rơi (mép bục 735: X−8 > 735)", 744, fall >= 0 ? Xs[fall] : -1);
                    Expect("đáp đúng mặt sàn dưới", 176, Last(Ys));
                    Expect("không lún dưới sàn", 176, Max(Ys));
                    Expect("X giữ khi rơi thẳng", 744, Last(Xs));
                    Expect("số lần đáp", 1, d.LandCount);
                };
                break;
            case "stairs":
                d.Teleport(620, 176, 1);
                rle = "U*140 -*4 D*140 -*4";
                finish = () => StairChecks(0, 64, new Vector2Int(684, 112), new Vector2Int(620, 176));
                break;
            case "stair2":
                d.Teleport(948, 112, -1);
                rle = "U*70 -*4 D*70 -*4";
                finish = () => StairChecks(2, 32, new Vector2Int(916, 80), new Vector2Int(948, 112));
                break;
            case "water":
                d.Teleport(530, 176, 1);
                rle = "R*40 -*80";
                finish = () =>
                {
                    Expect("chết 1 lần", 1, d.DeathCount);
                    Expect("loại ô chết (4 = Water)", (int)eTileCollision.Water, (int)d.LastDeath);
                };
                break;
            case "pit":
                d.Teleport(460, 176, 1);
                rle = "R*40 -*100";
                finish = () =>
                {
                    Expect("chết 1 lần", 1, d.DeathCount);
                    Expect("loại ô chết (5 = Pit, rơi khỏi đáy)", (int)eTileCollision.Pit, (int)d.LastDeath);
                    Expect("về điểm xuất phát sau DEAD_TICKS", 460, Last(Xs));
                };
                break;
            case "camR":
                d.Teleport(1000, 176, 1);
                rle = "R*60";
                finish = () =>
                {
                    Expect("dừng ở mép phải section (1040 − 8)", 1032, Last(Xs));
                    Expect("camera kẹp max 1040 − 256", 784, Last(Cams));
                };
                break;
            case "camL":
                d.Teleport(40, 176, -1);
                rle = "L*60";
                finish = () =>
                {
                    Expect("dừng ở mép trái section (0 + 8)", 8, Last(Xs));
                    Expect("camera kẹp min", 0, Last(Cams));
                };
                break;
            case "hit":
            {
                var t = SpawnTarget(200, false);
                d.Teleport(180, 176, 1);
                rle = "B -*20 B -*20";
                finish = () =>
                {
                    Expect("số đòn (2 lần B, mỗi đòn chồng 8 tick)", 2, HitTicks.Count);
                    Expect("bia nhận", 2, t.HitsTaken);
                    Expect("HP bia = 96 − 2×8 (12/8, đòn yếu −8)", 80, t.Health.Current);
                    Expect("thanh 16 vạch = ceil(80×16/96)", 14, t.Health.Bars(16));
                };
                break;
            }
            case "touch":
            {
                SpawnTarget(200, true);
                d.Teleport(196, 176, -1);
                rle = "-*200";
                finish = () =>
                {
                    Expect("số đòn thân trong 200 tick (bất tử chặn)", 3, HitTicks.Count);
                    Expect("khoảng cách đòn 1→2 = InvincibleTicks", d.InvincibleOnHit, HitTicks.Count > 1 ? HitTicks[1] - HitTicks[0] : -1);
                    Expect("khoảng cách đòn 2→3", d.InvincibleOnHit, HitTicks.Count > 2 ? HitTicks[2] - HitTicks[1] : -1);
                    Expect("dummy trúng", 3, d.HitsTaken);
                };
                break;
            }
            case "matrix":
            {
                SpawnTarget(300, true);
                SpawnTarget(306, true);
                d.Teleport(100, 176, 1);
                rle = "B -*30";
                finish = () =>
                {
                    Expect("EnemyBody↔Enemy, PlayerAttack↔Player: 0 đòn", 0, HitTicks.Count);
                    int ok = 0;
                    ok += CombatResolver.CanHit(eHitLayer.PlayerAttack, eHurtLayer.Enemy) ? 1 : 0;
                    ok += CombatResolver.CanHit(eHitLayer.PlayerAttack, eHurtLayer.Breakable) ? 1 : 0;
                    ok += CombatResolver.CanHit(eHitLayer.EnemyAttack, eHurtLayer.Player) ? 1 : 0;
                    ok += CombatResolver.CanHit(eHitLayer.EnemyBody, eHurtLayer.Player) ? 1 : 0;
                    ok += !CombatResolver.CanHit(eHitLayer.PlayerAttack, eHurtLayer.Player) ? 1 : 0;
                    ok += !CombatResolver.CanHit(eHitLayer.EnemyAttack, eHurtLayer.Enemy) ? 1 : 0;
                    ok += !CombatResolver.CanHit(eHitLayer.EnemyAttack, eHurtLayer.Breakable) ? 1 : 0;
                    ok += !CombatResolver.CanHit(eHitLayer.EnemyBody, eHurtLayer.Enemy) ? 1 : 0;
                    ok += !CombatResolver.CanHit(eHitLayer.EnemyBody, eHurtLayer.Breakable) ? 1 : 0;
                    Expect("ma trận 9/9 ô đúng", 9, ok);
                };
                break;
            }
            case "spawn":
            {
                SpawnPoint sp = world.SpawnPoints.Count > 0 ? world.SpawnPoints[0] : null;
                if (sp != null)
                {
                    if (sp.Live != null && sp.Live.Alive)
                        sp.Live.Kill(eKillReason.Cleared);
                    sp.ResetSpawn();
                }
                d.Teleport(300, 176, 1);
                rle = "-*70";
                var log = new StringBuilder();
                onTickAction = rel =>
                {
                    if (rel == 1 || rel == 20 || rel == 50) d.Teleport(800, 176, 1);
                    if (rel == 10 || rel == 40) d.Teleport(300, 176, 1);
                    if (rel == 30) world.KillGroup(eEntityGroup.Enemy);
                    if (sp != null && (rel % 10 == 2 || rel % 10 == 1))
                        log.Append(rel).Append(':').Append(sp.State).Append('/').Append(sp.SpawnCount).Append(' ');
                };
                finish = () =>
                {
                    Checks.Add("    nhật ký điểm sinh (tick:state/lần sinh): " + log);
                    Expect("số lần sinh (vào khung ×3: đầu, sau khi ra khỏi lề, sau khi bị hạ + ra/vào)", 3, sp != null ? sp.SpawnCount : -1);
                    Expect("bia đang sống ở cuối", 1, sp != null && sp.Live != null && sp.Live.Alive ? 1 : 0);
                };
                break;
            }
            case "freeze":
                d.Teleport(48, 176, 1);
                world.FreezeWorld(30);
                rle = "R*60";
                finish = () => Expect("đi 60 tick nhưng 30 tick đầu đóng băng ⇒ +30", 78, Last(Xs));
                break;
            case "freezeEnemy":
                d.Teleport(48, 176, 1);
                world.FreezeEnemies(30);
                rle = "R*60";
                finish = () => Expect("Stopwatch không đóng băng Player ⇒ +60", 108, Last(Xs));
                break;
            case "mix":
                // Tất định: 1789 tick trộn đi/nhảy/roi thử/thang ↗↗↖/rơi/hố/bia (spawn + đòn 2 chiều)/đóng băng + RNG mỗi tick.
                // Cùng replay + seed ⇒ phải ra cùng hash ở mọi targetFrameRate.
                world.ResetRng();
                d.Teleport(48, 176, 1);
                rle = "R*120 R+A R*50 -*10 B -*12 L*40 A -*45 R*120 "                         // 0–399   sàn trái, nhảy, đụng tường
                      + "R*20 U*140 -*5 R*30 U*80 -*5 R*60 B -*14 L*45 "                       // 400–799 thang 0 ↗, thang 1 ↗, bục y 80
                      + "R*12 B -*20 B -*20 L*30 A -*40 R*60 B -*20 -*94 "                     // 800–1099 bia x 860: chạm + roi thử
                      + "R*40 -*20 R*20 L*60 A -*30 R+A R*60 -*68 "                            // 1100–1399 rơi hố, hồi sinh, nhảy
                      + "U*70 -*5 L*40 D*10 R*40 D*70 R*60 -*94";                              // 1400–1788 thang 2 ↖, rơi khỏi bục
                onTickAction = rel =>
                {
                    world.Rng.Next(256);
                    if (rel == 400) d.Teleport(600, 176, 1);
                    if (rel == 800) d.Teleport(840, 176, -1);
                    if (rel == 1000) world.FreezeWorld(20);
                    if (rel == 1050) world.FreezeEnemies(30);
                    if (rel == 1100) d.Teleport(460, 176, 1);
                    if (rel == 1400) d.Teleport(948, 112, -1);
                };
                finish = () => Checks.Add("    (mix: chỉ so hash giữa các lần chạy / fps; RNG cuối " + world.Rng.State + ", bia sinh "
                                          + (world.SpawnPoints.Count > 0 ? world.SpawnPoints[0].SpawnCount : -1) + " lần)");
                break;
            default:
                Debug.LogError("[CvgpCoreProbe] kịch bản lạ " + scenario);
                return -1;
        }
        d.ResetStats();
        var ticks = NesReplay.Decode(rle);
        NesPad.I.PlayReplay(ticks);
        Begin(scenario, ticks.Count);
        return ticks.Count;
    }

    private static ArenaTarget SpawnTarget(int x, bool touch)
    {
        ArenaTarget prefab = null;
        for (int i = 0; i < world.SpawnPoints.Count && prefab == null; i++)
            prefab = world.SpawnPoints[i].prefab as ArenaTarget;
        var t = (ArenaTarget)world.Spawn(prefab, x, 176, -1);
        t.touchDamage = touch;
        t.Touch.active = touch;
        extras.Add(t);
        return t;
    }

    private static void ClearExtras()
    {
        for (int i = 0; i < extras.Count; i++)
            if (extras[i] != null && extras[i].Alive)
            {
                ((ArenaTarget)extras[i]).touchDamage = true;
                extras[i].Kill(eKillReason.Cleared);
            }
        extras.Clear();
    }

    private static void StairChecks(int stairIndex, int len, Vector2Int top, Vector2Int bottom)
    {
        var p = world.Tiles.GetStair(stairIndex);
        StairPath found;
        Expect("TryGetStair(chân, lên) = thang " + stairIndex, stairIndex,
            world.Tiles.TryGetStair(p.bottom.x, p.bottom.y, 1, 0, out found) ? found.index : -1);
        Expect("TryGetStair(đỉnh, xuống) = thang " + stairIndex, stairIndex,
            world.Tiles.TryGetStair(p.top.x, p.top.y, -1, 0, out found) ? found.index : -1);
        Expect("TryGetStair(chân, XUỐNG) không có", -1, world.Tiles.TryGetStair(p.bottom.x, p.bottom.y, -1, 0, out found) ? found.index : -1);
        Expect("chân.x", bottom.x, p.bottom.x);
        Expect("đỉnh.x", top.x, p.top.x);
        Expect("độ dài (px dọc)", len, p.Length);
        int reach = -1, off = 0;
        for (int i = 0; i < Xs.Count; i++)
        {
            if (reach < 0 && Xs[i] == top.x && Ys[i] == top.y)
                reach = i;
            if ((eDummyState)States[i] == eDummyState.Stair && p.ParamOf(Xs[i], Ys[i]) < 0)
                off++;
        }
        Expect("tick tới đỉnh (len / 0.5 px/tick)", len * 2 - 1, reach);
        Expect("điểm lệch khỏi đường chéo khi ở Stair", 0, off);
        Expect("về lại chân", bottom.x * 1000 + bottom.y, Last(Xs) * 1000 + Last(Ys));
    }

    // ---------------------------------------------------------------- ghi

    /// <summary>Bắt đầu ghi n tick kể từ tick KẾ TIẾP (khớp tick đầu của replay).</summary>
    public static void Begin(string label, int ticks)
    {
        world = StageWorld.Current;
        Unhook();
        Label = label;
        Target = ticks;
        Recorded = 0;
        Hash = 2166136261u;
        Fails = 0;
        Xs.Clear();
        Ys.Clear();
        Cams.Clear();
        States.Clear();
        HitTicks.Clear();
        HitText.Clear();
        Checks.Clear();
        PrevTickHashes.Clear();
        PrevTickHashes.AddRange(TickHashes);
        TickHashes.Clear();
        PrevTickDumps.Clear();
        PrevTickDumps.AddRange(TickDumps);
        TickDumps.Clear();
        CameraBad = 0;
        CameraFirstBad = "";
        StartTick = world.TickCount;
        startHits = world.Combat.TotalHits;
        world.TickEnded += OnTickEnded;
        world.Hit += OnHit;
        Running = true;
    }

    private static void Unhook()
    {
        if (world != null)
        {
            world.TickEnded -= OnTickEnded;
            world.Hit -= OnHit;
        }
        Running = false;
    }

    private static void OnHit(HitInfo h)
    {
        HitTicks.Add(h.tick - StartTick);
        HitText.Add((h.tick - StartTick) + ":" + h.attacker.name + "→" + h.victim.name + "(" + h.hitbox.layer + "→" + h.hurtbox.layer + ")");
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
        var d = Dummy;
        var cam = w.CameraCtl;
        int rel = w.TickCount - StartTick;
        Mix(rel);
        for (int g = 0; g < Entity.GROUP_COUNT; g++)
        {
            var list = w.GroupList(g);
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                Mix(e.fx);
                Mix(e.fy);
                Mix(e.Facing);
                Mix(e.Alive ? 1 : 0);
                Mix(e.InvincibleTicks);
            }
        }
        Mix(cam != null ? cam.X : 0);
        Mix(w.Rng.State);
        Mix(w.Combat.TotalHits - startHits);
        if (d != null)
        {
            Mix((int)d.Fsm.CurState);
            Xs.Add(d.X);
            Ys.Add(d.Y);
            States.Add((int)d.Fsm.CurState);
        }
        if (cam != null)
        {
            Cams.Add(cam.X);
            int want = cam.Locked || cam.IsScrolling || d == null ? cam.X : cam.Clamp(d.X - cam.FollowScreenX);
            float camPx = (cam.Cam.transform.position.x - NesCoord.CameraHome.x) * NesCoord.PPU;
            bool integral = Mathf.Abs(camPx - Mathf.Round(camPx)) < 1e-4f && (int)Mathf.Round(camPx) == cam.X;
            if (want != cam.X || !integral)
            {
                if (CameraBad == 0)
                    CameraFirstBad = "tick " + Recorded + " want " + want + " got " + cam.X + " px " + camPx;
                CameraBad++;
            }
        }
        TickHashes.Add(Hash);
        if (DumpEntities)
        {
            var sb = new StringBuilder();
            sb.Append(rel).Append(" cam ").Append(cam != null ? cam.X : 0).Append(" rng ").Append(w.Rng.State);
            for (int g = 0; g < Entity.GROUP_COUNT; g++)
            {
                var list = w.GroupList(g);
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    sb.Append(" | ").Append(e.name).Append(' ').Append(e.fx).Append(',').Append(e.fy).Append(" f").Append(e.Facing)
                      .Append(e.Alive ? " A" : " D").Append(" inv").Append(e.InvincibleTicks).Append(' ').Append(e.DebugState);
                }
            }
            TickDumps.Add(sb.ToString());
        }
        if (onTickAction != null)
            onTickAction(rel);
        if (++Recorded >= Target)
            Finish();
    }

    private static void Finish()
    {
        Unhook();
        Expect("camera sai luật/không nguyên px", 0, CameraBad);
        if (finish != null)
        {
            try
            {
                finish();
            }
            catch (Exception ex)
            {
                Checks.Add("    FAIL ngoại lệ khi chấm: " + ex.Message);
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

    private static void Expect(string what, int want, int got)
    {
        bool ok = want == got;
        if (!ok)
            Fails++;
        Checks.Add("    " + (ok ? "ĐẠT " : "FAIL ") + what + ": kỳ vọng " + want + ", được " + got);
    }

    /// <summary>Tick tương đối đầu tiên mà hash luỹ kế lần này ≠ lần trước (−1 = trùng hết).</summary>
    public static string FirstDiff()
    {
        int n = Math.Min(TickHashes.Count, PrevTickHashes.Count);
        for (int i = 0; i < n; i++)
            if (TickHashes[i] != PrevTickHashes[i])
            {
                string s = "lệch đầu tiên ở chỉ số " + i + " (tick tương đối " + (i + 1) + ")";
                if (i < TickDumps.Count && i < PrevTickDumps.Count)
                    s += "\n  trước: " + PrevTickDumps[i] + "\n  nay  : " + TickDumps[i]
                         + (i > 0 ? "\n  tick trước đó (nay): " + TickDumps[i - 1] : "");
                return s;
            }
        return "trùng " + n + " tick (số tick " + TickHashes.Count + " vs " + PrevTickHashes.Count + ")";
    }

    private static int Min(List<int> l)
    {
        int m = int.MaxValue;
        for (int i = 0; i < l.Count; i++)
            m = Math.Min(m, l[i]);
        return m;
    }

    private static int Max(List<int> l)
    {
        int m = int.MinValue;
        for (int i = 0; i < l.Count; i++)
            m = Math.Max(m, l[i]);
        return m;
    }

    private static int Last(List<int> l)
    {
        return l.Count > 0 ? l[l.Count - 1] : int.MinValue;
    }

    /// <summary>Tóm tắt lần chạy gần nhất.</summary>
    public static string Report(bool withTrace = false)
    {
        var sb = new StringBuilder();
        var d = Dummy;
        sb.Append(Label).Append(": ").Append(Running ? "ĐANG CHẠY " : "xong ").Append(Recorded).Append('/').Append(Target)
          .Append(" tick, hash ").Append(Hash.ToString("x8")).Append(", FAIL ").Append(Fails);
        if (Xs.Count > 0)
            sb.Append("\n  X ").Append(Xs[0]).Append("→").Append(Last(Xs)).Append(" [").Append(Min(Xs)).Append("..").Append(Max(Xs))
              .Append("]  Y ").Append(Ys[0]).Append("→").Append(Last(Ys)).Append(" [").Append(Min(Ys)).Append("..").Append(Max(Ys))
              .Append("]  cam →").Append(Cams.Count > 0 ? Last(Cams) : -1).Append(" [").Append(Cams.Count > 0 ? Min(Cams) : -1).Append("..")
              .Append(Cams.Count > 0 ? Max(Cams) : -1).Append("]  state cuối ").Append(d != null ? d.Fsm.CurState.ToString() : "-");
        if (d != null)
            sb.Append("\n  dummy: trúng ").Append(d.HitsTaken).Append(", chết ").Append(d.DeathCount).Append(" (").Append(d.LastDeath)
              .Append("), đáp ").Append(d.LandCount);
        if (HitText.Count > 0)
            sb.Append("\n  đòn (tick tương đối): ").Append(string.Join(", ", HitText.ToArray()));
        foreach (var c in Checks)
            sb.Append('\n').Append(c);
        if (withTrace)
        {
            sb.Append("\n  trace t:X,Y,state,cam");
            for (int i = 0; i < Xs.Count; i++)
                sb.Append(i % 10 == 0 ? "\n   " : " ").Append(i).Append(':').Append(Xs[i]).Append(',').Append(Ys[i]).Append(',')
                  .Append((eDummyState)States[i]).Append(',').Append(Cams[i]);
        }
        return sb.ToString();
    }
}
#endif
