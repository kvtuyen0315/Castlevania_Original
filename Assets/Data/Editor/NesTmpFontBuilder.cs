using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Dựng font TextMeshPro bitmap "NesFont TMP.asset" từ NesFont.asset (CVTI_MEMORY §3, §4.5).
/// Mỗi glyph 8×8 chép nguyên pixel vào atlas 128×128 (Point, ô 10×10 có đệm 1 px), metrics: cao 8, advance 8,
/// baseline ở đáy ô ⇒ TMP cỡ chữ 8 = 8 px NES (dùng trong Canvas World Space 1 đơn vị = 1 px NES).
/// Atlas + material là sub-asset; dựng lại giữ nguyên GUID nên prefab không mất tham chiếu.
/// Chạy lại sau CVTI/Build NesFont mỗi khi thêm glyph.
/// </summary>
public static class NesTmpFontBuilder
{
    const string SOURCE_PATH = "Assets/Data/Art/Font/NesFont.asset";
    const string TMP_PATH = "Assets/Data/Art/Font/NesFont TMP.asset";
    const int CELL = NesFont.CELL;
    const int PAD = 1;
    const int SLOT = CELL + PAD * 2;
    const int ATLAS = 128;

    [MenuItem("CVTI/Build NesFont TMP")]
    public static void Build()
    {
        var source = AssetDatabase.LoadAssetAtPath<NesFont>(SOURCE_PATH);
        if (source == null)
        {
            Debug.LogError($"[NesTmpFontBuilder] Không thấy {SOURCE_PATH}");
            return;
        }

        int perRow = ATLAS / SLOT;
        if (source.Glyphs.Count + 1 > perRow * perRow)
        {
            Debug.LogError($"[NesTmpFontBuilder] {source.Glyphs.Count} glyph không vừa atlas {ATLAS}×{ATLAS}");
            return;
        }

        var pixels = new Color32[ATLAS * ATLAS];
        var glyphs = new List<Glyph>();
        var characters = new List<TMP_Character>();

        // Dấu cách: không hình, chỉ tiến 1 ô.
        var space = new Glyph(0, new GlyphMetrics(0, 0, 0, 0, CELL), new GlyphRect(0, 0, 0, 0), 1f, 0);
        glyphs.Add(space);
        characters.Add(new TMP_Character(' ', space));
        // TMP tra ký tự '_' để kẻ gạch chân mỗi lần dựng chữ (thiếu ⇒ cảnh báo liên tục). Chưa có glyph '_' ⇒ dùng chung
        // glyph trống của dấu cách: không cảnh báo, gạch chân không hiện.
        characters.Add(new TMP_Character('_', space));

        for (int i = 0; i < source.Glyphs.Count; i++)
        {
            var g = source.Glyphs[i];
            int slot = i + 1;
            int x = (slot % perRow) * SLOT + PAD;
            int y = ATLAS - (slot / perRow + 1) * SLOT + PAD; // gốc dưới-trái như TextCore
            if (!CopyGlyph(g.sprite, pixels, x, y))
                return;

            var glyph = new Glyph((uint)slot, new GlyphMetrics(CELL, CELL, 0, CELL, CELL), new GlyphRect(x, y, CELL, CELL), 1f, 0);
            glyphs.Add(glyph);
            characters.Add(new TMP_Character(g.character, glyph));
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TMP_PATH);
        bool isNew = font == null;
        if (isNew)
            font = ScriptableObject.CreateInstance<TMP_FontAsset>();
        font.name = "NesFont TMP";

        var atlas = isNew ? null : font.atlasTexture;
        if (atlas == null)
        {
            atlas = new Texture2D(ATLAS, ATLAS, TextureFormat.RGBA32, false);
            atlas.name = "NesFont TMP Atlas";
        }
        atlas.filterMode = FilterMode.Point;
        atlas.wrapMode = TextureWrapMode.Clamp;
        atlas.SetPixels32(pixels);
        atlas.Apply(false, false);

        var material = isNew ? null : font.material;
        if (material == null)
        {
            var shader = Shader.Find("TextMeshPro/Bitmap");
            if (shader == null)
            {
                Debug.LogError("[NesTmpFontBuilder] Thiếu shader TextMeshPro/Bitmap (chưa import TMP Essential Resources?)");
                return;
            }
            material = new Material(shader);
            material.name = "NesFont TMP Material";
        }
        material.SetTexture(ShaderUtilities.ID_MainTex, atlas);

        var face = new FaceInfo
        {
            familyName = "NesFont",
            styleName = "Regular",
            pointSize = CELL,
            scale = 1f,
            lineHeight = CELL,
            ascentLine = CELL,
            capLine = CELL - 1,
            meanLine = CELL - 3,
            baseline = 0,
            descentLine = 0,
            superscriptOffset = CELL,
            superscriptSize = 0.5f,
            subscriptOffset = -2,
            subscriptSize = 0.5f,
            underlineOffset = -1,
            underlineThickness = 1,
            strikethroughOffset = 3,
            strikethroughThickness = 1,
            tabWidth = CELL,
        };

        SetField(font, "m_Version", "1.1.0");
        SetField(font, "m_FaceInfo", face);
        SetField(font, "m_GlyphTable", glyphs);
        SetField(font, "m_CharacterTable", characters);
        SetField(font, "m_AtlasWidth", ATLAS);
        SetField(font, "m_AtlasHeight", ATLAS);
        SetField(font, "m_AtlasPadding", PAD);
        SetField(font, "m_AtlasRenderMode", GlyphRenderMode.RASTER);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        font.atlasTextures = new[] { atlas };
        font.material = material;

        if (isNew)
        {
            AssetDatabase.CreateAsset(font, TMP_PATH);
            AssetDatabase.AddObjectToAsset(atlas, font);
            AssetDatabase.AddObjectToAsset(material, font);
        }
        font.ReadFontAssetDefinition();
        EditorUtility.SetDirty(atlas);
        EditorUtility.SetDirty(material);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        Debug.Log($"[NesTmpFontBuilder] {TMP_PATH}: {characters.Count} ký tự (gồm dấu cách), atlas {ATLAS}×{ATLAS}");
    }

    // Đọc thẳng file PNG (sprite import không bật Read/Write) — glyph là 1 PNG 8×8 riêng.
    static bool CopyGlyph(Sprite sprite, Color32[] atlas, int x, int y)
    {
        var path = AssetDatabase.GetAssetPath(sprite);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!tex.LoadImage(File.ReadAllBytes(path)) || tex.width != CELL || tex.height != CELL)
            {
                Debug.LogError($"[NesTmpFontBuilder] {path} không phải PNG {CELL}×{CELL}");
                return false;
            }
            var src = tex.GetPixels32();
            for (int j = 0; j < CELL; j++)
                for (int i = 0; i < CELL; i++)
                    atlas[(y + j) * ATLAS + x + i] = src[j * CELL + i];
            return true;
        }
        finally
        {
            Object.DestroyImmediate(tex);
        }
    }

    static void SetField(TMP_FontAsset font, string name, object value)
    {
        var field = typeof(TMP_FontAsset).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null)
            throw new System.MissingFieldException(nameof(TMP_FontAsset), name);
        field.SetValue(font, value);
    }
}
