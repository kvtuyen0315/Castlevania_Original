#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Harness CVM3 04 — dựng lại trong Unity các mẫu video đã đo (CHỈ Editor, scene Arena, Player = Simon) để so từng tick
/// (Tools/cvm3/kb4_compare.py): knockback (không tường trái/phải, tường trái, mép màn khoá phải, ngồi sát mép), lọ nước thánh
/// (thả giữa không trung cao 64 + 19 px như Map_3 trận Mummy, ném trên sàn phẳng quay trái như Boss_Map_1), đi khỏi mép đúng các mép
/// video (Map_1 EH/CB, Map_2 Hall A/B, Medusa). Qua MCP khi Play:
///   Kb4Probe.Run(Kb4Probe.CaseNames(), "&lt;thư mục&gt;") ⇒ chạy theo NesClock; xong Kb4Probe.Done ⇒ &lt;thư mục&gt;/&lt;case&gt;.csv + SUMMARY.txt
///   (+ ảnh Game view ở tick đánh dấu nếu Kb4Probe.Shots = true). Cuối bộ trả Arena (48,176), camera bám Simon, gỡ section Block 2.
/// KHÔNG nằm trong RegressionRunner (chỉ đo — kỳ vọng ĐẠT/CHƯA ở SimonProbe/AttackProbe/Map2MechProbe).
/// </summary>
public static class Kb4Probe
{
    const string TARGET_PREFAB = "Assets/Data/Prefabs/Test/ArenaTarget.prefab";

    public static string CaseNames() =>
        "kb_free_l,kb_free_r,kb_wall_l,kb_wall_r,kb_duck_wall_r,holy_drop64_r,holy_flat_l,"
        + "ledge_eh_l,ledge_cb_r,ledge_tha_r,ledge_tha_l,ledge_thb_l,ledge_med_r,ledge_med_l";

    public static bool Done { get; private set; } = true;
    public static bool Shots;
    public static readonly List<string> Summary = new List<string>();

    private static StageWorld world;
    private static Queue<string> queue;
    private static string outDir, cur;
    private static int rel, total, y0;
    private static StringBuilder trace;
    private static Action<int> onTick;
    private static readonly List<Entity> extras = new List<Entity>();
    private static readonly HashSet<int> shotTicks = new HashSet<int>();
    private static SimonController S => world != null ? world.Player as SimonController : null;

    public static string Run(string names, string dir)
    {
        world = StageWorld.Current;
        if (world == null || S == null)
            return "cần Play scene Arena (Player = Simon)";
        if (!Done)
            return "đang chạy";
        outDir = dir;
        Directory.CreateDirectory(dir);
        Summary.Clear();
        queue = new Queue<string>(names.Split(','));
        Done = false;
        CvgpDebug.CheatInvincible = false;
        Cvm2TerrainProbe.Setup();
        Next();
        return "bắt đầu " + names;
    }

    private static void Next()
    {
        world.TickEnded -= OnTickEnded;
        ClearExtras();
        if (queue.Count == 0)
        {
            BackToArena();
            File.WriteAllText(Path.Combine(outDir, "SUMMARY.txt"), string.Join("\n", Summary) + "\n");
            Done = true;
            return;
        }
        cur = queue.Dequeue();
        string rle = Setup(cur);
        if (rle == null)
        {
            Summary.Add(cur + ": KHÔNG DỰNG ĐƯỢC");
            Next();
            return;
        }
        var ticks = NesReplay.Decode(rle);
        total = ticks.Count;
        rel = 0;
        y0 = S.Y;
        trace = new StringBuilder("rel,x,y,state,frame,inv,visible,proj\n");
        NesPad.I.PlayReplay(ticks);
        world.TickEnded += OnTickEnded;
    }

    private static LevelSection Section(string name)
    {
        foreach (var l in world.Sections)
            if (l.name == name)
                return l;
        return Cvm2TerrainProbe.Find(name);
    }

    private static void Warp(string sec, int x, int y, int facing)
    {
        var s = Section(sec);
        world.Warp(s, x, y);
        world.ResetRng();
        S.Teleport(x, y, facing);
        var ses = GameSession.Current;
        ses.playerHp = GameSession.MAX_HP;
        ses.NotifyChanged();
        if (world.CameraCtl != null)
        {
            world.CameraCtl.Target = world.Player;
            world.CameraCtl.SnapToTarget();
        }
    }

    private static ArenaTarget Target(int x, int y)
    {
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<ArenaTarget>(TARGET_PREFAB);
        var t = (ArenaTarget)world.Spawn(prefab, x, y, -1);
        t.touchDamage = true;
        t.Touch.active = true;
        extras.Add(t);
        return t;
    }

    /// <summary>Dựng kịch bản; trả input RLE (null = không dựng được).</summary>
    private static string Setup(string c)
    {
        onTick = null;
        shotTicks.Clear();
        switch (c)
        {
            // ---- knockback: bia chạm thân từ phía trước; lùi ngược hướng mặt
            case "kb_free_l":   // = video Boss_Map_2 #3 (quay phải, lùi trái, không tường)
            case "kb_free_r":   // = video Boss_Map_3 #1 (quay trái, lùi phải)
                {
                    int f = c == "kb_free_l" ? 1 : -1;
                    Warp("Arena", 200, 176, f);
                    var t = Target(200 + f * 10, 176);   // chồng hộp (chạm 12 + nhận 16 ⇒ |dx| < 14)
                    onTick = r => { if (r == 40 && t.Alive) t.Kill(eKillReason.Cleared); };
                    foreach (var k in new[] { 1, 6, 12, 20, 26, 30, 33 }) shotTicks.Add(k);
                    return "-*80";
                }
            case "kb_wall_l":   // = video Boss_Map_2 #1 (quay phải, lùi trái 10 px tới tường trái)
                {
                    Warp("Arena", 118, 176, 1);
                    world.Tiles.WallMinX = 100;
                    if (world.CameraCtl != null) world.CameraCtl.Lock(100);
                    var t = Target(128, 176);
                    onTick = r => { if (r == 40 && t.Alive) t.Kill(eKillReason.Cleared); };
                    return "-*80";
                }
            case "kb_wall_r":     // = video Boss_Map_1 (quay trái, lùi phải 12 px tới mép màn khoá cx + 239)
            case "kb_duck_wall_r": // = video Boss_Map_3 #2 (ngồi quay trái sát mép phải, lùi 10–11 px)
                {
                    bool duck = c == "kb_duck_wall_r";
                    int x = duck ? 329 : 327;
                    Warp("Arena", x, 176, -1);
                    world.Tiles.WallMinX = 100;
                    int maxX = world.Config.simon.GetInt(eSimonParam.LockedScreenMaxX, 239);
                    world.Tiles.PlayerWallMaxX = 100 + maxX + 8;   // như BossArena khi khoá cx 100
                    if (world.CameraCtl != null) world.CameraCtl.Lock(100);
                    ArenaTarget t = null;
                    onTick = r =>
                    {
                        if (r == (duck ? 6 : 0)) t = Target(x - 10, 176);
                        if (r == 40 && t != null && t.Alive) t.Kill(eKillReason.Cleared);
                    };
                    return duck ? "D*80" : "-*80";
                }
            // ---- nước thánh
            case "holy_drop64_r":   // = video Map_3 trận Mummy: tâm lọ lúc sinh cao hơn sàn 64 + 19 px, quay phải
                {
                    Warp("Arena", 48, 176, 1);
                    foreach (var k in new[] { 2, 10, 18, 26, 34, 42, 50, 51, 53, 55 }) shotTicks.Add(k);   // n = rel − 2
                    onTick = r =>
                    {
                        if (r != 2) return;
                        var prefab = S.PrefabOf(eSubWeapon.HolyWater);
                        // video: Simon chân Y 144, lọ sinh (22, 125), sàn 208 ⇒ tâm lọ cao hơn sàn 83 px
                        var p = world.Spawn(prefab, 100, 176 - 83, 1) as SubWeaponProjectile;
                        if (p != null) p.Thrower = S;
                        extras.Add(p);
                    };
                    return "-*150";
                }
            case "holy_flat_l":     // = video Boss_Map_1 (đứng sàn, quay trái)
                {
                    Warp("Arena", 300, 176, -1);
                    var ses = GameSession.Current;
                    ses.subWeapon = eSubWeapon.HolyWater;
                    ses.hearts = 20;
                    ses.NotifyChanged();
                    foreach (var k in new[] { 18, 26, 34, 42, 50, 58 }) shotTicks.Add(k);
                    return "-*2 Sub -*140";
                }
            // ---- đi khỏi mép (mép = cạnh ô đặc hàng sàn; video Tools/cvm3/kb4_ledge.py)
            case "ledge_eh_l": Warp("EntranceHall", 940, 112, -1); foreach (var k in new[] { 16, 17, 18, 19, 21, 25 }) shotTicks.Add(k); return "L*30 -*20";   // mép trái 928 — video Map_1 f1882/1883
            case "ledge_cb_r": Warp("CryptB", 112, 64, 1); return "R*30 -*20";          // mép phải 128 — video Map_1 f3757/3758
            case "ledge_tha_r": Warp("TowerHallA", 304, 48, 1); foreach (var k in new[] { 19, 20, 21, 22, 24 }) shotTicks.Add(k); return "R*30 -*20";     // mép phải 320 — video Map_2 f2409/2410
            case "ledge_tha_l": Warp("TowerHallA", 304, 48, -1); return "L*30 -*20";    // mép trái 288 — video Map_2 f2507/2508
            case "ledge_thb_l": Warp("TowerHallB", 624, 48, -1); return "L*30 -*20";    // mép trái 608 — video Map_2 f3824/3825
            case "ledge_med_r": Warp("Medusa", 688, 128, 1); return "R*30 -*20";        // mép phải 704 — video Map_2 f6047/6048
            case "ledge_med_l": Warp("Medusa", 232, 128, -1); return "L*30 -*20";       // mép trái 224 — video Map_2 f6688/6690
        }
        return null;
    }

    private static void OnTickEnded(StageWorld w)
    {
        var s = S;
        if (onTick != null)
            onTick(rel);
        var proj = new StringBuilder();
        foreach (var e in w.GroupList(eEntityGroup.PlayerWeapon))
            if (e.Alive)
                proj.Append(proj.Length > 0 ? ";" : "").Append(e.GetType().Name).Append(' ').Append(e.X).Append(' ').Append(e.Y);
        string frame = s.Anim != null && s.Anim.Current != null ? s.Anim.Current.name + ":" + s.Anim.FrameIndex : "";
        trace.Append(rel).Append(',').Append(s.X).Append(',').Append(s.Y).Append(',').Append(s.Fsm.CurState).Append(',').Append(frame)
             .Append(',').Append(s.InvincibleTicks).Append(',').Append(s.Anim != null && s.Anim.Visible ? 1 : 0).Append(',').Append(proj).Append('\n');
        if (Shots && shotTicks.Contains(rel))
            ScreenCapture.CaptureScreenshot(Path.Combine(outDir, cur + "_" + rel.ToString("000") + ".png"));
        if (++rel >= total)
        {
            File.WriteAllText(Path.Combine(outDir, cur + ".csv"), trace.ToString());
            Summary.Add(cur + ": " + total + " tick");
            Next();
        }
    }

    private static void ClearExtras()
    {
        for (int i = 0; i < extras.Count; i++)
            if (extras[i] != null && extras[i].Alive)
                extras[i].Kill(eKillReason.Cleared);
        extras.Clear();
        if (world != null)
            world.ClearNonPlayer();
    }

    /// <summary>Trả Arena (48,176): gỡ section Block 2, camera bám Simon, máu đầy, phiên mới.</summary>
    public static void BackToArena()
    {
        world = StageWorld.Current;
        if (world == null || S == null)
            return;
        world.TickEnded -= OnTickEnded;
        ClearExtras();
        Cvm2TerrainProbe.Cleanup();
        Warp("Arena", 48, 176, 1);
        GameSession.Current.ResetForNewGame();
        GameSession.Current.NotifyChanged();
    }
}
#endif
