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
/// Routine 19/20: thêm 1 dòng Make… cho boss mới (lớp con BossBase). CVGP 19: BuildBlock24 (Medusa, Mummy Men, Frankenstein & Igor).
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
        BuildBlock24();
        BuildBlock56();
        AssetDatabase.SaveAssets();
        return "Boss prefabs: PhantomBat, QueenMedusa + Snake, MummyMan + MummyBandage, Frankenstein + Igor + IgorFireball, GrimReaper + DeathScythe, "
               + "DraculaPhase1 + DraculaPhase2 + DraculaFireball + DemonFireball → " + DIR;
    }

    static readonly RectInt FEET = new RectInt(-1, -2, 2, 2);

    /// <summary>
    /// CVGP 19 — boss Block 2–4 (theo wiki, GẦN ĐÚNG). Hộp = bbox pixel đục sprite (XẤP XỈ, như 13/14), đi đất viewOffset (0,1) + body FEET
    /// (checklist enemy 13), bay viewOffset (0,0):
    ///   • QueenMedusa: pivot tâm đầu medusa_head 32×32 ⇒ (−16,−16,32,32); con "Statue" (SpriteRenderer order −3, sprite bust gán lúc chạy).
    ///   • Snake: snake_0/1 16×16, pivot đáy-giữa. CVM2 09 (video): vẽ CAO hơn 1 px (viewOffset (0,−1) — Boss_Map_2 4 rắn trên sàn: hàng đáy
    ///     sprite ở NES 174 khi sàn 176) và hộp nhận/chạm = CẢ ô 16×16 đã dời (−8,−17,16,16) tâm −9 — video Map_2 f6843–6849: roi cấp 3 ĐỨNG
    ///     (hàng NES 156–159) hạ rắn đang bò (hàng đục 165–174) ⇒ hộp NES cao hơn phần đục (CVGP 19: (−8,−9,16,10) "roi đứng không tới").
    ///   • MummyMan: mummy_* 16×48 đục hàng 8/9–47 ⇒ (−8,−39,16,40) tâm −20; băng vải MummyBandage (tâm, hộp riêng trong lớp).
    ///   • Frankenstein: frankenstein_* 16×48 đục hàng 0/1–47 ⇒ (−8,−47,16,48) tâm −23; Igor igor_0/1 16×16 ⇒ (−8,−15,16,16) tâm −7;
    ///     cầu lửa Igor = lớp BoneTowerFireball (AnimSet Igor "fireball" — igor_fireball 8×16 cùng bbox cầu lửa Bone Tower).
    /// </summary>
    static void BuildBlock24()
    {
        var medusaAnim = Anim("MedusaAnimSet");
        var mummyAnim = Anim("MummyAnimSet");
        var igorAnim = Anim("IgorAnimSet");
        var snake = MakeEntity<Snake>("Snake", eEntityGroup.Enemy, FEET, new Vector2Int(0, -1), medusaAnim, ORDER, false, false, 32,
            so => Enemy(so, eEnemyId.Snake, new RectInt(-8, -17, 16, 16), -9));
        var bandage = MakeEntity<MummyBandage>("MummyBandage", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, mummyAnim, 1,
            false, true, 16, null);
        var igorBall = MakeEntity<BoneTowerFireball>("IgorFireball", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, igorAnim, 1,
            false, true, 16, null);
        var igor = MakeEntity<Igor>("Igor", eEntityGroup.Enemy, FEET, new Vector2Int(0, 1), igorAnim, ORDER, false, false, 32,
            so =>
            {
                Enemy(so, eEnemyId.Igor, new RectInt(-8, -15, 16, 16), -7);
                so.FindProperty("fireballPrefab").objectReferenceValue = igorBall;
            });
        Make<QueenMedusa>("QueenMedusa", eBossId.QueenMedusa, new RectInt(-1, -1, 2, 2), Vector2Int.zero, medusaAnim, new RectInt(-16, -16, 32, 32), 0,
            so =>
            {
                so.FindProperty("snakePrefab").objectReferenceValue = snake;
                var root = ((Component)so.targetObject).gameObject;
                var st = new GameObject("Statue");
                st.transform.SetParent(root.transform, false);
                var r = st.AddComponent<SpriteRenderer>();
                r.sortingLayerName = "Character";
                r.sortingOrder = ORDER - 1;
                so.FindProperty("statueView").objectReferenceValue = r;
            });
        Make<MummyMan>("MummyMan", eBossId.MummyMan, FEET, new Vector2Int(0, 1), mummyAnim, new RectInt(-8, -39, 16, 40), -20,
            so => so.FindProperty("bandagePrefab").objectReferenceValue = bandage);
        Make<Frankenstein>("Frankenstein", eBossId.Frankenstein, FEET, new Vector2Int(0, 1), Anim("FrankensteinAnimSet"), new RectInt(-8, -47, 16, 48), -23,
            so => so.FindProperty("igorPrefab").objectReferenceValue = igor);
    }

    /// <summary>
    /// CVGP 20 — boss Block 5–6 (theo wiki, GẦN ĐÚNG). Hộp = bbox pixel đục sprite (XẤP XỈ):
    ///   • GrimReaper: death_0 40×48 pivot tâm (20,24), đục cột 1–39 ⇒ (−19,−24,39,48) nhận + chạm; bay ⇒ viewOffset (0,0), body 2×2.
    ///     DeathScythe: scythe_0..3 16×16 đục kín (hộp trong lớp), nhóm EnemyShot, không tự gỡ khi ra khỏi màn (đuổi Simon).
    ///   • DraculaPhase1: dracula1_0 24×59 pivot (12,59) — hộp NHẬN = đầu (dracula1_head 8×16 ở (6,0), đục hàng 1–14 ⇒ (−6,−58,8,14));
    ///     hộp CHẠM = thân áo khép (−12,−58,24,58); hàng đáy sprite đục ⇒ viewOffset (0,0) (đứng sát mặt sàn như Zombie 07); con "Head"
    ///     (SpriteRenderer order −1, đầu bay ra lúc biến hình).
    ///   • DraculaPhase2: hộp mặt/thân đổi theo khung trong lớp (UpdateBoxes); mặc định khung đứng (−24,−64,24,32) / (−24,−64,48,64).
    ///   • DraculaFireball / DemonFireball: lớp DraculaFireball (BoneTowerFireball + tim khi phá), anim p1_fireball / p2_fireball (8×16,
    ///     cùng bbox cầu lửa Bone Tower).
    /// </summary>
    static void BuildBlock56()
    {
        var deathAnim = Anim("DeathAnimSet");
        var dracAnim = Anim("DraculaAnimSet");
        var scythe = MakeEntity<DeathScythe>("DeathScythe", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, deathAnim, 1,
            false, false, 32, null);
        Make<GrimReaper>("GrimReaper", eBossId.Death, new RectInt(-1, -1, 2, 2), Vector2Int.zero, deathAnim, new RectInt(-19, -24, 39, 48), 0,
            so => so.FindProperty("scythePrefab").objectReferenceValue = scythe);
        var ball1 = MakeEntity<DraculaFireball>("DraculaFireball", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, dracAnim, 1,
            false, true, 16, so => so.FindProperty("animName").stringValue = "p1_fireball");
        var ball2 = MakeEntity<DraculaFireball>("DemonFireball", eEntityGroup.EnemyShot, new RectInt(-1, -1, 2, 2), Vector2Int.zero, dracAnim, 1,
            false, true, 16, so => so.FindProperty("animName").stringValue = "p2_fireball");
        var p2 = Make<DraculaPhase2>("DraculaPhase2", eBossId.Dracula, FEET, Vector2Int.zero, dracAnim, new RectInt(-24, -64, 24, 32), -32,
            so =>
            {
                so.FindProperty("phase").intValue = 2;
                so.FindProperty("touchBox").rectIntValue = new RectInt(-24, -64, 48, 64);
                so.FindProperty("fireballPrefab").objectReferenceValue = ball2;
            });
        Make<DraculaPhase1>("DraculaPhase1", eBossId.Dracula, FEET, Vector2Int.zero, dracAnim, new RectInt(-6, -58, 8, 14), -29,
            so =>
            {
                so.FindProperty("touchBox").rectIntValue = new RectInt(-12, -58, 24, 58);
                so.FindProperty("fireballPrefab").objectReferenceValue = ball1;
                so.FindProperty("phase2Prefab").objectReferenceValue = p2;
                var root = ((Component)so.targetObject).gameObject;
                var h = new GameObject("Head");
                h.transform.SetParent(root.transform, false);
                var r = h.AddComponent<SpriteRenderer>();
                r.sortingLayerName = "Character";
                r.sortingOrder = ORDER + 1;
                r.enabled = false;
                so.FindProperty("headView").objectReferenceValue = r;
            });
    }

    static void Enemy(SerializedObject so, eEnemyId id, RectInt box, int centerDy)
    {
        so.FindProperty("enemyId").enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(eEnemyId)), id);
        so.FindProperty("hurtBox").rectIntValue = box;
        so.FindProperty("touchBox").rectIntValue = box;
        so.FindProperty("centerDy").intValue = centerDy;
    }

    /// <summary>Thực thể phụ của boss (rắn, Igor, đạn) — cùng khuôn CvgpEnemyBuilder.Make.</summary>
    static T MakeEntity<T>(string name, eEntityGroup group, RectInt body, Vector2Int viewOffset, AnimSet anim, int order, bool deadly,
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

    static T Make<T>(string name, eBossId id, RectInt body, Vector2Int viewOffset, AnimSet anim, RectInt box, int centerDy,
                     System.Action<SerializedObject> extra = null) where T : BossBase
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
