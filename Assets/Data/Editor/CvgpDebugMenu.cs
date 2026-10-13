using UnityEditor;
using UnityEngine;

/// <summary>
/// Menu CVGP/Debug/* (overlay hitbox, overlay lưới va chạm, log đổi state) và CVGP/Cheat/* (chỉ Editor, CVGP 07).
/// Cờ lưu EditorPrefs (CvgpDebug.PREF_*), có hiệu lực ngay trong Play; build thường không có menu ⇒ mọi cờ tắt.
/// </summary>
public static class CvgpDebugMenu
{
    const string M_HITBOX = "CVGP/Debug/Hitbox Overlay";
    const string M_TILES = "CVGP/Debug/Tile Collision Overlay";
    const string M_LOGSTATE = "CVGP/Debug/Log State Changes";
    const string M_INVINCIBLE = "CVGP/Cheat/Invincible";
    const string M_FULLHP = "CVGP/Cheat/Full HP";
    const string M_HP1 = "CVGP/Cheat/HP 1";
    const string M_KILL = "CVGP/Cheat/Kill All Enemies";
    const string M_WARP = "CVGP/Cheat/Warp Next Section";
    const string M_FREEZE = "CVGP/Cheat/Freeze Enemies 300 ticks";

    static void Toggle(string pref)
    {
        EditorPrefs.SetBool(pref, !EditorPrefs.GetBool(pref, false));
        CvgpDebug.LoadPrefs();
    }

    [MenuItem(M_HITBOX)] static void Hitbox() { Toggle(CvgpDebug.PREF_HITBOX); }
    [MenuItem(M_HITBOX, true)] static bool HitboxV() { Menu.SetChecked(M_HITBOX, EditorPrefs.GetBool(CvgpDebug.PREF_HITBOX, false)); return true; }

    [MenuItem(M_TILES)] static void Tiles() { Toggle(CvgpDebug.PREF_TILES); }
    [MenuItem(M_TILES, true)] static bool TilesV() { Menu.SetChecked(M_TILES, EditorPrefs.GetBool(CvgpDebug.PREF_TILES, false)); return true; }

    [MenuItem(M_LOGSTATE)] static void LogState() { Toggle(CvgpDebug.PREF_LOG_STATE); }
    [MenuItem(M_LOGSTATE, true)] static bool LogStateV() { Menu.SetChecked(M_LOGSTATE, EditorPrefs.GetBool(CvgpDebug.PREF_LOG_STATE, false)); return true; }

    [MenuItem(M_INVINCIBLE)] static void Invincible() { Toggle(CvgpDebug.PREF_CHEAT_INVINCIBLE); }
    [MenuItem(M_INVINCIBLE, true)] static bool InvincibleV() { Menu.SetChecked(M_INVINCIBLE, EditorPrefs.GetBool(CvgpDebug.PREF_CHEAT_INVINCIBLE, false)); return true; }

    static bool InStage()
    {
        return Application.isPlaying && StageWorld.Current != null;
    }

    [MenuItem(M_FULLHP)]
    static void FullHp()
    {
        GameSession.Current.playerHp = GameSession.MAX_HP;
        GameSession.Current.NotifyChanged();
    }

    [MenuItem(M_FULLHP, true)] static bool FullHpV() { return Application.isPlaying; }

    [MenuItem(M_HP1)]
    static void Hp1()
    {
        GameSession.Current.playerHp = 1;
        GameSession.Current.NotifyChanged();
    }

    [MenuItem(M_HP1, true)] static bool Hp1V() { return Application.isPlaying; }

    [MenuItem(M_KILL)]
    static void KillAll()
    {
        int n = StageWorld.Current.KillGroup(eEntityGroup.Enemy) + StageWorld.Current.KillGroup(eEntityGroup.EnemyShot);
        CvgpDebug.Log("cheat: hạ " + n + " enemy/đạn");
    }

    [MenuItem(M_KILL, true)] static bool KillAllV() { return InStage(); }

    [MenuItem(M_FREEZE)]
    static void Freeze()
    {
        StageWorld.Current.FreezeEnemies(300);
    }

    [MenuItem(M_FREEZE, true)] static bool FreezeV() { return InStage(); }

    /// <summary>Sang section kế tiếp trong danh sách con của StageWorld; Player đặt ở simonStart (hoặc x 48 trên mặt sàn đầu tiên).</summary>
    [MenuItem(M_WARP)]
    static void Warp()
    {
        var w = StageWorld.Current;
        var list = w.Sections;
        int i = 0;
        for (int k = 0; k < list.Count; k++)
            if (list[k] == w.Section)
                i = k;
        var next = list[(i + 1) % list.Count];
        var p = WarpPoint(next.data);
        w.Warp(next, p.x, p.y);
        CvgpDebug.Log("cheat: warp " + next.name + " (" + p.x + "," + p.y + ")");
    }

    [MenuItem(M_WARP, true)] static bool WarpV() { return InStage() && StageWorld.Current.Sections.Count > 1; }

    public static Vector2Int WarpPoint(LevelSectionData d)
    {
        if (d.hasSimonStart)
            return d.simonStart;
        // không có điểm bắt đầu (Crypt B): mặt trên ô đặc đầu tiên ở cột x 48
        var t = new TileCollision(d);
        for (int y = 1; y < d.height; y++)
            if (t.RowLandable(y, 40, 55))
                return new Vector2Int(48, y);
        return new Vector2Int(48, 0);
    }
}
