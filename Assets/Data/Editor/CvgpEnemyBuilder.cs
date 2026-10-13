using UnityEditor;
using UnityEngine;

/// <summary>
/// CVGP 13 — dựng prefab enemy Block 1 (Assets/Data/Prefabs/Enemy/{Ghoul, Panther, Bat, Fishman, FishmanFireball}.prefab);
/// CVGP 17 thêm Block 2–3 (BuildBlock23); CVGP 18 thêm Block 4–6 (BuildBlock46).
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
        BuildBlock23();
        BuildBlock46();
        AssetDatabase.SaveAssets();
        return "Enemy prefabs: Ghoul, Panther, Bat, Fishman, FishmanFireball + (CVGP 17) SpearKnight, MedusaHead, BoneTower, BoneTowerFireball, "
               + "Ghost, Raven, Skeleton, SkeletonBone, Fleaman + (CVGP 18) Eagle, BoneDragon, BoneDragonFireball, RedSkeleton, AxeKnight, "
               + "AxeKnightAxe, PhantomBatMinor → " + DIR;
    }

    /// <summary>
    /// CVGP 17 — enemy Block 2–3 (theo wiki, gần đúng). Hộp nhận/gây đòn = bbox pixel đục sprite (Tools: _cache/17 — XẤP XỈ):
    /// blackknight 16×31/32, skeleton 16×32 (đi đất, viewOffset (0,1) như Zombie ⇒ hộp (−8,−31,16,32), tâm −15); 2 đầu lâu Bone Tower
    /// 16×32 (−8,−31,16,32) tâm −15; fleaman 16×16 (−8,−15,16,16) tâm −7; bay (pivot tâm, viewOffset 0): Medusa Head/Ghost/Raven
    /// (−8,−8,16,16) — Raven đậu cũng dùng hộp này (raven_0 bbox y 3–14). Đạn order 1 như cầu lửa Fish Man.
    /// </summary>
    static void BuildBlock23()
    {
        var skull = Anim("DragonSkullCannonAnimSet");
        var skel = Anim("WhiteSkeletonAnimSet");
        var towerBall = Make<BoneTowerFireball>("BoneTowerFireball", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, skull, 1,
            false, true, 16, null);
        var bone = Make<SkeletonBone>("SkeletonBone", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, skel, 1,
            false, true, 16, null);
        Make<SpearKnight>("SpearKnight", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), Anim("BlackKnightAnimSet"), ORDER, true, false, 32,
            so => Enemy(so, eEnemyId.SpearKnight, new RectInt(-8, -31, 16, 32), -15));
        Make<MedusaHead>("MedusaHead", eEntityGroup.Enemy, new RectInt(-1, -1, 2, 2), Vector2Int.zero, Anim("MedusaHeadAnimSet"), ORDER, false, false, 32,
            so => Enemy(so, eEnemyId.MedusaHead, new RectInt(-8, -8, 16, 16), 0));
        Make<BoneTower>("BoneTower", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), skull, ORDER, false, false, 32,
            so =>
            {
                Enemy(so, eEnemyId.BoneTower, new RectInt(-8, -31, 16, 32), -15);
                so.FindProperty("fireballPrefab").objectReferenceValue = towerBall;
            });
        Make<Ghost>("Ghost", eEntityGroup.Enemy, new RectInt(-1, -1, 2, 2), Vector2Int.zero, Anim("GhostAnimSet"), ORDER, false, false, 32,
            so => Enemy(so, eEnemyId.Ghost, new RectInt(-8, -8, 16, 16), 0));
        Make<Raven>("Raven", eEntityGroup.Enemy, new RectInt(-1, -1, 2, 2), Vector2Int.zero, Anim("RavenAnimSet"), ORDER, false, false, 32,
            so => Enemy(so, eEnemyId.Raven, new RectInt(-8, -8, 16, 16), 0));
        Make<Skeleton>("Skeleton", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), skel, ORDER, true, false, 32,
            so =>
            {
                Enemy(so, eEnemyId.Skeleton, new RectInt(-8, -31, 16, 32), -15);
                so.FindProperty("bonePrefab").objectReferenceValue = bone;
            });
        Make<Fleaman>("Fleaman", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), Anim("FleamanAnimSet"), ORDER, true, false, 32,
            so => Enemy(so, eEnemyId.Fleaman, new RectInt(-8, -15, 16, 16), -7));
    }

    /// <summary>Số đốt cổ Bone Dragon dựng sẵn (Config Enemy.BoneDragon.segments ≤ số này).</summary>
    public const int DRAGON_NECK = 6;

    /// <summary>
    /// CVGP 18 — enemy Block 4–6 (theo wiki, gần đúng). Hộp = bbox pixel đục sprite (XẤP XỈ): eagle_0/1 hợp 32×32 tâm (−16,−16,32,32);
    /// redskeleton đi 16×32 (−8,−31,16,32) tâm −15; axeman 24×32 cột đục 2–23, pivot 12 ⇒ (−10,−31,22,32) tâm −15; Phantom Bat thường =
    /// hộp boss (CvgpBossBuilder.PHANTOM_BAT_BOX). Bone Dragon: pivot = chân cổ ở tường (không sprite); renderer con "Head" (view,
    /// order −2) + DRAGON_NECK con "Neck_k" (order −3, sprite đốt gán lúc chạy); hộp đầu/đốt BoneDragon.Shape() đặt mỗi tick.
    /// Đạn order 1: cầu lửa Bone Dragon (lớp BoneTowerFireball, AnimSet SkeleDragon "fireball") — Phantom Bat thường dùng chung (TẠM).
    /// </summary>
    static void BuildBlock46()
    {
        var dragonAnim = Anim("SkeleDragonAnimSet");
        var axeAnim = Anim("AxeManAnimSet");
        var dragonBall = Make<BoneTowerFireball>("BoneDragonFireball", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, dragonAnim, 1,
            false, true, 16, null);
        var axe = Make<AxeKnightAxe>("AxeKnightAxe", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, axeAnim, 1,
            false, true, 16, null);
        var flea = Load<Fleaman>("Fleaman");
        Make<Eagle>("Eagle", eEntityGroup.Enemy, new RectInt(-1, -1, 2, 2), Vector2Int.zero, Anim("EagleAnimSet"), ORDER, false, false, 32,
            so =>
            {
                Enemy(so, eEnemyId.Eagle, new RectInt(-16, -16, 32, 32), 0);
                so.FindProperty("fleamanPrefab").objectReferenceValue = flea;
            });
        Make<BoneDragon>("BoneDragon", eEntityGroup.Enemy, new RectInt(-1, -1, 2, 2), Vector2Int.zero, dragonAnim, ORDER, false, false, 32,
            so =>
            {
                Enemy(so, eEnemyId.BoneDragon, new RectInt(-8, -8, 16, 16), 0);
                so.FindProperty("fireballPrefab").objectReferenceValue = dragonBall;
                var root = ((Component)so.targetObject).gameObject;
                Object.DestroyImmediate(root.GetComponent<SpriteRenderer>());
                var headGo = new GameObject("Head");
                headGo.transform.SetParent(root.transform, false);
                var head = headGo.AddComponent<SpriteRenderer>();
                head.sortingLayerName = "Character";
                head.sortingOrder = ORDER;
                so.FindProperty("view").objectReferenceValue = head;
                var necks = so.FindProperty("neckViews");
                necks.arraySize = DRAGON_NECK;
                for (int i = 0; i < DRAGON_NECK; i++)
                {
                    var n = new GameObject("Neck_" + i);
                    n.transform.SetParent(root.transform, false);
                    var r = n.AddComponent<SpriteRenderer>();
                    r.sortingLayerName = "Character";
                    r.sortingOrder = ORDER - 1;
                    necks.GetArrayElementAtIndex(i).objectReferenceValue = r;
                }
            });
        Make<RedSkeleton>("RedSkeleton", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), Anim("RedSkeletonAnimSet"), ORDER, true, false, 32,
            so => Enemy(so, eEnemyId.RedSkeleton, new RectInt(-8, -31, 16, 32), -15));
        Make<AxeKnight>("AxeKnight", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), axeAnim, ORDER, true, false, 32,
            so =>
            {
                Enemy(so, eEnemyId.AxeKnight, new RectInt(-10, -31, 22, 32), -15);
                so.FindProperty("axePrefab").objectReferenceValue = axe;
            });
        Make<PhantomBatMinor>("PhantomBatMinor", eEntityGroup.Enemy, new RectInt(-1, -1, 2, 2), Vector2Int.zero, Anim("PhantomBatAnimSet"), ORDER,
            false, false, 32,
            so =>
            {
                Enemy(so, eEnemyId.PhantomBatMinor, CvgpBossBuilder.PHANTOM_BAT_BOX, 0);
                so.FindProperty("bossId").enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(eBossId)), eBossId.PhantomBat);
                so.FindProperty("phase").intValue = 1;
                so.FindProperty("fireballPrefab").objectReferenceValue = dragonBall;
            });
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
