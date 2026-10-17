using UnityEngine;

/// <summary>
/// CVM2 10 — đặt cơ chế địa hình động (bệ di động, bẫy ép gai — CVM2 06) vào luồng section qua lớp vật thể &lt;Section&gt;_Objects
/// (CVM2/Build Map2 Objects). Là EnemySpawner để StageWorld tự gom khi LoadSection (ResetSpawner) và gọi ở bước 8: tick đầu tiên
/// bộ sinh chạy sau khi nạp section ⇒ Map2Mechanisms.SpawnFromMarkers (vị trí = marker CVM2 05, biên/chu kỳ/pha = ConfigActorParam
/// Object.* CVM2 06) đúng 1 lần cho tới lần nạp/Warp kế. Không vùng, không prefab enemy (InZone bỏ qua).
/// Pha (startTick 06) đo từ frame ĐẦU section hiện; bước 8 bị dừng khi bộ sinh tạm dừng (cửa/đen) ⇒ tick sinh = tick đầu section chạy
/// — routine 11 căn với luồng thật (CVM2_MEMORY §6).
/// </summary>
public class MechSpawner : EnemySpawner
{
    public MovingPlatform platformPrefab;
    public Crusher crusherPrefab;

    private bool done;

    /// <summary>Số thực thể đã sinh lần gần nhất (harness đọc).</summary>
    public int MechCount { get; private set; }

    public override void ResetSpawner()
    {
        base.ResetSpawner();
        done = false;
        MechCount = 0;
    }

    protected override void OnTick(StageWorld w, int cx)
    {
        if (done)
            return;
        done = true;
        MechCount = Map2Mechanisms.SpawnFromMarkers(w, platformPrefab, crusherPrefab);
    }
}
