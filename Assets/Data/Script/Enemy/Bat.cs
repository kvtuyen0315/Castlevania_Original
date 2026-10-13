using UnityEngine;

/// <summary>State Bat (Vampire Bat).</summary>
public enum eBatState
{
    Perch,
    Fly,
}

/// <summary>
/// Bat — "Vampire Bat" trên sheet (CVGP 13). Đo video Map_1 (§2k, 7 vết Crypt A): bay ngang 1.125 px/tick theo hướng sinh (về phía
/// Simon), xuyên địa hình, dọc theo sóng y = y0 + A(1 − cos 2πt/P) với A 8.75 px, P 100 tick (sinh ở ĐỈNH rồi đi xuống — dư rms 0.4),
/// khung bat_red_0→1→2→1 mỗi 9 tick. Pivot = tâm sprite (viewOffset 0). Sinh: EdgeSpawner (mép trước mặt Simon, 1 con/lần).
/// Perch (dơi treo — wiki Enemies#Bats "Black Bat"; KHÔNG có trong video Map 1): đứng yên tới khi |dx| &lt; perchTriggerPx rồi bay
/// như trên về phía Simon (QUY ƯỚC — routine 17 chỉnh theo Stage 4).
/// </summary>
public class Bat : EnemyBase
{
    public StateMachine<Bat, eBatState> Fsm { get; private set; }
    [Tooltip("Sinh ở tư thế treo (Black Bat — wiki) thay vì bay ngay. SpawnPoint.param = 1 cũng bật cho 1 điểm.")]
    [SerializeField] private bool perchedByDefault;
    [System.NonSerialized] public bool perched;
    [System.NonSerialized] public int flyVx, baseFy, bobTick, perchTrigger;
    private int[] bob;   // độ lệch y fixed theo pha (1 chu kỳ)
    private AnimSet.Anim aPerch, aFly;

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
            if (owner.Anim != null && owner.aPerch != null)
                owner.Anim.Play(owner.aPerch, true);
        }

        public override void CheckNextState()
        {
            var p = owner.World.Player;
            if (p != null && Mathf.Abs(p.X - owner.X) < owner.perchTrigger)
            {
                owner.Facing = owner.FacingToPlayer();
                SetState(eBatState.Fly);
            }
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
        if (animSet != null)
        {
            aPerch = animSet.Get("perch");
            aFly = animSet.Get("fly");
        }
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        flyVx = EnemyParams.MoveSpeed(cfg, enemyId, 1.125f);
        float amp = EnemyParams.Get(cfg, enemyId, "bobAmplitudePx", 8.75f);
        int period = Mathf.Max(2, EnemyParams.Int(cfg, enemyId, "bobPeriodTicks", 100));
        perchTrigger = EnemyParams.Int(cfg, enemyId, "perchTriggerPx", 64);
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

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
