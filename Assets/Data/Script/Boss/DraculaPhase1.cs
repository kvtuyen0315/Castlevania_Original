using UnityEngine;

/// <summary>State Count Dracula dạng 1.</summary>
public enum eDraculaState
{
    Hidden,     // vô hình, không va chạm (giữa 2 lần hiện)
    Appear,     // hiện dần (nhấp nháy) ở chỗ mới, chưa va chạm
    Stand,      // áo khép — đầu nhận đòn
    Cape,       // mở áo choàng, bắn 3 cầu lửa
    Close,      // khép áo
    Vanish,     // biến mất dần (nhấp nháy), không va chạm
    Transform,  // hết máu: đầu bay ra, thân không đầu ⇒ biến thành quỷ (dạng 2)
}

/// <summary>
/// Count Dracula dạng 1 (CVGP 20, theo wiki — GẦN ĐÚNG, không có video Block 6). Wiki Bosses#The Count + Stages 16-18#Stage 18: "bắt đầu bằng
/// việc DỊCH CHUYỂN tới một chỗ ngẫu nhiên trong phòng, MỞ ÁO CHOÀNG rồi bắn 3 CẦU LỬA, sau đó BIẾN MẤT"; "CHỈ ĐẦU bị thương" — "đứng cách
/// đúng tầm roi khi ông ta hiện hình hoàn toàn rồi NHẢY quất vào mặt trước khi ông ta biến mất"; "nhảy quá sớm thì cầu lửa bắn CHẾCH LÊN"
/// (⇒ nhắm vào Simon lúc bắn); "khó đoán chỗ hiện, nhiều khả năng hiện NGAY TRÊN ĐẦU bạn nếu đứng yên"; 24/16 đòn (bảng Bosses, gán dạng
/// 1 — §4), không điểm riêng; Stopwatch vô dụng; "hết vạch máu cuối ⇒ ĐẦU BAY RA, thân biến thành quỷ". Ước lượng (ConfigActorParam
/// Boss.Dracula.p1* — ghi §2q):
///   Hidden  p1FirstDelayTicks (lần đầu) / p1HiddenMin..+Span ⇒ chọn chỗ: p1OnSimonPct % x = Simon, còn lại x ngẫu nhiên [cx + p1MinScreenX,
///           cx + p1MaxScreenX]; y = mặt sàn đầu tiên dưới cột x (phòng có bục). Quay về Simon.
///   Appear  p1AppearTicks nhấp nháy (hiện/ẩn mỗi p1FlickerTicks), không va chạm ("hiện hình hoàn toàn" mới đánh được).
///   Stand   p1StandTicks áo khép (dracula1_0) — đầu nhận đòn, thân chạm Simon gây sát thương.
///   Cape    áo mở (dracula1_1): tick p1FireDelayTicks bắn fireballsPerVolley cầu lửa từ ngực (chân − p1FireAbovePx) — quả giữa nhắm tâm thân
///           Simon (chân − p1AimAbovePx), 2 quả lệch ±fireSpreadDeg, fireballSpeed; giữ tới p1CapeTicks.
///   Close   p1CloseTicks áo khép ⇒ Vanish p1VanishTicks nhấp nháy ⇒ Hidden.
///   Transform (hết máu): 0 điểm, thanh ENEMY 0; ĐẦU (renderer con "Head") bay lên p1HeadFlyVy / lùi p1HeadFlyVx tới khi ra khỏi màn; thân
///           không đầu đứng đổi áo khép/mở mỗi p1HeadlessFlipTicks; sau p1TransformTicks: lửa bossdeath_m ở tâm thân + sinh DraculaPhase2
///           tại chỗ (cùng hướng) rồi gỡ dạng 1.
/// Hộp nhận đòn = ĐẦU (bbox dracula1_head trên khung có đầu); hộp chạm = thân áo khép. Sprite áo mở pivot thân x 12 (đầu không xê dịch).
/// </summary>
public class DraculaPhase1 : BossBase
{
    [SerializeField] private DraculaFireball fireballPrefab;
    [SerializeField] private DraculaPhase2 phase2Prefab;
    [Tooltip("Renderer con vẽ đầu bay ra lúc biến hình (sprite dracula1_head).")]
    [SerializeField] private SpriteRenderer headView;

    public StateMachine<DraculaPhase1, eDraculaState> Fsm { get; private set; }

    private AnimSet.Anim aIdle, aCape, aHeadless, aHead;
    [System.NonSerialized] public int firstDelay, hiddenMin, hiddenSpan, appearTicks, flicker, standTicks, fireDelay, capeTicks, closeTicks, vanishTicks;
    [System.NonSerialized] public int onSimonPct, minSX, maxSX, fireAbove, aimAbove, volley, fireSpeed, spreadCos, spreadSin;
    [System.NonSerialized] public int headVy, headVx, transformTicks, flipTicks;
    [System.NonSerialized] public int hiddenLen, headFx, headFy;
    [System.NonSerialized] public bool headFlying;
    /// <summary>Harness: số lần hiện, số lần hiện tại x Simon, số loạt bắn, số cầu lửa; x hiện lần tới bị ép (−1 = không).</summary>
    [System.NonSerialized] public int appears, appearsOnSimon, volleys, shots, debugForceX = -1;
    [System.NonSerialized] public DraculaPhase2 phase2;

    public override string DebugState => Fsm != null && Fsm.HasState ? (Stunned ? "Stun" : Fsm.CurState.ToString()) : "-";

    abstract class DracState : BaseState<DraculaPhase1>
    {
        protected void SetState(eDraculaState s) { owner.Fsm.SetState(s); }
    }

    class HiddenState : DracState
    {
        public override void OnEnterState()
        {
            owner.dormant = true;
            owner.hidden = true;
        }

        public override void CheckNextState()
        {
            if (StateFrame < owner.hiddenLen)
                return;
            owner.Teleport();
            SetState(eDraculaState.Appear);
        }
    }

    class AppearState : DracState
    {
        public override void OnEnterState()
        {
            owner.dormant = true;
            owner.Pose(owner.aIdle);
        }

        public override void OnExecuteState() { owner.hidden = (StateFrame / owner.flicker) % 2 == 1; }

        public override void CheckNextState()
        {
            if (StateFrame < owner.appearTicks)
                return;
            owner.hidden = false;
            if (owner.appears == 1)
                owner.StartBossMusic();
            owner.Activate();
            SetState(eDraculaState.Stand);
        }
    }

    class StandState : DracState
    {
        public override void OnEnterState() { owner.Pose(owner.aIdle); }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.standTicks)
                SetState(eDraculaState.Cape);
        }
    }

    class CapeState : DracState
    {
        public override void OnEnterState() { owner.Pose(owner.aCape); }

        public override void OnExecuteState()
        {
            if (StateFrame == owner.fireDelay)
                owner.Fire();
        }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.capeTicks)
                SetState(eDraculaState.Close);
        }
    }

    class CloseState : DracState
    {
        public override void OnEnterState() { owner.Pose(owner.aIdle); }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.closeTicks)
                SetState(eDraculaState.Vanish);
        }
    }

    class VanishState : DracState
    {
        public override void OnEnterState() { owner.dormant = true; }

        public override void OnExecuteState() { owner.hidden = (StateFrame / owner.flicker) % 2 == 0; }

        public override void CheckNextState()
        {
            if (StateFrame < owner.vanishTicks)
                return;
            owner.hiddenLen = owner.Roll(owner.hiddenMin, owner.hiddenSpan);
            SetState(eDraculaState.Hidden);
        }
    }

    class TransformState : DracState
    {
        public override void OnEnterState()
        {
            owner.dormant = true;
            owner.hidden = false;
            owner.headFlying = true;
            owner.headFx = owner.fx;
            owner.headFy = owner.fy - Fixed.FromPx(HeadAbovePx);
        }

        public override void OnExecuteState()
        {
            if (owner.Anim != null && owner.aHeadless != null)
                owner.Anim.SetFrame(owner.aHeadless, (StateFrame / owner.flipTicks) % 2);
            if (owner.headFlying)
            {
                owner.headFx -= owner.Facing * owner.headVx;   // lùi về sau lưng
                owner.headFy += owner.headVy;
                if (Fixed.ToPx(owner.headFy) < -16)
                    owner.headFlying = false;
            }
        }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.transformTicks)
                owner.BecomeDemon();
        }
    }

    /// <summary>Đầu: tâm sprite dracula1_head cao hơn chân (khung có đầu: đầu ở hàng 0–15 của 59 ⇒ tâm 59 − 8 = 51).</summary>
    private const int HeadAbovePx = 51;

    protected override void BuildStates()
    {
        Fsm = new StateMachine<DraculaPhase1, eDraculaState>(this, "DraculaPhase1");
        Fsm.Add(eDraculaState.Hidden, new HiddenState());
        Fsm.Add(eDraculaState.Appear, new AppearState());
        Fsm.Add(eDraculaState.Stand, new StandState());
        Fsm.Add(eDraculaState.Cape, new CapeState());
        Fsm.Add(eDraculaState.Close, new CloseState());
        Fsm.Add(eDraculaState.Vanish, new VanishState());
        Fsm.Add(eDraculaState.Transform, new TransformState());
        if (animSet != null)
        {
            aIdle = animSet.Get("p1_idle");
            aCape = animSet.Get("p1_cape_open");
            aHeadless = animSet.Get("p1_headless");
            aHead = animSet.Get("p1_head");
        }
    }

    protected override void LoadBoss(GameConfig cfg)
    {
        firstDelay = PInt(cfg, "p1FirstDelayTicks", 60);
        hiddenMin = PInt(cfg, "p1HiddenMinTicks", 48);
        hiddenSpan = PInt(cfg, "p1HiddenSpanTicks", 48);
        appearTicks = PInt(cfg, "p1AppearTicks", 32);
        flicker = Mathf.Max(1, PInt(cfg, "p1FlickerTicks", 2));
        standTicks = PInt(cfg, "p1StandTicks", 24);
        fireDelay = PInt(cfg, "p1FireDelayTicks", 8);
        capeTicks = PInt(cfg, "p1CapeTicks", 40);
        closeTicks = PInt(cfg, "p1CloseTicks", 16);
        vanishTicks = PInt(cfg, "p1VanishTicks", 24);
        onSimonPct = PInt(cfg, "p1OnSimonPct", 50);
        minSX = PInt(cfg, "p1MinScreenX", 32);
        maxSX = PInt(cfg, "p1MaxScreenX", 224);
        fireAbove = PInt(cfg, "p1FireAbovePx", 40);
        aimAbove = PInt(cfg, "aimAbovePx", 16);
        volley = PInt(cfg, "fireballsPerVolley", 3);
        fireSpeed = PFixed(cfg, "fireballSpeed", 1.5f);
        float deg = PInt(cfg, "fireSpreadDeg", 14);
        spreadCos = Mathf.RoundToInt(Mathf.Cos(deg * Mathf.Deg2Rad) * 256f);
        spreadSin = Mathf.RoundToInt(Mathf.Sin(deg * Mathf.Deg2Rad) * 256f);
        headVy = PFixed(cfg, "p1HeadFlyVy", -1.5f);
        headVx = PFixed(cfg, "p1HeadFlyVx", 0.5f);
        transformTicks = PInt(cfg, "p1TransformTicks", 96);
        flipTicks = Mathf.Max(1, PInt(cfg, "p1HeadlessFlipTicks", 8));
        hiddenLen = firstDelay;
        headFlying = false;
        appears = appearsOnSimon = volleys = shots = 0;
        debugForceX = -1;
        phase2 = null;
        Facing = -1;
        if (headView != null)
            headView.enabled = false;
        Fsm.SetState(eDraculaState.Hidden);
    }

    protected override void TickBoss()
    {
        Fsm.Tick();
    }

    private int CamX => World.CameraCtl != null ? World.CameraCtl.X : 0;

    /// <summary>Dịch chuyển: x = Simon (p1OnSimonPct %) hoặc ngẫu nhiên; y = mặt sàn dưới cột x; quay về Simon.</summary>
    private void Teleport()
    {
        appears++;
        var p = World.Player;
        int lo = CamX + minSX, hi = CamX + maxSX, x;
        bool onSimon = p != null && World.Rng.Next(100) < onSimonPct;
        if (debugForceX >= 0)
        {
            x = debugForceX;
            debugForceX = -1;
        }
        else if (onSimon)
        {
            x = Mathf.Clamp(p.X, lo, hi);
            appearsOnSimon++;
        }
        else
            x = Roll(lo, hi - lo);
        fx = Fixed.FromPx(x);
        fy = Fixed.FromPx(FloorBelow(x));
        if (p != null && p.X != x)
            Facing = p.X > x ? 1 : -1;
        else
            Facing = x < CamX + NesCoord.SCREEN_W / 2 ? 1 : -1;
    }

    /// <summary>Mặt sàn đầu tiên dưới cột x (quét từ trên xuống — phòng có bục).</summary>
    private int FloorBelow(int x)
    {
        var tiles = World.Tiles;
        if (tiles == null)
            return Y;
        for (int y = 16; y < tiles.Height; y++)
            if (tiles.RowLandable(y, x, x))
                return y;
        return Y;
    }

    private void Fire()
    {
        if (fireballPrefab == null)
            return;
        volleys++;
        var p = World.Player;
        int ox = X, oy = Y - fireAbove;
        long dx = p != null ? p.X - ox : Facing * 64, dy = p != null ? (p.Y - aimAbove) - oy : 0;
        long len = ISqrt((dx * dx + dy * dy) << 16);   // độ dài ×256 (căn nguyên — H7)
        if (len == 0)
        {
            dx = Facing;
            dy = 0;
            len = 256;
        }
        long cvx = fireSpeed * dx * 256 / len, cvy = fireSpeed * dy * 256 / len;
        for (int i = 0; i < volley; i++)
        {
            int k = i - volley / 2;   // −1, 0, +1 (3 quả): quay ±spread (cos/sin ×256 — nguyên)
            long vx1 = cvx, vy1 = cvy;
            for (int r = 0; r < Mathf.Abs(k); r++)
            {
                long s = k > 0 ? spreadSin : -spreadSin;
                long nx = (vx1 * spreadCos - vy1 * s) / 256, ny = (vx1 * s + vy1 * spreadCos) / 256;
                vx1 = nx;
                vy1 = ny;
            }
            var f = World.Spawn(fireballPrefab, ox, oy, vx1 >= 0 ? 1 : -1) as DraculaFireball;
            if (f == null)
                continue;
            f.vx = (int)vx1;
            f.vy = (int)vy1;
            shots++;
        }
    }

    public static long ISqrt(long v)
    {
        if (v <= 0)
            return 0;
        long r = (long)System.Math.Sqrt(v);
        while (r * r > v) r--;
        while ((r + 1) * (r + 1) <= v) r++;
        return r;
    }

    private void Pose(AnimSet.Anim a)
    {
        if (Anim != null && a != null)
            Anim.SetFrame(a, 0);
    }

    /// <summary>Hết máu dạng 1: KHÔNG chuỗi chết boss (không điểm, không quả cầu) — biến hình (wiki "đầu bay ra, thân biến thành quỷ").</summary>
    protected override void Die()
    {
        defeated = true;
        stunTicks = 0;
        SetEnemyBar(0);
        if (Score > 0)
            GameRules.AddScore(Score);   // ConfigBoss.Dracula.1 điểm trống (wiki không nêu điểm riêng dạng 1) ⇒ 0
        GameSession.Current.NotifyChanged();
        if (CvgpDebug.LogState)
            CvgpDebug.Log("[Boss] Dracula dạng 1 hết máu ⇒ biến hình (" + transformTicks + " tick)");
        Fsm.SetState(eDraculaState.Transform);
        RaiseDefeated();
    }

    private void BecomeDemon()
    {
        FxSprite.Spawn(World, "bossdeath_m", X, Y - 24, deathFxTicks);
        if (phase2Prefab != null)
        {
            phase2 = World.Spawn(phase2Prefab, X, Y, Facing) as DraculaPhase2;
            if (phase2 != null)
                phase2.arena = arena;
        }
        if (CvgpDebug.LogState)
            CvgpDebug.Log("[Boss] Dracula ⇒ dạng 2 ở (" + X + "," + Y + ")");
        Kill();
    }

    public override void SyncView()
    {
        base.SyncView();
        if (headView == null)
            return;
        bool show = headFlying && Alive;
        headView.enabled = show;
        if (!show)
            return;
        if (aHead != null && aHead.frames != null && aHead.frames.Length > 0)
            headView.sprite = aHead.frames[0];
        headView.flipX = Facing > 0;
        // renderer con: vị trí tuyệt đối đổi ra cục bộ (như tượng Medusa 19) — px nguyên
        headView.transform.localPosition = NesCoord.ToWorld(Fixed.ToPx(headFx) - X, Fixed.ToPx(headFy) - Y);
    }

    protected override void OnRemoved()
    {
        if (headView != null)
            headView.enabled = false;
        phase2 = null;
        base.OnRemoved();
    }
}
