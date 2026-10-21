#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness CVM2 06 — bệ di động, bẫy ép gai, hố chết (CHỈ Editor, scene Arena, Player = Simon). Qua MCP khi Play:
///   Map2MechProbe.RunSuite(Map2MechProbe.ScenarioNames()) ⇒ chạy theo NesClock, ghi từng tick, TỰ CHẤM ⇒ SuiteReport()  (RegressionRunner bộ "mech")
///   Map2MechProbe.ShotDir = "&lt;thư mục&gt;" trước khi chạy ⇒ chụp Game view ở các tick đánh dấu (so video — Tools/cvm2/mech6_pairs.py)
///   Map2MechProbe.SaveTraces("&lt;thư mục&gt;") sau khi chạy ⇒ vết từng tick (so quỹ đạo video — Tools/cvm2/mech6_compare.py)
///   Map2MechProbe.Enter(section, x, y) ⇒ thử tay (menu CVM2/Spawn/Mech …): dựng section Block 2 + sinh bệ/bẫy từ marker.
/// Section Block 2 dựng lúc chạy (Cvm2TerrainProbe.Setup — không đổi Arena.unity), gỡ cuối bộ. Kỳ vọng = số đo VIDEO (hằng VIDEO_* dưới,
/// CVM2_MEMORY §2i) — độc lập Config: Config lệch số đo ⇒ FAIL.
/// </summary>
public static class Map2MechProbe
{
    const string TARGET_PREFAB = "Assets/Data/Prefabs/Test/ArenaTarget.prefab";
    const string GHOUL_PREFAB = "Assets/Data/Prefabs/Enemy/Ghoul.prefab";

    // ---------------------------------------------------------------- số đo video (CVM2_MEMORY §2i)
    struct PlatV { public string id; public int center, amp, start; public double period; }
    static readonly PlatV[] VIDEO_PLAT =
    {
        new PlatV { id = "platform_lane_TEB", center = 64, amp = 16, period = 201.4, start = 4 },      // f1351–1552 (start: tick đã chạy khi section hiện f866)
        new PlatV { id = "platform_lane_HallB_1", center = 304, amp = 32, period = 402.4, start = 6 }, // f3030–4153 (f2801)
        new PlatV { id = "platform_lane_HallB_2", center = 432, amp = 24, period = 201.8, start = 6 }, // f3117–4037 (f2801)
    };
    struct CrushV { public string id; public int maxFrame, start, cycle; }
    static readonly CrushV[] VIDEO_CRUSH =
    {
        new CrushV { id = "crusher_1", maxFrame = 6, start = 55, cycle = 112 },
        new CrushV { id = "crusher_2", maxFrame = 4, start = 7, cycle = 80 },
        new CrushV { id = "crusher_3", maxFrame = 7, start = 7, cycle = 128 },
    };
    const int VIDEO_DOWN = 4, VIDEO_GRID = 16;

    /// <summary>Khung bẫy theo luật đo video (độc lập Crusher.FrameAt).</summary>
    static int VideoFrame(CrushV c, int t)
    {
        int b1 = VIDEO_GRID * (VIDEO_DOWN * c.maxFrame / VIDEO_GRID + 1);
        t = ((t % c.cycle) + c.cycle) % c.cycle;
        if (t < VIDEO_DOWN * c.maxFrame) return 1 + t / VIDEO_DOWN;
        if (t < b1) return c.maxFrame - 1;
        return Math.Max(0, c.maxFrame - 2 - (t - b1) / VIDEO_GRID);
    }

    // ---------------------------------------------------------------- kịch bản
    private class Scn
    {
        public string name, section, rle, note;
        public int x, y, facing;
        public int[] shots;            // tick rel chụp ảnh
        public Action setup;           // sau Warp + sinh cơ chế
        public Action check;
    }

    private static Scn[] scenarios;

    static Scn[] Scenarios()
    {
        if (scenarios != null)
            return scenarios;
        scenarios = new[]
        {
            new Scn { name = "plat_ride_teb", section = "TowerEntranceB", x = 80, y = 80, facing = -1, rle = "-*404",
                note = "TEB: Simon đứng yên giữa bệ lúc sinh (bệ ở mép phải x 64–95) 2 chu kỳ: bệ đúng công thức sin đo video, Simon trôi theo đúng dx, không rơi",
                shots = new[] { 20, 50, 80, 101, 140, 170 }, check = CheckRideTeb },
            new Scn { name = "plat_ride_hallb", section = "TowerHallB", x = 336, y = 80, facing = -1, rle = "-*402",
                note = "Hall B: Simon đứng yên giữa bệ 1 (x 320–351 lúc sinh — mép phải) 1 chu kỳ 402 tick; bệ 2 chạy riêng chu kỳ 201.8 — cả hai đúng công thức",
                shots = new[] { 30, 100, 200, 300 }, check = CheckRideHallB },
            new Scn { name = "plat_walk_cross", section = "TowerEntranceB", x = 104, y = 80, facing = -1, rle = "L*20 -*76 L*40 -*10",
                note = "TEB: đi trái từ bờ phải lên bệ, đứng yên để bệ chở sang mép trái (tới rel 96 = bệ chạm bờ trái x 32), đi tiếp lên bờ trái x 0–31 (video 0:49.0–0:51.4)",
                check = CheckWalkCross },
            new Scn { name = "plat_land", section = "TowerEntranceB", x = 120, y = 80, facing = -1, rle = "-*2 L+A L*45 -*30",
                note = "TEB: nhảy trái từ bờ phải, đáp lên mặt bệ y 80 giữa khe (video 0:50.7 Simon đáp bệ), sau đó được chở", check = CheckLand },
            new Scn { name = "plat_jump_off", section = "TowerEntranceB", x = 88, y = 80, facing = -1, rle = "-*20 A -*90",
                note = "TEB: nhảy THẲNG từ bệ đang chạy trái: x giữ nguyên suốt cú nhảy (không quán tính bệ — video 0:49.5–0:49.8 x 86), bệ trôi đi ⇒ rơi xuống khối hàng 7 y 112 (video 0:50.2)",
                shots = new[] { 22, 40, 60 }, check = CheckJumpOff },
            new Scn { name = "plat_whip_crouch", section = "TowerEntranceB", x = 80, y = 80, facing = -1, rle = "-*20 B -*30 D*60 -*10",
                note = "TEB: quất roi rồi ngồi trên bệ — vẫn được chở đúng dx (video 2:16.2–2:16.8 Simon quất trên bệ Hall B)", check = CheckWhipCrouch },
            new Scn { name = "plat_pit", section = "TowerEntranceB", x = 80, y = 80, facing = -1, rle = "-*150 L*40 -*170",
                note = "TEB: đứng trên bệ 150 tick rồi đi trái khỏi mép trái bệ khi bệ còn cách bờ ⇒ rơi hố cột 2–3 (x 32–63, trống tới đáy) ⇒ FallDeath Pit", check = CheckPlatPit },
            new Scn { name = "crusher_cycle", section = "TowerHallC", x = 400, y = 144, facing = -1, rle = "-*260",
                note = "Hall C: Simon đứng giữa bẫy 1 và 2 (x 392–407, an toàn) 260 tick: khung 3 bẫy đúng luật đo video (hạ 4 tick/nấc, lên theo lưới 16, giữ 16), chu kỳ 112/80/128, khung tối đa 6/4/7, chung lưới",
                shots = new[] { 0, 9, 25, 41, 60, 100, 140, 180, 220 }, check = CheckCrusherCycle },
            new Scn { name = "crusher_kill", section = "TowerHallC", x = 496, y = 144, facing = -1, rle = "-*140",
                note = "Hall C: Simon đứng ngay dưới bẫy 3 (đang nháy bất tử 94 tick) ⇒ khối gai chạm đầu ở khung 4 (tick rel 4) ⇒ chết ngay (Dead, Died Hazard) — bất tử nháy KHÔNG cứu",
                shots = new[] { 3, 4, 20 }, check = CheckCrusherKill },
            new Scn { name = "crusher_pass", section = "TowerHallC", x = 470, y = 144, facing = -1, rle = "-*160 L*135 -*10",
                note = "Hall C: đứng chỗ an toàn giữa bẫy 2 và 3 (x 462–477), chờ 160 tick rồi đi trái liên tục dưới bẫy 2 + 1 tới x 335 ⇒ không chết (cửa sổ an toàn W 154–167 / 250–263 tính từ luật đo video; video 2:36–2:41 Simon chờ rồi qua, không trúng)",
                shots = new[] { 170, 190, 215, 240 }, check = CheckCrusherPass },
            new Scn { name = "crusher_stopwatch", section = "TowerHallC", x = 470, y = 144, facing = 1, rle = "-*14 Sub -*36 R*20 -*140",
                note = "Hall C: ném đồng hồ khi bẫy 3 đang hạ (CVM3 03: đồng hồ có hiệu lực ngay tick bấm ⇒ bấm rel 14 thay 2) ⇒ 3 bẫy đứng yên (wiki + video 2:40.97 f4829–4922) nhưng gai VẪN giết: Simon đi phải vào bẫy 3 đang treo thấp ⇒ chết Hazard",
                shots = new[] { 30, 45 }, check = CheckStopwatch },
            new Scn { name = "crusher_resume_video", section = "TowerHallC", x = 300, y = 144, facing = 1, rle = "-*28 Sub -*240",
                note = "CVM3 03 — dựng lại video Map_2 f4827–4923: đồng hồ bấm 4 tick sau mốc lưới bẫy ⇒ 3 bẫy đứng 186 tick rồi đổi nấc CÙNG tick ở mốc lưới +192 (video: TIME 0270 giữ 192.3 tick, bẫy 1 k1→k0 + bẫy 2 k0→k1 f4923)",
                shots = new[] { 28, 214, 216 }, check = CheckCrusherResume },
            new Scn { name = "pit_respawn", section = "TowerHallA", x = 304, y = 120, facing = -1, rle = "-*420",
                note = "Hall A + StageRules (đầu STAGE 05 = Hall A (720,80)): thả giữa hố cột 18–19 ⇒ rơi ra đáy ⇒ FallDeath Pit ⇒ LifeLost Fall ⇒ đen ⇒ hồi sinh đầu stage, P −1",
                check = CheckPitRespawn },
            new Scn { name = "pit_knockback", section = "TowerEntranceB", x = 214, y = 80, facing = 1, rle = "-*200",
                note = "TEB: Simon quay phải đứng trên bờ phải khe 160–191 (trống tới đáy) x 214, bia chạm thân từ phải ⇒ knockback lùi trái 34 px (CVM3 04 — trước 12 px từ x 194) ⇒ cuối cung x 180 giữa khe ⇒ rơi hố ⇒ FallDeath Pit (wiki: Medusa Head hất xuống hố)",
                check = CheckKnockbackPit },
            new Scn { name = "enemy_pit", section = "TowerEntranceB", x = 300, y = 80, facing = -1, rle = "-*120",
                note = "TEB: Ghoul thả trên khe 160–191 rơi ra đáy section ⇒ biến mất (Kill, KHÔNG điểm)", check = CheckEnemyPit },
        };
        return scenarios;
    }

    // ---------------------------------------------------------------- ghi
    private struct Rec
    {
        public int sx, sy, sfx, deaths, lives, respawns, hits;
        public eSimonState st;
        public bool carried, dying;
        public int[] pfx, pleft;
        public int[] cf, cu;
        public string section;
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
    private static readonly Dictionary<string, List<string>> traces = new Dictionary<string, List<string>>();
    private static StageWorld world;
    private static int target, recorded, startTick;
    private static Scn cur;
    private static readonly List<Entity> extras = new List<Entity>();
    private static readonly List<MovingPlatform> plats = new List<MovingPlatform>();
    private static readonly List<Crusher> crushers = new List<Crusher>();
    private static SimonController hooked;
    private static int deaths, respawns;
    private static eTileCollision deathKind;
    private static int deathRel = -1, dyingRel = -1;
    private static StageRules addedRules;
    private static bool savedAutoRespawn = true;
    private static int savedStage = -1;
    private static int scoreAtStart;
    private static EnemyBase ghoul;

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    public static string ScenarioNames()
    {
        var sb = new StringBuilder();
        foreach (var s in Scenarios())
            sb.Append(sb.Length > 0 ? "," : "").Append(s.name);
        return sb.ToString();
    }

    public static int RunSuite(string names)
    {
        Unhook();
        suite.Clear();
        SuiteReports.Clear();
        traces.Clear();
        SuiteFails = 0;
        SuiteHash = 2166136261u;
        world = StageWorld.Current;
        savedStage = world != null ? world.Stage : -1;
        string setup = Cvm2TerrainProbe.Setup();
        if (setup.StartsWith("cần") || setup.StartsWith("không"))
        {
            Debug.LogError("[Map2MechProbe] " + setup);
            return -1;
        }
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
        BackToArena();
    }

    public static string SuiteReport()
    {
        var sb = new StringBuilder();
        sb.Append("MECH SUITE ").Append(SuiteDone ? "xong" : "ĐANG CHẠY").Append(", ").Append(SuiteReports.Count).Append(" kịch bản, FAIL ")
          .Append(SuiteFails).Append(", hash bộ ").Append(SuiteHash.ToString("x8")).Append('\n');
        foreach (var r in SuiteReports)
            sb.Append(r).Append('\n');
        return sb.ToString();
    }

    /// <summary>Gỡ section Block 2 + cơ chế, trả thế giới về Arena (48,176) như lúc vào Play.</summary>
    public static void BackToArena()
    {
        world = StageWorld.Current;
        if (world == null || Simon == null)
            return;
        Cleanup();
        Cvm2TerrainProbe.Cleanup();
        if (savedStage >= 0)
            world.Stage = savedStage;
        world.ResetRng();
        GameSession.Current.ResetForNewGame();
        CvgpDebug.CheatInvincible = false;
    }

    /// <summary>Thử tay: dựng section Block 2 (nếu chưa), warp + sinh bệ/bẫy từ marker. Gọi lại = sinh lại từ đầu.</summary>
    public static string Enter(string section, int x, int y)
    {
        world = StageWorld.Current;
        if (world == null || Simon == null)
            return "cần Play scene Arena (Player = Simon)";
        Cvm2TerrainProbe.Setup();
        int n = Go(section, x, y, -1);
        return n < 0 ? "không có section " + section : "đã vào " + section + " (" + x + "," + y + "), sinh " + n + " cơ chế";
    }

    // ---------------------------------------------------------------- dựng

    static int Go(string section, int x, int y, int facing)
    {
        var s = Cvm2TerrainProbe.Find(section);
        if (s == null)
        {
            Debug.LogError("[Map2MechProbe] không có section " + section);
            return -1;
        }
        world.Warp(s, x, y);
        Simon.Teleport(x, y, facing);
        world.ResetRng();
        var pp = UnityEditor.AssetDatabase.LoadAssetAtPath<MovingPlatform>(Map2Mechanisms.PLATFORM_PREFAB);
        var cp = UnityEditor.AssetDatabase.LoadAssetAtPath<Crusher>(Map2Mechanisms.CRUSHER_PREFAB);
        int n = Map2Mechanisms.SpawnFromMarkers(world, pp, cp);
        plats.Clear();
        crushers.Clear();
        foreach (var e in world.GroupList(eEntityGroup.Prop))
        {
            if (e is MovingPlatform) plats.Add((MovingPlatform)e);
            if (e is Crusher) crushers.Add((Crusher)e);
        }
        if (world.CameraCtl != null)
        {
            world.CameraCtl.Target = world.Player;
            world.CameraCtl.SnapToTarget();
        }
        return n;
    }

    static void Cleanup()
    {
        for (int i = 0; i < extras.Count; i++)
            if (extras[i] != null && extras[i].Alive)
                extras[i].Kill(eKillReason.Cleared);
        extras.Clear();
        ghoul = null;
        plats.Clear();
        crushers.Clear();
        if (addedRules != null)
        {
            addedRules.enabled = false;
            UnityEngine.Object.DestroyImmediate(addedRules);
            addedRules = null;
            if (Simon != null)
                Simon.AutoRespawn = savedAutoRespawn;
        }
        if (world != null)
        {
            world.FreezeEnemies(0);
            world.Paused = false;
            world.ClearNonPlayer();
        }
    }

    // ---------------------------------------------------------------- 1 kịch bản

    public static int Run(string scenario)
    {
        world = StageWorld.Current;
        var s = Simon;
        if (world == null || s == null)
        {
            Debug.LogError("[Map2MechProbe] cần scene Arena với Player = SimonController");
            return -1;
        }
        Scn sc = null;
        foreach (var c in Scenarios())
            if (c.name == scenario)
                sc = c;
        if (sc == null)
        {
            Debug.LogError("[Map2MechProbe] kịch bản lạ " + scenario);
            return -1;
        }
        var ticks = NesReplay.Decode(sc.rle);
        Cleanup();
        cur = sc;
        var ses = GameSession.Current;
        ses.ResetForNewGame();
        CvgpDebug.CheatInvincible = false;
        if (Go(sc.section, sc.x, sc.y, sc.facing) < 0)
            return -1;
        s.ResetStats();
        scoreAtStart = ses.score;
        switch (sc.name)
        {
            case "crusher_kill":
                s.InvincibleTicks = s.blinkTicks;   // đang nháy bất tử sau 1 đòn
                s.home = new Vector2Int(560, 144);  // autoRespawn Arena hồi sinh ngoài tầm bẫy (không bị giết lần 2)
                break;
            case "crusher_stopwatch":
            case "crusher_resume_video":
                ses.subWeapon = eSubWeapon.Stopwatch;
                ses.hearts = 20;
                ses.NotifyChanged();
                break;
            case "pit_respawn":
                {
                    savedAutoRespawn = s.AutoRespawn;
                    addedRules = world.gameObject.AddComponent<StageRules>();
                    var hallA = Cvm2TerrainProbe.Find("TowerHallA");
                    addedRules.StageStarts.Add(new StageRules.StageStart { stage = 5, section = hallA, useSectionStart = true });
                    ses.stage = 5;
                    world.Stage = 5;
                    ses.NotifyChanged();
                }
                break;
            case "pit_knockback":
                {
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<ArenaTarget>(TARGET_PREFAB);
                    var t = (ArenaTarget)world.Spawn(prefab, 225, 80, -1);   // CVM3 04: cùng khoảng cách 11 px như trước (Simon 194 / bia 205)
                    t.touchDamage = true;   // pool giữ cờ probe trước (CVM2_MEMORY §6) ⇒ đặt tường minh
                    t.Touch.active = true;
                    extras.Add(t);
                    ses.stage = 4;
                    world.Stage = 4;
                }
                break;
            case "enemy_pit":
                {
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyBase>(GHOUL_PREFAB);
                    ghoul = (EnemyBase)world.Spawn(prefab, 176, 60, -1);
                    extras.Add(ghoul);
                }
                break;
        }
        ses.NotifyChanged();
        NesPad.I.PlayReplay(ticks);
        Begin(sc.name, ticks.Count);
        return ticks.Count;
    }

    private static void Begin(string label, int ticks)
    {
        Unhook();
        Label = label;
        target = ticks;
        recorded = 0;
        Hash = 2166136261u;
        Fails = 0;
        deaths = respawns = 0;
        deathKind = eTileCollision.Empty;
        deathRel = dyingRel = -1;
        recs.Clear();
        checks.Clear();
        measures.Clear();
        traces[label] = new List<string> { "rel,sx,sy,state,section,carried,deaths,p0x,p0fx,p1x,p1fx,c1,c2,c3,u1,u2,u3" };
        startTick = world.TickCount;
        hooked = Simon;
        hooked.Died += OnDied;
        if (addedRules != null)
            addedRules.Respawned += OnRespawned;
        world.TickEnded += OnTickEnded;
        Running = true;
    }

    private static void Unhook()
    {
        if (world != null)
            world.TickEnded -= OnTickEnded;
        if (hooked != null)
        {
            hooked.Died -= OnDied;
            hooked = null;
        }
        if (addedRules != null)
            addedRules.Respawned -= OnRespawned;
        Running = false;
    }

    private static void OnDied(SimonController s, eTileCollision kind)
    {
        deaths++;
        deathKind = kind;
        if (deathRel < 0)
            deathRel = recorded;
    }

    private static void OnRespawned(StageRules r)
    {
        respawns++;
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
        int rel = recorded;
        var r = new Rec
        {
            sx = s.X, sy = s.Y, sfx = s.fx, st = s.Fsm.CurState, dying = s.IsDying, deaths = deaths, lives = GameSession.Current.lives,
            respawns = respawns, hits = s.hitsTaken, section = w.Section != null ? w.Section.name : "",
            pfx = new int[plats.Count], pleft = new int[plats.Count], cf = new int[crushers.Count], cu = new int[crushers.Count],
        };
        if (dyingRel < 0 && r.dying)
            dyingRel = rel;
        for (int i = 0; i < plats.Count; i++)
        {
            var p = plats[i];
            r.pfx[i] = p.Alive ? p.fx : int.MinValue;
            r.pleft[i] = p.Alive ? p.Left : int.MinValue;
            if (p.Alive && p.LastCarryTick == w.TickCount)
                r.carried = true;
            Mix(r.pfx[i]);
        }
        for (int i = 0; i < crushers.Count; i++)
        {
            r.cf[i] = crushers[i].Alive ? crushers[i].Frame : -1;
            r.cu[i] = crushers[i].Alive ? crushers[i].CycleTick : -1;
            Mix(r.cf[i]);
            Mix(r.cu[i]);
        }
        recs.Add(r);
        Mix(rel); Mix(s.fx); Mix(s.fy); Mix((int)r.st); Mix(deaths); Mix((int)deathKind); Mix(r.lives); Mix(respawns); Mix(r.hits);
        if (ghoul != null)
        {
            Mix(ghoul.Alive ? ghoul.fy : -1);
        }
        var tr = traces[cur.name];
        tr.Add(rel + "," + r.sx + "," + r.sy + "," + r.st + "," + r.section + "," + (r.carried ? 1 : 0) + "," + deaths
               + "," + (plats.Count > 0 ? r.pleft[0] + "," + r.pfx[0] : ",") + "," + (plats.Count > 1 ? r.pleft[1] + "," + r.pfx[1] : ",")
               + "," + Cf(r, 0) + "," + Cf(r, 1) + "," + Cf(r, 2) + "," + Cu(r, 0) + "," + Cu(r, 1) + "," + Cu(r, 2));
        Shoot(rel);
        if (++recorded >= target)
            Finish();
    }

    static string Cf(Rec r, int i) { return i < r.cf.Length ? r.cf[i].ToString() : ""; }
    static string Cu(Rec r, int i) { return i < r.cu.Length ? r.cu[i].ToString() : ""; }

    private static void Shoot(int rel)
    {
        if (string.IsNullOrEmpty(ShotDir) || cur.shots == null || Array.IndexOf(cur.shots, rel) < 0)
            return;
        Directory.CreateDirectory(ShotDir);
        ScreenCapture.CaptureScreenshot(Path.Combine(ShotDir, cur.name + "_r" + rel.ToString("000") + ".png"));
    }

    private static void Finish()
    {
        Unhook();
        NesPad.I.StopReplay();
        try
        {
            cur.check();
        }
        catch (Exception ex)
        {
            checks.Add("    FAIL ngoại lệ khi chấm: " + ex.Message);
            Fails++;
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
        if (cur != null)
            sb.Append("  ").Append(cur.note).Append('\n');
        foreach (var m in measures)
            sb.Append("  đo: ").Append(m).Append('\n');
        foreach (var c in checks)
            sb.Append(c).Append('\n');
        return sb.ToString();
    }

    public static void SaveTraces(string dir)
    {
        Directory.CreateDirectory(dir);
        foreach (var kv in traces)
            File.WriteAllLines(Path.Combine(dir, kv.Key + ".csv"), kv.Value.ToArray());
    }

    // ---------------------------------------------------------------- chấm

    static int PlatIndex(string id)
    {
        for (int i = 0; i < plats.Count; i++)
            if (plats[i].LaneId == id)
                return i;
        return -1;
    }

    static PlatV VPlat(string id)
    {
        foreach (var v in VIDEO_PLAT)
            if (v.id == id)
                return v;
        return default(PlatV);
    }

    /// <summary>Sai lệch lớn nhất (px) giữa vị trí bệ Unity và công thức video x = tâm + biên·cos(2π(start+rel+1)/chu kỳ) — bệ bước lần đầu ở rel 0.</summary>
    static double PlatFormulaErr(int pi, PlatV v, out int minLeft, out int maxLeft)
    {
        double worst = 0;
        minLeft = int.MaxValue;
        maxLeft = int.MinValue;
        for (int i = 0; i < recs.Count; i++)
        {
            if (recs[i].pfx[pi] == int.MinValue)
                continue;
            double want = v.center + v.amp * Math.Cos(2 * Math.PI * (v.start + i + 1) / v.period);
            worst = Math.Max(worst, Math.Abs(recs[i].pfx[pi] / 256.0 - want));
            minLeft = Math.Min(minLeft, recs[i].pleft[pi]);
            maxLeft = Math.Max(maxLeft, recs[i].pleft[pi]);
        }
        return worst;
    }

    static void CheckRideTeb()
    {
        int pi = PlatIndex("platform_lane_TEB");
        Expect("có 1 bệ TEB (marker platform_lane_TEB)", 1, plats.Count == 1 && pi == 0 ? 1 : 0);
        if (pi < 0) return;
        int mn, mx;
        double err = PlatFormulaErr(pi, VPlat(plats[pi].LaneId), out mn, out mx);
        Expect("bệ đúng công thức sin video (sai ≤ 0.01 px mọi tick)", 1, err <= 0.01 ? 1 : 0);
        Expect("mép trái bệ thấp nhất = 32 (bờ trái khe — video 32–33)", 32, mn);
        ExpectRange("mép trái bệ cao nhất 63–64 (bờ phải khe — video 63, 64 đúng 1 frame: đỉnh chỉ chạm lúc sinh)", 63, 64, mx);
        int offBad = 0, notIdle = 0, offY = 0, carried = 0;
        int off0 = recs[0].sfx - recs[0].pfx[pi];
        foreach (var r in recs)
        {
            if (r.sfx - r.pfx[pi] != off0) offBad++;
            if (r.st != eSimonState.Idle) notIdle++;
            if (r.sy != 80) offY++;
            if (r.carried) carried++;
        }
        Expect("Simon trôi đúng dx bệ (fx Simon − fx bệ không đổi)", 0, offBad);
        Expect("Simon đứng yên (Idle) suốt", 0, notIdle);
        Expect("Simon giữ mặt bệ y 80", 0, offY);
        ExpectRange("số tick được chở (bệ có dx ≠ 0)", 380, 404, carried);
        Expect("không chết", 0, deaths);
        measures.Add("bệ sai công thức tối đa " + err.ToString("0.0000") + " px, mép trái " + mn + "–" + mx + ", chở " + carried + " tick, Simon − bệ = " + off0 + " fixed");
    }

    static void CheckRideHallB()
    {
        Expect("Hall B có 2 bệ", 2, plats.Count);
        foreach (var id in new[] { "platform_lane_HallB_1", "platform_lane_HallB_2" })
        {
            int pi = PlatIndex(id);
            if (pi < 0) { Expect("có bệ " + id, 1, 0); continue; }
            int mn, mx;
            var v = VPlat(id);
            double err = PlatFormulaErr(pi, v, out mn, out mx);
            Expect(id + ": đúng công thức sin video (≤ 0.01 px)", 1, err <= 0.01 ? 1 : 0);
            Expect(id + ": mép trái thấp nhất (video " + (v.center - 16 - v.amp) + ")", v.center - 16 - v.amp, mn);
            ExpectRange(id + ": mép trái cao nhất (video " + (v.center - 16 + v.amp - 1) + "–" + (v.center - 16 + v.amp) + ")", v.center - 16 + v.amp - 1, v.center - 16 + v.amp, mx);
            measures.Add(id + " sai tối đa " + err.ToString("0.0000") + " px, mép trái " + mn + "–" + mx);
        }
        int p1 = PlatIndex("platform_lane_HallB_1");
        int bad = 0, offY = 0;
        if (p1 >= 0)
        {
            int off0 = recs[0].sfx - recs[0].pfx[p1];
            foreach (var r in recs)
            {
                if (r.sfx - r.pfx[p1] != off0) bad++;
                if (r.sy != 80) offY++;
            }
        }
        Expect("Simon trôi đúng dx bệ 1 suốt 1 chu kỳ", 0, bad);
        Expect("Simon giữ y 80", 0, offY);
        Expect("không chết", 0, deaths);
    }

    static void CheckWalkCross()
    {
        var last = recs[recs.Count - 1];
        int carried = 0, onLane = 0;
        foreach (var r in recs)
        {
            if (r.carried) carried++;
            if (r.sy == 80 && r.sx + 7 < 96 && r.sx - 8 >= 32) onLane++;
        }
        Expect("không chết", 0, deaths);
        Expect("cuối ở mặt bờ trái y 80", 80, last.sy);
        ExpectRange("cuối trên bờ trái (x tâm ≤ 31)", 0, 31, last.sx);
        ExpectRange("được chở khi đứng/đi trên bệ (tick)", 100, 170, carried);
        ExpectRange("đứng trọn trên khe (thân x 32–95, y 80) — chỉ bệ đỡ (tick)", 60, 200, onLane);
        measures.Add("cuối (" + last.sx + "," + last.sy + "), chở " + carried + " tick, thân trọn trên khe " + onLane + " tick");
    }

    static void CheckLand()
    {
        int landRel = -1;
        for (int i = 5; i < recs.Count; i++)
            if (recs[i].sy == 80 && recs[i - 1].st == eSimonState.Jump && recs[i].st != eSimonState.Jump)
            {
                landRel = i;
                break;
            }
        Expect("đáp xuống sau cú nhảy", 1, landRel >= 0 ? 1 : 0);
        if (landRel < 0) return;
        var r = recs[landRel];
        Expect("đáp ở y 80 với thân trọn trên khe (x tâm + 7 < 96 — chỉ bệ đỡ)", 1, r.sy == 80 && r.sx + 7 < 96 ? 1 : 0);
        int pi = PlatIndex("platform_lane_TEB");
        Expect("chỗ đáp nằm trên bệ", 1, pi >= 0 && r.sx + 7 >= r.pleft[pi] && r.sx - 8 < r.pleft[pi] + 32 ? 1 : 0);
        int carriedAfter = 0;
        for (int i = landRel; i < recs.Count; i++)
            if (recs[i].carried) carriedAfter++;
        ExpectRange("được chở sau khi đáp", 10, 999, carriedAfter);
        Expect("không chết", 0, deaths);
        measures.Add("đáp rel " + landRel + " tại (" + r.sx + "," + r.sy + "), bệ mép trái " + (pi >= 0 ? r.pleft[pi] : -1) + ", chở sau đó " + carriedAfter + " tick");
    }

    static void CheckJumpOff()
    {
        int j0 = -1;
        for (int i = 0; i < recs.Count; i++)
            if (recs[i].st == eSimonState.Jump) { j0 = i; break; }
        Expect("có nhảy", 1, j0 >= 0 ? 1 : 0);
        if (j0 < 0) return;
        int x0 = recs[j0].sx, moved = 0, end = j0;
        for (int i = j0; i < recs.Count; i++)
        {
            if (recs[i].st != eSimonState.Jump && recs[i].st != eSimonState.Fall && recs[i].st != eSimonState.Land)
                break;
            if (recs[i].sx != x0) moved++;
            end = i;
        }
        var last = recs[recs.Count - 1];
        Expect("x giữ nguyên suốt nhảy + rơi (không quán tính bệ)", 0, moved);
        Expect("rơi xuống khối hàng 7 y 112 (bệ đã trôi đi — video 0:50.2)", 112, last.sy);
        Expect("không chết", 0, deaths);
        measures.Add("nhảy rel " + j0 + " x " + x0 + " → chạm khối rel " + end + ", cuối (" + last.sx + "," + last.sy + ")");
    }

    static void CheckWhipCrouch()
    {
        int pi = PlatIndex("platform_lane_TEB");
        if (pi < 0) { Expect("có bệ", 1, 0); return; }
        int off0 = recs[0].sfx - recs[0].pfx[pi], bad = 0, atk = 0, crouch = 0, atkCarried = 0, crouchCarried = 0;
        foreach (var r in recs)
        {
            if (r.sfx - r.pfx[pi] != off0) bad++;
            if (r.st == eSimonState.Attack) { atk++; if (r.carried) atkCarried++; }
            if (r.st == eSimonState.Crouch) { crouch++; if (r.carried) crouchCarried++; }
        }
        ExpectRange("có quất roi trên bệ (tick Attack)", 10, 40, atk);
        ExpectRange("có ngồi trên bệ (tick Crouch)", 50, 70, crouch);
        Expect("được chở cả lúc quất", atk, atkCarried);
        Expect("được chở cả lúc ngồi", crouch, crouchCarried);
        Expect("Simon trôi đúng dx bệ suốt (quất/ngồi không làm lệch)", 0, bad);
        Expect("không chết", 0, deaths);
    }

    static void CheckPlatPit()
    {
        int pi = PlatIndex("platform_lane_TEB");
        Expect("rơi hố ⇒ chết 1 lần, loại Pit", 1, deaths == 1 && deathKind == eTileCollision.Pit ? 1 : 0);
        Expect("vào FallDeath (không phải Dead)", 1, CountState(eSimonState.FallDeath) > 0 && CountState(eSimonState.Dead) == 0 ? 1 : 0);
        int leave = -1;
        for (int i = 1; i < recs.Count; i++)
            if (recs[i].sy > 80 && recs[i - 1].sy == 80) { leave = i; break; }
        if (leave >= 0 && pi >= 0)
        {
            var r = recs[leave];
            // CVM3 04: rời bệ khi đầu dò sàn X + 6 (FootProbeHalfWidth — video 9 lần đi khỏi mép) qua mép trái bệ (trước: cả thân X + 7)
            Expect("rời bệ ở phía trái mép bệ, trên hố x 32–63", 1, r.sx + SimonProbe.VIDEO_FOOT < r.pleft[pi] && r.sx - 8 >= 32 - 8 && r.sx + 7 < 64 ? 1 : 0);
            measures.Add("rời bệ rel " + leave + " Simon x " + r.sx + ", bệ mép trái " + r.pleft[pi] + ", chết rel " + deathRel);
        }
    }

    static void CheckCrusherCycle()
    {
        Expect("Hall C có 3 bẫy", 3, crushers.Count);
        Expect("không chết (đứng giữa bẫy 1 và 2)", 0, deaths);
        int[] idx = new int[3];
        for (int c = 0; c < crushers.Count && c < 3; c++)
        {
            CrushV v = default(CrushV);
            foreach (var vv in VIDEO_CRUSH) if (vv.id == crushers[c].MarkerId) v = vv;
            int bad = 0, maxK = 0;
            var tr = new List<int>();   // tick rel đổi khung đi lên / bắt đầu hạ (lưới 16)
            for (int i = 0; i < recs.Count; i++)
            {
                int want = VideoFrame(v, v.start + i + 1);
                if (recs[i].cf[c] != want) bad++;
                maxK = Math.Max(maxK, recs[i].cf[c]);
                if (i > 0 && (recs[i].cf[c] < recs[i - 1].cf[c] && recs[i - 1].cf[c] != v.maxFrame || (recs[i - 1].cf[c] == 0 && recs[i].cf[c] == 1)))
                    tr.Add(i);
            }
            Expect(v.id + ": khung từng tick đúng luật video", 0, bad);
            Expect(v.id + ": khung tối đa", v.maxFrame, maxK);
            Expect(v.id + ": chu kỳ (tick)", v.cycle, crushers[c].CycleTicks);
            int offGrid = 0;
            int g0 = tr.Count > 0 ? tr[0] % VIDEO_GRID : -1;
            foreach (var t in tr) if (t % VIDEO_GRID != g0) offGrid++;
            Expect(v.id + ": mọi bước lên/bắt đầu hạ nằm trên lưới 16 tick", 0, offGrid);
            idx[c] = g0;
            measures.Add(v.id + ": khung tối đa " + maxK + ", chu kỳ " + crushers[c].CycleTicks + ", " + tr.Count + " bước lưới (pha " + g0 + ")");
        }
        Expect("3 bẫy CHUNG lưới (cùng pha mod 16 — video f4827/f4923 đổi nấc cùng frame)", 1, idx[0] == idx[1] && idx[1] == idx[2] ? 1 : 0);
    }

    static void CheckCrusherKill()
    {
        Expect("chết 1 lần, loại Hazard", 1, deaths == 1 && deathKind == eTileCollision.Hazard ? 1 : 0);
        Expect("vào Dead (chết tại chỗ, không rơi)", 1, CountState(eSimonState.Dead) > 0 && CountState(eSimonState.FallDeath) == 0 ? 1 : 0);
        Expect("bắt đầu chết đúng tick khối gai chạm đầu (rel 4: bẫy 3 u 7+5 = 12 ⇒ khung 4, đáy y 120 > đỉnh hộp nhận 114)", 4, dyingRel);
        int c3 = -1;
        for (int c = 0; c < crushers.Count; c++) if (crushers[c].MarkerId == "crusher_3") c3 = c;
        Expect("bẫy 3 đếm 1 lần giết", 1, c3 >= 0 ? crushers[c3].Kills : -1);
        Expect("bất tử nháy không cứu (đã đặt InvincibleTicks 94 lúc đầu)", 1, dyingRel >= 0 && dyingRel < 94 ? 1 : 0);
        measures.Add("chết rel " + dyingRel + " (Died rel " + deathRel + ")" + (c3 >= 0 && dyingRel >= 0 ? ", khung bẫy 3 lúc đó " + recs[dyingRel].cf[c3] : ""));
    }

    static void CheckCrusherPass()
    {
        var last = recs[recs.Count - 1];
        Expect("không chết", 0, deaths);
        Expect("đi hết tới x 335", 335, last.sx);
        Expect("trên sàn y 144", 144, last.sy);
        int minGap = int.MaxValue;
        for (int i = 0; i < recs.Count; i++)
            for (int c = 0; c < crushers.Count; c++)
            {
                var hx0 = recs[i].sx - 8; var hx1 = recs[i].sx + 8;
                int cx = crushers[c].X;
                if (hx1 > cx - 16 && hx0 < cx + 16)
                    minGap = Math.Min(minGap, 114 - (80 + 8 * recs[i].cf[c] + 8));
            }
        measures.Add("đi qua 3 bẫy, khoảng hở nhỏ nhất đầu Simon ↔ đáy gai " + minGap + " px");
    }

    static void CheckStopwatch()
    {
        int f0 = -1, f1 = -1;
        for (int i = 1; i < recs.Count; i++)
        {
            bool same = true;
            for (int c = 0; c < crushers.Count; c++)
                if (recs[i].cu[c] != recs[i - 1].cu[c]) same = false;
            if (same && f0 < 0) f0 = i;
            if (!same && f0 >= 0 && f1 < 0) f1 = i;
        }
        Expect("đồng hồ đóng băng cả 3 bẫy (chu kỳ đứng yên)", 1, f0 > 0 ? 1 : 0);
        int k3 = -1, c3 = -1;
        for (int c = 0; c < crushers.Count; c++) if (crushers[c].MarkerId == "crusher_3") c3 = c;
        if (f0 > 0 && c3 >= 0)
            k3 = recs[f0].cf[c3];
        ExpectRange("bẫy 3 đứng ở khung thấp (≥ 4 — đủ chạm đầu)", 4, 7, k3);
        Expect("gai đóng băng VẪN giết khi đi vào: chết Hazard", 1, deaths >= 1 && deathKind == eTileCollision.Hazard ? 1 : 0);
        Expect("chết trong lúc bẫy đang đóng băng", 1, dyingRel > f0 && (f1 < 0 || dyingRel < f1) ? 1 : 0);
        measures.Add("đóng băng từ rel " + f0 + (f1 > 0 ? " tới " + f1 + " (" + (f1 - f0) + " tick)" : " tới hết bản ghi") + ", bẫy 3 khung " + k3 + ", chết rel " + dyingRel);
    }

    /// <summary>CVM3 03: mốc lưới bẫy g0 (u ≡ 0 mod lưới) ngay trước lần bấm; bẫy đổi khung lần đầu sau đồng hồ phải ở g0 + 192 (video f4923), cả 3 cùng tick.</summary>
    static void CheckCrusherResume()
    {
        const int VIDEO_RESUME = 192, VIDEO_PRESS = 4, STOPWATCH = 186;
        int press = -1;
        for (int i = 1; i < recs.Count && press < 0; i++)
            if (recs[i].cu[0] == recs[i - 1].cu[0])
                press = i - 1;   // rel cuối còn chạy (tick bấm — đóng băng từ tick sau)
        int g0 = -1;
        for (int i = press; i >= 0 && g0 < 0; i--)
            if (recs[i].cu[0] % 16 == 0)
                g0 = i;
        var first = new int[crushers.Count];
        for (int c = 0; c < crushers.Count; c++)
        {
            first[c] = -1;
            for (int i = press + 2; i < recs.Count && first[c] < 0; i++)
                if (recs[i].cf[c] != recs[i - 1].cf[c])
                    first[c] = i;
        }
        Expect("bấm 4 tick sau mốc lưới (như video f4827 → f4829)", VIDEO_PRESS, press - g0);
        bool same = true;
        for (int c = 1; c < crushers.Count; c++) same &= first[c] == first[0];
        Expect("3 bẫy đổi nấc lại CÙNG tick", 1, same ? 1 : 0);
        ExpectRange("đổi nấc lại ở mốc lưới +192 sau mốc trước (video 192.3 ±2)", VIDEO_RESUME - 2, VIDEO_RESUME + 2, first[0] - g0);
        Expect("đổi nấc lại đúng mốc lưới (u ≡ 0 mod 16)", 0, first[0] >= 0 ? recs[first[0]].cu[0] % 16 : -1);
        ExpectRange("không đổi nấc trong lúc đóng băng (186 tick)", press + STOPWATCH + 1, 99999, first[0]);
        measures.Add("mốc lưới rel " + g0 + ", bấm rel " + press + ", đổi nấc lại rel " + string.Join("/", first) + " (= mốc + " + (first[0] - g0) + "; video +192.3)");
    }

    static void CheckPitRespawn()
    {
        Expect("rơi hố ⇒ Died Pit 1 lần", 1, deaths == 1 && deathKind == eTileCollision.Pit ? 1 : 0);
        Expect("StageRules hồi sinh 1 lần", 1, respawns);
        var last = recs[recs.Count - 1];
        Expect("mất 1 mạng (P 3 → 2)", 2, last.lives);
        Expect("hồi sinh ở đầu STAGE 05: section TowerHallA", 1, last.section == "TowerHallA" ? 1 : 0);
        Expect("hồi sinh tại simonStart (720,80)", 720 * 1000 + 80, last.sx * 1000 + last.sy);
        if (addedRules != null)
            Expect("nguyên nhân LifeLost = Fall", 1, addedRules.LastDeathCause == eDeathCause.Fall ? 1 : 0);
        measures.Add("Died rel " + deathRel + ", cuối " + last.section + " (" + last.sx + "," + last.sy + ") P-" + last.lives);
    }

    static void CheckKnockbackPit()
    {
        var first = -1;
        for (int i = 0; i < recs.Count; i++) if (recs[i].hits > 0) { first = i; break; }
        Expect("bị bia chạm 1 đòn", 1, first >= 0 ? 1 : 0);
        Expect("vào Hurt (knockback)", 1, CountState(eSimonState.Hurt) > 0 ? 1 : 0);
        Expect("knockback hất xuống hố ⇒ Died Pit", 1, deaths >= 1 && deathKind == eTileCollision.Pit ? 1 : 0);
        measures.Add("đòn rel " + first + ", chết rel " + deathRel + " (" + deathKind + ")");
    }

    static void CheckEnemyPit()
    {
        Expect("Ghoul rơi khỏi đáy ⇒ bị gỡ", 1, ghoul != null && !ghoul.Alive ? 1 : 0);
        Expect("không cộng điểm (rơi hố không phải bị hạ)", scoreAtStart, GameSession.Current.score);
        Expect("Simon không sao", 0, deaths);
    }

    // ---------------------------------------------------------------- tiện ích

    static int CountState(eSimonState s)
    {
        int n = 0;
        foreach (var r in recs) if (r.st == s) n++;
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
