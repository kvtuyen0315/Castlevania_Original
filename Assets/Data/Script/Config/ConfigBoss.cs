/// <summary>6 boss, 1 dòng / pha (ConfigBoss.csv).</summary>
public class ConfigBoss : ConfigTable<ConfigBossRow>
{
    public override string CsvName => "ConfigBoss";

    public ConfigBossRow Get(eBossId id, int phase = 1)
    {
        foreach (var r in rows)
            if (r.id == id && r.phase == phase)
                return r;
        UnityEngine.Debug.LogError("[Config] ConfigBoss thiếu " + id + " pha " + phase);
        return null;
    }

    public int PhaseCount(eBossId id)
    {
        int n = 0;
        foreach (var r in rows)
            if (r.id == id)
                n++;
        return n;
    }
}
