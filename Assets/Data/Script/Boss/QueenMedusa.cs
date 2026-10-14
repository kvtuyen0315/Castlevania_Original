using UnityEngine;

/// <summary>State Queen Medusa (boss Block 2).</summary>
public enum eMedusaState
{
    Statue,  // nằm trong tượng (ngủ — không va chạm)
    Emerge,  // vỡ ra khỏi tượng: đầu hiện, nháy đá ↔ sống
    Fly,     // bay sóng thấp lao về phía Simon + thả rắn
}

/// <summary>
/// Queen Medusa (CVGP 19, theo wiki — GẦN ĐÚNG, không có video Block 2). Wiki Bosses#Queen Medusa + Stages 4-6#Stage 6: "vỡ ra từ bức
/// tượng giữa phòng, không báo trước — đứng bên phải chờ nàng xuất hiện"; "cố va vào Simon", "hầu như luôn bay thấp sát đất" (Holy Water dễ
/// trúng); "khi bay thả rắn xuống sàn, rắn trườn về phía Simon, 1 đòn roi là chết"; Stopwatch đóng băng cả nàng lẫn rắn (boss cuối cùng
/// Stopwatch còn tác dụng); 12/8 đòn, 3000 đ. Ước lượng (ConfigActorParam Boss.QueenMedusa — SUY LUẬN, ghi §2p):
///   Statue  pivot = tâm ĐẦU; tượng (medusa_bust_0, đáy-giữa) đứng ở (statueX, chân tượng) — đầu cùng đỉnh tượng (statueHeadUpPx); hết
///           wakeDelayTicks tick kể từ lúc sinh (= vào phòng) ⇒ Emerge.
///   Emerge  tượng thành bust_1 (mất đầu), đầu hiện, Activate (nhận/gây đòn ngay — "không báo trước"), nháy medusa_stone ↔ medusa_head mỗi
///           emergeFlashTicks trong emergeTicks ⇒ nhạc boss ⇒ Fly.
///   Fly     ngang ±moveSpeed (ConfigBoss) về phía Simon; vượt Simon overshootPx hoặc chạm mép màn [screenMinX, screenMaxX] ⇒ quay về
///           phía Simon; dọc y = sàn − flyAboveFloorPx − A·sin(2πt/P) (tâm thấp: đáy sóng chạm chân, đỉnh sóng qua đầu Simon đứng).
///           Thả rắn: đầu snakeFirstTicks, rồi mỗi snakeMin..+span tick nếu số rắn sống &lt; maxSnakes ⇒ Snake rơi từ dưới đầu.
/// Khựng khi trúng / chạm nước thánh: BossBase (không có khung hurt ⇒ giữ khung). Chết: chuỗi boss (FX lửa tại tâm đầu, quả cầu); rắn
/// còn lại vẫn sống (wiki không nói — như lưỡi hái Death).
/// </summary>
public class QueenMedusa : BossBase
{
    [SerializeField] private Snake snakePrefab;
    [Tooltip("Renderer con vẽ bức tượng (đặt vị trí tuyệt đối mỗi SyncView).")]
    [SerializeField] private SpriteRenderer statueView;

    public StateMachine<QueenMedusa, eMedusaState> Fsm { get; private set; }

    private AnimSet.Anim aBust, aFly, aStone;
    [System.NonSerialized] public int statueX, floorY, headUp, wakeDelay, emergeTicks, emergeFlash;
    [System.NonSerialized] public int speed, flyAbove, amp, period, overshoot, scrMinX, scrMaxX;
    [System.NonSerialized] public int snakeFirst, snakeMin, snakeSpan, maxSnakes, snakeDrop;
    [System.NonSerialized] public int t, snakeTimer, dir;
    /// <summary>Đang hạ từ đầu tượng xuống tâm sóng (đầu Fly).</summary>
    [System.NonSerialized] public bool descending;
    private int[] wave = new int[0];
    /// <summary>Số rắn đã thả / số lần quay đầu (harness).</summary>
    [System.NonSerialized] public int snakes, turns;
    private readonly System.Collections.Generic.List<Snake> mySnakes = new System.Collections.Generic.List<Snake>();
    private readonly System.Collections.Generic.List<int> mySnakeTicks = new System.Collections.Generic.List<int>();

    public override string DebugState => Fsm != null && Fsm.HasState ? (dying ? "Dying" : Stunned ? "Stun" : Fsm.CurState.ToString()) + " t" + t : "-";

    abstract class MedusaState : BaseState<QueenMedusa>
    {
        protected void SetState(eMedusaState s) { owner.Fsm.SetState(s); }
    }

    class StatueState : MedusaState
    {
        public override void OnEnterState() { owner.hidden = true; }

        public override void CheckNextState()
        {
            if (owner.ageTicks >= owner.wakeDelay)
                SetState(eMedusaState.Emerge);
        }
    }

    class EmergeState : MedusaState
    {
        public override void OnEnterState()
        {
            owner.hidden = false;
            owner.Activate();
            owner.t = 0;
            owner.Flash(true);
        }

        public override void OnExecuteState()
        {
            owner.t++;
            owner.Flash((owner.t / Mathf.Max(1, owner.emergeFlash)) % 2 == 0);
        }

        public override void CheckNextState()
        {
            // đúng emergeTicks tick ở state Emerge (tick vào đã chạy Execute — như Wake của Phantom Bat)
            if (owner.t <= owner.emergeTicks)
                return;
            owner.StartBossMusic();
            SetState(eMedusaState.Fly);
        }
    }

    class FlyState : MedusaState
    {
        public override void OnEnterState()
        {
            owner.t = 0;
            owner.snakeTimer = owner.snakeFirst;
            owner.dir = owner.FacingToPlayer();
            owner.PlayAnim(owner.aFly);
        }

        public override void OnExecuteState() { owner.FlyTick(); }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<QueenMedusa, eMedusaState>(this, "QueenMedusa");
        Fsm.Add(eMedusaState.Statue, new StatueState());
        Fsm.Add(eMedusaState.Emerge, new EmergeState());
        Fsm.Add(eMedusaState.Fly, new FlyState());
        if (animSet != null)
        {
            aBust = animSet.Get("bust");
            aFly = animSet.Get("fly");
            aStone = animSet.Get("stone");
        }
    }

    /// <summary>Sinh tại CHÂN tượng (BossArena.bossPosition); đầu đặt ở đỉnh tượng.</summary>
    protected override void LoadBoss(GameConfig cfg)
    {
        statueX = X;
        floorY = Y;
        headUp = PInt(cfg, "statueHeadUpPx", 32);
        wakeDelay = PInt(cfg, "wakeDelayTicks", 60);
        emergeTicks = PInt(cfg, "emergeTicks", 32);
        emergeFlash = Mathf.Max(1, PInt(cfg, "emergeFlashTicks", 4));
        speed = BossMoveSpeed(cfg, 1f);
        flyAbove = PInt(cfg, "flyAboveFloorPx", 32);
        amp = PInt(cfg, "flyAmplitudePx", 16);
        period = Mathf.Max(2, PInt(cfg, "flyPeriodTicks", 96));
        overshoot = PInt(cfg, "overshootPx", 32);
        scrMinX = PInt(cfg, "screenMinX", 16);
        scrMaxX = PInt(cfg, "screenMaxX", 240);
        snakeFirst = PInt(cfg, "snakeFirstTicks", 48);
        snakeMin = PInt(cfg, "snakeMinTicks", 96);
        snakeSpan = PInt(cfg, "snakeSpanTicks", 64);
        maxSnakes = PInt(cfg, "maxSnakes", 2);
        snakeDrop = PInt(cfg, "snakeDropBelowPx", 8);
        fy = Fixed.FromPx(floorY - headUp);
        if (wave.Length != period)
            wave = new int[period];
        for (int i = 0; i < period; i++)
            wave[i] = -(int)System.Math.Round(amp * System.Math.Sin(2.0 * System.Math.PI * i / period) * Fixed.ONE);
        descending = true;
        t = snakeTimer = 0;
        snakes = turns = 0;
        dir = -1;
        mySnakes.Clear();
        mySnakeTicks.Clear();
        Fsm.SetState(eMedusaState.Statue);
    }

    protected override void TickBoss()
    {
        Fsm.Tick();
    }

    protected override void OnStunEnd()
    {
        if (Fsm.Is(eMedusaState.Fly))
            PlayAnim(aFly);
    }

    private void PlayAnim(AnimSet.Anim a)
    {
        if (Anim != null && a != null && !Anim.IsPlaying(a))
            Anim.Play(a, true);
    }

    private void Flash(bool stone)
    {
        PlayAnim(stone ? aStone : aFly);
    }

    private int CamX => World.CameraCtl != null ? World.CameraCtl.X : 0;

    private void FlyTick()
    {
        t++;
        var p = World.Player;
        int sx = X - CamX;
        // quay về phía Simon khi đã vượt qua Simon overshoot px, hoặc chạm mép màn theo chiều đang bay
        bool past = p != null && (dir > 0 ? X - p.X >= overshoot : p.X - X >= overshoot);
        bool edge = dir > 0 ? sx >= scrMaxX : sx <= scrMinX;
        if (past || edge)
        {
            int nd = p != null && p.X != X ? (p.X > X ? 1 : -1) : -dir;
            if (edge && nd == dir)
                nd = -dir;
            if (nd != dir)
            {
                dir = nd;
                turns++;
            }
        }
        fx += dir * speed;
        Facing = dir;
        // sóng dọc quanh tâm thấp (bảng tính 1 lần như Medusa Head 17 — tất định); đầu tượng ở cao hơn tâm ⇒ hạ thẳng xuống tâm trước
        int want = Fixed.FromPx(floorY - flyAbove) + wave[t % period];
        fy = descending ? Mathf.Min(want, fy + speed) : want;
        if (descending && fy >= want)
            descending = false;
        if (--snakeTimer <= 0)
        {
            if (SnakesAlive() < maxSnakes && snakePrefab != null)
                DropSnake();
            snakeTimer = Roll(snakeMin, snakeSpan);
        }
    }

    private int SnakesAlive()
    {
        int n = 0;
        for (int i = 0; i < mySnakes.Count; i++)
            if (mySnakes[i] != null && mySnakes[i].Alive && mySnakes[i].SpawnTick == mySnakeTicks[i])
                n++;
        return n;
    }

    private void DropSnake()
    {
        var p = World.Player;
        int f = p != null && p.X != X ? (p.X > X ? 1 : -1) : Facing;
        var sn = World.Spawn(snakePrefab, X, Y + snakeDrop, f) as Snake;
        if (sn == null)
            return;
        snakes++;
        for (int i = 0; i < mySnakes.Count; i++)
            if (mySnakes[i] == null || !mySnakes[i].Alive || mySnakes[i].SpawnTick != mySnakeTicks[i])
            {
                mySnakes[i] = sn;
                mySnakeTicks[i] = sn.SpawnTick;
                return;
            }
        mySnakes.Add(sn);
        mySnakeTicks.Add(sn.SpawnTick);
    }

    /// <summary>Vị trí tượng (harness / ảnh).</summary>
    public Vector2Int StatuePos => new Vector2Int(statueX, floorY);

    public override void SyncView()
    {
        base.SyncView();
        if (statueView == null)
            return;
        bool on = Alive && aBust != null && aBust.frames != null && aBust.frames.Length >= 2;
        statueView.enabled = on;
        if (!on)
            return;
        statueView.sprite = Fsm != null && Fsm.HasState && Fsm.Is(eMedusaState.Statue) ? aBust.frames[0] : aBust.frames[1];
        statueView.flipX = false;
        statueView.transform.localPosition = NesCoord.ToWorld(statueX - X, floorY - Y);
    }
}
