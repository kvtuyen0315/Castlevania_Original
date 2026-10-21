using UnityEngine;

/// <summary>
/// Băng gạc Mummy Man ném (CVGP 19 theo wiki; CVM3 09 đo lại 2 video — 17 băng, Tools/cvm3/boss9m_model.py, CVM3_MEMORY §2i). Nhóm EnemyShot,
/// hộp EnemyAttack (sát thương = DamageToPlayer — Block 3: 3 vạch, video Boss_Map_3 f542/f702/f798). Chuyển động (Launch):
///   x: ±vx đều theo hướng ném (video 1.43–1.55 px/tick ⇒ 1.5), sinh ở X Mummy (lệch ≤ 1.5 px);
///   y: tâm sinh y0 rồi y = y0 + A·sin(2π·t/P) — đi XUỐNG trước (A 24, P 102: 17 băng rms TB 0.65 px), xuyên địa hình;
///   khung bandage_0↔1 mỗi 9 tick (manifest); gỡ khi tâm ra khỏi [cx + inset, cx + 256 − inset) (video: mất ở cx+8 / cx+248).
/// QUẤT VỠ ĐƯỢC (video Boss_Map_3 f505–507, f637–640, f835–837, f900–903: roi chạm băng ⇒ lửa nhỏ + SCORE +100) ⇒ Hurtbox lớp Enemy như cầu
/// lửa Bone Tower (ConfigScore.EnemyProjectilePoints 100); vỡ ⇒ tia "hit" + lửa "death" (như enemy chết — suy luận khung lửa). Stopwatch:
/// Mummy Men miễn ("Stopwatch vô dụng" — wiki) ⇒ băng cũng không bị giữ (SUY LUẬN). Toạ độ = tâm sprite (bandage 16×16, pivot (8,8)).
/// </summary>
public class MummyBandage : Entity
{
    public static readonly RectInt BOX = new RectInt(-8, -3, 16, 9);   // bbox pixel đục bandage_0/1 (hàng 5–13, rộng 16)

    public Hitbox Hit { get; private set; }
    public Hurtbox Hurt { get; private set; }
    /// <summary>Bị Simon phá (harness).</summary>
    [System.NonSerialized] public bool broken;
    /// <summary>Tick từ lúc ném, tâm y lúc sinh (fixed), số đo sóng/biên — harness đọc.</summary>
    [System.NonSerialized] public int age, y0, amp, period, inset;
    private int[] wave;
    private static int[] cachedWave;
    private static int cachedAmp = -1, cachedPeriod = -1;

    protected override void OnBuild()
    {
        Hit = AddHitbox(eHitLayer.EnemyAttack, BOX);
        Hurt = AddHurtbox(eHurtLayer.Enemy, BOX);
    }

    protected override void OnSpawned()
    {
        Hit.damage = World.DamageToPlayer();
        Hit.Activate(BOX);
        Hurt.active = true;
        broken = false;
        age = 0;
        y0 = fy;
        amp = 0;
        period = 1;
        inset = 8;
        wave = null;
        if (Anim != null && animSet != null)
        {
            Anim.Visible = true;
            Anim.Play(animSet.Get("bandage"), true);
        }
    }

    /// <summary>Bắt đầu bay (gọi ngay sau World.Spawn): vx fixed (dấu = hướng), sóng biên ampPx / chu kỳ periodTicks, gỡ cách mép insetPx.</summary>
    public void Launch(int vxFixed, int ampPx, int periodTicks, int insetPx)
    {
        vx = vxFixed;
        y0 = fy;
        amp = ampPx;
        period = Mathf.Max(1, periodTicks);
        inset = insetPx;
        wave = Wave(amp, period);
    }

    /// <summary>Bảng sóng fixed: w[i] = round(A·sin(2πi/P)·256) — dựng 1 lần bằng System.Math (tất định).</summary>
    public static int[] Wave(int ampPx, int periodTicks)
    {
        if (cachedWave != null && cachedAmp == ampPx && cachedPeriod == periodTicks)
            return cachedWave;
        var w = new int[periodTicks];
        for (int i = 0; i < periodTicks; i++)
            w[i] = (int)System.Math.Round(Fixed.FromPx(ampPx) * System.Math.Sin(2.0 * System.Math.PI * i / periodTicks));
        cachedWave = w;
        cachedAmp = ampPx;
        cachedPeriod = periodTicks;
        return w;
    }

    public override bool IsFrozenBy(bool worldFrozen, bool enemyFrozen)
    {
        return worldFrozen;
    }

    public override void OnTick()
    {
        age++;
        fx += vx;
        if (wave != null)
            fy = y0 + wave[age % period];
        int cx = World.CameraCtl != null ? World.CameraCtl.X : 0;
        if (X < cx + inset || X >= cx + NesCoord.SCREEN_W - inset)
        {
            Kill(eKillReason.Offscreen);
            return;
        }
        base.OnTick();
    }

    public override void OnHitLanded(ref HitInfo hit)
    {
        Kill();
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        if (!Alive)
            return false;
        broken = true;
        var cfg = World.Config;
        FxSprite.Spawn(World, "hit", X, Y, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 16) : 16);
        FxSprite.Spawn(World, "death", X, Y);
        int pts = cfg != null && cfg.score != null ? cfg.score.GetInt(eScoreParam.EnemyProjectilePoints) : 0;
        if (pts > 0)
        {
            GameRules.AddScore(pts);
            GameSession.Current.NotifyChanged();
        }
        Kill();
        return true;
    }
}
