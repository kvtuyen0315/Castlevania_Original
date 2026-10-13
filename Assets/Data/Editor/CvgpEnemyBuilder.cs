using UnityEditor;
using UnityEngine;

/// <summary>
/// CVGP 13 — dựng prefab enemy Block 1 (Assets/Data/Prefabs/Enemy/{Ghoul, Panther, Bat, Fishman, FishmanFireball}.prefab).
/// Menu CVGP/Build Enemy Prefabs (chạy lại được: SaveAsPrefabAsset giữ GUID). Thông số dựng (đo/bbox — CVGP_MEMORY §2k/§3):
///   • viewOffset: đi đất (0,1) — video: pivot sprite ở hàng mặt sàn + 1 (Zombie/báo/Fish Man pivy 209/145/129 khi sàn 208/144/128 NES);
///     bay/đạn (0,0) (pivot tâm). Body = vùng CHÂN 2×2 (tường chỉ chặn ở tầm chân; sàn dò đúng cột pivot — EnemyBase.Grounded).
///   • hurt/touch = bbox pixel đục sprite (XẤP XỈ); centerDy = tâm thân (video lửa chết: Zombie/Fish Man chân −15, báo −7).
///   • layer Character order −2 (sau Simon 0 / roi ±1 — QUY ƯỚC, video không có cảnh chồng rõ); cầu lửa order 1.
/// </summary>
public static class CvgpEnemyBuilder
{
    public const string DIR = "Assets/Data/Prefabs/Enemy";
    const string ANIM_DIR = "Assets/Data/Config/Anim/";
    const int ORDER = -2;
    static readonly RectInt FEET = new RectInt(-1, -2, 2, 2);

    [MenuItem("CVGP/Build Enemy Prefabs")]
    static void Menu()
    {
        Debug.Log("[CVGP] " + Build());
    }

    public static string Build()
    {
        EnsureFolder(DIR);
        var fishAnim = Anim("FishManAnimSet");
        var fireball = Make<FishmanFireball>("FishmanFireball", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, fishAnim, 1,
            false, true, -4, null);
        Make<Ghoul>("Ghoul", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), Anim("ZombieAnimSet"), ORDER, true, false, 32,
            so => Enemy(so, eEnemyId.Ghoul, new RectInt(-8, -31, 16, 32), -15));
        Make<Panther>("Panther", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), Anim("BlackLeopardAnimSet"), ORDER, true, false, 32,
            so => Enemy(so, eEnemyId.Panther, Panther.BOX_SIT, -7));
        Make<Bat>("Bat", eEntityGroup.Enemy, new RectInt(-1, -1, 2, 2), Vector2Int.zero, Anim("VampireBatAnimSet"), ORDER, false, false, 32,
            so => Enemy(so, eEnemyId.Bat, new RectInt(-8, -7, 16, 14), 0));
        Make<Fishman>("Fishman", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), fishAnim, ORDER, false, false, 32,
            so =>
            {
                Enemy(so, eEnemyId.Fishman, new RectInt(-8, -31, 16, 32), -15);
                so.FindProperty("fireballPrefab").objectReferenceValue = fireball;
            });
        AssetDatabase.SaveAssets();
        return "Enemy prefabs: Ghoul, Panther, Bat, Fishman, FishmanFireball → " + DIR;
    }

    public static T Load<T>(string name) where T : Entity
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

    static void Enemy(SerializedObject so, eEnemyId id, RectInt box, int centerDy)
    {
        so.FindProperty("enemyId").enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(eEnemyId)), id);
        so.FindProperty("hurtBox").rectIntValue = box;
        so.FindProperty("touchBox").rectIntValue = box;
        so.FindProperty("centerDy").intValue = centerDy;
    }

    static T Make<T>(string name, eEntityGroup group, RectInt body, Vector2Int viewOffset, AnimSet anim, int order, bool deadly,
                     bool despawnOffscreen, int margin, System.Action<SerializedObject> extra) where T : Entity
    {
        var go = new GameObject(name);
        try
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Character";
            sr.sortingOrder = order;
            var c = go.AddComponent<T>();
            var so = new SerializedObject(c);
            so.FindProperty("group").enumValueIndex = (int)group;
            so.FindProperty("body").rectIntValue = body;
            so.FindProperty("view").objectReferenceValue = sr;
            so.FindProperty("viewOffset").vector2IntValue = viewOffset;
            so.FindProperty("animSet").objectReferenceValue = anim;
            so.FindProperty("checkDeadlyTiles").boolValue = deadly;
            so.FindProperty("despawnOffscreen").boolValue = despawnOffscreen;
            so.FindProperty("offscreenMargin").intValue = margin;
            if (extra != null)
                extra(so);
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
