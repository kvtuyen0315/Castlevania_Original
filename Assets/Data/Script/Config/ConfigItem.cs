/// <summary>22 item + 3 vật chứa (ConfigItem.csv).</summary>
public class ConfigItem : ConfigTable<ConfigItemRow>
{
    public override string CsvName => "ConfigItem";

    public ConfigItemRow Get(eItemId id)
    {
        foreach (var r in rows)
            if (r.id == id)
                return r;
        UnityEngine.Debug.LogError("[Config] ConfigItem thiếu " + id);
        return null;
    }
}
