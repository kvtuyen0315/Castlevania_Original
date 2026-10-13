using UnityEngine;

/// <summary>Nội dung vật chứa đặt trên map (ConfigDrop.csv — routine 11 điền nội dung Block 1 đã biết, routine 15 đặt vật thể theo objectId).</summary>
public class ConfigDrop : ConfigTable<ConfigDropRow>
{
    public override string CsvName => "ConfigDrop";

    /// <summary>Dòng theo objectId hoặc null (vật chứa không có dòng ⇒ nội dung mặc định của vật — không log).</summary>
    public ConfigDropRow Find(string objectId)
    {
        if (string.IsNullOrEmpty(objectId))
            return null;
        foreach (var r in rows)
            if (r.objectId == objectId)
                return r;
        return null;
    }
}
