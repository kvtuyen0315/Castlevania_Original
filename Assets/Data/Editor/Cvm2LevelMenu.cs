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

    /// <summary>
    /// CVM2 10 — lớp vật thể Block 2: Assets/Data/Config/Level/Block2/Map2Objects.json (sinh bởi python -P -E Tools/cvm2/gen_map2_objects.py —
    /// bảng MAP2_PLACEMENT.md đo video) ⇒ Assets/Data/Prefabs/Level/Block2/Objects/&lt;Section&gt;_Objects.prefab (nến, enemy, bộ sinh, bí mật,
    /// bệ/bẫy, phòng boss). Dùng chung phần dựng của CVGP/Build Map1 Objects; giống ⇒ không ghi ("ghi 0").
    /// </summary>
    [MenuItem("CVM2/Build Map2 Objects")]
    public static void BuildMap2ObjectsMenu()
    {
        Debug.Log(BuildMap2Objects());
    }

    public static string BuildMap2Objects()
    {
        return CvgpMap1Builder.BuildObjectsFrom(CvgpMap1Builder.TABLE_MAP2, CvgpMap1Builder.OBJ_DIR_MAP2, "Map2");
    }
}
