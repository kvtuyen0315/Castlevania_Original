#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CVM3 11 — kiểm hồi sinh / GAME OVER → CONTINUE ở Block 3 trên LUỒNG THẬT (Main.unity Play, NesClock chạy thời gian thực; chỉ đo — không đổi
/// gameplay). Mỗi lượt: ván mới ⇒ stage N (như cheat CVM3/Start Stage N) ⇒ chờ 40 tick ⇒ Simon.KillByTimeout ⇒ (còn mạng) hồi sinh đầu stage /
/// (hết mạng) màn GAME OVER ⇒ bấm Start (CONTINUE mặc định) ⇒ đầu stage. Ghi section, toạ độ, hướng, TIME, STAGE, P, cx lúc vào lại + ảnh.
///   Map3RespawnProbe.Run("7:2,8:2,9:2,9:1", shotDir)   (stage:mạng — 1 mạng ⇒ GAME OVER)   ⇒ Running / Done / Report()
/// </summary>
public static class Map3RespawnProbe
{
    class Job { public int stage, lives; }

    public static bool Running { get; private set; }
    public static bool Done { get; private set; }
    public static readonly List<string> Lines = new List<string>();

    static readonly List<Job> jobs = new List<Job>();
    static Job cur;
    static int phase, wait, respawns0;
    static string shotDir;
    static StageRules rules;

    static StageWorld W => StageWorld.Current;
    static SimonController S => W != null ? W.Player as SimonController : null;
    static bool InStage => GameManager.I != null && GameManager.I.State == eGameScreen.Stage1 && !GameManager.I.IsTransitioning && W != null;

    public static string Run(string spec, string dir)
    {
        if (!EditorApplication.isPlaying || GameManager.I == null)
            return "cần Play Main.unity";
        if (Running)
            return "đang chạy";
        jobs.Clear();
        Lines.Clear();
        foreach (var p in spec.Split(','))
        {
            var kv = p.Split(':');
            jobs.Add(new Job { stage = int.Parse(kv[0]), lives = int.Parse(kv[1]) });
        }
        shotDir = dir;
        if (dir != null)
            Directory.CreateDirectory(dir);
        CvgpDebug.CheatInvincible = false;
        Application.runInBackground = true;
        Running = true;
        Done = false;
        cur = null;
        NesClock.OnTick += Tick;
        return "chạy " + jobs.Count + " lượt";
    }

    static void Next()
    {
        if (jobs.Count == 0)
        {
            NesClock.OnTick -= Tick;
            Running = false;
            Done = true;
            return;
        }
        cur = jobs[0];
        jobs.RemoveAt(0);
        GameRules.NewGame();
        var s = GameSession.Current;
        s.stage = cur.stage;
        s.lives = cur.lives;
        s.NotifyChanged();
        GameManager.I.GoTo(eGameScreen.Stage1, 0);
        phase = 0;
        wait = 0;
        rules = null;
        Record("vào");
    }

    static void Tick(int frame)
    {
        if (!Running)
            return;
        if (cur == null)
        {
            Next();
            return;
        }
        var gm = GameManager.I;
        switch (phase)
        {
            case 0:   // chờ 40 tick trong màn chơi rồi giết
                if (!InStage)
                    return;
                if (++wait < 40)
                    return;
                rules = StageRules.Current;
                respawns0 = rules != null ? rules.RespawnCount : 0;
                Record("trước chết");
                S.KillByTimeout();
                phase = 1;
                wait = 0;
                return;
            case 1:   // chờ hồi sinh (RespawnCount tăng) hoặc màn GAME OVER
                if (gm.State == eGameScreen.GameOver && !gm.IsTransitioning)
                {
                    Record("GAME OVER");
                    Shot("gameover_st" + cur.stage);
                    phase = 2;
                    wait = 0;
                    return;
                }
                if (rules != null && rules.RespawnCount > respawns0 && InStage)
                {
                    if (++wait < 3)
                        return;
                    Record("hồi sinh");
                    Shot("respawn_st" + cur.stage);
                    cur = null;
                }
                return;
            case 2:   // bấm Start ⇒ CONTINUE
                if (++wait == 30)
                    NesPad.I.InjectPress(eNesButton.Start);
                if (wait > 30 && InStage)
                {
                    if (wait < 36)
                        return;
                    Record("CONTINUE");
                    Shot("continue_st" + cur.stage);
                    cur = null;
                }
                return;
        }
    }

    static void Record(string what)
    {
        var s = GameSession.Current;
        var simon = S;
        Lines.Add("St" + (cur != null ? cur.stage : 0) + "\t" + what + "\t" + (InStage ? W.Section.name + " cx " + W.CameraCtl.X + " Simon (" + simon.X + "," + simon.Y
                  + ") hướng " + simon.Facing + " " + simon.Fsm.CurState : GameManager.I.State.ToString())
                  + "\tSTAGE " + s.stage.ToString("00") + " TIME " + s.time.ToString("0000") + " P-" + s.lives.ToString("00") + " HP " + s.playerHp + " ♥" + s.hearts
                  + " roi " + s.whipLevel + " " + s.subWeapon + " SCORE " + s.score);
    }

    static void Shot(string name)
    {
        if (shotDir == null)
            return;
        string p = Path.Combine(shotDir, name + ".png");
        if (File.Exists(p))
            File.Delete(p);
        ScreenCapture.CaptureScreenshot(p);
    }

    public static string Report()
    {
        var sb = new StringBuilder();
        sb.Append(Running ? "ĐANG CHẠY\n" : "xong\n");
        foreach (var l in Lines)
            sb.Append(l).Append('\n');
        return sb.ToString();
    }
}
#endif
