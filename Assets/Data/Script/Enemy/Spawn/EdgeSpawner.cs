using UnityEngine;

/// <summary>Mép màn nơi EdgeSpawner sinh.</summary>
public enum eSpawnSide
{
    Right,            // mép phải (video Map_1: mọi Zombie — 31 lần)
    Left,
    Alternate,        // luân phiên phải/trái (wiki "từ cả hai phía" — QUY ƯỚC, không có video)
    FrontOfPlayer,    // mép trước mặt Simon; Simon cách mép đó < edgeScreenMarginPx ⇒ mép kia (video: dơi Crypt A 6/6)
    BehindPlayer,     // CVM2 08: mép SAU lưng Simon (video Map_2: Ghost 2/2 lần hiện ở x màn 224 / 30 = 96 px sau lưng Simon)
}

/// <summary>
/// Sinh enemy ở MÉP MÀN (CVGP 13) — Zombie (Ghoul) và dơi (Bat). Pivot sinh = cx + 256 − inset (phải) / cx + inset (trái), inset 8
/// (video: hiện nguyên hình sát mép); hướng = vào trong màn. Y: Zombie theo `rows` (mặt sàn px section — nhiều hàng ⇒ chọn ngẫu nhiên
/// trong các hàng có sàn tại cột sinh; video PB: gờ trên + sàn dưới); dơi theo Simon (`playerRelativeY`: tâm = chân Simon −
/// spawnAboveFeetPx). Nhịp (ConfigActorParam Enemy.&lt;id&gt;): vào vùng ⇒ chờ firstDelay(+span) ⇒ sinh NHÓM groupSize con cách nhau
/// groupGapTicks ⇒ nghỉ waveDelay(+span) ⇒ nhóm kế; tối đa maxAlive con sống (đủ thì chờ); có respawnDelayMinTicks ⇒ mỗi lần 1 con
/// bị gỡ, đặt lại chờ respawnDelay(+span) (dơi: 1 con/lần). releaseOnBlock = objectId khối gạch (BreakableBlock.ObjectId) ⇒ khối vỡ
/// thì sinh ngay 1 con (wiki Secrets: phá tường Pot Roast Stage 2 thả 1 dơi — video không thấy).
/// </summary>
public class EdgeSpawner : EnemySpawner
{
    public eSpawnSide side = eSpawnSide.Right;
    [Tooltip("Mặt sàn (Y px section) có thể sinh. Rỗng + !playerRelativeY ⇒ Y của Player.")]
    public int[] rows = new int[0];
    [Tooltip("Dơi: Y sinh = chân Simon − spawnAboveFeetPx (Config).")]
    public bool playerRelativeY;
    [Tooltip("ObjectId khối gạch (vd CryptA_X16_10): vỡ ⇒ sinh ngay 1 con.")]
    public string releaseOnBlock = "";
    [Tooltip("CVM2 10: tối đa số con trong 1 lần nạp section (0 = không giới hạn) — Ghost Hall C: video 1 con/vùng, không sinh lại.")]
    public int maxSpawns;
    /// <summary>Harness (CVM2 08 — so video): ≥ 0 ⇒ thay độ trễ con đầu (Roll firstDelay) bằng số tick này. Mặc định −1 = Config.</summary>
    [System.NonSerialized] public int forceFirstDelay = -1;

    // nhịp (đọc Config khi vào vùng)
    private int groupSize, groupGap, waveMin, waveSpan, respawnMin, respawnSpan, firstMin, firstSpan, maxAlive, inset, aboveFeet, edgeMargin;
    private bool loaded, wasInZone, respawnMode, forcePending;
    private int timer, pending, altSide = 1;

    public int Timer => timer;

    private void OnEnable()
    {
        BreakableBlock.Broken += OnBlockBroken;
    }

    private void OnDisable()
    {
        BreakableBlock.Broken -= OnBlockBroken;
    }

    private void OnBlockBroken(BreakableBlock b)
    {
        if (!string.IsNullOrEmpty(releaseOnBlock) && b.ObjectId == releaseOnBlock)
            forcePending = true;
    }

    public override void ResetSpawner()
    {
        base.ResetSpawner();
        loaded = false;
        wasInZone = false;
        forcePending = false;
        timer = 0;
        pending = 0;
        altSide = 1;
    }

    private void Load(StageWorld w)
    {
        loaded = true;
        var cfg = Cfg(w);
        var id = Id;
        groupSize = Mathf.Max(1, EnemyParams.IntOpt(cfg, id, "groupSize", 1));
        groupGap = EnemyParams.IntOpt(cfg, id, "groupGapTicks", 0);
        waveMin = EnemyParams.IntOpt(cfg, id, "waveDelayMinTicks", 0);
        waveSpan = EnemyParams.IntOpt(cfg, id, "waveDelaySpanTicks", 0);
        respawnMin = EnemyParams.IntOpt(cfg, id, "respawnDelayMinTicks", -1);
        respawnSpan = EnemyParams.IntOpt(cfg, id, "respawnDelaySpanTicks", 0);
        respawnMode = respawnMin >= 0;
        firstMin = EnemyParams.IntOpt(cfg, id, "firstDelayMinTicks", EnemyParams.IntOpt(cfg, id, "firstDelayTicks", 0));
        firstSpan = EnemyParams.IntOpt(cfg, id, "firstDelaySpanTicks", 0);
        maxAlive = Mathf.Max(1, EnemyParams.Int(cfg, id, "maxAlive", 3));
        inset = EnemyParams.Int(cfg, id, "spawnInsetPx", 8);
        aboveFeet = playerRelativeY ? EnemyParams.Int(cfg, id, "spawnAboveFeetPx", 14) : 0;
        edgeMargin = side == eSpawnSide.FrontOfPlayer ? EnemyParams.Int(cfg, id, "edgeScreenMarginPx", 64) : 0;
    }

    protected override void OnTick(StageWorld w, int cx)
    {
        if (!loaded)
            Load(w);
        if (respawnMode && RemovedThisTick > 0)
            timer = Mathf.Max(timer, Roll(w, respawnMin, respawnSpan));
        if (forcePending)
        {
            forcePending = false;
            Spawn(w, cx);
        }
        if (!InZone)
        {
            wasInZone = false;
            return;
        }
        if (!wasInZone)
        {
            wasInZone = true;
            timer = forceFirstDelay >= 0 ? forceFirstDelay : Roll(w, firstMin, firstSpan);
            pending = groupSize;
        }
        if (timer > 0)
        {
            timer--;
            if (timer > 0)
                return;
        }
        if (LiveCount >= maxAlive || (maxSpawns > 0 && SpawnCount >= maxSpawns))
            return;
        if (!Spawn(w, cx))
            return;
        pending--;
        if (pending > 0)
            timer = groupGap;
        else
        {
            pending = groupSize;
            timer = respawnMode ? 0 : Roll(w, waveMin, waveSpan);
        }
    }

    private int PickSide(StageWorld w, int cx)
    {
        switch (side)
        {
            case eSpawnSide.Left: return -1;
            case eSpawnSide.Alternate:
                altSide = -altSide;
                return -altSide;
            case eSpawnSide.BehindPlayer:
                {
                    var p = w.Player;
                    return p != null ? -p.Facing : -1;
                }
            case eSpawnSide.FrontOfPlayer:
                {
                    var p = w.Player;
                    int dir = p != null ? p.Facing : 1;
                    int sx = p != null ? p.X - cx : NesCoord.SCREEN_W / 2;
                    if (dir > 0 && NesCoord.SCREEN_W - sx < edgeMargin)
                        dir = -1;
                    else if (dir < 0 && sx < edgeMargin)
                        dir = 1;
                    return dir;
                }
            default: return 1;
        }
    }

    /// <summary>Sinh 1 con ở mép; trả false nếu không có chỗ (hàng không có sàn tại cột / ngoài section).</summary>
    private bool Spawn(StageWorld w, int cx)
    {
        int s = PickSide(w, cx);
        int x = s > 0 ? cx + NesCoord.SCREEN_W - inset : cx + inset;
        if (w.Tiles != null && (x < 0 || x >= w.Tiles.Width))
            return false;
        int y;
        var p = w.Player;
        if (playerRelativeY || rows == null || rows.Length == 0)
            y = (p != null ? p.Y : 0) - aboveFeet;
        else
        {
            int n = 0, pick = -1;
            for (int i = 0; i < rows.Length; i++)
                if (w.Tiles == null || w.Tiles.RowLandable(rows[i], x - 1, x))
                    n++;
            if (n == 0)
                return false;
            int k = n > 1 ? w.Rng.Next(n) : 0;
            for (int i = 0; i < rows.Length && pick < 0; i++)
                if (w.Tiles == null || w.Tiles.RowLandable(rows[i], x - 1, x))
                {
                    if (k == 0)
                        pick = rows[i];
                    k--;
                }
            y = pick;
        }
        return SpawnAt(w, x, y, -s) != null;
    }
}
