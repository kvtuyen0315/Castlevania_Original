using UnityEngine;

/// <summary>State Ghost.</summary>
public enum eGhostState
{
    Chase,
}

/// <summary>
/// Ghost (CVGP 17, theo wiki — GẦN ĐÚNG, không có video Block 2–3). Wiki Enemies#Ghosts: xuất hiện ở các điểm cố định, chậm nhưng
/// bám theo vị trí Simon và tiếp cận từ góc lạ; 3 đòn yếu / 2 đòn mạnh, 300 đ. Ước lượng (Enemy.Ghost): mỗi trục tăng tốc chaseAccel
/// về phía tâm thân Simon (chân − targetAboveFeetPx), trần ngang = ConfigEnemy.moveSpeed, trần dọc maxVy ⇒ lao quá rồi vòng lại
/// (quỹ đạo cong — "góc lạ"); xuyên địa hình; luôn quay mặt theo hướng bay ngang. Pivot = tâm sprite. Đặt bằng SpawnPoint.
/// </summary>
public class Ghost : EnemyBase
{
    public StateMachine<Ghost, eGhostState> Fsm { get; private set; }
    [System.NonSerialized] public int accel, maxVx, maxVy, targetAbove;
    private AnimSet.Anim aFly;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class GhostState : BaseState<Ghost>
    {
        protected void SetState(eGhostState s) { owner.Fsm.SetState(s); }
    }

    class ChaseState : GhostState
    {
        public override void OnEnterState()
        {
            if (owner.Anim != null && owner.aFly != null)
                owner.Anim.Play(owner.aFly, true);
        }

        public override void OnExecuteState()
        {
            var p = owner.World.Player;
            if (p != null)
            {
                owner.vx = Steer(owner.vx, Fixed.FromPx(p.X) - owner.fx, owner.accel, owner.maxVx);
                owner.vy = Steer(owner.vy, Fixed.FromPx(p.Y - owner.targetAbove) - owner.fy, owner.accel, owner.maxVy);
            }
            owner.fx += owner.vx;
            owner.fy += owner.vy;
            if (owner.vx != 0)
                owner.Facing = owner.vx > 0 ? 1 : -1;
        }
    }

    /// <summary>v += ±a theo dấu khoảng cách d (fixed), kẹp |v| ≤ max.</summary>
    private static int Steer(int v, int d, int a, int max)
    {
        if (d > 0) v += a;
        else if (d < 0) v -= a;
        return Mathf.Clamp(v, -max, max);
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Ghost, eGhostState>(this, "Ghost");
        Fsm.Add(eGhostState.Chase, new ChaseState());
        if (animSet != null)
            aFly = animSet.Get("fly");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        maxVx = EnemyParams.MoveSpeed(cfg, id, 0.625f);
        accel = EnemyParams.Fixed256(cfg, id, "chaseAccel", 0.015625f);
        maxVy = EnemyParams.Fixed256(cfg, id, "maxVy", 0.5f);
        targetAbove = EnemyParams.Int(cfg, id, "targetAboveFeetPx", 16);
        InvincibleOnHit = EnemyParams.Int(cfg, id, "hitInvincibleTicks", 16);
        multiHit = true;
        checkOffscreen = false;   // bám Simon — SpawnPoint lo gỡ
        vx = vy = 0;
        Facing = FacingToPlayer();
        Fsm.SetState(eGhostState.Chase);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
