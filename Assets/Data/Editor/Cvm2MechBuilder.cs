using UnityEditor;
using UnityEngine;

/// <summary>
/// CVM2 06 — prefab cơ chế địa hình động Map 2 (Assets/Data/Prefabs/Level/Block2/Mech/): MovingPlatform (sprite Object/platform),
/// Crusher (8 khung Object/crusher_0..7). Menu CVM2/Build Mech Prefabs (chạy lại được: SaveAsPrefabAsset giữ GUID).
/// Lớp vẽ BackgroundFX (sau Simon/enemy, trên nền) — bẫy là ô nền NES; bệ là sprite nhưng Simon luôn vẽ đè (video).
/// Thử tay (Play Arena, Player = Simon): CVM2/Spawn/Mech TowerEntranceB | TowerHallB | TowerHallC (Map2MechProbe.Enter).
/// </summary>
public static class Cvm2MechBuilder
{
    public const string DIR = "Assets/Data/Prefabs/Level/Block2/Mech";
    public const string PLATFORM = Map2Mechanisms.PLATFORM_PREFAB;
    public const string CRUSHER = Map2Mechanisms.CRUSHER_PREFAB;
    const string ART = "Assets/Data/Art/Object/";

    [MenuItem("CVM2/Build Mech Prefabs")]
    static void Menu()
    {
        Debug.Log("[CVM2] " + Build());
    }

    [MenuItem("CVM2/Spawn/Mech TowerEntranceB (bệ)")]
    static void SpawnTeb() { Debug.Log("[CVM2] " + Map2MechProbe.Enter("TowerEntranceB", 104, 80)); }

    [MenuItem("CVM2/Spawn/Mech TowerHallB (2 bệ)")]
    static void SpawnHallB() { Debug.Log("[CVM2] " + Map2MechProbe.Enter("TowerHallB", 368, 80)); }

    [MenuItem("CVM2/Spawn/Mech TowerHallC (3 bẫy ép)")]
    static void SpawnHallC() { Debug.Log("[CVM2] " + Map2MechProbe.Enter("TowerHallC", 400, 144)); }

    [MenuItem("CVM2/Spawn/Mech TowerEntranceB (bệ)", true)]
    [MenuItem("CVM2/Spawn/Mech TowerHallB (2 bệ)", true)]
    [MenuItem("CVM2/Spawn/Mech TowerHallC (3 bẫy ép)", true)]
    static bool CanSpawn() { return Application.isPlaying && StageWorld.Current != null; }

    public static string Build()
    {
        EnsureFolder(DIR);
        var plat = AssetDatabase.LoadAssetAtPath<Sprite>(ART + "platform.png");
        if (plat == null)
            return "LỖI: thiếu sprite " + ART + "platform.png";
        var frames = new Sprite[Crusher.FRAME_COUNT];
        for (int k = 0; k < frames.Length; k++)
        {
            frames[k] = AssetDatabase.LoadAssetAtPath<Sprite>(ART + "crusher_" + k + ".png");
            if (frames[k] == null)
                return "LỖI: thiếu sprite " + ART + "crusher_" + k + ".png";
        }
        // bệ: pivot sprite giữa-trên = mặt đứng; sprite NES vẽ thấp 1 px (video NES y 113 vs mặt sàn 112)
        Make<MovingPlatform>(PLATFORM, new RectInt(-16, 0, 32, 8), new Vector2Int(0, 1), plat, 1, null);
        // bẫy: ô nền NES — hàng đầu đúng mặt treo (video NES y 112) ⇒ viewOffset 0
        Make<Crusher>(CRUSHER, new RectInt(-16, 0, 32, 64), Vector2Int.zero, frames[0], 0, so =>
        {
            var p = so.FindProperty("frames");
            p.arraySize = frames.Length;
            for (int k = 0; k < frames.Length; k++)
                p.GetArrayElementAtIndex(k).objectReferenceValue = frames[k];
        });
        AssetDatabase.SaveAssets();
        return "Mech prefabs: " + PLATFORM + ", " + CRUSHER;
    }

    static void Make<T>(string path, RectInt body, Vector2Int viewOffset, Sprite sprite, int order, System.Action<SerializedObject> extra) where T : Entity
    {
        var go = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        try
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "BackgroundFX";
            sr.sortingOrder = order;
            sr.sprite = sprite;
            var c = go.AddComponent<T>();
            var so = new SerializedObject(c);
            so.FindProperty("group").enumValueIndex = (int)eEntityGroup.Prop;
            so.FindProperty("body").rectIntValue = body;
            so.FindProperty("view").objectReferenceValue = sr;
            so.FindProperty("viewOffset").vector2IntValue = viewOffset;
            so.FindProperty("animSet").objectReferenceValue = null;
            so.FindProperty("checkDeadlyTiles").boolValue = false;
            so.FindProperty("despawnOffscreen").boolValue = false;
            if (extra != null)
                extra(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(go, path);
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
