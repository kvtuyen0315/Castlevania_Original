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

    // ---------------------------------------------------------------- khối phá được (CVGP 11)

    private struct ClearedTile
    {
        public Vector3Int cell;
        public TileBase tile;
    }

    private readonly System.Collections.Generic.List<ClearedTile> cleared = new System.Collections.Generic.List<ClearedTile>(8);

    /// <summary>Xoá 4 tile 8×8 của ô va chạm 16×16 (col, row) trên BG (lỗ đen như video 1:39.6) — nhớ để RestoreCells.</summary>
    public void ClearCell(int col, int row)
    {
        if (bg == null)
            return;
        for (int dy = 0; dy < 2; dy++)
            for (int dx = 0; dx < 2; dx++)
            {
                // ô tile (c, −r−1) ⇒ góc tile = px (8c, 8r) (CVGP 06)
                var cell = new Vector3Int(col * 2 + dx, -(row * 2 + dy) - 1, 0);
                var t = bg.GetTile(cell);
                if (t == null)
                    continue;
                cleared.Add(new ClearedTile { cell = cell, tile = t });
                bg.SetTile(cell, null);
            }
    }

    /// <summary>Trả lại mọi tile đã xoá (nạp lại section ⇒ lưới va chạm mới, khối nguyên vẹn).</summary>
    public void RestoreCells()
    {
        if (bg == null || cleared.Count == 0)
            return;
        for (int i = 0; i < cleared.Count; i++)
            bg.SetTile(cleared[i].cell, cleared[i].tile);
        cleared.Clear();
    }

    /// <summary>cx kẹp trong biên camera của section (bossLockCx ≥ 0 thì chỉ kẹp — khoá do người gọi quyết định).</summary>
    public int ClampCamera(int cx)
    {
        if (data == null)
            return cx;
        return Mathf.Clamp(cx, data.cameraMinX, data.cameraMaxX);
    }
}
