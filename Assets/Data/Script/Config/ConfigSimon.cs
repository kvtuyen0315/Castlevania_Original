using UnityEngine;

/// <summary>Chỉ số Simon (ConfigSimon.csv, khoá eSimonParam).</summary>
public class ConfigSimon : ConfigKvTable<eSimonParam>
{
    public override string CsvName => "ConfigSimon";
}
