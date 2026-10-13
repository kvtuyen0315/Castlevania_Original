using UnityEditor;
using UnityEngine;

/// <summary>
/// CVGP 14 — cheat boss (chỉ Editor, trong Play): CVGP/Cheat/Warp Boss Room, CVGP/Cheat/Boss HP 1.
/// Warp: tới section "PhantomBat" của StageWorld đang chạy, Simon ở sàn (656, 176) (camera khoá cx 512 ngay tick sau); section chưa có
/// BossArena (màn chơi thật — routine 15 đặt) thì thêm 1 cái tạm (không lưu scene) với prefab Assets/Data/Prefabs/Boss/PhantomBat.
/// </summary>
public static class CvgpBossCheatMenu
{
    const string M_WARP = "CVGP/Cheat/Warp Boss Room";
    const string M_HP1 = "CVGP/Cheat/Boss HP 1";
    public const string ROOM_SECTION = "PhantomBat";
    public static readonly Vector2Int ROOM_ENTRY = new Vector2Int(656, 176);

    [MenuItem(M_WARP)]
    static void Warp()
    {
        Debug.Log("[CVGP] " + WarpBossRoom(ROOM_ENTRY.x, ROOM_ENTRY.y));
    }

    /// <summary>Warp Simon vào phòng boss (dùng chung với BossProbe).</summary>
    public static string WarpBossRoom(int x, int y)
    {
        var w = StageWorld.Current;
        if (w == null)
            return "không có StageWorld";
        LevelSection sec = null;
        foreach (var s in w.Sections)
            if (s.data != null && s.data.name == ROOM_SECTION)
                sec = s;
        if (sec == null)
            return "StageWorld không có section " + ROOM_SECTION;
        if (sec.GetComponentInChildren<BossArena>(true) == null)
        {
            var go = new GameObject("BossArena (cheat)");
            go.transform.SetParent(sec.transform, false);
            var a = go.AddComponent<BossArena>();
            a.prefab = CvgpBossBuilder.Load<PhantomBat>("PhantomBat");
        }
        w.Warp(sec, x, y);
        var simon = w.Player as SimonController;
        if (simon != null)
            simon.Teleport(x, y, 1);
        if (w.CameraCtl != null)
            w.CameraCtl.SnapToTarget();
        return "cheat: warp phòng boss (" + x + "," + y + ")";
    }

    [MenuItem(M_HP1)]
    static void Hp1()
    {
        var w = StageWorld.Current;
        int n = 0;
        foreach (var e in w.GroupList(eEntityGroup.Enemy))
        {
            var b = e as BossBase;
            if (b != null && b.Alive && !b.dying)
            {
                b.DebugSetHp(1);
                n++;
            }
        }
        CvgpDebug.Log("cheat: boss HP 1 (" + n + " boss)");
    }

    [MenuItem(M_WARP, true)] static bool WarpV() { return Application.isPlaying && StageWorld.Current != null; }
    [MenuItem(M_HP1, true)] static bool Hp1V() { return Application.isPlaying && StageWorld.Current != null; }
}
