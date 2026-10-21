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
    [Tooltip("CVM2 10: tốc độ trồi (px/tick); 0 = ConfigRule.SecretRiseSpeed (Block 1). Block 2 đo video Map_2 0.25 (vương miện, rương).")]
    public float riseSpeed;
    [Tooltip("CVM3 10: quãng trồi (px); 0 = ConfigRule.SecretRisePx (16). Block 3 đo video Map_3 14 (túi St7/St8: hàng đầu hiện ~6 tick sau khi ngồi).")]
    public int risePx;
    [Tooltip("CVM3 10: item KHÔNG bị gỡ khi ra khỏi màn (wiki St8 'có thể hiện ngoài màn — quay lại sau 1–2 s'); vẫn hết hạn theo tuổi thọ.")]
    public bool keepOffscreen;
    [Tooltip("CVM3 10: tuổi thọ item (ConfigItem lifetimeTicks) tính từ lúc TRỒI XONG (video Map_3 St7 nhặt ≈ 197 tick sau khi sinh > 188).")]
    public bool lifetimeFromRest;
    [Tooltip("CVM3 10: chỉ xét khi camera CHƯA khoá boss (wiki St9 'before the screen gets locked').")]
    public bool beforeBossLock;
}
