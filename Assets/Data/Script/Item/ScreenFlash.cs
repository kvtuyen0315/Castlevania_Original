using UnityEngine;

/// <summary>
/// Nháy màn Rosary (CVGP 11) — component trên camera gameplay (StageWorld tự gắn khi cần): khi bật, OnRenderImage thay pixel đen
/// của cả màn bằng màu xám (shader Hidden/CVGP/NesScreenFlash) — đúng như NES đổi màu nền chung (video: HUD cũng nháy).
/// StageWorld bật/tắt theo tick (tất định); tắt ⇒ không tốn gì (Unity bỏ qua OnRenderImage của component disabled).
/// </summary>
[DisallowMultipleComponent]
public class ScreenFlash : MonoBehaviour
{
    public Material material;
    public Color flashColor = new Color32(188, 188, 188, 255);

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (material == null)
        {
            Graphics.Blit(src, dst);
            return;
        }
        material.SetColor("_FlashColor", flashColor);
        Graphics.Blit(src, dst, material);
    }
}
