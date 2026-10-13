using UnityEngine;

/// <summary>Tự đi tới autoWalkTarget (cửa cuối stage, cổng lâu đài — routine 15 gọi SimonController.StartAutoWalk), bỏ qua input;
/// tới nơi (hoặc đụng tường) ⇒ Idle + sự kiện AutoWalkDone.</summary>
public class SimonAutoWalkState : SimonState
{
    private bool blocked;

    public override void OnEnterState()
    {
        blocked = false;
        owner.Facing = owner.autoWalkDir;
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
        if (owner.Anim != null)
            owner.Anim.Play(owner.aWalk);
    }

    public override void OnExecuteState()
    {
        int left = Fixed.FromPx(owner.autoWalkTarget) - owner.fx;
        if (left == 0)
            return;
        int step = Mathf.Min(owner.autoWalkSpeed, Mathf.Abs(left));
        blocked = owner.StepX(left > 0 ? step : -step);
    }

    public override void CheckNextState()
    {
        if (owner.fx != Fixed.FromPx(owner.autoWalkTarget) && !blocked)
            return;
        SetState(eSimonState.Idle);
        owner.RaiseAutoWalkDone();
    }

    public override void OnExitState()
    {
    }
}
