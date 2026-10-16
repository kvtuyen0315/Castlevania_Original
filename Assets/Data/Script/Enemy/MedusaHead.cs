using UnityEngine;

/// <summary>State MedusaHead.</summary>
public enum eMedusaHeadState
{
    Fly,
}

/// <summary>
/// MedusaHead (CVGP 17 theo wiki; CVM2 08 ĐO LẠI video Map_2 — 19 con ở Hall A/B + sảnh Medusa). Wiki Enemies#Medusa Heads: xuất hiện từ
/// phía Simon đang nhìn, bay ngang theo sóng, xuyên địa hình; 1/1 đòn, 300 đ; có thể húc Simon rơi hố.
/// Video (Enemy.MedusaHead): ngang 1.25 px/tick theo hướng sinh; dọc y = y0 + A·(1 − cos 2πt/P), A 33.75, P 100 (khớp sin 19 vết, rms
/// trung vị 0.95 px — tốt hơn cung parabol 1.27 / tam giác 2.95): SINH Ở ĐỈNH sóng rồi đi XUỐNG (đỉnh = chân Simon − 49 lúc sinh, kể cả
/// khi Simon đang nhảy; đáy = chân + 19 ⇒ đi xuyên sàn). Pivot = tâm sprite. Sinh: EdgeSpawner FrontOfPlayer + playerRelativeY, đều
/// 128 tick/con (15 khoảng 126.8–129.0).
/// </summary>
public class MedusaHead : EnemyBase
{
    public StateMachine<MedusaHead, eMedusaHeadState> Fsm { get; private set; }
    [System.NonSerialized] public int flyVx, baseFy, waveTick;
    private int[] wave;   // độ lệch y fixed theo pha (1 chu kỳ) — 0 ở đỉnh (lúc sinh), 2A ở đáy
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
        flyVx = EnemyParams.MoveSpeed(cfg, id, 1.25f);
        float amp = EnemyParams.Get(cfg, id, "waveAmplitudePx", 33.75f);
        int period = Mathf.Max(2, EnemyParams.Int(cfg, id, "wavePeriodTicks", 100));
        if (wave == null || wave.Length != period)
            wave = new int[period];
        // bảng sóng tính 1 lần bằng double (như Bat 13) ⇒ tất định; lưu fixed 1/256 px. Sinh ở đỉnh (pha 0) rồi đi xuống (video)
        for (int i = 0; i < period; i++)
            wave[i] = (int)System.Math.Round(amp * (1.0 - System.Math.Cos(2.0 * System.Math.PI * i / period)) * Fixed.ONE);
        Fsm.SetState(eMedusaHeadState.Fly);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
