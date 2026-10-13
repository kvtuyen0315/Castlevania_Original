using UnityEditor;
using UnityEngine;

/// <summary>
/// Preset import pixel-art NES cho Assets/Data/Art/** (CVTI_MEMORY §4).
/// Chỉ đặt giá trị lần đầu import (importSettingsMissing) để không đè chỉnh tay về sau.
/// Pivot: Assets/Data/Art/Character/** = Bottom-Center, còn lại = Top-Left (khớp toạ độ NES, xem NesCoord).
/// CVGP 05 thêm (không đổi quy tắc cũ): PNG có trong manifest Tools/cvgp/slices_gameplay.json ⇒ pivot Custom = [px, py]
/// của manifest; thư mục mới chưa có trong manifest: Enemy/Boss/Item = Bottom-Center, FX/SubWeapon = Center.
/// </summary>
public class NesSpriteImporter : AssetPostprocessor
{
    const string ART_ROOT = "Assets/Data/Art/";
    const string CHARACTER_ROOT = "Assets/Data/Art/Character/";
    static readonly string[] BOTTOM_CENTER_ROOTS = { "Assets/Data/Art/Enemy/", "Assets/Data/Art/Boss/", "Assets/Data/Art/Item/" };
    static readonly string[] CENTER_ROOTS = { "Assets/Data/Art/FX/", "Assets/Data/Art/SubWeapon/" };

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ART_ROOT) || !assetImporter.importSettingsMissing)
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = NesCoord.PPU;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        var slice = CvgpSpriteManifest.FindSlice(assetPath);
        if (slice != null && slice.pivot != null && slice.pivot.Length == 2)
        {
            // pivot Unity: gốc dưới-trái, chuẩn hoá theo kích thước sprite (rect manifest)
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2((float)slice.pivot[0] / slice.rect[2], 1f - (float)slice.pivot[1] / slice.rect[3]);
        }
        else
            settings.spriteAlignment = (int)DefaultAlignment(assetPath);
        importer.SetTextureSettings(settings);
    }

    static SpriteAlignment DefaultAlignment(string path)
    {
        if (path.StartsWith(CHARACTER_ROOT))
            return SpriteAlignment.BottomCenter;
        foreach (var root in BOTTOM_CENTER_ROOTS)
            if (path.StartsWith(root))
                return SpriteAlignment.BottomCenter;
        foreach (var root in CENTER_ROOTS)
            if (path.StartsWith(root))
                return SpriteAlignment.Center;
        return SpriteAlignment.TopLeft;
    }
}
