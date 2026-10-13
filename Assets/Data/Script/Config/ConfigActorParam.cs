/// <summary>Tham số hành vi riêng (ConfigActorParam.csv): actor = "Enemy.Bat", "Boss.PhantomBat", "SubWeapon.Axe", "Item.MagicCrystal".</summary>
public class ConfigActorParam : ConfigTable<ConfigActorParamRow>
{
    public override string CsvName => "ConfigActorParam";

    public static string ActorKey(eEnemyId id) { return "Enemy." + id; }
    public static string ActorKey(eBossId id) { return "Boss." + id; }
    public static string ActorKey(eSubWeapon id) { return "SubWeapon." + id; }
    public static string ActorKey(eItemId id) { return "Item." + id; }

    /// <summary>Dòng (actor, key) hoặc null nếu không có (không log — tham số tuỳ chọn).</summary>
    public ConfigActorParamRow Find(string actor, string key)
    {
        foreach (var r in rows)
            if (r.actor == actor && r.key == key)
                return r;
        return null;
    }
}
