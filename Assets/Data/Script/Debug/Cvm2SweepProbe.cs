#if UNITY_EDITOR
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Quét hồi quy giữa cụm CVM2 (routine 07) — CHỈ Editor, scene Arena (Player = Simon), đang Play. Đo hiệu năng cảnh ĐÔNG ở section Block 2:
///   Cvm2SweepProbe.RunCrowd("TowerHallB", ticks, seed)  ⇒ bệ (2) / bẫy (3) từ marker + enemy Block 2 + Simon bất tử ném nước thánh; CrowdReport()
/// Step TB/max, GC mỗi Step, thực thể tối đa, pool đã tạo, GameObject trước/sau (rò). Không thuộc bộ hồi quy (chỉ đo).
/// </summary>
public static class Cvm2SweepProbe
{
    const string ENEMY_DIR = "Assets/Data/Prefabs/Enemy/";

    public static bool CrowdDone { get; private set; } = true;
    static string crowdReport = "";
    static StageWorld world;
    static int target, ticks, maxEnt, maxShots, mechs, goBefore, stageSaved;
    static GameObject spawnerHost;

    static SimonController Simon => world != null ? world.Player as SimonController : null;

    static EnemyBase EP(string name)
    {
        return AssetDatabase.LoadAssetAtPath<EnemyBase>(ENEMY_DIR + name + ".prefab");
    }

    static EnemyBase Put(string name, int x, int y, int facing)
    {
        return world.Spawn(EP(name), x, y, facing) as EnemyBase;
    }

    public static int CountGameObjects()
    {
        return UnityEngine.Object.FindObjectsOfType<Transform>(true).Length;
    }

    public static string RunCrowd(string section, int tickCount, int seed)
    {
        world = StageWorld.Current;
        if (world == null || Simon == null)
            return "cần Play scene Arena (Player = Simon)";
        if (!CrowdDone)
            return "đang chạy";
        stageSaved = world.Stage;
        goBefore = CountGameObjects();
        var ses = GameSession.Current;
        ses.ResetForNewGame();
        ses.playerHp = GameSession.MAX_HP;
        ses.hearts = 99;
        ses.whipLevel = 3;
        ses.shotMultiplier = 3;
        ses.SetSubWeapon(eSubWeapon.HolyWater);
        ses.NotifyChanged();
        CvgpDebug.CheatInvincible = true;
        bool hallB = section == "TowerHallB";
        string enter = hallB ? Map2MechProbe.Enter("TowerHallB", 368, 80) : Map2MechProbe.Enter("TowerHallC", 400, 144);
        mechs = 0;
        foreach (var e in world.GroupList(eEntityGroup.Prop))
            if (e is MovingPlatform || e is Crusher)
                mechs++;
        world.Stage = hallB ? 5 : 6;
        if (hallB)
        {
            Put("SpearKnight", 420, 80, -1);
            Put("Ghost", 330, 60, -1);
            Put("Skeleton", 470, 80, -1);
            Put("Raven", 300, 50, -1);
        }
        else
        {
            Put("BoneTower", 248, 96, -1);
            Put("BoneTower", 312, 64, -1);
            Put("Ghost", 380, 90, -1);
            Put("SpearKnight", 520, 144, -1);
            Put("Skeleton", 450, 144, -1);
        }
        spawnerHost = new GameObject("Cvm2SweepProbe_Edge");
        spawnerHost.transform.SetParent(world.transform, false);
        var sp = spawnerHost.AddComponent<EdgeSpawner>();
        sp.prefab = EP("MedusaHead");
        sp.side = eSpawnSide.FrontOfPlayer;
        sp.playerRelativeY = true;
        world.AddSpawner(sp);
        world.Rng.Reset(seed);
        var rle = new StringBuilder();
        for (int n = 0; n < tickCount; n += 10)
            rle.Append(n > 0 ? " " : "").Append("Sub -*9");
        NesPad.I.PlayReplay(NesReplay.Decode(rle.ToString()));
        world.ResetPerf();
        StageWorld.MeasurePerf = true;
        target = tickCount;
        ticks = 0;
        maxEnt = maxShots = 0;
        CrowdDone = false;
        world.TickEnded += OnTick;
        return enter + "; cơ chế " + mechs;
    }

    static void OnTick(StageWorld w)
    {
        int ent = 0, sh = 0;
        for (int g = 0; g < Entity.GROUP_COUNT; g++)
            foreach (var e in w.GroupList(g))
                if (e.Alive)
                {
                    ent++;
                    if (e.Group == eEntityGroup.EnemyShot || e.Group == eEntityGroup.PlayerWeapon)
                        sh++;
                }
        maxEnt = Math.Max(maxEnt, ent);
        maxShots = Math.Max(maxShots, sh);
        if (++ticks < target)
            return;
        w.TickEnded -= OnTick;
        StageWorld.MeasurePerf = false;
        NesPad.I.StopReplay();
        int n = Math.Max(1, w.MeasuredSteps);
        crowdReport = "CROWD " + w.MeasuredSteps + " Step (" + mechs + " cơ chế Block 2 + enemy + nước thánh ×3): thực thể sống tối đa " + maxEnt
                      + " (vũ khí/đạn tối đa " + maxShots + "), Step TB " + (w.TotalStepMs / n).ToString("0.0000") + " ms, max " + w.MaxStepMs.ToString("0.000")
                      + " ms; GC tổng " + w.TotalStepAlloc + " B, Step có cấp phát " + w.AllocSteps + ", max " + w.MaxStepAlloc + " B; pool đã tạo "
                      + w.Pool.CreatedCount;
        if (spawnerHost != null)
        {
            var sp = spawnerHost.GetComponent<EnemySpawner>();
            if (sp != null)
                w.RemoveSpawner(sp);
            UnityEngine.Object.DestroyImmediate(spawnerHost);
            spawnerHost = null;
        }
        Map2MechProbe.BackToArena();
        crowdReport += "; GameObject trước " + goBefore + " / sau dọn " + CountGameObjects();
        CrowdDone = true;
    }

    public static string CrowdReport()
    {
        return CrowdDone ? crowdReport : "CROWD ĐANG CHẠY " + ticks + "/" + target;
    }
}
#endif
