using UnityEngine;

/// <summary>Hết máu (sau khi đáp knockback, hoặc ngay trên thang): death_0 (quỳ, vẽ như ngồi) DeathKneelTicks tick rồi death_1 (nằm);
/// sau DeathTicks báo sự kiện Died (routine 12: trừ mạng/hồi sinh). Video/wiki không có cảnh chết ⇒ số là quy ước/suy luận (ConfigSimon).</summary>
public class SimonDeadState : SimonState
{
    public override void OnEnterState()
    {
        owner.vx = 0;
        owner.Hurt.active = false;
        if (owner.Anim != null && owner.aDeath != null)
            owner.Anim.SetFrame(owner.aDeath, 0);
        owner.drawOffset = new Vector2Int(0, owner.crouchDrawY);
        owner.RaiseDyingStarted();
    }

    public override void OnExecuteState()
    {
        if (StateFrame == owner.deathKneelTicks && owner.Anim != null && owner.aDeath != null)
        {
            owner.Anim.SetFrame(owner.aDeath, 1);
            owner.drawOffset = Vector2Int.zero;
        }
    }

    public override void CheckNextState()
    {
        if (StateFrame >= owner.deathTicks)
            owner.RaiseDied(owner.hazardDeath ? eTileCollision.Hazard : eTileCollision.Empty);   // CVM2 06: gai bẫy ép ⇒ Hazard
    }

    public override void OnExitState()
    {
        owner.Hurt.active = true;
        owner.drawOffset = Vector2Int.zero;
    }
}
