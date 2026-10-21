using UnityEditor;
using UnityEngine;

/// <summary>
/// CVM3 05 — menu dựng địa hình Map 3 (Block 3 = Stage 7–9). Dữ liệu từ python (chạy trước, từ gốc repo):
///   python -P -E Tools/cvm3/build_tileset3.py   ⇒ Assets/Data/Art/Level/Block3/level3_tiles.png + Config/Level/Block3/*_tiles.csv + Level3Tiles.json
///                                                  (WalkwayB nới trái 112 px bằng tile có sẵn — camera video tới cx −112)
///   python -P -E Tools/cvm3/build_collision3.py ⇒ Assets/Data/Config/Level/Block3/Level3Map.json (+ ảnh overlay ClaudeNotes/cvm3/ref/level3/)
/// rồi CVM3/Build Map3 Tilemaps (= CvgpLevelBuilder.BuildMap3) ⇒ Tile l3t_NNN + LevelSectionData + prefab Prefabs/Level/Block3/&lt;Section&gt;.prefab.
/// Chạy lại ⇒ "ghi 0". KHÔNG đụng Block 1/2 (H12 — vẫn dựng bằng CVGP/Build Map1 Tilemaps, CVM2/Build Map2 Tilemaps).
/// </summary>
public static class Cvm3LevelMenu
{
    [MenuItem("CVM3/Build Map3 Tilemaps")]
    public static void BuildMap3Menu()
    {
        Debug.Log(CvgpLevelBuilder.BuildMap3());
    }

    /// <summary>
    /// CVM3 10 — lớp vật thể Block 3: Assets/Data/Config/Level/Block3/Map3Objects.json (sinh bởi python -P -E Tools/cvm3/gen_map3_objects.py —
    /// bảng ClaudeNotes/cvm3/MAP3_PLACEMENT.md đo video Map_3) ⇒ Assets/Data/Prefabs/Level/Block3/Objects/&lt;Section&gt;_Objects.prefab (nến, enemy,
    /// bộ sinh, bí mật ngồi ⇒ túi tiền, phòng Mummy). Dùng chung phần dựng của CVGP/Build Map1 Objects + CVM2/Build Map2 Objects; giống ⇒ không ghi.
    /// </summary>
    [MenuItem("CVM3/Build Map3 Objects")]
    public static void BuildMap3ObjectsMenu()
    {
        Debug.Log(BuildMap3Objects());
    }

    public static string BuildMap3Objects()
    {
        return CvgpMap1Builder.BuildObjectsFrom(CvgpMap1Builder.TABLE_MAP3, CvgpMap1Builder.OBJ_DIR_MAP3, "Map3");
    }
}
