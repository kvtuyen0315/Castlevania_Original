using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dựng Assets/Data/Art/Font/NesFont.asset từ các file glyph_&lt;tên&gt;.png (do Tools/cvti/slice_sprites.py cắt).
/// Tên file = chính ký tự (A–Z, 0–9), riêng ký tự đặc biệt dùng tên trong SPECIAL. Chạy lại khi thêm glyph (vd sheet HUD).
/// </summary>
public static class NesFontBuilder
{
    const string FONT_DIR = "Assets/Data/Art/Font";
    const string ASSET_PATH = FONT_DIR + "/NesFont.asset";

    static readonly Dictionary<string, char> SPECIAL = new Dictionary<string, char>
    {
        { "copyright", '©' },
        { "period", '.' },
        { "comma", ',' },
        { "hyphen", '-' },
        { "heart", '♥' },
    };

    [MenuItem("CVTI/Build NesFont")]
    public static void Build()
    {
        var list = new List<NesFont.Glyph>();
        foreach (var guid in AssetDatabase.FindAssets("glyph_ t:Sprite", new[] { FONT_DIR }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var key = Path.GetFileNameWithoutExtension(path).Substring("glyph_".Length);
            char c;
            if (key.Length == 1)
                c = key[0];
            else if (!SPECIAL.TryGetValue(key, out c))
            {
                Debug.LogError($"[NesFontBuilder] Không biết ký tự cho {path}");
                continue;
            }
            list.Add(new NesFont.Glyph { character = c, sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path) });
        }
        list.Sort((a, b) => a.character.CompareTo(b.character));

        var font = AssetDatabase.LoadAssetAtPath<NesFont>(ASSET_PATH);
        if (font == null)
        {
            font = ScriptableObject.CreateInstance<NesFont>();
            AssetDatabase.CreateAsset(font, ASSET_PATH);
        }
        font.SetGlyphs(list);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        Debug.Log($"[NesFontBuilder] {ASSET_PATH}: {list.Count} glyph");
    }
}
