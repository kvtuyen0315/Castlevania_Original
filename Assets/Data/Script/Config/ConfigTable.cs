using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bảng Config = danh sách dòng nhập từ 1 file CSV (Assets/Data/Config/Csv/&lt;CsvName&gt;.csv) bằng menu CVGP/Import Config.
/// Lớp con cụ thể nằm ở file riêng cùng tên (Unity cần để asset tham chiếu đúng script).
/// </summary>
public abstract class ConfigTableBase : ScriptableObject
{
    /// <summary>Tên file CSV (không đuôi) = tên asset.</summary>
    public abstract string CsvName { get; }

    public abstract Type RowType { get; }

    public abstract int Count { get; }

#if UNITY_EDITOR
    /// <summary>Chỉ importer gọi.</summary>
    public abstract void SetRowsUntyped(System.Collections.IList rows);
#endif
}

public abstract class ConfigTable<TRow> : ConfigTableBase where TRow : class, new()
{
    [SerializeField] protected List<TRow> rows = new List<TRow>();

    public IReadOnlyList<TRow> Rows => rows;

    public override Type RowType => typeof(TRow);

    public override int Count => rows.Count;

    /// <summary>Gọi khi danh sách dòng đổi (lớp con xoá cache tra cứu).</summary>
    protected virtual void OnRowsChanged()
    {
    }

    protected virtual void OnEnable()
    {
        OnRowsChanged();
    }

#if UNITY_EDITOR
    public override void SetRowsUntyped(System.Collections.IList value)
    {
        rows = new List<TRow>(value.Count);
        foreach (var r in value)
            rows.Add((TRow)r);
        OnRowsChanged();
    }
#endif
}

/// <summary>Bảng khoá–giá trị: key = tên giá trị của enum TKey (đủ mọi giá trị — importer kiểm).</summary>
public abstract class ConfigKvTable<TKey> : ConfigTable<ConfigKvRow> where TKey : struct, Enum
{
    private Dictionary<string, ConfigKvRow> map;

    protected override void OnRowsChanged()
    {
        map = null;
    }

    public ConfigKvRow GetRow(TKey key)
    {
        if (map == null)
        {
            map = new Dictionary<string, ConfigKvRow>();
            foreach (var r in rows)
                map[r.key] = r;
        }
        ConfigKvRow row;
        if (!map.TryGetValue(key.ToString(), out row))
        {
            Debug.LogError("[Config] " + CsvName + " thiếu khoá " + key);
            return null;
        }
        return row;
    }

    /// <summary>Giá trị chính (CfgFloat — has = false nếu chờ đo; khi đó xem min/max của dòng).</summary>
    public CfgFloat this[TKey key]
    {
        get
        {
            var r = GetRow(key);
            return r != null ? r.value : default(CfgFloat);
        }
    }

    /// <summary>Giá trị số nguyên (log lỗi nếu chưa có số).</summary>
    public int GetInt(TKey key, int fallback = 0)
    {
        var v = this[key];
        if (v.has)
            return Mathf.RoundToInt(v.value);
        v.Get(CsvName + "." + key);
        return fallback;
    }

    public float GetFloat(TKey key, float fallback = 0f)
    {
        return this[key].Get(CsvName + "." + key, fallback);
    }
}
