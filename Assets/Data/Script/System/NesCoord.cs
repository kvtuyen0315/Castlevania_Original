using UnityEngine;

/// <summary>
/// Quy ước toạ độ NES → Unity (CVTI_MEMORY §4).
/// NES: pixel, x từ trái, y từ TRÊN xuống, gốc (0,0) = góc trên-trái màn hình 256×240.
/// World: 1 unit = PPU pixel, y lật dấu ⇒ NES (x, y) = world (x / PPU, -y / PPU).
/// Camera đặt ở CameraHome (tâm màn hình) ⇒ góc trên-trái màn hình trùng world (0, 0).
/// Sprite thường pivot Top-Left: đặt transform tại ToWorld(góc trên-trái đo trên sheet).
/// Nhân vật pivot Bottom-Center: đặt tại ToWorld(tâm x, mép dưới y) — vd Simon 16×32 ở (120,177) ⇒ ToWorld(128, 209).
/// </summary>
public static class NesCoord
{
    public const int SCREEN_W = 256;
    public const int SCREEN_H = 240;
    public const int PPU = 16;

    public const float UNIT_PER_PIXEL = 1f / PPU;

    /// <summary>Vị trí camera khi màn hình không cuộn (tâm 256×240, z = -10).</summary>
    public static readonly Vector3 CameraHome = new Vector3(SCREEN_W * 0.5f / PPU, -SCREEN_H * 0.5f / PPU, -10f);

    public static Vector3 ToWorld(int x, int y)
    {
        return new Vector3((float)x / PPU, -(float)y / PPU, 0f);
    }

    /// <summary>Bản float cho vị trí lẻ pixel (vd Simon đi 0.5 px/frame); khi vẽ, camera upscaleRT tự làm tròn về pixel.</summary>
    public static Vector3 ToWorld(float x, float y)
    {
        return new Vector3(x / PPU, -y / PPU, 0f);
    }

    public static Vector2Int ToNes(Vector3 world)
    {
        return new Vector2Int(Mathf.RoundToInt(world.x * PPU), Mathf.RoundToInt(-world.y * PPU));
    }
}
