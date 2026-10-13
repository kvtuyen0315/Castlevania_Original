using UnityEngine;

/// <summary>
/// Phantom Bat bản thường (Stage 16, eEnemyId.PhantomBatMinor — CVGP 18, theo wiki — GẦN ĐÚNG, không có video Block 6). Wiki
/// Stages 16-18#Stage 16: "boss đầu tiên quay lại — 5 con và phun được cầu lửa"; có thể giết nhưng không đáng; "nhảy quất 1 phát làm
/// chúng chậm lại vừa đủ để cuộn chúng ra khỏi màn"; con ở dưới cầu nhắm vào độ cao cú nhảy của Simon. Wiki KHÔNG nêu máu/điểm.
/// DÙNG LẠI chuyển động PhantomBat (routine 14 — ngủ/thức/bổ nhào/ngang/vọt/lượn/lơ lửng đo video boss) với tham số riêng
/// ConfigActorParam "Enemy.PhantomBatMinor.*" (BossBase.PInt/PFixed) và khác boss ở:
///   • enemy thường: máu/điểm ConfigEnemy (ƯỚC LƯỢNG — wiki không nêu), không thanh ENEMY, không nhạc boss, không quả cầu, Rosary diệt
///     được (IsBoss = false), Stopwatch giữ được; chết như enemy thường (tia lửa + lửa "death" tại tâm, +điểm, Kill ngay).
///   • thức khi |Simon − dơi| &lt; wakeDistancePx (đặt sẵn bằng SpawnPoint); lúc thức các độ cao tuyệt đối của boss (đáy bổ nhào, đỉnh
///     vọt, 2 độ cao lượn) đặt lại theo CHÂN Simon (…AboveFeetPx) — boss đo ở phòng boss, cầu Stage 16 khác độ cao; đã thức ⇒ ra
///     khỏi màn thì gỡ ("cuộn chúng ra khỏi màn").
///   • phun cầu lửa: lơ lửng được fireDelayTicks tick ⇒ 1 quả nhắm tâm thân Simon (chân − aimAbovePx), tốc fireballSpeed; đạn =
///     BoneTowerFireball (sheet KHÔNG có cầu lửa Phantom Bat ⇒ TẠM dùng sprite cầu lửa Bone Dragon — H2, ghi §5).
///   • khựng khi trúng đòn hitStunTicks dài hơn boss ("1 đòn làm chậm").
/// </summary>
public class PhantomBatMinor : PhantomBat
{
    [SerializeField] private BoneTowerFireball fireballPrefab;

    [System.NonSerialized] public int wakeDist, swoopAbove, riseAbove, glideLowAbove, glideHighAbove, fireDelay, fireSpeed, aimAbove;
    /// <summary>Số cầu lửa đã phun / tuổi lúc thức (harness).</summary>
    [System.NonSerialized] public int shots, wakeAge;

    // khoá boss không dùng ở bản thường (thay bằng …AboveFeetPx / wakeDistancePx; chết như enemy thường)
    private static readonly string[] UNUSED = { "wakeSimonX", "swoopMaxY", "riseTopY", "glideLowY", "glideHighY", "deathFxTicks", "deathFxUpPx" };

    public override bool IsBoss => false;
    protected override bool UsesEnemyBar => false;
    protected override bool PlaysBossMusic => false;

    private static bool Unused(string key)
    {
        return System.Array.IndexOf(UNUSED, key) >= 0;
    }

    protected override int PInt(GameConfig cfg, string key, int fallback)
    {
        return Unused(key) ? fallback : EnemyParams.Int(cfg, enemyId, key, fallback);
    }

    protected override int PFixed(GameConfig cfg, string key, float fallback)
    {
        return Unused(key) ? Fixed.FromFloat(fallback) : EnemyParams.Fixed256(cfg, enemyId, key, fallback);
    }

    /// <summary>Máu/điểm ConfigEnemy (như EnemyBase); Stopwatch giữ được (enemy thường).</summary>
    protected override void ResetHealth(GameConfig cfg)
    {
        var row = cfg != null ? cfg.GetEnemy(enemyId) : null;
        string ctx = "ConfigEnemy." + enemyId;
        Health.Reset(row != null ? row.hitsWeak.Get(ctx + ".hitsWeak", 1) : 1, row != null ? row.hitsStrong.Get(ctx + ".hitsStrong", 1) : 1);
        Score = row != null && row.score.has ? row.score.value : 0;
        stopwatchAffects = true;
    }

    protected override void LoadBoss(GameConfig cfg)
    {
        base.LoadBoss(cfg);
        var id = enemyId;
        wakeDist = EnemyParams.Int(cfg, id, "wakeDistancePx", 96);
        swoopAbove = EnemyParams.Int(cfg, id, "swoopMaxAboveFeetPx", 24);
        riseAbove = EnemyParams.Int(cfg, id, "riseTopAboveFeetPx", 104);
        glideLowAbove = EnemyParams.Int(cfg, id, "glideLowAboveFeetPx", 96);
        glideHighAbove = EnemyParams.Int(cfg, id, "glideHighAboveFeetPx", 72);
        fireDelay = Mathf.Max(1, EnemyParams.Int(cfg, id, "fireDelayTicks", 16));
        fireSpeed = EnemyParams.Fixed256(cfg, id, "fireballSpeed", 1.5f);
        aimAbove = EnemyParams.Int(cfg, id, "aimAbovePx", 16);
        shots = 0;
        wakeAge = -1;
        if (Facing == 0)
            Facing = -1;
    }

    protected override bool ShouldWake(Entity p)
    {
        if (Mathf.Abs(p.X - X) >= wakeDist)
            return false;
        maxY = p.Y - swoopAbove;
        riseTopY = p.Y - riseAbove;
        glideLowY = p.Y - glideLowAbove;
        glideHighY = p.Y - glideHighAbove;
        wakeAge = ageTicks;
        checkOffscreen = true;
        return true;
    }

    protected override void OnHoverTick(int tick)
    {
        if (tick != fireDelay || fireballPrefab == null)
            return;
        shots++;
        var p = World.Player;
        long dx = p != null ? p.X - X : Facing * 64, dy = p != null ? (p.Y - aimAbove) - Y : 0;
        long len = ISqrt((dx * dx + dy * dy) << 16);   // độ dài ×256 (căn nguyên — H7) ⇒ |v| sai < 1/256
        if (len == 0)
        {
            dx = Facing;
            len = 256;
        }
        var f = World.Spawn(fireballPrefab, X, Y + 4, dx >= 0 ? 1 : -1) as BoneTowerFireball;   // quả cầu ở nửa trên ô 8×16
        if (f != null)
        {
            f.vx = (int)(fireSpeed * dx * 256 / len);
            f.vy = (int)(fireSpeed * dy * 256 / len);
        }
    }

    private static long ISqrt(long v)
    {
        if (v <= 0)
            return 0;
        long r = (long)System.Math.Sqrt(v);
        while (r * r > v) r--;
        while ((r + 1) * (r + 1) <= v) r++;
        return r;
    }

    /// <summary>Chết như enemy thường (không chuỗi chết boss / quả cầu).</summary>
    protected override void Die()
    {
        defeated = true;
        var cfg = World.Config;
        FxSprite.Spawn(World, "hit", X, CenterY, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 16) : 16);
        FxSprite.Spawn(World, "death", X, CenterY);
        if (AudioManager.I != null)
            AudioManager.I.PlaySfx(eSfx.HitFlame);
        if (Score > 0)
        {
            GameRules.AddScore(Score);
            GameSession.Current.NotifyChanged();
        }
        Kill();
        RaiseDefeated();
    }
}
