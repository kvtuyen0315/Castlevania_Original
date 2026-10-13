using UnityEngine;

/// <summary>
/// Bộ prefab + vật liệu của hệ item / vật chứa (CVGP 11) — asset Assets/Data/Config/ItemCatalog.asset, dựng bằng menu
/// CVGP/Build Item Prefabs. StageWorld giữ tham chiếu (field itemCatalog) ⇒ mọi nơi sinh item/FX qua World.Items
/// (nến, khối gạch, enemy chết — routine 13, boss — routine 14) mà không phải tự giữ prefab.
/// </summary>
[CreateAssetMenu(fileName = "ItemCatalog", menuName = "CVGP/ItemCatalog")]
public class ItemCatalog : ScriptableObject
{
    [Header("Prefab")]
    public ItemPickup itemPrefab;
    public FxSprite fxPrefab;
    public ScorePopup scorePrefab;
    public CandleProp candlePrefab;
    public CandleProp torchPrefab;
    public BreakableBlock blockPrefab;
    public SecretTrigger secretPrefab;

    [Header("Đổi màu")]
    [Tooltip("Vật liệu shader CVGP/NesPaletteSwap — gán tạm cho renderer khi đổi bảng màu (Simon nháy, lóe tàng hình).")]
    public Material paletteMaterial;
    [Tooltip("Vật liệu shader Hidden/CVGP/NesScreenFlash — Rosary.")]
    public Material flashMaterial;
    [Tooltip("Màu nền đen đổi sang khi Rosary nháy = xám sáng của tileset (video: trùng màu tường sáng).")]
    public Color32 flashColor = new Color32(188, 188, 188, 255);
    [Tooltip("Bảng màu Simon (SimonAnimSet.palettes: purple, blue, red, white).")]
    public AnimSet simonPalettes;

    /// <summary>Tên anim trong ItemAnimSet của từng item (CVGP 05 manifest).</summary>
    public static string AnimName(eItemId id)
    {
        switch (id)
        {
            case eItemId.SmallHeart: return "heart_small";
            case eItemId.LargeHeart: return "heart_large";
            case eItemId.RedMoneyBag: return "moneybag_red";
            case eItemId.PurpleMoneyBag: return "moneybag_purple";
            case eItemId.WhiteMoneyBag: return "moneybag_white";
            case eItemId.FlashingMoneyBag: return "moneybag_flash";
            case eItemId.Crown: return "crown";
            case eItemId.TreasureChest: return "chest";
            case eItemId.Moai: return "moai";
            case eItemId.MorningStar: return "whip";
            case eItemId.Dagger: return "dagger";
            case eItemId.Axe: return "axe";
            case eItemId.HolyWater: return "holywater";
            case eItemId.Cross: return "cross";
            case eItemId.Stopwatch: return "stopwatch";
            case eItemId.DoubleShot: return "double";
            case eItemId.TripleShot: return "triple";
            case eItemId.Rosary: return "rosary";
            case eItemId.InvisibilityPotion: return "jar";
            case eItemId.PotRoast: return "roast";
            case eItemId.OneUp: return "1up";
            case eItemId.MagicCrystal: return "crystal";
            default: return null;
        }
    }

    /// <summary>Vũ khí phụ ứng với item (None nếu không phải vũ khí phụ).</summary>
    public static eSubWeapon SubWeaponOf(eItemId id)
    {
        switch (id)
        {
            case eItemId.Dagger: return eSubWeapon.Dagger;
            case eItemId.Axe: return eSubWeapon.Axe;
            case eItemId.HolyWater: return eSubWeapon.HolyWater;
            case eItemId.Cross: return eSubWeapon.Cross;
            case eItemId.Stopwatch: return eSubWeapon.Stopwatch;
            default: return eSubWeapon.None;
        }
    }
}
