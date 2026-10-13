using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CVGP 12 — dựng prefab màn GAME OVER (Assets/Data/Prefabs/Screens/GameOverScreen.prefab) và đặt 1 instance (tắt) dưới ScreenRoot
/// của Main.unity nếu chưa có. Chạy lại được: dựng lại con của prefab, giữ GUID; scene chỉ ghi khi phải thêm instance.
/// Bố cục là QUY ƯỚC (video/wiki không có Game Over): "GAME OVER" giữa màn y 96, "CONTINUE" (96,128), "END" (96,144), con trỏ tim x 84.
/// </summary>
public static class CvgpGameOverBuilder
{
    const string PREFAB = "Assets/Data/Prefabs/Screens/GameOverScreen.prefab";
    const string FONT = "Assets/Data/Art/Font/NesFont.asset";
    const string HEART = "Assets/Data/Art/Item/heart_small.png";
    const string MAIN = "Assets/Scenes/Main.unity";

    [MenuItem("CVGP/Build GameOver Screen (CVGP 12)")]
    public static void Build()
    {
        var font = AssetDatabase.LoadAssetAtPath<NesFont>(FONT);
        var heart = AssetDatabase.LoadAssetAtPath<Sprite>(HEART);
        if (font == null || heart == null)
            throw new System.Exception("Thiếu " + FONT + " hoặc " + HEART);

        bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB) != null;
        var root = exists ? PrefabUtility.LoadPrefabContents(PREFAB) : new GameObject("GameOverScreen");
        try
        {
            for (int i = root.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            var screen = root.GetComponent<GameOverScreen>();
            if (screen == null)
                screen = root.AddComponent<GameOverScreen>();
            MakeText(root.transform, "Title", font, "GAME OVER", 92, 96);
            MakeText(root.transform, "Continue", font, "CONTINUE", 96, 128);
            MakeText(root.transform, "End", font, "END", 96, 144);
            var go = new GameObject("Cursor");
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = NesCoord.ToWorld(84 + 4, 128 + 16);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = heart;
            sr.sortingLayerName = "HUD";
            var so = new SerializedObject(screen);
            so.FindProperty("cursor").objectReferenceValue = sr;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, PREFAB);
        }
        finally
        {
            if (exists)
                PrefabUtility.UnloadPrefabContents(root);
            else
                Object.DestroyImmediate(root);
        }
        AddToMain();
        AssetDatabase.SaveAssets();
        Debug.Log("[CvgpGameOverBuilder] Đã dựng " + PREFAB);
    }

    static void MakeText(Transform parent, string name, NesFont font, string text, int x, int y)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = NesCoord.ToWorld(x, y);
        var t = go.AddComponent<NesText>();
        var so = new SerializedObject(t);
        so.FindProperty("font").objectReferenceValue = font;
        so.FindProperty("text").stringValue = text;
        so.FindProperty("sortingLayer").stringValue = "HUD";
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AddToMain()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != MAIN)
        {
            Debug.LogWarning("[CvgpGameOverBuilder] Scene đang mở không phải Main.unity — mở Main.unity rồi chạy lại để thêm instance");
            return;
        }
        GameObject screenRoot = null;
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "ScreenRoot")
                screenRoot = go;
        if (screenRoot == null)
        {
            Debug.LogError("[CvgpGameOverBuilder] Main.unity không có ScreenRoot");
            return;
        }
        if (screenRoot.GetComponentInChildren<GameOverScreen>(true) != null)
            return;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB);
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, screenRoot.transform);
        inst.transform.localPosition = Vector3.zero;
        inst.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[CvgpGameOverBuilder] Đã thêm GameOverScreen vào Main.unity/ScreenRoot");
    }
}
