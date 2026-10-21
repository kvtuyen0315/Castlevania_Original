#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// CVM3 09 — dựng PHÒNG BOSS THẬT Map 3 (Mummy Men) lúc chạy (CHỈ Editor, scene Arena — Player = Simon; BossB24Probe / Map3BossProbe dùng
/// chung). Section Block 3 "MummyMan" (Cvm3TerrainProbe.Setup — prefab do CVM3/Build Map3 Tilemaps sinh, KHÔNG sửa) + BossArena đọc từ
/// Tools/cvm3/map3_boss.json (nguồn duy nhất — routine 10 đưa vào Map3Objects.json) + nến có content trong JSON (nến 1503 = tim nhỏ ⇒ II theo
/// luật ItemDrops). Khối Pot Roast (83,9) do StageWorld.SpawnSectionProps tự sinh (ConfigDrop MummyMan_X83_9). Stage 9 (sát thương 3 vạch).
/// Khuôn Map2BossRoom (CVM2 09). Gỡ: Cvm3TerrainProbe.Cleanup().
/// </summary>
public static class Map3BossRoom
{
    public const string JSON = "Tools/cvm3/map3_boss.json";
    public const string SECTION = "MummyMan";

    [Serializable] private class Doc { public Sec[] sections; }
    [Serializable] private class Sec { public string id; public Map2BossRoom.ArenaSpec[] bossArenas; public CandleSpec[] candles; }

    [Serializable]
    public class CandleSpec
    {
        public int x, y;
        public string content, src;
    }

    static Sec Load()
    {
        string p = Path.Combine(Path.GetDirectoryName(Application.dataPath), JSON);
        if (!File.Exists(p))
        {
            Debug.LogError("[Map3BossRoom] thiếu " + JSON);
            return null;
        }
        var d = JsonUtility.FromJson<Doc>(File.ReadAllText(p));
        if (d != null && d.sections != null)
            foreach (var s in d.sections)
                if (s.id == SECTION)
                    return s;
        return null;
    }

    /// <summary>Mục bossArenas đầu tiên của section MummyMan (null nếu thiếu).</summary>
    public static Map2BossRoom.ArenaSpec Spec()
    {
        var s = Load();
        return s != null && s.bossArenas != null && s.bossArenas.Length > 0 ? s.bossArenas[0] : null;
    }

    /// <summary>Warp Simon vào section MummyMan tại (x, y) + BossArena theo JSON (host = GameObject giữ BossArena — huỷ khi xong).
    /// spawnNow ⇒ 2 Mummy sinh ngay (đứng chờ) trước tick đầu; candles ⇒ dựng nến có content.</summary>
    public static BossArena Enter(StageWorld w, int x, int y, int facing, bool spawnNow, bool candles, out GameObject host)
    {
        host = null;
        Cvm3TerrainProbe.Setup();
        var sec = Cvm3TerrainProbe.Find(SECTION);
        var s = Load();
        var spec = s != null && s.bossArenas != null && s.bossArenas.Length > 0 ? s.bossArenas[0] : null;
        if (sec == null || spec == null)
            return null;
        w.Warp(sec, x, y);
        var simon = w.Player as SimonController;
        if (simon != null)
            simon.Teleport(x, y, facing);
        if (w.CameraCtl != null)
        {
            w.CameraCtl.Target = w.Player;
            w.CameraCtl.SnapToTarget();
        }
        w.Stage = 9;
        host = new GameObject(spec.name + " (CVM3 09 thử)");
        host.transform.SetParent(w.transform, false);   // KHÔNG làm con section: Warp sau (cùng frame) sẽ gom lại BossArena cũ chưa huỷ
        var a = host.AddComponent<BossArena>();
        a.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<BossBase>(spec.prefab);
        a.zoneMinX = spec.zoneMinX;
        a.zoneMaxX = spec.zoneMaxX;
        a.useConfigPosition = spec.useConfigPosition;
        a.bossPosition = new Vector2Int(spec.bossPosition[0], spec.bossPosition[1]);
        a.screenWalls = spec.screenWalls;
        if (a.prefab == null)
            Debug.LogError("[Map3BossRoom] thiếu prefab " + spec.prefab);
        w.AddSpawner(a);
        if (spawnNow)
            a.SpawnBoss(w);
        if (candles && s.candles != null && w.Items != null && w.Items.candlePrefab != null)
            foreach (var c in s.candles)
            {
                if (string.IsNullOrEmpty(c.content))
                    continue;
                var cp = w.Spawn(w.Items.candlePrefab, c.x, c.y, -1) as CandleProp;
                if (cp == null)
                    continue;
                cp.content = (eItemId)Enum.Parse(typeof(eItemId), c.content);
                cp.objectId = null;
            }
        return a;
    }
}
#endif
