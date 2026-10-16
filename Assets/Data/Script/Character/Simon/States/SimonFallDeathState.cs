using UnityEngine;

/// <summary>Chạm nước/hố (wiki Gameplay: chết ngay): rơi thẳng FallSpeed px/tick xuyên địa hình ra khỏi đáy màn (ẩn khi ra hẳn
/// khỏi section), sau DeathTicks báo Died với loại ô.</summary>
public class SimonFallDeathState : SimonState
{
    public override void OnEnterState()
    {
        owner.airVx = 0;
        owner.Hurt.active = false;
        owner.PlayStatic(owner.aIdle);
        owner.drawOffset = Vector2Int.zero;
        owner.RaiseDyingStarted();
    }

    public override void OnExecuteState()
    {
        int limit = Fixed.FromPx((owner.World.Tiles != null ? owner.World.Tiles.Height : 192) + 64);
        if (owner.fy < limit)
            owner.fy = Mathf.Min(owner.fy + owner.fallSpeed, limit);
    }

    public override void CheckNextState()
    {
        if (StateFrame >= owner.deathTicks)
            owner.RaiseDied(owner.lastDeath);
    }

    public override void OnExitState()
    {
        owner.Hurt.active = true;
    }
}
