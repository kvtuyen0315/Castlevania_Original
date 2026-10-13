using UnityEditor;
using UnityEngine;

/// <summary>
/// CVGP 14 — dựng prefab boss (Assets/Data/Prefabs/Boss/PhantomBat.prefab). Menu CVGP/Build Boss Prefabs (chạy lại được:
/// SaveAsPrefabAsset giữ GUID). Thông số dựng (CVGP_MEMORY §2l / §3 "Boss"):
///   • pivot = tâm khung bay (khung 48×32 / 32×32 tâm (24,16) / (16,16) — video khớp mẫu đúng tâm) ⇒ viewOffset (0,0);
///     body = 2×2 (boss bay không va địa hình).
///   • hurt/touch = bbox pixel đục khung cánh nửa phantombat_2 (−16..15, −16..6) — XẤP XỈ (như enemy 13); video boss f360: mép phải
///     hộp này chạm thân Simon đúng frame Simon trúng đòn.
///   • layer Character order −2 như enemy (QUY ƯỚC).
/// Routine 19/20: thêm 1 dòng Make… cho boss mới (lớp con BossBase).
/// </summary>
public static class CvgpBossBuilder
{
    public const string DIR = "Assets/Data/Prefabs/Boss";
    const string ANIM_DIR = "Assets/Data/Config/Anim/";
    const int ORDER = -2;
    public static readonly RectInt PHANTOM_BAT_BOX = new RectInt(-16, -16, 32, 23);

    [MenuItem("CVGP/Build Boss Prefabs")]
    static void Menu()
    {
        Debug.Log("[CVGP] " + Build());
    }

    public static string Build()
    {
        EnsureFolder(DIR);
        Make<PhantomBat>("PhantomBat", eBossId.PhantomBat, new RectInt(-1, -1, 2, 2), Vector2Int.zero, Anim("PhantomBatAnimSet"), PHANTOM_BAT_BOX, 0);
        AssetDatabase.SaveAssets();
        return "Boss prefabs: PhantomBat → " + DIR;
    }

    public static T Load<T>(string name) where T : BossBase
    {
        return AssetDatabase.LoadAssetAtPath<T>(DIR + "/" + name + ".prefab");
    }

    static AnimSet Anim(string name)
    {
        var a = AssetDatabase.LoadAssetAtPath<AnimSet>(ANIM_DIR + name + ".asset");
        if (a == null)
            Debug.LogError("[CVGP] thiếu " + ANIM_DIR + name + ".asset — chạy CVGP/Build AnimSets");
        return a;
    }

    static T Make<T>(string name, eBossId id, RectInt body, Vector2Int viewOffset, AnimSet anim, RectInt box, int centerDy) where T : BossBase
    {
        var go = new GameObject(name);
        try
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Character";
            sr.sortingOrder = ORDER;
            var c = go.AddComponent<T>();
            var so = new SerializedObject(c);
            so.FindProperty("group").enumValueIndex = (int)eEntityGroup.Enemy;
            so.FindProperty("body").rectIntValue = body;
            so.FindProperty("view").objectReferenceValue = sr;
            so.FindProperty("viewOffset").vector2IntValue = viewOffset;
            so.FindProperty("animSet").objectReferenceValue = anim;
            so.FindProperty("checkDeadlyTiles").boolValue = false;
            so.FindProperty("despawnOffscreen").boolValue = false;
            so.FindProperty("enemyId").enumValueIndex = 0;
            so.FindProperty("bossId").enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(eBossId)), id);
            so.FindProperty("phase").intValue = 1;
            so.FindProperty("hurtBox").rectIntValue = box;
            so.FindProperty("touchBox").rectIntValue = box;
            so.FindProperty("centerDy").intValue = centerDy;
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
