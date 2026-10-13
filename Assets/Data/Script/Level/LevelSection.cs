using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Gốc prefab 1 section địa hình (Assets/Data/Prefabs/Level/Block1/&lt;Section&gt;.prefab — dựng bằng CVGP/Build Map1 Tilemaps).
/// Gốc transform = góc trên-trái section (px (0,0)); Grid ô 0.5 unit = tile 8×8; "BG" = Tilemap layer Background,
/// "FG" (nếu có) = cùng tile vẽ lại ở layer Foreground để che nhân vật (cổng Castle Entrance).
/// Không Update (H4): ai điều khiển camera (StageWorld, routine 07) gọi PlaceForCamera mỗi tick khi cx đổi.
/// </summary>
[DisallowMultipleComponent]
public class LevelSection : MonoBehaviour
{
    public LevelSectionData data;
    public Tilemap bg;
    public Tilemap fg;

    /// <summary>
    /// Đặt section sao cho mép trái màn = cx (px section) với camera đứng yên ở NesCoord.CameraHome:
    /// gốc ở NES (−cx, SCREEN_Y). Con của section đặt localPosition = NesCoord.ToWorld(x, y) theo px section.
    /// (Cách tương đương: giữ gốc ở NES (0, SCREEN_Y) và dời camera +cx — routine 07 chọn.)
    /// </summary>
    public void PlaceForCamera(int cx)
    {
        transform.localPosition = NesCoord.ToWorld(-cx, LevelSectionData.SCREEN_Y);
    }

    /// <summary>cx kẹp trong biên camera của section (bossLockCx ≥ 0 thì chỉ kẹp — khoá do người gọi quyết định).</summary>
    public int ClampCamera(int cx)
    {
        if (data == null)
            return cx;
        return Mathf.Clamp(cx, data.cameraMinX, data.cameraMaxX);
    }
}
