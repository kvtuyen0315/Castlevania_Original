using UnityEngine;

/// <summary>
/// Phòng boss (CVGP 14 — dùng lại cho 19/20). Component con của section phòng boss (như EnemySpawner — StageWorld gom khi LoadSection,
/// gọi Tick ở bước 8, bỏ qua khi đóng băng). Việc:
///   • Sinh boss (prefab) MỘT lần khi Player vào vùng [zoneMinX, zoneMaxX] (mặc định cả section ⇒ ngay tick đầu: video Map_1 3:15 dơi đã
///     treo trần thấy từ hành lang trước khi camera khoá) tại bossPosition (px section, pivot boss). Simon chết ⇒ StageRules.RestartStage
///     ⇒ Warp ⇒ ResetSpawner ⇒ boss sinh lại đầy máu.
///   • Khoá camera: NesCameraController.autoBossLock (LevelSectionData.bossLockCx — PB 512, video 3:17.4) — routine 07.
///   • Tường vô hình mép màn khi camera đã khoá (screenWalls): TileCollision.WallMinX/MaxX = [cx, cx + 256) ⇒ Simon (thân 16) đứng
///     trong [cx + 8, cx + 248] như ở mép section. SUY LUẬN (NES không cho Simon ra khỏi màn — video không có cảnh đi ra mép).
///   • Nhạc boss: do boss gọi (BossBase.StartBossMusic) đúng mốc đo — Phantom Bat: khi hết lượn tại chỗ (video).
/// </summary>
public class BossArena : EnemySpawner
{
    [Tooltip("Vị trí sinh boss (px section, pivot). Phantom Bat: tâm khung bay khi treo trần (623, 41) — ConfigActorParam sleepX/sleepY ghi đè nếu useConfigPosition.")]
    public Vector2Int bossPosition = new Vector2Int(623, 41);
    [Tooltip("Đọc vị trí từ ConfigActorParam Boss.<id>.sleepX / sleepY (Phantom Bat).")]
    public bool useConfigPosition = true;
    public bool screenWalls = true;

    public BossBase Boss { get; private set; }
    public bool BossSpawned { get; private set; }
    public bool WallsOn { get; private set; }

    public override void ResetSpawner()
    {
        base.ResetSpawner();
        Boss = null;
        BossSpawned = false;
        WallsOn = false;
    }

    protected override void OnTick(StageWorld w, int cx)
    {
        if (!BossSpawned && InZone)
            SpawnBoss(w);
        if (screenWalls && !WallsOn && w.CameraCtl != null && w.CameraCtl.Locked && w.Tiles != null)
        {
            WallsOn = true;
            w.Tiles.WallMinX = cx;
            w.Tiles.WallMaxX = cx + NesCoord.SCREEN_W;
            // CVM3 04: Simon dừng ở x màn LockedScreenMaxX (239 — video Boss_Map_1 f368–379, Boss_Map_3 f690–713) ⇒ cột cx + 239 + 8 chặn thân
            int maxX = w.Config != null && w.Config.simon != null ? w.Config.simon.GetInt(eSimonParam.LockedScreenMaxX, 0) : 0;
            if (maxX > 0)
                w.Tiles.PlayerWallMaxX = cx + maxX + 8;
        }
    }

    /// <summary>Sinh boss ngay (OnTick gọi khi Player vào vùng; harness gọi trước tick đầu).</summary>
    public BossBase SpawnBoss(StageWorld w)
    {
        if (BossSpawned)
            return Boss;
        BossSpawned = true;
        var pos = bossPosition;
        var bp = prefab as BossBase;
        if (useConfigPosition && bp != null && w.Config != null && w.Config.GetParam(bp.BossId, "sleepX") != null)
            pos = new Vector2Int(BossParams.Int(w.Config, bp.BossId, "sleepX", pos.x), BossParams.Int(w.Config, bp.BossId, "sleepY", pos.y));
        Boss = SpawnAt(w, pos.x, pos.y, -1) as BossBase;
        if (Boss != null)
            Boss.arena = this;
        return Boss;
    }
}
