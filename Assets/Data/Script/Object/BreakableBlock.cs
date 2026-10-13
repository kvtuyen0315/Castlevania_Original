using System;
using UnityEngine;

/// <summary>
/// Khối gạch phá được (CVGP 11) — nhóm Prop, 1 thực thể / ô Breakable 16×16 của lưới va chạm (06). StageWorld tự sinh cho mọi ô
/// Breakable chưa vỡ khi nạp section (ItemCatalog.blockPrefab). Không vẽ gì (gạch là tile nền); pivot = đáy-giữa ô, hộp nhận đòn = ô.
/// Chỉ ROI phá được (wiki Items#Pork Chop: "phải phá bằng roi"). Vỡ: TileCollision.Break + xoá 4 tile 8×8 trên Tilemap
/// (LevelSection.ClearCell — lỗ đen như video 1:39.6) + 4 mảnh "brick_debris" bay (ConfigActorParam Item.BreakableBlock — xấp xỉ)
/// + SFX BreakWall + item hiện NGAY tại ô (video 1:39.6: thịt nướng thấy cùng frame khối vỡ) rồi rơi theo trọng lực.
/// Nội dung: ConfigDrop["&lt;Section&gt;_X&lt;cột&gt;_&lt;hàng&gt;"] (vd CryptA_X16_10) hoặc <see cref="content"/> (mặc định không có gì).
/// </summary>
public class BreakableBlock : Entity
{
    public const int CELL = LevelSectionData.CELL;

    [NonSerialized] public int col, row;
    [NonSerialized] public eItemId content;
    [NonSerialized] public bool broken;
    [NonSerialized] public eItemId dropped;
    [NonSerialized] public ItemPickup spawnedItem;

    public Hurtbox Hurt { get; private set; }
    public string ObjectId => (World != null && World.Section != null && World.Section.data != null ? World.Section.data.name : "?") + "_X" + col + "_" + row;
    public override string DebugState => (broken ? "Broken " : "Block ") + col + "," + row;

    protected override void OnBuild()
    {
        Hurt = AddHurtbox(eHurtLayer.Breakable, new RectInt(-CELL / 2, -CELL, CELL, CELL));
    }

    protected override void OnSpawned()
    {
        broken = false;
        dropped = eItemId.None;
        spawnedItem = null;
        content = eItemId.None;
        Hurt.active = true;
    }

    /// <summary>Gắn vào ô (col, row) — StageWorld gọi ngay sau Spawn; nội dung lấy từ ConfigDrop theo ObjectId.</summary>
    public void Bind(int c, int r)
    {
        col = c;
        row = r;
        SetPositionPx(c * CELL + CELL / 2, r * CELL + CELL);
        content = ItemDrops.ContentOf(World, ObjectId, eItemId.None);
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        var simon = hit.attacker as SimonController;
        if (broken || simon == null || hit.hitbox != simon.WhipHit)
            return false;
        Break();
        return true;
    }

    /// <summary>Phá khối (roi — hoặc harness/cheat).</summary>
    public void Break()
    {
        if (broken)
            return;
        broken = true;
        Hurt.active = false;
        if (World.Tiles != null)
            World.Tiles.Break(col, row);
        if (World.Section != null)
            World.Section.ClearCell(col, row);
        SpawnDebris();
        if (AudioManager.I != null)
            AudioManager.I.PlaySfx(eSfx.BreakWall);
        dropped = ItemDrops.Resolve(World, content);
        spawnedItem = ItemDrops.Spawn(World, dropped, X, Y - 1);
        Kill();
    }

    private void SpawnDebris()
    {
        var cfg = World.Config;
        int vxT = Fixed.FromFloat(ItemPickup.Param(cfg, eItemId.BreakableBlock, "debrisVxTop", 0.75f));
        int vyT = Fixed.FromFloat(ItemPickup.Param(cfg, eItemId.BreakableBlock, "debrisVyTop", -1.5f));
        int vxB = Fixed.FromFloat(ItemPickup.Param(cfg, eItemId.BreakableBlock, "debrisVxBottom", 0.375f));
        int vyB = Fixed.FromFloat(ItemPickup.Param(cfg, eItemId.BreakableBlock, "debrisVyBottom", -0.75f));
        int g = Fixed.FromFloat(ItemPickup.Param(cfg, eItemId.BreakableBlock, "debrisGravity", 0.1875f));
        int x0 = X - 4, x1 = X + 4, yT = Y - 12, yB = Y - 4;   // tâm 4 phần tư ô
        FxSprite.Spawn(World, "brick_debris", x0, yT, 0, -vxT, vyT, g);
        FxSprite.Spawn(World, "brick_debris", x1, yT, 0, vxT, vyT, g);
        FxSprite.Spawn(World, "brick_debris", x0, yB, 0, -vxB, vyB, g);
        FxSprite.Spawn(World, "brick_debris", x1, yB, 0, vxB, vyB, g);
    }

    public override void SyncView()
    {
        transform.localPosition = NesCoord.ToWorld(X, Y);
    }
}
