using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CVGP 09 — dựng prefab vũ khí phụ (Assets/Data/Prefabs/Weapon/*.prefab) + gán vào Simon.prefab.
/// Menu CVGP/Build Weapon Prefabs (chạy lại được: SaveAsPrefabAsset giữ GUID; Simon.prefab chỉ ghi khi tham chiếu đổi).
/// Hộp đánh mỗi vũ khí = bbox pixel ĐỤC của sprite (đọc PNG gốc — sheet là chuẩn pixel, H1), quanh pivot sprite (manifest:
/// vũ khí tâm, lửa đáy-giữa), quy ước quay TRÁI của Entity; sprite vũ khí trên sheet vẽ hướng PHẢI (khớp mẫu video) ⇒ lật x.
/// Lửa: bbox hợp của 4 khung lửa đang dùng (fire_1..4).
/// </summary>
public static class CvgpWeaponBuilder
{
    public const string DIR = "Assets/Data/Prefabs/Weapon";
    const string ANIM = "Assets/Data/Config/Anim/SubWeaponAnimSet.asset";
    const string ART = "Assets/Data/Art/SubWeapon/";
    const string SIMON_PREFAB = "Assets/Data/Prefabs/Character/Simon.prefab";

    [MenuItem("CVGP/Build Weapon Prefabs")]
    static void Menu()
    {
        Debug.Log("[CVGP] " + Build());
    }

    public static string Build()
    {
        if (!AssetDatabase.IsValidFolder(DIR))
            AssetDatabase.CreateFolder("Assets/Data/Prefabs", "Weapon");
        var fire = Make<HolyFire>("HolyFire", eSubWeapon.HolyWater, "holywater_fire",
                                  new[] { "holywater_fire_1", "holywater_fire_2", "holywater_fire_3", "holywater_fire_4" }, true, null);
        var dagger = Make<Dagger>("Dagger", eSubWeapon.Dagger, "dagger", new[] { "dagger_0" }, false, null);
        var axe = Make<Axe>("Axe", eSubWeapon.Axe, "axe", new[] { "axe_0", "axe_1", "axe_2", "axe_3" }, false, null);
        var holy = Make<HolyWater>("HolyWater", eSubWeapon.HolyWater, "holywater_bottle", new[] { "holywater_bottle" }, false,
                                   so =>
                                   {
                                       so.FindProperty("firePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<HolyFire>(DIR + "/HolyFire.prefab");
                                       var b = OpaqueBox("holywater_bottle");
                                       so.FindProperty("bottomOffset").intValue = b.yMax - 8;   // pivot tâm 8×16 ⇒ hàng tâm = 8
                                   });
        var cross = Make<Cross>("Cross", eSubWeapon.Cross, "cross", new[] { "cross_0", "cross_spin_0", "cross_spin_1" }, false, null);
        var watch = Make<Stopwatch>("Stopwatch", eSubWeapon.Stopwatch, null, null, false, null);
        string simonMsg = AssignToSimon(dagger, axe, holy, cross, watch);
        AssetDatabase.SaveAssets();
        return "Weapon prefabs: " + string.Join(", ", new[] { dagger.name, axe.name, holy.name, fire.name, cross.name, watch.name }) + "; " + simonMsg;
    }

    /// <summary>Gán 5 prefab vào Simon.prefab (chỉ ghi khi khác).</summary>
    public static string AssignToSimon(SubWeaponProjectile dagger, SubWeaponProjectile axe, SubWeaponProjectile holy,
                                       SubWeaponProjectile cross, SubWeaponProjectile watch)
    {
        var root = PrefabUtility.LoadPrefabContents(SIMON_PREFAB);
        try
        {
            var c = root.GetComponent<SimonController>();
            if (c.PrefabOf(eSubWeapon.Dagger) == dagger && c.PrefabOf(eSubWeapon.Axe) == axe && c.PrefabOf(eSubWeapon.HolyWater) == holy
                && c.PrefabOf(eSubWeapon.Cross) == cross && c.PrefabOf(eSubWeapon.Stopwatch) == watch)
                return "Simon.prefab không đổi";
            c.SetWeaponPrefabs(dagger, axe, holy, cross, watch);
            PrefabUtility.SaveAsPrefabAsset(root, SIMON_PREFAB);
            return "Simon.prefab đã gán";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Tải prefab đã dựng (CvgpArenaBuilder.BuildSimonPrefab gọi khi dựng lại Simon).</summary>
    public static T Load<T>(string name) where T : SubWeaponProjectile
    {
        return AssetDatabase.LoadAssetAtPath<T>(DIR + "/" + name + ".prefab");
    }

    static T Make<T>(string name, eSubWeapon kind, string animName, string[] sprites, bool bottomPivot,
                     System.Action<SerializedObject> extra) where T : SubWeaponProjectile
    {
        var go = new GameObject(name);
        try
        {
            SpriteRenderer sr = null;
            if (sprites != null)
            {
                sr = go.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Character";
                sr.sortingOrder = 1;   // vũ khí vẽ trước thân Simon (thân 0, roi ±1)
            }
            var c = go.AddComponent<T>();
            var so = new SerializedObject(c);
            so.FindProperty("group").enumValueIndex = (int)eEntityGroup.PlayerWeapon;
            so.FindProperty("body").rectIntValue = new RectInt(-1, -1, 2, 2);
            so.FindProperty("view").objectReferenceValue = sr;
            so.FindProperty("viewOffset").vector2IntValue = Vector2Int.zero;
            so.FindProperty("animSet").objectReferenceValue = sprites != null ? AssetDatabase.LoadAssetAtPath<AnimSet>(ANIM) : null;
            so.FindProperty("checkDeadlyTiles").boolValue = false;
            so.FindProperty("despawnOffscreen").boolValue = kind != eSubWeapon.Stopwatch;
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.FindProperty("animName").stringValue = animName ?? "";
            so.FindProperty("spriteFacesRight").boolValue = true;
            so.FindProperty("countsAsShot").boolValue = true;
            so.FindProperty("hitRect").rectIntValue = sprites != null ? HitRect(sprites, bottomPivot) : new RectInt(0, 0, 0, 0);
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

    /// <summary>Hợp bbox đục các khung, đổi sang toạ độ quanh pivot (tâm hoặc đáy-giữa), rồi lật sang quy ước quay TRÁI.</summary>
    static RectInt HitRect(string[] sprites, bool bottomPivot)
    {
        int x0 = int.MaxValue, y0 = int.MaxValue, x1 = int.MinValue, y1 = int.MinValue, w = 0, h = 0;
        foreach (var s in sprites)
        {
            var b = OpaqueBox(s, out w, out h);
            x0 = Mathf.Min(x0, b.xMin - w / 2);
            x1 = Mathf.Max(x1, b.xMax - w / 2);
            int py = bottomPivot ? h : h / 2;
            y0 = Mathf.Min(y0, b.yMin - py);
            y1 = Mathf.Max(y1, b.yMax - py);
        }
        // sheet hướng PHẢI = Facing +1; hộp lưu quay TRÁI ⇒ lật: x' = −(x + w)
        int rw = x1 - x0;
        return new RectInt(-(x0 + rw), y0, rw, y1 - y0);
    }

    static RectInt OpaqueBox(string sprite)
    {
        int w, h;
        return OpaqueBox(sprite, out w, out h);
    }

    /// <summary>Bbox pixel alpha &gt; 0 của PNG (y từ TRÊN xuống), đọc thẳng file (không cần Read/Write enabled).</summary>
    static RectInt OpaqueBox(string sprite, out int w, out int h)
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            tex.LoadImage(File.ReadAllBytes(ART + sprite + ".png"));
            w = tex.width;
            h = tex.height;
            var px = tex.GetPixels32();
            int x0 = w, y0 = h, x1 = -1, y1 = -1;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (px[y * w + x].a > 0)
                    {
                        int ty = h - 1 - y;   // Texture2D: hàng 0 = đáy
                        x0 = Mathf.Min(x0, x);
                        x1 = Mathf.Max(x1, x);
                        y0 = Mathf.Min(y0, ty);
                        y1 = Mathf.Max(y1, ty);
                    }
            return new RectInt(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
        }
        finally
        {
            Object.DestroyImmediate(tex);
        }
    }
}
