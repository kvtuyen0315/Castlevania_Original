using UnityEngine;

/// <summary>
/// Số fixed-point 1/256 px (như byte phụ pixel của NES) cho vị trí/vận tốc gameplay (CVGP 07, H4).
/// Vị trí lưu int (fx = px × 256 + phần lẻ), cộng vận tốc nguyên ⇒ không float tích luỹ, tất định mọi máy.
/// Pixel vẽ = phần nguyên làm tròn XUỐNG (ToPx dịch phải số học ⇒ đúng cả số âm).
/// </summary>
public static class Fixed
{
    public const int SHIFT = 8;
    public const int ONE = 1 << SHIFT;
    public const int HALF = ONE >> 1;
    public const int FRAC_MASK = ONE - 1;

    public static int FromPx(int px)
    {
        return px * ONE;
    }

    /// <summary>Pixel = floor(f / 256).</summary>
    public static int ToPx(int f)
    {
        return f >> SHIFT;
    }

    public static int Frac(int f)
    {
        return f & FRAC_MASK;
    }

    /// <summary>Đổi số thực (px hoặc px/tick, đọc từ Config lúc khởi tạo) sang fixed — làm tròn gần nhất. KHÔNG gọi mỗi tick.</summary>
    public static int FromFloat(float px)
    {
        return Mathf.RoundToInt(px * ONE);
    }

    public static float ToFloat(int f)
    {
        return f / (float)ONE;
    }
}
