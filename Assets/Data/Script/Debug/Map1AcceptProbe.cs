#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Harness NGHIỆM THU Map 1 (CVGP 16; routine 21 chạy lại) — CHỈ Editor, scene Main.unity khi Play. Chỉ ĐO, không đổi gameplay. Qua MCP:
///   StartFlow(fps, shotDir)   ⇒ luồng THẬT theo thời gian thực (NesClock bật, targetFrameRate = fps): GoTo(Title,0) = L0, nhấn Start ở L352
///                               (như harness H8), phát map1_route.txt từ tick Stage 01 bật (L922); CVM2 11: tới tick màn STAGE 04 (StageScreen)
///                               bật thì phát tiếp map2_route.txt tới STAGE 07 stub + 60 tick (không có map2_route ⇒ dừng ở STAGE 04 + 60 như
///                               cũ); hash trạng thái CÙNG công thức Map1RouteProbe (= Map2RouteProbe.Replay), "trước STAGE 04" = RegressionRunner.PRE4 (CVM3 04: 59cafbaa)
///                               (Map 1 không đổi); log mốc luồng, SFX, enemy sinh/chết, item, vật vỡ, TIME;
///                               shotDir ≠ null ⇒ ảnh H8 L10/L600/L930. Cheat bất tử bật suốt (như lộ trình 15), trả lại khi xong.
///   StartSpam(stage, ticks, seed, withStart) ⇒ bấm ngẫu nhiên MỌI nút mỗi tick (không bất tử) từ đầu stage; đếm lỗi console.
///   StartKeyLog() / StopKeyLog() ⇒ ghi Held/Pressed NesPad mỗi tick (kiểm phím thật bấm từ ngoài — PowerShell keybd_event).
///   StartTerrain(dir, jobs)  ⇒ dừng thế giới, ẩn sprite (sorting layer BackgroundFX/Character/Overlay), chụp nền section ở các cx.
///   StartHud(dir, jobs)      ⇒ dừng thế giới, đặt GameSession theo thời điểm video rồi chụp (so dải HUD — hud_pairs 12).
///   Report() / Events / SaveReport(path).
/// Thứ tự trong tick: PreTick chạy TRƯỚC GameManager.HandleTick (gỡ rồi đăng ký lại handler) ⇒ quan sát trạng thái SAU tick trước — tương
/// đương Map1RouteProbe.Observe sau mỗi Step.
/// </summary>
public static class Map1AcceptProbe
{
    public enum eMode { None, Flow, Spam, KeyLog, Shots }

    public static eMode Mode { get; private set; }
    public static bool Running { get; private set; }
    public static bool Finished { get; private set; }
    public static string Error { get; private set; } = "";
    public static int Tick { get; private set; }
    public static uint Hash { get; private set; }
    public static int Fps { get; private set; }
    public static readonly List<string> Marks = new List<string>();
    public static readonly List<string> Events = new List<string>();
    public static readonly List<uint> Checkpoints = new List<uint>();
    public static readonly List<int> ObjectCounts = new List<int>();
    /// <summary>Lỗi console (Error/Exception/Assert) trong lúc chạy.</summary>
    public static readonly List<string> ConsoleErrors = new List<string>();
    /// <summary>CVM2 11: hash ngay trước tick màn STAGE 04 bật (= Map1RouteProbe.PreStage4Hash) + tick đó.</summary>
    public static uint PreStage4Hash { get; private set; }
    public static int Stage4Tick => stage4Tick;

    // ---- trạng thái chung
    private static Action<int> gmHandler;
    private static bool hooked;
    private static int f0 = -1, replayFrame = -1, stage4Tick = -1, bossWakeTick = -1, stage7Tick = -1;
    private static List<int> route2;     // CVM2 11: map2_route (null ⇒ luồng cũ dừng ở STAGE 04)
    private static bool route2Started;
    private static eGameScreen lastScreen;
    private static string lastSection = "";
    private static bool prevInvincible, prevLogFlow;
    private static int prevTargetFps, prevVSync;
    private static string shotDir;
    private static List<int> route;

    // ---- đo khung
    private static double frameSum, frameMax;
    private static int frameCount;
    private static int lastTime = -1;
    private static readonly List<int> timeChangeTicks = new List<int>();
    private static AudioSource tallyChannel;
    private static float tallyLastTime;
    private static AudioClip tallyLastClip;
    private static readonly HashSet<CandleProp> brokenSeen = new HashSet<CandleProp>();
    private static readonly List<CandleProp> brokenPending = new List<CandleProp>();

    private static StageWorld W => StageWorld.Current;
    private static SimonController S => W != null ? W.Player as SimonController : null;
    private static int Cx => W != null && W.CameraCtl != null ? W.CameraCtl.X : -1;
    private static string Sec => W != null && W.Section != null ? W.Section.name : "";
    private static bool InStage => GameManager.I != null && GameManager.I.State == eGameScreen.Stage1 && !GameManager.I.IsTransitioning && W != null;

    // ================================================================ móc tick

    private static void Hook()
    {
        if (hooked)
            return;
        var mi = typeof(GameManager).GetMethod("HandleTick", BindingFlags.Instance | BindingFlags.NonPublic);
        gmHandler = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), GameManager.I, mi);
        NesClock.OnSystemTick -= gmHandler;
        NesClock.OnSystemTick += PreTick;
        NesClock.OnSystemTick += gmHandler;
        NesClock.OnTick += PostTick;
        Application.logMessageReceived += OnLog;
        EditorApplication.update += Pump;
        hooked = true;
    }

    private static void Unhook()
    {
        if (!hooked)
            return;
        NesClock.OnSystemTick -= PreTick;
        NesClock.OnTick -= PostTick;
        Application.logMessageReceived -= OnLog;
        EditorApplication.update -= Pump;
        hooked = false;
    }

    private static bool CanStart(out string why)
    {
        why = null;
        if (!EditorApplication.isPlaying || GameManager.I == null || NesClock.I == null || NesPad.I == null)
            why = "cần Play Main.unity";
        else if (Running)
            why = "đang chạy " + Mode;
        else if (Map1RouteProbe.Running)
            why = "Map1RouteProbe đang chạy";
        return why == null;
    }

    private static void ResetCommon(eMode m)
    {
        Mode = m;
        Error = "";
        Finished = false;
        Tick = 0;
        Hash = 2166136261u;
        Marks.Clear();
        Events.Clear();
        Checkpoints.Clear();
        ConsoleErrors.Clear();
        timeChangeTicks.Clear();
        brokenSeen.Clear();
        brokenPending.Clear();
        lastTime = -1;
        f0 = replayFrame = stage4Tick = bossWakeTick = stage7Tick = -1;
        route2 = null;
        route2Started = false;
        PreStage4Hash = 0;
        lastSection = "";
        frameSum = frameMax = 0;
        frameCount = 0;
        tallyChannel = null;
        tallyLastClip = null;
        tallyLastTime = 0f;
        prevTargetFps = Application.targetFrameRate;
        prevVSync = QualitySettings.vSyncCount;
        prevLogFlow = CvtiDebug.LogFlow;
        prevInvincible = CvgpDebug.CheatInvincible;
    }

    private static void Finish(string err)
    {
        Unhook();
        UnsubscribeGameEvents();
        if (NesPad.I != null)
            NesPad.I.StopReplay();
        Application.targetFrameRate = prevTargetFps;
        QualitySettings.vSyncCount = prevVSync;
        CvtiDebug.LogFlow = prevLogFlow;
        CvgpDebug.CheatInvincible = prevInvincible;
        EditorPrefs.SetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, prevInvincible);
        StageWorld.MeasurePerf = false;
        Running = false;
        Finished = true;
        Error = err ?? "";
        Checkpoints.Add(Hash);
        ObjectCounts.Add(UnityEngine.Object.FindObjectsOfType<Transform>(true).Length);
        Mark(string.IsNullOrEmpty(err) ? "end" : "LỖI " + err);
    }

    public static void Abort()
    {
        if (Running)
            Finish("huỷ tay");
    }

    // ================================================================ 1) Luồng thật Title → … → STAGE 04 (thời gian thực)

    public static string StartFlow(int fps, string dir)
    {
        string why;
        if (!CanStart(out why))
            return why;
        if (!File.Exists(Map1RouteProbe.REPLAY))
            return "thiếu " + Map1RouteProbe.REPLAY;
        ResetCommon(eMode.Flow);
        Fps = fps;
        route = NesReplay.Decode(File.ReadAllText(Map1RouteProbe.REPLAY));
        route2 = File.Exists(Map2RouteProbe.REPLAY) ? NesReplay.Decode(File.ReadAllText(Map2RouteProbe.REPLAY)) : null;
        shotDir = dir;
        if (dir != null)
            Directory.CreateDirectory(dir);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        Application.runInBackground = true;
        CvgpDebug.CheatInvincible = true;
        EditorPrefs.SetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, true);
        CvtiDebug.LogFlow = true;
        StageWorld.MeasurePerf = true;
        SubscribeGameEvents();
        Hook();
        f0 = NesClock.Frame;
        if (!GameManager.I.GoTo(eGameScreen.Title, 0))
        {
            Finish("GameManager đang chuyển cảnh");
            return Error;
        }
        Running = true;
        Mark("GoTo(Title,0) abs " + f0 + " = L0");
        return "flow bắt đầu @" + fps + " fps, route " + route.Count + " tick" + (route2 != null ? " + map2 " + route2.Count + " tick" : "");
    }

    private static int Lof(int frame) => f0 >= 0 ? frame - f0 : -1;

    private static void PreTick(int frame)
    {
        if (!Running)
            return;
        switch (Mode)
        {
            case eMode.Flow: PreTickFlow(frame); break;
            case eMode.Spam: PreTickSpam(frame); break;
        }
    }

    private static void PreTickFlow(int frame)
    {
        int L = Lof(frame);
        if (L == 352)
            NesPad.I.InjectPress(eNesButton.Start);
        // ảnh chụp CUỐI khung Unity ⇒ gồm tick L (60 fps: 1 tick/khung) — như harness H8 (OnSystemTick sau GameManager)
        if (shotDir != null && (L == 10 || L == 600 || L == 930))
            Capture("h8_L" + L);
        // Bắt đầu phát lộ trình đúng tick màn đen → Stage1 (NesPad.Tick của tick này đọc phần tử 0, StageWorld bước 1 cùng tick — như Map1RouteProbe)
        if (replayFrame < 0)
        {
            var gm = GameManager.I;
            if (gm.IsTransitioning && PendingNext() == eGameScreen.Stage1 && BlackLeft() == 1)
            {
                NesPad.I.PlayReplay(route);
                replayFrame = frame;
                lastScreen = eGameScreen.Stage1;
                Mark("phát lộ trình ở L" + L + " (abs " + frame + ")");
            }
            return;
        }
        // CVM2 11: tick màn đen cuối trước STAGE 04 (bản đồ lâu đài → StageScreen stage 4) ⇒ phát map2_route (như Map2RouteProbe)
        if (route2 != null && !route2Started)
        {
            var gm = GameManager.I;
            if (gm.IsTransitioning && PendingNext() == eGameScreen.Stage1 && BlackLeft() == 1 && GameSession.Current.stage == 4)
            {
                NesPad.I.PlayReplay(route2);
                route2Started = true;
                Mark("phát map2_route ở L" + L + " (" + route2.Count + " tick)");
            }
        }
        int k = frame - replayFrame;   // tick k của lộ trình đã xong
        if (k < 1)
            return;
        Tick = k;
        Observe();
        if (route2 == null && stage4Tick >= 0 && Tick - stage4Tick >= Map1RouteProbe.TAIL_TICKS)
        {
            Finish(null);
            return;
        }
        if (route2 != null && stage7Tick >= 0 && Tick - stage7Tick >= Map2RouteProbe.TAIL_TICKS)
        {
            Finish(null);
            return;
        }
        if (Tick > route.Count + (route2 != null ? route2.Count : 0) + 200)
            Finish(route2 != null ? "hết lộ trình mà chưa tới STAGE 07" : "hết lộ trình mà chưa tới STAGE 04");
    }

    private static eGameScreen PendingNext()
    {
        var f = typeof(GameManager).GetField("pendingNext", BindingFlags.Instance | BindingFlags.NonPublic);
        return f != null ? (eGameScreen)f.GetValue(GameManager.I) : eGameScreen.Boot;
    }

    private static int BlackLeft()
    {
        var f = typeof(GameManager).GetField("blackLeft", BindingFlags.Instance | BindingFlags.NonPublic);
        return f != null ? (int)f.GetValue(GameManager.I) : -1;
    }

    /// <summary>Chép NGUYÊN công thức Map1RouteProbe.Observe (hash so được với e0b2d54c).</summary>
    private static void Observe()
    {
        var gm = GameManager.I;
        if (gm.State != lastScreen && !gm.IsTransitioning)
        {
            lastScreen = gm.State;
            Mark("màn " + gm.State);
            if (Map1RouteProbe.IsStage4Screen(gm.State) && stage4Tick < 0)
            {
                stage4Tick = Tick;
                PreStage4Hash = Hash;
            }
            if (gm.State == eGameScreen.Stage7Stub && stage7Tick < 0)
                stage7Tick = Tick;
        }
        if (InStage)
        {
            if (Sec != lastSection)
            {
                lastSection = Sec;
                Mark("section " + Sec);
            }
            var b = Boss();
            if (b != null && !b.dormant && bossWakeTick < 0)
            {
                bossWakeTick = W.TickCount;
                Mark("boss thức");
            }
            PollCandles();
        }
        uint h = Hash;
        h = Mix(h, (int)gm.State);
        h = Mix(h, gm.IsTransitioning ? 1 : 0);
        var s = GameSession.Current;
        h = Mix(h, s.score); h = Mix(h, s.time); h = Mix(h, s.hearts); h = Mix(h, s.stage); h = Mix(h, s.playerHp);
        h = Mix(h, s.enemyHp); h = Mix(h, s.lives); h = Mix(h, s.whipLevel); h = Mix(h, (int)s.subWeapon); h = Mix(h, s.shotMultiplier);
        if (InStage)
        {
            h = Mix(h, W.TickCount);
            h = Mix(h, Cx);
            h = Mix(h, Sec.Length * 31 + (Sec.Length > 0 ? Sec[0] : 0));
            for (int g = 0; g < Entity.GROUP_COUNT; g++)
            {
                var list = W.GroupList(g);
                h = Mix(h, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    h = Mix(h, list[i].fx);
                    h = Mix(h, list[i].fy);
                }
            }
            var simon = S;
            if (simon != null)
                h = Mix(h, (int)simon.Fsm.CurState);
        }
        Hash = h;
        if (Tick % 500 == 0)
            Checkpoints.Add(Hash);
        // TIME
        if (s.time != lastTime)
        {
            if (lastTime >= 0)
                timeChangeTicks.Add(Tick);
            lastTime = s.time;
        }
        PollTallySfx();
    }

    private static uint Mix(uint h, int v)
    {
        return (h ^ (uint)v) * 16777619u;
    }

    private static BossBase Boss()
    {
        if (W == null)
            return null;
        foreach (var sp in W.Spawners)
        {
            var a = sp as BossArena;
            if (a != null)
                return a.Boss;
        }
        return null;
    }

    private static void PollCandles()
    {
        var props = W.GroupList(eEntityGroup.Prop);
        for (int i = 0; i < props.Count; i++)
        {
            var c = props[i] as CandleProp;
            if (c != null && c.broken && brokenSeen.Add(c))
            {
                brokenPending.Add(c);
                Ev("vỡ " + c.objectId + " (" + c.X + "," + c.Y + ")");
            }
        }
        for (int i = brokenPending.Count - 1; i >= 0; i--)
        {
            var c = brokenPending[i];
            if (c.dropped != eItemId.None)
            {
                Ev("rơi " + c.objectId + " ⇒ " + c.dropped + " sau " + c.breakAge + " tick");
                brokenPending.RemoveAt(i);
            }
        }
    }

    private static void PollTallySfx()
    {
        if (AudioManager.I == null)
            return;
        if (tallyChannel == null)
        {
            var f = typeof(AudioManager).GetField("sfxChannel", BindingFlags.Instance | BindingFlags.NonPublic);
            tallyChannel = f != null ? f.GetValue(AudioManager.I) as AudioSource : null;
            if (tallyChannel == null)
                return;
        }
        if (tallyChannel.isPlaying && (tallyChannel.clip != tallyLastClip || tallyChannel.time < tallyLastTime - 0.001f))
            Ev("SFX(kênh đơn) " + (tallyChannel.clip != null ? tallyChannel.clip.name : "?"));
        tallyLastClip = tallyChannel.clip;
        tallyLastTime = tallyChannel.isPlaying ? tallyChannel.time : 0f;
    }

    // ---- sự kiện game

    private static void SubscribeGameEvents()
    {
        StageFlow.Milestone += OnMilestone;
        EnemySpawner.Spawned += OnSpawned;
        EnemyBase.Defeated += OnDefeated;
        BossBase.BossDefeated += OnBossDefeated;
        BossBase.CrystalSpawned += OnCrystalSpawned;
        ItemEffects.Picked += OnPicked;
        BreakableBlock.Broken += OnBlock;
    }

    private static void UnsubscribeGameEvents()
    {
        StageFlow.Milestone -= OnMilestone;
        EnemySpawner.Spawned -= OnSpawned;
        EnemyBase.Defeated -= OnDefeated;
        BossBase.BossDefeated -= OnBossDefeated;
        BossBase.CrystalSpawned -= OnCrystalSpawned;
        ItemEffects.Picked -= OnPicked;
        BreakableBlock.Broken -= OnBlock;
    }

    private static void OnMilestone(StageFlow f, string m) { Ev("flow " + m); }
    private static void OnSpawned(EnemySpawner sp, EnemyBase e) { Ev("sinh " + (e != null ? e.GetType().Name + " (" + e.X + "," + e.Y + ")" : "?") + " từ " + (sp != null ? sp.name : "?")); }
    private static void OnDefeated(EnemyBase e) { Ev("hạ " + e.GetType().Name + " (" + e.X + "," + e.Y + ") +" + e.Score); }
    private static void OnBossDefeated(BossBase b) { Ev("boss hạ +" + b.Score); }
    private static void OnCrystalSpawned(BossBase b, ItemPickup p) { Ev("quả cầu hiện " + (p != null ? p.X + "," + p.Y : "?")); }
    private static void OnPicked(eItemId id, string log) { Ev("nhặt " + id + " " + log); }
    private static void OnBlock(BreakableBlock b) { Ev("khối vỡ " + (b != null ? b.ObjectId : "?")); }

    private static void OnLog(string msg, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            if (ConsoleErrors.Count < 200)
                ConsoleErrors.Add("[" + type + "] t" + Tick + " " + msg);
            return;
        }
        if (!Running || msg == null || !msg.StartsWith("[CVTI]"))
            return;
        int i = msg.IndexOf("): ", StringComparison.Ordinal);
        string body = i >= 0 ? msg.Substring(i + 3) : msg;
        Ev(body);
    }

    private static void PostTick(int frame)
    {
        if (!Running || Mode != eMode.KeyLog || NesPad.I == null)
            return;
        int held = NesPad.I.HeldMask, pressed = NesPad.I.PressedMask;
        if (held != 0 || pressed != 0 || keyLastHeld != 0)
            keyLog.Add(frame + "\t" + Bits(held) + "\t" + Bits(pressed));
        keyLastHeld = held;
    }

    private static void Pump()
    {
        if (!Running)
            return;
        double dt = Time.unscaledDeltaTime * 1000.0;
        if (dt > 0 && dt < 1000)
        {
            frameSum += dt;
            frameCount++;
            if (dt > frameMax)
                frameMax = dt;
        }
        if (Mode == eMode.Shots)
            PumpShots();
    }

    private static void Capture(string name)
    {
        string p = Path.Combine(shotDir, name + ".png");
        if (File.Exists(p))
            File.Delete(p);
        ScreenCapture.CaptureScreenshot(p);
        Mark("ảnh " + name);
    }

    private static void Ev(string m)
    {
        var s = GameSession.Current;
        var simon = S;
        string where = InStage ? Sec + " cx " + Cx + " w" + W.TickCount + " S " + (simon != null ? simon.X + "," + simon.Y : "?") : (GameManager.I != null ? GameManager.I.State.ToString() : "?");
        Events.Add(Tick + "\tL" + Lof(NesClock.Frame) + "\t" + where + "\t" + m + "\tT" + s.time.ToString("0000") + " ♥" + s.hearts + " SC" + s.score);
    }

    private static void Mark(string m)
    {
        var s = GameSession.Current;
        var simon = S;
        Marks.Add(Tick + "\tL" + Lof(NesClock.Frame) + "\t" + m + "\t" + (InStage ? Sec + " cx " + Cx + " Simon " + (simon != null ? simon.X + "," + simon.Y : "?") + " t" + W.TickCount : (GameManager.I != null ? GameManager.I.State.ToString() : "?"))
                  + "\tSCORE " + s.score.ToString("000000") + " TIME " + s.time.ToString("0000") + " ST " + s.stage.ToString("00") + " ♥" + s.hearts
                  + " P" + s.lives + " HP " + s.playerHp + " E " + s.enemyHp + " roi " + s.whipLevel + " " + s.subWeapon + "×" + s.shotMultiplier);
    }

    // ================================================================ 2) Spam phím

    private static System.Random spamRng;
    private static int spamTicks, spamStart, spamScreenChanges, spamPauses, spamWorldTicks0;
    private static bool spamWithStart;
    private static eGameScreen spamLastScreen;

    /// <summary>
    /// warpSection ≠ null ⇒ ngay sau khi vào màn: Warp + Teleport Simon tới (warpX, warpY); lives &gt; 0 ⇒ đặt số mạng; hp &gt; 0 ⇒ đặt máu
    /// (lượt spam PB/CB của routine 16/21 đặt tay các số này — CVM2 00 đưa vào tham số).
    /// </summary>
    public static string StartSpam(int stage, int ticks, int seed, bool withStart, string warpSection = null, int warpX = 0, int warpY = 0, int lives = 0, int hp = 0)
    {
        string why;
        if (!CanStart(out why))
            return why;
        ResetCommon(eMode.Spam);
        spamRng = new System.Random(seed);
        spamTicks = ticks;
        spamWithStart = withStart;
        spamScreenChanges = spamPauses = 0;
        Application.runInBackground = true;
        CvgpDebug.CheatInvincible = false;
        SubscribeGameEvents();
        Hook();
        GameRules.NewGame();
        GameSession.Current.stage = Mathf.Clamp(stage, 1, StageScreen.LAST_PLAYABLE_STAGE);   // CVM2 11: + Block 2 (stage 4–6)
        if (!GameManager.I.GoTo(eGameScreen.Stage1, 0))
        {
            Finish("GameManager đang chuyển cảnh");
            return Error;
        }
        spamLastScreen = GameManager.I.State;
        spamStart = NesClock.Frame;
        spamWorldTicks0 = W != null ? W.TickCount : 0;
        if (warpSection != null && W != null && S != null)
            foreach (var sec in W.Sections)
                if (sec.name == warpSection)
                {
                    W.Warp(sec, warpX, warpY);
                    S.Teleport(warpX, warpY, 1);
                    break;
                }
        if (lives > 0)
            GameSession.Current.lives = lives;
        if (hp > 0)
        {
            GameSession.Current.playerHp = hp;
            GameSession.Current.NotifyChanged();
        }
        Running = true;
        Mark("spam stage " + stage + " " + ticks + " tick seed " + seed + (withStart ? " (cả Start/Select)" : " (không Start/Select)"));
        return "spam bắt đầu";
    }

    private static void PreTickSpam(int frame)
    {
        Tick = frame - spamStart;
        if (Tick > spamTicks)
        {
            Mark("spam xong: đổi màn " + spamScreenChanges + ", pause bật " + spamPauses + ", tick thế giới " + (W != null ? W.TickCount.ToString() : "-")
                 + ", Simon " + (S != null ? S.Fsm.CurState + " " + S.X + "," + S.Y : "-") + ", section " + Sec);
            Finish(null);
            return;
        }
        int mask = spamRng.Next(0, 1 << NesPad.BUTTON_COUNT);
        if (!spamWithStart)
            mask &= ~((1 << (int)eNesButton.Start) | (1 << (int)eNesButton.Select));
        for (int b = 0; b < NesPad.BUTTON_COUNT; b++)
            if ((mask & (1 << b)) != 0)
                NesPad.I.Inject((eNesButton)b, 1);
        var gm = GameManager.I;
        if (gm.State != spamLastScreen && !gm.IsTransitioning)
        {
            spamLastScreen = gm.State;
            spamScreenChanges++;
            Mark("màn " + gm.State);
        }
        if (W != null && W.Paused && InStage)
            spamPauses++;
        if (InStage && Sec != lastSection)
        {
            lastSection = Sec;
            Mark("section " + Sec);
        }
    }

    // ================================================================ 3) Ghi phím thật

    private static readonly List<string> keyLog = new List<string>();
    private static int keyLastHeld;

    public static string StartKeyLog()
    {
        string why;
        if (!CanStart(out why))
            return why;
        ResetCommon(eMode.KeyLog);
        keyLog.Clear();
        keyLastHeld = 0;
        Application.runInBackground = true;
        Hook();
        f0 = NesClock.Frame;
        Running = true;
        return "ghi phím từ frame " + f0 + " — joystick: [" + string.Join(", ", Input.GetJoystickNames()) + "]";
    }

    public static string StopKeyLog()
    {
        if (Mode != eMode.KeyLog)
            return "không ghi phím";
        if (Running)
            Finish(null);
        var sb = new StringBuilder();
        sb.Append("frame\theld\tpressed\n");
        foreach (var l in keyLog)
            sb.Append(l).Append('\n');
        return sb.ToString();
    }

    private static string Bits(int m)
    {
        if (m == 0)
            return "-";
        var sb = new StringBuilder();
        for (int b = 0; b < NesPad.BUTTON_COUNT; b++)
            if ((m & (1 << b)) != 0)
            {
                if (sb.Length > 0) sb.Append('+');
                sb.Append(((eNesButton)b).ToString());
            }
        return sb.ToString();
    }

    // ================================================================ 4) Ảnh nền / HUD (thế giới dừng)

    /// <summary>Việc chụp: section + cx (nền) hoặc trạng thái HUD.</summary>
    public class ShotJob
    {
        public string name, section;
        public int cx = -1;
        public bool hideSprites;
        public Action apply;
    }

    private static List<ShotJob> jobs;
    private static int jobIndex, jobWait;
    private static string jobFile;
    private static readonly List<SpriteRenderer> hidden = new List<SpriteRenderer>();
    private static bool prevPaused;

    /// <summary>Chụp nền: jobs "Section:cx,cx,…;Section:cx…" — ẩn sprite động, giữ Tilemap BG/FG + HUD.</summary>
    public static string StartTerrain(string dir, string spec)
    {
        var list = new List<ShotJob>();
        foreach (var part in spec.Split(';'))
        {
            var kv = part.Split(':');
            if (kv.Length != 2)
                continue;
            foreach (var c in kv[1].Split(','))
                list.Add(new ShotJob { name = "terrain_" + kv[0] + "_cx" + c.Trim(), section = kv[0].Trim(), cx = int.Parse(c.Trim()), hideSprites = true });
        }
        return StartShots(dir, list);
    }

    /// <summary>Chụp HUD theo bảng giá trị: "k:score,time,stage,hearts,lives,php,ehp,sub,mult;…" (sub = tên eSubWeapon).</summary>
    public static string StartHud(string dir, string spec)
    {
        var list = new List<ShotJob>();
        foreach (var part in spec.Split(';'))
        {
            var kv = part.Split(':');
            if (kv.Length != 2)
                continue;
            var v = kv[1].Split(',');
            int score = int.Parse(v[0]), time = int.Parse(v[1]), stage = int.Parse(v[2]), hearts = int.Parse(v[3]), lives = int.Parse(v[4]),
                php = int.Parse(v[5]), ehp = int.Parse(v[6]), mult = int.Parse(v[8]);
            var sub = (eSubWeapon)Enum.Parse(typeof(eSubWeapon), v[7]);
            list.Add(new ShotJob
            {
                name = "hud_" + kv[0].Trim(),
                apply = () =>
                {
                    var s = GameSession.Current;
                    s.score = score; s.time = time; s.stage = stage; s.hearts = hearts; s.lives = lives;
                    s.playerHp = s.shownPlayerHp = php; s.enemyHp = s.shownEnemyHp = ehp;
                    s.subWeapon = sub; s.shotMultiplier = mult; s.shotIconVisible = true;
                    s.NotifyChanged();
                }
            });
        }
        return StartShots(dir, list);
    }

    private static string StartShots(string dir, List<ShotJob> list)
    {
        string why;
        if (!CanStart(out why))
            return why;
        if (!InStage)
            return "cần đang ở màn chơi (Stage)";
        ResetCommon(eMode.Shots);
        shotDir = dir;
        Directory.CreateDirectory(dir);
        jobs = list;
        jobIndex = 0;
        jobWait = 0;
        jobFile = null;
        prevPaused = W.Paused;
        W.Paused = true;
        Application.runInBackground = true;
        Hook();
        Running = true;
        return "chụp " + list.Count + " ảnh";
    }

    private static void PumpShots()
    {
        if (jobFile != null)
        {
            if (File.Exists(jobFile) && new FileInfo(jobFile).Length > 0 && ++jobWait >= 3)
            {
                jobFile = null;
                jobIndex++;
                jobWait = 0;   // việc kế: dựng cảnh lại từ đầu
            }
            return;
        }
        if (jobIndex >= jobs.Count)
        {
            ShowSprites();
            if (W != null)
                W.Paused = prevPaused;
            Finish(null);
            return;
        }
        var j = jobs[jobIndex];
        if (jobWait == 0)
        {
            // dựng cảnh, chờ 2 vòng update (PixelPerfectCamera + HUD) rồi chụp
            if (j.section != null)
            {
                LevelSection target = null;
                foreach (var s in W.Sections)
                    if (s.name == j.section)
                        target = s;
                if (target == null)
                {
                    Mark("không có section " + j.section);
                    jobIndex++;
                    return;
                }
                if (W.Section != target)
                {
                    W.Warp(target, target.data.simonStart.x, target.data.simonStart.y);
                    if (S != null)
                        S.Teleport(target.data.simonStart.x, target.data.simonStart.y, 1);
                }
                W.CameraCtl.Target = null;
                W.CameraCtl.SnapTo(j.cx);
            }
            if (j.apply != null)
                j.apply();
            if (j.hideSprites)
                HideSprites();
            else
                ShowSprites();
            jobWait = -3;
            return;
        }
        if (jobWait < 0)
        {
            jobWait++;
            if (jobWait < 0)
                return;
        }
        jobFile = Path.Combine(shotDir, j.name + ".png");
        if (File.Exists(jobFile))
            File.Delete(jobFile);
        ScreenCapture.CaptureScreenshot(jobFile);
        jobWait = 0;
        Mark("ảnh " + j.name + (j.section != null ? " (" + j.section + " cx " + Cx + ")" : ""));
        // jobWait = 0 + jobFile ≠ null ⇒ chờ file
    }

    // forceRenderingOff (không đụng enabled): SyncView của thực thể vẫn chạy khi thế giới dừng và tự bật lại renderer (Simon nháy).
    private static void HideSprites()
    {
        ShowSprites();
        foreach (var r in UnityEngine.Object.FindObjectsOfType<SpriteRenderer>())
        {
            string l = r.sortingLayerName;
            if (!r.forceRenderingOff && (l == "BackgroundFX" || l == "Character" || l == "Overlay"))
            {
                r.forceRenderingOff = true;
                hidden.Add(r);
            }
        }
    }

    private static void ShowSprites()
    {
        foreach (var r in hidden)
            if (r != null)
                r.forceRenderingOff = false;
        hidden.Clear();
    }

    // ================================================================ báo cáo

    public static string Report()
    {
        var sb = new StringBuilder();
        sb.Append(Mode).Append(Running ? " ĐANG CHẠY" : Finished ? " xong" : "").Append(" @").Append(Fps).Append(" fps — tick ").Append(Tick)
          .Append(", hash ").Append(Hash.ToString("x8")).Append(Error.Length > 0 ? ", LỖI: " + Error : "").Append('\n');
        sb.Append("trước STAGE 04: tick ").Append(stage4Tick).Append(" hash ").Append(PreStage4Hash.ToString("x8")).Append(" (chuẩn 8464 / " + RegressionRunner.PRE4 + " — CVM3 04)")
          .Append(" | STAGE 07 ở tick ").Append(stage7Tick).Append('\n');
        sb.Append("checkpoints: ");
        foreach (var c in Checkpoints)
            sb.Append(c.ToString("x8")).Append(' ');
        sb.Append("\nGameObject sau mỗi lần: ");
        foreach (var c in ObjectCounts)
            sb.Append(c).Append(' ');
        sb.Append("\nkhung: ").Append(frameCount).Append(" khung, TB ").Append(frameCount > 0 ? (frameSum / frameCount).ToString("0.00") : "-").Append(" ms, max ")
          .Append(frameMax.ToString("0.0")).Append(" ms");
        var w = W;
        if (w != null && w.MeasuredSteps > 0)
            sb.Append(" | Step TB ").Append((w.TotalStepMs / w.MeasuredSteps).ToString("0.000")).Append(" ms, max ").Append(w.MaxStepMs.ToString("0.00"))
              .Append(" ms, GC/Step tổng ").Append(w.TotalStepAlloc).Append(" B (").Append(w.AllocSteps).Append(" step có cấp phát / ").Append(w.MeasuredSteps).Append(')');
        sb.Append("\nTIME đổi ở tick lộ trình: ").Append(timeChangeTicks.Count).Append(" lần");
        if (timeChangeTicks.Count > 1)
        {
            var gaps = new Dictionary<int, int>();
            for (int i = 1; i < timeChangeTicks.Count; i++)
            {
                int g = timeChangeTicks[i] - timeChangeTicks[i - 1];
                int c;
                gaps.TryGetValue(g, out c);
                gaps[g] = c + 1;
            }
            sb.Append(" — khoảng cách:");
            foreach (var kv in gaps)
                sb.Append(' ').Append(kv.Key).Append('×').Append(kv.Value);
        }
        sb.Append("\nlỗi console: ").Append(ConsoleErrors.Count).Append('\n');
        foreach (var e in ConsoleErrors)
            sb.Append("  ").Append(e).Append('\n');
        foreach (var m in Marks)
            sb.Append(m).Append('\n');
        return sb.ToString();
    }

    public static string SaveReport(string path)
    {
        var sb = new StringBuilder(Report());
        sb.Append("\n# sự kiện (tick lộ trình \\t L luồng \\t nơi \\t sự kiện \\t HUD)\n");
        foreach (var e in Events)
            sb.Append(e).Append('\n');
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        return path + " (" + Events.Count + " sự kiện)";
    }
}
#endif
