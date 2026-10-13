using UnityEditor;
using UnityEngine;

/// <summary>
/// CVGP 09 — cheat tấn công (chỉ Editor, trong Play): CVGP/Cheat/Whip Level 1|2|3, SubWeapon &lt;x&gt;, Shot I|II|III, Hearts 99.
/// Đổi GameSession (+ NotifyChanged cho HUD); vũ khí khác ⇒ mất multiplier như nhặt thật (GameSession.SetSubWeapon).
/// </summary>
public static class CvgpAttackCheatMenu
{
    const string R = "CVGP/Cheat/";

    static void Whip(int lv)
    {
        GameSession.Current.whipLevel = lv;
        GameSession.Current.NotifyChanged();
        CvgpDebug.Log("cheat: roi cấp " + lv);
    }

    static void Sub(eSubWeapon w)
    {
        GameSession.Current.SetSubWeapon(w);
        CvgpDebug.Log("cheat: vũ khí phụ " + w);
    }

    static void Shot(int m)
    {
        GameSession.Current.shotMultiplier = m;
        GameSession.Current.NotifyChanged();
        CvgpDebug.Log("cheat: shot ×" + m);
    }

    [MenuItem(R + "Whip Level 1")] static void W1() { Whip(1); }
    [MenuItem(R + "Whip Level 2")] static void W2() { Whip(2); }
    [MenuItem(R + "Whip Level 3")] static void W3() { Whip(3); }
    [MenuItem(R + "SubWeapon None")] static void S0() { Sub(eSubWeapon.None); }
    [MenuItem(R + "SubWeapon Dagger")] static void S1() { Sub(eSubWeapon.Dagger); }
    [MenuItem(R + "SubWeapon Axe")] static void S2() { Sub(eSubWeapon.Axe); }
    [MenuItem(R + "SubWeapon Holy Water")] static void S3() { Sub(eSubWeapon.HolyWater); }
    [MenuItem(R + "SubWeapon Cross")] static void S4() { Sub(eSubWeapon.Cross); }
    [MenuItem(R + "SubWeapon Stopwatch")] static void S5() { Sub(eSubWeapon.Stopwatch); }
    [MenuItem(R + "Shot I")] static void M1() { Shot(1); }
    [MenuItem(R + "Shot II")] static void M2() { Shot(2); }
    [MenuItem(R + "Shot III")] static void M3() { Shot(3); }

    [MenuItem(R + "Hearts 99")]
    static void Hearts()
    {
        GameSession.Current.hearts = 99;
        GameSession.Current.NotifyChanged();
    }

    [MenuItem(R + "Whip Level 1", true)] static bool V1() { return Application.isPlaying; }
    [MenuItem(R + "Whip Level 2", true)] static bool V2() { return Application.isPlaying; }
    [MenuItem(R + "Whip Level 3", true)] static bool V3() { return Application.isPlaying; }
    [MenuItem(R + "SubWeapon None", true)] static bool VS0() { return Application.isPlaying; }
    [MenuItem(R + "SubWeapon Dagger", true)] static bool VS1() { return Application.isPlaying; }
    [MenuItem(R + "SubWeapon Axe", true)] static bool VS2() { return Application.isPlaying; }
    [MenuItem(R + "SubWeapon Holy Water", true)] static bool VS3() { return Application.isPlaying; }
    [MenuItem(R + "SubWeapon Cross", true)] static bool VS4() { return Application.isPlaying; }
    [MenuItem(R + "SubWeapon Stopwatch", true)] static bool VS5() { return Application.isPlaying; }
    [MenuItem(R + "Shot I", true)] static bool VM1() { return Application.isPlaying; }
    [MenuItem(R + "Shot II", true)] static bool VM2() { return Application.isPlaying; }
    [MenuItem(R + "Shot III", true)] static bool VM3() { return Application.isPlaying; }
    [MenuItem(R + "Hearts 99", true)] static bool VH() { return Application.isPlaying; }
}
