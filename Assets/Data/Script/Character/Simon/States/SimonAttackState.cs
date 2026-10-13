using UnityEngine;

/// <summary>
/// Vung roi / ném vũ khí phụ (CVGP 09) — 1 state cho 4 tư thế (eAttackPose; chọn lúc bấm ở SimonController.StartAttack).
/// Nhịp 3 khung đo video Map_1 (52 cú roi, Tools/cvgp/whip_timing.py): chuẩn bị WhipWindupTicks → vắt WhipRaiseTicks →
/// duỗi WhipExtendTicks (6/7/9 = 22 tick); ném dùng cùng nhịp (dao 1:32.9, rìu 3:27.9). Khung do state đặt mỗi tick
/// (AnimPlayer.SetFrame) vì roi cấp 3 đổi anim màu mỗi WhipColorTicks tick (whip_l3c1..4, thân use_*_2..5 đổi màu chuôi).
/// Hitbox roi bật suốt khung duỗi (1 đòn ⇒ mỗi mục tiêu trúng 1 lần). Vũ khí phụ sinh ở tick SubWeaponSpawnTick của khung duỗi.
/// Trên đất: đứng yên (video 2:04.5); trên không: cú nhảy/rơi chạy tiếp (AirStep — video 0:24.1…), xong quay về Jump (co
/// chân, giữ jumpTick) hoặc Fall; trên thang: đứng nguyên bậc, xong về StairIdle (đang giữa bậc ⇒ đi tiếp tới bậc).
/// Kết thúc kiểm ở ĐẦU Execute của tick atkTotal+1 ⇒ state kế tiếp chạy chuyển động của tick đó (không đi 2 lần/tick).
/// </summary>
public class SimonAttackState : SimonState
{
    public override void OnEnterState()
    {
        owner.vx = 0;
        owner.atkPhase = 0;
        owner.atkColor = 0;
        owner.WhipHit.Deactivate();
        ApplyPose();
        ApplyFrame(0);
    }

    public override void OnExecuteState()
    {
        int t = StateFrame;
        if (t > owner.atkTotal)
        {
            End();
            return;
        }
        Motion();
        int phase = t <= owner.atkWindup ? 0 : t <= owner.atkWindup + owner.atkRaise ? 1 : 2;
        owner.atkPhase = phase;
        ApplyPose();
        ApplyFrame(t - 1);
        if (owner.atkThrow)
        {
            int k = t - owner.atkWindup - owner.atkRaise - 1;   // tick thứ k (0 = đầu) của khung duỗi
            if (k == owner.subSpawnTick && !owner.atkSpawned)
            {
                owner.atkSpawned = true;
                owner.SpawnSubWeapon();
            }
            return;
        }
        if (phase == 2)
        {
            var a = owner.Anim != null ? owner.Anim.Current : null;
            var rect = owner.WhipRect(a);
            owner.WhipHit.strength = owner.whipStrength[owner.atkLevel];
            if (!owner.WhipHit.active)
                owner.WhipHit.Activate(rect);
            else
                owner.WhipHit.local = rect;
        }
        else
            owner.WhipHit.Deactivate();
    }

    public override void CheckNextState()
    {
    }

    public override void OnExitState()
    {
        owner.WhipHit.Deactivate();
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
    }

    /// <summary>Chuyển động trong lúc đánh: trên không tiếp tục bảng nhảy / rơi; trên đất đứng yên (mất sàn ⇒ rơi).</summary>
    private void Motion()
    {
        if (owner.atkPose >= eAttackPose.StairsUp)
            return;
        if (!owner.atkAir)
        {
            if (!owner.OnGround())
            {
                owner.atkAir = true;
                owner.atkFalling = true;
                owner.atkFallTicks = 0;
                owner.airVx = 0;
                owner.atkPose = eAttackPose.Stand;
            }
            else
                return;
        }
        if (owner.atkLanded || !owner.CanStepY)   // đã đáp, hoặc tick này state trước đã di chuyển (chạy nối)
            return;
        if (!owner.atkFalling)
        {
            if (owner.AirStep())
                owner.atkLanded = true;
            else if (owner.jumpTick >= owner.jumpTicks)
                owner.atkFalling = true;
            return;
        }
        if (owner.airVx != 0)
            owner.StepX(owner.airVx);
        owner.atkFallTicks++;
        if (owner.StepY(owner.fallSpeed))
        {
            owner.atkLanded = true;
            owner.landCount++;
        }
    }

    private void ApplyPose()
    {
        switch (owner.atkPose)
        {
            case eAttackPose.Duck:
                owner.drawOffset = new Vector2Int(0, owner.crouchDrawY);
                owner.Hurt.local = owner.crouchHurt;
                break;
            case eAttackPose.StairsDown:
                // cùng chỗ vẽ với khung xuống thang (video 1:47.1: pivot khớp mẫu 57 cả stairs_down_0 lẫn use_stairs_down)
                owner.drawOffset = new Vector2Int(owner.stairDownBack * owner.stair.upDx, 0);
                owner.Hurt.local = owner.standHurt;
                break;
            default:
                owner.drawOffset = Vector2Int.zero;
                owner.Hurt.local = owner.standHurt;
                break;
        }
    }

    /// <summary>Đặt khung thân (+ roi) theo pha; roi cấp 3 đổi màu theo tick trong đòn (c1→c2→c3→c4).</summary>
    private void ApplyFrame(int tick0)
    {
        if (owner.Anim == null)
            return;
        int pose = (int)owner.atkPose;
        AnimSet.Anim a;
        if (owner.atkThrow)
            a = owner.aThrow[pose];
        else
        {
            owner.atkColor = owner.atkLevel == 3 ? (tick0 / owner.whipColorTicks) % 4 : 0;
            a = owner.aWhip[owner.atkLevel, owner.atkColor, pose];
        }
        if (a != null)
            owner.Anim.SetFrame(a, owner.atkPhase);
    }

    private void End()
    {
        if (owner.atkPose >= eAttackPose.StairsUp)
        {
            if (owner.AtStairTop || owner.AtStairBottom)
                owner.ExitStair();
            else if (owner.AtStairStep)
                SetState(eSimonState.StairIdle);
            else
                SetState(owner.stairMoveDir > 0 ? eSimonState.StairUp : eSimonState.StairDown);
            return;
        }
        if (owner.atkAir && !owner.atkLanded)
        {
            if (owner.atkFalling)
            {
                int ft = owner.atkFallTicks;
                SetState(eSimonState.Fall);
                owner.fallTicks = ft;
            }
            else
            {
                // tiếp tục cú nhảy (giữ jumpTick) + co chân như video 2:14.6
                int jt = owner.jumpTick;
                SetState(eSimonState.Jump);
                owner.jumpTick = jt;
                owner.jumpLanded = false;
                owner.jumpTuckAfterAttack = true;
                owner.ApplyJumpPose();
            }
            return;
        }
        if (owner.atkPose == eAttackPose.Duck && owner.Held(eNesButton.Down))
            SetState(eSimonState.Crouch);
        else
            owner.ToGroundState();
    }
}
