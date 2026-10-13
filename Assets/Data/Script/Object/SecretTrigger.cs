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

    [NonSerialized] public int held;
    [NonSerialized] public bool fired;
    [NonSerialized] public ItemPickup spawnedItem;

    public override string DebugState => condition + " " + held + (fired ? " fired" : "");

    protected override void OnSpawned()
    {
        held = 0;
        fired = false;
        spawnedItem = null;
    }

    public override void OnTick()
    {
        if (fired)
            return;
        var p = World.Player as SimonController;
        if (p == null || !p.Alive || p.IsDying)
            return;
        bool inArea = area.Contains(new Vector2Int(p.X, p.Y));
        bool ok;
        switch (condition)
        {
            case eSecretCondition.Crouch: ok = inArea && p.Fsm.Is(eSimonState.Crouch); break;
            case eSecretCondition.StandStill: ok = inArea && p.Fsm.Is(eSimonState.Idle); break;
            default: ok = inArea; break;
        }
        held = ok ? held + 1 : 0;
        int need = holdTicks > 0 ? holdTicks
                 : condition == eSecretCondition.Crouch && World.Config != null ? World.Config.rules.GetInt(eRuleParam.SecretCrouchTicks, 4) : 1;
        if (held < need)
            return;
        fired = true;
        spawnedItem = ItemDrops.Spawn(World, item, spawnAt.x, spawnAt.y, rise);
        Kill();
    }

    public override void SyncView()
    {
        transform.localPosition = NesCoord.ToWorld(X, Y);
    }
}
