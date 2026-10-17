using UnityEngine;

/// <summary>State Queen Medusa (boss Block 2).</summary>
public enum eMedusaState
{
    Statue,  // nằm trong tượng (ẩn — không va chạm); thức khi camera đã khoá phòng đủ wakeDelay tick
    Fly,     // bay 1 đoạn: ngang ±speed theo hướng chọn lúc bắt đầu đoạn, dọc sóng nhào (đi xuống trước) / nảy (sau khi va Simon)
    Hover,   // dừng tại chỗ hoverTicks (vẫn nháy màu) ⇒ thả rắn ⇒ đoạn bay mới về phía Simon
}

/// <summary>
/// Queen Medusa — CVM2 09: ĐO LẠI theo 2 video (Boss_Map_2 0–23 s, bảng màu khác ⇒ chỉ chuyển động/thời gian; Map_2 3:44–3:58 bảng màu
/// chuẩn) bằng Tools/cvm2/boss9_fit.py + boss9_model.py (CVM2_MEMORY §2k). Thay mô hình ước lượng CVGP 19 (tượng 60 tick → nháy 32 →
/// bay sóng 16/96 vượt Simon 32):
///   Statue  ẩn, không va chạm. Tượng: phòng THẬT (section có marker "statueHead" — CVM2 05) = tile NỀN (không vẽ gì); phòng thử Arena
///           = renderer con bust_0. Thức sau wakeDelayTicks tick kể từ khi camera KHOÁ phòng (video Map_2: khoá cx 0 f6735 → đầu tượng
///           thành đen f6825 = 180 tick).
///   Thức    (1 tick) xoá tile đầu tượng (LevelSection.ClearTileRect(marker statueHead) — thân tượng ở lại suốt trận) / renderer bust_1;
///           đầu hiện tại đỉnh tượng (chân tượng + (emergeDx, −statueHeadUpPx)), nhận/gây đòn NGAY, thả rắn, bắt đầu đoạn bay đầu
///           (firstMoveTicks) về phía Simon; nhạc boss sau musicDelayTicks (video boss: Poison Mind +0.95 s; Map_2 vẫn đúng mốc dù
///           Simon dùng đồng hồ ngay ⇒ bộ đếm nhạc không bị Stopwatch giữ).
///   Fly     ngang ±speed (ConfigBoss.moveSpeed 0.875 — 5 đoạn 0.80–0.98); dọc NHÀO y = y0 + P(1 − cos 2πτ/T)/2 (P 13, T 68 — rms
///           0.68–0.97 px 4 đoạn); sâu nhất = sàn − floorClearancePx (video: tâm đầu NES 168 = sàn 176 − 8). Va chạm Simon (đòn thân
///           được nhận) ⇒ QUAY ngang ngay (video f196) và sóng chuyển NẢY: nhào nốt tới đáy rồi y = yb − Pb(1 − cos 2πτ/Tb)/2 (Pb 18,
///           Tb 104 — 1 mẫu, rms 0.53). Chạm mép màn [screenMinX, screenMaxX] ⇒ quay (suy luận — video không có). Hết thời lượng đoạn
///           ⇒ Hover. Khựng (BossBase: trúng đòn / chạm nước thánh) dừng cả bộ đếm đoạn (video: đoạn C 164 tick ≈ 10 × 16 khi trừ khựng).
///   Hover   đứng yên hoverTicks (3 lần 62–63 tick) ⇒ thả rắn (3/3 lần đúng lúc rời chỗ) ⇒ đoạn mới: hướng về Simon, y0 = độ cao hiện
///           tại, thời lượng moveUnitTicks × (1 + ngẫu nhiên 0..moveUnitsMax−1) (video 64 / 160 / 16 — bội 16; phân bố suy luận).
/// Nháy màu: anim "fly" = medusa_head ↔ medusa_stone mỗi 9 tick (đo 3 đoạn: TB 9.1–9.5); khựng ⇒ "hurt" = medusa_stone giữ (video:
/// tím suốt lúc trong lửa); Stopwatch ⇒ đứng + anim dừng (BossBase — video Map_2 xanh suốt lúc bị đóng băng dù trúng roi).
/// Chết: chuỗi BossBase (lửa bossdeath_m 62 tick tại tâm đầu — video Map_2 f7069–7100; +3000; quả cầu sau 152 tick ở (cx+128, 104)
/// — video 150 / 154). Máu ConfigBoss 16/16 (video: roi cấp 3 = 16 nhát, lọ nước thánh 1 vạch — CVM2_MEMORY §2k).
/// </summary>
public class QueenMedusa : BossBase
{
    [SerializeField] private Snake snakePrefab;
    [Tooltip("Renderer con vẽ bức tượng ở phòng thử Arena (phòng thật: tượng là tile nền — marker statueHead).")]
    [SerializeField] private SpriteRenderer statueView;

    public StateMachine<QueenMedusa, eMedusaState> Fsm { get; private set; }

    private AnimSet.Anim aBust, aFly;
    [System.NonSerialized] public int statueX, floorY, emergeDx, headUp, wakeDelay, lockTicks, musicDelay, musicAt;
    [System.NonSerialized] public int speed, dipT, riseT, floorClear, scrMinX, scrMaxX;
    [System.NonSerialized] public int firstMove, moveUnit, moveUnits, hoverTicks, moveLeft, hoverLeft;
    [System.NonSerialized] public int snakeDrop, maxSnakes;
    [System.NonSerialized] public int tau, dir, y0, mode;
    /// <summary>Đã va Simon trong lúc nhào nửa đầu ⇒ chuyển sóng nảy khi tới đáy.</summary>
    [System.NonSerialized] public bool pendingRise;
    private int[] dip = new int[0], rise = new int[0];
    /// <summary>Tượng là tile nền của section (phòng thật) — vùng xoá khi thức.</summary>
    private LevelSection bgSection;
    private RectInt bgHead;
    [System.NonSerialized] public bool statueCleared;
    /// <summary>Harness: số rắn đã thả / lần quay (mép + va chạm) / lần va Simon / số đoạn bay.</summary>
    [System.NonSerialized] public int snakes, turns, bounces, segments;
    /// <summary>Harness (so video): thời lượng các đoạn bay sau đoạn đầu lấy lần lượt từ đây (rỗng ⇒ ngẫu nhiên Config).</summary>
    [System.NonSerialized] public System.Collections.Generic.Queue<int> forcedMoves = new System.Collections.Generic.Queue<int>();
    private readonly System.Collections.Generic.List<Snake> mySnakes = new System.Collections.Generic.List<Snake>();
    private readonly System.Collections.Generic.List<int> mySnakeTicks = new System.Collections.Generic.List<int>();

    public override string DebugState => Fsm != null && Fsm.HasState ? (dying ? "Dying" : Stunned ? "Stun" : Fsm.CurState.ToString()) + " t" + tau : "-";

    abstract class MedusaState : BaseState<QueenMedusa>
    {
        protected void SetState(eMedusaState s) { owner.Fsm.SetState(s); }
    }

    class StatueState : MedusaState
    {
        public override void OnEnterState() { owner.hidden = true; }

        public override void OnExecuteState()
        {
            // đếm từ khi camera khoá phòng (Arena thử: khoá ngay tick đầu); không có camera ⇒ đếm từ lúc sinh
            var cam = owner.World.CameraCtl;
            if (cam == null || cam.Locked)
                owner.lockTicks++;
        }

        public override void CheckNextState()
        {
            if (owner.lockTicks >= owner.wakeDelay)
            {
                owner.Wake();
                SetState(eMedusaState.Fly);
            }
        }
    }

    class FlyState : MedusaState
    {
        public override void OnExecuteState() { owner.FlyTick(); }

        public override void CheckNextState()
        {
            if (owner.moveLeft <= 0)
                SetState(eMedusaState.Hover);
        }
    }

    class HoverState : MedusaState
    {
        public override void OnEnterState() { owner.hoverLeft = owner.hoverTicks; }

        public override void OnExecuteState() { owner.hoverLeft--; }

        public override void CheckNextState()
        {
            if (owner.hoverLeft > 0)
                return;
            owner.DropSnake();
            owner.StartSegment(owner.NextMove());
            SetState(eMedusaState.Fly);
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<QueenMedusa, eMedusaState>(this, "QueenMedusa");
        Fsm.Add(eMedusaState.Statue, new StatueState());
        Fsm.Add(eMedusaState.Fly, new FlyState());
        Fsm.Add(eMedusaState.Hover, new HoverState());
        if (animSet != null)
        {
            aBust = animSet.Get("bust");
            aFly = animSet.Get("fly");
        }
    }

    /// <summary>Sinh tại CHÂN tượng (BossArena.bossPosition): phòng thật (112, 144) — marker statue [96,104,32,40]; phòng thử (128, 176).</summary>
    protected override void LoadBoss(GameConfig cfg)
    {
        statueX = X;
        floorY = Y;
        emergeDx = PInt(cfg, "emergeDx", 2);
        headUp = PInt(cfg, "statueHeadUpPx", 31);
        wakeDelay = PInt(cfg, "wakeDelayTicks", 180);
        musicDelay = PInt(cfg, "musicDelayTicks", 57);
        speed = BossMoveSpeed(cfg, 0.875f);
        dipT = Mathf.Max(2, PInt(cfg, "flyPeriodTicks", 68));
        riseT = Mathf.Max(2, PInt(cfg, "bouncePeriodTicks", 104));
        dip = Wave(dip, dipT, PFixed(cfg, "flyDipPx", 13f));
        rise = Wave(rise, riseT, PFixed(cfg, "bounceRisePx", 18f));
        floorClear = PInt(cfg, "floorClearancePx", 8);
        scrMinX = PInt(cfg, "screenMinX", 16);
        scrMaxX = PInt(cfg, "screenMaxX", 240);
        firstMove = PInt(cfg, "firstMoveTicks", 64);
        moveUnit = PInt(cfg, "moveUnitTicks", 16);
        moveUnits = Mathf.Max(1, PInt(cfg, "moveUnitsMax", 10));
        hoverTicks = PInt(cfg, "hoverTicks", 64);
        maxSnakes = PInt(cfg, "maxSnakes", 2);
        snakeDrop = PInt(cfg, "snakeDropBelowPx", 5);
        // tượng trên nền (phòng thật): marker statueHead của section đang chơi
        bgSection = null;
        var sec = World.Section;
        var m = sec != null ? MarkerOfKind(sec.data, "statueHead") : null;
        if (m != null)
        {
            bgSection = sec;
            bgHead = m.rect;
        }
        statueCleared = false;
        lockTicks = 0;
        musicAt = -1;
        tau = mode = 0;
        y0 = fy;
        moveLeft = hoverLeft = 0;
        pendingRise = false;
        dir = -1;
        snakes = turns = bounces = segments = 0;
        forcedMoves.Clear();
        mySnakes.Clear();
        mySnakeTicks.Clear();
        Fsm.SetState(eMedusaState.Statue);
    }

    /// <summary>Marker đầu tiên có kind (CVM2 05: id "statue_head", kind "statueHead").</summary>
    public static LevelSectionData.Marker MarkerOfKind(LevelSectionData d, string kind)
    {
        if (d != null && d.markers != null)
            foreach (var mk in d.markers)
                if (mk.kind == kind)
                    return mk;
        return null;
    }

    /// <summary>Bảng sóng (fixed): w[i] = P/2 · (1 − cos 2πi/T) — dựng 1 lần bằng Math.Cos rồi làm tròn (tất định).</summary>
    private static int[] Wave(int[] w, int period, int pFixed)
    {
        if (w.Length != period)
            w = new int[period];
        for (int i = 0; i < period; i++)
            w[i] = (int)System.Math.Round(pFixed * 0.5 * (1.0 - System.Math.Cos(2.0 * System.Math.PI * i / period)));
        return w;
    }

    protected override void TickBoss()
    {
        Fsm.Tick();
    }

    public override void OnTick()
    {
        // nhạc boss theo đồng hồ thế giới (Stopwatch không giữ — video Map_2 3:47.5 → ≈3:48.1 khi Medusa đang bị đóng băng)
        if (musicAt >= 0 && !dying && World.TickCount >= musicAt)
        {
            musicAt = -1;
            StartBossMusic();
        }
        base.OnTick();
    }

    protected override void OnStunEnd()
    {
        if (Anim != null && aFly != null && !Anim.IsPlaying(aFly))
            Anim.Play(aFly, true);
    }

    /// <summary>Thức: xoá đầu tượng, đầu hiện ở đỉnh tượng, va chạm ngay, thả rắn, đoạn bay đầu, hẹn nhạc boss.</summary>
    private void Wake()
    {
        hidden = false;
        fx = Fixed.FromPx(statueX + emergeDx);
        fy = Fixed.FromPx(floorY - headUp);
        if (bgSection != null)
            bgSection.ClearTileRect(bgHead.x, bgHead.y, bgHead.width, bgHead.height);
        statueCleared = true;
        Activate();
        musicAt = World.TickCount + musicDelay;
        if (Anim != null && aFly != null)
            Anim.Play(aFly, true);
        DropSnake();
        StartSegment(firstMove);
    }

    private int NextMove()
    {
        if (forcedMoves.Count > 0)
            return forcedMoves.Dequeue();
        return moveUnit * Roll(1, moveUnits - 1);
    }

    /// <summary>Đoạn bay mới: hướng về Simon, sóng nhào từ độ cao hiện tại.</summary>
    private void StartSegment(int ticks)
    {
        var p = World.Player;
        dir = p != null && p.X != X ? (p.X > X ? 1 : -1) : dir;
        Facing = dir;
        moveLeft = ticks;
        tau = 0;
        mode = 0;
        y0 = fy;
        pendingRise = false;
        segments++;
    }

    private int CamX => World.CameraCtl != null ? World.CameraCtl.X : 0;

    private void FlyTick()
    {
        tau++;
        moveLeft--;
        int sx = X - CamX;
        if (dir > 0 ? sx >= scrMaxX : sx <= scrMinX)
        {
            dir = -dir;
            turns++;
        }
        fx += dir * speed;
        Facing = dir;
        int y;
        if (mode == 0)
        {
            int ph = tau % dipT;
            y = y0 + dip[ph];
            if (pendingRise && ph >= dipT / 2)
            {
                // va Simon lúc đang nhào: tới đáy thì chuyển sóng nảy (đi lên trước)
                mode = 1;
                y0 = y;
                tau = 0;
                pendingRise = false;
            }
        }
        else
            y = y0 - rise[tau % riseT];
        int lowest = Fixed.FromPx(floorY - floorClear);
        fy = y > lowest ? lowest : y;
    }

    /// <summary>Đòn thân trúng Simon (Simon nhận đòn) ⇒ nảy: quay ngang, sóng nảy (video Boss_Map_2 f196: Simon trúng đòn #2 ⇒ Medusa
    /// đổi chiều 40 → 45 trong 1 frame, bay ngược 112 px, lên 139 rồi lại xuống 157).</summary>
    public override void OnHitLanded(ref HitInfo hit)
    {
        base.OnHitLanded(ref hit);
        if (hit.victim == null || hit.victim != World.Player || dying || !Fsm.Is(eMedusaState.Fly))
            return;
        dir = -dir;
        Facing = dir;
        turns++;
        bounces++;
        if (mode == 0 && tau % dipT < dipT / 2)
            pendingRise = true;
        else
        {
            mode = 1;
            y0 = fy;
            tau = 0;
            pendingRise = false;
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

    /// <summary>Thả rắn ngay dưới tâm đầu (video: x = x Medusa, y = tâm đầu + 5 — 4/4 con), rơi rồi trườn về phía Simon.</summary>
    private void DropSnake()
    {
        if (snakePrefab == null || SnakesAlive() >= maxSnakes)
            return;
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

    /// <summary>Vị trí chân tượng (harness / ảnh).</summary>
    public Vector2Int StatuePos => new Vector2Int(statueX, floorY);
    /// <summary>Tượng là tile nền (phòng thật) — harness.</summary>
    public bool BackgroundStatue => bgSection != null;

    /// <summary>Harness (so video theo đoạn): đặt Medusa đã thức tại (x, y) bắt đầu 1 đoạn bay ticks tick theo hướng d (sóng nhào).</summary>
    public void DebugStartFly(int x, int y, int d, int ticks)
    {
        if (Fsm.Is(eMedusaState.Statue))
        {
            hidden = false;
            statueCleared = true;
            if (bgSection != null)
                bgSection.ClearTileRect(bgHead.x, bgHead.y, bgHead.width, bgHead.height);
            Activate();
            if (Anim != null && aFly != null)
                Anim.Play(aFly, true);
        }
        fx = Fixed.FromPx(x);
        fy = Fixed.FromPx(y);
        StartSegment(ticks);
        dir = d;
        Facing = d;
        Fsm.SetState(eMedusaState.Fly);
    }

    public override void SyncView()
    {
        base.SyncView();
        if (statueView == null)
            return;
        bool on = Alive && bgSection == null && aBust != null && aBust.frames != null && aBust.frames.Length >= 2;
        statueView.enabled = on;
        if (!on)
            return;
        statueView.sprite = statueCleared ? aBust.frames[1] : aBust.frames[0];
        statueView.flipX = false;
        statueView.transform.localPosition = NesCoord.ToWorld(statueX - X, floorY - Y);
    }
}
