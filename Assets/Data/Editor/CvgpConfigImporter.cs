using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CVGP 04 — nhập Config: Assets/Data/Config/Csv/*.csv → Assets/Data/Config/*.asset (+ GameConfig.asset).
/// Không dùng afsdk.GSheetScriptableObject.ParseCSV vì nó không phân biệt ô trống với 0, nuốt lỗi và dừng ở dòng trống (CVGP_MEMORY §4).
/// Kiểm: đủ/không thừa cột, số ô mỗi dòng, kiểu, enum, id trùng/thiếu, số âm/vô lý, ô số không có nguồn ⇒ báo lỗi file:dòng:cột
/// và KHÔNG ghi asset nào. Chạy lại với CSV không đổi ⇒ asset không đổi (so JSON trước khi ghi; giữ GUID).
/// Menu CVGP/Export Stats Report sinh ClaudeNotes/cvgp/CHARACTER_STATS.md từ cùng CSV.
/// </summary>
public static class CvgpConfigImporter
{
    const string CSV_DIR = "Assets/Data/Config/Csv";
    const string ASSET_DIR = "Assets/Data/Config";
    const string ROOT_PATH = "Assets/Data/Config/GameConfig.asset";
    const string REPORT_REL = "ClaudeNotes/cvgp/CHARACTER_STATS.md";

    static readonly string[] ORIGIN_TAGS = { "wiki:", "video:", "đo:", "suy luận", "CVTI", "quy ước" };
    static readonly Regex PENDING_RX = new Regex(@"chờ đo routine \d\d|không rõ|n/a", RegexOptions.CultureInvariant);
    static readonly Regex CHO_DO_RX = new Regex(@"chờ đo(?! routine \d\d)", RegexOptions.CultureInvariant);
    static readonly Regex KEY_RX = new Regex(@"^[A-Za-z][A-Za-z0-9]*$");

    // ---------------- Đặc tả bảng ----------------

    class Spec
    {
        public string name;
        public Type tableType;
        public Type rowType;
        public Type kvEnum;           // bảng khoá–giá trị
        public Func<object, string> key; // khoá duy nhất của dòng (null = không kiểm)
        public string title;
    }

    static readonly Spec[] SPECS =
    {
        new Spec { name = "ConfigSimon", tableType = typeof(ConfigSimon), rowType = typeof(ConfigKvRow), kvEnum = typeof(eSimonParam), title = "Simon" },
        new Spec { name = "ConfigEnemy", tableType = typeof(ConfigEnemy), rowType = typeof(ConfigEnemyRow), key = r => ((ConfigEnemyRow)r).id.ToString(), title = "Enemy" },
        new Spec { name = "ConfigBoss", tableType = typeof(ConfigBoss), rowType = typeof(ConfigBossRow), key = r => ((ConfigBossRow)r).id + "/" + ((ConfigBossRow)r).phase, title = "Boss" },
        new Spec { name = "ConfigActorParam", tableType = typeof(ConfigActorParam), rowType = typeof(ConfigActorParamRow), key = r => ((ConfigActorParamRow)r).actor + "/" + ((ConfigActorParamRow)r).key, title = "Tham số hành vi" },
        new Spec { name = "ConfigItem", tableType = typeof(ConfigItem), rowType = typeof(ConfigItemRow), key = r => ((ConfigItemRow)r).id.ToString(), title = "Item" },
        new Spec { name = "ConfigSubWeapon", tableType = typeof(ConfigSubWeapon), rowType = typeof(ConfigSubWeaponRow), key = r => ((ConfigSubWeaponRow)r).id.ToString(), title = "Vũ khí phụ" },
        new Spec { name = "ConfigWhip", tableType = typeof(ConfigWhip), rowType = typeof(ConfigWhipRow), key = r => ((ConfigWhipRow)r).level.ToString(), title = "Roi" },
        new Spec { name = "ConfigBlock", tableType = typeof(ConfigBlock), rowType = typeof(ConfigBlockRow), key = r => ((ConfigBlockRow)r).block.ToString(), title = "Block" },
        new Spec { name = "ConfigStage", tableType = typeof(ConfigStage), rowType = typeof(ConfigStageRow), key = r => ((ConfigStageRow)r).stage.ToString(), title = "Stage" },
        new Spec { name = "ConfigScore", tableType = typeof(ConfigScore), rowType = typeof(ConfigKvRow), kvEnum = typeof(eScoreParam), title = "Điểm" },
        new Spec { name = "ConfigRule", tableType = typeof(ConfigRule), rowType = typeof(ConfigKvRow), kvEnum = typeof(eRuleParam), title = "Luật" },
        new Spec { name = "ConfigDrop", tableType = typeof(ConfigDrop), rowType = typeof(ConfigDropRow), key = r => ((ConfigDropRow)r).objectId, title = "Vật chứa" },
    };

    // ---------------- Kết quả đọc ----------------

    class Parsed
    {
        public Spec spec;
        public string[] header;
        public List<string[]> cells = new List<string[]>();
        public List<int> lines = new List<int>();
        public List<object> rows = new List<object>();
    }

    class Errors
    {
        public readonly List<string> list = new List<string>();

        public void Add(string file, int line, string col, string msg)
        {
            list.Add(file + ".csv:" + line + (string.IsNullOrEmpty(col) ? "" : " [" + col + "]") + " — " + msg);
        }
    }

    // ---------------- Menu ----------------

    [MenuItem("CVGP/Import Config")]
    public static void ImportMenu()
    {
        Import();
    }

    [MenuItem("CVGP/Export Stats Report")]
    public static void ExportMenu()
    {
        ExportReport();
    }

    /// <summary>Đọc + kiểm toàn bộ CSV; có lỗi ⇒ log từng lỗi, không ghi gì, trả false.</summary>
    public static bool Import()
    {
        Errors err;
        var parsed = ParseAll(out err);
        if (err.list.Count > 0)
        {
            foreach (var e in err.list)
                Debug.LogError("[CVGP Config] " + e);
            Debug.LogError("[CVGP Config] Import THẤT BẠI: " + err.list.Count + " lỗi — không ghi asset nào.");
            return false;
        }

        int changed = 0, same = 0;
        var tables = new Dictionary<string, ConfigTableBase>();
        foreach (var p in parsed)
        {
            bool wrote;
            tables[p.spec.name] = WriteTable(p, out wrote);
            if (wrote) changed++; else same++;
        }

        bool rootWrote = WriteRoot(tables);
        AssetDatabase.SaveAssets();
        Debug.Log("[CVGP Config] Import OK: " + parsed.Count + " bảng (" + parsed.Sum(p => p.rows.Count) + " dòng) — ghi lại " + changed
                  + ", không đổi " + same + "; GameConfig " + (rootWrote ? "ghi lại" : "không đổi") + ".");
        return true;
    }

    // ---------------- Đọc & kiểm ----------------

    static List<Parsed> ParseAll(out Errors err)
    {
        err = new Errors();
        var result = new List<Parsed>();
        foreach (var spec in SPECS)
        {
            var path = CSV_DIR + "/" + spec.name + ".csv";
            if (!File.Exists(path))
            {
                err.Add(spec.name, 0, null, "thiếu file " + path);
                continue;
            }
            var p = ParseTable(spec, File.ReadAllText(path, Encoding.UTF8), err);
            if (p != null)
                result.Add(p);
        }
        if (err.list.Count == 0)
            CrossCheck(result, err);
        return result;
    }

    static Parsed ParseTable(Spec spec, string text, Errors err)
    {
        var grid = ReadCsv(text, spec.name, err);
        if (grid.Count == 0)
        {
            err.Add(spec.name, 1, null, "file rỗng (thiếu dòng tiêu đề)");
            return null;
        }

        var p = new Parsed { spec = spec, header = grid[0].Value };
        int errBefore = err.list.Count;
        var fields = spec.rowType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var colOf = new Dictionary<string, int>();
        for (int c = 0; c < p.header.Length; c++)
        {
            var h = p.header[c];
            if (colOf.ContainsKey(h))
                err.Add(spec.name, 1, h, "cột trùng tên");
            colOf[h] = c;
            if (!fields.Any(f => f.Name == h))
                err.Add(spec.name, 1, h, "cột lạ (không có field " + spec.rowType.Name + "." + h + ")");
        }
        foreach (var f in fields)
            if (!colOf.ContainsKey(f.Name))
                err.Add(spec.name, 1, f.Name, "thiếu cột bắt buộc");
        if (err.list.Count > errBefore)
            return p;

        var seen = new Dictionary<string, int>();
        for (int i = 1; i < grid.Count; i++)
        {
            int line = grid[i].Key;
            var cells = grid[i].Value;
            if (cells.Length != p.header.Length)
            {
                err.Add(spec.name, line, null, "có " + cells.Length + " ô, tiêu đề có " + p.header.Length + " cột");
                continue;
            }
            var row = Activator.CreateInstance(spec.rowType);
            foreach (var f in fields)
                ParseCell(spec, f, row, cells, colOf, line, err);
            CheckSources(spec, fields, cells, colOf, line, err);
            CheckRow(spec, row, line, err);

            if (spec.key != null || spec.kvEnum != null)
            {
                string k = spec.key != null ? spec.key(row) : ((ConfigKvRow)row).key;
                int prev;
                if (seen.TryGetValue(k, out prev))
                    err.Add(spec.name, line, null, "khoá trùng '" + k + "' (đã có ở dòng " + prev + ")");
                else
                    seen[k] = line;
            }
            p.rows.Add(row);
            p.cells.Add(cells);
            p.lines.Add(line);
        }

        CheckCoverage(p, seen, err);
        return p;
    }

    static void ParseCell(Spec spec, FieldInfo f, object row, string[] cells, Dictionary<string, int> colOf, int line, Errors err)
    {
        string raw = cells[colOf[f.Name]].Trim();
        var t = f.FieldType;
        try
        {
            if (t == typeof(string))
            {
                f.SetValue(row, raw);
                if (raw.Length > 0 && f.GetCustomAttribute<CfgIntListAttribute>() != null)
                    foreach (var part in raw.Split(';'))
                        ParseInt(part.Trim());
                var el = f.GetCustomAttribute<CfgEnumListAttribute>();
                if (raw.Length > 0 && el != null)
                    foreach (var part in raw.Split(';'))
                        ParseEnum(el.enumType, part.Trim());
                return;
            }
            if (raw.Length == 0 && !(t == typeof(CfgInt) || t == typeof(CfgFloat)))
            {
                err.Add(spec.name, line, f.Name, "ô bắt buộc đang trống");
                return;
            }
            if (t == typeof(int))
                f.SetValue(row, ParseInt(raw));
            else if (t == typeof(float))
                f.SetValue(row, ParseFloat(raw));
            else if (t == typeof(bool))
                f.SetValue(row, ParseBool(raw));
            else if (t.IsEnum)
                f.SetValue(row, ParseEnum(t, raw));
            else if (t == typeof(CfgInt))
                f.SetValue(row, raw.Length == 0 || raw == "-" ? new CfgInt() : new CfgInt(ParseInt(raw)));
            else if (t == typeof(CfgFloat))
                f.SetValue(row, raw.Length == 0 || raw == "-" ? new CfgFloat() : new CfgFloat(ParseFloat(raw)));
            else
                err.Add(spec.name, line, f.Name, "kiểu field chưa hỗ trợ: " + t.Name);
        }
        catch (FormatException ex)
        {
            err.Add(spec.name, line, f.Name, "giá trị '" + raw + "' không hợp lệ: " + ex.Message);
        }
    }

    static bool IsNumeric(Type t)
    {
        return t == typeof(int) || t == typeof(float) || t == typeof(CfgInt) || t == typeof(CfgFloat);
    }

    /// <summary>Cột nguồn của 1 field: [CfgSrc] → src_&lt;field&gt; → source; ô nguồn riêng trống thì lùi về source.</summary>
    static string SourceOf(FieldInfo f, string[] cells, Dictionary<string, int> colOf)
    {
        var a = f.GetCustomAttribute<CfgSrcAttribute>();
        string col = a != null ? a.column : (colOf.ContainsKey("src_" + f.Name) ? "src_" + f.Name : "source");
        string s = colOf.ContainsKey(col) ? cells[colOf[col]].Trim() : "";
        if (s.Length == 0 && colOf.ContainsKey("source"))
            s = cells[colOf["source"]].Trim();
        return s;
    }

    static void CheckSources(Spec spec, FieldInfo[] fields, string[] cells, Dictionary<string, int> colOf, int line, Errors err)
    {
        foreach (var f in fields)
        {
            if (f.FieldType == typeof(string) && (f.Name == "source" || f.Name.StartsWith("src_")))
            {
                var txt = cells[colOf[f.Name]];
                if (CHO_DO_RX.IsMatch(txt))
                    err.Add(spec.name, line, f.Name, "'chờ đo' phải ghi rõ 'chờ đo routine NN'");
                continue;
            }
            if (!IsNumeric(f.FieldType))
                continue;
            string raw = cells[colOf[f.Name]].Trim();
            if (raw == "-")
                continue;
            // Bảng khoá–giá trị: min/max là tuỳ chọn (chỉ có khi đo ra khoảng).
            if (raw.Length == 0 && (f.Name == "min" || f.Name == "max"))
                continue;
            string src = SourceOf(f, cells, colOf);
            if (raw.Length == 0)
            {
                if (!PENDING_RX.IsMatch(src))
                    err.Add(spec.name, line, f.Name, "ô số trống nhưng nguồn không ghi 'chờ đo routine NN' / 'không rõ' / 'n/a'");
            }
            else if (!ORIGIN_TAGS.Any(src.Contains))
            {
                err.Add(spec.name, line, f.Name, "ô số không có nguồn (cần wiki:/video:/đo:/suy luận/CVTI/quy ước) — nguồn: '" + src + "'");
            }
            if (raw.Length > 0 && f.FieldType != typeof(string) && f.GetCustomAttribute<CfgAllowNegativeAttribute>() == null
                && !(spec.kvEnum != null || spec.rowType == typeof(ConfigActorParamRow)) && raw.StartsWith("-"))
                err.Add(spec.name, line, f.Name, "số âm không hợp lệ");
        }
    }

    static void CheckRow(Spec spec, object row, int line, Errors err)
    {
        string n = spec.name;
        var kv = row as ConfigKvRow;
        if (kv != null)
        {
            if (!Enum.GetNames(spec.kvEnum).Contains(kv.key))
                err.Add(n, line, "key", "khoá '" + kv.key + "' không có trong " + spec.kvEnum.Name);
            CheckKvValues(n, line, kv.key, kv.value, kv.min, kv.max, err);
            return;
        }
        var ap = row as ConfigActorParamRow;
        if (ap != null)
        {
            CheckActor(n, line, ap.actor, err);
            if (string.IsNullOrEmpty(ap.key) || !KEY_RX.IsMatch(ap.key))
                err.Add(n, line, "key", "khoá '" + ap.key + "' phải là định danh chữ/số");
            CheckKvValues(n, line, ap.key, ap.value, ap.min, ap.max, err);
            return;
        }
        var e = row as ConfigEnemyRow;
        if (e != null)
        {
            if (e.id == eEnemyId.None) err.Add(n, line, "id", "id None không được dùng");
            CheckHits(n, line, e.hitsWeak, e.hitsStrong, err);
            CheckScore(n, line, e.score, err);
            if (e.moveSpeed.has && (e.moveSpeed.value <= 0 || e.moveSpeed.value > 8)) err.Add(n, line, "moveSpeed", "tốc độ vô lý (0, 8] px/tick");
            CheckStages(n, line, e.stages, err);
            return;
        }
        var b = row as ConfigBossRow;
        if (b != null)
        {
            if (b.id == eBossId.None) err.Add(n, line, "id", "id None không được dùng");
            if (b.phase < 1 || b.phase > 3) err.Add(n, line, "phase", "pha phải 1–3");
            if (b.stage < 1 || b.stage > 18) err.Add(n, line, "stage", "stage phải 1–18");
            if (b.entityCount < 1) err.Add(n, line, "entityCount", "phải ≥ 1");
            CheckHits(n, line, b.hitsWeak, b.hitsStrong, err);
            CheckScore(n, line, b.score, err);
            return;
        }
        var it = row as ConfigItemRow;
        if (it != null)
        {
            if (it.id == eItemId.None) err.Add(n, line, "id", "id None không được dùng");
            bool isContainer = (int)it.id >= 100;
            if (isContainer != (it.kind == eItemKind.Container)) err.Add(n, line, "kind", "id ≥ 100 ⇔ kind Container");
            CheckScore(n, line, it.score, err);
            if (it.hearts.has && (it.hearts.value < 1 || it.hearts.value > 99)) err.Add(n, line, "hearts", "số tim vô lý");
            if (it.healBars.has && (it.healBars.value < 1 || it.healBars.value > GameSession.MAX_HP)) err.Add(n, line, "healBars", "vượt 16 vạch");
            return;
        }
        var s = row as ConfigSubWeaponRow;
        if (s != null)
        {
            if (s.id == eSubWeapon.None) err.Add(n, line, "id", "id None không được dùng");
            if (s.heartCost < 1 || s.heartCost > 99) err.Add(n, line, "heartCost", "giá tim vô lý");
            if (s.pierces.has && s.pierces.value != 0 && s.pierces.value != 1) err.Add(n, line, "pierces", "chỉ 0/1");
            return;
        }
        var w = row as ConfigWhipRow;
        if (w != null)
        {
            if (w.level < 1 || w.level > 3) err.Add(n, line, "level", "cấp phải 1–3");
            if (w.colorCount < 1) err.Add(n, line, "colorCount", "phải ≥ 1");
            return;
        }
        var bl = row as ConfigBlockRow;
        if (bl != null)
        {
            if (bl.block < 1 || bl.block > 6) err.Add(n, line, "block", "block phải 1–6");
            if (bl.damageToSimon < 1 || bl.damageToSimon > GameSession.MAX_HP) err.Add(n, line, "damageToSimon", "sát thương vô lý");
            if (bl.timeLimit.has && (bl.timeLimit.value < 1 || bl.timeLimit.value > 9999)) err.Add(n, line, "timeLimit", "giờ vô lý (HUD 4 chữ số)");
            CheckStages(n, line, bl.stages, err);
            return;
        }
        var st = row as ConfigStageRow;
        if (st != null)
        {
            if (st.stage < 1 || st.stage > 18) err.Add(n, line, "stage", "stage phải 1–18");
            if (st.block != (st.stage - 1) / 3 + 1) err.Add(n, line, "block", "stage " + st.stage + " thuộc block " + ((st.stage - 1) / 3 + 1));
            return;
        }
        var d = row as ConfigDropRow;
        if (d != null)
        {
            if (string.IsNullOrEmpty(d.objectId)) err.Add(n, line, "objectId", "trống");
            if ((int)d.container < 100) err.Add(n, line, "container", "phải là vật chứa (Torch/WallCandle/BreakableBlock)");
            if ((int)d.item >= 100) err.Add(n, line, "item", "không được là vật chứa");
            if (d.stage < 1 || d.stage > 18) err.Add(n, line, "stage", "stage phải 1–18");
        }
    }

    static void CheckKvValues(string n, int line, string key, CfgFloat v, CfgFloat min, CfgFloat max, Errors err)
    {
        bool neg = key != null && key.IndexOf("vy", StringComparison.OrdinalIgnoreCase) >= 0;
        if (!neg && ((v.has && v.value < 0) || (min.has && min.value < 0) || (max.has && max.value < 0)))
            err.Add(n, line, "value", "số âm không hợp lệ (chỉ khoá vận tốc y 'Vy' được âm)");
        if (min.has && max.has && min.value > max.value)
            err.Add(n, line, "min", "min > max");
        if (v.has && min.has && max.has && (v.value < min.value || v.value > max.value))
            err.Add(n, line, "value", "giá trị nằm ngoài [min, max]");
    }

    static void CheckHits(string n, int line, CfgInt weak, CfgInt strong, Errors err)
    {
        if (weak.has && weak.value < 1) err.Add(n, line, "hitsWeak", "phải ≥ 1");
        if (strong.has && strong.value < 1) err.Add(n, line, "hitsStrong", "phải ≥ 1");
        if (weak.has && strong.has && strong.value > weak.value) err.Add(n, line, "hitsStrong", "đòn mạnh cần nhiều đòn hơn đòn yếu — vô lý");
    }

    static void CheckScore(string n, int line, CfgInt score, Errors err)
    {
        if (score.has && (score.value < 0 || score.value % 100 != 0 || score.value > 999999))
            err.Add(n, line, "score", "điểm phải là bội 100, 0–999999");
    }

    static void CheckStages(string n, int line, string stages, Errors err)
    {
        if (string.IsNullOrEmpty(stages))
            return;
        foreach (var part in stages.Split(';'))
        {
            int v;
            if (int.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) && (v < 1 || v > 18))
                err.Add(n, line, "stages", "stage " + v + " ngoài 1–18");
        }
    }

    static void CheckActor(string n, int line, string actor, Errors err)
    {
        var parts = (actor ?? "").Split('.');
        Type t = null;
        if (parts.Length == 2)
        {
            switch (parts[0])
            {
                case "Enemy": t = typeof(eEnemyId); break;
                case "Boss": t = typeof(eBossId); break;
                case "SubWeapon": t = typeof(eSubWeapon); break;
                case "Item": t = typeof(eItemId); break;
            }
        }
        if (t == null || !Enum.GetNames(t).Contains(parts[1]) || parts[1] == "None")
            err.Add(n, line, "actor", "actor '" + actor + "' phải dạng Enemy.X / Boss.X / SubWeapon.X / Item.X với X là id hợp lệ");
    }

    /// <summary>Đủ 100 % id: mọi giá trị enum (trừ None) có dòng.</summary>
    static void CheckCoverage(Parsed p, Dictionary<string, int> seen, Errors err)
    {
        Type full = p.spec.kvEnum;
        if (p.spec.rowType == typeof(ConfigEnemyRow)) full = typeof(eEnemyId);
        if (p.spec.rowType == typeof(ConfigItemRow)) full = typeof(eItemId);
        if (p.spec.rowType == typeof(ConfigSubWeaponRow)) full = typeof(eSubWeapon);
        if (full != null)
        {
            foreach (var name in Enum.GetNames(full))
                if (name != "None" && !seen.ContainsKey(name))
                    err.Add(p.spec.name, 0, null, "thiếu dòng cho " + full.Name + "." + name);
        }
        if (p.spec.rowType == typeof(ConfigBossRow))
            foreach (eBossId id in Enum.GetValues(typeof(eBossId)))
                if (id != eBossId.None && !seen.ContainsKey(id + "/1"))
                    err.Add(p.spec.name, 0, null, "thiếu dòng boss " + id + " pha 1");
        Action<int> range = max =>
        {
            for (int i = 1; i <= max; i++)
                if (!seen.ContainsKey(i.ToString()))
                    err.Add(p.spec.name, 0, null, "thiếu dòng số " + i);
        };
        if (p.spec.rowType == typeof(ConfigWhipRow)) range(3);
        if (p.spec.rowType == typeof(ConfigBlockRow)) range(6);
        if (p.spec.rowType == typeof(ConfigStageRow)) range(18);
    }

    /// <summary>Kiểm chéo bảng: block ↔ stage ↔ boss.</summary>
    static void CrossCheck(List<Parsed> all, Errors err)
    {
        var blocks = all.First(p => p.spec.name == "ConfigBlock");
        var stages = all.First(p => p.spec.name == "ConfigStage");
        var bosses = all.First(p => p.spec.name == "ConfigBoss");
        for (int i = 0; i < blocks.rows.Count; i++)
        {
            var b = (ConfigBlockRow)blocks.rows[i];
            var list = b.stages.Split(';').Select(s => int.Parse(s.Trim(), CultureInfo.InvariantCulture)).ToArray();
            foreach (var s in list)
            {
                var st = stages.rows.Cast<ConfigStageRow>().FirstOrDefault(x => x.stage == s);
                if (st == null || st.block != b.block)
                    err.Add("ConfigBlock", blocks.lines[i], "stages", "stage " + s + " không thuộc block " + b.block + " trong ConfigStage");
            }
            var bossStage = stages.rows.Cast<ConfigStageRow>().Where(x => x.block == b.block && x.boss != eBossId.None).Select(x => x.boss).ToArray();
            if (bossStage.Length != 1 || bossStage[0] != b.boss)
                err.Add("ConfigBlock", blocks.lines[i], "boss", "boss " + b.boss + " không khớp ConfigStage của block " + b.block);
        }
        for (int i = 0; i < bosses.rows.Count; i++)
        {
            var bo = (ConfigBossRow)bosses.rows[i];
            var st = stages.rows.Cast<ConfigStageRow>().FirstOrDefault(x => x.stage == bo.stage);
            if (st == null || st.boss != bo.id)
                err.Add("ConfigBoss", bosses.lines[i], "stage", "ConfigStage stage " + bo.stage + " không ghi boss " + bo.id);
        }
    }

    // ---------------- Kiểu số ----------------

    static int ParseInt(string s)
    {
        int v;
        if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
            throw new FormatException("cần số nguyên");
        return v;
    }

    static float ParseFloat(string s)
    {
        float v;
        if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) || float.IsNaN(v) || float.IsInfinity(v))
            throw new FormatException("cần số thực (dấu chấm thập phân)");
        return v;
    }

    static bool ParseBool(string s)
    {
        switch (s.ToLowerInvariant())
        {
            case "1": case "true": return true;
            case "0": case "false": return false;
        }
        throw new FormatException("cần 1/0 hoặc true/false");
    }

    static object ParseEnum(Type t, string s)
    {
        if (s.Length == 0 || char.IsDigit(s[0]) || s[0] == '-' || !Enum.GetNames(t).Contains(s))
            throw new FormatException("không có trong " + t.Name);
        return Enum.Parse(t, s);
    }

    // ---------------- CSV (RFC 4180, UTF-8) ----------------

    /// <summary>Trả (số dòng file, các ô). Bỏ dòng trống. Ô có ngoặc kép được chứa dấu phẩy/xuống dòng, "" = ".</summary>
    static List<KeyValuePair<int, string[]>> ReadCsv(string text, string file, Errors err)
    {
        var rows = new List<KeyValuePair<int, string[]>>();
        if (text.Length > 0 && text[0] == '\uFEFF')
            text = text.Substring(1);
        var cells = new List<string>();
        var sb = new StringBuilder();
        bool inQ = false, rowHasData = false;
        int line = 1, rowLine = 1;
        for (int i = 0; i <= text.Length; i++)
        {
            char c = i < text.Length ? text[i] : '\n';
            if (inQ)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQ = false;
                }
                else
                {
                    if (c == '\n') line++;
                    if (i == text.Length) { err.Add(file, rowLine, null, "ngoặc kép chưa đóng"); break; }
                    sb.Append(c);
                }
                continue;
            }
            if (c == '"') { inQ = true; rowHasData = true; continue; }
            if (c == ',') { cells.Add(sb.ToString()); sb.Length = 0; rowHasData = true; continue; }
            if (c == '\r') continue;
            if (c == '\n')
            {
                if (rowHasData || sb.Length > 0)
                {
                    cells.Add(sb.ToString());
                    rows.Add(new KeyValuePair<int, string[]>(rowLine, cells.ToArray()));
                }
                cells.Clear();
                sb.Length = 0;
                rowHasData = false;
                line++;
                rowLine = line;
                continue;
            }
            sb.Append(c);
        }
        return rows;
    }

    // ---------------- Ghi asset ----------------

    static ConfigTableBase WriteTable(Parsed p, out bool wrote)
    {
        var path = ASSET_DIR + "/" + p.spec.name + ".asset";
        var fresh = (ConfigTableBase)ScriptableObject.CreateInstance(p.spec.tableType);
        fresh.SetRowsUntyped((IList)p.rows);
        var existing = AssetDatabase.LoadAssetAtPath<ConfigTableBase>(path);
        if (existing == null)
        {
            fresh.name = p.spec.name;
            AssetDatabase.CreateAsset(fresh, path);
            wrote = true;
            return fresh;
        }
        fresh.name = existing.name;
        if (EditorJsonUtility.ToJson(fresh) == EditorJsonUtility.ToJson(existing))
        {
            UnityEngine.Object.DestroyImmediate(fresh);
            wrote = false;
            return existing;
        }
        EditorUtility.CopySerialized(fresh, existing);
        UnityEngine.Object.DestroyImmediate(fresh);
        EditorUtility.SetDirty(existing);
        wrote = true;
        return existing;
    }

    static bool WriteRoot(Dictionary<string, ConfigTableBase> t)
    {
        var root = AssetDatabase.LoadAssetAtPath<GameConfig>(ROOT_PATH);
        bool created = false;
        if (root == null)
        {
            root = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(root, ROOT_PATH);
            created = true;
        }
        var before = EditorJsonUtility.ToJson(root);
        root.simon = (ConfigSimon)t["ConfigSimon"];
        root.enemies = (ConfigEnemy)t["ConfigEnemy"];
        root.bosses = (ConfigBoss)t["ConfigBoss"];
        root.actorParams = (ConfigActorParam)t["ConfigActorParam"];
        root.items = (ConfigItem)t["ConfigItem"];
        root.subWeapons = (ConfigSubWeapon)t["ConfigSubWeapon"];
        root.whips = (ConfigWhip)t["ConfigWhip"];
        root.blocks = (ConfigBlock)t["ConfigBlock"];
        root.stages = (ConfigStage)t["ConfigStage"];
        root.score = (ConfigScore)t["ConfigScore"];
        root.rules = (ConfigRule)t["ConfigRule"];
        root.drops = (ConfigDrop)t["ConfigDrop"];
        bool changed = created || before != EditorJsonUtility.ToJson(root);
        if (changed)
            EditorUtility.SetDirty(root);
        return changed;
    }

    // ---------------- Báo cáo CHARACTER_STATS.md ----------------

    public static bool ExportReport()
    {
        Errors err;
        var parsed = ParseAll(out err);
        if (err.list.Count > 0)
        {
            foreach (var e in err.list)
                Debug.LogError("[CVGP Config] " + e);
            Debug.LogError("[CVGP Config] Export THẤT BẠI: CSV có " + err.list.Count + " lỗi.");
            return false;
        }
        var P = parsed.ToDictionary(p => p.spec.name);
        var sb = new StringBuilder();
        sb.AppendLine("# CHARACTER_STATS — Bảng chỉ số Castlevania (NES) của project");
        sb.AppendLine();
        sb.AppendLine("> **Tự sinh** bởi menu `CVGP/Export Stats Report` từ `Assets/Data/Config/Csv/*.csv` (CVGP 04) — đừng sửa tay: sửa CSV rồi chạy `CVGP/Import Config` + `CVGP/Export Stats Report`.");
        sb.AppendLine("> Ô *nghiêng* = chưa có số: *chờ đo routine NN* (routine đó đo từ video), *không rõ* (wiki không nêu / không có video), *n/a* (có điều kiện). `—` = không áp dụng.");
        sb.AppendLine("> Nhãn nguồn: `wiki:<Trang>#<Mục>` (StrategyWiki, bản Wayback — `ClaudeNotes/cvgp/wiki/CV_WIKI_DATA.md`), `video:Map_1 mm:ss` / `video:Boss_Map_1`, `video:planner` (số đo sơ bộ `ref/MAP1_TIMELINE.md` — routine ghi kèm sẽ đo lại), `đo:` (routine đo), `suy luận`, `quy ước`.");
        sb.AppendLine();

        var enemies = P["ConfigEnemy"];
        var bosses = P["ConfigBoss"];
        var items = P["ConfigItem"];
        int normal = enemies.rows.Cast<ConfigEnemyRow>().Count(r => r.kind == eEnemyKind.Normal);
        int sub = enemies.rows.Count - normal;
        int bossCount = bosses.rows.Cast<ConfigBossRow>().Select(r => r.id).Distinct().Count();
        int itemCount = items.rows.Cast<ConfigItemRow>().Count(r => r.kind != eItemKind.Container);
        int containers = items.rows.Count - itemCount;
        sb.AppendLine("**Đếm:** " + normal + " enemy thường + " + sub + " thực thể phụ · " + bossCount + " boss (" + bosses.rows.Count + " dòng pha) · "
                      + itemCount + " item + " + containers + " vật chứa · " + P["ConfigSubWeapon"].rows.Count + " vũ khí phụ · " + P["ConfigWhip"].rows.Count
                      + " cấp roi · " + P["ConfigBlock"].rows.Count + " block / " + P["ConfigStage"].rows.Count + " stage.");
        sb.AppendLine();

        sb.AppendLine("## 1. Simon Belmont");
        sb.AppendLine();
        Kv(sb, P["ConfigSimon"]);
        sb.AppendLine("### Sát thương Simon nhận mỗi lần trúng đòn (theo STAGE, mọi loại đòn như nhau — wiki Gameplay#Damage)");
        sb.AppendLine();
        sb.AppendLine("| Block | Stage | Vạch mất / đòn | Số đòn chết (16 vạch) | Giờ giới hạn | Boss | Nguồn |");
        sb.AppendLine("|---|---|---|---|---|---|---|");
        var blocks = P["ConfigBlock"];
        for (int i = 0; i < blocks.rows.Count; i++)
        {
            var b = (ConfigBlockRow)blocks.rows[i];
            int hits = (GameSession.MAX_HP + b.damageToSimon - 1) / b.damageToSimon;
            sb.AppendLine("| " + b.block + " | " + b.stages.Replace(";", ", ") + " | " + b.damageToSimon + " | " + hits + " | "
                          + Cell(blocks, i, "timeLimit") + " | " + b.boss + " | " + Esc(Src(blocks, i, "src_damage")) + "; giờ: " + Esc(Src(blocks, i, "src_timeLimit")) + " |");
        }
        sb.AppendLine();
        sb.AppendLine("Rơi hố/nước/gai = chết ngay; Second Quest (Stage 19+) mọi đòn 4 vạch (ConfigRule).");
        sb.AppendLine();

        sb.AppendLine("## 2. Enemy (15 loại thường + thực thể phụ)");
        sb.AppendLine();
        sb.AppendLine("Máu = **số đòn** để hạ: *yếu* (roi da, Holy Water, Dagger) / *mạnh* (roi xích ngắn/dài, Cross, Axe). Sát thương lên Simon: bảng §1 (theo stage).");
        sb.AppendLine();
        Table(sb, enemies, new[] { "id", "nameAlt", "sheetName", "kind", "stages", "hitsWeak", "hitsStrong", "score", "canDie", "projectile", "moveSpeed", "behaviorShort" },
            new[] { "Id (wiki)", "Tên khác", "Nhóm sheet", "Loại", "Stage", "Đòn yếu", "Đòn mạnh", "Điểm", "Chết được", "Đạn", "Tốc độ (px/tick)", "Hành vi" });

        sb.AppendLine("## 3. Boss");
        sb.AppendLine();
        Table(sb, bosses, new[] { "id", "phase", "stage", "entityCount", "hitsWeak", "hitsStrong", "hitsScope", "score", "stopwatchAffects", "weakPoint", "attacks", "itemsInRoom" },
            new[] { "Boss", "Pha", "Stage", "Số thực thể", "Đòn yếu", "Đòn mạnh", "Đòn tính cho", "Điểm", "Stopwatch tác dụng", "Điểm yếu", "Đòn tấn công", "Item trong phòng" });

        sb.AppendLine("## 4. Tham số hành vi đo được (enemy / boss / vũ khí / item)");
        sb.AppendLine();
        Table(sb, P["ConfigActorParam"], new[] { "actor", "key", "value", "min", "max", "unit", "note" },
            new[] { "Thực thể", "Tham số", "Giá trị", "Min", "Max", "Đơn vị", "Ghi chú" });

        sb.AppendLine("## 5. Item & vật chứa");
        sb.AppendLine();
        Table(sb, items, new[] { "id", "nameAlt", "kind", "hearts", "score", "healBars", "healFull", "effectTicks", "lifetimeTicks", "fallStyle", "effectShort" },
            new[] { "Item", "Tên khác", "Loại", "+Tim", "Điểm", "+Vạch máu", "Hồi đầy", "Hiệu lực (tick)", "Tồn tại (tick)", "Kiểu rơi", "Hiệu ứng" });

        sb.AppendLine("## 6. Vũ khí phụ");
        sb.AppendLine();
        Table(sb, P["ConfigSubWeapon"], new[] { "id", "heartCost", "strength", "pierces", "returns", "freezeTicks", "speedX", "speedY0", "gravity", "behaviorShort" },
            new[] { "Vũ khí", "Giá tim", "Nhóm", "Xuyên mục tiêu", "Quay về", "Đóng băng (tick)", "vx (px/tick)", "vy0 (px/tick)", "g (px/tick²)", "Đặc tính" });

        sb.AppendLine("## 7. Roi (Vampire Killer)");
        sb.AppendLine();
        Table(sb, P["ConfigWhip"], new[] { "level", "nameEn", "strength", "damageVsLeather", "spriteLenPx", "reachPx", "upgradeHearts", "colorCount" },
            new[] { "Cấp", "Tên", "Nhóm", "Sát thương so roi da", "Dài mảnh roi sheet (px)", "Tầm tay→đầu roi (px)", "Tim cần (wiki)", "Số màu" });

        sb.AppendLine("## 8. Block & Stage");
        sb.AppendLine();
        Table(sb, blocks, new[] { "block", "stages", "boss", "timeLimit", "damageToSimon", "bossMusic", "descriptionShort" },
            new[] { "Block", "Stage", "Boss", "Giờ", "Vạch/đòn", "Nhạc boss", "Mô tả" });
        Table(sb, P["ConfigStage"], new[] { "stage", "block", "areaShort", "enemies", "boss", "music" },
            new[] { "Stage", "Block", "Khu vực", "Enemy", "Boss", "Nhạc" });

        sb.AppendLine("## 9. Điểm, 1UP, tally");
        sb.AppendLine();
        Kv(sb, P["ConfigScore"]);
        sb.AppendLine("## 10. Luật chung, HUD, chuyển cảnh");
        sb.AppendLine();
        Kv(sb, P["ConfigRule"]);
        sb.AppendLine("## 11. Nội dung vật chứa (ConfigDrop)");
        sb.AppendLine();
        if (P["ConfigDrop"].rows.Count == 0)
            sb.AppendLine("*Chưa có dòng — routine 11/15 điền vị trí cụ thể (wiki chỉ nêu một phần: `CV_WIKI_DATA §6`).*");
        else
            Table(sb, P["ConfigDrop"], new[] { "objectId", "stage", "container", "item", "note" }, new[] { "Vật", "Stage", "Vật chứa", "Item", "Ghi chú" });
        sb.AppendLine();

        var full = Path.GetFullPath(Path.Combine(Application.dataPath, "..", REPORT_REL));
        var text = sb.ToString().Replace("\r\n", "\n");
        File.WriteAllText(full, text, new UTF8Encoding(false));
        Debug.Log("[CVGP Config] Đã ghi " + REPORT_REL + " (" + text.Length + " ký tự).");
        return true;
    }

    static int Col(Parsed p, string name)
    {
        return Array.IndexOf(p.header, name);
    }

    /// <summary>Nguồn của 1 ô (theo cùng luật với kiểm tra).</summary>
    static string Src(Parsed p, int row, string fieldOrSrc)
    {
        var colOf = new Dictionary<string, int>();
        for (int c = 0; c < p.header.Length; c++) colOf[p.header[c]] = c;
        if (fieldOrSrc.StartsWith("src_") || fieldOrSrc == "source")
        {
            var s = p.cells[row][colOf[fieldOrSrc]].Trim();
            return s.Length > 0 ? s : p.cells[row][colOf["source"]].Trim();
        }
        var f = p.spec.rowType.GetField(fieldOrSrc);
        return SourceOf(f, p.cells[row], colOf);
    }

    static string Cell(Parsed p, int row, string col)
    {
        var raw = p.cells[row][Col(p, col)].Trim();
        var f = p.spec.rowType.GetField(col);
        if (raw == "-")
            return "—";
        if (raw.Length == 0)
        {
            if (col == "min" || col == "max")
                return "";
            if (f != null && IsNumeric(f.FieldType))
                return "*" + PendingLabel(Src(p, row, col)) + "*";
            return "";
        }
        if (f != null && f.FieldType == typeof(bool))
            return ParseBool(raw) ? "có" : "không";
        bool isList = f != null && (f.GetCustomAttribute<CfgIntListAttribute>() != null || f.GetCustomAttribute<CfgEnumListAttribute>() != null);
        return Esc(isList ? raw.Replace(";", ", ") : raw);
    }

    static string PendingLabel(string src)
    {
        var m = PENDING_RX.Match(src);
        return m.Success ? m.Value : "chưa có";
    }

    static string Esc(string s)
    {
        return (s ?? "").Replace("|", "\\|").Replace("\n", " ");
    }

    /// <summary>Bảng rộng: cột chọn + cột "Nguồn" (source của dòng + mọi src_* có nội dung).</summary>
    static void Table(StringBuilder sb, Parsed p, string[] cols, string[] titles)
    {
        sb.AppendLine("| " + string.Join(" | ", titles) + " | Nguồn |");
        sb.AppendLine("|" + string.Concat(Enumerable.Repeat("---|", titles.Length + 1)));
        var srcCols = p.header.Where(h => h.StartsWith("src_")).ToArray();
        for (int i = 0; i < p.rows.Count; i++)
        {
            var cells = cols.Select(c => Cell(p, i, c));
            var src = new List<string>();
            int sc = Col(p, "source");
            if (sc >= 0 && p.cells[i][sc].Trim().Length > 0) src.Add(p.cells[i][sc].Trim());
            foreach (var c in srcCols)
            {
                var v = p.cells[i][Col(p, c)].Trim();
                if (v.Length > 0) src.Add("*" + c.Substring(4) + ":* " + v);
            }
            sb.AppendLine("| " + string.Join(" | ", cells) + " | " + Esc(string.Join(" · ", src)) + " |");
        }
        sb.AppendLine();
    }

    static void Kv(StringBuilder sb, Parsed p)
    {
        sb.AppendLine("| Khoá | Giá trị | Khoảng đo (min–max) | Đơn vị | Nguồn | Ghi chú |");
        sb.AppendLine("|---|---|---|---|---|---|");
        for (int i = 0; i < p.rows.Count; i++)
        {
            var r = (ConfigKvRow)p.rows[i];
            string range = r.min.has || r.max.has ? (r.min.has ? r.min.ToString() : "?") + "–" + (r.max.has ? r.max.ToString() : "?") : "";
            sb.AppendLine("| " + r.key + " | " + Cell(p, i, "value") + " | " + range + " | " + Esc(r.unit) + " | " + Esc(r.source) + " | " + Esc(r.note) + " |");
        }
        sb.AppendLine();
    }
}
