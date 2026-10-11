using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dựng prefab HUD (Assets/Data/Prefabs/UI/HUD.prefab) và nội dung IntroScreen / Stage1Stub (CVTI_MEMORY §4.5, §4.6).
/// Chạy lại được: xoá con cũ rồi dựng lại, giữ GUID prefab màn (Main.unity không mất tham chiếu).
/// Toạ độ là NES (x phải, y xuống) — đặt qua NesCoord. Khi có sheet HUD/Stage 1 thật: đổi đường dẫn sprite ở đây rồi chạy lại.
/// </summary>
public static class CvtiIntroPrefabBuilder
{
    const string HUD_PATH = "Assets/Data/Prefabs/UI/HUD.prefab";
    const string INTRO_PATH = "Assets/Data/Prefabs/Screens/IntroScreen.prefab";
    const string STAGE1_PATH = "Assets/Data/Prefabs/Screens/Stage1Stub.prefab";
    const string FONT_PATH = "Assets/Data/Art/Font/NesFont TMP.asset";
    const string ART = "Assets/Data/Art/";

    static readonly Color32 NES_WHITE = new Color32(252, 252, 252, 255);

    [MenuItem("CVTI/Build HUD + Intro + Stage1 prefabs")]
    public static void BuildAll()
    {
        BuildHud();
        BuildIntro();
        BuildStage1();
        AssetDatabase.SaveAssets();
        Debug.Log("[CvtiIntroPrefabBuilder] Đã dựng HUD, IntroScreen, Stage1Stub");
    }

    static void BuildHud()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        if (font == null)
            throw new System.Exception("Thiếu " + FONT_PATH + " — chạy CVTI/Build NesFont TMP trước");

        var root = new GameObject("HUD", typeof(RectTransform));
        try
        {
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = "HUD";
            var rt = (RectTransform)root.transform;
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(NesCoord.SCREEN_W, 48);
            rt.localScale = Vector3.one * NesCoord.UNIT_PER_PIXEL;
            rt.localPosition = Vector3.zero;

            var status = MakeText(root, "StatusLine", font, 0, 16, 31, "SCORE-000000 TIME 0300 STAGE 01");
            MakeText(root, "PlayerLabel", font, 0, 24, 6, "PLAYER");
            MakeText(root, "EnemyLabel", font, 0, 32, 5, "ENEMY");
            var playerTicks = MakeTicks(root, "PlayerBar", LoadSprite("Intro/hud_tick_player_temp.png"), 25);
            var enemyTicks = MakeTicks(root, "EnemyBar", LoadSprite("Intro/hud_tick_enemy_temp.png"), 33);
            MakeImage(root, "SubWeaponFrame", LoadSprite("Intro/hud_subweapon_frame_temp.png"), 128, 24);
            MakeImage(root, "Heart", LoadSprite("Intro/hud_heart_temp.png"), 168, 24);
            var hearts = MakeText(root, "Hearts", font, 176, 24, 3, "-05");
            var lives = MakeText(root, "Lives", font, 168, 32, 4, "P-03");

            var hud = root.AddComponent<Hud>();
            var so = new SerializedObject(hud);
            so.FindProperty("statusLine").objectReferenceValue = status;
            so.FindProperty("heartsText").objectReferenceValue = hearts;
            so.FindProperty("livesText").objectReferenceValue = lives;
            SetArray(so.FindProperty("playerTicks"), playerTicks);
            SetArray(so.FindProperty("enemyTicks"), enemyTicks);
            so.ApplyModifiedPropertiesWithoutUndo();

            System.IO.Directory.CreateDirectory("Assets/Data/Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(root, HUD_PATH);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    static void BuildIntro()
    {
        var root = PrefabUtility.LoadPrefabContents(INTRO_PATH);
        try
        {
            ClearChildren(root.transform);
            AddHud(root.transform);
            MakeRenderer(root.transform, "Background", LoadSprite("Intro/intro_bg.png"), "Background", 0, NesCoord.ToWorld(0, 48));
            var cloud = MakeRenderer(root.transform, "Cloud", LoadSprite("Intro/intro_cloud.png"), "BackgroundFX", 0, NesCoord.ToWorld(222, 73));
            var batA = MakeRenderer(root.transform, "BatA", LoadSprite("Intro/intro_bat_0.png"), "BackgroundFX", 1, Vector3.zero);
            var batB = MakeRenderer(root.transform, "BatB", LoadSprite("Intro/intro_bat_0.png"), "BackgroundFX", 1, Vector3.zero);
            var walk0 = LoadSprite("Character/Simon/walk_0.png");
            var simon = MakeRenderer(root.transform, "Simon", walk0, "Character", 0, NesCoord.ToWorld(251, 210));

            var so = new SerializedObject(root.GetComponent<IntroScreen>());
            so.FindProperty("simon").objectReferenceValue = simon;
            SetArray(so.FindProperty("simonWalk"), new Object[] { walk0, LoadSprite("Character/Simon/walk_1.png"), LoadSprite("Character/Simon/walk_2.png") });
            so.FindProperty("simonBack").objectReferenceValue = LoadSprite("Character/Simon/back_0.png");
            so.FindProperty("cloud").objectReferenceValue = cloud.transform;
            so.FindProperty("batA").objectReferenceValue = batA;
            so.FindProperty("batB").objectReferenceValue = batB;
            SetArray(so.FindProperty("batFrames"), new Object[] { LoadSprite("Intro/intro_bat_0.png"), LoadSprite("Intro/intro_bat_1.png") });
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, INTRO_PATH);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void BuildStage1()
    {
        var root = PrefabUtility.LoadPrefabContents(STAGE1_PATH);
        try
        {
            ClearChildren(root.transform);
            AddHud(root.transform);
            // Chưa có sheet Stage 1 ⇒ nền đen (camera xoá đen). Simon đứng quay phải như video 16.4 s: góc (42,161), chân y 193.
            var simon = MakeRenderer(root.transform, "Simon", LoadSprite("Character/Simon/idle_0.png"), "Character", 0, NesCoord.ToWorld(50, 193));
            simon.flipX = true;
            PrefabUtility.SaveAsPrefabAsset(root, STAGE1_PATH);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void AddHud(Transform parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HUD_PATH);
        var hud = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        hud.transform.localPosition = Vector3.zero;
    }

    static void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(t.GetChild(i).gameObject);
    }

    static Sprite LoadSprite(string relPath)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(ART + relPath);
        if (s == null)
            throw new System.Exception("Thiếu sprite " + ART + relPath);
        return s;
    }

    static SpriteRenderer MakeRenderer(Transform parent, string name, Sprite sprite, string layer, int order, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = layer;
        sr.sortingOrder = order;
        return sr;
    }

    // RectTransform góc trên-trái tại (x, y) px NES trong canvas HUD.
    static RectTransform MakeRect(GameObject parent, string name, int x, int y, int w, int h)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent.transform, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    static TextMeshProUGUI MakeText(GameObject parent, string name, TMP_FontAsset font, int x, int y, int chars, string text)
    {
        var tmp = MakeRect(parent, name, x, y, chars * NesFont.CELL, NesFont.CELL).gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.fontSize = NesFont.CELL;
        tmp.color = NES_WHITE;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.margin = Vector4.zero;
        tmp.richText = false;
        tmp.raycastTarget = false;
        tmp.text = text;
        return tmp;
    }

    static Image MakeImage(GameObject parent, string name, Sprite sprite, int x, int y)
    {
        var size = sprite.rect.size;
        var img = MakeRect(parent, name, x, y, (int)size.x, (int)size.y).gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        return img;
    }

    // 16 vạch máu 3×6, bước 4 px từ x 56 (đo từ video).
    static Image[] MakeTicks(GameObject parent, string name, Sprite tick, int y)
    {
        var bar = MakeRect(parent, name, 56, y, GameSession.MAX_HP * 4, 6).gameObject;
        var ticks = new Image[GameSession.MAX_HP];
        for (int i = 0; i < ticks.Length; i++)
            ticks[i] = MakeImage(bar, "Tick" + i, tick, i * 4, 0);
        return ticks;
    }

    static void SetArray(SerializedProperty prop, Object[] values)
    {
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }
}
