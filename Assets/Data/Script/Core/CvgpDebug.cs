using UnityEngine;

/// <summary>
/// Cờ debug + cheat của lõi gameplay (CVGP 07). Bật bằng menu CVGP/Debug/* và CVGP/Cheat/* (EditorPrefs, chỉ Editor —
/// build thường mọi cờ = false). Log qua Log() ⇒ "[CVGP] tick N: …" (không Debug.Log rác trong code gameplay).
/// </summary>
public static class CvgpDebug
{
    public const string PREF_HITBOX = "CVGP.HitboxOverlay";
    public const string PREF_TILES = "CVGP.TileOverlay";
    public const string PREF_LOG_STATE = "CVGP.LogState";
    public const string PREF_CHEAT_INVINCIBLE = "CVGP.CheatInvincible";
    public const string PREF_LOG_RULES = "CVGP.LogRules";

    /// <summary>Vẽ hộp địa hình/hitbox/hurtbox + toạ độ + state (CvgpDebugOverlay).</summary>
    public static bool ShowHitbox;
    /// <summary>Tô lưới va chạm 16×16 + đường cầu thang của section đang chơi.</summary>
    public static bool ShowTiles;
    /// <summary>Log mọi lần StateMachine đổi state.</summary>
    public static bool LogState;
    /// <summary>Cheat: hurtbox nhóm Player không nhận đòn.</summary>
    public static bool CheatInvincible;
    /// <summary>CVGP 12: log luật chơi (TIME, mất mạng, hồi sinh, 1UP, tally, pause, Game Over).</summary>
    public static bool LogRules;

    public static void Log(string message)
    {
        Debug.Log("[CVGP] tick " + NesClock.Frame + ": " + message);
    }

    /// <summary>Đọc lại cờ từ EditorPrefs (StageWorld gọi khi bật; menu gọi khi đổi).</summary>
    public static void LoadPrefs()
    {
#if UNITY_EDITOR
        ShowHitbox = UnityEditor.EditorPrefs.GetBool(PREF_HITBOX, false);
        ShowTiles = UnityEditor.EditorPrefs.GetBool(PREF_TILES, false);
        LogState = UnityEditor.EditorPrefs.GetBool(PREF_LOG_STATE, false);
        CheatInvincible = UnityEditor.EditorPrefs.GetBool(PREF_CHEAT_INVINCIBLE, false);
        LogRules = UnityEditor.EditorPrefs.GetBool(PREF_LOG_RULES, false);
#endif
    }
}
