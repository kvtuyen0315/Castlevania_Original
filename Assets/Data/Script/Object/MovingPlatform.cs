using UnityEngine;

/// <summary>
/// Bệ di động Map 2 (CVM2 06) — Entity nhóm Prop, KHÔNG Update: StageWorld gọi StepPlatform() ở bước (1b), TRƯỚC nhóm Player (H4/H7).
/// Chuyển động đo video Map_2 (CVM2_MEMORY §2i — 3 bệ, 5 lượt, mọi frame): CHỈ ngang, DAO ĐỘNG ĐIỀU HOÀ quanh tâm
///   x = tâm + biên·cos(2π·t / chu kỳ)   (khớp sin rms 0.37–0.56 px; gia tốc hằng 0.59–1.13; tốc độ đều 1.34–2.38 ⇒ loại)
/// t = tick từ lúc sinh (bệ đứng yên khi đóng băng thế giới; Stopwatch KHÔNG đóng băng — wiki không nói, video không có ⇒ CVM2_MEMORY §4).
/// Lúc nạp section bệ ở MÉP PHẢI, vận tốc 0 (pha 26–28 % ở 3/3 bệ ⇒ cos = 1) và đã chạy startTick tick khi section hiện (TEB 4, Hall B 6 —
/// bắt đầu trong màn đen chuyển tầng); chạy liên tục cả khi ra khỏi khung camera (pha 2 lượt Hall B liền nhau qua ~1800 tick). Pivot = (GIỮA bệ, MẶT TRÊN) px section; sprite là sprite NES ⇒ vẽ thấp 1 px (viewOffset (0,1) —
/// video: hàng đầu bệ NES y 113 khi mặt sàn hai bên 112).
/// Mặt trên = sàn MỘT CHIỀU cho thực thể RidesPlatforms (TileCollision.PlatformAt): đáp từ trên xuống, đi xuyên từ dưới lên (video 0:50.6
/// Simon nhảy từ khối dưới qua mép bệ rồi đáp lên). Người đứng trên (chân = Top, chồng ngang bệ CŨ) được dời đúng dx fixed bằng MoveX
/// (chặn tường) — video 0:48.9–0:49.5 / 2:12.0–2:13.0 Simon đứng yên trôi theo bệ; nhảy khỏi bệ KHÔNG giữ vận tốc bệ (video 0:49.5–0:49.8:
/// nhảy thẳng, x giữ 86 khi bệ trôi 10 px). Enemy không đứng bệ (video không có).
/// Pha = bộ tích luỹ uint 32 bit (1 vòng = 2^32) + bảng cos 1024 ô Q16 nội suy tuyến tính (dựng 1 lần) ⇒ tất định, không trôi biên độ.
/// </summary>
public class MovingPlatform : Entity
{
    private const int TABLE_BITS = 10;
    private static int[] cosQ16;

    [Tooltip("Bề rộng mặt bệ (px) — sprite Object/platform phần đục 32×8.")]
    [SerializeField] private int width = 32;

    private int centerFx, ampPx;
    private uint phase, step;

    public int Width => width;
    /// <summary>Hàng mặt trên (px section) — chân người đứng trên có Y = Top.</summary>
    public int Top => Y;
    public int Left => X - width / 2;
    /// <summary>Id marker làn bệ (LevelSectionData.markers) — khoá tham số ConfigActorParam Object.MovingPlatform.</summary>
    public string LaneId { get; private set; }
    public int CenterX => Fixed.ToPx(centerFx);
    public int Amplitude => ampPx;
    public uint Phase => phase;
    /// <summary>dx fixed của tick gần nhất (đã cộng cho người đứng trên nếu có).</summary>
    public int LastDx { get; private set; }
    /// <summary>Tick gần nhất có người được chở (debug/harness).</summary>
    public int LastCarryTick { get; private set; } = -1;

    public override bool IsFrozenBy(bool worldFrozen, bool enemyFrozen)
    {
        return worldFrozen;
    }

    protected override void OnSpawned()
    {
        LastDx = 0;
        LastCarryTick = -1;
        if (World != null)
            World.AddPlatform(this);
    }

    protected override void OnRemoved()
    {
        if (World != null)
            World.RemovePlatform(this);
    }

    /// <summary>Đặt quỹ đạo (sau Spawn): tâm pivot centerX, mặt trên top, biên ±amplitude px, chu kỳ periodTicks, đã chạy startTick tick
    /// kể từ mép phải (Config).</summary>
    public void Configure(string laneId, int centerX, int top, int amplitude, float periodTicks, int startTick)
    {
        LaneId = laneId;
        centerFx = Fixed.FromPx(centerX);
        ampPx = amplitude;
        step = periodTicks > 1f ? (uint)System.Math.Round(4294967296.0 / periodTicks) : 0u;
        phase = unchecked((uint)((ulong)step * (ulong)System.Math.Max(0, startTick)));
        fx = PositionFx(phase);
        fy = Fixed.FromPx(top);
        LastDx = 0;
    }

    /// <summary>Bước (1b): tiến pha 1 tick, dời người đứng trên đúng dx (fixed).</summary>
    internal void StepPlatform()
    {
        int oldFx = fx, oldLeft = Left, top = Top;
        phase += step;
        fx = PositionFx(phase);
        LastDx = fx - oldFx;
        if (LastDx == 0)
            return;
        var e = World.Player;
        if (e == null || !e.Alive || !e.RidesPlatforms || World.Tiles == null)
            return;
        var b = e.BodyRect;
        if (b.yMax != top || b.xMax - 1 < oldLeft || b.xMin >= oldLeft + width)
            return;
        World.Tiles.MoveX(e, LastDx);
        LastCarryTick = World.TickCount;
    }

    public override void OnTick()
    {
        // di chuyển ở StepPlatform (bước 1b); sprite tĩnh
    }

    private int PositionFx(uint ph)
    {
        if (cosQ16 == null)
            BuildTable();
        int i = (int)(ph >> (32 - TABLE_BITS));
        int frac = (int)((ph >> (32 - TABLE_BITS - 16)) & 0xFFFF);
        int a = cosQ16[i], c = cosQ16[(i + 1) & ((1 << TABLE_BITS) - 1)];
        long cq = a + (((long)(c - a) * frac) >> 16);                // cos Q16
        return centerFx + (int)(((long)ampPx * Fixed.ONE * cq) >> 16);  // fixed 1/256 px
    }

    private static void BuildTable()
    {
        int n = 1 << TABLE_BITS;
        var t = new int[n];
        for (int i = 0; i < n; i++)
            t[i] = (int)System.Math.Round(System.Math.Cos(2.0 * System.Math.PI * i / n) * 65536.0);
        cosQ16 = t;
    }
}
