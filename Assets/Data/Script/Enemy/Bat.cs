using UnityEngine;

/// <summary>State Bat (Vampire Bat).</summary>
public enum eBatState
{
    Perch,
    Fly,
    Swoop,   // CVGP 17: dơi treo sà xuống; CVM2 08 (video Map_2): sà 1/4 sin rồi Glide
    Glide,   // CVM2 08: dơi treo sau khi sà — bay ngang thẳng (không sóng) tới khi ra khỏi màn
}

/// <summary>
/// Bat — "Vampire Bat" trên sheet (CVGP 13). Đo video Map_1 (§2k, 7 vết Crypt A): bay ngang 1.125 px/tick theo hướng sinh (về phía
/// Simon), xuyên địa hình, dọc theo sóng y = y0 + A(1 − cos 2πt/P) với A 8.75 px, P 100 tick (sinh ở ĐỈNH rồi đi xuống — dư rms 0.4),
/// khung bat_red_0→1→2→1 mỗi 9 tick. Pivot = tâm sprite (viewOffset 0). Sinh: EdgeSpawner (mép trước mặt Simon, 1 con/lần).
/// Perch (dơi treo — "Black Bat" wiki; CVM2 08 ĐO video Map_2: 3 con TEA/TEB/Hall C, bảng màu TÍM): treo bat_purple_0 tới khi
/// |dx| &lt; perchTriggerPx (96 — Hall C: kích hoạt ở |dx| 94) VÀ chân Simon thấp hơn dơi &lt; perchTriggerDyPx (80 — Hall C: đứng dy 87
/// chưa, nhảy lên dy 78 thì dơi rời chỗ; TEA nhảy tới dy 107 với |dx| 16 vẫn không) ⇒ Swoop: ngang swoopVx (1.5) + xuống swoopDropPx (31) theo 1/4 sin trong
/// swoopTicks (42) (rms 0.43 px) ⇒ Glide: ngang swoopVx, y giữ nguyên tới khi ra khỏi màn; khung bat_purple_1→2→3→2 mỗi 9 tick.
/// </summary>
public class Bat : EnemyBase
{
    public StateMachine<Bat, eBatState> Fsm { get; private set; }
    [Tooltip("Sinh ở tư thế treo (Black Bat — wiki) thay vì bay ngay. SpawnPoint.param = 1 cũng bật cho 1 điểm.")]
    [SerializeField] private bool perchedByDefault;
    [System.NonSerialized] public bool perched;
    [System.NonSerialized] public int flyVx, baseFy, bobTick, perchTrigger, perchTriggerDy, swoopVx, swoopTick;
    private int[] bob;     // độ lệch y fixed theo pha (1 chu kỳ)
    private int[] swoop;   // độ sà y fixed theo tick (1/4 sin, swoopTicks + 1 ô)
    private AnimSet.Anim aPerch, aFly, aHang, aGlide;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState + " t" + bobTick : "-";

    abstract class BatState : BaseState<Bat>
    {
        protected void SetState(eBatState s) { owner.Fsm.SetState(s); }
    }

    class PerchState : BatState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = false;
            var a = owner.aHang != null ? owner.aHang : owner.aPerch;
            if (owner.Anim != null && a != null)
                owner.Anim.Play(a, true);
        }

        public override void CheckNextState()
        {
            var p = owner.World.Player;
            if (p != null && Mathf.Abs(p.X - owner.X) < owner.perchTrigger && p.Y - owner.Y < owner.perchTriggerDy)
            {
                owner.Facing = owner.FacingToPlayer();
                SetState(eBatState.Swoop);
            }
        }
    }

    class SwoopState : BatState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = true;
            owner.baseFy = owner.fy;
            owner.swoopTick = 0;
            var a = owner.aGlide != null ? owner.aGlide : owner.aFly;
            if (owner.Anim != null && a != null)
                owner.Anim.Play(a, true);
        }

        public override void OnExecuteState()
        {
            if (owner.swoopTick >= owner.swoop.Length - 1)
            {
                SetState(eBatState.Glide);   // sà xong — Glide chạy luôn trong tick này (1 bước ngang, không bước kép)
                return;
            }
            owner.swoopTick++;
            owner.fx += owner.Facing * owner.swoopVx;
            owner.fy = owner.baseFy + owner.swoop[owner.swoopTick];
        }
    }

    class GlideState : BatState
    {
        public override void OnEnterState() { owner.checkOffscreen = true; }

        public override void OnExecuteState()
        {
            owner.fx += owner.Facing * owner.swoopVx;
        }
    }

    class FlyState : BatState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = true;
            owner.baseFy = owner.fy;
            owner.bobTick = 0;
            if (owner.Anim != null && owner.aFly != null)
                owner.Anim.Play(owner.aFly, true);
        }

        public override void OnExecuteState()
        {
            owner.bobTick++;
            owner.fx += owner.Facing * owner.flyVx;
            owner.fy = owner.baseFy + owner.bob[owner.bobTick % owner.bob.Length];
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Bat, eBatState>(this, "Bat");
        Fsm.Add(eBatState.Perch, new PerchState());
        Fsm.Add(eBatState.Fly, new FlyState());
        Fsm.Add(eBatState.Swoop, new SwoopState());
        Fsm.Add(eBatState.Glide, new GlideState());
        if (animSet != null)
        {
            aPerch = animSet.Get("perch");
            aFly = animSet.Get("fly");
            aHang = animSet.Get("purple_hang");    // CVM2 08: dơi treo Map_2 = bat_purple_0
            aGlide = animSet.Get("purple_fly");    // bat_purple_1→2→3→2 × 9 tick
        }
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        flyVx = EnemyParams.MoveSpeed(cfg, enemyId, 1.125f);
        float amp = EnemyParams.Get(cfg, enemyId, "bobAmplitudePx", 8.75f);
        int period = Mathf.Max(2, EnemyParams.Int(cfg, enemyId, "bobPeriodTicks", 100));
        perchTrigger = EnemyParams.Int(cfg, enemyId, "perchTriggerPx", 96);
        perchTriggerDy = EnemyParams.Int(cfg, enemyId, "perchTriggerDyPx", 80);
        swoopVx = EnemyParams.Fixed256(cfg, enemyId, "swoopVx", 1.5f);
        float drop = EnemyParams.Get(cfg, enemyId, "swoopDropPx", 31f);
        int sTicks = Mathf.Max(1, EnemyParams.Int(cfg, enemyId, "swoopTicks", 42));
        if (swoop == null || swoop.Length != sTicks + 1)
            swoop = new int[sTicks + 1];
        // 1/4 sin (vận tốc dọc giảm dần về 0 ở cuối — video Hall C f5043–5061, rms 0.43 px); tính 1 lần bằng double ⇒ tất định
        for (int i = 0; i <= sTicks; i++)
            swoop[i] = (int)System.Math.Round(drop * System.Math.Sin(0.5 * System.Math.PI * i / sTicks) * Fixed.ONE);
        if (bob == null || bob.Length != period)
            bob = new int[period];
        // bảng sóng tính 1 lần (double IEEE ⇒ như nhau mọi lần chạy), lưu fixed 1/256 px (H4/H7)
        for (int i = 0; i < period; i++)
            bob[i] = (int)System.Math.Round(amp * (1.0 - System.Math.Cos(2.0 * System.Math.PI * i / period)) * Fixed.ONE);
        bobTick = 0;
        baseFy = fy;
        perched = perchedByDefault || (Spawner != null && Spawner.param == 1);
        Fsm.SetState(perched ? eBatState.Perch : eBatState.Fly);
    }

    /// <summary>Chuyển sang tư thế treo ngay sau khi sinh (cheat / harness CVGP 17 — màn thật dùng SpawnPoint.param = 1).</summary>
    public void ForcePerch()
    {
        perched = true;
        Fsm.SetState(eBatState.Perch);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
