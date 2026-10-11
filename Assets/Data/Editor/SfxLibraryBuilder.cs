using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dựng Assets/Data/Audio/SfxLibrary.asset từ Assets/Data/Audio/SFX/sfx_NN_*.wav (NN = giá trị eSfx).
/// Chạy lại được: cập nhật asset sẵn có, giữ GUID.
/// </summary>
public static class SfxLibraryBuilder
{
    const string SFX_DIR = "Assets/Data/Audio/SFX";
    const string ASSET_PATH = "Assets/Data/Audio/SfxLibrary.asset";

    [MenuItem("CVGP/Build SfxLibrary")]
    public static void Build()
    {
        var entries = new List<SfxLibrary.Entry>();
        var rx = new Regex(@"^sfx_(\d\d)_");
        foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { SFX_DIR }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            var m = rx.Match(System.IO.Path.GetFileName(path));
            if (clip == null || !m.Success)
                continue;
            var id = (eSfx)int.Parse(m.Groups[1].Value);
            if (!System.Enum.IsDefined(typeof(eSfx), id))
            {
                Debug.LogWarning("SfxLibraryBuilder: số " + m.Groups[1].Value + " không có trong eSfx — bỏ qua " + path);
                continue;
            }
            entries.Add(new SfxLibrary.Entry { id = id, clip = clip });
        }
        entries.Sort((a, b) => ((int)a.id).CompareTo((int)b.id));

        var lib = AssetDatabase.LoadAssetAtPath<SfxLibrary>(ASSET_PATH);
        if (lib == null)
        {
            lib = ScriptableObject.CreateInstance<SfxLibrary>();
            AssetDatabase.CreateAsset(lib, ASSET_PATH);
        }
        lib.SetEntries(entries.ToArray());
        EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssets();
        Debug.Log("SfxLibrary: " + entries.Count + " mục");
    }
}
