using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nền bộ sinh enemy theo vùng (CVGP 13, CVGP_PLAN §5.6 "SpawnZone") — component con của section (như SpawnPoint) hoặc thêm lúc chạy
/// (StageWorld.AddSpawner). StageWorld (bước 8, sau SpawnPoint) gọi Tick mỗi tick, BỎ QUA khi đóng băng thế giới / Stopwatch (QUY ƯỚC:
/// đồng hồ dừng cả bộ đếm sinh). Chỉ sinh khi Player (Simon) đứng trong vùng x [zoneMinX, zoneMaxX] px section. RNG = StageWorld.Rng
/// (tất định, H7). Số liệu nhịp sinh đọc ConfigActorParam "Enemy.&lt;id của prefab&gt;" (H6); component chỉ giữ hình học vùng/hàng.
/// Theo dõi enemy đã sinh theo (instance, SpawnTick) ⇒ pool tái dùng instance không bị đếm nhầm.
/// </summary>
public abstract class EnemySpawner : MonoBehaviour
{
    public EnemyBase prefab;
    [Tooltip("Vùng Simon phải đứng trong (px section, x tâm) để bộ sinh chạy.")]
    public int zoneMinX;
    public int zoneMaxX = 4096;
    [Tooltip("CVM2 10: giới hạn thêm theo Y Simon (mặt sàn, px section) — tách tầng trên/dưới cùng khoảng x (Ghost Hall C). Mặc định cả section.")]
    public int zoneMinY = -4096;
    public int zoneMaxY = 4096;
    public bool spawnEnabled = true;

    private readonly List<EnemyBase> live = new List<EnemyBase>(4);
    private readonly List<int> liveTick = new List<int>(4);

    public int SpawnCount { get; private set; }
    public int LiveCount => live.Count;
    /// <summary>Số enemy vừa bị gỡ trong lần Tick này (sau Prune).</summary>
    protected int RemovedThisTick { get; private set; }
    public bool InZone { get; private set; }

    /// <summary>Mỗi lần sinh (harness ghi tick + vị trí).</summary>
    public static event Action<EnemySpawner, EnemyBase> Spawned;

    protected GameConfig Cfg(StageWorld w) { return w.Config; }
    protected eEnemyId Id => prefab != null ? prefab.EnemyId : eEnemyId.None;

    public virtual void ResetSpawner()
    {
        live.Clear();
        liveTick.Clear();
        SpawnCount = 0;
        InZone = false;
        RemovedThisTick = 0;
    }

    internal void Tick(StageWorld w, int cx)
    {
        Prune();
        var p = w.Player;
        InZone = spawnEnabled && prefab != null && p != null && p.Alive && p.X >= zoneMinX && p.X <= zoneMaxX && p.Y >= zoneMinY && p.Y <= zoneMaxY;
        OnTick(w, cx);
    }

    protected abstract void OnTick(StageWorld w, int cx);

    private void Prune()
    {
        RemovedThisTick = 0;
        for (int i = live.Count - 1; i >= 0; i--)
        {
            var e = live[i];
            if (e == null || !e.Alive || e.SpawnTick != liveTick[i])
            {
                live.RemoveAt(i);
                liveTick.RemoveAt(i);
                RemovedThisTick++;
                OnLiveRemoved(i);
            }
        }
    }

    /// <summary>Phần tử thứ index của danh sách sống vừa bị gỡ (lớp con giữ suất riêng thì cập nhật).</summary>
    protected virtual void OnLiveRemoved(int index)
    {
    }

    public bool IsTracked(EnemyBase e)
    {
        for (int i = 0; i < live.Count; i++)
            if (live[i] == e && e.SpawnTick == liveTick[i])
                return true;
        return false;
    }

    protected EnemyBase SpawnAt(StageWorld w, int x, int y, int facing)
    {
        var e = w.Spawn(prefab, x, y, facing) as EnemyBase;
        if (e == null)
            return null;
        live.Add(e);
        liveTick.Add(e.SpawnTick);
        SpawnCount++;
        if (Spawned != null)
            Spawned(this, e);
        return e;
    }

    /// <summary>Số nguyên ngẫu nhiên đều [min, min + span] (span ≤ 0 ⇒ min).</summary>
    protected static int Roll(StageWorld w, int min, int span)
    {
        return span > 0 ? min + w.Rng.Next(span + 1) : min;
    }
}
