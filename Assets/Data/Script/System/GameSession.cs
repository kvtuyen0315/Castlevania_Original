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
        NotifyChanged();
    }

    public void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
