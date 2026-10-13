/// <summary>Roi cấp 1/2/3 (ConfigWhip.csv).</summary>
public class ConfigWhip : ConfigTable<ConfigWhipRow>
{
    public override string CsvName => "ConfigWhip";

    public ConfigWhipRow Get(int level)
    {
        foreach (var r in rows)
            if (r.level == level)
                return r;
        UnityEngine.Debug.LogError("[Config] ConfigWhip thiếu cấp " + level);
        return null;
    }

    public int MaxLevel
    {
        get
        {
            int m = 0;
            foreach (var r in rows)
                if (r.level > m)
                    m = r.level;
            return m;
        }
    }
}
