/// <summary>18 stage (ConfigStage.csv).</summary>
public class ConfigStage : ConfigTable<ConfigStageRow>
{
    public override string CsvName => "ConfigStage";

    public ConfigStageRow Get(int stage)
    {
        foreach (var r in rows)
            if (r.stage == stage)
                return r;
        UnityEngine.Debug.LogError("[Config] ConfigStage thiếu stage " + stage);
        return null;
    }
}
