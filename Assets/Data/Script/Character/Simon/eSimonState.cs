/// <summary>
/// State của Simon (CVGP 08). Routine 09 THÊM state tấn công (roi/ném) vào cuối enum + 1 lớp/state trong Simon/States —
/// không cần sửa state có sẵn: state tấn công dùng lại SimonController.AirStep()/StairMove()/GroundCheck…
/// </summary>
public enum eSimonState
{
    Idle,
    Walk,
    Crouch,
    Jump,
    Fall,
    Land,           // ngồi khựng khi đáp sau rơi cao
    StairApproach,  // tự đi tới chân/đỉnh thang rồi bước lên/xuống
    StairIdle,
    StairUp,
    StairDown,
    Hurt,           // knockback
    Dead,
    FallDeath,      // rơi hố / nước
    AutoWalk,       // cửa, cổng, cutscene (routine 15)
}
