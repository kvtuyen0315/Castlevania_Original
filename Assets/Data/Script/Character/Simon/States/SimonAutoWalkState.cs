using UnityEngine;

/// <summary>Tự đi tới autoWalkTarget (cửa cuối stage, cổng lâu đài — routine 15 gọi SimonController.StartAutoWalk), bỏ qua input;
/// tới nơi (hoặc đụng tường; autoWalkNoClip ⇒ không va địa hình) ⇒ Idle + sự kiện AutoWalkDone. noClip (qua cửa) ⇒ tới nơi thì ĐỨNG YÊN
/// trong state này (khung idle, không xét sàn — Simon đang ở ngoài mép section nguồn) tới khi luồng stage nạp section đích (Teleport).</summary>
public class SimonAutoWalkState : SimonState
{
    private bool blocked, arrived;

    public override void OnEnterState()
    {
        blocked = false;
        arrived = false;
        owner.Facing = owner.autoWalkDir;
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
        if (owner.Anim != null)
            owner.Anim.Play(owner.aWalk);
    }

    public override void OnExecuteState()
    {
        if (arrived)
            return;
        int left = Fixed.FromPx(owner.autoWalkTarget) - owner.fx;
        if (left == 0)
            return;
        int step = Mathf.Min(owner.autoWalkSpeed, Mathf.Abs(left));
        if (owner.autoWalkNoClip)
            owner.fx += left > 0 ? step : -step;   // qua cửa: không va ô cửa / mép section (CVGP 15)
        else
            blocked = owner.StepX(left > 0 ? step : -step);
    }

    public override void CheckNextState()
    {
        if (arrived || (owner.fx != Fixed.FromPx(owner.autoWalkTarget) && !blocked))
            return;
        if (owner.autoWalkNoClip)
        {
            arrived = true;
            owner.PlayStatic(owner.aIdle);
            owner.RaiseAutoWalkDone();
            return;
        }
        SetState(eSimonState.Idle);
        owner.RaiseAutoWalkDone();
    }

    public override void OnExitState()
    {
    }
}
