using System;
using UnityEngine;

/// <summary>Nguyên nhân mất mạng (wiki Gameplay#Damage: hết máu, hết giờ, rơi hố/nước/gai).</summary>
public enum eDeathCause
{
    Damage,
    TimeUp,
    Fall,
}

/// <summary>
/// Luật một ván (CVGP 12) — 1 chỗ duy nhất đổi điểm/mạng/giờ/tim giữa các stage. Trạng thái nằm ở GameSession, số liệu ở Config
/// (ConfigScore, ConfigRule, ConfigSimon, ConfigBlock — H6). Gọi từ StageRules (màn chơi), ItemEffects/vũ khí phụ (điểm),
/// GameOverScreen (Continue) và luồng stage (routine 15: EnterStage / StartNextBlock).
///   Điểm: AddScore ⇒ 1UP ở OneUpFirst (30 000), OneUpSecond (80 000), rồi mỗi OneUpEvery (50 000) — wiki Gameplay.
///   Mất mạng: LoseLife (P giảm 1; về 0 ⇒ Game Over — SUY LUẬN: P-03 đầu ván = 3 lượt chơi, HUD không bao giờ hiện P-00 khi chơi).
///   Chết/Continue: ApplyDeathReset (roi cấp WhipLevelOnDeath, mất vũ khí phụ + multiplier, tim HeartsOnDeath, máu đầy, giờ về
///   giới hạn block nếu TimeResetOnDeath); Continue = đầu stage đang chơi (wiki, ContinueAtStageStart) + đủ mạng, điểm 0 (quy ước).
///   Giờ theo BLOCK (video Map_1: không đặt lại giữa Stage 01→02→03; Stage 04 bắt đầu 0400).
/// </summary>
public static class GameRules
{
    /// <summary>Vừa được 1UP (mạng sau khi cộng).</summary>
    public static event Action<int> OneUp;

    static GameConfig Cfg => GameConfig.Current;
    static GameSession S => GameSession.Current;

    static int Rule(eRuleParam k, int fallback) => Cfg != null ? Cfg.rules.GetInt(k, fallback) : fallback;
    static int Score(eScoreParam k, int fallback) => Cfg != null ? Cfg.score.GetInt(k, fallback) : fallback;
    static int Simon(eSimonParam k, int fallback) => Cfg != null ? Cfg.simon.GetInt(k, fallback) : fallback;

    // ---------------------------------------------------------------- điểm + 1UP

    /// <summary>Mốc 1UP đầu tiên lớn hơn <paramref name="score"/> (30 000, 80 000, 130 000, …).</summary>
    public static int NextOneUpAbove(int score)
    {
        int first = Score(eScoreParam.OneUpFirst, 30000), second = Score(eScoreParam.OneUpSecond, 80000);
        int every = Mathf.Max(1, Score(eScoreParam.OneUpEvery, 50000));
        if (score < first)
            return first;
        if (score < second)
            return second;
        return second + ((score - second) / every + 1) * every;
    }

    /// <summary>Cộng điểm (trần ScoreCap) + 1UP khi vượt mốc. Không gọi NotifyChanged (người gọi gom 1 lần). Trả số mạng vừa thêm.</summary>
    public static int AddScore(int pts)
    {
        var s = S;
        if (pts <= 0)
            return 0;
        if (s.nextOneUp <= 0)
            s.nextOneUp = NextOneUpAbove(s.score);
        s.score = Mathf.Min(Rule(eRuleParam.ScoreCap, 999999), s.score + pts);
        int gained = 0, cap = Rule(eRuleParam.LivesCap, 99);
        while (s.score >= s.nextOneUp)
        {
            s.nextOneUp = NextOneUpAbove(s.nextOneUp);
            if (s.lives < cap)
            {
                s.lives++;
                gained++;
            }
        }
        if (gained > 0)
        {
            Log("1UP: điểm " + s.score + " ⇒ P-" + s.lives.ToString("00") + " (mốc kế " + s.nextOneUp + ")");
            if (OneUp != null)
                OneUp(s.lives);
        }
        return gained;
    }

    // ---------------------------------------------------------------- ván / stage / block

    /// <summary>Giờ giới hạn của block chứa stage (ConfigBlock.timeLimit; Block 1 = 300, Block 2 = 400 — video).</summary>
    public static int TimeLimitOfStage(int stage)
    {
        var b = Cfg != null ? Cfg.GetBlockOfStage(stage) : null;
        return b != null ? b.timeLimit.Get("ConfigBlock.timeLimit block " + b.block, 300) : 300;
    }

    public static int BlockOfStage(int stage)
    {
        var st = Cfg != null ? Cfg.GetStage(stage) : null;
        return st != null ? st.block : 1;
    }

    /// <summary>Stage đầu của block chứa stage (1, 4, 7 …).</summary>
    public static int FirstStageOfBlock(int stage)
    {
        int b = BlockOfStage(stage), s = stage;
        while (s > 1 && BlockOfStage(s - 1) == b)
            s--;
        return s;
    }

    /// <summary>Ván mới (Title → Intro): giá trị đầu ván từ Config.</summary>
    public static void NewGame()
    {
        var s = S;
        s.ResetForNewGame();
        s.lives = Simon(eSimonParam.StartLives, 3);
        s.hearts = Simon(eSimonParam.StartHearts, 5);
        s.playerHp = s.shownPlayerHp = Simon(eSimonParam.MaxHp, GameSession.MAX_HP);
        s.stage = 1;
        s.block = BlockOfStage(1);
        s.time = TimeLimitOfStage(1);
        s.nextOneUp = NextOneUpAbove(s.score);
        s.NotifyChanged();
    }

    /// <summary>
    /// Vào stage <paramref name="stage"/> (luồng stage — routine 15; hồi sinh). Khác block với stage trước, hoặc
    /// <paramref name="resetTime"/> ⇒ giờ = giới hạn block. Thanh ENEMY về đầy (video 3:59.2 Stage 04 E 16).
    /// </summary>
    public static void EnterStage(int stage, bool resetTime)
    {
        var s = S;
        int b = BlockOfStage(stage);
        if (b != s.block)
            resetTime = true;
        s.stage = stage;
        s.block = b;
        if (resetTime)
            s.time = TimeLimitOfStage(stage);
        s.enemyHp = s.shownEnemyHp = GameSession.MAX_HP;
        s.NotifyChanged();
        Log("vào STAGE " + stage.ToString("00") + " (block " + b + ", TIME " + s.time.ToString("0000") + (resetTime ? " đặt lại" : " giữ") + ")");
    }

    /// <summary>
    /// Sau tally của boss (màn bản đồ lâu đài — routine 15): STAGE = stage đầu block kế, tim về HeartsOnNewBlock; roi, vũ khí phụ,
    /// II/III, điểm, mạng GIỮ (video Map_1 3:52.3: STAGE 04, tim 05, TIME 0000, rìu + II còn). Giờ đặt khi EnterStage.
    /// </summary>
    public static void StartNextBlock()
    {
        var s = S;
        int next = s.stage + 1;
        while (next <= 18 && BlockOfStage(next) == s.block)
            next++;
        s.stage = next;
        s.hearts = Simon(eSimonParam.HeartsOnNewBlock, 5);
        s.NotifyChanged();
        Log("sang block mới: STAGE " + next.ToString("00") + ", tim " + s.hearts);
    }

    // ---------------------------------------------------------------- chết / Game Over / Continue

    /// <summary>Mất 1 mạng. Trả true nếu còn mạng (hồi sinh), false ⇒ Game Over.</summary>
    public static bool LoseLife(eDeathCause cause)
    {
        var s = S;
        s.lives = Mathf.Max(0, s.lives - 1);
        s.NotifyChanged();
        Log("mất mạng (" + cause + ") ⇒ P-" + s.lives.ToString("00"));
        return s.lives > 0;
    }

    /// <summary>Đặt lại trạng thái khi hồi sinh / Continue (ConfigRule: WhipLevelOnDeath, LoseSubWeaponOnDeath, HeartsOnDeath, TimeResetOnDeath).</summary>
    public static void ApplyDeathReset()
    {
        var s = S;
        s.whipLevel = Rule(eRuleParam.WhipLevelOnDeath, 1);
        if (Rule(eRuleParam.LoseSubWeaponOnDeath, 1) != 0)
            s.SetSubWeapon(eSubWeapon.None);
        s.shotMultiplier = 1;      // wiki: multiplier gắn với vũ khí phụ — mất cùng vũ khí (SUY LUẬN)
        s.subWeaponHits = 0;
        s.shotIconVisible = true;
        s.hearts = Rule(eRuleParam.HeartsOnDeath, 5);
        s.playerHp = s.shownPlayerHp = Simon(eSimonParam.MaxHp, GameSession.MAX_HP);
        s.enemyHp = s.shownEnemyHp = GameSession.MAX_HP;
        if (Rule(eRuleParam.TimeResetOnDeath, 1) != 0)
            s.time = TimeLimitOfStage(s.stage);
        s.NotifyChanged();
    }

    /// <summary>Continue (màn GAME OVER): đủ mạng, điểm 0 nếu ContinueResetScore, về đầu stage đang chơi (wiki) hoặc đầu block.</summary>
    public static void Continue()
    {
        var s = S;
        if (Rule(eRuleParam.ContinueAtStageStart, 1) == 0)
            s.stage = FirstStageOfBlock(s.stage);
        s.lives = Simon(eSimonParam.StartLives, 3);
        if (Rule(eRuleParam.ContinueResetScore, 1) != 0)
            s.score = 0;
        s.nextOneUp = NextOneUpAbove(s.score);
        ApplyDeathReset();
        s.time = TimeLimitOfStage(s.stage);
        s.NotifyChanged();
        Log("CONTINUE: STAGE " + s.stage.ToString("00") + ", P-" + s.lives.ToString("00") + ", điểm " + s.score);
    }

    /// <summary>Hết mạng: sang màn GAME OVER sau <paramref name="blackTicks"/> tick đen. Không có GameManager (scene thử) ⇒ false.</summary>
    public static bool EnterGameOver(int blackTicks)
    {
        Log("GAME OVER");
        if (GameManager.I == null)
            return false;
        return GameManager.I.GoTo(eGameScreen.GameOver, blackTicks);
    }

    static void Log(string msg)
    {
        if (CvgpDebug.LogRules)
            CvgpDebug.Log("[Rules] " + msg);
    }
}
