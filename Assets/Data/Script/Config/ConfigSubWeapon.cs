/// <summary>5 vũ khí phụ (ConfigSubWeapon.csv).</summary>
public class ConfigSubWeapon : ConfigTable<ConfigSubWeaponRow>
{
    public override string CsvName => "ConfigSubWeapon";

    public ConfigSubWeaponRow Get(eSubWeapon id)
    {
        foreach (var r in rows)
            if (r.id == id)
                return r;
        UnityEngine.Debug.LogError("[Config] ConfigSubWeapon thiếu " + id);
        return null;
    }
}
