using System;

public enum eSubWeapon
{
    None,
    Dagger,
    Axe,
    HolyWater,
    Cross,
    Stopwatch,
}

/// <summary>
/// Dữ liệu ván chơi mà HUD hiển thị (CVTI_MEMORY §4.5). Giá trị đầu ván lấy từ video (Intro/Stage 1):
/// SCORE-000000 TIME 0300 STAGE 01, PLAYER/ENEMY đầy 16 vạch, tim 05, mạng 03, chưa có vũ khí phụ.
/// HUD chỉ đọc; logic gameplay (đếm giờ, trừ máu…) là cụm sau — đổi giá trị xong gọi NotifyChanged().
/// </summary>
public class GameSession
{
    public const int MAX_HP = 16;

    public static GameSession Current { get; } = new GameSession();

    public int score;
    public int time;
    public int stage;
    public int playerHp;
    public int enemyHp;
    public int hearts;
    public int lives;
    public eSubWeapon subWeapon;
    // CVGP 09 — tấn công (thêm trường, không đổi nghĩa trường CVTI). HUD (routine 12) đọc whipLevel/shotMultiplier.
    /// <summary>Cấp roi 1–3 (Morning Star nâng cấp — routine 11; chết ⇒ ConfigRule.WhipLevelOnDeath — routine 12).</summary>
    public int whipLevel;
    /// <summary>Số vũ khí phụ tối đa trên màn: 1 thường, 2 = Double Shot, 3 = Triple Shot (ConfigRule.*OnScreen).</summary>
    public int shotMultiplier;
    /// <summary>Số lần vũ khí phụ HIỆN TẠI trúng enemy (wiki: 10 lần ⇒ rơi multiplier — routine 11 đọc/đặt lại).</summary>
    public int subWeaponHits;

    public event Action Changed;

    public GameSession()
    {
        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        score = 0;
        time = 300;
        stage = 1;
        playerHp = MAX_HP;
        enemyHp = MAX_HP;
        hearts = 5;
        lives = 3;
        subWeapon = eSubWeapon.None;
        whipLevel = 1;
        shotMultiplier = 1;
        subWeaponHits = 0;
        NotifyChanged();
    }

    /// <summary>Đổi vũ khí phụ (nhặt item — routine 11; cheat): vũ khí KHÁC ⇒ mất multiplier + đếm trúng (wiki Items#Double Shot).</summary>
    public void SetSubWeapon(eSubWeapon w)
    {
        if (w != subWeapon)
        {
            shotMultiplier = 1;
            subWeaponHits = 0;
        }
        subWeapon = w;
        NotifyChanged();
    }

    public void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
