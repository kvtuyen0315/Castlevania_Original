using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CVGP 11 — dựng prefab hệ item / vật chứa (Assets/Data/Prefabs/Item/*.prefab), 2 vật liệu shader (Assets/Data/Art/Material/)
/// và ItemCatalog (Assets/Data/Config/ItemCatalog.asset). Menu CVGP/Build Item Prefabs (chạy lại được: SaveAsPrefabAsset giữ GUID,
/// catalog/vật liệu chỉ ghi khi khác). Gán catalog cho StageWorld của scene đang mở: CVGP/Assign ItemCatalog To Open Scene.
/// Thứ tự vẽ (layer Character): Simon 0, roi/vũ khí ±1, item 2, FX 3, chữ điểm 4; nến/chậu ở BackgroundFX (sau nhân vật).
/// </summary>
public static class CvgpItemBuilder
{
    public const string DIR = "Assets/Data/Prefabs/Item";
    public const string CATALOG = "Assets/Data/Config/ItemCatalog.asset";
    const string MAT_DIR = "Assets/Data/Art/Material";
    const string ANIM_DIR = "Assets/Data/Config/Anim/";

    [MenuItem("CVGP/Build Item Prefabs")]
    static void Menu()
    {
        Debug.Log("[CVGP] " + Build());
    }

    [MenuItem("CVGP/Assign ItemCatalog To Open Scene")]
    static void MenuAssign()
    {
        Debug.Log("[CVGP] " + AssignToOpenScene());
    }

    public static string Build()
    {
        EnsureFolder(DIR);
        EnsureFolder(MAT_DIR);
        var palMat = Mat("NesPaletteSwap", "CVGP/NesPaletteSwap");
        var flashMat = Mat("NesScreenFlash", "Hidden/CVGP/NesScreenFlash");
        var itemAnim = AssetDatabase.LoadAssetAtPath<AnimSet>(ANIM_DIR + "ItemAnimSet.asset");
        var objAnim = AssetDatabase.LoadAssetAtPath<AnimSet>(ANIM_DIR + "ObjectAnimSet.asset");
        var fxAnim = AssetDatabase.LoadAssetAtPath<AnimSet>(ANIM_DIR + "FXAnimSet.asset");

        var item = Make<ItemPickup>("ItemPickup", eEntityGroup.Item, ItemPickup.BOX16, new Vector2Int(0, 1), itemAnim, "Character", 2, true, null);
        var fx = Make<FxSprite>("Fx", eEntityGroup.Effect, new RectInt(-1, -1, 2, 2), Vector2Int.zero, fxAnim, "Character", 3, false, null);
        var score = Make<ScorePopup>("ScorePopup", eEntityGroup.Effect, new RectInt(0, 0, 16, 8), Vector2Int.zero, objAnim, "Character", 4, false,
            (go, so) =>
            {
                var c = new GameObject("Second");
                c.transform.SetParent(go.transform, false);
                var sr = c.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Character";
                sr.sortingOrder = 4;
                so.FindProperty("second").objectReferenceValue = sr;
            });
        var candle = Make<CandleProp>("Candle", eEntityGroup.Prop, new RectInt(-4, -16, 8, 16), new Vector2Int(0, 1), objAnim, "BackgroundFX", 0, false,
            (go, so) =>
            {
                so.FindProperty("animName").stringValue = "candle";
                so.FindProperty("height").intValue = 16;
                so.FindProperty("content").enumValueIndex = EnumIndex(eItemId.SmallHeart);
            });
        var torch = Make<CandleProp>("Torch", eEntityGroup.Prop, new RectInt(-8, -32, 16, 32), new Vector2Int(0, 1), objAnim, "BackgroundFX", 0, false,
            (go, so) =>
            {
                so.FindProperty("animName").stringValue = "torch";
                so.FindProperty("height").intValue = 32;
                so.FindProperty("content").enumValueIndex = EnumIndex(eItemId.None);
            });
        var block = Make<BreakableBlock>("BreakableBlock", eEntityGroup.Prop, new RectInt(-8, -16, 16, 16), Vector2Int.zero, null, null, 0, false, null);
        var secret = Make<SecretTrigger>("SecretTrigger", eEntityGroup.Prop, new RectInt(-1, -1, 2, 2), Vector2Int.zero, null, null, 0, false, null);

        var cat = AssetDatabase.LoadAssetAtPath<ItemCatalog>(CATALOG);
        bool created = cat == null;
        if (created)
        {
            cat = ScriptableObject.CreateInstance<ItemCatalog>();
            AssetDatabase.CreateAsset(cat, CATALOG);
        }
        var cso = new SerializedObject(cat);
        cso.FindProperty("itemPrefab").objectReferenceValue = item;
        cso.FindProperty("fxPrefab").objectReferenceValue = fx;
        cso.FindProperty("scorePrefab").objectReferenceValue = score;
        cso.FindProperty("candlePrefab").objectReferenceValue = candle;
        cso.FindProperty("torchPrefab").objectReferenceValue = torch;
        cso.FindProperty("blockPrefab").objectReferenceValue = block;
        cso.FindProperty("secretPrefab").objectReferenceValue = secret;
        cso.FindProperty("paletteMaterial").objectReferenceValue = palMat;
        cso.FindProperty("flashMaterial").objectReferenceValue = flashMat;
        cso.FindProperty("simonPalettes").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AnimSet>(ANIM_DIR + "SimonAnimSet.asset");
        bool changed = cso.ApplyModifiedPropertiesWithoutUndo();
        if (changed)
            EditorUtility.SetDirty(cat);
        AssetDatabase.SaveAssets();
        return "Item prefabs: ItemPickup, Fx, ScorePopup, Candle, Torch, BreakableBlock, SecretTrigger; catalog " + (created ? "tạo mới" : changed ? "cập nhật" : "không đổi");
    }

    /// <summary>Gán ItemCatalog cho mọi StageWorld trong scene đang mở + lưu scene (chỉ khi khác).</summary>
    public static string AssignToOpenScene()
    {
        var cat = AssetDatabase.LoadAssetAtPath<ItemCatalog>(CATALOG);
        if (cat == null)
            return "chưa có " + CATALOG + " — chạy CVGP/Build Item Prefabs";
        int n = 0;
        foreach (var w in Object.FindObjectsOfType<StageWorld>(true))
        {
            var so = new SerializedObject(w);
            var p = so.FindProperty("itemCatalog");
            if (p.objectReferenceValue == cat)
                continue;
            p.objectReferenceValue = cat;
            so.ApplyModifiedPropertiesWithoutUndo();
            n++;
        }
        var scene = EditorSceneManager.GetActiveScene();
        if (n > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        return "gán ItemCatalog cho " + n + " StageWorld trong " + scene.path;
    }

    static int EnumIndex(eItemId id)
    {
        return System.Array.IndexOf(System.Enum.GetValues(typeof(eItemId)), id);
    }

    static Material Mat(string name, string shaderName)
    {
        string path = MAT_DIR + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        var sh = Shader.Find(shaderName);
        if (sh == null)
        {
            Debug.LogError("[CVGP] không tìm thấy shader " + shaderName);
            return m;
        }
        if (m == null)
        {
            m = new Material(sh) { name = name };
            AssetDatabase.CreateAsset(m, path);
        }
        else if (m.shader != sh)
        {
            m.shader = sh;
            EditorUtility.SetDirty(m);
        }
        return m;
    }

    static T Make<T>(string name, eEntityGroup group, RectInt body, Vector2Int viewOffset, AnimSet anim, string layer, int order,
                     bool despawnOffscreen, System.Action<GameObject, SerializedObject> extra) where T : Entity
    {
        var go = new GameObject(name);
        try
        {
            SpriteRenderer sr = null;
            if (layer != null)
            {
                sr = go.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = layer;
                sr.sortingOrder = order;
            }
            var c = go.AddComponent<T>();
            var so = new SerializedObject(c);
            so.FindProperty("group").enumValueIndex = (int)group;
            so.FindProperty("body").rectIntValue = body;
            so.FindProperty("view").objectReferenceValue = sr;
            so.FindProperty("viewOffset").vector2IntValue = viewOffset;
            so.FindProperty("animSet").objectReferenceValue = anim;
            so.FindProperty("checkDeadlyTiles").boolValue = false;
            so.FindProperty("despawnOffscreen").boolValue = despawnOffscreen;
            so.FindProperty("offscreenMargin").intValue = despawnOffscreen ? -8 : 32;
            if (extra != null)
                extra(go, so);
            so.ApplyModifiedPropertiesWithoutUndo();
            string path = DIR + "/" + name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }
}
