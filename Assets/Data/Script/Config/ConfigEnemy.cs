/// <summary>15 enemy thường + 3 thực thể phụ (ConfigEnemy.csv).</summary>
public class ConfigEnemy : ConfigTable<ConfigEnemyRow>
{
    public override string CsvName => "ConfigEnemy";

    public ConfigEnemyRow Get(eEnemyId id)
    {
        foreach (var r in rows)
            if (r.id == id)
                return r;
        UnityEngine.Debug.LogError("[Config] ConfigEnemy thiếu " + id);
        return null;
    }
}
