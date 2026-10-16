/// <summary>
/// 16 bài nhạc (Art_Source/music → Assets/Data/Audio/Music/mNN_*.mp3, CVM2 03). Giá trị = số thứ tự file gốc (NN) để truy vết.
/// Bài nào ở đâu: xác nhận bằng tương quan chéo âm thanh video (CVM2_MEMORY §2d) — không theo tên file.
/// </summary>
public enum eMusic
{
    None = 0,
    Underground = 1,        // FDS nhập tên — không dùng
    Prologue = 2,           // Intro (video Title_Intro 8.43 s)
    VampireKiller = 3,      // Block 1 (Stage 01–03; video Map_1 / Title_Intro)
    Stalker = 4,            // Block 2 (Stage 04–06; video Map_2)
    WickedChild = 5,        // Block 3 (suy luận — chưa có video)
    WalkingOnTheEdge = 6,   // Block 4 (suy luận)
    HeartOfFire = 7,        // Block 5 (suy luận)
    OutOfTime = 8,          // Stage 16–17 (suy luận)
    NothingToLose = 9,      // Stage 18 (suy luận)
    PoisonMind = 10,        // boss (video Map_1, Boss_Map_1, Map_2, Boss_Map_2)
    BlackNight = 11,        // Dracula (suy luận)
    StageClear = 12,        // nhặt quả cầu (4 video)
    GameComplete = 13,      // kết game — chưa dùng
    Voyager = 14,           // ending — chưa dùng
    Death = 15,             // mất mạng (suy luận — không video)
    GameOver = 16,          // GAME OVER (suy luận — không video)
}
