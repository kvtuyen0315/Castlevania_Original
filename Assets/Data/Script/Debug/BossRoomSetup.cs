#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// CVGP 19 — dựng phòng boss thử trong scene Arena lúc chạy (CHỈ Editor; menu CVGP/Spawn Boss/* + BossB24Probe dùng chung). Không lưu
/// scene: lần đầu nạp prefab section Assets/Data/Prefabs/Level/Test/&lt;phòng&gt;.prefab (Tools/cvgp/build_boss_rooms.py + menu CVGP/Build
/// Arena Boss Rooms) làm con StageWorld (tắt) + StageWorld.AddSection, rồi Warp Simon vào; BossArena tạm (con StageWorld, AddSpawner) với
/// prefab boss ở vị trí quy ước phòng thử; camera khoá cx 0 (bossLockCx 0 ⇒ tường mép màn bật); Stage = stage boss (sát thương theo stage).
/// </summary>
public static class BossRoomSetup
{
    public const string ROOM_DIR = "Assets/Data/Prefabs/Level/Test/";
    public const string BOSS_DIR = "Assets/Data/Prefabs/Boss/";

    /// <summary>Thông số 1 phòng thử: section, prefab boss, vị trí boss (px section), Simon vào, stage wiki.</summary>
    public struct Room
    {
        public string section, boss;
        public Vector2Int bossPos, simon;
        public int facing, stage;
    }

    // QUY ƯỚC phòng thử (không có bản đồ Block 2–4): tượng Medusa giữa phòng (wiki), Mummy con đầu bên phải (con 2 cách 40 về trái),
    // Monster nửa phải (wiki). Stage = stage boss wiki (6 / 9 / 12) ⇒ sát thương 2 / 3 / 3 vạch.
    public static readonly Room MEDUSA = new Room { section = "BossMedusa", boss = "QueenMedusa", bossPos = new Vector2Int(128, 176), simon = new Vector2Int(32, 176), facing = 1, stage = 6 };
    public static readonly Room MUMMY = new Room { section = "BossMummy", boss = "MummyMan", bossPos = new Vector2Int(216, 176), simon = new Vector2Int(144, 112), facing = 1, stage = 9 };
    public static readonly Room FRANK = new Room { section = "BossFrank", boss = "Frankenstein", bossPos = new Vector2Int(176, 176), simon = new Vector2Int(16, 144), facing = 1, stage = 12 };

    public static Room ByName(string n)
    {
        switch (n)
        {
            case "QueenMedusa": return MEDUSA;
            case "MummyMan": return MUMMY;
            default: return FRANK;
        }
    }

    /// <summary>Section phòng trong StageWorld (nạp prefab lần đầu).</summary>
    public static LevelSection EnsureSection(StageWorld w, string name)
    {
        foreach (var s in w.Sections)
            if (s.data != null && s.data.name == name)
                return s;
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelSection>(ROOM_DIR + name + ".prefab");
        if (prefab == null)
        {
            Debug.LogError("[CVGP] thiếu " + ROOM_DIR + name + ".prefab — chạy CVGP/Build Arena Boss Rooms");
            return null;
        }
        var sec = Object.Instantiate(prefab, w.transform, false);
        sec.name = name;
        sec.gameObject.SetActive(false);
        w.AddSection(sec);
        return sec;
    }

    /// <summary>Warp vào phòng + BossArena tạm (trả về; spawnNow ⇒ boss sinh ngay trước tick đầu). host = GameObject giữ BossArena (huỷ khi xong).</summary>
    public static BossArena Enter(StageWorld w, Room r, bool spawnNow, out GameObject host)
    {
        host = null;
        var sec = EnsureSection(w, r.section);
        if (sec == null)
            return null;
        w.Warp(sec, r.simon.x, r.simon.y);
        var simon = w.Player as SimonController;
        if (simon != null)
            simon.Teleport(r.simon.x, r.simon.y, r.facing);
        if (w.CameraCtl != null)
            w.CameraCtl.SnapToTarget();
        w.Stage = r.stage;
        host = new GameObject("BossArena (" + r.section + " thử)");
        host.transform.SetParent(w.transform, false);
        var a = host.AddComponent<BossArena>();
        a.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<BossBase>(BOSS_DIR + r.boss + ".prefab");
        a.bossPosition = r.bossPos;
        a.useConfigPosition = false;
        a.screenWalls = true;
        if (a.prefab == null)
            Debug.LogError("[CVGP] thiếu " + BOSS_DIR + r.boss + ".prefab — chạy CVGP/Build Boss Prefabs");
        w.AddSpawner(a);
        if (spawnNow)
            a.SpawnBoss(w);

        return a;
    }
}
#endif
