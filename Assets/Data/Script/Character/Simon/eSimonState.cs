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
    Attack,         // CVGP 09: vung roi / ném vũ khí phụ — 1 state cho 4 tư thế (đứng, ngồi, trên không, cầu thang)
}

/// <summary>Tư thế đòn (CVGP 09) — chọn khung thân use_&lt;tư thế&gt;_k và offset roi; trên không dùng Stand (nhãn sheet "Standing/Jumping").</summary>
public enum eAttackPose
{
    Stand,
    Duck,
    StairsUp,
    StairsDown,
}
