using System.Collections.Generic;
using UnityEngine;

/// <summary>State Death (Grim Reaper — boss Block 5).</summary>
public enum eDeathState
{
    Enter,  // hiện ở giữa-trên phòng, chưa va chạm (wakeDelayTicks)
    Move,   // bay thẳng tới điểm đích (trục trội moveSpeed)
    Hover,  // dừng ở đích, gọi lưỡi hái
}

/// <summary>
/// Death / Grim Reaper (CVGP 20, theo wiki — GẦN ĐÚNG, không có video Block 5). Wiki Bosses#Death + Stages 13-15#Stage 15: "bay lơ lửng NGẪU
/// NHIÊN quanh phòng, ƯU TIÊN CÁC GÓC"; "không tấn công trực tiếp — dựa vào lưỡi hái nó tạo ra, TỐI ĐA 3 cái"; "bản thân rất chậm";
/// "nấp ở góc nhưng đừng lâu vì Death có thể bay tới chỗ bạn"; "chạm nó khi nó bay sang phía kia thì bị đánh"; Holy Water ném lên BỤC
/// TRÊN-PHẢI giết nó "trước khi nó kịp vào vị trí"; 24/16 đòn, 7000 đ, Stopwatch vô dụng; chết rồi lưỡi hái vẫn còn. Ước lượng
/// (ConfigActorParam Boss.Death + ConfigBoss.moveSpeed — ghi §2q):
///   Enter  sinh ở bossPosition (giữa-trên), đứng wakeDelayTicks không va chạm ⇒ Activate + nhạc boss ⇒ Move tới góc firstCorner (trên-phải).
///   Move   thẳng tới đích, trục trội moveSpeed (0.5 — chậm hơn Phantom Bat 0.75 đo 14), mặt theo hướng bay.
///   Hover  đứng hoverMin..+span tick, quay về Simon; tick summonFirst rồi mỗi summonGap: còn &lt; maxScythes lưỡi hái sống ⇒ sinh 1 cái
///          lệch ngang ±scytheSpawnDx (luân phiên, phía Simon trước). Hết ⇒ đích mới: cornerPct % một GÓC khác góc hiện tại (4 góc
///          [cornerXMin|cornerXMax] × [cornerYHigh|cornerYLow] màn), còn lại 1 điểm ngẫu nhiên trong vùng.
/// Chết: chuỗi chết boss (7000 đ, lửa, quả cầu) — lưỡi hái KHÔNG bị gỡ (wiki).
/// </summary>
public class GrimReaper : BossBase
{
    [SerializeField] private DeathScythe scythePrefab;

    public StateMachine<GrimReaper, eDeathState> Fsm { get; private set; }

    private AnimSet.Anim aFly;
    [System.NonSerialized] public int wakeDelay, speed, cornerXMin, cornerXMax, cornerYHigh, cornerYLow, cornerPct, roamYMin, roamYMax, firstCorner;
    [System.NonSerialized] public int hoverMin, hoverSpan, summonFirst, summonGap, maxScythes, spawnDx, spawnDy;
    [System.NonSerialized] public int t, T, x0f, y0f, dxf, dyf, hoverLen, corner, sideToggle;
    [System.NonSerialized] public Vector2Int target;
    /// <summary>Harness: số lần dừng, số lần dừng ở góc, số lưỡi hái đã gọi.</summary>
    [System.NonSerialized] public int stops, cornerStops, summoned;

    private readonly List<DeathScythe> scythes = new List<DeathScythe>();
    private readonly List<int> scytheTicks = new List<int>();

    public override string DebugState => Fsm != null && Fsm.HasState ? (dying ? "Dying" : Stunned ? "Stun" : Fsm.CurState.ToString()) : "-";

    abstract class DeathStateBase : BaseState<GrimReaper>
    {
        protected void SetState(eDeathState s) { owner.Fsm.SetState(s); }
    }

    class EnterState : DeathStateBase
    {
        public override void OnEnterState() { owner.PlayFly(); }

        public override void CheckNextState()
        {
            if (owner.ageTicks < owner.wakeDelay)
                return;
            owner.Activate();
            owner.StartBossMusic();
            owner.BeginMove(owner.CornerPoint(owner.firstCorner), owner.firstCorner);
            SetState(eDeathState.Move);
        }
    }

    class MoveState : DeathStateBase
    {
        public override void OnExecuteState()
        {
            owner.t++;
            long tt = owner.t, TT = owner.T;
            owner.fx = owner.x0f + (int)(owner.dxf * tt / TT);
            owner.fy = owner.y0f + (int)(owner.dyf * tt / TT);
        }

        public override void CheckNextState()
        {
            if (owner.t >= owner.T)
                SetState(eDeathState.Hover);
        }
    }

    class HoverState : DeathStateBase
    {
        public override void OnEnterState()
        {
            owner.stops++;
            if (owner.corner >= 0)
                owner.cornerStops++;
            owner.hoverLen = owner.Roll(owner.hoverMin, owner.hoverSpan);
        }

        public override void OnExecuteState()
        {
            owner.Facing = owner.FacingToPlayer();
            int k = StateFrame - owner.summonFirst;
            if (k >= 0 && k % owner.summonGap == 0 && owner.AliveScythes() < owner.maxScythes)
                owner.Summon();
        }

        public override void CheckNextState()
        {
            if (StateFrame < owner.hoverLen)
                return;
            owner.PickTarget();
            SetState(eDeathState.Move);
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<GrimReaper, eDeathState>(this, "GrimReaper");
        Fsm.Add(eDeathState.Enter, new EnterState());
        Fsm.Add(eDeathState.Move, new MoveState());
        Fsm.Add(eDeathState.Hover, new HoverState());
        if (animSet != null)
            aFly = animSet.Get("fly");
    }

    protected override void LoadBoss(GameConfig cfg)
    {
        wakeDelay = PInt(cfg, "wakeDelayTicks", 32);
        speed = Mathf.Max(1, BossMoveSpeed(cfg, 0.5f));
        cornerXMin = PInt(cfg, "cornerXMinScreen", 40);
        cornerXMax = PInt(cfg, "cornerXMaxScreen", 216);
        cornerYHigh = PInt(cfg, "cornerYHigh", 88);
        cornerYLow = PInt(cfg, "cornerYLow", 152);
        cornerPct = PInt(cfg, "cornerPct", 75);
        roamYMin = PInt(cfg, "roamYMin", 64);
        roamYMax = PInt(cfg, "roamYMax", 152);
        firstCorner = PInt(cfg, "firstCorner", 1);
        hoverMin = PInt(cfg, "hoverMinTicks", 96);
        hoverSpan = PInt(cfg, "hoverSpanTicks", 64);
        summonFirst = PInt(cfg, "summonFirstTicks", 16);
        summonGap = Mathf.Max(1, PInt(cfg, "summonGapTicks", 32));
        maxScythes = PInt(cfg, "maxScythes", 3);
        spawnDx = PInt(cfg, "scytheSpawnDx", 24);
        spawnDy = PInt(cfg, "scytheSpawnDy", 0);
        t = T = 0;
        corner = -1;
        sideToggle = 0;
        stops = cornerStops = summoned = 0;
        target = new Vector2Int(X, Y);
        scythes.Clear();
        scytheTicks.Clear();
        Facing = -1;
        Fsm.SetState(eDeathState.Enter);
    }

    protected override void TickBoss()
    {
        Fsm.Tick();
    }

    protected override void OnStunEnd()
    {
        PlayFly();
    }

    private int CamX => World.CameraCtl != null ? World.CameraCtl.X : 0;

    /// <summary>Góc i (0 trên-trái, 1 trên-phải, 2 dưới-trái, 3 dưới-phải) — px section.</summary>
    public Vector2Int CornerPoint(int i)
    {
        return new Vector2Int(CamX + ((i & 1) == 0 ? cornerXMin : cornerXMax), (i & 2) == 0 ? cornerYHigh : cornerYLow);
    }

    private void PickTarget()
    {
        if (World.Rng.Next(100) < cornerPct)
        {
            int c = World.Rng.Next(3);   // 1 trong 3 góc còn lại (đang không ở góc ⇒ góc c của 0..2 — vẫn đều trên 3 góc)
            if (corner >= 0 && c >= corner)
                c++;
            BeginMove(CornerPoint(c), c);
        }
        else
            BeginMove(new Vector2Int(Roll(CamX + cornerXMin, cornerXMax - cornerXMin), Roll(roamYMin, roamYMax - roamYMin)), -1);
    }

    private void BeginMove(Vector2Int to, int cornerIndex)
    {
        target = to;
        corner = cornerIndex;
        t = 0;
        x0f = fx;
        y0f = fy;
        dxf = Fixed.FromPx(to.x) - fx;
        dyf = Fixed.FromPx(to.y) - fy;
        int n = Mathf.Max(Mathf.Abs(dxf), Mathf.Abs(dyf));
        T = Mathf.Max(1, (n + speed - 1) / speed);
        if (dxf != 0)
            Facing = dxf > 0 ? 1 : -1;
    }

    /// <summary>Số lưỡi hái do Death này gọi còn sống (thực thể tái dùng ⇒ so SpawnTick).</summary>
    public int AliveScythes()
    {
        int n = 0;
        for (int i = scythes.Count - 1; i >= 0; i--)
        {
            var s = scythes[i];
            if (s != null && s.Alive && s.SpawnTick == scytheTicks[i])
                n++;
            else
            {
                scythes.RemoveAt(i);
                scytheTicks.RemoveAt(i);
            }
        }
        return n;
    }

    private void Summon()
    {
        if (scythePrefab == null)
            return;
        var p = World.Player;
        int towards = p != null && p.X < X ? -1 : 1;
        int side = (sideToggle++ & 1) == 0 ? towards : -towards;
        var s = World.Spawn(scythePrefab, X + side * spawnDx, Y + spawnDy, side) as DeathScythe;
        if (s == null)
            return;
        summoned++;
        scythes.Add(s);
        scytheTicks.Add(s.SpawnTick);
    }

    private void PlayFly()
    {
        if (Anim != null && aFly != null && !Anim.IsPlaying(aFly))
            Anim.Play(aFly, true);
    }

    protected override void OnRemoved()
    {
        scythes.Clear();   // lưỡi hái còn lại sống tiếp (wiki) — chỉ bỏ tham chiếu
        scytheTicks.Clear();
        base.OnRemoved();
    }
}
