using UnityEditor;
using UnityEngine;

/// <summary>
/// CVGP 19 (+ 20: Death, Count Dracula) — menu thử boss Block 2–6 (chỉ Editor, trong Play, scene Arena): CVGP/Spawn Boss/&lt;Boss&gt; ⇒ BossRoomSetup.Enter: nạp phòng thử
/// (Assets/Data/Prefabs/Level/Test/Boss*.prefab), Warp Simon vào, BossArena tạm sinh boss (khoá camera, thanh ENEMY, chết ⇒ quả cầu ⇒
/// tally nếu màn có StageRules). Không lưu gì vào scene/prefab. Về Arena: CVGP/Spawn Boss/Back To Arena.
/// </summary>
public static class CvgpBossSpawnMenu
{
    const string ROOT = "CVGP/Spawn Boss/";
    static GameObject host;

    [MenuItem(ROOT + "Queen Medusa")] static void Medusa() { Enter(BossRoomSetup.MEDUSA); }
    [MenuItem(ROOT + "Mummy Men")] static void Mummy() { Enter(BossRoomSetup.MUMMY); }
    [MenuItem(ROOT + "Frankenstein & Igor")] static void Frank() { Enter(BossRoomSetup.FRANK); }
    [MenuItem(ROOT + "Death (Grim Reaper)")] static void Death() { Enter(BossRoomSetup.DEATH); }
    [MenuItem(ROOT + "Count Dracula")] static void Dracula() { Enter(BossRoomSetup.DRACULA); }

    [MenuItem(ROOT + "Back To Arena")]
    static void Back()
    {
        var w = StageWorld.Current;
        Drop(w);
        foreach (var s in w.Sections)
            if (s.data != null && s.data.name == "Arena")
            {
                w.Warp(s, 48, 176);
                var simon = w.Player as SimonController;
                if (simon != null)
                    simon.Teleport(48, 176, 1);
                if (w.CameraCtl != null)
                    w.CameraCtl.SnapToTarget();
            }
    }

    static void Drop(StageWorld w)
    {
        if (host == null)
            return;
        var a = host.GetComponent<BossArena>();
        if (a != null)
            w.RemoveSpawner(a);
        Object.Destroy(host);
        host = null;
    }

    static void Enter(BossRoomSetup.Room r)
    {
        var w = StageWorld.Current;
        Drop(w);
        var a = BossRoomSetup.Enter(w, r, false, out host);
        Debug.Log("[CVGP] Spawn Boss: " + r.boss + " ở phòng " + r.section + (a != null ? "" : " — LỖI (xem console)"));
    }

    [MenuItem(ROOT + "Queen Medusa", true)] static bool V1() { return Ok(); }
    [MenuItem(ROOT + "Mummy Men", true)] static bool V2() { return Ok(); }
    [MenuItem(ROOT + "Frankenstein & Igor", true)] static bool V3() { return Ok(); }
    [MenuItem(ROOT + "Back To Arena", true)] static bool V4() { return Ok(); }
    [MenuItem(ROOT + "Death (Grim Reaper)", true)] static bool V5() { return Ok(); }
    [MenuItem(ROOT + "Count Dracula", true)] static bool V6() { return Ok(); }

    static bool Ok() { return Application.isPlaying && StageWorld.Current != null && StageWorld.Current.Player != null; }
}
