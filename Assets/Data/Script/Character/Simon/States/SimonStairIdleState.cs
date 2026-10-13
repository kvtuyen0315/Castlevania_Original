/// <summary>Đứng trên thang (ở bội StairStepPx): giữ khung theo vị trí; ↑/hướng lên ⇒ StairUp, ↓/hướng xuống ⇒ StairDown.
/// Không nhảy, không rơi trên thang (CV1).</summary>
public class SimonStairIdleState : SimonState
{
    public override void OnEnterState()
    {
        owner.ApplyStairPose();
    }

    public override void OnExecuteState()
    {
    }

    public override void CheckNextState()
    {
        if (owner.TryAttack())
            return;
        int dir = owner.StairInput();
        if (dir > 0 && !owner.AtStairTop)
            SetState(eSimonState.StairUp);
        else if (dir < 0 && !owner.AtStairBottom)
            SetState(eSimonState.StairDown);
    }

    public override void OnExitState()
    {
    }
}
