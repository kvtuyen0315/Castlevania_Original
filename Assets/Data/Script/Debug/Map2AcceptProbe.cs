#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Harness NGHIỆM THU Map 2 (CVM2 12) — CHỈ Editor, scene Main.unity khi Play. Chỉ ĐO, không đổi gameplay/hash. Qua MCP:
///   StartTelemetry(path) / StopTelemetry() ⇒ ghi vết THỤ ĐỘNG (không tiêm input, không đổi trạng thái) mọi tick của phần Map 2 (stage ≥ 4):
///        W = cuối tick thế giới (StageWorld.TickEnded): màn, section, cx, khoá camera, Simon, HUD; E = thực thể động (enemy, đạn, item,
///        vũ khí phụ, bệ, bẫy ép); A = vật tĩnh hiện (nến, khối); S = màn khác (bản đồ, đen, STAGE 07); F/M/N/K/B/C/P/U/L/R/X = sự kiện
///        (mốc luồng, nhạc, sinh, hạ, boss, quả cầu, nhặt, 1UP, mất mạng, hồi sinh, khối vỡ). Chạy kèm Map1AcceptProbe.StartFlow (luồng thật).
///   Run(names, outPath)  ⇒ kịch bản KHÔNG bất tử trên map thật (StageScreen thật, NesClock tắt — tự Step như Map2RouteProbe):
///        pit_teb, pit_hallb, crusher_hallc, gameover_st6, damage_halla, knock_pit_hallb — chết hố/gai ⇒ hồi sinh đầu stage, Game Over ⇒
///        Continue, sát thương 2 vạch, knockback hất xuống hố. Ghi báo cáo + vết.
///   Ledger(ledgerFile, outPath) ⇒ "sổ điểm" video Map_2: đặt trạng thái đầu Map_2 (SCORE 23370, P3, tim 5, rìu II, roi 3) rồi áp từng sự kiện
///        HUD của video qua ĐÚNG đường code game (GameRules.AddScore với điểm Config như EnemyBase/BossBase, ItemEffects.Apply, giá tim
///        ConfigSubWeapon, EnterStage, StageClearTally Block 2) — so HUD sau mỗi sự kiện với video; tally cuối phải ra 44670.
/// </summary>
public static class Map2AcceptProbe
{
    // ================================================================ 1) vết thụ động

    public static bool TelemetryOn { get; private set; }
    public static int TelemetryLines { get; private set; }

    private static StringBuilder tsb;
    private static string tpath;
    private static StageWorld boundWorld;
    private static StageRules boundRules;
    private static int lastWorldFrame = -1;
    private static readonly Dictionary<int, int> lastSeen = new Dictionary<int, int>();   // instanceID ⇒ SpawnTick đã ghi A
    private static readonly HashSet<string> staticSeen = new HashSet<string>();

    private static StageWorld W => StageWorld.Current;
    private static SimonController S => W != null ? W.Player as SimonController : null;
    private static string Sec => W != null && W.Section != null ? W.Section.name : "";
    private static int Cx => W != null && W.CameraCtl != null ? W.CameraCtl.X : -1;
    private static bool InStage => GameManager.I != null && GameManager.I.State == eGameScreen.Stage1 && !GameManager.I.IsTransitioning && W != null;

    public static string StartTelemetry(string path)
    {
        if (!EditorApplication.isPlaying || GameManager.I == null)
            return "cần Play Main.unity";
        if (TelemetryOn)
            return "đang ghi";
        tsb = new StringBuilder(1 << 22);
        tpath = path;
        TelemetryLines = 0;
        lastWorldFrame = -1;
        lastSeen.Clear();
        staticSeen.Clear();
        boundWorld = null;
        boundRules = null;
        Line("#\tW frame wt sec cx locked sx sy sfx sfy state facing inv score time hearts stage lives hp ehp sub mult screen trans");
        Line("#\tE frame wt key type x y fx fy facing state extra");
        NesClock.OnTick += OnClockTick;
        StageFlow.Milestone += OnMilestone;
        AudioManager.MusicEvent += OnMusic;
        EnemySpawner.Spawned += OnSpawned;
        EnemyBase.Defeated += OnDefeated;
        BossBase.BossDefeated += OnBossDefeated;
        BossBase.CrystalSpawned += OnCrystal;
        ItemEffects.Picked += OnPicked;
        GameRules.OneUp += OnOneUp;
        BreakableBlock.Broken += OnBlock;
        TelemetryOn = true;
        return "ghi vết → " + path;
    }

    public static string StopTelemetry()
    {
        if (!TelemetryOn)
            return "không ghi";
        NesClock.OnTick -= OnClockTick;
        StageFlow.Milestone -= OnMilestone;
        AudioManager.MusicEvent -= OnMusic;
        EnemySpawner.Spawned -= OnSpawned;
        EnemyBase.Defeated -= OnDefeated;
        BossBase.BossDefeated -= OnBossDefeated;
        BossBase.CrystalSpawned -= OnCrystal;
        ItemEffects.Picked -= OnPicked;
        GameRules.OneUp -= OnOneUp;
        BreakableBlock.Broken -= OnBlock;
        Bind(null);
        TelemetryOn = false;
        Directory.CreateDirectory(Path.GetDirectoryName(tpath));
        File.WriteAllText(tpath, tsb.ToString(), new UTF8Encoding(false));
        string r = tpath + " (" + TelemetryLines + " dòng)";
        tsb = null;
        return r;
    }

    /// <summary>Phần Map 2 = stage ≥ 4 (bản đồ lâu đài sau Block 1 đã là stage 4 — đầu video Map_2). CVM3 12: đặt 7 ⇒ chỉ ghi phần Map 3.</summary>
    public static int MinStage = 4;
    private static bool Active => GameSession.Current != null && GameSession.Current.stage >= MinStage;

    private static void Line(string s)
    {
        tsb.Append(s).Append('\n');
        TelemetryLines++;
    }

    private static void Bind(StageWorld w)
    {
        if (boundWorld != null)
            boundWorld.TickEnded -= OnWorldTickEnded;
        boundWorld = w;
        if (boundWorld != null)
            boundWorld.TickEnded += OnWorldTickEnded;
    }

    private static void BindRules(StageRules r)
    {
        if (boundRules != null)
        {
            boundRules.LifeLost -= OnLifeLost;
            boundRules.Respawned -= OnRespawned;
        }
        boundRules = r;
        if (boundRules != null)
        {
            boundRules.LifeLost += OnLifeLost;
            boundRules.Respawned += OnRespawned;
        }
    }

    private static void OnClockTick(int frame)
    {
        if (!TelemetryOn)
            return;
        if (StageWorld.Current != boundWorld)
            Bind(StageWorld.Current);
        if (StageRules.Current != boundRules)
            BindRules(StageRules.Current);
        if (!Active)
            return;
        var gm = GameManager.I;
        // màn chơi đang chạy ⇒ dòng W do TickEnded ghi; còn lại (bản đồ, đen, STAGE 07, GAME OVER) ghi S ở đây
        if (gm != null && gm.State == eGameScreen.Stage1 && !gm.IsTransitioning)
            return;
        Line("S\t" + frame + "\t" + Hud() + "\t" + (gm != null ? gm.State.ToString() : "?") + "\t" + (gm != null && gm.IsTransitioning ? 1 : 0));
    }

    private static string Hud()
    {
        var s = GameSession.Current;
        return s.score + "\t" + s.time + "\t" + s.hearts + "\t" + s.stage + "\t" + s.lives + "\t" + s.playerHp + "\t" + s.enemyHp + "\t" + s.subWeapon + "\t" + s.shotMultiplier;
    }

    private static void OnWorldTickEnded(StageWorld w)
    {
        if (!TelemetryOn || !Active || w != StageWorld.Current)
            return;
        int frame = NesClock.Frame;
        lastWorldFrame = frame;
        var gm = GameManager.I;
        var simon = w.Player as SimonController;
        var cam = w.CameraCtl;
        string sim = simon != null
            ? simon.X + "\t" + simon.Y + "\t" + simon.fx + "\t" + simon.fy + "\t" + (simon.Fsm != null && simon.Fsm.HasState ? simon.Fsm.CurState.ToString() : "-") + "\t" + simon.Facing + "\t" + simon.InvincibleTicks
            : "-\t-\t-\t-\t-\t-\t-";
        Line("W\t" + frame + "\t" + w.TickCount + "\t" + (w.Section != null ? w.Section.name : "-") + "\t" + (cam != null ? cam.X : -1) + "\t" + (cam != null && cam.Locked ? 1 : 0)
             + "\t" + sim + "\t" + Hud() + "\t" + (gm != null ? gm.State.ToString() : "?") + "\t" + (gm != null && gm.IsTransitioning ? 1 : 0));
        for (int g = 1; g < Entity.GROUP_COUNT; g++)
        {
            if (g == (int)eEntityGroup.Effect)
                continue;
            var list = w.GroupList(g);
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null || !e.Alive)
                    continue;
                string type = e.GetType().Name;
                bool dynamicProp = e is MovingPlatform || e is Crusher;
                if (g == (int)eEntityGroup.Prop && !dynamicProp)
                {
                    string key = e.GetInstanceID() + "_" + e.SpawnTick;
                    if (staticSeen.Add(key))
                        Line("A\t" + frame + "\t" + w.TickCount + "\t" + key + "\t" + type + "\t" + e.X + "\t" + e.Y + "\t" + Sec + "\t" + Cx + "\t" + PropId(e));
                    continue;
                }
                string extra = "";
                var plat = e as MovingPlatform;
                if (plat != null)
                    extra = "left=" + plat.Left + " lane=" + plat.LaneId;
                var cr = e as Crusher;
                if (cr != null)
                    extra = "frame=" + cr.Frame + " u=" + cr.CycleTick + " id=" + cr.MarkerId;
                var boss = e as BossBase;
                if (boss != null)
                    extra = "dormant=" + (boss.dormant ? 1 : 0) + " dying=" + (boss.dying ? 1 : 0) + " defeated=" + (boss.defeated ? 1 : 0)
                            + " sprite=" + (e.View != null && e.View.sprite != null ? e.View.sprite.name : "-");
                var it = e as ItemPickup;
                if (it != null)
                    extra = "item=" + it.id;
                Line("E\t" + frame + "\t" + w.TickCount + "\t" + e.GetInstanceID() + "_" + e.SpawnTick + "\t" + type + "\t" + e.X + "\t" + e.Y + "\t" + e.fx + "\t" + e.fy
                     + "\t" + e.Facing + "\t" + e.DebugState + "\t" + extra);
            }
        }
    }

    private static string PropId(Entity e)
    {
        var c = e as CandleProp;
        if (c != null)
            return c.objectId;
        var b = e as BreakableBlock;
        if (b != null)
            return b.ObjectId;
        return "-";
    }

    private static string Where()
    {
        var w = W;
        return NesClock.Frame + "\t" + (w != null ? w.TickCount : -1) + "\t" + Sec + "\t" + Cx + "\t" + (S != null ? S.X + "," + S.Y : "-") + "\t" + GameSession.Current.score;
    }

    private static void Ev(string kind, string m)
    {
        if (TelemetryOn && Active)
            Line(kind + "\t" + Where() + "\t" + m);
    }

    private static void OnMilestone(StageFlow f, string m) { Ev("F", m); }
    private static void OnMusic(string what, eMusic id) { Ev("M", what + " " + id); }
    private static void OnSpawned(EnemySpawner sp, EnemyBase e) { Ev("N", (e != null ? e.GetType().Name + " " + e.GetInstanceID() + "_" + e.SpawnTick + " (" + e.X + "," + e.Y + ") f" + e.Facing : "?") + " từ " + (sp != null ? sp.name : "?")); }
    private static void OnDefeated(EnemyBase e) { Ev("K", e.GetType().Name + " " + e.GetInstanceID() + "_" + e.SpawnTick + " (" + e.X + "," + e.Y + ") +" + e.Score); }
    private static void OnBossDefeated(BossBase b) { Ev("B", b.GetType().Name + " (" + b.X + "," + b.Y + ") +" + b.Score); }
    private static void OnCrystal(BossBase b, ItemPickup p) { Ev("C", p != null ? p.X + "," + p.Y : "?"); }
    private static void OnPicked(eItemId id, string log) { Ev("P", id + " | " + log); }
    private static void OnOneUp(int lives) { Ev("U", "P-" + lives); }
    private static void OnBlock(BreakableBlock b) { Ev("X", b != null ? b.ObjectId : "?"); }
    private static void OnLifeLost(eDeathCause c, bool more) { Ev("L", c + (more ? " còn mạng" : " GAME OVER")); }
    private static void OnRespawned(StageRules r) { Ev("R", "hồi sinh"); }

    // ================================================================ 2) kịch bản trên map thật (không bất tử)

    public static bool Running { get; private set; }
    public static bool Finished { get; private set; }
    public static string Error { get; private set; } = "";
    private static readonly StringBuilder rep = new StringBuilder();
    private static readonly List<string> trace = new List<string>();
    private static readonly List<string> consoleErrors = new List<string>();
    private static Queue<string> queue;
    private static string outFile;
    private static MethodInfo stepMethod;
    private static bool prevInvincible;
    private static int passCount, failCount;

    private class Scn
    {
        public string name;
        public int stage, lives, maxTicks = 3000;
        public string warpSec;
        public int wx, wy, wf;
        public Func<int, int> input;   // tuổi (tick từ lúc sẵn sàng) ⇒ mask
        public Func<string> check;     // null = đạt; chuỗi = lý do trượt
        public Func<bool> done;        // xong sớm
    }

    private static Scn cur;
    private static int age, ready, gameOverAge;
    // ghi trong lúc chạy
    private static readonly List<string> deaths = new List<string>();
    private static readonly List<string> respawns = new List<string>();
    private static readonly List<int> hpDrops = new List<int>();
    private static readonly List<string> hits = new List<string>();
    private static bool sawGameOver, continued;
    private static int lastHp, lastScreenFrame;
    private static eGameScreen lastScreen;
    private static string afterRespawn = "", afterContinue = "";
    private static int kbStartX = int.MinValue, kbMaxDx, lastDropAge = -1;
    private static readonly List<int> kbs = new List<int>();

    const int U = 1 << (int)eNesButton.Up, D = 1 << (int)eNesButton.Down, L = 1 << (int)eNesButton.Left, R = 1 << (int)eNesButton.Right,
              A = 1 << (int)eNesButton.A, B = 1 << (int)eNesButton.B;

    public static string ScenarioNames() => "pit_teb,pit_hallb,crusher_hallc,gameover_st6,damage_halla,knock_pit_hallb";

    private static Scn Make(string n)
    {
        switch (n)
        {
            case "pit_teb":
                // TEB khe 2 [160,192) ở tầng y 80 (CVM2 05 marker pit): đứng x 196 đi trái ⇒ rơi ⇒ hồi sinh đầu STAGE 04 = TEA (64,176) phải
                return new Scn { name = n, stage = 4, lives = 3, warpSec = "TowerEntranceB", wx = 196, wy = 80, wf = -1, input = a => L,
                    check = () => Expect(eDeathCause.Fall, "TowerEntranceA", 64, 176, 1, 0, 4, 2) };
            case "pit_hallb":
                // Hall B khe cạnh chân thang [128,160) ở y 160: đứng x 124 đi phải ⇒ rơi ⇒ đầu STAGE 05 = Hall A (720,80) trái cx 512
                return new Scn { name = n, stage = 5, lives = 3, warpSec = "TowerHallB", wx = 124, wy = 160, wf = 1, input = a => R,
                    check = () => Expect(eDeathCause.Fall, "TowerHallA", 720, 80, -1, 512, 5, 2) };
            case "crusher_hallc":
                // Hall C bẫy ép 3 (x 480–511, tối đa khung 7): đứng yên dưới bẫy ở sàn y 144 ⇒ gai ⇒ chết ngay ⇒ đầu STAGE 06 = Hall C (720,112) trái
                return new Scn { name = n, stage = 6, lives = 3, warpSec = "TowerHallC", wx = 496, wy = 144, wf = -1, input = a => 0,
                    check = () => Expect(eDeathCause.Hazard, "TowerHallC", 720, 112, -1, 512, 6, 2) };
            case "gameover_st6":
                // P 1 + gai ⇒ GAME OVER ⇒ Start (CONTINUE) ⇒ đầu STAGE 06, P 3, điểm 0
                return new Scn { name = n, stage = 6, lives = 1, warpSec = "TowerHallC", wx = 496, wy = 144, wf = -1, input = a => 0, maxTicks = 2500,
                    check = CheckGameOver };
            case "damage_halla":
                // đứng yên ở đầu STAGE 05 (Hall A 720,80 quay trái) 1500 tick: Medusa Head trúng ⇒ mỗi lần −2 vạch (ConfigBlock block 2);
                // 8 đòn = 16 vạch ⇒ chết (Damage) ⇒ hồi sinh đầu STAGE 05
                return new Scn { name = n, stage = 5, lives = 3, input = a => 0, maxTicks = 1500, check = CheckDamage };
            case "knock_pit_hallb":
                // Hall B tầng dưới x 176 (bờ phải khe [128,160)), quay PHẢI: Medusa Head từ phía trước ⇒ knockback lùi TRÁI 34 px ⇒ cuối cung x 142 giữa khe
                // ⇒ rơi hố (CVM3 04: trước đứng x 164 — knockback 34 px nay bay QUA khe 32 px, đáp bờ trái)
                return new Scn { name = n, stage = 5, lives = 3, warpSec = "TowerHallB", wx = 176, wy = 160, wf = 1, input = a => 0, maxTicks = 2000,
                    check = () =>
                    {
                        string e = Expect(eDeathCause.Fall, "TowerHallA", 720, 80, -1, 512, 5, 2);
                        if (hits.Count == 0)
                            e = (e == null ? "" : e + "; ") + "không có đòn trúng trước khi rơi";
                        return e;
                    } };
        }
        return null;
    }

    public static string Run(string names, string outPath)
    {
        if (!EditorApplication.isPlaying || GameManager.I == null || NesClock.I == null || NesPad.I == null)
            return "cần Play Main.unity";
        if (Running || Map1AcceptProbe.Running || Map2RouteProbe.Running)
            return "đang chạy";
        queue = new Queue<string>(names.Split(','));
        outFile = outPath;
        rep.Length = 0;
        trace.Clear();
        consoleErrors.Clear();
        passCount = failCount = 0;
        Error = "";
        Finished = false;
        prevInvincible = CvgpDebug.CheatInvincible;
        CvgpDebug.CheatInvincible = false;
        EditorPrefs.SetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, false);
        stepMethod = typeof(NesClock).GetMethod("Step", BindingFlags.Instance | BindingFlags.NonPublic);
        NesClock.I.enabled = false;
        Application.logMessageReceived += OnLog;
        Running = true;
        EditorApplication.update += Pump;
        Next();
        return "chạy " + names;
    }

    private static void OnLog(string msg, string stack, LogType type)
    {
        if ((type == LogType.Error || type == LogType.Exception || type == LogType.Assert) && consoleErrors.Count < 100)
            consoleErrors.Add("[" + type + "] " + (cur != null ? cur.name : "-") + " " + msg);
    }

    private static void Next()
    {
        UnbindScn();
        if (queue.Count == 0)
        {
            Finish(null);
            return;
        }
        string n = queue.Dequeue().Trim();
        cur = Make(n);
        if (cur == null)
        {
            rep.Append("?? kịch bản lạ ").Append(n).Append('\n');
            Next();
            return;
        }
        age = 0;
        ready = -1;
        deaths.Clear();
        respawns.Clear();
        hpDrops.Clear();
        hits.Clear();
        sawGameOver = continued = false;
        afterRespawn = afterContinue = "";
        kbStartX = int.MinValue;
        kbMaxDx = 0;
        lastDropAge = -1;
        kbs.Clear();
        gameOverAge = 0;
        GameRules.NewGame();
        var s = GameSession.Current;
        s.stage = cur.stage;
        s.block = GameRules.BlockOfStage(cur.stage);
        s.time = GameRules.TimeLimitOfStage(cur.stage);
        s.lives = cur.lives;
        s.NotifyChanged();
        if (!GameManager.I.GoTo(eGameScreen.Stage1, 0))
        {
            Finish("GameManager đang chuyển cảnh");
            return;
        }
        lastScreen = GameManager.I.State;
        trace.Add("== " + cur.name);
    }

    private static void BindScn()
    {
        var r = StageRules.Current;
        if (r != null && r != boundScnRules)
        {
            UnbindScn();
            boundScnRules = r;
            r.LifeLost += ScnLifeLost;
            r.Respawned += ScnRespawned;
        }
        var w = StageWorld.Current;
        if (w != null && w != boundScnWorld)
        {
            if (boundScnWorld != null)
                boundScnWorld.Hit -= ScnHit;
            boundScnWorld = w;
            w.Hit += ScnHit;
        }
    }

    private static StageRules boundScnRules;
    private static StageWorld boundScnWorld;

    private static void UnbindScn()
    {
        if (boundScnRules != null)
        {
            boundScnRules.LifeLost -= ScnLifeLost;
            boundScnRules.Respawned -= ScnRespawned;
            boundScnRules = null;
        }
        if (boundScnWorld != null)
        {
            boundScnWorld.Hit -= ScnHit;
            boundScnWorld = null;
        }
    }

    private static void ScnLifeLost(eDeathCause c, bool more)
    {
        deaths.Add(c + (more ? "" : " GAMEOVER") + " @" + age + " (" + (S != null ? S.X + "," + S.Y : "?") + ") P" + GameSession.Current.lives);
    }

    private static void ScnRespawned(StageRules r)
    {
        respawns.Add("@" + age);
        afterRespawn = State();
    }

    private static void ScnHit(HitInfo h)
    {
        if (S != null && h.victim == S)
            hits.Add("@" + age + " dmg " + h.damage + " từ " + (h.attacker != null ? h.attacker.GetType().Name : "?") + " Simon (" + S.X + "," + S.Y + ")");
    }

    private static string State()
    {
        var s = GameSession.Current;
        return Sec + " (" + (S != null ? S.X + "," + S.Y : "?") + ") f" + (S != null ? S.Facing : 0) + " cx " + Cx + " ST" + s.stage + " P" + s.lives + " SC" + s.score
               + " T" + s.time + " ♥" + s.hearts + " HP" + s.playerHp + " roi" + s.whipLevel + " " + s.subWeapon;
    }

    /// <summary>Kỳ vọng chết + hồi sinh: nguyên nhân, section/vị trí/hướng/cx đầu stage, stage, mạng sau.</summary>
    private static string Expect(eDeathCause cause, string sec, int x, int y, int facing, int cx, int stage, int lives)
    {
        var errs = new List<string>();
        if (deaths.Count == 0)
            errs.Add("không chết");
        else if (!deaths[0].StartsWith(cause.ToString()))
            errs.Add("chết vì " + deaths[0] + " ≠ " + cause);
        if (respawns.Count == 0)
            errs.Add("không hồi sinh");
        string want = sec + " (" + x + "," + y + ") f" + facing + " cx " + cx + " ST" + stage + " P" + lives;
        if (!afterRespawn.StartsWith(want))
            errs.Add("sau hồi sinh '" + afterRespawn + "' ≠ '" + want + " …'");
        if (afterRespawn.IndexOf(" T400 ", StringComparison.Ordinal) < 0)
            errs.Add("TIME sau hồi sinh ≠ 400");
        return errs.Count == 0 ? null : string.Join("; ", errs.ToArray());
    }

    private static string CheckGameOver()
    {
        var errs = new List<string>();
        if (deaths.Count == 0 || deaths[0].IndexOf("GAMEOVER", StringComparison.Ordinal) < 0 || !deaths[0].StartsWith("Hazard"))
            errs.Add("chết '" + (deaths.Count > 0 ? deaths[0] : "-") + "' ≠ Hazard GAMEOVER");
        if (!sawGameOver)
            errs.Add("không thấy màn GAME OVER");
        if (!continued)
            errs.Add("không CONTINUE");
        string want = "TowerHallC (720,112) f-1 cx 512 ST6 P3 SC0 T400";
        if (!afterContinue.StartsWith(want))
            errs.Add("sau CONTINUE '" + afterContinue + "' ≠ '" + want + " …'");
        return errs.Count == 0 ? null : string.Join("; ", errs.ToArray());
    }

    private static string CheckDamage()
    {
        var errs = new List<string>();
        if (hpDrops.Count == 0)
            errs.Add("không trúng đòn nào");
        int sum = 0;
        foreach (int d in hpDrops)
        {
            sum += d;
            if (d != 2)
                errs.Add("mất " + d + " vạch ≠ 2");
        }
        // hết 16 vạch (8 đòn) ⇒ chết Damage ⇒ hồi sinh đầu STAGE 05 — đúng luật; chết sớm hơn/khác nguyên nhân ⇒ sai
        if (deaths.Count > 0)
        {
            if (sum < GameSession.MAX_HP)
                errs.Add("chết khi mới mất " + sum + " vạch: " + deaths[0]);
            string e = Expect(eDeathCause.Damage, "TowerHallA", 720, 80, -1, 512, 5, 2);
            if (e != null)
                errs.Add(e);
        }
        return errs.Count == 0 ? null : string.Join("; ", errs.ToArray());
    }

    private static void Pump()
    {
        if (!Running)
            return;
        for (int k = 0; k < 40 && Running; k++)
        {
            var gm = GameManager.I;
            BindScn();
            int mask = 0;
            if (ready >= 0 && InStage && cur.input != null && !sawGameOver)
                mask = cur.input(age);
            if (gm.State == eGameScreen.GameOver && !gm.IsTransitioning)
            {
                sawGameOver = true;
                if (++gameOverAge == 60)
                    NesPad.I.InjectPress(eNesButton.Start);
            }
            for (int b = 0; b < NesPad.BUTTON_COUNT; b++)
                if ((mask & (1 << b)) != 0)
                    NesPad.I.Inject((eNesButton)b, 1);
            stepMethod.Invoke(NesClock.I, null);
            ObserveScn();
            if (!Running)
                return;
        }
    }

    private static void ObserveScn()
    {
        var gm = GameManager.I;
        if (gm.State != lastScreen && !gm.IsTransitioning)
        {
            trace.Add(age + "\tmàn " + gm.State);
            if (lastScreen == eGameScreen.GameOver && gm.State == eGameScreen.Stage1)
            {
                continued = true;
                afterContinue = "";
            }
            lastScreen = gm.State;
        }
        if (ready < 0)
        {
            if (InStage && S != null && S.Fsm != null && S.Fsm.HasState && W.GetComponent<StageFlow>() != null && !W.GetComponent<StageFlow>().Busy && W.TickCount > 1)
            {
                if (cur.warpSec != null)
                {
                    foreach (var sec in W.Sections)
                        if (sec.name == cur.warpSec)
                        {
                            W.Warp(sec, cur.wx, cur.wy);
                            S.Teleport(cur.wx, cur.wy, cur.wf);
                            if (W.CameraCtl != null)
                                W.CameraCtl.SnapToTarget();
                            break;
                        }
                }
                ready = 0;
                lastHp = GameSession.Current.playerHp;
                trace.Add("0\tsẵn sàng " + State());
            }
            return;
        }
        age++;
        var s = GameSession.Current;
        if (continued && afterContinue.Length == 0 && InStage && S != null && S.Fsm != null && S.Fsm.HasState)
            afterContinue = State();
        if (s.playerHp < lastHp && InStage && S != null && !S.IsDying)
        {
            hpDrops.Add(lastHp - s.playerHp);
            kbStartX = S.X;
            kbMaxDx = 0;
            lastDropAge = age;
            trace.Add(age + "\tHP " + lastHp + "→" + s.playerHp + " " + State());
        }
        if (kbStartX != int.MinValue && S != null && InStage)
        {
            // lùi theo TỪNG đòn: đo tới khi Simon rời trạng thái Hurt
            if (S.Fsm.Is(eSimonState.Hurt) || age == lastDropAge)
                kbMaxDx = Math.Max(kbMaxDx, Math.Abs(S.X - kbStartX));
            else
            {
                kbs.Add(kbMaxDx);
                kbStartX = int.MinValue;
                kbMaxDx = 0;
            }
        }
        lastHp = s.playerHp;
        if (age % 30 == 0 && InStage && S != null)
            trace.Add(age + "\t" + State() + " " + (S.Fsm.HasState ? S.Fsm.CurState.ToString() : "-"));
        bool end = age >= cur.maxTicks
                   || (cur.name != "gameover_st6" && cur.name != "damage_halla" && respawns.Count > 0 && age - ParseAge(respawns[0]) > 30)
                   || (cur.name == "gameover_st6" && continued && afterContinue.Length > 0 && age > 0);
        if (end)
        {
            string err = cur.check();
            if (err == null) passCount++; else failCount++;
            rep.Append(err == null ? "PASS " : "FAIL ").Append(cur.name).Append(" (").Append(age).Append(" tick)")
               .Append(err != null ? " — " + err : "").Append('\n')
               .Append("   chết: ").Append(string.Join(" | ", deaths.ToArray())).Append('\n')
               .Append("   hồi sinh: ").Append(string.Join(" ", respawns.ToArray())).Append(" ⇒ ").Append(afterRespawn).Append('\n');
            if (cur.name == "gameover_st6")
                rep.Append("   GAME OVER ").Append(sawGameOver ? "có" : "KHÔNG").Append(", CONTINUE ⇒ ").Append(afterContinue).Append('\n');
            if (hits.Count > 0 || hpDrops.Count > 0)
            {
                rep.Append("   đòn vào Simon: ").Append(string.Join(" | ", hits.ToArray())).Append('\n');
                rep.Append("   HP mất mỗi lần: ").Append(string.Join(",", hpDrops.ConvertAll(x => x.ToString()).ToArray())).Append(", lùi mỗi đòn (px, tới khi hết Hurt): ").Append(string.Join(",", kbs.ConvertAll(x => x.ToString()).ToArray())).Append('\n');
            }
            Next();
        }
    }

    private static int ParseAge(string s)
    {
        int v;
        return int.TryParse(s.TrimStart('@'), out v) ? v : 0;
    }

    private static void Finish(string err)
    {
        EditorApplication.update -= Pump;
        Application.logMessageReceived -= OnLog;
        UnbindScn();
        if (NesClock.I != null)
            NesClock.I.enabled = true;
        CvgpDebug.CheatInvincible = prevInvincible;
        EditorPrefs.SetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, prevInvincible);
        Running = false;
        Finished = true;
        Error = err ?? "";
        rep.Insert(0, "Map2AcceptProbe.Run: " + passCount + "/" + (passCount + failCount) + " ĐẠT, lỗi console " + consoleErrors.Count + (Error.Length > 0 ? ", LỖI " + Error : "") + "\n");
        foreach (var e in consoleErrors)
            rep.Append("  ").Append(e).Append('\n');
        rep.Append("\n# vết\n");
        foreach (var t in trace)
            rep.Append(t).Append('\n');
        if (outFile != null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outFile));
            File.WriteAllText(outFile, rep.ToString(), new UTF8Encoding(false));
        }
    }

    public static string Report() => rep.ToString();

    // ================================================================ 3) sổ điểm video Map_2

    /// <summary>
    /// File sổ (Tools/cvm2/accept12_ledger.py sinh từ ref/data/map2_events.txt): mỗi dòng "frame \t op \t arg \t score \t hearts \t lives \t sub \t mult"
    /// (−1 = không so). op: start (score,hearts,lives,sub,mult,whip=arg), kill eEnemyId, boss eBossId, item eItemId, use eSubWeapon (−giá tim),
    /// stage n (EnterStage), time n (đặt TIME — HUD video lúc nhặt cầu), tally (StageClearTally Block 2 tới xong), nextblock (StartNextBlock).
    /// Cần đang ở màn chơi (StageWorld) — thế giới dừng trong lúc áp.
    /// </summary>
    public static string Ledger(string ledgerFile, string outPath)
    {
        if (!EditorApplication.isPlaying || W == null || GameSession.Current == null)
            return "cần Play Main.unity, đang ở màn chơi";
        var cfg = W.Config;
        var s = GameSession.Current;
        var sb = new StringBuilder();
        int ok = 0, bad = 0;
        bool prevPaused = W.Paused;
        W.Paused = true;
        int oneUps = 0;
        Action<int> onUp = l => oneUps++;
        GameRules.OneUp += onUp;
        try
        {
            foreach (var raw in File.ReadAllLines(ledgerFile))
            {
                if (raw.Length == 0 || raw[0] == '#')
                    continue;
                var c = raw.Split('\t');
                string frame = c[0], op = c[1], arg = c[2];
                int eScore = int.Parse(c[3]), eHearts = int.Parse(c[4]), eLives = int.Parse(c[5]), eMult = int.Parse(c[7]);
                string eSub = c[6];
                string note = "";
                switch (op)
                {
                    case "start":
                        GameRules.NewGame();
                        s.stage = 4;
                        s.block = GameRules.BlockOfStage(4);
                        s.time = GameRules.TimeLimitOfStage(4);
                        s.score = eScore;
                        s.hearts = eHearts;
                        s.lives = eLives;
                        s.whipLevel = int.Parse(arg);
                        s.SetSubWeapon((eSubWeapon)Enum.Parse(typeof(eSubWeapon), eSub));
                        s.shotMultiplier = eMult;
                        s.nextOneUp = GameRules.NextOneUpAbove(s.score);
                        note = "TIME " + s.time;
                        break;
                    case "kill":
                        {
                            var id = (eEnemyId)Enum.Parse(typeof(eEnemyId), arg);
                            var row = cfg.GetEnemy(id);
                            int pts = row != null && row.score.has ? row.score.value : 0;   // = EnemyBase.ResetHealth ⇒ Score
                            GameRules.AddScore(pts);
                            note = "+" + pts + " (ConfigEnemy." + id + ")";
                        }
                        break;
                    case "boss":
                        {
                            var id = (eBossId)Enum.Parse(typeof(eBossId), arg);
                            var row = cfg.GetBoss(id, 1);
                            int pts = row != null && row.score.has ? row.score.value : 0;   // = BossBase.ResetHealth ⇒ Score
                            GameRules.AddScore(pts);
                            note = "+" + pts + " (ConfigBoss." + id + ")";
                        }
                        break;
                    case "item":
                        {
                            var id = (eItemId)Enum.Parse(typeof(eItemId), arg);
                            ItemEffects.Apply(W, id, S != null ? S.X : 0, S != null ? S.Y : 0, S);
                            note = ItemEffects.LastLog;
                        }
                        break;
                    case "use":
                        {
                            var w = (eSubWeapon)Enum.Parse(typeof(eSubWeapon), arg);
                            var row = cfg.GetSubWeapon(w);
                            int cost = row != null ? row.heartCost : 1;
                            s.hearts -= cost;
                            note = "−" + cost + " tim (ConfigSubWeapon." + w + ".heartCost)";
                        }
                        break;
                    case "stage":
                        GameRules.EnterStage(int.Parse(arg), false);
                        note = "EnterStage(" + arg + ") TIME " + s.time;
                        break;
                    case "time":
                        s.time = int.Parse(arg);
                        note = "TIME = " + arg + " (HUD video lúc nhặt cầu)";
                        break;
                    case "tally":
                        {
                            var t = new StageClearTally();
                            t.Start(cfg, Map2FlowTiming.TALLY_WAIT_TICKS, Map2FlowTiming.TALLY_GAP_TICKS);
                            int n = 0;
                            while (!t.Tick() && n < 20000)
                                n++;
                            note = "tally Block 2: TIME " + t.TimeCounted + " đv (+" + (t.TimeCounted * 10) + "), tim " + t.HeartsCounted + " (+" + (t.HeartsCounted * 100) + "), cộng "
                                   + t.PointsAdded + "; pha: TIME bắt đầu tick " + t.TimeStartTick + ", hết " + t.TimeEndTick + ", tim " + t.HeartStartTick + "→" + t.HeartEndTick + ", xong " + t.DoneTick;
                        }
                        break;
                    case "nextblock":
                        GameRules.StartNextBlock();
                        note = "StartNextBlock ⇒ STAGE " + s.stage + " tim " + s.hearts;
                        break;
                    default:
                        note = "?? op lạ";
                        break;
                }
                s.NotifyChanged();
                var errs = new List<string>();
                if (eScore >= 0 && s.score != eScore) errs.Add("SCORE " + s.score + " ≠ " + eScore);
                if (eHearts >= 0 && s.hearts != eHearts) errs.Add("tim " + s.hearts + " ≠ " + eHearts);
                if (eLives >= 0 && s.lives != eLives) errs.Add("P " + s.lives + " ≠ " + eLives);
                if (eSub != "-" && s.subWeapon.ToString() != eSub) errs.Add("vũ khí " + s.subWeapon + " ≠ " + eSub);
                if (eMult >= 0 && s.shotMultiplier != eMult) errs.Add("hệ số " + s.shotMultiplier + " ≠ " + eMult);
                if (errs.Count == 0) ok++; else bad++;
                sb.Append(errs.Count == 0 ? "OK  " : "LỆCH").Append('\t').Append(frame).Append('\t').Append(op).Append(' ').Append(arg).Append('\t')
                  .Append("SCORE ").Append(s.score.ToString("000000")).Append(" ♥").Append(s.hearts).Append(" P").Append(s.lives).Append(' ').Append(s.subWeapon)
                  .Append('×').Append(s.shotMultiplier).Append(" T").Append(s.time).Append(" ST").Append(s.stage).Append('\t').Append(note)
                  .Append(errs.Count > 0 ? "\t" + string.Join("; ", errs.ToArray()) : "").Append('\n');
            }
        }
        catch (Exception e)
        {
            sb.Append("NGOẠI LỆ ").Append(e.Message).Append('\n');
            bad++;
        }
        finally
        {
            GameRules.OneUp -= onUp;
            W.Paused = prevPaused;
        }
        string head = "Ledger: " + ok + "/" + (ok + bad) + " sự kiện khớp HUD video, 1UP " + oneUps + " lần, SCORE cuối " + s.score + "\n";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, head + sb, new UTF8Encoding(false));
        return head;
    }
}
#endif
