using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD dùng chung Intro + màn chơi (CVTI_MEMORY §4.5). Canvas World Space 256×48, scale 1/16 ⇒ 1 đơn vị canvas = 1 px NES,
/// chữ TextMeshProUGUI font "NesFont TMP" cỡ 8. Là con của prefab màn nên tắt cùng màn khi cắt đen.
/// Bố cục đo từ video: dòng trạng thái (0,16); PLAYER (0,24) + 16 vạch từ x 56 bước 4; ENEMY (0,32);
/// khung vũ khí phụ (128,24); tim (168,24) + "-05" (176,24); "P-03" (168,32).
/// CVGP 12 (đo video Map_1, Tools/cvgp/hud_rules.py): icon vũ khí phụ = sprite item 16×16 góc (136,27) trong khung (khớp mẫu rìu
/// 3:25 lệch 0; dao trên HUD là sprite item LẬT NGANG — mũi sang phải: lệch 8 px thay vì 20); icon II/III = sprite item góc (208,24)
/// đổi bảng màu như video (đen→đỏ khung, xanh→cam thanh ENEMY, kem→trắng — shader CVGP/NesPaletteSwap, không vẽ sprite mới);
/// vạch máu đã mất vẽ khung rỗng trắng 3×6 (video 3:30 ENEMY 0, boss 12.2 s).
/// Thanh máu đọc giá trị ĐANG HIỆN (GameSession.ShownPlayerHp — StageRules chạy dần 1 vạch / 8 tick).
/// </summary>
public class Hud : MonoBehaviour
{
    [SerializeField] private TMP_Text statusLine;
    [SerializeField] private TMP_Text heartsText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private Image[] playerTicks = new Image[0];
    [SerializeField] private Image[] enemyTicks = new Image[0];
    [Header("CVGP 12")]
    [SerializeField] private Sprite playerTickFull;
    [SerializeField] private Sprite enemyTickFull;
    [Tooltip("Vạch đã mất — sprite TẠM (Tools/cvti/make_hud_temp.py): chưa có sheet HUD bản gốc.")]
    [SerializeField] private Sprite tickEmpty;
    [SerializeField] private Image subWeaponIcon;
    [Tooltip("Theo eSubWeapon: None, Dagger, Axe, HolyWater, Cross, Stopwatch (sprite Item — CVGP 05).")]
    [SerializeField] private Sprite[] subWeaponSprites = new Sprite[0];
    [Tooltip("Theo eSubWeapon: lật ngang icon (dao — video Map_1 0:20 mũi dao sang phải).")]
    [SerializeField] private bool[] subWeaponFlipX = new bool[0];
    [SerializeField] private Image shotIcon;
    [SerializeField] private Sprite doubleShotSprite;
    [SerializeField] private Sprite tripleShotSprite;
    [Tooltip("CVGP/NesPaletteSwap — Hud tạo bản sao vật liệu cho icon II/III.")]
    [SerializeField] private Material paletteSwapMaterial;
    [SerializeField] private Color32[] shotPaletteFrom = new Color32[0];
    [SerializeField] private Color32[] shotPaletteTo = new Color32[0];

    private Vector2 subIconPos;
    private bool subIconPosSaved;

    private void Awake()
    {
        if (shotIcon != null && paletteSwapMaterial != null && shotPaletteFrom.Length > 0)
        {
            var m = new Material(paletteSwapMaterial) { name = "HudShotPalette" };
            for (int i = 0; i < 3; i++)
            {
                m.SetColor("_From" + i, i < shotPaletteFrom.Length ? (Color)shotPaletteFrom[i] : Color.clear);
                m.SetColor("_To" + i, i < shotPaletteTo.Length ? (Color)shotPaletteTo[i] : Color.clear);
            }
            m.SetFloat("_Swap", 1f);
            shotIcon.material = m;
        }
    }

    private void OnEnable()
    {
        GameSession.Current.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        GameSession.Current.Changed -= Refresh;
    }

    public void Refresh()
    {
        var s = GameSession.Current;
        if (statusLine != null)
            statusLine.SetText("SCORE-" + s.score.ToString("000000") + " TIME " + s.time.ToString("0000") + " STAGE " + s.stage.ToString("00"));
        if (heartsText != null)
            heartsText.SetText("-" + s.hearts.ToString("00"));
        if (livesText != null)
            livesText.SetText("P-" + s.lives.ToString("00"));
        SetTicks(playerTicks, s.ShownPlayerHp, playerTickFull);
        SetTicks(enemyTicks, s.ShownEnemyHp, enemyTickFull);

        if (subWeaponIcon != null)
        {
            int w = (int)s.subWeapon;
            var sp = w > 0 && w < subWeaponSprites.Length ? subWeaponSprites[w] : null;
            subWeaponIcon.sprite = sp;
            subWeaponIcon.enabled = sp != null;
            // lật quanh góc trên-trái: scale −1 rồi dời đúng bề rộng icon
            var rt = subWeaponIcon.rectTransform;
            if (!subIconPosSaved)
            {
                subIconPos = rt.anchoredPosition;
                subIconPosSaved = true;
            }
            bool flip = w > 0 && w < subWeaponFlipX.Length && subWeaponFlipX[w];
            rt.localScale = new Vector3(flip ? -1f : 1f, 1f, 1f);
            rt.anchoredPosition = subIconPos + new Vector2(flip ? rt.sizeDelta.x : 0f, 0f);
        }
        if (shotIcon != null)
        {
            var sp = s.shotMultiplier >= 3 ? tripleShotSprite : s.shotMultiplier == 2 ? doubleShotSprite : null;
            shotIcon.sprite = sp;
            shotIcon.enabled = sp != null && s.shotIconVisible;
        }
    }

    // Vạch còn = sprite đầy; đã mất = khung rỗng (chưa gán tickEmpty ⇒ không vẽ như CVTI).
    void SetTicks(Image[] ticks, int hp, Sprite full)
    {
        for (int i = 0; i < ticks.Length; i++)
        {
            var t = ticks[i];
            if (t == null)
                continue;
            bool on = i < hp;
            if (on && full != null)
                t.sprite = full;
            else if (!on && tickEmpty != null)
                t.sprite = tickEmpty;
            t.enabled = on || tickEmpty != null;
        }
    }
}
