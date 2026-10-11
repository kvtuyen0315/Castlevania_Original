using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chuỗi chữ NES dựng bằng glyph của NesFont: mỗi ký tự 1 SpriteRenderer con, lưới 8 px (CVTI_MEMORY §4.3).
/// Transform = góc trên-trái ký tự đầu (pivot glyph Top-Left) ⇒ đặt bằng SetNesPosition(x, y) theo toạ độ NES.
/// '\n' xuống dòng 8 px; dấu cách và ký tự thiếu glyph chỉ tiến 1 ô (thiếu glyph: warning 1 lần mỗi ký tự).
/// </summary>
public class NesText : MonoBehaviour
{
    [SerializeField] private NesFont font;
    [SerializeField, TextArea] private string text = "";
    [SerializeField] private Color color = Color.white;
    [SerializeField] private string sortingLayer = "HUD";
    [SerializeField] private int sortingOrder;

    static readonly HashSet<char> warnedMissing = new HashSet<char>();

    private readonly List<SpriteRenderer> glyphs = new List<SpriteRenderer>();
    private bool visible = true;
    private bool built;

    public string Text => text;
    public bool Visible => visible;

    private void Awake()
    {
        if (!built)
            SetText(text);
    }

    public void SetText(string value)
    {
        text = value ?? "";
        built = true;

        int used = 0, col = 0, row = 0;
        foreach (var c in text)
        {
            if (c == '\n')
            {
                col = 0;
                row++;
                continue;
            }
            if (c != ' ')
            {
                Sprite sprite;
                if (font != null && font.TryGetSprite(c, out sprite))
                {
                    var sr = GetGlyph(used++);
                    sr.sprite = sprite;
                    sr.transform.localPosition = new Vector3(col * NesFont.CELL * NesCoord.UNIT_PER_PIXEL, -row * NesFont.CELL * NesCoord.UNIT_PER_PIXEL, 0f);
                }
                else if (warnedMissing.Add(c))
                    Debug.LogWarning("[NesText] Không có glyph '" + c + "' trong NesFont (" + name + ": \"" + text + "\")", this);
            }
            col++;
        }

        for (int i = used; i < glyphs.Count; i++)
            glyphs[i].gameObject.SetActive(false);
        ApplyStyle();
    }

    /// <summary>Ẩn/hiện cả chuỗi (dùng cho nháy) — không dựng lại glyph.</summary>
    public void SetVisible(bool value)
    {
        visible = value;
        ApplyStyle();
    }

    public void SetColor(Color value)
    {
        color = value;
        ApplyStyle();
    }

    public void SetNesPosition(int x, int y)
    {
        transform.localPosition = NesCoord.ToWorld(x, y);
    }

    SpriteRenderer GetGlyph(int index)
    {
        while (glyphs.Count <= index)
        {
            var go = new GameObject("c" + glyphs.Count);
            go.transform.SetParent(transform, false);
            glyphs.Add(go.AddComponent<SpriteRenderer>());
        }
        var sr = glyphs[index];
        sr.gameObject.SetActive(true);
        return sr;
    }

    void ApplyStyle()
    {
        foreach (var sr in glyphs)
        {
            sr.enabled = visible;
            sr.color = color;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = sortingOrder;
        }
    }
}
