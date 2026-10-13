using UnityEngine;

/// <summary>
/// Tham số kho báu bí mật gắn cạnh SpawnPoint của SecretTrigger (CVGP 15 — dựng bằng CVGP/Build Map1 Objects từ Map1Objects.json):
/// SecretTrigger.OnSpawned chép các trường này (prefab SecretTrigger dùng chung, pool tái dùng).
/// </summary>
public class SecretTriggerSpec : MonoBehaviour
{
    [Tooltip("Vùng (px section) chứa pivot Simon.")]
    public RectInt area = new RectInt(0, 0, 16, 16);
    public eSecretCondition condition = eSecretCondition.Crouch;
    public int holdTicks;
    public eItemId item = eItemId.FlashingMoneyBag;
    [Tooltip("Pivot item (x tâm, y = mặt sàn chỗ item đứng).")]
    public Vector2Int spawnAt;
    public bool rise = true;
}
