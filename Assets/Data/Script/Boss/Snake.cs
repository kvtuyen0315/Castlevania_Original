using UnityEngine;

/// <summary>State rắn của Queen Medusa.</summary>
public enum eSnakeState
{
    Drop,    // rơi từ dưới đầu Medusa xuống sàn
    Crawl,   // trườn trên sàn về phía Simon
}

/// <summary>
/// Snake — rắn Queen Medusa thả (CVGP 19, theo wiki — GẦN ĐÚNG). Wiki Bosses#Queen Medusa: "thả rắn xuống sàn, rắn trườn về phía Simon; 1
/// đòn roi là chết"; Stopwatch đóng băng cả rắn (Stages 4-6). ConfigEnemy.Snake 1/1 đòn, điểm không rõ (wiki không nêu ⇒ 0). Ước lượng
/// (Enemy.Snake): rơi thẳng gravity (= item 0.25) tới mặt sàn, rồi trườn moveSpeed (ConfigEnemy) theo hướng về Simon LÚC CHẠM SÀN (không
/// đổi hướng — "trườn về phía Simon"), đụng tường ⇒ quay đầu, ra khỏi màn ⇒ gỡ. Khung snake_0/1 (sheet: 2 khung cùng nhóm rắn —
/// SUY LUẬN là khung trườn), pivot đáy-giữa. Enemy thường (Rosary diệt được).
/// </summary>
public class Snake : EnemyBase
{
    public StateMachine<Snake, eSnakeState> Fsm { get; private set; }
    [System.NonSerialized] public int gravity, speed;
    /// <summary>Tick (tuổi) lúc chạm sàn (harness).</summary>
    [System.NonSerialized] public int landAge;
    private AnimSet.Anim aSnake;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class SnakeState : BaseState<Snake>
    {
        protected void SetState(eSnakeState s) { owner.Fsm.SetState(s); }
    }

    class DropState : SnakeState
    {
        public override void OnExecuteState()
        {
            owner.vy += owner.gravity;
            if (owner.StepFall(owner.vy))
            {
                owner.vy = 0;
                owner.landAge = owner.ageTicks;
                owner.Facing = owner.FacingToPlayer();
                SetState(eSnakeState.Crawl);
            }
            else if (owner.World.Tiles != null && owner.Y > owner.World.Tiles.Height + 16)
                owner.Kill(eKillReason.Offscreen);
        }
    }

    class CrawlState : SnakeState
    {
        public override void OnExecuteState()
        {
            if (owner.StepX(owner.Facing * owner.speed))
                owner.Facing = -owner.Facing;
            if (!owner.Grounded())
                SetState(eSnakeState.Drop);
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Snake, eSnakeState>(this, "Snake");
        Fsm.Add(eSnakeState.Drop, new DropState());
        Fsm.Add(eSnakeState.Crawl, new CrawlState());
        if (animSet != null)
            aSnake = animSet.Get("snake");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        gravity = EnemyParams.Fixed256(cfg, enemyId, "gravity", 0.25f);
        speed = EnemyParams.MoveSpeed(cfg, enemyId, 0.5f);
        landAge = -1;
        vy = 0;
        if (Anim != null && aSnake != null)
            Anim.Play(aSnake, true);
        Fsm.SetState(eSnakeState.Drop);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
