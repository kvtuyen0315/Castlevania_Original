#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// CVM2 09 — dựng PHÒNG BOSS THẬT Map 2 lúc chạy (CHỈ Editor, scene Arena — Player = Simon; BossB24Probe / Map2BossProbe dùng chung).
/// Section Block 2 "Medusa" (Cvm2TerrainProbe.Setup — prefab do CVM2/Build Map2 Tilemaps sinh, KHÔNG sửa) + BossArena đọc từ
/// Tools/cvm2/map2_boss.json (nguồn duy nhất — routine 10 đưa vào Map2Objects.json). Stage 6 (sát thương 2 vạch). Gỡ: Leave().
/// </summary>
public static class Map2BossRoom
{
    public const string JSON = "Tools/cvm2/map2_boss.json";
    public const string SECTION = "Medusa";

    [Serializable] private class Doc { public Sec[] sections; }
    [Serializable] private class Sec { public string id; public ArenaSpec[] bossArenas; }

    [Serializable]
    public class ArenaSpec
    {
        public string name, prefab, src;
        public int zoneMinX, zoneMaxX;
        public bool useConfigPosition, screenWalls;
        public int[] bossPosition;
    }

    /// <summary>Mục bossArenas đầu tiên của section Medusa trong map2_boss.json (null nếu thiếu).</summary>
    public static ArenaSpec Spec()
    {
        string p = Path.Combine(Path.GetDirectoryName(Application.dataPath), JSON);
        if (!File.Exists(p))
        {
            Debug.LogError("[Map2BossRoom] thiếu " + JSON);
            return null;
        }
        var d = JsonUtility.FromJson<Doc>(File.ReadAllText(p));
        if (d != null && d.sections != null)
            foreach (var s in d.sections)
                if (s.id == SECTION && s.bossArenas != null && s.bossArenas.Length > 0)
                    return s.bossArenas[0];
        return null;
    }

    /// <summary>Warp Simon vào section Medusa tại (x, y) + BossArena theo JSON (host = GameObject giữ BossArena — huỷ khi xong).
    /// spawnNow ⇒ Medusa sinh ngay (ẩn trong tượng) trước tick đầu.</summary>
    public static BossArena Enter(StageWorld w, int x, int y, int facing, bool spawnNow, out GameObject host)
    {
        host = null;
        Cvm2TerrainProbe.Setup();
        var sec = Cvm2TerrainProbe.Find(SECTION);
        var spec = Spec();
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
        w.Stage = 6;
        host = new GameObject(spec.name + " (CVM2 09 thử)");
        host.transform.SetParent(w.transform, false);   // KHÔNG làm con section: Warp sau (cùng frame) sẽ gom lại BossArena cũ chưa huỷ
        var a = host.AddComponent<BossArena>();
        a.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<BossBase>(spec.prefab);
        a.zoneMinX = spec.zoneMinX;
        a.zoneMaxX = spec.zoneMaxX;
        a.useConfigPosition = spec.useConfigPosition;
        a.bossPosition = new Vector2Int(spec.bossPosition[0], spec.bossPosition[1]);
        a.screenWalls = spec.screenWalls;
        if (a.prefab == null)
            Debug.LogError("[Map2BossRoom] thiếu prefab " + spec.prefab);
        w.AddSpawner(a);
        if (spawnNow)
            a.SpawnBoss(w);
        return a;
    }

    /// <summary>Số tile nền còn trong vùng px (x, y, w, h) của section đang chơi (kiểm xoá đầu tượng).</summary>
    public static int TilesIn(StageWorld w, RectInt r)
    {
        var s = w.Section;
        if (s == null || s.bg == null)
            return -1;
        int n = 0;
        for (int row = r.y / 8; row < (r.y + r.height + 7) / 8; row++)
            for (int c = r.x / 8; c < (r.x + r.width + 7) / 8; c++)
                if (s.bg.GetTile(new Vector3Int(c, -row - 1, 0)) != null)
                    n++;
        return n;
    }
}
#endif
