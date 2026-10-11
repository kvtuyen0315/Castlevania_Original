/// <summary>
/// 37 SFX của pack 393996. Giá trị = số thứ tự file gốc (NN.wav) để truy vết; 0 = không có.
/// Tên "UnknownNN" = chưa định danh được bằng đối chiếu âm thanh video (CVGP_MEMORY §3).
/// Độ tin cậy: chắc = khớp >= 2 sự kiện cùng loại + đã xem khung hình; còn lại = khả năng (1 sự kiện/gián tiếp).
/// </summary>
public enum eSfx
{
    None = 0,
    Unknown01 = 1,
    Unknown02 = 2,
    Unknown03 = 3,
    TallyTime = 4,          // chắc: đếm TIME cuối màn
    TallyHeart = 5,         // chắc: đếm tim cuối màn
    TallyHeartAlt = 6,      // khả năng: biến thể của TallyHeart (khớp cùng sự kiện, kém hơn)
    MapStep = 7,            // chắc: nhịp bước trên bản đồ lâu đài
    Unknown08 = 8,
    ThrowDagger = 9,        // chắc: ném dao găm
    Unknown10 = 10,
    Unknown11 = 11,
    Unknown12 = 12,
    Unknown13 = 13,
    FishJump = 14,          // chắc: Fish Man nhảy khỏi nước
    FishSplash = 15,        // chắc: Fish Man rơi lại xuống nước
    Unknown16 = 16,
    Unknown17 = 17,
    DoorAlt = 18,           // khả năng: cửa cuối màn (biến thể của Door)
    Unknown19 = 19,
    HitFlame = 20,          // chắc: roi trúng chậu lửa / quái
    BreakWall = 21,         // chắc: roi đập vỡ tường gạch / nến lớn (mảnh tím)
    PickupHeart = 22,       // chắc: nhặt tim nhỏ và tim lớn
    PickupBag = 23,         // chắc: nhặt túi tiền (100/400/700/1000)
    PickupWeapon = 24,      // chắc: nhặt roi / vũ khí phụ / II
    EnterGate = 25,         // khả năng: Simon bước vào cổng lâu đài
    Unknown26 = 26,
    PotionInvisible = 27,   // khả năng: nhặt bình vô hình
    InvisibleEnd = 28,      // khả năng: hết vô hình
    Door = 29,              // chắc: cửa cuối màn mở/đóng
    HolyWater = 30,         // khả năng: nước thánh vỡ/cháy (chỉ có trong video boss)
    HolyWaterAlt = 31,      // khả năng: biến thể của HolyWater
    Unknown32 = 32,
    Unknown33 = 33,
    Unknown34 = 34,
    Rosary = 35,            // chắc: nhặt rosary (nháy trắng)
    Unknown36 = 36,
    Unknown37 = 37,
}
