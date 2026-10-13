using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Nhóm thực thể = thứ tự tick trong StageWorld (CVGP_PLAN §5.1) — đừng đổi thứ tự giá trị.</summary>
public enum eEntityGroup
{
    Player = 0,        // Simon (state machine)
    PlayerWeapon = 1,  // vũ khí phụ đã ném, đạn của Simon
    Enemy = 2,         // enemy + boss (+ thực thể phụ của boss)
    EnemyShot = 3,     // đạn enemy/boss
    Item = 4,          // item rơi
    Prop = 5,          // nến, đuốc, khối phá, cửa…
    Effect = 6,        // hiệu ứng, chữ điểm (không va chạm)
}

/// <summary>Lý do thực thể rời thế giới (SpawnPoint dựa vào đây để quyết định hồi sinh).</summary>
public enum eKillReason
{
    Killed,     // bị hạ / tự kết thúc trong gameplay
    Offscreen,  // ra khỏi khung camera + lề
    Cleared,    // dọn khi đổi section / tắt StageWorld
}

/// <summary>
/// Thực thể gameplay (CVGP 07) — MonoBehaviour KHÔNG Update: StageWorld gọi OnTick() theo nhóm, thứ tự cố định (H4/H7).
/// Vị trí logic = pivot (x tâm thân, y = MẶT SÀN dưới chân — hàng pixel đầu tiên đặc dưới chân khi đứng) trong toạ độ px
/// section (gốc trên-trái vùng 192 dòng), lưu fixed 1/256 px (fx, fy). Hộp quy ước quay TRÁI như sheet (y âm = lên):
/// quay phải ⇒ lật quanh pivot (xMin' = −(xMin + w)); hộp body mặc định (−8, −32, 16, 32) phủ hàng Y−32..Y−1.
/// Vẽ: transform.localPosition = ToWorld(X + viewOffset.x, Y + viewOffset.y) dưới gốc thực thể của StageWorld (NES (0,32)).
/// Lớp con: override OnSpawned (đặt lại MỌI trạng thái — thực thể có thể lấy lại từ pool), OnTick (logic rồi base.OnTick()
/// để chạy AnimPlayer), OnHurt/OnHitLanded (combat), OnDeadlyTile (nước/hố).
/// </summary>
[DisallowMultipleComponent]
public class Entity : MonoBehaviour
{
    public const int GROUP_COUNT = 7;

    [SerializeField] protected eEntityGroup group;
    [Tooltip("Hộp va chạm địa hình (px quanh pivot, quay TRÁI, y âm = lên). Hàng chân = Y − 1, hàng dò sàn = Y.")]
    [SerializeField] protected RectInt body = new RectInt(-8, -32, 16, 32);
    [Tooltip("Renderer chính (thường trên chính GameObject này). Null = không vẽ.")]
    [SerializeField] protected SpriteRenderer view;
    [Tooltip("Pivot sprite vẽ ở (X + x, Y + y). Sprite nhân vật có hàng đáy trong suốt (CVTI §4.6: chân sàn 192 ↔ pivot 193) ⇒ (0, 1).")]
    [SerializeField] protected Vector2Int viewOffset = new Vector2Int(0, 1);
    [SerializeField] protected AnimSet animSet;
    [Tooltip("Sau mỗi tick, chạm ô Water/Pit/Hazard hoặc rơi khỏi đáy section ⇒ OnDeadlyTile.")]
    [SerializeField] protected bool checkDeadlyTiles;
    [Tooltip("Tự huỷ (eKillReason.Offscreen) khi ra khỏi khung camera quá lề — đạn, hiệu ứng.")]
    [SerializeField] protected bool despawnOffscreen;
    [SerializeField] protected int offscreenMargin = 32;
    [Tooltip("Số tick bất tử sau mỗi đòn nhận (0 = không). Simon đọc ConfigSimon.InvincibleTicks ở OnSpawned.")]
    [SerializeField] protected int invincibleOnHit;

    /// <summary>Vị trí fixed 1/256 px trong section.</summary>
    [NonSerialized] public int fx, fy;
    /// <summary>Vận tốc fixed/tick — tiện cho lớp con (lõi không tự cộng).</summary>
    [NonSerialized] public int vx, vy;

    private readonly List<Hitbox> hitboxes = new List<Hitbox>(2);
    private readonly List<Hurtbox> hurtboxes = new List<Hurtbox>(2);
    private bool built;

    public eEntityGroup Group => group;
    /// <summary>−1 = quay trái (như sheet), +1 = quay phải.</summary>
    public int Facing { get; set; } = -1;
    public int X => Fixed.ToPx(fx);
    public int Y => Fixed.ToPx(fy);
    public StageWorld World { get; private set; }
    /// <summary>Đang ở trong thế giới (đã Register, chưa Kill).</summary>
    public bool Alive { get; private set; }
    public eKillReason KillReason { get; private set; }
    public int InvincibleTicks { get; set; }
    public int InvincibleOnHit { get => invincibleOnHit; set => invincibleOnHit = value; }
    public RectInt Body { get => body; set => body = value; }
    public bool CheckDeadlyTiles => checkDeadlyTiles;
    public bool DespawnOffscreen => despawnOffscreen;
    public int OffscreenMargin => offscreenMargin;
    public AnimPlayer Anim { get; private set; }
    public AnimSet AnimSet => animSet;
    public SpriteRenderer View => view;
    public IReadOnlyList<Hitbox> Hitboxes => hitboxes;
    public IReadOnlyList<Hurtbox> Hurtboxes => hurtboxes;
    /// <summary>Prefab gốc nếu lấy từ EntityPool (null = đặt sẵn trong scene).</summary>
    public Entity SourcePrefab { get; internal set; }
    public SpawnPoint Spawner { get; internal set; }
    /// <summary>Tick (StageWorld.TickCount) lúc Register — thực thể sinh trong tick T bắt đầu OnTick từ T+1.</summary>
    public int SpawnTick { get; private set; }

    /// <summary>Hộp body đã lật theo hướng, toạ độ px section.</summary>
    public RectInt BodyRect => ToSection(body);

    /// <summary>Tên state hiện tại cho debug overlay.</summary>
    public virtual string DebugState => "";

    protected virtual void Awake()
    {
        EnsureBuilt();
    }

    private void EnsureBuilt()
    {
        if (built)
            return;
        built = true;
        if (view != null)
            Anim = new AnimPlayer(this, view) { Set = animSet };
        OnBuild();
    }

    /// <summary>Gọi 1 lần (Awake): tạo Hitbox/Hurtbox (AddHitbox/AddHurtbox), state machine…</summary>
    protected virtual void OnBuild()
    {
    }

    protected Hitbox AddHitbox(eHitLayer layer, RectInt local)
    {
        var h = new Hitbox(this, layer, local);
        hitboxes.Add(h);
        return h;
    }

    protected Hurtbox AddHurtbox(eHurtLayer layer, RectInt local)
    {
        var h = new Hurtbox(this, layer, local);
        hurtboxes.Add(h);
        return h;
    }

    public void SetPositionPx(int x, int y)
    {
        fx = Fixed.FromPx(x);
        fy = Fixed.FromPx(y);
    }

    /// <summary>Hộp cục bộ (quay trái) → px section theo hướng hiện tại.</summary>
    public RectInt ToSection(RectInt local)
    {
        int x = Facing > 0 ? -(local.x + local.width) : local.x;
        return new RectInt(X + x, Y + local.y, local.width, local.height);
    }

    // ---------------------------------------------------------------- vòng đời (StageWorld gọi)

    internal void Attach(StageWorld world, int tick)
    {
        EnsureBuilt();
        World = world;
        Alive = true;
        SpawnTick = tick;
        InvincibleTicks = 0;
        for (int i = 0; i < hitboxes.Count; i++)
            hitboxes[i].ResetHits();
        OnSpawned();
    }

    internal void Detach()
    {
        OnRemoved();
        World = null;
        Spawner = null;
    }

    /// <summary>Rời thế giới: hết Tick/va chạm ngay; StageWorld gỡ khỏi danh sách cuối tick (trả pool / tắt GameObject).</summary>
    public void Kill(eKillReason reason = eKillReason.Killed)
    {
        if (!Alive)
            return;
        Alive = false;
        KillReason = reason;
    }

    /// <summary>Vừa vào thế giới (vị trí/hướng đã đặt). Đặt lại MỌI trạng thái ở đây (pool tái dùng instance).</summary>
    protected virtual void OnSpawned()
    {
    }

    /// <summary>Vừa bị gỡ khỏi thế giới (sau Kill, cuối tick).</summary>
    protected virtual void OnRemoved()
    {
    }

    /// <summary>1 tick logic. Lớp con: state machine rồi base.OnTick() (chạy AnimPlayer).</summary>
    public virtual void OnTick()
    {
        if (Anim != null)
            Anim.Tick();
    }

    /// <summary>Có bị đóng băng không (thế giới: nhặt roi; enemy: Stopwatch). Boss miễn nhiễm Stopwatch thì override.</summary>
    public virtual bool IsFrozenBy(bool worldFrozen, bool enemyFrozen)
    {
        return worldFrozen || (enemyFrozen && (group == eEntityGroup.Enemy || group == eEntityGroup.EnemyShot));
    }

    /// <summary>Bị hitbox đối phương chạm (đã qua ma trận nhóm, bất tử, 1 lần/đòn). Trả true = nhận đòn ⇒ bất tử invincibleOnHit tick + sự kiện Hit.</summary>
    public virtual bool OnHurt(ref HitInfo hit)
    {
        return false;
    }

    /// <summary>Hitbox của mình vừa gây 1 đòn được nhận.</summary>
    public virtual void OnHitLanded(ref HitInfo hit)
    {
    }

    /// <summary>Chạm ô chết (Water/Pit/Hazard) hoặc rơi khỏi đáy section (Pit). Mặc định: Kill.</summary>
    public virtual void OnDeadlyTile(eTileCollision kind)
    {
        Kill();
    }

    /// <summary>Đặt transform + sprite theo trạng thái logic (cuối mỗi tick).</summary>
    public virtual void SyncView()
    {
        transform.localPosition = NesCoord.ToWorld(X + viewOffset.x, Y + viewOffset.y);
        if (Anim != null)
            Anim.Apply(Facing);
        else if (view != null)
            view.flipX = Facing > 0;
    }

    // ---------------------------------------------------------------- tiện ích vật lý (lưới của section đang chơi)

    /// <summary>Đi ngang dx (fixed) có chặn từng px. Trả true nếu đụng tường (dừng sát tường, phần lẻ = 0).</summary>
    protected bool MoveX(int dx)
    {
        return World.Tiles.MoveX(this, dx);
    }

    /// <summary>Đi dọc dy (fixed; dương = xuống). Trả true nếu chạm sàn (dy &gt; 0) hoặc trần (dy &lt; 0, collideCeiling).</summary>
    protected bool MoveY(int dy, bool collideCeiling = true)
    {
        return World.Tiles.MoveY(this, dy, collideCeiling);
    }

    protected bool IsOnGround()
    {
        return World.Tiles.IsOnGround(this);
    }
}
