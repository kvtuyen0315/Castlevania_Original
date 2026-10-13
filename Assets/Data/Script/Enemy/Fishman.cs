using UnityEngine;

/// <summary>State Fishman (Fish Man).</summary>
public enum eFishmanState
{
    Rise,    // nhảy lên khỏi nước (thẳng đứng, xuyên lối đi từ dưới)
    Fall,    // rơi (sau đỉnh / bước khỏi mép)
    Stand,   // vừa đáp
    Walk,
    Shoot,
    Sink,    // rơi lại xuống nước: ẩn rồi gỡ
}

/// <summary>
/// Fishman — "Fish Man" trên sheet (CVGP 13). Đo video Map_1 Crypt B (§2k, 12 lần nhảy): trồi ở cột x = 32 + 64k (WaterSpawner),
/// nhảy THẲNG ĐỨNG từ dưới nước vy0 −5.875, g 0.125 (đỉnh chân Y ≈ 54 cố định), xuyên lối đi từ dưới lên rồi đáp khi rơi; ẩn khi chân
/// còn dưới mặt nước ≥ 16 px; lúc trồi: 3 bọt nước tím (FX water_bubble) + SFX FishJump. Đáp: đứng 8 tick (khung fishman_0 suốt cú
/// nhảy) rồi đi 0.75 px/tick về phía Simon (khung 0/1 mỗi 32 tick kể từ lúc đáp). Bước khỏi mép / rơi trượt lối ⇒ chạm mặt nước:
/// bọt + SFX FishSplash, ẩn, gỡ (0 điểm). Bắn cầu lửa (wiki Enemies#Fishmen — video KHÔNG có cảnh bắn): QUY ƯỚC firstShotTicks /
/// shotEveryTicks / shootPoseTicks (tư thế fishman_2), cầu lửa FishmanFireball bay ngang theo hướng nhìn.
/// </summary>
public class Fishman : EnemyBase
{
    [SerializeField] private FishmanFireball fireballPrefab;

    public StateMachine<Fishman, eFishmanState> Fsm { get; private set; }
    [System.NonSerialized] public int surfaceY, jumpVy0, gravity, walkVx, standTicks, hiddenBelow, groundTicks, nextShot;
    [System.NonSerialized] public int firstShot, shotEvery, shootPose, fireballVx, fireballHeight;
    [System.NonSerialized] public int bubbleSideVx, bubbleCenterVx, bubbleCenterVy, bubbleGravity, bubbleSideTicks, bubbleCenterTicks;
    /// <summary>Tuổi lúc đáp / số phát bắn (harness).</summary>
    [System.NonSerialized] public int landAge, shots;
    private AnimSet.Anim aWalk, aShoot;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class FishState : BaseState<Fishman>
    {
        protected void SetState(eFishmanState s) { owner.Fsm.SetState(s); }
    }

    class RiseState : FishState
    {
        public override void OnEnterState()
        {
            owner.PoseStill();
        }

        public override void OnExecuteState()
        {
            owner.vy += owner.gravity;
            owner.StepFall(owner.vy);
            owner.hidden = owner.Y >= owner.surfaceY + owner.hiddenBelow;
        }

        public override void CheckNextState()
        {
            if (owner.vy >= 0)
                SetState(eFishmanState.Fall);
        }
    }

    class FallState : FishState
    {
        public override void OnEnterState()
        {
            owner.PoseStill();
        }

        public override void OnExecuteState()
        {
            owner.vy += owner.gravity;
            if (owner.StepFall(owner.vy))
            {
                owner.vy = 0;
                owner.landAge = owner.ageTicks;
                owner.Facing = owner.FacingToPlayer();
                SetState(eFishmanState.Stand);
                return;
            }
            owner.hidden = owner.Y >= owner.surfaceY + owner.hiddenBelow;
            if (owner.Y >= owner.surfaceY && owner.vy > 0 && owner.World.Tiles.TypeAt(owner.X, owner.surfaceY) == eTileCollision.Water)
                SetState(eFishmanState.Sink);
        }
    }

    class StandState : FishState
    {
        public override void OnEnterState()
        {
            owner.groundTicks = 0;
            owner.nextShot = owner.firstShot;
            if (owner.Anim != null && owner.aWalk != null)
                owner.Anim.Play(owner.aWalk, true);   // khung 0/1 đếm từ lúc đáp (video f3550→3566)
        }

        public override void OnExecuteState()
        {
            owner.groundTicks++;
        }

        public override void CheckNextState()
        {
            if (StateFrame > owner.standTicks)   // tick đáp + standTicks tick đứng ⇒ bước đầu ở tick thứ standTicks sau khi đáp
                SetState(eFishmanState.Walk);
        }
    }

    class WalkState : FishState
    {
        public override void OnEnterState()
        {
            if (owner.Anim != null && owner.aWalk != null)
                owner.Anim.Play(owner.aWalk);
        }

        public override void OnExecuteState()
        {
            owner.groundTicks++;
            if (owner.StepX(owner.Facing * owner.walkVx))
                owner.Facing = -owner.Facing;
        }

        public override void CheckNextState()
        {
            if (!owner.Grounded())
                SetState(eFishmanState.Fall);
            else if (owner.groundTicks >= owner.nextShot)
                SetState(eFishmanState.Shoot);
        }
    }

    class ShootState : FishState
    {
        public override void OnEnterState()
        {
            owner.Facing = owner.FacingToPlayer();
            if (owner.Anim != null && owner.aShoot != null)
                owner.Anim.Play(owner.aShoot, true);
            owner.FireBall();
        }

        public override void OnExecuteState()
        {
            owner.groundTicks++;
        }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.shootPose)
            {
                owner.nextShot = owner.groundTicks + owner.shotEvery;
                SetState(eFishmanState.Walk);
            }
        }
    }

    class SinkState : FishState
    {
        public override void OnEnterState()
        {
            owner.hidden = true;
            owner.Splash(false);
        }

        public override void CheckNextState()
        {
            owner.Kill(eKillReason.Killed);
        }
    }

    private void PoseStill()
    {
        if (Anim != null && aWalk != null)
            Anim.SetFrame(aWalk, 0);   // video: fishman_0 suốt cú nhảy (8/8 vết)
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Fishman, eFishmanState>(this, "Fishman");
        Fsm.Add(eFishmanState.Rise, new RiseState());
        Fsm.Add(eFishmanState.Fall, new FallState());
        Fsm.Add(eFishmanState.Stand, new StandState());
        Fsm.Add(eFishmanState.Walk, new WalkState());
        Fsm.Add(eFishmanState.Shoot, new ShootState());
        Fsm.Add(eFishmanState.Sink, new SinkState());
        if (animSet != null)
        {
            aWalk = animSet.Get("walk");
            aShoot = animSet.Get("shoot");
        }
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        walkVx = EnemyParams.MoveSpeed(cfg, id, 0.75f);
        jumpVy0 = EnemyParams.Fixed256(cfg, id, "jumpVy0", -5.875f);
        gravity = EnemyParams.Fixed256(cfg, id, "gravity", 0.125f);
        standTicks = EnemyParams.Int(cfg, id, "standTicks", 8);
        hiddenBelow = EnemyParams.Int(cfg, id, "hiddenBelowSurfacePx", 16);
        firstShot = EnemyParams.Int(cfg, id, "firstShotTicks", 96);
        shotEvery = EnemyParams.Int(cfg, id, "shotEveryTicks", 160);
        shootPose = EnemyParams.Int(cfg, id, "shootPoseTicks", 24);
        fireballVx = EnemyParams.Fixed256(cfg, id, "fireballSpeed", 2f);
        fireballHeight = EnemyParams.Int(cfg, id, "fireballHeightAboveFeetPx", 22);
        bubbleSideVx = EnemyParams.Fixed256(cfg, id, "bubbleSideVx", 0.375f);
        bubbleCenterVx = EnemyParams.Fixed256(cfg, id, "bubbleCenterVx", 0.25f);
        bubbleCenterVy = EnemyParams.Fixed256(cfg, id, "bubbleCenterVy", -0.5f);
        bubbleGravity = EnemyParams.Fixed256(cfg, id, "bubbleGravity", 0.125f);
        bubbleSideTicks = EnemyParams.Int(cfg, id, "bubbleSideTicks", 24);
        bubbleCenterTicks = EnemyParams.Int(cfg, id, "bubbleCenterTicks", 34);
        surfaceY = World.Tiles != null ? World.Tiles.Height : Y;
        landAge = -1;
        shots = 0;
        groundTicks = 0;
        nextShot = firstShot;
        vy = 0;
        Fsm.SetState(Grounded() ? eFishmanState.Stand : eFishmanState.Fall);
    }

    /// <summary>WaterSpawner gọi ngay sau Spawn (pivot ở mặt nước + spawnBelowSurfacePx): trồi lên — ẩn, bọt + SFX FishJump.</summary>
    public void Emerge(int waterSurfaceY)
    {
        surfaceY = waterSurfaceY;
        vy = jumpVy0;
        hidden = Y >= surfaceY + hiddenBelow;
        Fsm.SetState(eFishmanState.Rise);
        Splash(true);
        ApplyVisibility();
    }

    /// <summary>3 bọt nước tím tại mặt nước (video f3515–3539 — xấp xỉ) + SFX FishJump (trồi) / FishSplash (rơi lại).</summary>
    private void Splash(bool jump)
    {
        int x = X;
        FxSprite.Spawn(World, "water_bubble", x - 3, surfaceY - 12, bubbleSideTicks, -bubbleSideVx, 0, bubbleGravity);
        FxSprite.Spawn(World, "water_bubble", x + 3, surfaceY - 12, bubbleSideTicks, bubbleSideVx, 0, bubbleGravity);
        FxSprite.Spawn(World, "water_bubble", x + 2, surfaceY - 27, bubbleCenterTicks, bubbleCenterVx, bubbleCenterVy, bubbleGravity);
        if (AudioManager.I != null)
            AudioManager.I.PlaySfx(jump ? eSfx.FishJump : eSfx.FishSplash);
    }

    private void FireBall()
    {
        shots++;
        if (fireballPrefab == null)
            return;
        var f = World.Spawn(fireballPrefab, X + Facing * 8, Y - fireballHeight, Facing) as FishmanFireball;
        if (f != null)
            f.vx = Facing * fireballVx;
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
