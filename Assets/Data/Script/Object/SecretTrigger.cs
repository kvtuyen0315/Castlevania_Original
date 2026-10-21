using System;
using UnityEngine;

/// <summary>Điều kiện kích hoạt kho báu bí mật (wiki Secrets — CV_WIKI_DATA §2.2, §6).</summary>
public enum eSecretCondition
{
    /// <summary>Pivot Simon đi vào vùng (kể cả đang nhảy) — Stage 1: nhảy VƯỢT qua cửa lâu đài.</summary>
    Enter,
    /// <summary>Simon NGỒI trong vùng ≥ holdTicks — Stage 2: ngồi trên khối thấp nhất (video 2:19.7–2:20.0).</summary>
    Crouch,
    /// <summary>Simon đứng yên (Idle) trong vùng ≥ holdTicks — Stage 5/6/14 "đứng yên vài giây" (Block 2+, thời lượng chờ đo).</summary>
    StandStill,
    /// <summary>CVM3 10: Simon giữ ← (trạng thái Walk, quay trái — kể cả bị tường chặn) trong vùng ≥ holdTicks — Stage 9 "đứng trên khối
    /// nằm trên khối thịt nướng, đi sang trái vài giây" (wiki Secrets; đẩy vào tường (81,7)).</summary>
    WalkLeft,
}

/// <summary>
/// Vùng kho báu bí mật (CVGP 11) — nhóm Prop, không vẽ, không va chạm. Mỗi tick (sau Player) xét điều kiện; đạt ⇒ sinh item tại
/// <see cref="spawnAt"/> (pivot item = đáy ô 16 px) kiểu TRỒI từ sau sàn (video 2:20.0–2:20.9: túi 1000 trồi 16 px) rồi tự gỡ (1 lần).
/// Routine 15 đặt vùng/điểm sinh theo video; khối phải phá trước (CB (29,6)) là BreakableBlock riêng.
/// </summary>
public class SecretTrigger : Entity
{
    [Tooltip("Vùng (px section) chứa pivot Simon.")]
    public RectInt area = new RectInt(0, 0, 16, 16);
    public eSecretCondition condition = eSecretCondition.Crouch;
    [Tooltip("Số tick phải giữ điều kiện; 0 ⇒ ConfigRule.SecretCrouchTicks (Crouch) / 1 (Enter). StandStill bắt buộc đặt (wiki chỉ nói 'vài giây').")]
    public int holdTicks;
    public eItemId item = eItemId.FlashingMoneyBag;
    [Tooltip("Pivot item (x tâm, y = mặt sàn chỗ item đứng).")]
    public Vector2Int spawnAt;
    public bool rise = true;
    [Tooltip("CVM2 10: tốc độ trồi px/tick (0 = ConfigRule.SecretRiseSpeed).")]
    public float riseSpeed;
    [Tooltip("CVM3 10: quãng trồi px (0 = ConfigRule.SecretRisePx).")]
    public int risePx;
    [Tooltip("CVM3 10: item không bị gỡ khi ra khỏi màn.")]
    public bool keepOffscreen;
    [Tooltip("CVM3 10: tuổi thọ item tính từ lúc trồi xong.")]
    public bool lifetimeFromRest;
    [Tooltip("CVM3 10: chỉ xét khi camera chưa khoá boss.")]
    public bool beforeBossLock;

    [NonSerialized] public int held;
    [NonSerialized] public bool fired;
    [NonSerialized] public ItemPickup spawnedItem;

    public override string DebugState => condition + " " + held + (fired ? " fired" : "");

    protected override void OnSpawned()
    {
        // CVGP 15: đặt trên map bằng SpawnPoint + SecretTriggerSpec (MAP1_PLACEMENT) ⇒ chép tham số của điểm sinh
        var spec = Spawner != null ? Spawner.GetComponent<SecretTriggerSpec>() : null;
        if (spec != null)
        {
            area = spec.area;
            condition = spec.condition;
            holdTicks = spec.holdTicks;
            item = spec.item;
            spawnAt = spec.spawnAt;
            rise = spec.rise;
            riseSpeed = spec.riseSpeed;
            risePx = spec.risePx;
            keepOffscreen = spec.keepOffscreen;
            lifetimeFromRest = spec.lifetimeFromRest;
            beforeBossLock = spec.beforeBossLock;
        }
        held = 0;
        fired = false;
        spawnedItem = null;
    }

    public override void OnTick()
    {
        if (fired)
            return;
        var p = World.Player as SimonController;
        // CVGP 15: Simon đang bị kịch bản điều khiển (tự đi vào cổng) không tính — chỉ người chơi tự nhảy vượt cổng mới lộ túi
        if (p == null || !p.Alive || p.IsDying || p.Fsm.Is(eSimonState.AutoWalk))
            return;
        bool inArea = area.Contains(new Vector2Int(p.X, p.Y));
        if (beforeBossLock && World.CameraCtl != null && World.CameraCtl.Locked)
            inArea = false;
        bool ok;
        switch (condition)
        {
            case eSecretCondition.Crouch: ok = inArea && p.Fsm.Is(eSimonState.Crouch); break;
            case eSecretCondition.StandStill: ok = inArea && p.Fsm.Is(eSimonState.Idle); break;
            case eSecretCondition.WalkLeft: ok = inArea && p.Fsm.Is(eSimonState.Walk) && p.Facing < 0; break;
            default: ok = inArea; break;
        }
        held = ok ? held + 1 : 0;
        int need = holdTicks > 0 ? holdTicks
                 : condition == eSecretCondition.Crouch && World.Config != null ? World.Config.rules.GetInt(eRuleParam.SecretCrouchTicks, 4) : 1;
        if (held < need)
            return;
        fired = true;
        spawnedItem = ItemDrops.Spawn(World, item, spawnAt.x, spawnAt.y, rise);
        if (spawnedItem != null && rise && riseSpeed > 0f)
            spawnedItem.OverrideRiseSpeed(Fixed.FromFloat(riseSpeed));
        if (spawnedItem != null)   // CVM3 10: tuỳ chọn Block 3 (mặc định tắt ⇒ Block 1/2 như cũ)
        {
            if (rise && risePx > 0)
                spawnedItem.OverrideRisePx(risePx);
            if (keepOffscreen)
                spawnedItem.KeepOffscreen();
            spawnedItem.lifetimeFromRest = lifetimeFromRest;
        }
        Kill();
    }

    public override void SyncView()
    {
        transform.localPosition = NesCoord.ToWorld(X, Y);
    }
}
