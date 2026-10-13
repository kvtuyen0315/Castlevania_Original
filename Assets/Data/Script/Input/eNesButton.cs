/// <summary>
/// 8 nút tay cầm NES + SubWeapon (phím tắt ảo của Up + B — Simon hiểu là "ném vũ khí phụ", KHÔNG giả Up).
/// Giá trị enum = số bit trong bitmask NesPad / replay (đừng đổi thứ tự — làm hỏng replay đã lưu).
/// </summary>
public enum eNesButton
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    A = 4,
    B = 5,
    Select = 6,
    Start = 7,
    SubWeapon = 8,
}
