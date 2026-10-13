using UnityEngine;

/// <summary>
/// Nội dung vật chứa + sinh item (CVGP 11). Nội dung cố định lấy từ ConfigDrop (objectId — wiki/video, CV_WIKI_DATA §6), không có
/// dòng ⇒ mặc định của vật (nến tường: tim nhỏ — wiki Items "phần lớn các nến"; khối gạch: không có gì). Luật đổi nội dung (wiki):
///   • nến "vốn chứa tim nhỏ" ⇒ Morning Star khi roi &lt; 3 và (MorningStarNeedsHearts ⇒ số tim đang có ≥ ConfigWhip.upgradeHearts
///     của cấp kế — wiki "4 cho lần 1, 8 cho lần 2"; cách hiểu "tim đang có" là SUY LUẬN, CVGP_MEMORY §5);
///   • ngược lại, nếu vũ khí phụ hiện tại (không phải Stopwatch) đã trúng enemy ≥ MultiplierHitsToDrop lần ⇒ Double/Triple Shot
///     (wiki Items#Double Shot; áp vào ô "tim nhỏ" là SUY LUẬN) và đặt lại đếm.
/// Không có bảng rơi ngẫu nhiên (wiki/video không cho xác suất — không bịa).
/// </summary>
public static class ItemDrops
{
    /// <summary>Nội dung cố định theo objectId (ConfigDrop) hoặc fallback.</summary>
    public static eItemId ContentOf(StageWorld world, string objectId, eItemId fallback)
    {
        var cfg = world.Config;
        var row = cfg != null && cfg.drops != null ? cfg.drops.Find(objectId) : null;
        return row != null ? row.item : fallback;
    }

    /// <summary>Áp luật đổi nội dung lúc vật vỡ (đọc trạng thái ván tại thời điểm vỡ).</summary>
    public static eItemId Resolve(StageWorld world, eItemId content)
    {
        if (content != eItemId.SmallHeart)
            return content;
        var cfg = world.Config;
        var s = GameSession.Current;
        if (cfg == null)
            return content;
        if (s.whipLevel < 3)
        {
            bool need = cfg.rules.GetInt(eRuleParam.MorningStarNeedsHearts, 1) != 0;
            var next = cfg.GetWhip(s.whipLevel + 1);
            int hearts = next != null && next.upgradeHearts.has ? next.upgradeHearts.value : 0;
            if (!need || s.hearts >= hearts)
                return eItemId.MorningStar;
        }
        if (s.subWeapon != eSubWeapon.None && s.subWeapon != eSubWeapon.Stopwatch && s.shotMultiplier < 3
            && s.subWeaponHits >= cfg.rules.GetInt(eRuleParam.MultiplierHitsToDrop, 10))
        {
            s.subWeaponHits = 0;
            return s.shotMultiplier >= 2 ? eItemId.TripleShot : eItemId.DoubleShot;
        }
        return content;
    }

    /// <summary>Sinh item <paramref name="id"/> có pivot (đáy ô 16 px) tại (x, y). rise = trồi từ sau sàn (bí mật).</summary>
    public static ItemPickup Spawn(StageWorld world, eItemId id, int x, int y, bool rise = false)
    {
        if (id == eItemId.None || (int)id >= 100)
            return null;
        var cat = world.Items;
        if (cat == null || cat.itemPrefab == null)
        {
            Debug.LogError("[Item] StageWorld thiếu ItemCatalog/itemPrefab — chạy CVGP/Build Item Prefabs");
            return null;
        }
        var it = world.Spawn(cat.itemPrefab, x, y, -1) as ItemPickup;
        if (it != null)
            it.Setup(id, rise);
        return it;
    }
}
