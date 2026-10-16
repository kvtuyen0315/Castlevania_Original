using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dựng Assets/Data/Audio/MusicLibrary.asset từ Tools/cvm2/music_loops.json (đo bởi Tools/cvm2/music3.py loop2 + export — CVM2 03)
/// + clip Assets/Data/Audio/Music/&lt;file&gt;. Chạy lại được: cập nhật asset sẵn có (giữ GUID); gán AudioManager.musicLibrary trên
/// prefab GameManager nếu còn trống. Ghi log số mục / số thay đổi.
/// </summary>
public static class MusicLibraryBuilder
{
    const string JSON_PATH = "Tools/cvm2/music_loops.json";
    const string ASSET_PATH = "Assets/Data/Audio/MusicLibrary.asset";
    const string GM_PREFAB = "Assets/Data/Prefabs/GameManager.prefab";

    [Serializable]
    class Row
    {
        public string id;
        public int num;
        public string file;
        public int startSample;
        public bool loop;
        public int loopStart;
        public int loopEnd;
        public float loopSeconds;
        public string note;
        public string src;
    }

    [Serializable]
    class Rows
    {
        public Row[] rows;
    }

    [MenuItem("CVGP/Build MusicLibrary")]
    public static void Build()
    {
        string json = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath), JSON_PATH));
        var rows = JsonUtility.FromJson<Rows>("{\"rows\":" + json + "}").rows;
        var entries = new List<MusicLibrary.Entry>();
        int errors = 0;
        foreach (var r in rows)
        {
            eMusic id;
            if (!Enum.TryParse(r.id, out id) || (int)id != r.num)
            {
                Debug.LogError("MusicLibraryBuilder: id '" + r.id + "' / số " + r.num + " không khớp eMusic");
                errors++;
                continue;
            }
            string path = MusicImporter.MUSIC_ROOT + r.file;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogError("MusicLibraryBuilder: thiếu clip " + path);
                errors++;
                continue;
            }
            if (r.loop && (r.loopStart <= r.startSample || r.loopEnd <= r.loopStart || r.loopEnd >= clip.samples))
            {
                Debug.LogError("MusicLibraryBuilder: điểm lặp sai " + r.id + " (" + r.loopStart + "–" + r.loopEnd + ", clip " + clip.samples + ")");
                errors++;
                continue;
            }
            entries.Add(new MusicLibrary.Entry
            {
                id = id,
                clip = clip,
                startSample = r.startSample,
                loop = r.loop,
                loopStart = r.loop ? r.loopStart : 0,
                loopEnd = r.loop ? r.loopEnd : 0,
                source = r.src + (string.IsNullOrEmpty(r.note) ? "" : " — " + r.note),
            });
        }
        entries.Sort((a, b) => ((int)a.id).CompareTo((int)b.id));

        var lib = AssetDatabase.LoadAssetAtPath<MusicLibrary>(ASSET_PATH);
        bool created = lib == null;
        if (created)
        {
            lib = ScriptableObject.CreateInstance<MusicLibrary>();
            AssetDatabase.CreateAsset(lib, ASSET_PATH);
        }
        string before = EditorJsonUtility.ToJson(lib);
        lib.SetEntries(entries.ToArray());
        bool changed = created || before != EditorJsonUtility.ToJson(lib);
        if (changed)
            EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssets();

        bool assigned = AssignToGameManager(lib);
        Debug.Log("MusicLibrary: " + entries.Count + " mục, " + errors + " lỗi, " + (changed ? "ghi" : "không đổi")
                  + (assigned ? " + gán AudioManager.musicLibrary (GameManager.prefab)" : ""));
    }

    static bool AssignToGameManager(MusicLibrary lib)
    {
        var root = PrefabUtility.LoadPrefabContents(GM_PREFAB);
        try
        {
            var am = root.GetComponentInChildren<AudioManager>(true);
            if (am == null)
                return false;
            var so = new SerializedObject(am);
            var prop = so.FindProperty("musicLibrary");
            if (prop == null || prop.objectReferenceValue == lib)
                return false;
            prop.objectReferenceValue = lib;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, GM_PREFAB);
            return true;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
