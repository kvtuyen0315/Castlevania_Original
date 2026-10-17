using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CVGP 15 — lắp ghép Map 1.
///   • CVGP/Build Map1 Objects: đọc Assets/Data/Config/Level/Block1/Map1Objects.json (sinh bởi Tools/cvgp/gen_map1_objects.py —
///     bảng vị trí đo video, MAP1_PLACEMENT.md) ⇒ Assets/Data/Prefabs/Level/Block1/Objects/&lt;Section&gt;_Objects.prefab: nến/chậu
///     (SpawnPoint prefab Candle/Torch, dataKey = objectId ConfigDrop, không hồi sinh khi đã phá, hồi khi chỉ ra khỏi màn), báo (SpawnPoint),
///     EdgeSpawner/WaterSpawner (vùng sinh), kho báu bí mật (SpawnPoint SecretTrigger + SecretTriggerSpec), BossArena. Lớp vật thể là prefab
///     RIÊNG (không lồng vào prefab section của 06 — scene Arena dùng chung prefab section, thêm vật vào đó sẽ đổi kết quả bộ kiểm 07–14).
///     Nội dung so EditorJsonUtility với prefab cũ ⇒ không khác thì không ghi (chạy lại = 0 ghi).
///   • CVGP/Build Map1 Screens: dựng Assets/Data/Prefabs/Screens/{StageScreen, CastleMapScreen, Stage4Stub}.prefab (giữ GUID + fileID gốc)
///     và lắp vào Main.unity/ScreenRoot thay instance Stage1Stub (Stage1Stub.prefab + script giữ lại cho builder CVTI — không còn trong scene).
/// </summary>
public static class CvgpMap1Builder
{
    const string TABLE = "Assets/Data/Config/Level/Block1/Map1Objects.json";
    const string SECTION_DIR = "Assets/Data/Prefabs/Level/Block1/";
    const string OBJ_DIR = "Assets/Data/Prefabs/Level/Block1/Objects";
    const string SCREEN_DIR = "Assets/Data/Prefabs/Screens/";
    const string STAGE_SCREEN = SCREEN_DIR + "StageScreen.prefab";
    const string CASTLEMAP_SCREEN = SCREEN_DIR + "CastleMapScreen.prefab";
    const string STAGE4_SCREEN = SCREEN_DIR + "Stage4Stub.prefab";
    const string STAGE1_STUB = SCREEN_DIR + "Stage1Stub.prefab";
    const string MAIN_SCENE = "Assets/Scenes/Main.unity";
    const string HUD_PREFAB = "Assets/Data/Prefabs/UI/HUD.prefab";
    const string GAME_CONFIG = "Assets/Data/Config/GameConfig.asset";
    static readonly string[] SECTIONS = { "CastleEntrance", "EntranceHall", "CryptA", "CryptB", "PhantomBat" };

    // ------------------------------------------------------------------ bảng JSON (JsonUtility)
    // CVM2 10: thêm trường tuỳ chọn cho Map 2 (Point.param, Edge.zoneMinY/zoneMaxY/maxSpawns, Secret.riseSpeed, Section.mechSpawners,
    // Section.objectWindowInsetPx = khung hiện nến/enemy [cx + N, cx + 256 − N) ⇒ activate/despawnMargin −N) —
    // Map1Objects.json không có ⇒ giá trị 0 = hành vi/serialize cũ (prefab Block 1 dựng lại "không đổi").
    [Serializable] class Candle { public string id, kind, content, src; public int x, y; }
    [Serializable] class Point { public string name, prefab, src; public int x, y, facing, param; public bool respawn; }
    [Serializable] class Edge { public string name, prefab, side, releaseOnBlock, src; public int[] rows; public bool playerRelativeY; public int zoneMinX, zoneMaxX, zoneMinY, zoneMaxY, maxSpawns; }
    [Serializable] class Water { public string name, prefab, src; public int surfaceY, zoneMinX, zoneMaxX; }
    [Serializable] class Secret { public string name, condition, item, src; public int x, y, holdTicks; public int[] area, spawnAt; public bool rise; public float riseSpeed; }
    [Serializable] class Arena { public string name, prefab, src; public int zoneMinX, zoneMaxX; public bool useConfigPosition, screenWalls; public int[] bossPosition; }
    [Serializable] class Mech { public string name, platformPrefab, crusherPrefab, src; }
    [Serializable] class Section { public string id; public Candle[] candles; public Point[] spawnPoints; public Edge[] edgeSpawners; public Water[] waterSpawners; public Secret[] secrets; public Arena[] bossArenas; public Mech[] mechSpawners; public int objectWindowInsetPx; }
    [Serializable] class Table { public string note; public Section[] sections; }

    /// <summary>CVM2 10 — bảng + thư mục lớp vật thể Map 2 (Block 2) — menu CVM2/Build Map2 Objects (Cvm2LevelMenu).</summary>
    public const string TABLE_MAP2 = "Assets/Data/Config/Level/Block2/Map2Objects.json";
    public const string OBJ_DIR_MAP2 = "Assets/Data/Prefabs/Level/Block2/Objects";

    [MenuItem("CVGP/Build Map1 Objects")]
    static void BuildObjectsMenu()
    {
        Debug.Log(BuildObjects());
    }

    [MenuItem("CVGP/Build Map1 Screens")]
    static void BuildScreensMenu()
    {
        Debug.Log(BuildScreens());
    }

    // ------------------------------------------------------------------ lớp vật thể

    public static string BuildObjects()
    {
        return BuildObjectsFrom(TABLE, OBJ_DIR, "Map1");
    }

    /// <summary>CVM2 10: phần chung Map 1 / Map 2 — bảng JSON ⇒ &lt;objDir&gt;/&lt;Section&gt;_Objects.prefab (giống ⇒ không ghi).</summary>
    public static string BuildObjectsFrom(string tablePath, string objDir, string tag)
    {
        if (EditorApplication.isPlaying)
            return "[" + tag + "] đang Play — thoát Play rồi dựng";
        var table = JsonUtility.FromJson<Table>(File.ReadAllText(tablePath, Encoding.UTF8));
        var cat = AssetDatabase.LoadAssetAtPath<ItemCatalog>(CvgpItemBuilder.CATALOG);
        if (table == null || table.sections == null || cat == null)
            return "[" + tag + "] lỗi: thiếu bảng " + tablePath + " hoặc ItemCatalog";
        var errors = new List<string>();
        EnsureFolder(objDir);
        int written = 0, same = 0, n = 0;
        foreach (var s in table.sections)
        {
            string path = objDir + "/" + s.id + "_Objects.prefab";
            var root = new GameObject(s.id + "_Objects");
            try
            {
                n += Fill(root.transform, s, cat, errors);
                if (errors.Count > 0)
                    continue;
                var old = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (old != null && Signature(old) == Signature(root))
                {
                    same++;
                    continue;
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
                written++;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
        if (errors.Count > 0)
            return "[" + tag + "] lỗi bảng — không ghi:\n" + string.Join("\n", errors.ToArray());
        AssetDatabase.SaveAssets();
        return "[" + tag + "] Build " + tag + " Objects: " + n + " vật / " + table.sections.Length + " section — ghi " + written + ", không đổi " + same;
    }

    static int Fill(Transform root, Section s, ItemCatalog cat, List<string> errors)
    {
        int n = 0;
        if (s.candles != null)
            foreach (var c in s.candles)
            {
                var prefab = c.kind == "torch" ? cat.torchPrefab : cat.candlePrefab;
                var sp = MakePoint(root, "Candle_" + c.id, prefab, c.x, c.y, -1, false);
                sp.keepIfOffscreen = true;
                sp.dataKey = c.id;
                Window(sp, s.objectWindowInsetPx);
                n++;
            }
        if (s.spawnPoints != null)
            foreach (var p in s.spawnPoints)
            {
                var prefab = LoadEntity<Entity>(p.prefab, errors);
                var sp = MakePoint(root, p.name, prefab, p.x, p.y, p.facing, p.respawn);
                sp.param = p.param;   // CVM2 10: dơi treo param 1 (Bat.Perch)
                Window(sp, s.objectWindowInsetPx);
                n++;
            }
        if (s.edgeSpawners != null)
            foreach (var e in s.edgeSpawners)
            {
                var go = Child(root, e.name, 0, 0);
                var sp = go.AddComponent<EdgeSpawner>();
                sp.prefab = LoadEntity<EnemyBase>(e.prefab, errors);
                eSpawnSide side;
                if (!Enum.TryParse(e.side, out side))
                    errors.Add(s.id + "/" + e.name + ": side lạ " + e.side);
                sp.side = side;
                sp.rows = e.rows ?? new int[0];
                sp.playerRelativeY = e.playerRelativeY;
                sp.zoneMinX = e.zoneMinX;
                sp.zoneMaxX = e.zoneMaxX;
                sp.releaseOnBlock = e.releaseOnBlock ?? "";
                if (e.zoneMaxY > e.zoneMinY)   // CVM2 10: có khoá ⇒ vùng theo Y; không có (0, 0) ⇒ mặc định cả section
                {
                    sp.zoneMinY = e.zoneMinY;
                    sp.zoneMaxY = e.zoneMaxY;
                }
                sp.maxSpawns = e.maxSpawns;
                n++;
            }
        if (s.waterSpawners != null)
            foreach (var w in s.waterSpawners)
            {
                var go = Child(root, w.name, 0, w.surfaceY);
                var sp = go.AddComponent<WaterSpawner>();
                sp.prefab = LoadEntity<EnemyBase>(w.prefab, errors);
                sp.surfaceY = w.surfaceY;
                sp.zoneMinX = w.zoneMinX;
                sp.zoneMaxX = w.zoneMaxX;
                n++;
            }
        if (s.secrets != null)
            foreach (var q in s.secrets)
            {
                var sp = MakePoint(root, q.name, cat.secretPrefab, q.x, q.y, -1, false);
                sp.keepIfOffscreen = true;
                var spec = sp.gameObject.AddComponent<SecretTriggerSpec>();
                spec.area = new RectInt(q.area[0], q.area[1], q.area[2], q.area[3]);
                eSecretCondition cond;
                if (!Enum.TryParse(q.condition, out cond))
                    errors.Add(s.id + "/" + q.name + ": condition lạ " + q.condition);
                spec.condition = cond;
                spec.holdTicks = q.holdTicks;
                eItemId item;
                if (!Enum.TryParse(q.item, out item))
                    errors.Add(s.id + "/" + q.name + ": item lạ " + q.item);
                spec.item = item;
                spec.spawnAt = new Vector2Int(q.spawnAt[0], q.spawnAt[1]);
                spec.rise = q.rise;
                spec.riseSpeed = q.riseSpeed;
                n++;
            }
        if (s.bossArenas != null)
            foreach (var a in s.bossArenas)
            {
                var go = Child(root, a.name, a.bossPosition[0], a.bossPosition[1]);
                var ar = go.AddComponent<BossArena>();
                ar.prefab = LoadEntity<EnemyBase>(a.prefab, errors);
                ar.zoneMinX = a.zoneMinX;
                ar.zoneMaxX = a.zoneMaxX;
                ar.useConfigPosition = a.useConfigPosition;
                ar.bossPosition = new Vector2Int(a.bossPosition[0], a.bossPosition[1]);
                ar.screenWalls = a.screenWalls;
                n++;
            }
        if (s.mechSpawners != null)   // CVM2 10: bệ di động / bẫy ép (sinh từ marker section — Map2Mechanisms)
            foreach (var m in s.mechSpawners)
            {
                var go = Child(root, m.name, 0, 0);
                var ms = go.AddComponent<MechSpawner>();
                ms.platformPrefab = string.IsNullOrEmpty(m.platformPrefab) ? null : LoadEntity<MovingPlatform>(m.platformPrefab, errors);
                ms.crusherPrefab = string.IsNullOrEmpty(m.crusherPrefab) ? null : LoadEntity<Crusher>(m.crusherPrefab, errors);
                n++;
            }
        return n;
    }

    /// <summary>CVM2 10: inset &gt; 0 ⇒ điểm chỉ sinh/giữ khi x ∈ [cx + inset, cx + 256 − inset) (video Map_2: nến/Bone Tower chỉ hiện ở x màn 8–248).</summary>
    static void Window(SpawnPoint sp, int inset)
    {
        if (inset <= 0)
            return;
        sp.activateMargin = -inset;
        sp.despawnMargin = -inset;
    }

    static SpawnPoint MakePoint(Transform root, string name, Entity prefab, int x, int y, int facing, bool respawn)
    {
        var go = Child(root, name, x, y);
        var sp = go.AddComponent<SpawnPoint>();
        sp.prefab = prefab;
        sp.position = new Vector2Int(x, y);
        sp.facing = facing;
        sp.respawn = respawn;
        return sp;
    }

    static GameObject Child(Transform root, string name, int x, int y)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.localPosition = NesCoord.ToWorld(x, y);
        return go;
    }

    static T LoadEntity<T>(string path, List<string> errors) where T : Component
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var c = go != null ? go.GetComponent<T>() : null;
        if (c == null)
            errors.Add("thiếu prefab " + typeof(T).Name + ": " + path);
        return c;
    }

    /// <summary>Chuỗi so sánh nội dung (tên + vị trí + JSON từng component trừ Transform) — prefab giống ⇒ không ghi lại.</summary>
    static string Signature(GameObject root)
    {
        var sb = new StringBuilder();
        foreach (Transform t in root.transform)
        {
            sb.Append(t.name).Append('|').Append(t.localPosition.ToString("F4")).Append('\n');
            foreach (var c in t.GetComponents<Component>())
            {
                if (c is Transform)
                    continue;
                sb.Append(c.GetType().Name).Append(':').Append(EditorJsonUtility.ToJson(c)).Append('\n');
            }
        }
        return sb.ToString();
    }

    // ------------------------------------------------------------------ màn hình

    public static string BuildScreens()
    {
        if (EditorApplication.isPlaying)
            return "[Map1] đang Play — thoát Play rồi dựng";
        foreach (var id in SECTIONS)
            if (AssetDatabase.LoadAssetAtPath<GameObject>(OBJ_DIR + "/" + id + "_Objects.prefab") == null)
                return "[Map1] thiếu " + id + "_Objects.prefab — chạy CVGP/Build Map1 Objects trước";
        BuildStageScreen();
        BuildCastleMap();
        BuildStage4();
        var msg = InstallInMain();
        AssetDatabase.SaveAssets();
        return "[Map1] Build Map1 Screens: StageScreen, CastleMapScreen, Stage4Stub — " + msg;
    }

    static GameObject OpenOrNew(string path, string name)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            for (int i = root.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            return root;
        }
        return new GameObject(name);
    }

    static void Save(GameObject root, string path)
    {
        bool existed = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
        PrefabUtility.SaveAsPrefabAsset(root, path);
        if (existed)
            PrefabUtility.UnloadPrefabContents(root);
        else
            UnityEngine.Object.DestroyImmediate(root);
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    static GameObject AddHud(Transform parent)
    {
        var hud = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HUD_PREFAB), parent);
        hud.name = "HUD";
        hud.transform.localPosition = Vector3.zero;
        return hud;
    }

    static void BuildStageScreen()
    {
        var root = OpenOrNew(STAGE_SCREEN, "StageScreen");
        root.transform.localPosition = Vector3.zero;
        var screen = GetOrAdd<StageScreen>(root);
        var hud = AddHud(root.transform);

        var worldGo = new GameObject("StageWorld");
        worldGo.transform.SetParent(root.transform, false);
        var world = worldGo.AddComponent<StageWorld>();
        var cam = worldGo.AddComponent<NesCameraController>();
        var rules = worldGo.AddComponent<StageRules>();
        var flow = worldGo.AddComponent<StageFlow>();
        var overlay = worldGo.AddComponent<CvgpDebugOverlay>();

        var secs = new Dictionary<string, LevelSection>();
        foreach (var id in SECTIONS)
        {
            var s = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SECTION_DIR + id + ".prefab"), worldGo.transform);
            s.name = id;
            var objs = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(OBJ_DIR + "/" + id + "_Objects.prefab"), s.transform);
            objs.name = id + "_Objects";
            s.SetActive(id == "CastleEntrance");
            secs[id] = s.GetComponent<LevelSection>();
        }
        var entities = new GameObject("Entities");
        entities.transform.SetParent(worldGo.transform, false);
        entities.transform.localPosition = NesCoord.ToWorld(0, LevelSectionData.SCREEN_Y);
        var simon = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CvgpArenaBuilder.SIMON_PREFAB), entities.transform);
        simon.name = "Simon";
        var doorGo = new GameObject("Door");
        doorGo.transform.SetParent(entities.transform, false);
        var door = doorGo.AddComponent<SpriteRenderer>();
        door.sortingLayerName = "Foreground";
        door.sortingOrder = 1;
        door.enabled = false;

        var wso = new SerializedObject(world);
        wso.FindProperty("fallbackConfig").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameConfig>(GAME_CONFIG);
        wso.FindProperty("cameraController").objectReferenceValue = cam;
        wso.FindProperty("startSection").objectReferenceValue = secs["CastleEntrance"];
        wso.FindProperty("entityRoot").objectReferenceValue = entities.transform;
        wso.FindProperty("stage").intValue = 1;
        wso.FindProperty("itemCatalog").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ItemCatalog>(CvgpItemBuilder.CATALOG);
        wso.ApplyModifiedPropertiesWithoutUndo();

        var cso = new SerializedObject(cam);
        var fl = cso.FindProperty("followers");
        fl.arraySize = 1;
        fl.GetArrayElementAtIndex(0).objectReferenceValue = hud.transform;
        cso.ApplyModifiedPropertiesWithoutUndo();

        // đầu mỗi stage (hồi sinh / Continue / vào màn): Stage 01 = simonStart Castle Entrance (CVTI khung-khớp-khung (50,160));
        // Stage 02/03 = điểm vào sau cửa đo khớp mẫu (48,80) — LevelSectionData 06 ghi 49 (x_med lệch +1 khi quay phải — §6)
        var rso = new SerializedObject(rules);
        rso.FindProperty("world").objectReferenceValue = world;
        var st = rso.FindProperty("stageStarts");
        st.arraySize = 3;
        SetStart(st.GetArrayElementAtIndex(0), 1, secs["CastleEntrance"], true, Vector2Int.zero);
        SetStart(st.GetArrayElementAtIndex(1), 2, secs["CryptA"], false, new Vector2Int(Map1FlowTiming.STAGE_ENTRY_X, Map1FlowTiming.STAGE_ENTRY_Y));
        SetStart(st.GetArrayElementAtIndex(2), 3, secs["PhantomBat"], false, new Vector2Int(Map1FlowTiming.STAGE_ENTRY_X, Map1FlowTiming.STAGE_ENTRY_Y));
        rso.ApplyModifiedPropertiesWithoutUndo();

        var fso = new SerializedObject(flow);
        fso.FindProperty("world").objectReferenceValue = world;
        fso.FindProperty("rules").objectReferenceValue = rules;
        fso.FindProperty("doorView").objectReferenceValue = door;
        fso.FindProperty("doorLeaf").objectReferenceValue = LoadSprite("Assets/Data/Art/Object/door_0.png");
        fso.FindProperty("doorOpen").objectReferenceValue = LoadSprite("Assets/Data/Art/Object/door_1.png");
        fso.ApplyModifiedPropertiesWithoutUndo();

        var oso = new SerializedObject(overlay);
        oso.FindProperty("world").objectReferenceValue = world;
        oso.ApplyModifiedPropertiesWithoutUndo();

        var sso = new SerializedObject(screen);
        sso.FindProperty("world").objectReferenceValue = world;
        sso.FindProperty("rules").objectReferenceValue = rules;
        sso.FindProperty("flow").objectReferenceValue = flow;
        sso.ApplyModifiedPropertiesWithoutUndo();
        Save(root, STAGE_SCREEN);
    }

    static void SetStart(SerializedProperty p, int stage, LevelSection sec, bool useSectionStart, Vector2Int pos)
    {
        p.FindPropertyRelative("stage").intValue = stage;
        p.FindPropertyRelative("section").objectReferenceValue = sec;
        p.FindPropertyRelative("useSectionStart").boolValue = useSectionStart;
        p.FindPropertyRelative("position").vector2IntValue = pos;
    }

    static void BuildCastleMap()
    {
        var root = OpenOrNew(CASTLEMAP_SCREEN, "CastleMapScreen");
        root.transform.localPosition = Vector3.zero;
        var screen = GetOrAdd<CastleMapScreen>(root);
        AddHud(root.transform);
        const string A = "Assets/Data/Art/CastleMap/";
        var map = MakeRenderer(root.transform, "Map", LoadSprite(A + "castlemap_bg.png"), "Background", 0, NesCoord.SCREEN_W, Map1FlowTiming.MAP_Y);
        var bat = MakeRenderer(root.transform, "Bat", LoadSprite(A + "castlemap_bat_0.png"), "BackgroundFX", 0, Map1FlowTiming.MAP_BAT_X, Map1FlowTiming.MAP_BAT_Y);
        var marker = MakeRenderer(root.transform, "Marker", LoadSprite(A + "castlemap_marker.png"), "BackgroundFX", 1, Map1FlowTiming.MAP_MARKER_X, Map1FlowTiming.MAP_MARKER_Y);
        const string S = "Assets/Data/Art/Character/Simon/";
        var simon = MakeRenderer(root.transform, "Simon", LoadSprite(S + "walk_0.png"), "Character", 0, Map1FlowTiming.MAP_SIMON_START_X, Map1FlowTiming.MAP_SIMON_Y + 1);
        bat.enabled = marker.enabled = simon.enabled = false;
        var so = new SerializedObject(screen);
        so.FindProperty("map").objectReferenceValue = map;
        so.FindProperty("bat").objectReferenceValue = bat;
        so.FindProperty("marker").objectReferenceValue = marker;
        so.FindProperty("simon").objectReferenceValue = simon;
        SetArray(so.FindProperty("batFrames"), LoadSprite(A + "castlemap_bat_0.png"), LoadSprite(A + "castlemap_bat_1.png"));
        SetArray(so.FindProperty("simonWalk"), LoadSprite(S + "walk_0.png"), LoadSprite(S + "walk_1.png"), LoadSprite(S + "walk_2.png"));
        so.ApplyModifiedPropertiesWithoutUndo();
        Save(root, CASTLEMAP_SCREEN);
    }

    static void BuildStage4()
    {
        var root = OpenOrNew(STAGE4_SCREEN, "Stage4Stub");
        root.transform.localPosition = Vector3.zero;
        GetOrAdd<Stage4Stub>(root);
        AddHud(root.transform);
        Save(root, STAGE4_SCREEN);
    }

    static SpriteRenderer MakeRenderer(Transform parent, string name, Sprite sprite, string layer, int order, int x, int y)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = NesCoord.ToWorld(x, y);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = layer;
        sr.sortingOrder = order;
        return sr;
    }

    static void SetArray(SerializedProperty p, params UnityEngine.Object[] values)
    {
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    static Sprite LoadSprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null)
            Debug.LogError("[Map1] thiếu sprite " + path);
        return s;
    }

    /// <summary>Main.unity/ScreenRoot: bỏ instance Stage1Stub, thêm StageScreen (cùng chỗ), CastleMapScreen, Stage4Stub — tắt sẵn.</summary>
    static string InstallInMain()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != MAIN_SCENE)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return "huỷ (scene đang mở chưa lưu)";
            scene = EditorSceneManager.OpenScene(MAIN_SCENE, OpenSceneMode.Single);
        }
        GameObject screenRoot = null;
        foreach (var r in scene.GetRootGameObjects())
            if (r.name == "ScreenRoot")
                screenRoot = r;
        if (screenRoot == null)
            return "lỗi: Main.unity không có ScreenRoot";
        int index = -1;
        var changes = new List<string>();
        for (int i = screenRoot.transform.childCount - 1; i >= 0; i--)
        {
            var c = screenRoot.transform.GetChild(i).gameObject;
            if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(c) == STAGE1_STUB)
            {
                index = c.transform.GetSiblingIndex();
                UnityEngine.Object.DestroyImmediate(c);
                changes.Add("bỏ Stage1Stub");
            }
        }
        Ensure(screenRoot.transform, STAGE_SCREEN, index, changes);
        Ensure(screenRoot.transform, CASTLEMAP_SCREEN, -1, changes);
        Ensure(screenRoot.transform, STAGE4_SCREEN, -1, changes);
        if (changes.Count == 0)
            return "Main.unity không đổi";
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Main.unity: " + string.Join(", ", changes.ToArray());
    }

    static void Ensure(Transform parent, string prefabPath, int index, List<string> changes)
    {
        foreach (Transform c in parent)
            if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(c.gameObject) == prefabPath)
                return;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), parent);
        go.transform.localPosition = Vector3.zero;
        if (index >= 0)
            go.transform.SetSiblingIndex(index);
        go.SetActive(false);
        changes.Add("thêm " + Path.GetFileNameWithoutExtension(prefabPath));
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
