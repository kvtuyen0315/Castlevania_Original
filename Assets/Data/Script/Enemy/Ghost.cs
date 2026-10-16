using UnityEngine;

/// <summary>State Ghost.</summary>
public enum eGhostState
{
    Chase,
}

/// <summary>
/// Ghost (CVGP 17 theo wiki; CVM2 08 ĐO LẠI video Map_2 — 2 con Hall C). Wiki Enemies#Ghosts: chậm nhưng bám theo vị trí Simon, tới từ
/// góc lạ; 3 đòn yếu / 2 đòn mạnh, 300 đ. Video (Enemy.Ghost): hiện cách Simon 96 px phía SAU lưng, tâm ngang mặt sàn Simon, rồi bay về
/// phía Simon NGAY với tốc độ đều 0.75 px/tick ngang (không thấy pha tăng tốc) và ≈ 0.1–0.12 px/tick dọc (lên dần về tâm thân Simon) ⇒
/// sinh với vx = trần ngang hướng về Simon (spawnFullSpeed); mỗi trục vẫn tăng tốc chaseAccel về tâm thân Simon (chân −
/// targetAboveFeetPx), trần ngang = ConfigEnemy.moveSpeed, trần dọc maxVy ⇒ lao quá rồi vòng lại (wiki — video không có lần vượt qua);
/// xuyên địa hình; quay mặt theo hướng bay ngang. Mỗi đòn chưa hạ: khựng hitStunTicks (video 7–8 frame). Pivot = tâm sprite.
/// </summary>
public class Ghost : EnemyBase
{
    public StateMachine<Ghost, eGhostState> Fsm { get; private set; }
    [System.NonSerialized] public int accel, maxVx, maxVy, targetAbove;
    [System.NonSerialized] public bool spawnFullSpeed;
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
        maxVx = EnemyParams.MoveSpeed(cfg, id, 0.75f);
        accel = EnemyParams.Fixed256(cfg, id, "chaseAccel", 0.015625f);
        maxVy = EnemyParams.Fixed256(cfg, id, "maxVy", 0.125f);
        targetAbove = EnemyParams.Int(cfg, id, "targetAboveFeetPx", 16);
        spawnFullSpeed = EnemyParams.Int(cfg, id, "spawnFullSpeed", 1) != 0;
        InvincibleOnHit = EnemyParams.Int(cfg, id, "hitInvincibleTicks", 16);
        hurtStunTicks = EnemyParams.Int(cfg, id, "hitStunTicks", 16);
        multiHit = true;
        checkOffscreen = false;   // bám Simon — SpawnPoint/bộ sinh lo gỡ
        vx = vy = 0;
        Facing = FacingToPlayer();
        if (spawnFullSpeed)
            vx = Facing * maxVx;   // video: bay đều 0.75 ngay từ frame đầu
        Fsm.SetState(eGhostState.Chase);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
