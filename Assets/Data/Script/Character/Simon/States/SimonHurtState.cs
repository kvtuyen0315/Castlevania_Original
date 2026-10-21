using UnityEngine;

/// <summary>
/// Knockback (CVM3 04 — đo 6 lần trúng ở video Boss_Map_1/2/3, Tools/cvm3/kb4_track.py): khung hurt_0, lùi NGƯỢC hướng mặt KnockbackSpeedX
/// (1 px/tick) SUỐT cung tới khi đáp (tường/mép màn chặn ngang — cung dọc chạy tiếp); độ cao theo BẢNG như nhảy: lên KnockbackRise01..10
/// (cao 14 sau tick 10), giữ đỉnh KnockbackApexTicks (14) tick, xuống = gương ⇒ chạm sàn phẳng ở tick 34 (lùi 34 px); chưa chạm đất
/// (bị hất khỏi mép/xuống hố) ⇒ rơi FallSpeed, vẫn lùi (suy luận như hết bảng nhảy). Trước CVM3 04: parabol lùi 12 px/12 tick — số đo
/// Boss_Map_1 bị mép màn khoá chặn (x màn 239). Suốt cung không nháy (bất tử giữ đầy); đáp đất ⇒ bắt đầu đếm InvincibleTicks. Hết máu ⇒ Dead khi đáp.
/// </summary>
public class SimonHurtState : SimonState
{
    public override void OnEnterState()
    {
        owner.kbTick = 0;
        owner.kbLanded = false;
        owner.airVx = owner.kbDir * owner.kbVx;
        owner.PlayStatic(owner.aHurt);
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
        owner.InvincibleTicks = owner.blinkTicks;
    }

    public override void OnExecuteState()
    {
        owner.InvincibleTicks = owner.blinkTicks;
        owner.kbTick++;
        owner.StepX(owner.airVx);
        int dy = owner.kbTick <= owner.kbTicks ? Fixed.FromPx(owner.kbDy[owner.kbTick]) : owner.fallSpeed;
        if (dy != 0 && owner.StepY(dy, true) && dy > 0)
        {
            owner.kbLanded = true;
            owner.landCount++;
        }
    }

    public override void CheckNextState()
    {
        if (!owner.kbLanded)
            return;
        if (GameSession.Current.playerHp <= 0)
            SetState(eSimonState.Dead);
        else
            owner.ToGroundState();
    }

    public override void OnExitState()
    {
    }
}
