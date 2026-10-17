using UnityEngine;

/// <summary>State rắn của Queen Medusa.</summary>
public enum eSnakeState
{
    Drop,    // rơi từ dưới đầu Medusa xuống sàn
    Crawl,   // trườn trên sàn về phía Simon
}

/// <summary>
/// Snake — rắn Queen Medusa thả. CVM2 09: ĐO LẠI theo video Boss_Map_2 (4 con, Tools/cvm2/boss9_fit.py + boss9_model.py —
/// CVM2_MEMORY §2k), thay ước lượng CVGP 19:
///   Drop   sinh ở (x Medusa, tâm đầu + 5) lúc Medusa thức / rời chỗ dừng; rơi gravity 0.125 px/tick² từ đứng yên (khớp 4 con: g
///          0.10–0.135, rms 0.14–1.2 px) tới mặt sàn.
///   Crawl  quay về phía Simon LÚC CHẠM SÀN (4/4 con về phía Simon) rồi trườn thẳng moveSpeed 1.375 px/tick (4 con 1.376–1.396) —
///          KHÔNG bị tường chặn/quay đầu (video: bò qua cột cửa trái x 8–15 tới pivot x 8 rồi mất); ra mép màn (pivot ngoài
///          [cx + despawnInsetPx, cx + 256 − despawnInsetPx]) ⇒ gỡ; giữ hàng sàn lúc chạm (không rơi khỏi mép — suy luận).
/// Khung snake_0 ↔ snake_1 mỗi 9 tick (4 con TB 8.7–9.0), chạy từ lúc sinh. 1 đòn roi (wiki + video Map_2 3:48.2 roi cấp 3); +100 điểm
/// (video Map_2 3:48.20 — wiki không nêu); chạm Simon −2 vạch (Stage 6 — video Boss_Map_2 f352 PLAYER 6→4). Enemy thường (Rosary diệt).
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
            // không chặn tường (video f381–387: qua cột cửa trái), giữ hàng sàn lúc chạm (suy luận: video không có hố/bậc — phòng Medusa
            // bên phải x ≥ 224 là bậc cao y 128 trên tường gạch, lưới va chạm dưới bậc để trống ⇒ không cho rắn rơi vào khoảng trống đó)
            owner.fx += owner.Facing * owner.speed;
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
        gravity = EnemyParams.Fixed256(cfg, enemyId, "gravity", 0.125f);
        speed = EnemyParams.MoveSpeed(cfg, enemyId, 1.375f);
        despawnInset = EnemyParams.Int(cfg, enemyId, "despawnInsetPx", 8);
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
