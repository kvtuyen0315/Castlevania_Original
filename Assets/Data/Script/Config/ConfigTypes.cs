using System;
using UnityEngine;

// Kiểu dùng chung của hệ Config (CVGP 04). Quy ước ô CSV (CVGP_MEMORY §4):
//   số            → giá trị đã biết; nguồn (cột src_<x> hoặc source) phải có thẻ gốc: wiki: / video: / đo: / suy luận / CVTI: / quy ước:
//   ô trống       → CHƯA biết; nguồn phải ghi "chờ đo routine NN" hoặc "không rõ" hoặc "n/a"
//   "-"           → không áp dụng cho dòng này (không cần nguồn)

/// <summary>Số nguyên có thể chưa biết (ô CSV trống/"-" ⇒ has = false).</summary>
[Serializable]
public struct CfgInt
{
    public bool has;
    public int value;

    public CfgInt(int v)
    {
        has = true;
        value = v;
    }

    /// <summary>Giá trị; chưa có thì log lỗi 1 dòng (kèm ngữ cảnh) và trả fallback.</summary>
    public int Get(string context, int fallback = 0)
    {
        if (has)
            return value;
        Debug.LogError("[Config] Chưa có số liệu: " + context + " (ô trống/chờ đo)");
        return fallback;
    }

    public override string ToString()
    {
        return has ? value.ToString() : "?";
    }
}

/// <summary>Số thực có thể chưa biết (ô CSV trống/"-" ⇒ has = false). Đơn vị px/tick, tick… ghi ở CSV.</summary>
[Serializable]
public struct CfgFloat
{
    public bool has;
    public float value;

    public CfgFloat(float v)
    {
        has = true;
        value = v;
    }

    public float Get(string context, float fallback = 0f)
    {
        if (has)
            return value;
        Debug.LogError("[Config] Chưa có số liệu: " + context + " (ô trống/chờ đo)");
        return fallback;
    }

    /// <summary>Đổi px/tick sang fixed-point 1/256 px (H4) — làm tròn gần nhất.</summary>
    public int ToFixed256(string context)
    {
        return Mathf.RoundToInt(Get(context) * 256f);
    }

    public override string ToString()
    {
        return has ? value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) : "?";
    }
}

/// <summary>Ô số này lấy nguồn ở cột src khác (mặc định: src_&lt;tên field&gt; nếu có, không thì source).</summary>
[AttributeUsage(AttributeTargets.Field)]
public class CfgSrcAttribute : Attribute
{
    public readonly string column;

    public CfgSrcAttribute(string column)
    {
        this.column = column;
    }
}

/// <summary>Cho phép số âm (vd vận tốc y — NES y hướng xuống nên đi lên là âm).</summary>
[AttributeUsage(AttributeTargets.Field)]
public class CfgAllowNegativeAttribute : Attribute
{
}

/// <summary>Chuỗi là danh sách giá trị enum cách nhau ';' — importer kiểm từng phần tử.</summary>
[AttributeUsage(AttributeTargets.Field)]
public class CfgEnumListAttribute : Attribute
{
    public readonly Type enumType;

    public CfgEnumListAttribute(Type enumType)
    {
        this.enumType = enumType;
    }
}

/// <summary>Chuỗi là danh sách số nguyên cách nhau ';' (vd stage "1;3").</summary>
[AttributeUsage(AttributeTargets.Field)]
public class CfgIntListAttribute : Attribute
{
}

/// <summary>Phân loại vũ khí theo wiki Enemies: yếu = roi da, Holy Water, Dagger; mạnh = roi xích, Cross, Axe.</summary>
public enum eHitStrength
{
    None,
    Weak,
    Strong,
}

public enum eEnemyKind
{
    Normal,
    Sub,
}

/// <summary>Số đòn của boss tính cho: 1 thực thể / tổng các thực thể / chỉ thực thể chính (Frankenstein — Igor bất tử).</summary>
public enum eHitsScope
{
    Single,
    TotalAllEntities,
    MainEntityOnly,
}

public enum eItemKind
{
    Heart,
    Score,
    Whip,
    SubWeapon,
    Multiplier,
    Special,
    Heal,
    Life,
    Crystal,
    Container,
}

/// <summary>Dòng của bảng khoá–giá trị (ConfigSimon / ConfigScore / ConfigRule): key = tên enum.</summary>
[Serializable]
public class ConfigKvRow
{
    public string key;
    public CfgFloat value;
    public CfgFloat min;
    public CfgFloat max;
    public string unit;
    public string source;
    public string note;
}
