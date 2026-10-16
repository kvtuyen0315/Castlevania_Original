#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness luật chơi + HUD sống (CVGP 12; routine 15/16/21 chạy lại) — CHỈ Editor, scene Arena (Player = Simon), Play Mode. Qua MCP:
///   RulesProbe.RunSuite(RulesProbe.ScenarioNames())  ⇒ chạy ĐỒNG BỘ (tắt NesClock, tự gọi NesClock.Step từng tick — không phụ thuộc
///                                                      fps/focus), thêm StageRules vào StageWorld nếu chưa có, TỰ CHẤM ⇒ SuiteReport()
///   RulesProbe.Run(tên)                               ⇒ 1 kịch bản; RulesProbe.StepTicks(n) ⇒ bước tay (chụp ảnh giữa chừng)
/// Kỳ vọng = hằng VIDEO_* / WIKI_* (đo video routine 12 — CVGP_MEMORY §2j; wiki) — độc lập Config. Log từng mốc kèm tick thế giới.
/// </summary>
public static class RulesProbe
{
    // ---------------------------------------------------------------- kỳ vọng
    public const int VIDEO_TIME_UNIT = 64;          // 165 đv / 10556 tick = 63.98
    public const int VIDEO_BAR_STEP = 8;            // ENEMY giảm 8.2 tick/vạch, PLAYER hồi 7.88
    public const int VIDEO_TALLY_WAIT = 260;        // nhặt cầu f6391 → TIME giảm đầu f6521
    public const int VIDEO_TALLY_GAP = 54;          // TIME 0 f6586 → tim giảm đầu f6613
    public const int VIDEO_TALLY_HEART = 8;         // 7.99 tick/tim
    public const int VIDEO_TALLY_END = 120;         // tim 0 f6896 → đen f6956
    public const int VIDEO_SHOT_BLINK = 120;        // II nháy f5987–6047
    public const int VIDEO_SHOT_HALF = 4;
    public const int VIDEO_SCORE_BEFORE = 14900, VIDEO_SCORE_AFTER = 23370, VIDEO_TIME_LEFT = 127, VIDEO_HEARTS_LEFT = 72;
    public const int WIKI_1UP_1 = 30000, WIKI_1UP_2 = 80000, WIKI_1UP_EVERY = 50000;
    public const int SIMON_DEATH_TICKS = 124;       // ConfigSimon.DeathTicks (routine 08 — quy ước) — chỉ để in mốc

    public static readonly List<string> Report = new List<string>();
    public static readonly List<string> Events = new List<string>();
    public static int Pass, Fail;
    public static uint SuiteHash;

    static MethodInfo stepMethod;
    static StageWorld world;
    static StageRules rules;
    static SimonController simon;
    static string cur;
    static bool addedRules, savedAutoRespawn = true;   // trả lại cuối bộ (CVM2 00 — gộp cvgp-fix-05)

    public static string ScenarioNames()
    {
        return "clock600,freeze_whip,hold,pause,timeup,damage,water,pit,gameover,oneup,tally,bars,shotblink,tally";
    }

    // ---------------------------------------------------------------- hạ tầng

    static bool Setup()
    {
        world = StageWorld.Current;
        if (world == null || NesClock.I == null)
        {
            Report.Add("LỖI: cần Play scene Arena (StageWorld + NesClock)");
            return false;
        }
        rules = world.GetComponent<StageRules>();
        simon = world.Player as SimonController;
        if (rules == null)
        {
            savedAutoRespawn = simon != null ? simon.AutoRespawn : true;   // StageRules tắt autoRespawn của Simon và không trả lại
            rules = world.gameObject.AddComponent<StageRules>();
            addedRules = true;
        }
        if (stepMethod == null)
            stepMethod = typeof(NesClock).GetMethod("Step", BindingFlags.Instance | BindingFlags.NonPublic);
        NesClock.I.enabled = false;
        if (world.Player == null)
            StepTicks(1);
        simon = world.Player as SimonController;
        rules.LifeLost -= OnLifeLost;
        rules.LifeLost += OnLifeLost;
        rules.Respawned -= OnRespawned;
        rules.Respawned += OnRespawned;
        rules.TallyDone -= OnTallyDone;
        rules.TallyDone += OnTallyDone;
        GameRules.OneUp -= OnOneUp;
        GameRules.OneUp += OnOneUp;
        return simon != null;
    }

    /// <summary>Bước tay n tick (NesClock.Step: OnSystemTick + OnTick ⇒ NesPad.Tick + StageWorld.Step).</summary>
    public static void StepTicks(int n)
    {
        if (stepMethod == null)
            stepMethod = typeof(NesClock).GetMethod("Step", BindingFlags.Instance | BindingFlags.NonPublic);
        if (NesClock.I != null)
            NesClock.I.enabled = false;
        for (int i = 0; i < n; i++)
            stepMethod.Invoke(NesClock.I, null);
    }

    public static void Release()
    {
        if (NesClock.I != null)
            NesClock.I.enabled = true;
    }

    static int lifeLostTick, respawnTick, tallyDoneTick, oneUps;
    static eDeathCause lifeLostCause;
    static bool lifeLostAlive;

    static void OnLifeLost(eDeathCause c, bool alive) { lifeLostTick = NesClock.Frame; lifeLostCause = c; lifeLostAlive = alive; Ev("LifeLost " + c + " alive=" + alive); }
    static void OnRespawned(StageRules r) { respawnTick = NesClock.Frame; Ev("Respawned"); }
    static void OnTallyDone(StageRules r) { tallyDoneTick = NesClock.Frame; Ev("TallyDone"); }
    static void OnOneUp(int lives) { oneUps++; Ev("1UP lives=" + lives); }

    static void Ev(string s)
    {
        var line = cur + " f" + NesClock.Frame + " w" + (world != null ? world.TickCount : 0) + ": " + s;
        Events.Add(line);
        Mix(cur + ": " + s);   // băm phần tương đối (không số frame tuyệt đối) ⇒ so tất định giữa các lần chạy
    }

    static void Mix(string s)
    {
        foreach (char c in s)
        {
            SuiteHash ^= c;
            SuiteHash *= 16777619u;
        }
    }

    static void Expect(bool ok, string what)
    {
        if (ok) Pass++; else Fail++;
        var line = (ok ? "  ĐẠT " : "  FAIL ") + cur + ": " + what;
        Report.Add(line);
        Mix(line);
    }

    /// <summary>Đầu mỗi kịch bản: ván mới + đầu stage (Simon (48,176) Arena), không pause, không đen.</summary>
    static void Fresh()
    {
        if (rules.IsPaused)
            rules.TogglePause();
        GameRules.NewGame();
        rules.RestartStage();
        simon.Teleport(simon.home.x, simon.home.y, 1);
        lifeLostTick = respawnTick = tallyDoneTick = -1;
        oneUps = 0;
    }

    static int RunUntil(Func<bool> cond, int max)
    {
        for (int i = 1; i <= max; i++)
        {
            StepTicks(1);
            if (cond())
                return i;
        }
        return -1;
    }

    static GameSession S => GameSession.Current;

    // ---------------------------------------------------------------- suite

    public static string RunSuite(string names)
    {
        Report.Clear();
        Events.Clear();
        Pass = Fail = 0;
        SuiteHash = 2166136261u;
        if (!Setup())
            return SuiteReport();
        bool logRules = CvgpDebug.LogRules;
        CvgpDebug.LogRules = false;
        try
        {
            foreach (var n in names.Split(','))
                Run(n.Trim());
        }
        catch (Exception e)
        {
            Report.Add("EXCEPTION " + cur + ": " + e);
            Fail++;
        }
        finally
        {
            CvgpDebug.LogRules = logRules;
            Fresh();
            Release();
            Restore();
        }
        return SuiteReport();
    }

    /// <summary>Trả trạng thái cuối bộ: gỡ StageRules do bộ này thêm + Simon.AutoRespawn + sự kiện ⇒ bộ khác chạy cùng phiên Play (CVM2 00).</summary>
    static void Restore()
    {
        if (rules != null)
        {
            rules.LifeLost -= OnLifeLost;
            rules.Respawned -= OnRespawned;
            rules.TallyDone -= OnTallyDone;
        }
        GameRules.OneUp -= OnOneUp;
        if (addedRules && rules != null)
        {
            rules.enabled = false;
            UnityEngine.Object.DestroyImmediate(rules);   // bộ kế chạy NGAY cùng frame
            if (simon != null)
                simon.AutoRespawn = savedAutoRespawn;
        }
        addedRules = false;
        rules = null;
        world = null;
        simon = null;
    }

    public static string SuiteReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("RulesProbe: ĐẠT " + Pass + " / FAIL " + Fail + " — hash " + SuiteHash.ToString("x8"));
        foreach (var r in Report)
            sb.AppendLine(r);
        sb.AppendLine("— sự kiện:");
        foreach (var e in Events)
            sb.AppendLine("  " + e);
        return sb.ToString();
    }

    public static void Run(string name)
    {
        cur = name;
        if (world == null && !Setup())
            return;
        Fresh();
        switch (name)
        {
            case "clock600": Clock600(); break;
            case "freeze_whip": FreezeWhip(); break;
            case "hold": Hold(); break;
            case "pause": Pause(); break;
            case "timeup": TimeUp(); break;
            case "damage": Damage(); break;
            case "water": FallInto(520, "Water"); break;
            case "pit": FallInto(462, "Pit"); break;
            case "gameover": GameOverLocal(); break;
            case "oneup": OneUpScn(); break;
            case "tally": Tally(); break;
            case "bars": Bars(); break;
            case "shotblink": ShotBlink(); break;
            default: Report.Add("?? kịch bản " + name); Fail++; break;
        }
    }

    // ---------------------------------------------------------------- kịch bản

    static List<int> DecrementTicks(int ticks)
    {
        var list = new List<int>();
        int t0 = world.TickCount, prev = S.time;
        for (int i = 0; i < ticks; i++)
        {
            StepTicks(1);
            if (S.time != prev)
            {
                list.Add(world.TickCount - t0);
                prev = S.time;
            }
        }
        return list;
    }

    static void Clock600()
    {
        int start = S.time;
        var dec = DecrementTicks(600);
        Ev("TIME " + start + "→" + S.time + " giảm ở tick " + string.Join(",", dec));
        int expect = 600 / VIDEO_TIME_UNIT;
        Expect(start - S.time == expect, "600 tick ⇒ −" + (start - S.time) + " đv (kỳ vọng " + expect + " = 600/64; ±1 % của 9.375)");
        bool regular = dec.Count > 0 && dec[0] == VIDEO_TIME_UNIT;
        for (int i = 1; i < dec.Count; i++)
            regular &= dec[i] - dec[i - 1] == VIDEO_TIME_UNIT;
        Expect(regular, "đv đầu sau 64 tick, rồi đều 64 tick (video: đv đầu 60–64 tick sau khi vào section)");
    }

    static void FreezeWhip()
    {
        StepTicks(10);
        world.FreezeWorld(46);
        int start = S.time;
        var dec = DecrementTicks(100);
        Expect(start - S.time == 1 && dec.Count == 1 && dec[0] == 54, "đóng băng nhặt roi 46 tick: TIME vẫn chạy (giảm ở tick 64 từ đầu stage) — video 0:06.5");
    }

    static void Hold()
    {
        rules.SetClockHold(eClockHold.Script, true);
        int start = S.time;
        StepTicks(200);
        Expect(S.time == start, "giữ eClockHold.Script 200 tick ⇒ TIME đứng " + S.time.ToString("0000") + " (video cổng 0:16.2–0:20.0, cửa 6.8 s)");
        rules.SetClockHold(eClockHold.Script, false);
        var dec = DecrementTicks(64);
        Expect(dec.Count == 1 && dec[0] == 64, "thả giữ ⇒ đv kế sau 64 tick (" + string.Join(",", dec) + ")");
    }

    static void Pause()
    {
        StepTicks(30);
        NesPad.I.InjectPress(eNesButton.Start);
        StepTicks(1);
        Expect(rules.IsPaused && world.Paused, "Start ⇒ PAUSE");
        int wt = world.TickCount, t = S.time, x = simon.X;
        NesPad.I.Inject(eNesButton.Right, 100);
        StepTicks(100);
        Expect(world.TickCount == wt && S.time == t && simon.X == x, "đang pause 100 tick: thế giới/TIME/Simon đứng (TickCount +" + (world.TickCount - wt) + ")");
        StepTicks(1);
        NesPad.I.InjectPress(eNesButton.Start);
        StepTicks(1);
        Expect(!rules.IsPaused && !world.Paused, "Start lần 2 ⇒ chạy tiếp");
        var dec = DecrementTicks(64);
        Expect(dec.Count == 1 && dec[0] == 64 - 31, "TIME tính theo tick THẾ GIỚI (không đếm lúc pause): 30 tick trước + tick bỏ pause + 33 = 64 (" + string.Join(",", dec) + ")");
        // không pause khi đang chết
        S.time = 1;
        RunUntil(() => simon.IsDying, 70);
        NesPad.I.InjectPress(eNesButton.Start);
        StepTicks(1);
        Expect(!rules.IsPaused, "Simon đang chết ⇒ Start không pause");
        RunUntil(() => respawnTick >= 0, 400);
    }

    static void PrepareLoadout()
    {
        S.whipLevel = 3;
        S.SetSubWeapon(eSubWeapon.Axe);
        S.shotMultiplier = 2;
        S.hearts = 40;
        S.score = 12340;
        S.NotifyChanged();
    }

    static void CheckRespawned(string why, int livesBefore)
    {
        Expect(S.lives == livesBefore - 1, why + ": P-" + livesBefore.ToString("00") + " → P-" + S.lives.ToString("00"));
        Expect(S.time == 300 && S.hearts == 5 && S.whipLevel == 1 && S.subWeapon == eSubWeapon.None && S.shotMultiplier == 1
               && S.playerHp == 16 && S.score == 12340,
               why + ": hồi sinh reset TIME 0300, tim 05, roi 1, mất vũ khí + II, máu 16, giữ điểm (TIME " + S.time + ", tim " + S.hearts
               + ", roi " + S.whipLevel + ", " + S.subWeapon + ", shot " + S.shotMultiplier + ", máu " + S.playerHp + ", điểm " + S.score + ")");
        Expect(simon.X == simon.home.x && simon.Y == simon.home.y && simon.Fsm.Is(eSimonState.Idle) && !simon.InputLocked,
               why + ": Simon về đầu stage (" + simon.X + "," + simon.Y + ") Idle");
        int black = respawnTick - lifeLostTick;
        Expect(black == 32, why + ": màn đen " + black + " tick (ConfigRule.RespawnBlackTicks 32 — suy luận)");
    }

    static void TimeUp()
    {
        PrepareLoadout();
        S.time = 2;
        S.NotifyChanged();
        int f0 = NesClock.Frame;
        int zero = RunUntil(() => S.time == 0, 200);
        Expect(zero == 128 && simon.Fsm.Is(eSimonState.Dead), "TIME 0002 → 0000 sau " + zero + " tick ⇒ Simon Dead ngay (wiki: hết giờ mất mạng)");
        int lives = S.lives;
        RunUntil(() => lifeLostTick >= 0, 400);
        Expect(lifeLostCause == eDeathCause.TimeUp && lifeLostAlive, "LifeLost TimeUp sau " + (lifeLostTick - f0 - zero) + " tick chết (DeathTicks " + SIMON_DEATH_TICKS + ")");
        RunUntil(() => respawnTick >= 0, 100);
        CheckRespawned("hết giờ", lives);
        var dec = DecrementTicks(64);
        Expect(dec.Count == 1 && dec[0] == 64, "sau hồi sinh: đv đầu sau 64 tick (" + string.Join(",", dec) + ")");
    }

    static void Damage()
    {
        PrepareLoadout();
        S.playerHp = S.shownPlayerHp = 2;
        S.NotifyChanged();
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<ArenaTarget>("Assets/Data/Prefabs/Test/ArenaTarget.prefab");
        simon.Teleport(300, 176, 1);
        world.CameraCtl.SnapToTarget();
        var target = world.Spawn(prefab, 330, 176, -1);
        ((ArenaTarget)target).touchDamage = true;   // bia lấy từ pool mang touchDamage = false của probe trước (RegressionProbe/AttackProbe) — CVM2 00
        NesPad.I.Inject(eNesButton.Right, 40);
        int hit = RunUntil(() => S.playerHp == 0, 60);
        Expect(hit > 0, "chạm bia (EnemyBody) mất 2 vạch ⇒ máu 0 sau " + hit + " tick");
        int lives = S.lives;
        RunUntil(() => lifeLostTick >= 0, 600);
        Expect(lifeLostCause == eDeathCause.Damage && lifeLostAlive, "knockback → đáp → Dead → LifeLost Damage");
        RunUntil(() => respawnTick >= 0, 100);
        CheckRespawned("hết máu", lives);
        Expect(target == null || !target.Alive, "hồi sinh dọn thực thể (bia đã gỡ)");
    }

    static void FallInto(int x, string kind)
    {
        PrepareLoadout();
        simon.Teleport(x, 176, 1);
        world.CameraCtl.SnapToTarget();
        NesPad.I.Inject(eNesButton.Right, 40);
        int d = RunUntil(() => simon.Fsm.Is(eSimonState.FallDeath), 60);
        Expect(d > 0 && simon.lastDeath.ToString() == kind, "đi phải từ x " + x + " ⇒ FallDeath (" + simon.lastDeath + ") sau " + d + " tick");
        int lives = S.lives;
        RunUntil(() => lifeLostTick >= 0, 300);
        Expect(lifeLostCause == eDeathCause.Fall && lifeLostAlive, "LifeLost Fall");
        RunUntil(() => respawnTick >= 0, 100);
        CheckRespawned("rơi " + kind, lives);
    }

    static void GameOverLocal()
    {
        S.lives = 1;
        S.score = 5000;
        S.time = 1;
        S.NotifyChanged();
        RunUntil(() => lifeLostTick >= 0, 400);
        Expect(!lifeLostAlive && S.lives == 0, "mạng cuối chết ⇒ P-00, GAME OVER (Arena không có GameManager ⇒ GameRules.EnterGameOver = false)");
        RunUntil(() => respawnTick >= 0, 100);
        Expect(respawnTick - lifeLostTick == 32, "đen GameOverBlackTicks 32 rồi tự CONTINUE");
        Expect(S.lives == 3 && S.score == 0 && S.time == 300 && S.stage == 1 && S.nextOneUp == WIKI_1UP_1,
               "CONTINUE: P-03, điểm 0, TIME 0300, STAGE 01 (đầu stage đang chơi — wiki), mốc 1UP " + S.nextOneUp);
    }

    static void OneUpScn()
    {
        int l = S.lives;
        GameRules.AddScore(29990);
        Expect(S.lives == l && oneUps == 0, "29 990 điểm: chưa 1UP");
        GameRules.AddScore(10);
        Expect(S.lives == l + 1, "30 000 ⇒ 1UP (P-" + S.lives.ToString("00") + ")");
        GameRules.AddScore(49990);
        Expect(S.lives == l + 1, "79 990: chưa");
        GameRules.AddScore(10);
        Expect(S.lives == l + 2, "80 000 ⇒ 1UP");
        GameRules.AddScore(49999);
        Expect(S.lives == l + 2, "129 999: chưa");
        GameRules.AddScore(1);
        Expect(S.lives == l + 3 && S.nextOneUp == 180000, "130 000 ⇒ 1UP, mốc kế 180 000 (mỗi 50 000)");
        GameRules.AddScore(200000);
        Expect(S.lives == l + 7 && S.score == 330000, "cộng 1 lần 200 000 qua 4 mốc ⇒ +4 mạng (P-" + S.lives.ToString("00") + ")");
        S.NotifyChanged();
    }

    static void Tally()
    {
        S.time = VIDEO_TIME_LEFT;
        S.hearts = VIDEO_HEARTS_LEFT;
        S.score = VIDEO_SCORE_BEFORE;
        S.playerHp = S.shownPlayerHp = 8;
        S.NotifyChanged();
        StepTicks(5);
        ItemEffects.Apply(world, eItemId.MagicCrystal, simon.X, simon.Y, simon);   // ngoài Step ⇒ coi như nhặt ở tick kế
        var hpAt = new List<int>();
        int full = -1;
        NesPad.I.Inject(eNesButton.Right, 60);
        int x0 = simon.X;
        var tl = rules.Tally;
        int done = RunUntil(() =>
        {
            if (full < 0 && S.shownPlayerHp == 16) full = tl.Ticks;
            if (tl.Ticks <= 60 && hpAt.Count < 9 && (hpAt.Count == 0 || S.shownPlayerHp != hpAt[hpAt.Count - 1])) hpAt.Add(S.shownPlayerHp);
            return tallyDoneTick >= 0;
        }, 2000);
        Ev(string.Format("tally: TIME bắt đầu {0}, TIME=0 {1}, tim bắt đầu {2}, tim=0 {3}, xong {4} (tick kể từ nhặt = 1); điểm {5}",
            tl.TimeStartTick, tl.TimeEndTick, tl.HeartStartTick, tl.HeartEndTick, tl.DoneTick, S.score));
        Expect(simon.X == x0, "khoá input sau quả cầu (giữ → 60 tick, x " + x0 + "→" + simon.X + ")");
        Expect(tl.TimeStartTick - 1 == VIDEO_TALLY_WAIT, "chờ " + (tl.TimeStartTick - 1) + " tick từ lúc nhặt tới TIME giảm đầu (video 260)");
        Expect(tl.TimeEndTick - tl.TimeStartTick + 1 == VIDEO_TIME_LEFT, "TIME 127 → 0 trong " + (tl.TimeEndTick - tl.TimeStartTick + 1) + " tick (1 đv/tick — video 128 tick)");
        Expect(tl.HeartStartTick - tl.TimeEndTick == VIDEO_TALLY_GAP, "nghỉ " + (tl.HeartStartTick - tl.TimeEndTick) + " tick (video 54)");
        Expect(tl.HeartEndTick - tl.HeartStartTick == (VIDEO_HEARTS_LEFT - 1) * VIDEO_TALLY_HEART, "72 tim 1/8 tick: " + (tl.HeartEndTick - tl.HeartStartTick) + " tick (video 566 ± 4)");
        Expect(tl.DoneTick - tl.HeartEndTick == VIDEO_TALLY_END, "chờ " + (tl.DoneTick - tl.HeartEndTick) + " tick rồi xong (video 120 tới màn đen)");
        Expect(S.score == VIDEO_SCORE_AFTER && S.time == 0 && S.hearts == 0, "điểm " + VIDEO_SCORE_BEFORE + " + 127×10 + 72×100 = " + S.score + " (video 023370)");
        Expect(full == 1 + 7 * VIDEO_BAR_STEP, "máu HIỆN 8→16 từng vạch mỗi 8 tick: đầy ở tick " + full + " (" + string.Join(",", hpAt) + ") — video 7.88 tick/vạch");
        NesPad.I.InjectPress(eNesButton.Start);
        StepTicks(1);
        Expect(!rules.IsPaused, "không pause khi đã tally");
        if (done < 0)
            Report.Add("  (tally không xong trong 2000 tick)");
    }

    static void Bars()
    {
        StepTicks(2);
        S.playerHp = 14;
        S.enemyHp = 12;
        S.NotifyChanged();
        var p = new List<int>();
        var e = new List<int>();
        int t0 = world.TickCount, lp = S.shownPlayerHp, le = S.shownEnemyHp;
        for (int i = 0; i < 40; i++)
        {
            StepTicks(1);
            if (S.shownPlayerHp != lp) { p.Add(world.TickCount - t0); lp = S.shownPlayerHp; }
            if (S.shownEnemyHp != le) { e.Add(world.TickCount - t0); le = S.shownEnemyHp; }
        }
        Expect(p.Count == 2 && p[0] == 1 && p[1] == 1 + VIDEO_BAR_STEP, "PLAYER 16→14 hiện 2 bước ở tick " + string.Join(",", p) + " (video: 2 bước cách 8)");
        Expect(e.Count == 4 && e[3] - e[0] == 3 * VIDEO_BAR_STEP, "ENEMY 16→12: 4 bước " + string.Join(",", e));
        S.playerHp = 16;
        S.NotifyChanged();
        var h = new List<int>();
        t0 = world.TickCount;
        lp = S.shownPlayerHp;
        for (int i = 0; i < 20; i++)
        {
            StepTicks(1);
            if (S.shownPlayerHp != lp) { h.Add(world.TickCount - t0); lp = S.shownPlayerHp; }
        }
        Expect(h.Count == 2 && h[1] - h[0] == VIDEO_BAR_STEP, "hồi 14→16 cách " + (h.Count == 2 ? h[1] - h[0] : -1) + " tick");
    }

    static void ShotBlink()
    {
        S.SetSubWeapon(eSubWeapon.Axe);
        StepTicks(3);
        S.shotMultiplier = 2;
        S.NotifyChanged();
        var sb = new StringBuilder();
        int on = 0, changes = 0;
        bool last = true;
        for (int i = 0; i < 130; i++)
        {
            StepTicks(1);
            bool v = S.shotIconVisible;
            if (i < 128)
                sb.Append(v ? '#' : '.');
            if (v) on++;
            if (v != last) changes++;
            last = v;
        }
        Ev("II: " + sb);
        Expect(changes == 30 && S.shotIconVisible, "II nháy 4 hiện/4 ẩn trong 120 tick rồi sáng hẳn (" + changes + " lần đổi; video f5987–6047 ~30 mốc)");
    }
}
#endif
