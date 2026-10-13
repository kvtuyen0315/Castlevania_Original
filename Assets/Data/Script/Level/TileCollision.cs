using UnityEngine;

/// <summary>
/// Đường đi của 1 cầu thang (CVGP 07) — đường chéo 45° qua TÂM MẶT các bậc 8×8 (LevelSectionData.Stair, CVGP 06):
/// tâm bậc k = (foot.x + 4 + 8·k·upDx, foot.y − 8·k). Bậc cao nhất ngang mặt tầng trên (EH stair0: bậc 7 y 112 = bục y 112),
/// bậc thấp nhất cao hơn sàn dưới 8 px ⇒ điểm CHÂN = "bậc −1" nằm trên sàn dưới, điểm ĐỈNH = bậc steps−1 trên tầng trên.
/// PointAt(t): t = số px đã lên tính từ chân (ngang = dọc vì 45°). x là tâm bậc — lệch tâm thân Simon (nếu video khác)
/// do routine 08 thêm hằng riêng, không sửa đường này.
/// </summary>
public struct StairPath
{
    public int index;
    public LevelSectionData.Stair stair;
    /// <summary>+1 = thang ↗ (đi lên sang phải), −1 = ↖.</summary>
    public int upDx;
    /// <summary>Điểm chân: (x tâm bậc ảo k = −1, y = mặt sàn dưới).</summary>
    public Vector2Int bottom;
    /// <summary>Điểm đỉnh: (x tâm bậc cao nhất, y = mặt bậc cao nhất = mặt tầng trên).</summary>
    public Vector2Int top;

    public int Length => bottom.y - top.y;

    public Vector2Int PointAt(int t)
    {
        return new Vector2Int(bottom.x + upDx * t, bottom.y - t);
    }

    /// <summary>t của điểm (x, y) nếu nằm trên đường (0..Length), không thì −1.</summary>
    public int ParamOf(int x, int y)
    {
        int t = bottom.y - y;
        if (t < 0 || t > Length)
            return -1;
        return bottom.x + upDx * t == x ? t : -1;
    }

    public static StairPath From(LevelSectionData.Stair s, int index)
    {
        int up = s.dir == eStairDir.UpRight ? 1 : -1;
        return new StairPath
        {
            index = index,
            stair = s,
            upDx = up,
            bottom = new Vector2Int(s.foot.x + 4 - 8 * up, s.foot.y + 8),
            top = new Vector2Int(s.foot.x + 4 + 8 * up * (s.steps - 1), s.foot.y - 8 * (s.steps - 1)),
        };
    }
}

/// <summary>
/// Va chạm địa hình lúc chạy cho 1 section (CVGP 07) — đọc lưới 16×16 của LevelSectionData (CVGP 06), giữ trạng thái khối đã phá.
/// Quy ước toạ độ: px section (gốc trên-trái vùng 192 dòng, y xuống). Ngoài lưới: trái/phải = Solid (mép section là tường),
/// trên = Empty (nhảy lên trên được), dưới = Empty (rơi khỏi đáy ⇒ CheckDeadly trả Pit khi hộp ra hẳn khỏi đáy).
/// Đặc (IsSolid) = Solid, StairFloor, Breakable chưa vỡ. Chết = Water, Pit, Hazard. Door KHÔNG đặc (CVGP 15: video Map_1 f2551/f4919
/// Simon đứng ở x 1516 / 492 — thân 16 px đã lấn ô cửa 16×16 (1520 / 496) khi chuỗi qua cửa bắt đầu ⇒ cửa là vùng kích hoạt, không phải tường).
/// Di chuyển (MoveX/MoveY) chặn TỪNG PX nên không xuyên tường ở mọi tốc độ; chạm tường/sàn ⇒ dừng sát, phần lẻ = 0.
/// Đáp sàn chỉ ở MẶT TRÊN ô đặc (ô ngay trên không đặc) ⇒ đi xuyên từ dưới lên (collideCeiling=false) thì rơi lại đúng mặt trên.
/// </summary>
public class TileCollision
{
    public const int CELL = LevelSectionData.CELL;

    public readonly LevelSectionData data;
    private readonly int cols, rows;
    private readonly eTileCollision[] cells;
    private readonly bool[] broken;
    private readonly StairPath[] stairs;

    public int Width { get; }
    public int Height { get; }
    /// <summary>CVGP 14 — tường vô hình (px): cột x &lt; WallMinX hoặc ≥ WallMaxX coi là đặc khi đi ngang (BossArena: mép màn khi camera khoá).
    /// Dựng lại mỗi LoadSection ⇒ mặc định không có.</summary>
    public int WallMinX = int.MinValue, WallMaxX = int.MaxValue;
    public int StairCount => stairs.Length;

    public TileCollision(LevelSectionData data)
    {
        this.data = data;
        cols = data.cols;
        rows = data.rows;
        Width = data.width;
        Height = data.height;
        cells = new eTileCollision[cols * rows];
        broken = new bool[cols * rows];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                cells[r * cols + c] = data.GetCell(c, r);
        int n = data.stairs != null ? data.stairs.Length : 0;
        stairs = new StairPath[n];
        for (int i = 0; i < n; i++)
            stairs[i] = StairPath.From(data.stairs[i], i);
    }

    // ---------------------------------------------------------------- tra ô

    public eTileCollision CellType(int col, int row)
    {
        if (col < 0 || col >= cols)
            return eTileCollision.Solid;
        if (row < 0 || row >= rows)
            return eTileCollision.Empty;
        int i = row * cols + col;
        var t = cells[i];
        if (t == eTileCollision.Breakable && broken[i])
            return eTileCollision.Empty;
        return t;
    }

    public eTileCollision TypeAt(int x, int y)
    {
        return CellType(FloorDiv(x, CELL), FloorDiv(y, CELL));
    }

    public static bool IsSolidType(eTileCollision t)
    {
        return t == eTileCollision.Solid || t == eTileCollision.StairFloor || t == eTileCollision.Breakable;
    }

    public static bool IsDeadlyType(eTileCollision t)
    {
        return t == eTileCollision.Water || t == eTileCollision.Pit || t == eTileCollision.Hazard;
    }

    public bool IsSolid(int x, int y)
    {
        return IsSolidType(TypeAt(x, y));
    }

    /// <summary>Có pixel đặc nào trên cột x, hàng y0..y1 (gồm 2 đầu).</summary>
    public bool ColumnHitsSolid(int x, int y0, int y1)
    {
        if (x < WallMinX || x >= WallMaxX)
            return true;
        int c = FloorDiv(x, CELL);
        int r1 = FloorDiv(y1, CELL);
        for (int r = FloorDiv(y0, CELL); r <= r1; r++)
            if (IsSolidType(CellType(c, r)))
                return true;
        return false;
    }

    /// <summary>Có pixel đặc nào trên hàng y, cột x0..x1 (gồm 2 đầu).</summary>
    public bool RowHitsSolid(int y, int x0, int x1)
    {
        int r = FloorDiv(y, CELL);
        int c1 = FloorDiv(x1, CELL);
        for (int c = FloorDiv(x0, CELL); c <= c1; c++)
            if (IsSolidType(CellType(c, r)))
                return true;
        return false;
    }

    /// <summary>Hàng y có MẶT TRÊN ô đặc nào trong cột x0..x1 (ô chứa y đặc, ô chứa y−1 không đặc).</summary>
    public bool RowLandable(int y, int x0, int x1)
    {
        int r = FloorDiv(y, CELL);
        int rAbove = FloorDiv(y - 1, CELL);
        int c1 = FloorDiv(x1, CELL);
        for (int c = FloorDiv(x0, CELL); c <= c1; c++)
            if (rAbove != r && IsSolidType(CellType(c, r)) && !IsSolidType(CellType(c, rAbove)))
                return true;
        return false;
    }

    public bool BoxHitsSolid(RectInt b)
    {
        int c1 = FloorDiv(b.xMax - 1, CELL), r1 = FloorDiv(b.yMax - 1, CELL);
        for (int r = FloorDiv(b.y, CELL); r <= r1; r++)
            for (int c = FloorDiv(b.x, CELL); c <= c1; c++)
                if (IsSolidType(CellType(c, r)))
                    return true;
        return false;
    }

    /// <summary>Đứng trên mặt sàn: hàng Y (ngay dưới hộp) là mặt trên ô đặc trong bề rộng hộp.</summary>
    public bool IsOnGround(Entity e)
    {
        var b = e.BodyRect;
        return RowLandable(b.yMax, b.xMin, b.xMax - 1);
    }

    /// <summary>Hộp chạm ô chết (Water/Pit/Hazard) hoặc đã ra hẳn khỏi đáy section (⇒ Pit).</summary>
    public bool CheckDeadly(RectInt b, out eTileCollision kind)
    {
        kind = eTileCollision.Empty;
        if (b.y >= Height)
        {
            kind = eTileCollision.Pit;
            return true;
        }
        int c1 = FloorDiv(b.xMax - 1, CELL), r1 = FloorDiv(b.yMax - 1, CELL);
        for (int r = FloorDiv(b.y, CELL); r <= r1; r++)
            for (int c = FloorDiv(b.x, CELL); c <= c1; c++)
            {
                var t = CellType(c, r);
                if (IsDeadlyType(t))
                {
                    kind = t;
                    return true;
                }
            }
        return false;
    }

    // ---------------------------------------------------------------- di chuyển chặn từng px

    /// <summary>Đi ngang dx (fixed). Đụng tường ⇒ dừng ở px cuối còn trống (phần lẻ 0), trả true.</summary>
    public bool MoveX(Entity e, int dx)
    {
        if (dx == 0)
            return false;
        int oldPx = e.X;
        int nf = e.fx + dx;
        int newPx = Fixed.ToPx(nf);
        if (newPx == oldPx)
        {
            e.fx = nf;
            return false;
        }
        var b = e.BodyRect;
        int dir = newPx > oldPx ? 1 : -1;
        int steps = (newPx - oldPx) * dir;
        for (int s = 1; s <= steps; s++)
        {
            int col = dir > 0 ? b.xMax - 1 + s : b.xMin - s;
            if (ColumnHitsSolid(col, b.yMin, b.yMax - 1))
            {
                e.fx = Fixed.FromPx(oldPx + dir * (s - 1));
                return true;
            }
        }
        e.fx = nf;
        return false;
    }

    /// <summary>
    /// Đi dọc dy (fixed; dương = xuống). Xuống: gặp mặt trên ô đặc ⇒ đứng đúng mặt sàn (Y = hàng mặt sàn), trả true.
    /// Lên: collideCeiling ⇒ gặp ô đặc thì dừng sát, trả true; không thì đi xuyên.
    /// </summary>
    public bool MoveY(Entity e, int dy, bool collideCeiling = true)
    {
        if (dy == 0)
            return false;
        int oldPx = e.Y;
        int nf = e.fy + dy;
        int newPx = Fixed.ToPx(nf);
        if (newPx == oldPx)
        {
            e.fy = nf;
            return false;
        }
        var b = e.BodyRect;
        if (newPx > oldPx)
        {
            int steps = newPx - oldPx;
            for (int s = 1; s <= steps; s++)
                if (RowLandable(b.yMax + s - 1, b.xMin, b.xMax - 1))
                {
                    e.fy = Fixed.FromPx(oldPx + s - 1);
                    return true;
                }
        }
        else if (collideCeiling)
        {
            int steps = oldPx - newPx;
            for (int s = 1; s <= steps; s++)
                if (RowHitsSolid(b.yMin - s, b.xMin, b.xMax - 1))
                {
                    e.fy = Fixed.FromPx(oldPx - (s - 1));
                    return true;
                }
        }
        e.fy = nf;
        return false;
    }

    // ---------------------------------------------------------------- khối phá được

    /// <summary>Phá ô Breakable (col, row). Trả true nếu vừa phá (ô đúng loại và chưa vỡ).</summary>
    public bool Break(int col, int row)
    {
        if (col < 0 || col >= cols || row < 0 || row >= rows)
            return false;
        int i = row * cols + col;
        if (cells[i] != eTileCollision.Breakable || broken[i])
            return false;
        broken[i] = true;
        return true;
    }

    public bool IsBroken(int col, int row)
    {
        return col >= 0 && col < cols && row >= 0 && row < rows && broken[row * cols + col];
    }

    // ---------------------------------------------------------------- cầu thang

    public StairPath GetStair(int index)
    {
        return stairs[index];
    }

    /// <summary>
    /// Tìm thang để BƯỚC VÀO từ điểm (x, y = mặt sàn đang đứng): dir &gt; 0 (lên) ⇒ thang có điểm CHÂN cùng y và |x − chân.x| ≤ range;
    /// dir &lt; 0 (xuống) ⇒ thang có điểm ĐỈNH cùng y và |x − đỉnh.x| ≤ range. Nhiều thang ⇒ gần nhất, hoà ⇒ chỉ số nhỏ.
    /// </summary>
    public bool TryGetStair(int x, int y, int dir, int range, out StairPath path)
    {
        path = default(StairPath);
        int best = int.MaxValue;
        for (int i = 0; i < stairs.Length; i++)
        {
            var p = dir > 0 ? stairs[i].bottom : stairs[i].top;
            if (p.y != y)
                continue;
            int d = Mathf.Abs(x - p.x);
            if (d <= range && d < best)
            {
                best = d;
                path = stairs[i];
            }
        }
        return best != int.MaxValue;
    }

    /// <summary>Điểm (x, y) nằm trên đường thang nào (t = px đã lên từ chân).</summary>
    public bool TryGetStairAt(int x, int y, out StairPath path, out int t)
    {
        for (int i = 0; i < stairs.Length; i++)
        {
            t = stairs[i].ParamOf(x, y);
            if (t >= 0)
            {
                path = stairs[i];
                return true;
            }
        }
        path = default(StairPath);
        t = -1;
        return false;
    }

    public static int FloorDiv(int a, int b)
    {
        return a >= 0 ? a / b : -((-a + b - 1) / b);
    }
}
