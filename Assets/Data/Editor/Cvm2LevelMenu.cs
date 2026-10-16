using UnityEditor;
using UnityEngine;

/// <summary>
/// CVM2 05 — menu dựng địa hình Map 2 (Block 2 = Stage 4–6). Dữ liệu từ python (chạy trước, từ gốc repo):
///   python -P -E Tools/cvm2/build_tileset2.py   ⇒ Assets/Data/Art/Level/Block2/level2_tiles.png + Config/Level/Block2/*_tiles.csv + Level2Tiles.json
///   python -P -E Tools/cvm2/build_collision2.py ⇒ Assets/Data/Config/Level/Block2/Level2Map.json (+ ảnh overlay ClaudeNotes/cvm2/ref/level2/)
/// rồi CVM2/Build Map2 Tilemaps (= CvgpLevelBuilder.BuildMap2) ⇒ Tile l2t_NNN + LevelSectionData + prefab Prefabs/Level/Block2/&lt;Section&gt;.prefab.
/// Chạy lại ⇒ "ghi 0". KHÔNG đụng Block 1 (H12 — Block 1 vẫn dựng bằng CVGP/Build Map1 Tilemaps).
/// </summary>
public static class Cvm2LevelMenu
{
    [MenuItem("CVM2/Build Map2 Tilemaps")]
    public static void BuildMap2Menu()
    {
        Debug.Log(CvgpLevelBuilder.BuildMap2());
    }
}
