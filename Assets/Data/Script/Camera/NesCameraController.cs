using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera cuộn ngang theo section (CVGP 07) — điều khiển MainCamera sẵn có (giữ PixelPerfectCamera + con LetterboxClear, CVTI §4.1):
/// camera = NesCoord.CameraHome + (X / PPU, 0) với X = mép trái màn tính bằng px section (nguyên px ⇒ pixel-perfect).
/// Section luôn đặt gốc ở NES (0, 32) (LevelSection.PlaceForCamera(0)); camera dời thay vì dời section.
/// Luật bám (đo routine 07 — Tools/cvgp/measure_camera.py trên Map_1): X = kẹp(target.X − FollowScreenX, MinX, MaxX),
/// FollowScreenX = 128 (ConfigRule.CameraFollowScreenX) — pivot Simon luôn ở x màn 128 khi cuộn, CẢ HAI hướng, không vùng chết,
/// không trễ (cùng tick), kể cả trên cầu thang (0.5 px/tick) và khi nhảy.
/// Khoá: Lock(cx) (MinX = MaxX); autoBossLock ⇒ tự khoá khi X chạm LevelSectionData.bossLockCx (PB cx 512, video 3:17.4).
/// Cuộn kịch bản: ScrollTo(x, tốc độ fixed) — chuỗi qua cửa (128 px @ 0.90–0.91 px/tick, routine 15), bỏ qua biên.
/// Followers (HUD): dời cùng camera để giữ nguyên chỗ trên màn; ResetHome khi StageWorld tắt ⇒ Title/Intro thấy camera như cũ.
/// Không Update: StageWorld gọi Tick() ở bước 7 mỗi tick.
/// </summary>
[DisallowMultipleComponent]
public class NesCameraController : MonoBehaviour
{
    /// <summary>Mặc định khi Config chưa có — = số đo routine 07 (ConfigRule.CameraFollowScreenX).</summary>
    public const int DEFAULT_FOLLOW_X = 128;
    /// <summary>Dải HUD NES y 0–47 (vùng chơi nhìn thấy từ y 48 — CVGP_PLAN §3a): section đặt ở y 32 nên 16 dòng đầu nằm DƯỚI HUD.</summary>
    public const int HUD_HEIGHT = 48;
    /// <summary>Thứ tự vẽ nền HUD trong sorting layer HUD (dưới mọi chữ/vạch của HUD).</summary>
    public const int HUD_BACKDROP_ORDER = -1000;

    [Tooltip("Null ⇒ Camera.main (MainCamera của scene).")]
    [SerializeField] private Camera targetCamera;
    [Tooltip("Transform dời theo camera (giữ nguyên vị trí trên màn) — HUD của màn gameplay.")]
    [SerializeField] private List<Transform> followers = new List<Transform>();
    [Tooltip("Tự khoá khi X chạm bossLockCx của section (phòng boss).")]
    [SerializeField] private bool autoBossLock = true;
    [Tooltip("Nền đen NES y 0–47 dưới HUD (che 16 dòng đầu section) — dời theo camera. Tắt nếu màn tự có nền HUD.")]
    [SerializeField] private bool hudBackdrop = true;

    private readonly List<Vector3> followerHome = new List<Vector3>();
    private SpriteRenderer backdrop;
    private bool scripted;
    private int scrollFx;
    private int scrollTargetX;
    private int scrollSpeed;
    private int bossLockCx = -1;

    public int X { get; private set; }
    public int MinX { get; private set; }
    public int MaxX { get; private set; }
    public bool Locked { get; private set; }
    public bool IsScrolling => scripted;
    public Entity Target { get; set; }
    public int FollowScreenX { get; set; } = DEFAULT_FOLLOW_X;
    public Camera Cam => targetCamera;

    /// <summary>Gọi khi StageWorld bật: tìm camera, nhớ vị trí gốc của followers (camera đang ở CameraHome).</summary>
    public void Init(GameConfig config)
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        if (config != null && config.rules != null)
            FollowScreenX = config.rules.GetInt(eRuleParam.CameraFollowScreenX, DEFAULT_FOLLOW_X);
        followerHome.Clear();
        for (int i = 0; i < followers.Count; i++)
            followerHome.Add(followers[i] != null ? followers[i].position : Vector3.zero);
        X = 0;
        scripted = false;
        if (hudBackdrop && backdrop == null)
            CreateBackdrop();
    }

    /// <summary>Hình chữ nhật đen 256×48 px (không phải art — màu nền HUD của game là đen tuyền), layer HUD dưới cùng.</summary>
    private void CreateBackdrop()
    {
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "HudBackdrop" };
        tex.SetPixel(0, 0, Color.black);
        tex.Apply();
        var go = new GameObject("HudBackdrop");
        go.transform.SetParent(transform, false);
        backdrop = go.AddComponent<SpriteRenderer>();
        backdrop.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 1f), 1f);
        backdrop.sortingLayerName = "HUD";
        backdrop.sortingOrder = HUD_BACKDROP_ORDER;
        go.transform.localScale = new Vector3(NesCoord.SCREEN_W / (float)NesCoord.PPU, HUD_HEIGHT / (float)NesCoord.PPU, 1f);
        go.transform.position = NesCoord.ToWorld(0, 0);
        AddFollower(go.transform);
    }

    public void AddFollower(Transform t)
    {
        if (t == null || followers.Contains(t))
            return;
        followers.Add(t);
        followerHome.Add(t.position - Offset(X));
    }

    /// <summary>Biên theo section (bỏ khoá cũ).</summary>
    public void Bind(LevelSectionData data)
    {
        MinX = data.cameraMinX;
        MaxX = data.cameraMaxX;
        bossLockCx = data.bossLockCx;
        Locked = false;
        scripted = false;
    }

    public int Clamp(int x)
    {
        return x < MinX ? MinX : x > MaxX ? MaxX : x;
    }

    /// <summary>Cắt thẳng tới cx (kẹp biên).</summary>
    public void SnapTo(int cx)
    {
        scripted = false;
        X = Clamp(cx);
        Apply();
    }

    public void SnapToTarget()
    {
        SnapTo(Target != null ? Target.X - FollowScreenX : X);
    }

    public void Lock(int cx)
    {
        MinX = MaxX = cx;
        Locked = true;
        X = cx;
    }

    /// <summary>Cuộn kịch bản tới targetX với tốc độ speedFixed (1/256 px/tick), bỏ qua biên/target.</summary>
    public void ScrollTo(int targetX, int speedFixed)
    {
        scripted = true;
        scrollFx = Fixed.FromPx(X);
        scrollTargetX = targetX;
        scrollSpeed = Mathf.Max(1, speedFixed);
    }

    public void Tick()
    {
        if (scripted)
        {
            int dir = scrollTargetX > X ? 1 : scrollTargetX < X ? -1 : 0;
            scrollFx += dir * scrollSpeed;
            X = Fixed.ToPx(scrollFx);
            if (dir == 0 || (dir > 0 && X >= scrollTargetX) || (dir < 0 && X <= scrollTargetX))
            {
                X = scrollTargetX;
                scripted = false;
            }
        }
        else if (Target != null)
            X = Clamp(Target.X - FollowScreenX);
        if (autoBossLock && !Locked && bossLockCx >= 0 && X >= bossLockCx)
            Lock(bossLockCx);
        Apply();
    }

    /// <summary>Trả camera + followers về chỗ gốc (StageWorld tắt).</summary>
    public void ResetHome()
    {
        scripted = false;
        X = 0;
        Apply();
    }

    private static Vector3 Offset(int x)
    {
        return new Vector3(x / (float)NesCoord.PPU, 0f, 0f);
    }

    private void Apply()
    {
        var off = Offset(X);
        if (targetCamera != null)
            targetCamera.transform.position = NesCoord.CameraHome + off;
        for (int i = 0; i < followers.Count; i++)
            if (followers[i] != null)
                followers[i].position = followerHome[i] + off;
    }
}
