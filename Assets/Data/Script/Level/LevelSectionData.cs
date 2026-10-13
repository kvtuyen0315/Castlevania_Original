using System;
using UnityEngine;

/// <summary>
/// Dữ liệu 1 section địa hình (= 1 vùng của sheet Level 1): lưới va chạm 16×16, cầu thang, nối section, biên camera, điểm bắt đầu stage.
/// Sinh bởi menu CVGP/Build Map1 Tilemaps (CvgpLevelBuilder) từ Assets/Data/Config/Level/Block1/Level1Map.json
/// (Tools/cvgp/build_collision.py + level1_collision_overrides.json + level1_sections.json) — KHÔNG sửa tay asset.
/// Toạ độ = px trong section: gốc góc trên-trái vùng 192 dòng, x phải, y xuống. Dòng r ↔ NES y = SCREEN_Y + r.
/// </summary>
public class LevelSectionData : ScriptableObject
{
    public const int CELL = 16;
    /// <summary>Dòng 0 của section nằm ở NES y 32 (16 dòng đầu dưới HUD) — ZNCC 4 ảnh ref Map_1, offset 28–36 thử, 32 tốt nhất (CVGP 06).</summary>
    public const int SCREEN_Y = 32;
    /// <summary>Ký tự lưới theo thứ tự eTileCollision.</summary>
    public const string TYPE_CHARS = ".#S=~PDX!";

    [Serializable]
    public class Stair
    {
        public string id;
        public eStairDir dir;
        public int steps;
        public Vector2Int foot;   // góc trên-trái vạch bậc THẤP nhất (mặt bậc ở y này; bậc cao 8 px)
        public Vector2Int top;    // góc trên-trái vạch bậc CAO nhất
        public bool cutBottom;    // bậc thấp nhất chạm mép dưới section (thang đi tiếp sang section dưới)
        public bool cutTop;       // bậc cao nhất chạm mép trên section

        /// <summary>Góc trên-trái bậc thứ k (0 = chân).</summary>
        public Vector2Int Tread(int k)
        {
            return new Vector2Int(foot.x + (dir == eStairDir.UpRight ? 8 : -8) * k, foot.y - 8 * k);
        }
    }

    [Serializable]
    public class Connection
    {
        public string id;
        public eLevelLink kind;
        public string target;          // id section đích ("" = không nối trong Map 1)
        public int targetStage;
        public RectInt trigger;        // px section: cột cửa / vòm cổng / hình bao các bậc thang
        public int triggerSimonX;      // cửa: tâm x Simon khi chuỗi qua cửa bắt đầu (đo video), -1 = không dùng
        public string stair;           // thang nguồn (kind Stair)
        public string targetStair;
        public Vector2Int targetOrigin; // gốc section đích tính trong toạ độ section nguồn: đích(x,y) = nguồn(x,y) - targetOrigin
        public Vector2Int entry;       // vị trí Simon trong section đích (x tâm, y mặt sàn) — cửa/cổng
        [TextArea] public string src;
    }

    [Serializable]
    public class CellRange
    {
        public int col0, col1, row0, row1;   // ô 16×16, gồm cả hai đầu
        [TextArea] public string src;
    }

    public string id;
    public int stage;
    public int width;
    public int height;
    public int cols;
    public int rows;
    public int cameraMinX;
    public int cameraMaxX;
    public int bossLockCx = -1;            // -1 = không khoá
    public bool hasSimonStart;
    public Vector2Int simonStart;          // x tâm thân, y mặt sàn
    [TextArea] public string simonStartSrc;
    public string[] collision;             // hàng ô 16×16 từ trên xuống, 1 ký tự/ô (TYPE_CHARS)
    public CellRange[] foreground;         // vùng tile vẽ lại ở lớp FG (đè nhân vật)
    public Stair[] stairs;
    public Connection[] connections;

    [NonSerialized] eTileCollision[] cache;

    void OnEnable()
    {
        cache = null;
    }

    void OnValidate()
    {
        cache = null;
    }

    public static char ToChar(eTileCollision t)
    {
        return TYPE_CHARS[(int)t];
    }

    public static bool TryFromChar(char c, out eTileCollision t)
    {
        int i = TYPE_CHARS.IndexOf(c);
        t = i < 0 ? eTileCollision.Empty : (eTileCollision)i;
        return i >= 0;
    }

    /// <summary>Loại ô (cột, hàng 16×16); ngoài lưới = Empty.</summary>
    public eTileCollision GetCell(int col, int row)
    {
        if (col < 0 || row < 0 || col >= cols || row >= rows)
            return eTileCollision.Empty;
        if (cache == null)
            BuildCache();
        return cache[row * cols + col];
    }

    /// <summary>Loại ô chứa điểm px (x, y) của section.</summary>
    public eTileCollision GetAt(int x, int y)
    {
        return GetCell(FloorDiv(x, CELL), FloorDiv(y, CELL));
    }

    public Stair FindStair(string stairId)
    {
        if (stairs != null)
            foreach (var s in stairs)
                if (s.id == stairId)
                    return s;
        return null;
    }

    public Connection FindConnection(string connectionId)
    {
        if (connections != null)
            foreach (var c in connections)
                if (c.id == connectionId)
                    return c;
        return null;
    }

    void BuildCache()
    {
        cache = new eTileCollision[cols * rows];
        for (int r = 0; r < rows && collision != null && r < collision.Length; r++)
            for (int c = 0; c < cols && c < collision[r].Length; c++)
            {
                eTileCollision t;
                if (!TryFromChar(collision[r][c], out t))
                    Debug.LogError(name + ": ký tự lưới lạ '" + collision[r][c] + "' ở (" + c + "," + r + ")");
                cache[r * cols + c] = t;
            }
    }

    static int FloorDiv(int a, int b)
    {
        return a >= 0 ? a / b : -((-a + b - 1) / b);
    }
}
