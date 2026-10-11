using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bảng glyph 8×8 NES: ký tự → sprite (CVTI_MEMORY §3).
/// Glyph cắt từ dòng copyright + PUSH START KEY của sheet Title (Tools/cvti/slices.json), pivot Top-Left, nền trong suốt.
/// Dấu cách không có sprite: chỉ tiến 1 ô. Dựng lại asset bằng menu CVTI/Build NesFont (NesFontBuilder).
/// </summary>
[CreateAssetMenu(fileName = "NesFont", menuName = "CVTI/NesFont")]
public class NesFont : ScriptableObject
{
    [Serializable]
    public struct Glyph
    {
        public char character;
        public Sprite sprite;
    }

    /// <summary>Bề rộng 1 ô chữ NES (pixel).</summary>
    public const int CELL = 8;

    [SerializeField] private List<Glyph> glyphs = new List<Glyph>();

    private Dictionary<char, Sprite> lookup;

    public IReadOnlyList<Glyph> Glyphs => glyphs;

    public void SetGlyphs(List<Glyph> list)
    {
        glyphs = list;
        lookup = null;
    }

    public bool TryGetSprite(char c, out Sprite sprite)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<char, Sprite>();
            foreach (var g in glyphs)
                lookup[g.character] = g.sprite;
        }
        return lookup.TryGetValue(c, out sprite);
    }

    /// <summary>Có vẽ được cả chuỗi không (dấu cách luôn được); trả về ký tự thiếu đầu tiên.</summary>
    public bool CanRender(string text, out char missing)
    {
        foreach (var c in text)
        {
            if (c != ' ' && !TryGetSprite(c, out _))
            {
                missing = c;
                return false;
            }
        }
        missing = '\0';
        return true;
    }

    private void OnValidate()
    {
        lookup = null;
    }
}
