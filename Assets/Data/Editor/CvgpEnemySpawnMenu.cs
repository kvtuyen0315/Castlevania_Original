using UnityEditor;
using UnityEngine;

/// <summary>
/// CVGP 17 — menu thử enemy (chỉ Editor, trong Play, scene Arena hoặc màn chơi): CVGP/Spawn/&lt;Enemy&gt; sinh 1 con trước mặt Simon
/// (enemy đi đất: cùng mặt sàn Simon, cách 96 px; bay: tâm cao hơn chân Simon 24 px; Raven đậu cao hơn 40 px; Medusa Head ở mép màn
/// trước mặt). Spawn/Medusa Head Spawner thêm 1 EdgeSpawner tạm (FrontOfPlayer) như Stage 5. Không lưu gì vào scene/prefab.
/// </summary>
public static class CvgpEnemySpawnMenu
{
    const string ROOT = "CVGP/Spawn/";
    const int AHEAD = 96;

    [MenuItem(ROOT + "Spear Knight")] static void Knight() { Ground("SpearKnight"); }
    [MenuItem(ROOT + "Medusa Head")] static void Medusa() { Edge("MedusaHead"); }
    [MenuItem(ROOT + "Medusa Head Spawner")] static void MedusaSpawner() { AddSpawner("MedusaHead"); }
    [MenuItem(ROOT + "Bone Tower")] static void Tower() { Ground("BoneTower"); }
    [MenuItem(ROOT + "Ghost")] static void GhostM() { Air("Ghost", 24); }
    [MenuItem(ROOT + "Raven")] static void RavenM() { Air("Raven", 40); }
    [MenuItem(ROOT + "Skeleton")] static void SkeletonM() { Ground("Skeleton"); }
    [MenuItem(ROOT + "Fleaman")] static void FleamanM() { Ground("Fleaman"); }
    [MenuItem(ROOT + "Black Bat (treo)")] static void BlackBat() { Air("Bat", 48, true); }

    static StageWorld W => StageWorld.Current;

    static EnemyBase Prefab(string name)
    {
        var p = CvgpEnemyBuilder.Load<EnemyBase>(name);
        if (p == null)
            Debug.LogError("[CVGP] thiếu prefab " + name + " — chạy CVGP/Build Enemy Prefabs");
        return p;
    }

    static void Ground(string name)
    {
        var p = W.Player;
        Put(name, p.X + p.Facing * AHEAD, p.Y, -p.Facing, false);
    }

    static void Air(string name, int above, bool perch = false)
    {
        var p = W.Player;
        Put(name, p.X + p.Facing * AHEAD, p.Y - above, -p.Facing, perch);
    }

    static void Edge(string name)
    {
        var p = W.Player;
        int cx = W.CameraCtl != null ? W.CameraCtl.X : 0;
        int x = p.Facing > 0 ? cx + NesCoord.SCREEN_W - 8 : cx + 8;
        Put(name, x, p.Y - 24, -p.Facing, false);
    }

    static void Put(string name, int x, int y, int facing, bool perch)
    {
        var pf = Prefab(name);
        if (pf == null)
            return;
        var e = W.Spawn(pf, x, y, facing) as EnemyBase;
        var bat = e as Bat;
        if (bat != null && perch)
            bat.ForcePerch();
        CvgpDebug.Log("spawn " + name + " (" + x + "," + y + ")");
    }

    static void AddSpawner(string name)
    {
        var go = new GameObject("CVGP Spawn " + name + " (tạm)");
        go.transform.SetParent(W.transform, false);
        var sp = go.AddComponent<EdgeSpawner>();
        sp.prefab = Prefab(name);
        sp.side = eSpawnSide.FrontOfPlayer;
        sp.playerRelativeY = true;
        W.AddSpawner(sp);
        CvgpDebug.Log("thêm EdgeSpawner " + name + " (FrontOfPlayer)");
    }

    static bool Valid() { return Application.isPlaying && StageWorld.Current != null && StageWorld.Current.Player != null; }
    [MenuItem(ROOT + "Spear Knight", true)] static bool V1() { return Valid(); }
    [MenuItem(ROOT + "Medusa Head", true)] static bool V2() { return Valid(); }
    [MenuItem(ROOT + "Medusa Head Spawner", true)] static bool V3() { return Valid(); }
    [MenuItem(ROOT + "Bone Tower", true)] static bool V4() { return Valid(); }
    [MenuItem(ROOT + "Ghost", true)] static bool V5() { return Valid(); }
    [MenuItem(ROOT + "Raven", true)] static bool V6() { return Valid(); }
    [MenuItem(ROOT + "Skeleton", true)] static bool V7() { return Valid(); }
    [MenuItem(ROOT + "Fleaman", true)] static bool V8() { return Valid(); }
    [MenuItem(ROOT + "Black Bat (treo)", true)] static bool V9() { return Valid(); }
}
