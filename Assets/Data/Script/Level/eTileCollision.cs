/// <summary>
/// Loại ô va chạm 16×16 của địa hình (CVGP 06). Ký tự lưới trong LevelSectionData.collision = LevelSectionData.TYPE_CHARS[(int)loại].
/// Giá trị enum = thứ tự "types" của Tools/cvgp/build_collision.py — đổi thứ tự phải sửa cả hai.
/// </summary>
public enum eTileCollision : byte
{
    Empty = 0,       // nền, lan can, song sắt, trụ, cây… (đi xuyên qua)
    Solid = 1,       // gạch đứng được + tường
    Stair = 2,       // ô chứa bậc thang 8×8 — KHÔNG đặc; đi thang theo LevelSectionData.stairs
    StairFloor = 3,  // miệng cầu thang nằm trong hàng sàn: đặc mặt trên (đi ngang qua được — video), xuống bằng thang
    Water = 4,       // chạm là chết (Crypt B)
    Pit = 5,         // hố chết (Map 1 không có ô nào — rơi khỏi đáy section xử lý ở lõi vật lý)
    Door = 6,        // cột cửa cuối stage (trigger ở LevelSectionData.connections)
    Breakable = 7,   // khối phá được bằng roi — đặc tới khi bị phá (trạng thái lúc chạy do 07/11 giữ)
    Hazard = 8,      // gai/lửa… (Map 1 không có)
}

/// <summary>Hướng cầu thang: UpRight = đi lên sang phải (↗, chân ở trái-dưới); UpLeft = đi lên sang trái (↖, chân ở phải-dưới).</summary>
public enum eStairDir
{
    UpRight = 0,
    UpLeft = 1,
}

/// <summary>Kiểu nối giữa 2 section.</summary>
public enum eLevelLink
{
    Gate = 0,   // cổng lâu đài cuối Castle Entrance (tự đi vào, cắt đen)
    Door = 1,   // cửa cuối stage (cuộn 128 → cửa mở → tự đi → cửa đóng → cuộn 128)
    Stair = 2,  // cầu thang xuyên mép trên/dưới section (Crypt A ↔ Crypt B)
}
