using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CVGP 07 — dựng scene thử Assets/Scenes/Test/Arena.unity (menu CVGP/Build Arena Scene; đã có scene ⇒ hỏi ghi đè).
///   MainCamera      : BẢN SAO từ Main.unity (PixelPerfectCamera + con LetterboxClear — CVTI §4.1 "scene test là bản sao")
///   Systems         : NesClock + NesPad (InputBindings.asset) — không có GameManager ⇒ StageWorld tự gọi NesPad.Tick
///   StageWorld      : StageWorld + NesCameraController + CvgpDebugOverlay; con: section Arena (bật) + 5 section Map 1 (tắt, để
///                     warp/chụp overlay — "1 prefab section Map 1 đặt tạm"); Entities/{Simon (bật), ArenaDummy (tắt)} + ArenaPlayerSelect
///                     (CVGP 08: menu CVGP/Arena/Player = ArenaDummy để chạy lại bộ kiểm 07); SpawnPoint bia ở Arena x 860
///   HUD             : instance HUD.prefab ở NES (0,0), là follower của camera (kiểm HUD bám camera khi cuộn)
/// Prefab: Assets/Data/Prefabs/Character/Simon.prefab (CVGP 08 — Simon thật) + prefab thử Assets/Data/Prefabs/Test/{ArenaDummy,ArenaTarget}.prefab
/// (tạo nếu thiếu, cập nhật trường nếu đã có).
/// Cần chạy trước: CVGP/Build Map1 Tilemaps (tileset) — builder tự gọi CVGP/Build Arena Section.
/// </summary>
public static class CvgpArenaBuilder
{
    public const string SCENE = "Assets/Scenes/Test/Arena.unity";
    const string MAIN_SCENE = "Assets/Scenes/Main.unity";
    const string PREFAB_DIR = "Assets/Data/Prefabs/Test";
    const string DUMMY_PREFAB = PREFAB_DIR + "/ArenaDummy.prefab";
    const string TARGET_PREFAB = PREFAB_DIR + "/ArenaTarget.prefab";
    public const string SIMON_PREFAB = "Assets/Data/Prefabs/Character/Simon.prefab";
    const string ARENA_SECTION = "Assets/Data/Prefabs/Level/Test/Arena.prefab";
    const string MAP1_DIR = "Assets/Data/Prefabs/Level/Block1/";
    static readonly string[] MAP1_SECTIONS = { "CastleEntrance", "EntranceHall", "CryptA", "CryptB", "PhantomBat" };
    const string HUD_PREFAB = "Assets/Data/Prefabs/UI/HUD.prefab";
    const string GAME_CONFIG = "Assets/Data/Config/GameConfig.asset";
    const string BINDINGS = "Assets/Data/Config/InputBindings.asset";
    const string SIMON_ANIM = "Assets/Data/Config/Anim/SimonAnimSet.asset";
    const string ZOMBIE_ANIM = "Assets/Data/Config/Anim/ZombieAnimSet.asset";
    /// <summary>Điểm sinh bia trong Arena (px section): sàn hàng 11, xa vùng kiểm đi/tường/thang.</summary>
    static readonly Vector2Int TARGET_SPAWN = new Vector2Int(860, 176);

    [MenuItem("CVGP/Build Arena Scene")]
    static void BuildMenu()
    {
        if (File.Exists(SCENE) && !EditorUtility.DisplayDialog("Arena", SCENE + " đã có — dựng lại (ghi đè)?", "Ghi đè", "Huỷ"))
            return;
        Debug.Log(Build(true));
    }

    /// <summary>Dựng prefab thử + scene. overwrite = false và scene đã có ⇒ chỉ dựng prefab.</summary>
    public static string Build(bool overwrite)
    {
        var arenaMsg = CvgpLevelBuilder.BuildArena();
        if (arenaMsg.Contains("lỗi"))
            return arenaMsg;
        if (EditorApplication.isPlaying)
            return "Arena: đang Play — thoát Play rồi dựng";
        EnsureFolder(PREFAB_DIR);
        BuildDummyPrefab();
        BuildTargetPrefab();
        BuildSimonPrefab();
        if (File.Exists(SCENE) && !overwrite)
            return arenaMsg + "; prefab thử OK; scene giữ nguyên";
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return "Arena: huỷ (scene đang mở chưa lưu)";
        BuildScene();
        return arenaMsg + "; prefab thử OK; scene " + SCENE + " đã dựng";
    }

    // ------------------------------------------------------------------ prefab thử

    static void BuildDummyPrefab()
    {
        var go = new GameObject("ArenaDummy");
        try
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Character";
            var d = go.AddComponent<ArenaDummy>();
            var so = new SerializedObject(d);
            so.FindProperty("group").enumValueIndex = (int)eEntityGroup.Player;
            so.FindProperty("body").rectIntValue = new RectInt(-8, -32, 16, 32);
            so.FindProperty("view").objectReferenceValue = sr;
            so.FindProperty("viewOffset").vector2IntValue = new Vector2Int(0, 1);
            so.FindProperty("animSet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AnimSet>(SIMON_ANIM);
            so.FindProperty("checkDeadlyTiles").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(go, DUMMY_PREFAB);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    /// <summary>Simon thật (CVGP 08): nhóm Player, body 16×32, pivot vẽ (0,1) như sprite CVTI, SimonAnimSet, chết khi chạm nước/hố.</summary>
    public static void BuildSimonPrefab()
    {
        EnsureFolder(Path.GetDirectoryName(SIMON_PREFAB).Replace('\\', '/'));
        var go = new GameObject("Simon");
        try
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Character";
            var c = go.AddComponent<SimonController>();
            var so = new SerializedObject(c);
            so.FindProperty("group").enumValueIndex = (int)eEntityGroup.Player;
            so.FindProperty("body").rectIntValue = new RectInt(-8, -32, 16, 32);
            so.FindProperty("view").objectReferenceValue = sr;
            so.FindProperty("viewOffset").vector2IntValue = new Vector2Int(0, 1);
            so.FindProperty("animSet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AnimSet>(SIMON_ANIM);
            so.FindProperty("checkDeadlyTiles").boolValue = true;
            so.FindProperty("despawnOffscreen").boolValue = false;
            so.FindProperty("autoRespawn").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(go, SIMON_PREFAB);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    static void BuildTargetPrefab()
    {
        var go = new GameObject("ArenaTarget");
        try
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Character";
            var t = go.AddComponent<ArenaTarget>();
            var so = new SerializedObject(t);
            so.FindProperty("group").enumValueIndex = (int)eEntityGroup.Enemy;
            so.FindProperty("body").rectIntValue = new RectInt(-8, -32, 16, 32);
            so.FindProperty("view").objectReferenceValue = sr;
            so.FindProperty("viewOffset").vector2IntValue = Vector2Int.zero;   // sprite Zombie cắt sát đáy (hàng 31 đục)
            so.FindProperty("animSet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AnimSet>(ZOMBIE_ANIM);
            so.FindProperty("despawnOffscreen").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(go, TARGET_PREFAB);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    // ------------------------------------------------------------------ scene

    static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        // nạp lại SAU NewScene (scene mới Single giải phóng asset không dùng ⇒ tham chiếu cũ chết)
        var dummyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DUMMY_PREFAB);
        var targetPrefab = AssetDatabase.LoadAssetAtPath<ArenaTarget>(TARGET_PREFAB);
        var simonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SIMON_PREFAB);

        // camera: bản sao từ Main.unity
        var main = EditorSceneManager.OpenScene(MAIN_SCENE, OpenSceneMode.Additive);
        GameObject camSrc = null;
        foreach (var root in main.GetRootGameObjects())
            if (root.name == "MainCamera")
                camSrc = root;
        SceneManager.SetActiveScene(scene);
        var cam = Object.Instantiate(camSrc);
        cam.name = "MainCamera";
        SceneManager.MoveGameObjectToScene(cam, scene);
        EditorSceneManager.CloseScene(main, true);

        var systems = new GameObject("Systems");
        var clockGo = new GameObject("NesClock");
        clockGo.transform.SetParent(systems.transform, false);
        clockGo.AddComponent<NesClock>();
        var padGo = new GameObject("NesPad");
        padGo.transform.SetParent(systems.transform, false);
        var pad = padGo.AddComponent<NesPad>();
        var padSo = new SerializedObject(pad);
        padSo.FindProperty("bindings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<InputBindings>(BINDINGS);
        padSo.ApplyModifiedPropertiesWithoutUndo();

        var worldGo = new GameObject("StageWorld");
        var world = worldGo.AddComponent<StageWorld>();
        var camCtl = worldGo.AddComponent<NesCameraController>();
        var overlay = worldGo.AddComponent<CvgpDebugOverlay>();

        var arena = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ARENA_SECTION), scene);
        arena.transform.SetParent(worldGo.transform, false);
        foreach (var id in MAP1_SECTIONS)
        {
            var s = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(MAP1_DIR + id + ".prefab"), scene);
            s.transform.SetParent(worldGo.transform, false);
            s.SetActive(false);
        }
        var spGo = new GameObject("Spawn_ArenaTarget");
        spGo.transform.SetParent(arena.transform, false);
        spGo.transform.localPosition = NesCoord.ToWorld(TARGET_SPAWN.x, TARGET_SPAWN.y);
        var sp = spGo.AddComponent<SpawnPoint>();
        sp.prefab = targetPrefab;
        sp.position = TARGET_SPAWN;
        sp.facing = -1;

        var entities = new GameObject("Entities");
        entities.transform.SetParent(worldGo.transform, false);
        entities.transform.localPosition = NesCoord.ToWorld(0, LevelSectionData.SCREEN_Y);
        // Simon (Player mặc định) trước, ArenaDummy tắt — ArenaPlayerSelect bật 1 trong 2 ở Awake theo EditorPrefs
        var simon = (GameObject)PrefabUtility.InstantiatePrefab(simonPrefab, scene);
        simon.transform.SetParent(entities.transform, false);
        var simonSo = new SerializedObject(simon.GetComponent<SimonController>());
        simonSo.FindProperty("autoRespawn").boolValue = true;
        simonSo.ApplyModifiedPropertiesWithoutUndo();
        var dummy = (GameObject)PrefabUtility.InstantiatePrefab(dummyPrefab, scene);
        dummy.transform.SetParent(entities.transform, false);
        dummy.SetActive(false);
        var sel = entities.AddComponent<ArenaPlayerSelect>();
        var selSo = new SerializedObject(sel);
        selSo.FindProperty("simon").objectReferenceValue = simon;
        selSo.FindProperty("dummy").objectReferenceValue = dummy;
        selSo.ApplyModifiedPropertiesWithoutUndo();

        var hud = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HUD_PREFAB), scene);
        hud.transform.position = Vector3.zero;

        var wso = new SerializedObject(world);
        wso.FindProperty("fallbackConfig").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameConfig>(GAME_CONFIG);
        wso.FindProperty("cameraController").objectReferenceValue = camCtl;
        wso.FindProperty("startSection").objectReferenceValue = arena.GetComponent<LevelSection>();
        wso.FindProperty("entityRoot").objectReferenceValue = entities.transform;
        wso.ApplyModifiedPropertiesWithoutUndo();

        var cso = new SerializedObject(camCtl);
        cso.FindProperty("targetCamera").objectReferenceValue = cam.GetComponent<Camera>();
        var fl = cso.FindProperty("followers");
        fl.arraySize = 1;
        fl.GetArrayElementAtIndex(0).objectReferenceValue = hud.transform;
        cso.ApplyModifiedPropertiesWithoutUndo();

        var oso = new SerializedObject(overlay);
        oso.FindProperty("world").objectReferenceValue = world;
        oso.ApplyModifiedPropertiesWithoutUndo();

        EnsureFolder(Path.GetDirectoryName(SCENE).Replace('\\', '/'));
        EditorSceneManager.SaveScene(scene, SCENE);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
