using UnityEngine;

/// <summary>
/// Bẫy ép gai Map 2 — Hall C, 3 cái (CVM2 06). Entity nhóm Prop, KHÔNG Update. Trên NES bẫy là ô NỀN (hàng đầu NES y 112 đúng mặt treo,
/// không lệch +1 như sprite; dài ra từng nấc 8 px = 8 khung crusher_0..7 cắt từ sheet) ⇒ vẽ BackgroundFX, sau Simon.
/// Nhịp đo video Map_2 (CVM2_MEMORY §2i — mọi frame 2:34–2:42 + 3:05–3:16, 3 bẫy × ~6 chu kỳ):
///   hạ 1 nấc mỗi DownStepTicks (4) tick tới khung maxFrame → nấc kế tiếp (4 tick sau) ĐỔI CHIỀU lên 1 nấc → lên 1 nấc mỗi mốc lưới
///   GridTicks (16) tick → khung 0 giữ đúng 1 ô lưới (16 tick) → hạ lại. Chu kỳ = b1 + 16·(maxFrame − 1), b1 = mốc lưới đầu sau 4·maxFrame.
///   maxFrame 6 / 4 / 7 ⇒ chu kỳ 112 / 80 / 128 tick (bẫy 2 hạ ngắn nhất — đúng wiki "drops a much shorter distance").
/// u = tick trong chu kỳ (0 = bắt đầu hạ, ở mốc lưới); 3 bẫy cùng lưới 16 tick (đổi nấc CÙNG frame; video: lưới trùng nhịp giảm TIME).
/// Đồng hồ (Stopwatch) đóng băng chu kỳ (wiki Stages 4–6 + video 2:40.97: 3 bẫy đứng yên f4829–4922) nhưng gai VẪN chết người (wiki: dùng
/// đồng hồ khi gai ở trên rồi chạy qua ⇒ gai treo vẫn là chướng ngại). Không nhận đòn (ô nền — roi đi xuyên).
/// Chạm khối gai đáy (32 × BlockHeight px dưới cùng; dây xích không giết — suy luận) ⇒ Simon chết ngay (wiki Gameplay: "hitting spikes will
/// instantly kill you"; video Map_2 Simon không chạm lần nào) — bỏ qua bất tử nháy/tàng hình như nước/hố, cheat Invincible miễn.
/// </summary>
public class Crusher : Entity
{
    public const int FRAME_COUNT = 8;
    public const int STEP_PX = 8;

    [Tooltip("crusher_0..7 (32 × 8(k+1), pivot giữa-trên = điểm treo).")]
    [SerializeField] private Sprite[] frames = new Sprite[FRAME_COUNT];
    [Tooltip("Khối gai đáy (px): 3 hàng thanh + 1 trống + 4 hàng gai của sheet.")]
    [SerializeField] private int blockHeight = 8;
    [SerializeField] private int width = 32;

    private int maxFrame = 7, downStep = 4, grid = 16, b1 = 32, cycle = 128, u;

    public string MarkerId { get; private set; }
    /// <summary>Khung đang hiện 0..maxFrame (= số nấc đã hạ).</summary>
    public int Frame { get; private set; }
    public int MaxFrame => maxFrame;
    public int CycleTicks => cycle;
    /// <summary>Tick trong chu kỳ (0 = bắt đầu hạ).</summary>
    public int CycleTick => u;
    /// <summary>Số lần đã giết Simon (harness).</summary>
    public int Kills { get; private set; }
    /// <summary>Hộp gai chết người (px section) của khung hiện tại.</summary>
    public RectInt DeadlyRect => new RectInt(X - width / 2, Y + STEP_PX * Frame, width, blockHeight);

    public override bool IsFrozenBy(bool worldFrozen, bool enemyFrozen)
    {
        return worldFrozen;   // Stopwatch: chỉ dừng chu kỳ trong OnTick — gai vẫn kiểm chạm
    }

    protected override void OnSpawned()
    {
        Kills = 0;
        Frame = 0;
        u = 0;
    }

    /// <summary>Đặt bẫy (sau Spawn): treo tại (centerX, top), maxFrame nấc, nhịp hạ/lưới (tick), u lúc sinh (tick trong chu kỳ).</summary>
    public void Configure(string markerId, int centerX, int top, int maxFrameK, int downStepTicks, int gridTicks, int startTick)
    {
        MarkerId = markerId;
        maxFrame = Mathf.Clamp(maxFrameK, 1, FRAME_COUNT - 1);
        downStep = Mathf.Max(1, downStepTicks);
        grid = Mathf.Max(1, gridTicks);
        b1 = grid * (downStep * maxFrame / grid + 1);
        cycle = b1 + grid * (maxFrame - 1);
        u = ((startTick % cycle) + cycle) % cycle;
        Frame = FrameAt(u);
        SetPositionPx(centerX, top);
    }

    /// <summary>Khung ở tick u của chu kỳ (công thức đo video — xem tóm tắt lớp).</summary>
    public int FrameAt(int t)
    {
        if (t < downStep * maxFrame)
            return 1 + t / downStep;
        if (t < b1)
            return maxFrame - 1;
        return Mathf.Max(0, maxFrame - 2 - (t - b1) / grid);
    }

    public override void OnTick()
    {
        if (!World.IsEnemyFrozen)
        {
            u = (u + 1) % cycle;
            Frame = FrameAt(u);
        }
        var s = World.Player as SimonController;
        if (s == null || !s.Alive || s.IsDying || !s.Hurt.active || CvgpDebug.CheatInvincible)
            return;
        if (CombatResolver.Overlaps(DeadlyRect, s.Hurt.SectionRect))
        {
            Kills++;
            s.KillByHazard();
        }
    }

    public override void SyncView()
    {
        transform.localPosition = NesCoord.ToWorld(X + viewOffset.x, Y + viewOffset.y);
        if (view != null && frames != null && Frame < frames.Length)
            view.sprite = frames[Frame];
    }
}
