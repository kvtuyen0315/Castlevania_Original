using UnityEngine;

/// <summary>
/// Điểm sinh thực thể theo khung camera (CVGP 07, CVGP_PLAN §5.6). Đặt làm con của prefab/instance section; toạ độ = px section
/// (`position`, pivot chân). StageWorld (bước 8) gọi UpdateSpawn mỗi tick:
///   Ready    : x điểm vào khung [cx − activateMargin, cx + 256 + activateMargin) ⇒ sinh (pool) ⇒ Alive
///   Alive    : thực thể ra khỏi [cx − despawnMargin, cx + 256 + despawnMargin) ⇒ Kill(Offscreen); thực thể bị gỡ ⇒ Consumed
///   Consumed : điểm ra khỏi khung kích hoạt ⇒ Ready (respawn) / Done (không hồi sinh)
/// ⇒ enemy bị hạ chỉ hồi sinh khi điểm sinh cuộn ra khỏi màn rồi vào lại (kiểu NES). Lề mặc định là QUY ƯỚC — luật thật từng
/// enemy (zombie sinh liên tục…) routine 13/15 đo video rồi đặt lề/tham số hoặc viết SpawnZone riêng.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    public enum eState
    {
        Ready,
        Alive,
        Consumed,
        Done,
    }

    /// <summary>Lề kích hoạt mặc định (px ngoài mép màn) — QUY ƯỚC routine 07, chờ đo routine 13/15.</summary>
    public const int DEFAULT_ACTIVATE_MARGIN = 16;
    /// <summary>Lề huỷ mặc định — QUY ƯỚC routine 07, chờ đo routine 13/15.</summary>
    public const int DEFAULT_DESPAWN_MARGIN = 32;

    public Entity prefab;
    [Tooltip("px section: x tâm, y mặt sàn dưới chân (pivot thực thể).")]
    public Vector2Int position;
    [Tooltip("−1 trái, +1 phải, 0 = quay về phía Player lúc sinh.")]
    public int facing = -1;
    [Tooltip("Tham số tự do cho loại thực thể (vd item rơi, biến thể).")]
    public int param;
    public bool respawn = true;
    public int activateMargin = DEFAULT_ACTIVATE_MARGIN;
    public int despawnMargin = DEFAULT_DESPAWN_MARGIN;

    public eState State { get; private set; }
    public Entity Live { get; private set; }
    public int SpawnCount { get; private set; }

    public void ResetSpawn()
    {
        State = eState.Ready;
        Live = null;
        SpawnCount = 0;
    }

    internal void UpdateSpawn(StageWorld world, int cx)
    {
        int x = position.x;
        bool inWindow = x >= cx - activateMargin && x < cx + NesCoord.SCREEN_W + activateMargin;
        switch (State)
        {
            case eState.Ready:
                if (inWindow && prefab != null)
                {
                    int f = facing != 0 ? facing : world.Player != null && world.Player.X > x ? 1 : -1;
                    Live = world.Spawn(prefab, x, position.y, f, this);
                    SpawnCount++;
                    State = eState.Alive;
                }
                break;
            case eState.Alive:
                if (Live != null && Live.Alive)
                {
                    int ex = Live.X;
                    if (ex < cx - despawnMargin || ex >= cx + NesCoord.SCREEN_W + despawnMargin)
                        Live.Kill(eKillReason.Offscreen);
                }
                break;
            case eState.Consumed:
                if (!inWindow)
                    State = respawn ? eState.Ready : eState.Done;
                break;
        }
    }

    /// <summary>StageWorld báo thực thể của điểm này đã bị gỡ.</summary>
    internal void NotifyRemoved(Entity e)
    {
        if (e != Live)
            return;
        Live = null;
        State = eState.Consumed;
    }
}
