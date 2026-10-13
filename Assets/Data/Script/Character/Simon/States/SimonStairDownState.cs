/// <summary>Xuống thang StairSpeedX px/tick; khung stairs_down_0/walk_1 theo vị trí, vẽ lùi StairDownDrawBack (video Map_1 1:47.5,
/// 1:54.4); tới chân ⇒ ra sàn dưới; tới bội StairStepPx mà không giữ hướng xuống ⇒ dừng hoặc quay lên.</summary>
public class SimonStairDownState : SimonState
{
    public override void OnEnterState()
    {
        owner.stairMoveDir = -1;
        owner.ApplyStairPose();
    }

    public override void OnExecuteState()
    {
        owner.StairMove(-1);
    }

    public override void CheckNextState()
    {
        if (owner.AtStairBottom)
        {
            owner.ExitStair();
            return;
        }
        if (!owner.AtStairStep)
            return;
        int dir = owner.StairInput();
        if (dir > 0)
            SetState(eSimonState.StairUp);
        else if (dir == 0)
            SetState(eSimonState.StairIdle);
    }

    public override void OnExitState()
    {
    }
}
