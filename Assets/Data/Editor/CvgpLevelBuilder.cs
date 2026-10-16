using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// CVGP 06 — dựng địa hình Map 1 (Level 1 = Stage 1–3) trong Unity từ dữ liệu python:
///   Assets/Data/Config/Level/Block1/Level1Tiles.json + &lt;Section&gt;_tiles.csv   (Tools/cvgp/build_tileset.py)
///   Assets/Data/Config/Level/Block1/Level1Map.json                             (Tools/cvgp/build_collision.py)
/// Ra: tileset level1_tiles.png cắt Multiple theo lưới 8×8 (sprite l1t_NNN, pivot tâm) · 118 Tile asset
/// Assets/Data/Art/Level/Block1/Tiles/l1t_NNN.asset · LevelSectionData Assets/Data/Config/Level/Block1/&lt;Section&gt;.asset ·
/// prefab Assets/Data/Prefabs/Level/Block1/&lt;Section&gt;.prefab (Grid ô 0.5 + Tilemap BG/FG + LevelSection).
/// Chạy lại được: mọi asset chỉ ghi khi nội dung khác (giữ GUID, chạy lần 2 = 0 ghi); có lỗi dữ liệu ⇒ không ghi gì.
/// CVGP 07: cùng đường dựng cho section thử Arena (Tools/cvgp/build_arena.py → Assets/Data/Config/Level/Arena/*.json) —
/// menu CVGP/Build Arena Section ⇒ Assets/Data/Config/Level/Arena/Arena.asset + Assets/Data/Prefabs/Level/Test/Arena.prefab
/// (dùng chung tileset + Tile asset của Block 1).
/// CVM2 05: tổng quát theo block — Spec mang thư mục Tile + tiền tố tên tile; Block 2 (Map 2, Stage 4–6) từ
/// Assets/Data/Config/Level/Block2/Level2Tiles.json + Level2Map.json (Tools/cvm2/build_tileset2.py + build_collision2.py) ⇒ tileset
/// level2_tiles.png (sprite l2t_NNN) · Tile Assets/Data/Art/Level/Block2/Tiles/l2t_NNN.asset · Assets/Data/Config/Level/Block2/&lt;Section&gt;.asset ·
/// Assets/Data/Prefabs/Level/Block2/&lt;Section&gt;.prefab — menu CVM2/Build Map2 Tilemaps (Cvm2LevelMenu). Block 1/Arena: đường dựng y hệt cũ.
/// </summary>
public static class CvgpLevelBuilder
{
    const string CFG_DIR = "Assets/Data/Config/Level/Block1";
    const string TILES_JSON = CFG_DIR + "/Level1Tiles.json";
    const string MAP_JSON = CFG_DIR + "/Level1Map.json";
    const string TILE_DIR = "Assets/Data/Art/Level/Block1/Tiles";
    const string PREFAB_DIR = "Assets/Data/Prefabs/Level/Block1";
    const string TILE_PREFIX = "l1t_";
    const float CELL_UNITS = 0.5f;   // tile 8 px / PPU 16
    const string ARENA_CFG_DIR = "Assets/Data/Config/Level/Arena";
    const string ARENA_PREFAB_DIR = "Assets/Data/Prefabs/Level/Test";

    /// <summary>Bộ đường dẫn của 1 lần dựng (Map 1, Arena, phòng boss thử, Map 2). tileDir/tilePrefix: Tile asset + tên sprite tileset.</summary>
    class Spec
    {
        public string label, tilesJson, mapJson, cfgDir, prefabDir;
        public string tileDir = TILE_DIR, tilePrefix = TILE_PREFIX;
    }

    static readonly Spec MAP1 = new Spec { label = "Map1", tilesJson = TILES_JSON, mapJson = MAP_JSON, cfgDir = CFG_DIR, prefabDir = PREFAB_DIR };
    static readonly Spec ARENA = new Spec
    {
        label = "Arena", tilesJson = ARENA_CFG_DIR + "/ArenaTiles.json", mapJson = ARENA_CFG_DIR + "/ArenaMap.json",
        cfgDir = ARENA_CFG_DIR, prefabDir = ARENA_PREFAB_DIR
    };

    /// <summary>CVM2 05: Map 2 = Block 2 (Stage 4–6) — tileset + Tile riêng (l2t_), dữ liệu + prefab ở thư mục Block2 (H12).</summary>
    const string MAP2_CFG_DIR = "Assets/Data/Config/Level/Block2";
    static readonly Spec MAP2 = new Spec
    {
        label = "Map2", tilesJson = MAP2_CFG_DIR + "/Level2Tiles.json", mapJson = MAP2_CFG_DIR + "/Level2Map.json",
        cfgDir = MAP2_CFG_DIR, prefabDir = "Assets/Data/Prefabs/Level/Block2",
        tileDir = "Assets/Data/Art/Level/Block2/Tiles", tilePrefix = "l2t_"
    };

    /// <summary>CVGP 19: 3 phòng boss thử (Tools/cvgp/build_boss_rooms.py — ghép ô Map 1) ⇒ Arena/Boss*.asset + Prefabs/Level/Test/Boss*.prefab.</summary>
    static readonly Spec BOSS_ROOMS = new Spec
    {
        label = "BossRooms", tilesJson = ARENA_CFG_DIR + "/BossRoomsTiles.json", mapJson = ARENA_CFG_DIR + "/BossRoomsMap.json",
        cfgDir = ARENA_CFG_DIR, prefabDir = ARENA_PREFAB_DIR
    };

    [Serializable] class TilesDoc { public int tileSize; public int columns; public int count; public string png; public int levelRowToNesY; public TilesSection[] sections; }
    [Serializable] class TilesSection { public string id; public int stage; public int[] sheetRect; public int width; public int height; public int cols8; public int rows8; public string csv; }
    [Serializable] class MapDoc { public int cellSize; public int levelRowToNesY; public string[] order; public MapSection[] sections; }
    [Serializable] class MapFg { public int[] cols; public int[] rows; public string src; }
    [Serializable] class MapStair { public string id; public string dir; public int steps; public int[] foot; public int[] top; public bool cutBottom; public bool cutTop; }
    [Serializable] class MapConn { public string id; public string kind; public string target; public int targetStage; public int[] trigger; public int triggerSimonX; public string stair; public string targetStair; public int[] targetOrigin; public int[] entry; public int dir; public string src; }
    [Serializable] class MapMarker { public string id; public string kind; public int[] rect; public string src; }
    [Serializable]
    class MapSection
    {
        public string id; public int stage; public int width; public int height; public int cols; public int rows;
        public int cameraMinX; public int cameraMaxX; public int bossLockCx;
        public bool hasSimonStart; public int[] simonStart; public int simonStartDir; public string simonStartSrc;
        public string[] collision; public MapFg[] foreground; public MapStair[] stairs; public MapConn[] connections;
        public MapMarker[] markers;
    }

    [MenuItem("CVGP/Build Map1 Tilemaps")]
    public static void BuildMenu()
    {
        Debug.Log(Build());
    }

    [MenuItem("CVGP/Build Arena Section")]
    public static void BuildArenaMenu()
    {
        Debug.Log(BuildArena());
    }

    [MenuItem("CVGP/Build Arena Boss Rooms")]
    public static void BuildBossRoomsMenu()
    {
        Debug.Log(BuildBossRooms());
    }

    /// <summary>Phòng boss thử (CVGP 19) — chạy SAU Build() (cần tileset + Tile asset Block 1).</summary>
    public static string BuildBossRooms()
    {
        return Build(BOSS_ROOMS);
    }

    /// <summary>Trả chuỗi tóm tắt; lỗi được log bằng Debug.LogError.</summary>
    public static string Build()
    {
        return Build(MAP1);
    }

    /// <summary>Section thử Arena (CVGP 07) — chạy SAU Build() (cần tileset + Tile asset Block 1).</summary>
    public static string BuildArena()
    {
        return Build(ARENA);
    }

    /// <summary>CVM2 05: địa hình Map 2 (Block 2) — menu CVM2/Build Map2 Tilemaps (Cvm2LevelMenu). Độc lập Block 1.</summary>
    public static string BuildMap2()
    {
        return Build(MAP2);
    }

    static string Build(Spec spec)
    {
        var errors = new List<string>();
        TilesDoc tiles = ReadJson<TilesDoc>(spec.tilesJson, errors);
        MapDoc map = ReadJson<MapDoc>(spec.mapJson, errors);
        if (errors.Count > 0)
            return Fail(spec, errors);

        // ---- kiểm dữ liệu trước khi ghi bất cứ gì
        if (tiles.tileSize != 8 || map.cellSize != LevelSectionData.CELL)
            errors.Add("tileSize/cellSize phải là 8/16");
        if (tiles.levelRowToNesY != LevelSectionData.SCREEN_Y || map.levelRowToNesY != LevelSectionData.SCREEN_Y)
            errors.Add("levelRowToNesY phải = LevelSectionData.SCREEN_Y (" + LevelSectionData.SCREEN_Y + ")");
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(tiles.png);
        int texRows = (tiles.count + tiles.columns - 1) / tiles.columns;
        if (tex == null)
            errors.Add("không tải được tileset " + tiles.png);
        else if (tex.width != tiles.columns * tiles.tileSize || tex.height != texRows * tiles.tileSize)
            errors.Add("tileset " + tex.width + "×" + tex.height + " khác mô tả");
        var grids = new Dictionary<string, int[,]>();
        var mapById = new Dictionary<string, MapSection>();
        foreach (var ms in map.sections)
            mapById[ms.id] = ms;
        foreach (var ts in tiles.sections)
        {
            MapSection ms;
            if (!mapById.TryGetValue(ts.id, out ms))
            {
                errors.Add(ts.id + ": thiếu trong Level1Map.json");
                continue;
            }
            if (ms.width != ts.width || ms.height != ts.height || ms.stage != ts.stage || ms.cols * 2 != ts.cols8 || ms.rows * 2 != ts.rows8)
                errors.Add(ts.id + ": kích thước/stage lệch giữa 2 JSON");
            grids[ts.id] = ReadCsv(ts.csv, ts.rows8, ts.cols8, tiles.count, errors);
            ValidateSection(ms, errors);
        }
        if (map.sections.Length != tiles.sections.Length)
            errors.Add("số section lệch giữa 2 JSON");
        if (errors.Count > 0)
            return Fail(spec, errors);

        var sb = new StringBuilder(spec.label + ": ");
        // ---- 1. tileset Multiple
        bool texChanged = EnsureTileset(spec, tiles, texRows);
        sb.Append("tileset ").Append(texChanged ? "cắt lại" : "không đổi");
        var sprites = new Sprite[tiles.count];
        foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(tiles.png))
        {
            var sp = o as Sprite;
            int idx;
            if (sp != null && sp.name.StartsWith(spec.tilePrefix) && int.TryParse(sp.name.Substring(spec.tilePrefix.Length), out idx) && idx >= 0 && idx < tiles.count)
                sprites[idx] = sp;
        }
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] == null)
                errors.Add("thiếu sprite " + TileName(spec, i));
        if (errors.Count > 0)
            return Fail(spec, errors);

        // ---- 2. Tile asset
        EnsureFolder(spec.tileDir);
        var tileAssets = new Tile[tiles.count];
        int tileWritten = 0;
        for (int i = 0; i < tiles.count; i++)
        {
            string path = spec.tileDir + "/" + TileName(spec, i) + ".asset";
            var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (t == null)
            {
                t = ScriptableObject.CreateInstance<Tile>();
                SetupTile(t, sprites[i]);
                AssetDatabase.CreateAsset(t, path);
                tileWritten++;
            }
            else if (t.sprite != sprites[i] || t.colliderType != Tile.ColliderType.None || t.color != Color.white
                     || t.flags != TileFlags.LockColor || t.transform != Matrix4x4.identity || t.gameObject != null)
            {
                SetupTile(t, sprites[i]);
                EditorUtility.SetDirty(t);
                tileWritten++;
            }
            tileAssets[i] = t;
        }
        sb.Append("; tile ").Append(tiles.count).Append(" (ghi ").Append(tileWritten).Append(")");

        // ---- 3. LevelSectionData + prefab
        EnsureFolder(spec.cfgDir);
        EnsureFolder(spec.prefabDir);
        int dataWritten = 0, prefabWritten = 0;
        foreach (var ts in tiles.sections)
        {
            var ms = mapById[ts.id];
            var data = WriteSectionData(spec, ms, ref dataWritten);
            if (WritePrefab(spec, ms, data, grids[ts.id], tileAssets))
                prefabWritten++;
        }
        AssetDatabase.SaveAssets();
        sb.Append("; section ").Append(tiles.sections.Length).Append(" (data ghi ").Append(dataWritten)
          .Append(", prefab ghi ").Append(prefabWritten).Append(")");
        return sb.ToString();
    }

    // ------------------------------------------------------------------ kiểm dữ liệu
    static void ValidateSection(MapSection ms, List<string> errors)
    {
        if (ms.collision == null || ms.collision.Length != ms.rows)
            errors.Add(ms.id + ": collision phải có " + ms.rows + " hàng");
        else
            for (int r = 0; r < ms.rows; r++)
            {
                if (ms.collision[r].Length != ms.cols)
                    errors.Add(ms.id + ": hàng " + r + " dài " + ms.collision[r].Length + " ≠ " + ms.cols);
                foreach (char c in ms.collision[r])
                {
                    eTileCollision t;
                    if (!LevelSectionData.TryFromChar(c, out t))
                        errors.Add(ms.id + ": ký tự lưới lạ '" + c + "' hàng " + r);
                }
            }
        if (ms.cameraMinX != 0 || ms.cameraMaxX != ms.width - NesCoord.SCREEN_W)
            errors.Add(ms.id + ": biên camera phải [0, rộng-256]");
        if (ms.simonStart == null || ms.simonStart.Length != 2)
            errors.Add(ms.id + ": simonStart phải 2 số");
        foreach (var s in ms.stairs ?? new MapStair[0])
        {
            if (s.dir != "UpRight" && s.dir != "UpLeft")
                errors.Add(ms.id + "/" + s.id + ": dir lạ " + s.dir);
            if (s.foot == null || s.foot.Length != 2 || s.top == null || s.top.Length != 2 || s.steps < 1)
                errors.Add(ms.id + "/" + s.id + ": foot/top/steps sai");
        }
        foreach (var c in ms.connections ?? new MapConn[0])
        {
            eLevelLink k;
            if (!TryEnum(c.kind, out k))
                errors.Add(ms.id + "/" + c.id + ": kind lạ " + c.kind);
            if (c.trigger == null || c.trigger.Length != 4 || c.targetOrigin == null || c.targetOrigin.Length != 2 || c.entry == null || c.entry.Length != 2)
                errors.Add(ms.id + "/" + c.id + ": trigger/targetOrigin/entry sai độ dài");
            if (c.dir < -1 || c.dir > 1)
                errors.Add(ms.id + "/" + c.id + ": dir phải −1/0/+1");
        }
        if (ms.simonStartDir < -1 || ms.simonStartDir > 1)
            errors.Add(ms.id + ": simonStartDir phải −1/0/+1");
        foreach (var m in ms.markers ?? new MapMarker[0])
            if (string.IsNullOrEmpty(m.id) || string.IsNullOrEmpty(m.kind) || m.rect == null || m.rect.Length != 4 || m.rect[2] <= 0 || m.rect[3] <= 0
                || m.rect[0] < 0 || m.rect[1] < 0 || m.rect[0] + m.rect[2] > ms.width || m.rect[1] + m.rect[3] > ms.height)
                errors.Add(ms.id + "/" + m.id + ": marker sai");
        foreach (var f in ms.foreground ?? new MapFg[0])
            if (f.cols == null || f.cols.Length != 2 || f.rows == null || f.rows.Length != 2
                || f.cols[0] < 0 || f.cols[1] >= ms.cols || f.rows[0] < 0 || f.rows[1] >= ms.rows || f.cols[0] > f.cols[1] || f.rows[0] > f.rows[1])
                errors.Add(ms.id + ": foreground ngoài lưới");
    }

    static int[,] ReadCsv(string path, int rows, int cols, int count, List<string> errors)
    {
        var g = new int[rows, cols];
        if (!File.Exists(path))
        {
            errors.Add("không thấy " + path);
            return g;
        }
        var lines = File.ReadAllLines(path);
        if (lines.Length != rows)
            errors.Add(path + ": " + lines.Length + " hàng ≠ " + rows);
        for (int r = 0; r < Math.Min(rows, lines.Length); r++)
        {
            var parts = lines[r].Split(',');
            if (parts.Length != cols)
            {
                errors.Add(path + ":" + (r + 1) + " có " + parts.Length + " cột ≠ " + cols);
                continue;
            }
            for (int c = 0; c < cols; c++)
            {
                int v;
                if (!int.TryParse(parts[c], out v) || v < 0 || v >= count)
                    errors.Add(path + ":" + (r + 1) + " cột " + c + " chỉ số tile sai '" + parts[c] + "'");
                g[r, c] = v;
            }
        }
        return g;
    }

    // ------------------------------------------------------------------ tileset
    static bool EnsureTileset(Spec spec, TilesDoc tiles, int texRows)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(tiles.png);
        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (importer.spriteImportMode != SpriteImportMode.Multiple) { importer.spriteImportMode = SpriteImportMode.Multiple; changed = true; }
        if (!Mathf.Approximately(importer.spritePixelsPerUnit, NesCoord.PPU)) { importer.spritePixelsPerUnit = NesCoord.PPU; changed = true; }
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
        if (importer.wrapMode != TextureWrapMode.Clamp) { importer.wrapMode = TextureWrapMode.Clamp; changed = true; }
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        if (settings.spriteMeshType != SpriteMeshType.FullRect)
        {
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            changed = true;
        }
        if (changed)
            importer.SaveAndReimport();

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dp = factory.GetSpriteEditorDataProviderFromObject(importer);
        dp.InitSpriteEditorDataProvider();
        var want = new SpriteRect[tiles.count];
        int ts = tiles.tileSize;
        for (int i = 0; i < tiles.count; i++)
        {
            int r = i / tiles.columns, c = i % tiles.columns;
            want[i] = new SpriteRect
            {
                name = TileName(spec, i),
                rect = new Rect(c * ts, (texRows - 1 - r) * ts, ts, ts),   // Unity: gốc dưới-trái
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = Vector4.zero,
                spriteID = DeterministicGuid(tiles.png + "#" + TileName(spec, i)),
            };
        }
        var have = dp.GetSpriteRects();
        bool same = have != null && have.Length == want.Length;
        for (int i = 0; same && i < want.Length; i++)
            same = have[i].name == want[i].name && have[i].rect == want[i].rect && have[i].alignment == want[i].alignment
                   && have[i].pivot == want[i].pivot && have[i].spriteID == want[i].spriteID;
        if (same)
            return changed;
        dp.SetSpriteRects(want);
        var names = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (names != null)
        {
            var pairs = new List<SpriteNameFileIdPair>();
            foreach (var s in want)
                pairs.Add(new SpriteNameFileIdPair(s.name, s.spriteID));
            names.SetNameFileIdPairs(pairs);
        }
        dp.Apply();
        importer.SaveAndReimport();
        return true;
    }

    static GUID DeterministicGuid(string key)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var h = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
            var sb = new StringBuilder(32);
            foreach (var b in h)
                sb.Append(b.ToString("x2"));
            return new GUID(sb.ToString());
        }
    }

    static void SetupTile(Tile t, Sprite s)
    {
        t.sprite = s;
        t.color = Color.white;
        t.transform = Matrix4x4.identity;
        t.flags = TileFlags.LockColor;
        t.gameObject = null;
        t.colliderType = Tile.ColliderType.None;   // va chạm gameplay = lưới dữ liệu riêng (H4), không TilemapCollider2D
    }

    static string TileName(Spec spec, int i)
    {
        return spec.tilePrefix + i.ToString("000");
    }

    // ------------------------------------------------------------------ LevelSectionData
    static LevelSectionData WriteSectionData(Spec spec, MapSection ms, ref int written)
    {
        string path = spec.cfgDir + "/" + ms.id + ".asset";
        var temp = ScriptableObject.CreateInstance<LevelSectionData>();
        temp.name = ms.id;
        temp.id = ms.id;
        temp.stage = ms.stage;
        temp.width = ms.width;
        temp.height = ms.height;
        temp.cols = ms.cols;
        temp.rows = ms.rows;
        temp.cameraMinX = ms.cameraMinX;
        temp.cameraMaxX = ms.cameraMaxX;
        temp.bossLockCx = ms.bossLockCx;
        temp.hasSimonStart = ms.hasSimonStart;
        temp.simonStart = V2(ms.simonStart);
        temp.simonStartDir = ms.simonStartDir;     // CVM2 05 — Block 1 không có khoá ⇒ 0 (như trường chưa có trong .asset cũ)
        temp.simonStartSrc = ms.simonStartSrc;
        temp.collision = ms.collision;
        var fgs = new List<LevelSectionData.CellRange>();
        foreach (var f in ms.foreground ?? new MapFg[0])
            fgs.Add(new LevelSectionData.CellRange { col0 = f.cols[0], col1 = f.cols[1], row0 = f.rows[0], row1 = f.rows[1], src = f.src });
        temp.foreground = fgs.ToArray();
        var st = new List<LevelSectionData.Stair>();
        foreach (var s in ms.stairs ?? new MapStair[0])
        {
            eStairDir d;
            TryEnum(s.dir, out d);
            st.Add(new LevelSectionData.Stair { id = s.id, dir = d, steps = s.steps, foot = V2(s.foot), top = V2(s.top), cutBottom = s.cutBottom, cutTop = s.cutTop });
        }
        temp.stairs = st.ToArray();
        var cn = new List<LevelSectionData.Connection>();
        foreach (var c in ms.connections ?? new MapConn[0])
        {
            eLevelLink k;
            TryEnum(c.kind, out k);
            cn.Add(new LevelSectionData.Connection
            {
                id = c.id, kind = k, target = c.target ?? "", targetStage = c.targetStage,
                trigger = new RectInt(c.trigger[0], c.trigger[1], c.trigger[2], c.trigger[3]),
                triggerSimonX = c.triggerSimonX, stair = c.stair ?? "", targetStair = c.targetStair ?? "",
                targetOrigin = V2(c.targetOrigin), entry = V2(c.entry), dir = c.dir, src = c.src ?? ""
            });
        }
        temp.connections = cn.ToArray();
        var mk = new List<LevelSectionData.Marker>();
        foreach (var m in ms.markers ?? new MapMarker[0])
            mk.Add(new LevelSectionData.Marker { id = m.id, kind = m.kind, rect = new RectInt(m.rect[0], m.rect[1], m.rect[2], m.rect[3]), src = m.src ?? "" });
        temp.markers = mk.ToArray();

        var existing = AssetDatabase.LoadAssetAtPath<LevelSectionData>(path);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(temp, path);
            written++;
            return temp;
        }
        if (EditorJsonUtility.ToJson(existing) != EditorJsonUtility.ToJson(temp))
        {
            EditorUtility.CopySerialized(temp, existing);
            EditorUtility.SetDirty(existing);
            written++;
        }
        UnityEngine.Object.DestroyImmediate(temp);
        return existing;
    }

    // ------------------------------------------------------------------ prefab
    /// <summary>Trả true nếu đã ghi prefab.</summary>
    static bool WritePrefab(Spec spec, MapSection ms, LevelSectionData data, int[,] grid, Tile[] tileAssets)
    {
        string path = spec.prefabDir + "/" + ms.id + ".prefab";
        bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
        var root = exists ? PrefabUtility.LoadPrefabContents(path) : new GameObject(ms.id);
        int changes = exists ? 0 : 1;
        try
        {
            if (root.transform.localPosition != Vector3.zero || root.transform.localRotation != Quaternion.identity || root.transform.localScale != Vector3.one)
            {
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;
                root.transform.localScale = Vector3.one;
                changes++;
            }
            var gridComp = GetOrAdd<Grid>(root, ref changes);
            var cell = new Vector3(CELL_UNITS, CELL_UNITS, 0f);
            if (gridComp.cellSize != cell || gridComp.cellGap != Vector3.zero || gridComp.cellLayout != GridLayout.CellLayout.Rectangle
                || gridComp.cellSwizzle != GridLayout.CellSwizzle.XYZ)
            {
                gridComp.cellSize = cell;
                gridComp.cellGap = Vector3.zero;
                gridComp.cellLayout = GridLayout.CellLayout.Rectangle;
                gridComp.cellSwizzle = GridLayout.CellSwizzle.XYZ;
                changes++;
            }
            var section = GetOrAdd<LevelSection>(root, ref changes);

            int rows8 = grid.GetLength(0), cols8 = grid.GetLength(1);
            var bgWant = new Tile[rows8, cols8];
            for (int r = 0; r < rows8; r++)
                for (int c = 0; c < cols8; c++)
                    bgWant[r, c] = tileAssets[grid[r, c]];
            var bg = EnsureTilemap(root, "BG", "Background", 0, bgWant, ref changes);

            Tilemap fg = null;
            bool hasFg = ms.foreground != null && ms.foreground.Length > 0;
            var fgTr = root.transform.Find("FG");
            if (hasFg)
            {
                var fgWant = new Tile[rows8, cols8];
                foreach (var f in ms.foreground)
                    for (int r = f.rows[0] * 2; r <= f.rows[1] * 2 + 1; r++)
                        for (int c = f.cols[0] * 2; c <= f.cols[1] * 2 + 1; c++)
                            fgWant[r, c] = tileAssets[grid[r, c]];
                fg = EnsureTilemap(root, "FG", "Foreground", 0, fgWant, ref changes);
            }
            else if (fgTr != null)
            {
                UnityEngine.Object.DestroyImmediate(fgTr.gameObject);
                changes++;
            }

            if (section.data != data || section.bg != bg || section.fg != fg)
            {
                section.data = data;
                section.bg = bg;
                section.fg = fg;
                changes++;
            }
            if (changes == 0)
                return false;
            PrefabUtility.SaveAsPrefabAsset(root, path);
            return true;
        }
        finally
        {
            if (exists)
                PrefabUtility.UnloadPrefabContents(root);
            else
                UnityEngine.Object.DestroyImmediate(root);
        }
    }

    /// <summary>Tilemap con "name": tile (hàng r từ trên, cột c) đặt ở ô (c, −r−1) ⇒ góc trên-trái tile = px (8c, 8r) của section.</summary>
    static Tilemap EnsureTilemap(GameObject root, string name, string sortingLayer, int order, Tile[,] want, ref int changes)
    {
        var tr = root.transform.Find(name);
        GameObject go;
        if (tr == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            changes++;
        }
        else
            go = tr.gameObject;
        if (go.transform.localPosition != Vector3.zero || go.transform.localRotation != Quaternion.identity || go.transform.localScale != Vector3.one)
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            changes++;
        }
        var tm = GetOrAdd<Tilemap>(go, ref changes);
        var tr2 = GetOrAdd<TilemapRenderer>(go, ref changes);
        if (tm.tileAnchor != new Vector3(0.5f, 0.5f, 0f) || tm.color != Color.white || tm.orientation != Tilemap.Orientation.XY)
        {
            tm.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
            tm.color = Color.white;
            tm.orientation = Tilemap.Orientation.XY;
            changes++;
        }
        if (tr2.sortingLayerName != sortingLayer || tr2.sortingOrder != order || tr2.mode != TilemapRenderer.Mode.Chunk)
        {
            tr2.sortingLayerName = sortingLayer;
            tr2.sortingOrder = order;
            tr2.mode = TilemapRenderer.Mode.Chunk;
            changes++;
        }
        int rows8 = want.GetLength(0), cols8 = want.GetLength(1);
        // ô thừa ngoài lưới mong muốn ⇒ xoá
        var bounds = tm.cellBounds;
        foreach (var p in bounds.allPositionsWithin)
        {
            int c = p.x, r = -p.y - 1;
            bool inside = c >= 0 && c < cols8 && r >= 0 && r < rows8 && p.z == 0;
            if (!inside && tm.GetTile(p) != null)
            {
                tm.SetTile(p, null);
                changes++;
            }
        }
        for (int r = 0; r < rows8; r++)
            for (int c = 0; c < cols8; c++)
            {
                var pos = new Vector3Int(c, -r - 1, 0);
                if (tm.GetTile(pos) != want[r, c])
                {
                    tm.SetTile(pos, want[r, c]);
                    changes++;
                }
            }
        var before = tm.cellBounds;
        tm.CompressBounds();
        if (tm.cellBounds != before)
            changes++;
        return tm;
    }

    // ------------------------------------------------------------------ tiện ích
    static T GetOrAdd<T>(GameObject go, ref int changes) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c != null)
            return c;
        changes++;
        return go.AddComponent<T>();
    }

    static bool TryEnum<T>(string s, out T v) where T : struct
    {
        return Enum.TryParse(s, false, out v) && Enum.IsDefined(typeof(T), v);
    }

    static Vector2Int V2(int[] a)
    {
        return a == null || a.Length < 2 ? Vector2Int.zero : new Vector2Int(a[0], a[1]);
    }

    static T ReadJson<T>(string path, List<string> errors) where T : class
    {
        if (!File.Exists(path))
        {
            errors.Add("không thấy " + path + " (chạy Tools/cvgp/build_tileset.py + build_collision.py)");
            return null;
        }
        try
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            errors.Add(path + ": " + e.Message);
            return null;
        }
    }

    static string Fail(Spec spec, List<string> errors)
    {
        foreach (var e in errors)
            Debug.LogError("CvgpLevelBuilder: " + e);
        return spec.label + ": " + errors.Count + " lỗi — KHÔNG ghi asset nào";
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
