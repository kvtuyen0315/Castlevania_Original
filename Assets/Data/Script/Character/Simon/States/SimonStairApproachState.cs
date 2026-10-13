using UnityEngine;

/// <summary>Bấm ↑/↓ khi đứng phía NGOÀI chân/đỉnh thang (trong StairEnterRange): tự đi WalkSpeed tới đúng điểm vào rồi leo/xuống
/// (suy luận — video chỉ có cảnh vào thang từ gầm thang).</summary>
public class SimonStairApproachState : SimonState
{
    public override void OnEnterState()
    {
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
        if (owner.approachX != owner.X)
            owner.Facing = owner.approachX > owner.X ? 1 : -1;
        if (owner.Anim != null)
            owner.Anim.Play(owner.aWalk);
    }

    public override void OnExecuteState()
    {
        int left = Fixed.FromPx(owner.approachX) - owner.fx;
        if (left == 0)
            return;
        int step = Mathf.Min(owner.walkSpeed, Mathf.Abs(left));
        owner.StepX(left > 0 ? step : -step);
    }

    public override void CheckNextState()
    {
        if (owner.fx != Fixed.FromPx(owner.approachX))
            return;
        bool up = owner.approachNext == eSimonState.StairUp;
        owner.stairMoveDir = up ? 1 : -1;
        owner.SetStairT(up ? 0 : Fixed.FromPx(owner.stair.Length));
        SetState(owner.approachNext);
    }

    public override void OnExitState()
    {
    }
}
