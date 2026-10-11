using UnityEditor;
using UnityEngine;

/// <summary>
/// Preset import pixel-art NES cho Assets/Data/Art/** (CVTI_MEMORY §4).
/// Chỉ đặt giá trị lần đầu import (importSettingsMissing) để không đè chỉnh tay về sau.
/// Pivot: Assets/Data/Art/Character/** = Bottom-Center, còn lại = Top-Left (khớp toạ độ NES, xem NesCoord).
/// </summary>
public class NesSpriteImporter : AssetPostprocessor
{
    const string ART_ROOT = "Assets/Data/Art/";
    const string CHARACTER_ROOT = "Assets/Data/Art/Character/";

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
        settings.spriteAlignment = (int)(assetPath.StartsWith(CHARACTER_ROOT) ? SpriteAlignment.BottomCenter : SpriteAlignment.TopLeft);
        importer.SetTextureSettings(settings);
    }
}
