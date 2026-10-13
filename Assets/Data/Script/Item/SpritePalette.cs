using UnityEngine;

/// <summary>
/// Đổi bảng màu NES cho SpriteRenderer lúc chạy (CVGP 11 — nợ 05/09 "cơ chế đổi bảng màu"): gán tạm vật liệu shader
/// CVGP/NesPaletteSwap + MaterialPropertyBlock 3 cặp màu (AnimSet.Palette from → to); palette null ⇒ trả vật liệu gốc.
/// Không cấp phát mỗi tick (1 MaterialPropertyBlock dùng chung; chỉ đổi khi bảng màu khác lần trước).
/// </summary>
public static class SpritePalette
{
    private static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
    private static readonly int[] FROM = { Shader.PropertyToID("_From0"), Shader.PropertyToID("_From1"), Shader.PropertyToID("_From2") };
    private static readonly int[] TO = { Shader.PropertyToID("_To0"), Shader.PropertyToID("_To1"), Shader.PropertyToID("_To2") };
    private static readonly int SWAP = Shader.PropertyToID("_Swap");

    public static void Apply(SpriteRenderer r, AnimSet.Palette p, Material swapMaterial, Material original)
    {
        if (r == null)
            return;
        if (p == null || swapMaterial == null)
        {
            if (original != null && r.sharedMaterial != original)
                r.sharedMaterial = original;
            if (r.HasPropertyBlock())
                r.SetPropertyBlock(null);
            return;
        }
        if (r.sharedMaterial != swapMaterial)
            r.sharedMaterial = swapMaterial;
        r.GetPropertyBlock(block);
        for (int i = 0; i < 3; i++)
        {
            block.SetColor(FROM[i], i < p.from.Length ? (Color)p.from[i] : Color.clear);
            block.SetColor(TO[i], i < p.to.Length ? (Color)p.to[i] : Color.clear);
        }
        block.SetFloat(SWAP, 1f);
        r.SetPropertyBlock(block);
    }
}
