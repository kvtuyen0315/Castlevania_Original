#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Harness NGHIỆM THU Map 3 (CVM3 12) — CHỈ Editor, scene Main.unity khi Play. Chỉ ĐO, không đổi gameplay/hash. Qua MCP:
///   Vết thụ động phần Map 3 của luồng thật: Map2AcceptProbe.MinStage = 7; Map2AcceptProbe.StartTelemetry(path) + Map1AcceptProbe.StartFlow(60, dir).
///   Run(names, outPath) ⇒ kịch bản trên MAP THẬT Block 3 (StageScreen thật, NesClock tắt — tự Step như Map2AcceptProbe.Run), không bất tử trừ
///        kịch bản ghi rõ: hố WEB + mép rơi, cầu hỏng St9, knockback hất xuống hố (WalkwayB, cầu hỏng St9), 3 vạch/đòn ⇒ chết ở đòn 6 (WalkwayA),
///        đồng hồ ở St8 (đóng băng Medusa Head), quỹ đạo lọ nước thánh phòng Mummy, GAME OVER ⇒ CONTINUE St9, bí mật túi St7 / St8.
///   Ledger(ledgerFile, outPath) ⇒ sổ điểm video Map_3 (Tools/cvm3/accept12_ledger.py): đặt trạng thái đầu Map_3 (SCORE 44670, P4, tim 5, đồng hồ,
///        roi 3, STAGE 07 TIME 500) rồi áp từng sự kiện HUD video qua ĐÚNG đường code game; tally Block 3 phải ra 73630 (trừ luật chưa dựng — ghi rõ).
/// </summary>
public static class Map3AcceptProbe
{
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

    private static StageWorld W => StageWorld.Current;
    private static SimonController S => W != null ? W.Player as SimonController : null;
    private static string Sec => W != null && W.Section != null ? W.Section.name : "";
    private static int Cx => W != null && W.CameraCtl != null ? W.CameraCtl.X : -1;
    private static bool InStage => GameManager.I != null && GameManager.I.State == eGameScreen.Stage1 && !GameManager.I.IsTransitioning && W != null;

    const int U = 1 << (int)eNesButton.Up, D = 1 << (int)eNesButton.Down, L = 1 << (int)eNesButton.Left, R = 1 << (int)eNesButton.Right,
              A = 1 << (int)eNesButton.A, B = 1 << (int)eNesButton.B;

    private class Scn
    {
        public string name;
        public int stage, lives = 3, maxTicks = 3000;
        public string warpSec;
        public int wx, wy, wf;
        public bool invincible;                // cheat bất tử (kịch bản không đo sát thương — ghi rõ trong báo cáo)
        public eSubWeapon sub = eSubWeapon.None;
        public int hearts = -1;
        public Func<int, int> input;           // tuổi (tick từ lúc sẵn sàng) ⇒ mask
        public Func<string> check;             // null = đạt; chuỗi = lý do trượt
        public Func<bool> done;                // xong sớm
        public bool endOnRespawn = true;
    }

    private static Scn cur;
    private static int age, ready, gameOverAge;
    // ghi trong lúc chạy
    private static readonly List<string> deaths = new List<string>();
    private static readonly List<string> respawns = new List<string>();
    private static readonly List<int> hpDrops = new List<int>();
    private static readonly List<int> hpDropAges = new List<int>();
    private static readonly List<string> hits = new List<string>();
    private static bool sawGameOver, continued;
    private static int lastHp, scoreAtReady, heartsAtReady;
    private static eGameScreen lastScreen;
    private static string afterRespawn = "", afterContinue = "", atReady = "";
    private static int kbStartX = int.MinValue, kbMaxDx, lastDropAge = -1;
    private static readonly List<int> kbs = new List<int>();
    private static int fallAge = -1, fallX = int.MinValue, fallY;
    // lọ nước thánh / lửa
    private static int holyX0 = int.MinValue, holyY0, holyThrowAge = -1, fireAge = -1, fireX, fireY, holyLastN = -1;
    private static readonly List<string> holyTrack = new List<string>();
    private static Entity holyBottle;
    // đồng hồ
    private static int pressAge = -1;
    private static readonly Dictionary<Entity, List<string>> mhTracks = new Dictionary<Entity, List<string>>();
    private static readonly List<int> timeChangeAges = new List<int>();
    private static int lastTime = -1, heartsBeforePress = -1, heartsAfterPress = -1;
    // túi bí mật
    private static ItemPickup bag;
    private static int bagSpawnAge = -1, bagRestAge = -1, bagGoneAge = -1, bagLastY = int.MinValue, bagFirstY, bagRestY, bagX, bagOffscreenTicks, crouchAge = -1;
    private static bool bagPicked;
    private static readonly List<string> bagNotes = new List<string>();

    /// <summary>≠ null ⇒ chụp Game view ở mốc kịch bản (đòn đầu/đòn 6 +12, bắt đầu rơi, đồng hồ +90, lọ +24, lửa +2, túi nghỉ +2, túi ngoài màn) —
    /// dừng bước tick trong khung đó (ảnh = trạng thái đúng tick).</summary>
    public static string ShotDir;
    private static bool shotThisFrame;
    private static readonly HashSet<string> shotsDone = new HashSet<string>();

    private static void Shot(string tag)
    {
        if (ShotDir == null || cur == null || !shotsDone.Add(tag))
            return;
        Directory.CreateDirectory(ShotDir);
        string p = Path.Combine(ShotDir, cur.name + "_" + tag + "_a" + age + ".png");
        if (File.Exists(p))
            File.Delete(p);
        ScreenCapture.CaptureScreenshot(p);
        trace.Add(age + "\tảnh " + Path.GetFileName(p) + " " + State());
        shotThisFrame = true;
    }

    public static string ScenarioNames() => "pit_web,pit_bridge_st9,knock_bridge_st9,knock_pit_wb,damage_wa,stopwatch_wa,holy_mm,gameover_st9,secret_st7,secret_st8,secret_st8_off";

    private static Scn Make(string n)
    {
        switch (n)
        {
            case "pit_web":
                // WEB khe [96,160) ở sàn y 160 (CVM3 05 marker pit_10_6): đứng x 80 đi phải ⇒ rơi ở x 102 (dò sàn X±6 — CVM3 04) ⇒ đầu STAGE 07
                return new Scn { name = n, stage = 7, warpSec = "WalkwayEntranceB", wx = 80, wy = 160, wf = 1, input = a => R,
                    check = () => Join(Expect(eDeathCause.Fall, "WalkwayEntranceA", 720, 160, -1, 512, 7, 2, 500), FallAt(102)) };
            case "pit_bridge_st9":
                // cầu hỏng St9 khe 1 [768,800) (MummyMan pit_6_48): đứng bục x 760 đi phải ⇒ rơi ở x 774 ⇒ đầu STAGE 09 MummyMan (48,96) phải
                return new Scn { name = n, stage = 9, warpSec = "MummyMan", wx = 760, wy = 96, wf = 1, input = a => R,
                    check = () => Join(Expect(eDeathCause.Fall, "MummyMan", 48, 96, 1, 0, 9, 2, 500), FallAt(774)) };
            case "knock_bridge_st9":
                // cầu hỏng St9: đứng giữa bục [800,832) x 816 quay PHẢI (Skeleton S5 sinh ở (815,96), Raven #11 (747,41) gần) ⇒ đòn đầu tiên
                // ⇒ knockback lùi TRÁI 34 px ⇒ x 782 giữa khe [768,800) ⇒ rơi hố ⇒ đầu STAGE 09
                return new Scn { name = n, stage = 9, warpSec = "MummyMan", wx = 816, wy = 96, wf = 1, input = a => 0, maxTicks = 1500,
                    check = () => Join(Expect(eDeathCause.Fall, "MummyMan", 48, 96, 1, 0, 9, 2, 500), HitBeforeFall()) };
            case "knock_pit_wb":
                // WalkwayB cầu y 128, bờ phải khe [624,656) x 680 quay PHẢI (Bone Tower x 735 trước mặt, Raven #6 (663,121) sau lưng)
                // ⇒ đòn đầu ⇒ lùi TRÁI 34 px ⇒ x 646 giữa khe ⇒ rơi hố ⇒ đầu STAGE 08 = WalkwayA (48,128) phải
                return new Scn { name = n, stage = 8, warpSec = "WalkwayB", wx = 680, wy = 128, wf = 1, input = a => 0, maxTicks = 2000,
                    check = () => Join(Expect(eDeathCause.Fall, "WalkwayA", 48, 128, 1, 0, 8, 2, 500), HitBeforeFall()) };
            case "damage_wa":
                // đầu STAGE 08 (WalkwayA) đi phải tới x 400 (vùng Medusa Head [144, …]) rồi đứng quay phải: Medusa Head trước mặt ⇒ mỗi đòn
                // −3 vạch (ConfigBlock block 3) ⇒ 5 đòn = 15, đòn 6 ⇒ chết Damage ⇒ hồi sinh đầu STAGE 08
                return new Scn { name = n, stage = 8, input = a => S != null && S.X < 400 && hpDrops.Count == 0 ? R : 0, maxTicks = 4000,
                    endOnRespawn = true, check = CheckDamage3 };
            case "stopwatch_wa":
                // St8 WalkwayA: cầm đồng hồ, tim 20, đi tới x 400; khi có Medusa Head sống ≥ 30 tick ⇒ bấm ↑+B 1 tick ⇒ đo đóng băng trên map thật
                // (bất tử: chỉ đo thời lượng băng — trúng đòn sẽ chặn ném)
                return new Scn { name = n, stage = 8, invincible = true, sub = eSubWeapon.Stopwatch, hearts = 20, maxTicks = 1500, endOnRespawn = false,
                    input = StopwatchInput, check = CheckStopwatch, done = () => pressAge >= 0 && age > pressAge + 420 };
            case "holy_mm":
                // phòng Mummy (chưa khoá): đứng khối cao trái (1302,112) quay PHẢI như video Map_3 3:22 ⇒ ném 1 lọ ⇒ sàn phòng y 176
                // (tâm lọ lúc sinh cao hơn sàn 83 px — video: vỡ n49, lửa ở X + 73)
                return new Scn { name = n, stage = 9, invincible = true, warpSec = "MummyMan", wx = 1302, wy = 112, wf = 1, sub = eSubWeapon.HolyWater,
                    hearts = 20, maxTicks = 220, endOnRespawn = false, input = a => a == 5 ? U | B : 0, check = CheckHoly };
            case "gameover_st9":
                // P 1 + rơi khe cầu hỏng ⇒ GAME OVER ⇒ Start (CONTINUE) ⇒ đầu STAGE 09, P 3, điểm 0
                return new Scn { name = n, stage = 9, lives = 1, warpSec = "MummyMan", wx = 760, wy = 96, wf = 1, input = a => R, maxTicks = 2500,
                    endOnRespawn = false, check = CheckGameOver };
            case "secret_st7":
                // WEB bục thấp nhất (mép phải x 537, y 176) ngồi 40 tick ⇒ túi trồi ở bục cao trái (424, Y 129); KHÔNG nhặt ⇒ đo tuổi thọ từ lúc nghỉ
                // (bất tử: Raven #3 đậu sát chỗ ngồi — chỉ đo bí mật)
                return new Scn { name = n, stage = 7, invincible = true, warpSec = "WalkwayEntranceB", wx = 537, wy = 176, wf = 1, maxTicks = 420,
                    endOnRespawn = false, input = a => a < 40 ? D : 0, check = () => CheckBag(424, 129, false), done = () => bagGoneAge >= 0 };
            case "secret_st8":
                // WalkwayB cầu ngay trên 2 khối (x 246, y 128) ngồi 20 tick ⇒ túi trồi ở mút trái cầu (128, Y 129) ⇒ đi trái nhặt (+1000) như video 1:55.9
                return new Scn { name = n, stage = 8, invincible = true, warpSec = "WalkwayB", wx = 246, wy = 128, wf = 1, maxTicks = 400,
                    endOnRespawn = false, input = a => a < 20 ? D : (bagPicked ? 0 : L), check = () => CheckBag(128, 129, true), done = () => bagPicked && age > 10 };
            case "secret_st8_off":
                // như trên nhưng ngồi xong đi PHẢI 100 tick (túi ra NGOÀI màn trái) rồi đứng ⇒ túi vẫn còn (keepOffscreen) tới hết tuổi thọ
                return new Scn { name = n, stage = 8, invincible = true, warpSec = "WalkwayB", wx = 246, wy = 128, wf = 1, maxTicks = 420,
                    endOnRespawn = false, input = a => a < 20 ? D : (a < 120 ? R : 0), check = () => CheckBag(128, 129, false), done = () => bagGoneAge >= 0 };
        }
        return null;
    }

    public static string Run(string names, string outPath)
    {
        if (!EditorApplication.isPlaying || GameManager.I == null || NesClock.I == null || NesPad.I == null)
            return "cần Play Main.unity";
        if (Running || Map1AcceptProbe.Running || Map2RouteProbe.Running || Map3RouteProbe.Running || Map2AcceptProbe.Running)
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
        SetInvincible(false);
        stepMethod = typeof(NesClock).GetMethod("Step", BindingFlags.Instance | BindingFlags.NonPublic);
        NesClock.I.enabled = false;
        Application.logMessageReceived += OnLog;
        Running = true;
        EditorApplication.update += Pump;
        Next();
        return "chạy " + names;
    }

    private static void SetInvincible(bool on)
    {
        CvgpDebug.CheatInvincible = on;
        EditorPrefs.SetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, on);
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
        hpDropAges.Clear();
        hits.Clear();
        sawGameOver = continued = false;
        afterRespawn = afterContinue = atReady = "";
        kbStartX = int.MinValue;
        kbMaxDx = 0;
        lastDropAge = -1;
        kbs.Clear();
        gameOverAge = 0;
        fallAge = -1;
        fallX = int.MinValue;
        holyX0 = int.MinValue;
        holyThrowAge = fireAge = holyLastN = -1;
        holyTrack.Clear();
        holyBottle = null;
        pressAge = -1;
        mhTracks.Clear();
        timeChangeAges.Clear();
        lastTime = heartsBeforePress = heartsAfterPress = -1;
        bag = null;
        bagSpawnAge = bagRestAge = bagGoneAge = crouchAge = -1;
        bagLastY = int.MinValue;
        bagOffscreenTicks = 0;
        bagPicked = false;
        bagNotes.Clear();
        shotsDone.Clear();
        SetInvincible(false);
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

    private static StageRules boundScnRules;
    private static StageWorld boundScnWorld;

    private static void BindScn()
    {
        var r = StageRules.Current;
        if (r != null && r != boundScnRules)
        {
            if (boundScnRules != null)
            {
                boundScnRules.LifeLost -= ScnLifeLost;
                boundScnRules.Respawned -= ScnRespawned;
            }
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
            hits.Add("@" + age + " dmg " + h.damage + " từ " + (h.attacker != null ? h.attacker.GetType().Name + " (" + h.attacker.X + "," + h.attacker.Y + ")" : "?")
                     + " Simon (" + S.X + "," + S.Y + ") f" + S.Facing);
    }

    private static string State()
    {
        var s = GameSession.Current;
        return Sec + " (" + (S != null ? S.X + "," + S.Y : "?") + ") f" + (S != null ? S.Facing : 0) + " cx " + Cx + " ST" + s.stage + " P" + s.lives + " SC" + s.score
               + " T" + s.time + " ♥" + s.hearts + " HP" + s.playerHp + " roi" + s.whipLevel + " " + s.subWeapon;
    }

    private static string Join(params string[] parts)
    {
        var l = new List<string>();
        foreach (var p in parts)
            if (!string.IsNullOrEmpty(p))
                l.Add(p);
        return l.Count == 0 ? null : string.Join("; ", l.ToArray());
    }

    /// <summary>Kỳ vọng chết + hồi sinh: nguyên nhân, section/vị trí/hướng/cx đầu stage, stage, mạng sau, TIME sau hồi sinh.</summary>
    private static string Expect(eDeathCause cause, string sec, int x, int y, int facing, int cx, int stage, int lives, int time)
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
        if (afterRespawn.IndexOf(" T" + time + " ", StringComparison.Ordinal) < 0)
            errs.Add("TIME sau hồi sinh ≠ " + time);
        return errs.Count == 0 ? null : string.Join("; ", errs.ToArray());
    }

    private static string FallAt(int x)
    {
        if (fallX == int.MinValue)
            return "không thấy tick bắt đầu rơi";
        return fallX == x ? null : "bắt đầu rơi ở x " + fallX + " ≠ " + x + " (dò sàn X±6)";
    }

    private static string HitBeforeFall()
    {
        if (hits.Count == 0)
            return "không có đòn trúng trước khi rơi";
        if (fallAge >= 0 && lastDropAge > fallAge)
            return "đòn trúng SAU khi rơi";
        return null;
    }

    private static string CheckGameOver()
    {
        var errs = new List<string>();
        if (deaths.Count == 0 || deaths[0].IndexOf("GAMEOVER", StringComparison.Ordinal) < 0 || !deaths[0].StartsWith("Fall"))
            errs.Add("chết '" + (deaths.Count > 0 ? deaths[0] : "-") + "' ≠ Fall GAMEOVER");
        if (!sawGameOver)
            errs.Add("không thấy màn GAME OVER");
        if (!continued)
            errs.Add("không CONTINUE");
        string want = "MummyMan (48,96) f1 cx 0 ST9 P3 SC0 T500";
        if (!afterContinue.StartsWith(want))
            errs.Add("sau CONTINUE '" + afterContinue + "' ≠ '" + want + " …'");
        return errs.Count == 0 ? null : string.Join("; ", errs.ToArray());
    }

    private static string CheckDamage3()
    {
        var errs = new List<string>();
        if (hpDrops.Count == 0)
            errs.Add("không trúng đòn nào");
        int sum = 0;
        for (int i = 0; i < hpDrops.Count; i++)
        {
            sum += hpDrops[i];
            bool last = sum >= GameSession.MAX_HP;
            // đòn cuối chỉ lấy phần máu còn lại (16 = 5 × 3 + 1)
            if (hpDrops[i] != 3 && !(last && hpDrops[i] == GameSession.MAX_HP - (sum - hpDrops[i])))
                errs.Add("đòn " + (i + 1) + " mất " + hpDrops[i] + " vạch ≠ 3");
        }
        if (deaths.Count == 0)
            errs.Add("không chết (mất tổng " + sum + " vạch qua " + hpDrops.Count + " đòn)");
        else
        {
            if (sum < GameSession.MAX_HP)
                errs.Add("chết khi mới mất " + sum + " vạch: " + deaths[0]);
            if (hpDrops.Count != 6)
                errs.Add("chết sau " + hpDrops.Count + " đòn ≠ 6");
            string e = Expect(eDeathCause.Damage, "WalkwayA", 48, 128, 1, 0, 8, 2, 500);
            if (e != null)
                errs.Add(e);
        }
        return errs.Count == 0 ? null : string.Join("; ", errs.ToArray());
    }

    // ---- đồng hồ
    private static bool pressed;

    private static int StopwatchInput(int a)
    {
        if (S == null)
            return 0;
        if (pressAge < 0)
        {
            if (S.X < 400)
                return R;
            // Medusa Head sống lâu nhất ≥ 30 tick ⇒ bấm
            foreach (var kv in mhTracks)
                if (kv.Key != null && kv.Key.Alive && kv.Value.Count >= 30)
                {
                    pressAge = a;
                    heartsBeforePress = GameSession.Current.hearts;
                    return U | B;
                }
        }
        return 0;
    }

    private static string CheckStopwatch()
    {
        var errs = new List<string>();
        if (pressAge < 0)
            return "không bấm được đồng hồ";
        if (heartsBeforePress - heartsAfterPress != 5)
            errs.Add("tim " + heartsBeforePress + " → " + heartsAfterPress + " (≠ −5)");
        int nFrozen = 0;
        foreach (var kv in mhTracks)
        {
            // mỗi dòng "age x y" — tìm đoạn đứng yên dài nhất bắt đầu ở/sau tick bấm
            var pts = new List<int[]>();
            foreach (var s in kv.Value)
            {
                var c = s.Split(' ');
                pts.Add(new[] { int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]) });
            }
            int runStart = -1, best = 0, bestStart = -1, bestEnd = -1;
            for (int i = 1; i < pts.Count; i++)
            {
                bool same = pts[i][1] == pts[i - 1][1] && pts[i][2] == pts[i - 1][2] && pts[i][0] == pts[i - 1][0] + 1;
                if (same)
                {
                    if (runStart < 0)
                        runStart = pts[i - 1][0];
                    int len = pts[i][0] - runStart;
                    if (len > best && runStart >= pressAge)
                    {
                        best = len;
                        bestStart = runStart;
                        bestEnd = pts[i][0];
                    }
                }
                else
                    runStart = -1;
            }
            if (best > 0)
            {
                nFrozen++;
                trace.Add("đồng hồ: Medusa Head đứng yên từ tuổi " + bestStart + " tới " + bestEnd + " (vị trí giữ " + best + " tick sau tick đầu; tick bấm = tuổi " + (pressAge + 1) + ")");
                // input tính ở tuổi a được xử lý trong bước tạo tuổi a+1 (= tick bấm, tim −5 cùng tick); StageWorld.FreezeEnemies(186) băng từ
                // TickCount+1 (nhóm Enemy chạy SAU Player trong tick bấm vẫn bước — CVM3 03 "băng 5–190" khi bấm tick 4) ⇒ enemy đứng các tick
                // bấm+1 … bấm+186 ⇒ vị trí cuối tick bấm giữ tới hết tick bấm+186 = 186 tick
                int pressTick = pressAge + 1;
                if (bestStart != pressTick || bestEnd - bestStart != 186)
                    errs.Add("Medusa Head đứng " + bestStart + "→" + bestEnd + " (" + (bestEnd - bestStart) + " tick) ≠ " + pressTick + "→" + (pressTick + 186));
            }
        }
        if (nFrozen == 0)
            errs.Add("không có Medusa Head nào đứng yên sau khi bấm");
        trace.Add("TIME đổi ở tuổi: " + string.Join(",", timeChangeAges.ConvertAll(x => x.ToString()).ToArray()));
        return errs.Count == 0 ? null : string.Join("; ", errs.ToArray());
    }

    // ---- lọ nước thánh
    private static string CheckHoly()
    {
        var errs = new List<string>();
        if (holyThrowAge < 0)
            return "không thấy lọ";
        // mô hình video (CVM3 04 §2e): x = X0 + floor(1.5 n); vy0 −1.5, g 33/256, vy += g trước rồi y += vy; tâm lọ lúc sinh = Y chân − 19
        // (gốc = điểm n0 quan sát được; độ lệch gốc so Simon kiểm riêng)
        int bad = 0, n = 0;
        var c0 = holyTrack[0].Split(' ');
        int ox = int.Parse(c0[1]), oy = int.Parse(c0[2]), on = int.Parse(c0[0]);
        if (on != 0 || ox != holyX0 || oy != holyY0 - 19)
            errs.Add("điểm sinh n" + on + " (" + ox + "," + oy + ") ≠ (X Simon " + holyX0 + ", Y chân − 19 = " + (holyY0 - 19) + ")");
        foreach (var t in holyTrack)
        {
            var c = t.Split(' ');
            int k = int.Parse(c[0]), x = int.Parse(c[1]), y = int.Parse(c[2]);
            int ex = ox + (384 * k >> 8);
            int fy = oy << 8, vy = -384;
            for (int j = 0; j < k; j++)
            {
                vy += 33;
                fy += vy;
            }
            int ey = fy >> 8;
            n++;
            if (x != ex || y != ey)
            {
                bad++;
                if (bad <= 5)
                    errs.Add("n" + k + " (" + x + "," + y + ") ≠ mô hình (" + ex + "," + ey + ")");
            }
        }
        trace.Add("lọ: " + n + " điểm, lệch mô hình " + bad + "; n cuối " + holyLastN + "; lửa tuổi " + fireAge + " ở (" + fireX + "," + fireY + "), X0 " + holyX0 + " Y0 " + holyY0);
        if (fireAge < 0)
            errs.Add("không có lửa");
        else
        {
            if (fireAge - holyThrowAge != 49)
                errs.Add("lọ vỡ (lửa hiện) ở n" + (fireAge - holyThrowAge) + " ≠ n49 (video)");
            if (fireX != ox + 73)
                errs.Add("lửa x " + fireX + " ≠ X0 + 73 = " + (ox + 73));
        }
        return errs.Count == 0 ? null : string.Join("; ", errs.ToArray());
    }

    // ---- túi bí mật
    private static string CheckBag(int x, int restY, bool expectPick)
    {
        var errs = new List<string>();
        if (bagSpawnAge < 0)
            return "túi không hiện (ngồi từ tuổi " + crouchAge + ")";
        if (bagX != x)
            errs.Add("túi x " + bagX + " ≠ " + x);
        if (bagRestAge < 0)
            errs.Add("túi chưa nghỉ");
        else if (bagRestY != restY)
            errs.Add("túi nghỉ Y " + bagRestY + " ≠ " + restY);
        if (bagFirstY - bagRestY != 14)
            errs.Add("quãng trồi " + (bagFirstY - bagRestY) + " ≠ 14");
        if (expectPick)
        {
            if (!bagPicked)
                errs.Add("không nhặt được túi");
            else if (GameSession.Current.score - scoreAtReady != 1000)
                errs.Add("SCORE +" + (GameSession.Current.score - scoreAtReady) + " ≠ +1000");
        }
        else
        {
            if (bagPicked)
                errs.Add("túi bị nhặt (không mong đợi)");
            if (bagGoneAge < 0)
                errs.Add("túi chưa biến mất trong " + age + " tick");
            else if (bagRestAge >= 0 && bagGoneAge - bagRestAge != 188 && bagGoneAge - bagRestAge != 189)
                errs.Add("túi mất sau " + (bagGoneAge - bagRestAge) + " tick từ lúc nghỉ ≠ 188");
        }
        bagNotes.Add("ngồi tuổi " + crouchAge + " ⇒ túi hiện tuổi " + bagSpawnAge + " (+" + (bagSpawnAge - crouchAge) + ") ở (" + bagX + "," + bagFirstY + ") ⇒ nghỉ tuổi "
                     + bagRestAge + " (+" + (bagRestAge - crouchAge) + ") Y " + bagRestY + " ⇒ " + (bagPicked ? "nhặt" : "mất") + " tuổi " + bagGoneAge
                     + (bagRestAge >= 0 && bagGoneAge >= 0 ? " (" + (bagGoneAge - bagRestAge) + " sau khi nghỉ)" : "") + "; ngoài màn " + bagOffscreenTicks + " tick");
        return errs.Count == 0 ? null : string.Join("; ", errs.ToArray());
    }

    private static void Pump()
    {
        if (!Running)
            return;
        shotThisFrame = false;
        for (int k = 0; k < 40 && Running && !shotThisFrame; k++)
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
            var flow = W != null ? W.GetComponent<StageFlow>() : null;
            if (InStage && S != null && S.Fsm != null && S.Fsm.HasState && flow != null && !flow.Busy && W.TickCount > 1)
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
                var s0 = GameSession.Current;
                if (cur.sub != eSubWeapon.None)
                    s0.SetSubWeapon(cur.sub);
                if (cur.hearts >= 0)
                    s0.hearts = cur.hearts;
                s0.NotifyChanged();
                SetInvincible(cur.invincible);
                ready = 0;
                lastHp = s0.playerHp;
                scoreAtReady = s0.score;
                heartsAtReady = s0.hearts;
                lastTime = s0.time;
                atReady = State();
                trace.Add("0\tsẵn sàng " + atReady + (cur.invincible ? " (bất tử)" : ""));
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
            hpDropAges.Add(age);
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
        if (InStage && S != null && fallAge < 0 && deaths.Count == 0 && (S.Fsm.Is(eSimonState.Fall) || S.Fsm.Is(eSimonState.FallDeath)))
        {
            fallAge = age;
            fallX = S.X;
            fallY = S.Y;
            trace.Add(age + "\tbắt đầu rơi " + State());
            Shot("roi");
        }
        if (lastDropAge > 0 && age == lastDropAge + 12 && hpDrops.Count == 1)
            Shot("don1");
        if (lastDropAge > 0 && age == lastDropAge + 12 && hpDrops.Count == 6)
            Shot("don6");
        if (pressAge >= 0 && age == pressAge + 90)
            Shot("dongho90");
        if (fireAge >= 0 && age == fireAge + 2)
            Shot("lua");
        if (holyThrowAge >= 0 && age == holyThrowAge + 24)
            Shot("lo24");
        if (bagRestAge >= 0 && age == bagRestAge + 2)
            Shot("tui");
        if (bagOffscreenTicks == 60)
            Shot("tui_ngoai_man");
        if (s.time != lastTime)
        {
            timeChangeAges.Add(age);
            lastTime = s.time;
        }
        if (InStage && deaths.Count == 0)
        {
            ObserveProjectiles();
            ObserveBag();
        }
        if (age % 30 == 0 && InStage && S != null)
            trace.Add(age + "\t" + State() + " " + (S.Fsm.HasState ? S.Fsm.CurState.ToString() : "-"));
        bool end = age >= cur.maxTicks
                   || (cur.endOnRespawn && respawns.Count > 0 && age - ParseAge(respawns[0]) > 30)
                   || (cur.name == "gameover_st9" && continued && afterContinue.Length > 0)
                   || (cur.done != null && cur.done());
        if (end)
        {
            string err = cur.check();
            if (err == null) passCount++; else failCount++;
            rep.Append(err == null ? "PASS " : "FAIL ").Append(cur.name).Append(" (").Append(age).Append(" tick").Append(cur.invincible ? ", bất tử" : "").Append(")")
               .Append(err != null ? " — " + err : "").Append('\n')
               .Append("   sẵn sàng: ").Append(atReady).Append('\n');
            if (deaths.Count > 0 || respawns.Count > 0)
                rep.Append("   chết: ").Append(string.Join(" | ", deaths.ToArray())).Append('\n')
                   .Append("   hồi sinh: ").Append(string.Join(" ", respawns.ToArray())).Append(" ⇒ ").Append(afterRespawn).Append('\n');
            if (fallAge >= 0)
                rep.Append("   bắt đầu rơi: tuổi ").Append(fallAge).Append(" x ").Append(fallX).Append(" y ").Append(fallY).Append('\n');
            if (cur.name == "gameover_st9")
                rep.Append("   GAME OVER ").Append(sawGameOver ? "có" : "KHÔNG").Append(", CONTINUE ⇒ ").Append(afterContinue).Append('\n');
            if (hits.Count > 0 || hpDrops.Count > 0)
            {
                rep.Append("   đòn vào Simon: ").Append(string.Join(" | ", hits.ToArray())).Append('\n');
                rep.Append("   HP mất mỗi lần: ").Append(string.Join(",", hpDrops.ConvertAll(x => x.ToString()).ToArray()))
                   .Append(" (tuổi ").Append(string.Join(",", hpDropAges.ConvertAll(x => x.ToString()).ToArray())).Append(")")
                   .Append(", lùi mỗi đòn (px, tới khi hết Hurt): ").Append(string.Join(",", kbs.ConvertAll(x => x.ToString()).ToArray())).Append('\n');
            }
            if (pressAge >= 0)
                rep.Append("   đồng hồ: bấm tuổi ").Append(pressAge).Append(", tim ").Append(heartsBeforePress).Append(" → ").Append(heartsAfterPress)
                   .Append(", TIME đổi ở tuổi ").Append(string.Join(",", timeChangeAges.ConvertAll(x => x.ToString()).ToArray())).Append('\n');
            if (holyThrowAge >= 0)
            {
                rep.Append("   lọ: ném tuổi ").Append(holyThrowAge).Append(" X0 ").Append(holyX0).Append(" Y0 ").Append(holyY0).Append(", n cuối ").Append(holyLastN)
                   .Append(", lửa tuổi ").Append(fireAge).Append(" (").Append(fireX).Append(',').Append(fireY).Append(")\n   vết lọ (n x y): ")
                   .Append(string.Join(" | ", holyTrack.ToArray())).Append('\n');
            }
            foreach (var bn in bagNotes)
                rep.Append("   túi: ").Append(bn).Append('\n');
            Next();
        }
    }

    private static void ObserveProjectiles()
    {
        var w = W;
        for (int g = 0; g < Entity.GROUP_COUNT; g++)
        {
            var list = w.GroupList(g);
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null || !e.Alive)
                    continue;
                if (e is HolyWater)
                {
                    if (holyBottle == null)
                    {
                        holyBottle = e;
                        holyThrowAge = age;
                        holyX0 = S.X;
                        holyY0 = S.Y;
                    }
                    if (e == holyBottle)
                    {
                        int n = age - holyThrowAge;
                        holyTrack.Add(n + " " + e.X + " " + e.Y);
                        holyLastN = n;
                    }
                }
                else if (e is HolyFire && fireAge < 0 && holyBottle != null)
                {
                    fireAge = age;
                    fireX = e.X;
                    fireY = e.Y;
                }
                else if (e is MedusaHead)
                {
                    List<string> tr;
                    if (!mhTracks.TryGetValue(e, out tr))
                        mhTracks[e] = tr = new List<string>();
                    tr.Add(age + " " + e.fx + " " + e.fy);
                }
            }
        }
        if (pressAge >= 0 && heartsAfterPress < 0 && age > pressAge)
            heartsAfterPress = GameSession.Current.hearts;
    }

    private static void ObserveBag()
    {
        if (S != null && crouchAge < 0 && S.Fsm.Is(eSimonState.Crouch))
            crouchAge = age;
        if (bag == null)
        {
            var list = W.GroupList(eEntityGroup.Item);
            for (int i = 0; i < list.Count; i++)
            {
                var it = list[i] as ItemPickup;
                if (it != null && it.Alive && it.id == eItemId.FlashingMoneyBag && !it.finalCrystal)
                {
                    bag = it;
                    bagSpawnAge = age;
                    bagX = it.X;
                    bagFirstY = bagLastY = it.Y;
                    break;
                }
            }
            return;
        }
        if (bagGoneAge >= 0)
            return;
        if (!bag.Alive || bag.picked)
        {
            bagGoneAge = age;
            bagPicked = bag.picked || GameSession.Current.score - scoreAtReady >= 1000;
            return;
        }
        if (bagRestAge < 0 && bag.motion == ItemPickup.eMotion.Rest)
        {
            bagRestAge = age;
            bagRestY = bag.Y;
        }
        bagLastY = bag.Y;
        if (bag.X < Cx + 8 || bag.X >= Cx + 248)
            bagOffscreenTicks++;
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
        SetInvincible(prevInvincible);
        Running = false;
        Finished = true;
        Error = err ?? "";
        rep.Insert(0, "Map3AcceptProbe.Run: " + passCount + "/" + (passCount + failCount) + " ĐẠT, lỗi console " + consoleErrors.Count + (Error.Length > 0 ? ", LỖI " + Error : "") + "\n");
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

    // ================================================================ sổ điểm video Map_3

    /// <summary>
    /// File sổ (Tools/cvm3/accept12_ledger.py sinh từ ref/data/map3_events.txt + drops3.tsv + map3_hud.csv): mỗi dòng
    /// "frame \t op \t arg \t score \t hearts \t lives \t sub \t mult" (giá trị HUD video SAU sự kiện; −1 / "-" = không so).
    /// op: start (score,hearts,lives,sub,mult; whip = arg), kill eEnemyId, boss eBossId (MỖI con Mummy 1 dòng), item eItemId, use eSubWeapon,
    /// shot (phá đạn enemy — ConfigScore.EnemyProjectilePoints), stage n (EnterStage), time n (TIME HUD video lúc nhặt cầu), tally (StageClearTally
    /// Block 3 — Map3FlowTiming), nextblock (StartNextBlock), known n (luật CHƯA DỰNG: cộng n để sổ đi tiếp — dòng tính "LỆCH-BIẾT", không phải khớp).
    /// </summary>
    public static string Ledger(string ledgerFile, string outPath)
    {
        if (!EditorApplication.isPlaying || W == null || GameSession.Current == null)
            return "cần Play Main.unity, đang ở màn chơi";
        var cfg = W.Config;
        var s = GameSession.Current;
        var sb = new StringBuilder();
        int ok = 0, bad = 0, known = 0, knownSum = 0;
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
                bool isKnown = false;
                switch (op)
                {
                    case "start":
                        GameRules.NewGame();
                        s.stage = 7;
                        s.block = GameRules.BlockOfStage(7);
                        s.time = GameRules.TimeLimitOfStage(7);
                        s.score = eScore;
                        s.hearts = eHearts;
                        s.lives = eLives;
                        s.whipLevel = int.Parse(arg);
                        s.SetSubWeapon((eSubWeapon)Enum.Parse(typeof(eSubWeapon), eSub));
                        s.shotMultiplier = eMult;
                        s.nextOneUp = GameRules.NextOneUpAbove(s.score);
                        note = "TIME " + s.time + " (ConfigBlock Block " + s.block + ")";
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
                            int pts = row != null && row.score.has ? row.score.value : 0;   // = BossBase.ResetHealth ⇒ Score (MummyMan: mỗi con)
                            GameRules.AddScore(pts);
                            note = "+" + pts + " (ConfigBoss." + id + ")";
                        }
                        break;
                    case "shot":
                        {
                            int pts = cfg.score != null ? cfg.score.GetInt(eScoreParam.EnemyProjectilePoints) : 0;   // = MummyBandage / cầu lửa Bone Tower vỡ
                            GameRules.AddScore(pts);
                            note = "+" + pts + " (ConfigScore.EnemyProjectilePoints — phá đạn enemy)";
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
                            t.Start(cfg, Map3FlowTiming.TALLY_WAIT_TICKS, Map3FlowTiming.TALLY_GAP_TICKS);
                            int n = 0;
                            while (!t.Tick() && n < 20000)
                                n++;
                            note = "tally Block 3: TIME " + t.TimeCounted + " đv (+" + (t.TimeCounted * 10) + "), tim " + t.HeartsCounted + " (+" + (t.HeartsCounted * 100) + "), cộng "
                                   + t.PointsAdded + "; pha: TIME bắt đầu tick " + t.TimeStartTick + ", hết " + t.TimeEndTick + ", tim " + t.HeartStartTick + "→" + t.HeartEndTick + ", xong " + t.DoneTick;
                        }
                        break;
                    case "nextblock":
                        GameRules.StartNextBlock();
                        note = "StartNextBlock ⇒ STAGE " + s.stage + " tim " + s.hearts;
                        break;
                    case "known":
                        {
                            int pts = int.Parse(arg);
                            s.score += pts;   // KHÔNG qua luật game — luật chưa dựng; chỉ để các dòng sau so tiếp
                            isKnown = true;
                            note = "+" + pts + " LUẬT CHƯA DỰNG: " + (c.Length > 8 ? c[8] : "");
                        }
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
                string tag;
                if (isKnown)
                {
                    known++;
                    knownSum += int.Parse(arg);
                    tag = errs.Count == 0 ? "BIẾT" : "LỆCH";
                    if (errs.Count > 0) bad++;
                }
                else if (errs.Count == 0)
                {
                    ok++;
                    tag = "OK  ";
                }
                else
                {
                    bad++;
                    tag = "LỆCH";
                }
                sb.Append(tag).Append('\t').Append(frame).Append('\t').Append(op).Append(' ').Append(arg).Append('\t')
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
        string head = "Ledger Map_3: " + ok + "/" + (ok + bad) + " sự kiện khớp HUD video qua code game, " + known + " dòng luật CHƯA DỰNG (+" + knownSum
                      + " ⇒ Unity thuần thiếu " + knownSum + "), lệch " + bad + ", 1UP " + oneUps + " lần, SCORE cuối " + s.score + "\n";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, head + sb, new UTF8Encoding(false));
        return head;
    }
}
#endif
