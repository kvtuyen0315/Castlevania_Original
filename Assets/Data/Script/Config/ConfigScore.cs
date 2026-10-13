using UnityEngine;

/// <summary>Điểm, 1UP, tally (ConfigScore.csv, khoá eScoreParam).</summary>
public class ConfigScore : ConfigKvTable<eScoreParam>
{
    public override string CsvName => "ConfigScore";
}
