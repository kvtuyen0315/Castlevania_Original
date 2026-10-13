using UnityEngine;

/// <summary>State MedusaHead.</summary>
public enum eMedusaHeadState
{
    Fly,
}

/// <summary>
/// MedusaHead (CVGP 17, theo wiki — GẦN ĐÚNG, không có video Block 2–3). Wiki Enemies#Medusa Heads: xuất hiện từ phía Simon đang
/// nhìn (quay lưng ⇒ tới từ cả hai phía), bay ngang theo đường sóng, xuyên địa hình; 1/1 đòn, 300 đ; có thể húc Simon rơi hố.
/// Ước lượng (Enemy.MedusaHead): ngang 1.0 px/tick theo hướng sinh; dọc y = y0 − A·sin(2πt/P), A 24, P 96 (sinh ở TÂM sóng rồi đi
/// lên) ⇒ đỉnh qua đầu Simon, đáy chạm chân (wiki: "nhảy qua hoặc đi/ngồi dưới"). Pivot = tâm sprite. Sinh: EdgeSpawner
/// FrontOfPlayer + playerRelativeY (tâm sóng = chân Simon − spawnAboveFeetPx) — cùng bộ sinh dơi routine 13.
/// </summary>
public class MedusaHead : EnemyBase
{
    public StateMachine<MedusaHead, eMedusaHeadState> Fsm { get; private set; }
    [System.NonSerialized] public int flyVx, baseFy, waveTick;
    private int[] wave;   // độ lệch y fixed theo pha (1 chu kỳ)
    private AnimSet.Anim aFly;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState + " t" + waveTick : "-";

    abstract class HeadState : BaseState<MedusaHead>
    {
        protected void SetState(eMedusaHeadState s) { owner.Fsm.SetState(s); }
    }

    class FlyState : HeadState
    {
        public override void OnEnterState()
        {
            owner.baseFy = owner.fy;
            owner.waveTick = 0;
            if (owner.Anim != null && owner.aFly != null)
                owner.Anim.Play(owner.aFly, true);
        }

        public override void OnExecuteState()
        {
            owner.waveTick++;
            owner.fx += owner.Facing * owner.flyVx;
            owner.fy = owner.baseFy + owner.wave[owner.waveTick % owner.wave.Length];
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<MedusaHead, eMedusaHeadState>(this, "MedusaHead");
        Fsm.Add(eMedusaHeadState.Fly, new FlyState());
        if (animSet != null)
            aFly = animSet.Get("fly");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        flyVx = EnemyParams.MoveSpeed(cfg, id, 1f);
        float amp = EnemyParams.Get(cfg, id, "waveAmplitudePx", 24f);
        int period = Mathf.Max(2, EnemyParams.Int(cfg, id, "wavePeriodTicks", 96));
        if (wave == null || wave.Length != period)
            wave = new int[period];
        // bảng sóng tính 1 lần bằng double (như Bat 13) ⇒ tất định; lưu fixed 1/256 px
        for (int i = 0; i < period; i++)
            wave[i] = -(int)System.Math.Round(amp * System.Math.Sin(2.0 * System.Math.PI * i / period) * Fixed.ONE);
        Fsm.SetState(eMedusaHeadState.Fly);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
