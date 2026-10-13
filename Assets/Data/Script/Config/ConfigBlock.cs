/// <summary>6 block × 3 stage (ConfigBlock.csv): giờ, boss, bậc sát thương Simon nhận.</summary>
public class ConfigBlock : ConfigTable<ConfigBlockRow>
{
    public override string CsvName => "ConfigBlock";

    public ConfigBlockRow Get(int block)
    {
        foreach (var r in rows)
            if (r.block == block)
                return r;
        UnityEngine.Debug.LogError("[Config] ConfigBlock thiếu block " + block);
        return null;
    }
}
