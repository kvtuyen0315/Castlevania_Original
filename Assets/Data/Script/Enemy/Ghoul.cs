using UnityEngine;

/// <summary>State Ghoul (Zombie).</summary>
public enum eGhoulState
{
    Walk,
    Fall,
}

/// <summary>
/// Ghoul — "Zombie" trên sheet (CVGP 13). Đo video Map_1 (CVGP_MEMORY §2k): đi thẳng 0.875 px/tick theo hướng sinh, không quay đầu
/// (19 vết), khung zombie_0/1 mỗi 17 tick; bước khỏi mép gờ (dò sàn ĐÚNG cột pivot — PB x 127 khi mép 128) ⇒ rơi THẲNG ĐỨNG (x giữ)
/// vy từ 0, g 0.125 rồi đi tiếp. Sinh: EdgeSpawner (mép màn, nhóm 3 cách 32 tick). Đụng tường ⇒ quay đầu (quy ước — video không có).
/// </summary>
public class Ghoul : EnemyBase
{
    public StateMachine<Ghoul, eGhoulState> Fsm { get; private set; }
    [System.NonSerialized] public int walkVx, gravity;
    private AnimSet.Anim aWalk;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class GhoulState : BaseState<Ghoul>
    {
        protected void SetState(eGhoulState s) { owner.Fsm.SetState(s); }
    }

    class WalkState : GhoulState
    {
        public override void OnExecuteState()
        {
            if (owner.StepX(owner.Facing * owner.walkVx))
                owner.Facing = -owner.Facing;
        }

        public override void CheckNextState()
        {
            if (!owner.Grounded())
                SetState(eGhoulState.Fall);
        }
    }

    class FallState : GhoulState
    {
        public override void OnEnterState() { owner.vy = 0; }

        public override void OnExecuteState()
        {
            owner.vy += owner.gravity;
            if (owner.StepFall(owner.vy))
            {
                owner.vy = 0;
                SetState(eGhoulState.Walk);
            }
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Ghoul, eGhoulState>(this, "Ghoul");
        Fsm.Add(eGhoulState.Walk, new WalkState());
        Fsm.Add(eGhoulState.Fall, new FallState());
        if (animSet != null)
            aWalk = animSet.Get("walk");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        walkVx = EnemyParams.MoveSpeed(cfg, enemyId, 0.875f);
        gravity = EnemyParams.Fixed256(cfg, enemyId, "fallGravity", 0.125f);
        vy = 0;
        Fsm.SetState(Grounded() ? eGhoulState.Walk : eGhoulState.Fall);
        if (Anim != null && aWalk != null)
            Anim.Play(aWalk, true);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
