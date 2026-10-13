using UnityEngine;

/// <summary>Luật chung, HUD, chuyển cảnh (ConfigRule.csv, khoá eRuleParam).</summary>
public class ConfigRule : ConfigKvTable<eRuleParam>
{
    public override string CsvName => "ConfigRule";
}
