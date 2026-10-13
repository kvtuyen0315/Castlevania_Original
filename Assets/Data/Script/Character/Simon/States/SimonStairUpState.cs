/// <summary>Lên thang StairSpeedX px/tick theo đường chéo (đo video: 0.5 + 0.5, liên tục); khung theo vị trí; tới đỉnh ⇒ ra sàn
/// trên; tới bội StairStepPx mà không giữ hướng lên ⇒ dừng (StairIdle) hoặc quay xuống.</summary>
public class SimonStairUpState : SimonState
{
    public override void OnEnterState()
    {
        owner.stairMoveDir = 1;
        owner.ApplyStairPose();
    }

    public override void OnExecuteState()
    {
        owner.StairMove(1);
    }

    public override void CheckNextState()
    {
        if (owner.AtStairTop)
        {
            owner.ExitStair();
            return;
        }
        if (owner.TryAttack())
            return;
        if (!owner.AtStairStep)
            return;
        int dir = owner.StairInput();
        if (dir < 0)
            SetState(eSimonState.StairDown);
        else if (dir == 0)
            SetState(eSimonState.StairIdle);
    }

    public override void OnExitState()
    {
    }
}
