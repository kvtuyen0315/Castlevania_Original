#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Harness quét hồi quy giữa cụm (CVGP 10; routine 16/21 chạy lại) — CHỈ Editor, scene Arena (Player = Simon). Gọi qua MCP execute_code khi Play:
///   RegressionProbe.RunLong(targets, perf)  ⇒ 1 replay DÀI liên tục (LONG_RLE: đi, nhảy, roi cấp 3, dao, tường 32 px, hố, nước, thang ↗ lên/xuống, ngồi + roi, nhảy + roi)
///                                              hash từng tick (Simon + mọi thực thể + camera) ⇒ LongReport() (so hash giữa các targetFrameRate)
///   RegressionProbe.StepShots(...)          ⇒ chạy NesClock BẰNG TAY (1 tick mỗi vòng EditorApplication.update) và chụp Game view đúng tick sau đó;
///                                              ghi CSV vết mỗi tick (X, cam, vị trí transform × 16) ⇒ Tools/cvgp/regress_pixel.py chấm.
/// </summary>
public static class RegressionProbe
{
    /// <summary>
    /// Toạ độ Arena (xem SimonProbe): xuất phát (48,176) quay phải; tường thấp x 416–431; hố 480–511; nước 544–575; stair0 ↗ chân (620,176) đỉnh (684,112).
    /// Đoạn: đi 60 · nhảy → · roi · dao · đi tới 380 · nhảy lên tường · rời tường · đi tới 471 · nhảy qua hố · đi tới 536 · nhảy qua nước ·
    /// đi tới 616 · leo thang · dao · roi · xuống thang · ngồi + roi · đi trái · nhảy trái + roi trên không.
    /// </summary>
    public const string LONG_RLE =
        "-*2 R*60 R+A R*39 -*5 B -*25 Sub -*40 R*232 R+A R*50 R*30 R*10 R+A R*39 R*25 R+A R*39 R*40 " +
        "U*135 -*5 Sub -*30 B -*30 D*140 D*2 D+B D*30 -*10 L*60 L+A L*9 L+B L*40 -*10 R*40 B -*30 Sub -*40";

    public static bool Running { get; private set; }
    public static int Target { get; private set; }
    public static int Recorded { get; private set; }
    public static uint Hash { get; private set; }
    public static bool Perf;
    public static int Deaths;
    public static int Hits;
    public static readonly List<int> Xs = new List<int>();
    public static readonly List<int> Ys = new List<int>();
    public static readonly List<int> Cams = new List<int>();
    public static readonly List<int> States = new List<int>();
    public static readonly List<uint> TickHashes = new List<uint>();
    public static readonly List<uint> PrevTickHashes = new List<uint>();
    public static readonly List<string> Seen = new List<string>();

    private static StageWorld world;
    private static readonly List<ArenaTarget> extras = new List<ArenaTarget>();
    private static int startTick;
    private static SimonController hooked;
    private static int lastFrame = -1;
    private static float lastRealtime;
    private static double frameSum, frameMax;
    private static int frameCount;

    public static SimonController Simon => world != null ? world.Player as SimonController : null;

    // ---------------------------------------------------------------- chuẩn bị chung

    private static bool Prepare(int targets, int firstTargetX, bool touch)
    {
        world = StageWorld.Current;
        var s = Simon;
        if (world == null || s == null)
        {
            Debug.LogError("[RegressionProbe] cần scene Arena (Player = SimonController)");
            return false;
        }
        Unhook();
        ClearExtras();
        world.ClearNonPlayer();
        world.ResetRng();
        foreach (var l in world.Sections)
            if (l.name == "Arena")
                world.Warp(l, 48, 176);
        var ses = GameSession.Current;
        ses.playerHp = GameSession.MAX_HP;
        ses.hearts = 40;
        ses.whipLevel = 3;
        ses.shotMultiplier = 1;
        ses.subWeapon = eSubWeapon.None;
        ses.SetSubWeapon(eSubWeapon.Dagger);
        ses.subWeaponHits = 0;
        ses.NotifyChanged();
        s.Teleport(48, 176, 1);
        s.ResetStats();
        if (world.CameraCtl != null)
            world.CameraCtl.SnapToTarget();
        var prefab = AssetDatabase.LoadAssetAtPath<ArenaTarget>("Assets/Data/Prefabs/Test/ArenaTarget.prefab");
        for (int i = 0; i < targets; i++)
        {
            var t = (ArenaTarget)world.Spawn(prefab, firstTargetX + 24 * i, 176, -1);
            t.touchDamage = touch;
            extras.Add(t);
        }
        return true;
    }

    private static void ClearExtras()
    {
        for (int i = 0; i < extras.Count; i++)
            if (extras[i] != null && extras[i].Alive)
                extras[i].Kill(eKillReason.Cleared);
        extras.Clear();
    }

    // ---------------------------------------------------------------- replay dài

    /// <summary>
    /// targets bia ArenaTarget (touchDamage tắt) đặt từ x 200 cách 24 px; perf = bật StageWorld.MeasurePerf + đo thời gian khung. Trả số tick.
    /// </summary>
    public static int RunLong(int targets, bool perf)
    {
        return RunReplay(LONG_RLE, targets, 200, perf);
    }

    public static int RunReplay(string rle, int targets, int firstTargetX, bool perf)
    {
        if (!Prepare(targets, firstTargetX, false))
            return -1;
        var ticks = NesReplay.Decode(rle);
        Perf = perf;
        Target = ticks.Count;
        Recorded = 0;
        Hash = 2166136261u;
        Deaths = 0;
        Hits = 0;
        Xs.Clear(); Ys.Clear(); Cams.Clear(); States.Clear();
        PrevTickHashes.Clear();
        PrevTickHashes.AddRange(TickHashes);
        TickHashes.Clear();
        Seen.Clear();
        lastFrame = -1;
        frameSum = frameMax = 0;
        frameCount = 0;
        if (perf)
        {
            world.ResetPerf();
            StageWorld.MeasurePerf = true;
        }
        startTick = world.TickCount;
        if (perf)
            EditorApplication.update += FrameSample;
        world.TickEnded += OnTickEnded;
        hooked = Simon;
        hooked.Died += OnDied;
        Running = true;
        NesPad.I.PlayReplay(ticks);
        return ticks.Count;
    }

    // đo thời gian mỗi khung Editor (mọi khung, kể cả khung không có tick): ms giữa 2 khung liên tiếp
    private static void FrameSample()
    {
        if (Time.frameCount == lastFrame)
            return;
        float now = Time.realtimeSinceStartup;
        if (lastFrame >= 0)
        {
            double ms = (now - lastRealtime) * 1000.0;
            frameSum += ms;
            frameMax = Math.Max(frameMax, ms);
            frameCount++;
        }
        lastFrame = Time.frameCount;
        lastRealtime = now;
    }

    private static void Unhook()
    {
        EditorApplication.update -= FrameSample;
        if (world != null)
            world.TickEnded -= OnTickEnded;
        if (hooked != null)
        {
            hooked.Died -= OnDied;
            hooked = null;
        }
        Running = false;
    }

    private static void OnDied(SimonController s, eTileCollision kind)
    {
        Deaths++;
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
        int rel = w.TickCount - startTick - 1;
        Mix(rel);
        int cam = w.CameraCtl != null ? w.CameraCtl.X : 0;
        int fi = s.Anim != null ? s.Anim.FrameIndex : 0;
        Xs.Add(s.X);
        Ys.Add(s.Y);
        Cams.Add(cam);
        States.Add((int)s.Fsm.CurState);
        var ses = GameSession.Current;
        Mix(s.fx); Mix(s.fy); Mix(s.Facing); Mix((int)s.Fsm.CurState); Mix(s.InvincibleTicks); Mix(fi);
        Mix(s.drawOffset.x); Mix(s.drawOffset.y); Mix(ses.playerHp); Mix(ses.hearts); Mix(s.StairTPx); Mix(cam);
        for (int g = 1; g < Entity.GROUP_COUNT; g++)
        {
            var list = w.GroupList(g);
            for (int i = 0; i < list.Count; i++)
            {
                Mix(list[i].fx);
                Mix(list[i].fy);
                Mix(list[i].Alive ? 1 : 0);
            }
        }
        TickHashes.Add(Hash);
        string name = s.Fsm.CurState.ToString();
        if (!Seen.Contains(name))
            Seen.Add(name);
        if (++Recorded >= Target)
        {
            Unhook();
            NesPad.I.StopReplay();
            StageWorld.MeasurePerf = false;
            ClearExtras();
        }
    }

    public static bool LongDone => !Running && Recorded > 0 && Recorded >= Target;

    public static string LongReport()
    {
        var sb = new StringBuilder();
        int maxX = 0, minY = 999, maxY = 0, maxCam = 0;
        foreach (var x in Xs) maxX = Math.Max(maxX, x);
        foreach (var y in Ys) { minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
        foreach (var c in Cams) maxCam = Math.Max(maxCam, c);
        sb.Append("LONG ").Append(LongDone ? "xong" : "ĐANG CHẠY").Append(": ").Append(Recorded).Append('/').Append(Target).Append(" tick, hash ")
          .Append(Hash.ToString("x8")).Append(", chết ").Append(Deaths).Append(", x max ").Append(maxX).Append(", Y ").Append(minY).Append("..").Append(maxY)
          .Append(", cam max ").Append(maxCam).Append(", state đã qua: ").Append(string.Join(",", Seen.ToArray()));
        if (Xs.Count > 0)
            sb.Append(", cuối (").Append(Xs[Xs.Count - 1]).Append(',').Append(Ys[Ys.Count - 1]).Append(')');
        return sb.ToString();
    }

    public static string PerfReport()
    {
        var w = StageWorld.Current;
        int n = Math.Max(1, w.MeasuredSteps);
        int ent = 0;
        for (int g = 0; g < Entity.GROUP_COUNT; g++)
            ent += w.GroupList(g).Count;
        return "perf: " + w.MeasuredSteps + " Step, alloc tổng " + w.TotalStepAlloc + " B, Step có alloc " + w.AllocSteps + ", max " + w.MaxStepAlloc + " B; Step TB "
               + (w.TotalStepMs / n).ToString("0.0000") + " ms, max " + w.MaxStepMs.ToString("0.000") + " ms; khung Editor TB "
               + (frameCount > 0 ? (frameSum / frameCount).ToString("0.00") : "?") + " ms (" + frameCount + " khung), max " + frameMax.ToString("0.00")
               + " ms; thực thể (cuối) " + ent + ", pool đã tạo " + w.Pool.CreatedCount;
    }

    public static string FirstDiff()
    {
        int n = Math.Min(TickHashes.Count, PrevTickHashes.Count);
        for (int i = 0; i < n; i++)
            if (TickHashes[i] != PrevTickHashes[i])
                return "tick " + i;
        return TickHashes.Count == PrevTickHashes.Count ? "trùng" : "khác độ dài";
    }

    /// <summary>Vết "rel x,y state cam" (debug).</summary>
    public static string Trace(int from, int to)
    {
        var sb = new StringBuilder();
        for (int i = Math.Max(0, from); i <= to && i < Xs.Count; i++)
            sb.Append(i).Append(' ').Append(Xs[i]).Append(',').Append(Ys[i]).Append(' ').Append((eSimonState)States[i]).Append(" cam").Append(Cams[i]).Append('\n');
        return sb.ToString();
    }

    public static string Csv()
    {
        var sb = new StringBuilder("rel,x,y,state,cam,hash\n");
        for (int i = 0; i < Xs.Count; i++)
            sb.Append(i).Append(',').Append(Xs[i]).Append(',').Append(Ys[i]).Append(',').Append((eSimonState)States[i]).Append(',').Append(Cams[i]).Append(',')
              .Append(TickHashes[i].ToString("x8")).Append('\n');
        return sb.ToString();
    }

    // ---------------------------------------------------------------- chạy tay từng tick + chụp (pixel-perfect)

    public static bool ShotsRunning { get; private set; }
    public static readonly List<string> StepLog = new List<string>();

    private static string shotDir, shotTag, waitFile;
    private static int shotFirst, shotCount, stepRel, stepTotal, waited, settle;
    private static MethodInfo stepMethod;

    /// <summary>
    /// Tắt NesClock, tự gọi Step() 1 lần mỗi vòng EditorApplication.update; chụp từ tick shotFirst (shotCount ảnh liên tiếp) vào dir/tag_NNNN.png,
    /// chờ file xuất hiện rồi mới bước tiếp (ảnh đúng tick). StepLog: "rel,x,y,cam,simonWorldX*16,simonWorldY*16,camWorldX*16".
    /// Simon đặt (startX,176) quay phải; camera cắt về Simon. Chờ settle vòng update đầu để PixelPerfectCamera cập nhật kích thước.
    /// </summary>
    public static int StepShots(string dir, string tag, int startX, string rle, int firstShot, int count)
    {
        if (!Prepare(0, 0, false))
            return -1;
        GameSession.Current.whipLevel = 1;
        Simon.Teleport(startX, 176, 1);
        if (world.CameraCtl != null)
            world.CameraCtl.SnapToTarget();
        var ticks = NesReplay.Decode(rle);
        Directory.CreateDirectory(dir);
        shotDir = dir;
        shotTag = tag;
        shotFirst = firstShot;
        shotCount = count;
        stepRel = 0;
        stepTotal = ticks.Count;
        waitFile = null;
        waited = 0;
        settle = 12;
        StepLog.Clear();
        stepMethod = typeof(NesClock).GetMethod("Step", BindingFlags.Instance | BindingFlags.NonPublic);
        NesClock.I.enabled = false;
        NesPad.I.PlayReplay(ticks);
        ShotsRunning = true;
        EditorApplication.update += StepPump;
        return ticks.Count;
    }

    public static void StopStepShots()
    {
        EditorApplication.update -= StepPump;
        if (NesClock.I != null)
            NesClock.I.enabled = true;
        ShotsRunning = false;
    }

    private static void StepPump()
    {
        if (!ShotsRunning)
            return;
        if (settle > 0)
        {
            settle--;
            return;
        }
        if (waitFile != null)
        {
            if (File.Exists(waitFile) && new FileInfo(waitFile).Length > 0 && ++waited >= 2)
                waitFile = null;
            else
                return;
        }
        if (stepRel >= stepTotal)
        {
            StopStepShots();
            NesPad.I.StopReplay();
            return;
        }
        stepMethod.Invoke(NesClock.I, null);
        var s = Simon;
        var cam = Camera.main;
        StepLog.Add(stepRel + "," + s.X + "," + s.Y + "," + (world.CameraCtl != null ? world.CameraCtl.X : 0) + ","
                    + (s.transform.position.x * 16f).ToString("0.####") + "," + (s.transform.position.y * 16f).ToString("0.####") + ","
                    + (cam != null ? (cam.transform.position.x * 16f).ToString("0.####") : "?"));
        if (stepRel >= shotFirst && stepRel < shotFirst + shotCount)
        {
            waitFile = Path.Combine(shotDir, shotTag + "_" + stepRel.ToString("0000") + ".png");
            if (File.Exists(waitFile))
                File.Delete(waitFile);
            ScreenCapture.CaptureScreenshot(waitFile);
            waited = 0;
        }
        stepRel++;
    }
}
#endif
