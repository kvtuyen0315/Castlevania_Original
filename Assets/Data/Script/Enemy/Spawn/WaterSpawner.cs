using UnityEngine;

/// <summary>
/// Sinh Fish Man nhảy khỏi nước (CVGP 13). Video Map_1 Crypt B (12 lần nhảy): cột x thế giới = gridOffsetPx + gridStepPx·k (32 + 64k),
/// chỉ cột trong khung camera [cx + 16, cx + 240] có NƯỚC ở hàng mặt nước và cách Simon ≥ minPlayerDistPx; chọn ngẫu nhiên (RNG thế giới).
/// Tối đa maxAlive (2) con; mỗi "suất" có đồng hồ riêng: vào vùng chờ firstDelayTicks, con của suất bị gỡ ⇒ chờ respawnDelay(+span)
/// (video f3517: 2 con trồi cùng lúc). Pivot sinh = (x, surfaceY + spawnBelowSurfacePx) rồi Fishman.Emerge(surfaceY).
/// </summary>
public class WaterSpawner : EnemySpawner
{
    [Tooltip("Hàng mặt nước (Y px section; Crypt B = 160 — đỉnh ô nước hàng 10).")]
    public int surfaceY = 160;

    private int maxAlive, step, offset, minDist, below, respawnMin, respawnSpan, firstDelay;
    private bool loaded, wasInZone;
    private int[] slotTimer = new int[0];
    private EnemyBase[] slotEnemy = new EnemyBase[0];
    private int[] slotTick = new int[0];

    public override void ResetSpawner()
    {
        base.ResetSpawner();
        loaded = false;
        wasInZone = false;
    }

    private void Load(StageWorld w)
    {
        loaded = true;
        var cfg = Cfg(w);
        var id = Id;
        maxAlive = Mathf.Max(1, EnemyParams.Int(cfg, id, "maxAlive", 2));
        step = Mathf.Max(8, EnemyParams.Int(cfg, id, "gridStepPx", 64));
        offset = EnemyParams.Int(cfg, id, "gridOffsetPx", 32);
        minDist = EnemyParams.Int(cfg, id, "minPlayerDistPx", 32);
        below = EnemyParams.Int(cfg, id, "spawnBelowSurfacePx", 32);
        respawnMin = EnemyParams.Int(cfg, id, "respawnDelayMinTicks", 44);
        respawnSpan = EnemyParams.Int(cfg, id, "respawnDelaySpanTicks", 132);
        firstDelay = EnemyParams.Int(cfg, id, "firstDelayTicks", 0);
        slotTimer = new int[maxAlive];
        slotEnemy = new EnemyBase[maxAlive];
        slotTick = new int[maxAlive];
    }

    protected override void OnTick(StageWorld w, int cx)
    {
        if (!loaded)
            Load(w);
        for (int i = 0; i < maxAlive; i++)
        {
            var e = slotEnemy[i];
            if (e != null && (!e.Alive || e.SpawnTick != slotTick[i]))
            {
                slotEnemy[i] = null;
                slotTimer[i] = Roll(w, respawnMin, respawnSpan);
            }
        }
        if (!InZone)
        {
            wasInZone = false;
            return;
        }
        if (!wasInZone)
        {
            wasInZone = true;
            for (int i = 0; i < maxAlive; i++)
                if (slotEnemy[i] == null)
                    slotTimer[i] = firstDelay;
        }
        for (int i = 0; i < maxAlive; i++)
        {
            if (slotEnemy[i] != null)
                continue;
            if (slotTimer[i] > 0)
            {
                slotTimer[i]--;
                if (slotTimer[i] > 0)
                    continue;
            }
            int x = PickColumn(w, cx);
            if (x < 0)
                continue;
            var p = w.Player;
            int facing = p != null && p.X > x ? 1 : -1;
            var e = SpawnAt(w, x, surfaceY + below, facing);
            var fish = e as Fishman;
            if (fish != null)
                fish.Emerge(surfaceY);
            slotEnemy[i] = e;
            slotTick[i] = e != null ? e.SpawnTick : 0;
        }
    }

    private int PickColumn(StageWorld w, int cx)
    {
        var p = w.Player;
        int n = 0;
        for (int pass = 0; pass < 2; pass++)
        {
            int k = pass == 1 ? w.Rng.Next(n) : -1;
            if (pass == 1 && n == 0)
                return -1;
            int a = cx + 16 - offset, first = a <= 0 ? 0 : (a + step - 1) / step;
            for (int x = offset + first * step; x <= cx + NesCoord.SCREEN_W - 16; x += step)
            {
                if (!Usable(w, x, p))
                    continue;
                if (pass == 0)
                    n++;
                else if (k-- == 0)
                    return x;
            }
            if (n == 0)
                return -1;
        }
        return -1;
    }

    private bool Usable(StageWorld w, int x, Entity p)
    {
        if (x < 0 || (w.Tiles != null && x >= w.Tiles.Width))
            return false;
        if (w.Tiles != null && w.Tiles.TypeAt(x, surfaceY) != eTileCollision.Water)
            return false;
        if (p != null && Mathf.Abs(p.X - x) < minDist)
            return false;
        for (int i = 0; i < slotEnemy.Length; i++)
            if (slotEnemy[i] != null && slotEnemy[i].Alive && slotEnemy[i].X == x)
                return false;
        return true;
    }
}
