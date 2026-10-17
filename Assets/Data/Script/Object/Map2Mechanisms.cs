using UnityEngine;

/// <summary>
/// CVM2 06 — sinh cơ chế địa hình động của section đang chơi từ LevelSectionData.markers (CVM2 05):
///   kind "platformLane" ⇒ MovingPlatform (ConfigActorParam Object.MovingPlatform: centerX&lt;H&gt;, amplitude&lt;H&gt;, periodTicks&lt;H&gt;, startTick&lt;H&gt;)
///   kind "crusher"      ⇒ Crusher (Object.Crusher: maxFrame&lt;H&gt;, startTick&lt;H&gt; + chung downStepTicks, gridTicks)
/// &lt;H&gt; = hậu tố id marker (bỏ tiền tố "platform_lane_"/"crusher_" và dấu "_": platform_lane_HallB_1 ⇒ HallB1, crusher_2 ⇒ 2).
/// Mặt treo/mặt bệ = rect.y của marker. Gọi NGAY sau Warp/LoadSection (cùng tick — thực thể chạy từ tick sau, như SpawnSectionProps).
/// Routine 06 chỉ dùng ở harness (Map2MechProbe); CVM2 10: luồng section gọi qua MechSpawner (lớp &lt;Section&gt;_Objects, tick đầu sau khi nạp).
/// </summary>
public static class Map2Mechanisms
{
    public const string KIND_PLATFORM = "platformLane";
    public const string KIND_CRUSHER = "crusher";
    /// <summary>Prefab dựng bằng menu CVM2/Build Mech Prefabs (Cvm2MechBuilder).</summary>
    public const string PLATFORM_PREFAB = "Assets/Data/Prefabs/Level/Block2/Mech/MovingPlatform.prefab";
    public const string CRUSHER_PREFAB = "Assets/Data/Prefabs/Level/Block2/Mech/Crusher.prefab";

    public static string Suffix(string markerId)
    {
        string s = markerId ?? "";
        if (s.StartsWith("platform_lane_"))
            s = s.Substring("platform_lane_".Length);
        else if (s.StartsWith("crusher_"))
            s = s.Substring("crusher_".Length);
        return s.Replace("_", "");
    }

    /// <summary>Sinh bệ + bẫy cho mọi marker của section hiện tại. Trả số thực thể đã sinh (thiếu dòng Config ⇒ log lỗi, bỏ qua marker đó).</summary>
    public static int SpawnFromMarkers(StageWorld w, MovingPlatform platformPrefab, Crusher crusherPrefab)
    {
        if (w == null || w.Section == null || w.Section.data == null || w.Section.data.markers == null)
            return 0;
        var cfg = w.Config;
        int n = 0;
        foreach (var m in w.Section.data.markers)
        {
            if (m == null)
                continue;
            string h = Suffix(m.id);
            if (m.kind == KIND_PLATFORM && platformPrefab != null)
            {
                string a = ConfigActorParam.ActorKey(eObjectId.MovingPlatform);
                float c = Param(cfg, a, "centerX" + h), amp = Param(cfg, a, "amplitude" + h), per = Param(cfg, a, "periodTicks" + h);
                float st = Param(cfg, a, "startTick" + h);
                if (float.IsNaN(c) || float.IsNaN(amp) || float.IsNaN(per) || float.IsNaN(st))
                    continue;
                var p = (MovingPlatform)w.Spawn(platformPrefab, Mathf.RoundToInt(c), m.rect.y, -1);
                p.Configure(m.id, Mathf.RoundToInt(c), m.rect.y, Mathf.RoundToInt(amp), per, Mathf.RoundToInt(st));
                p.SyncView();
                n++;
            }
            else if (m.kind == KIND_CRUSHER && crusherPrefab != null)
            {
                string a = ConfigActorParam.ActorKey(eObjectId.Crusher);
                float k = Param(cfg, a, "maxFrame" + h), st = Param(cfg, a, "startTick" + h);
                float down = Param(cfg, a, "downStepTicks"), grid = Param(cfg, a, "gridTicks");
                if (float.IsNaN(k) || float.IsNaN(st) || float.IsNaN(down) || float.IsNaN(grid))
                    continue;
                int cx = m.rect.x + m.rect.width / 2;
                var cr = (Crusher)w.Spawn(crusherPrefab, cx, m.rect.y, -1);
                cr.Configure(m.id, cx, m.rect.y, Mathf.RoundToInt(k), Mathf.RoundToInt(down), Mathf.RoundToInt(grid), Mathf.RoundToInt(st));
                cr.SyncView();
                n++;
            }
        }
        return n;
    }

    /// <summary>Tham số bắt buộc (thiếu ⇒ log lỗi + NaN).</summary>
    public static float Param(GameConfig cfg, string actor, string key)
    {
        var r = cfg != null && cfg.actorParams != null ? cfg.actorParams.Find(actor, key) : null;
        if (r == null || !r.value.has)
        {
            Debug.LogError("[Config] thiếu ConfigActorParam " + actor + "." + key);
            return float.NaN;
        }
        return r.value.value;
    }
}
