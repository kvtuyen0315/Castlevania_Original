using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Đọc manifest sprite gameplay Tools/cvgp/slices_gameplay.json (nguồn sự thật — CVGP_MEMORY §4, routine 05).
/// Dùng chung cho NesSpriteImporter (pivot từng sprite) và AnimSetBuilder (anim + bảng màu).
/// Trường không khai báo ở đây (keyColor, fill, alsoAt…) do tool python dùng, JsonUtility bỏ qua.
/// </summary>
public static class CvgpSpriteManifest
{
    public const string RELATIVE_PATH = "Tools/cvgp/slices_gameplay.json";
    public const string MAP2_RELATIVE_PATH = "Tools/cvm2/slices_map2.json";   // CVM2 01: sprite mới Map 2 (chỉ cần pivot)
    public const string ART_ROOT = "Assets/Data/Art/";

    [Serializable]
    public class Slice
    {
        public string name;
        public string outDir;
        public int[] rect;
        public int[] pivot;   // [px, py] tính từ góc TRÊN-TRÁI sprite
        public string src;
    }

    [Serializable]
    public class Anim
    {
        public string set;
        public string name;
        public string[] frames;
        public int[] durations;
        public bool loop;
        public bool measured;
        public string[] overlays;
        public int[] overlayOffsets;   // phẳng [x0,y0,x1,y1,…]
        public bool[] overlayOnTop;
        public string note;
    }

    [Serializable]
    public class Palette
    {
        public string set;
        public string name;
        public int[] from;   // phẳng [r,g,b,…]
        public int[] to;
        public string note;
    }

    [Serializable]
    public class Doc
    {
        public string animDir;
        public Slice[] slices;
        public Anim[] anims;
        public Palette[] palettes;
    }

    static Doc cached;
    static DateTime cachedTime;
    static Dictionary<string, Slice> byAssetPath;

    public static string FullPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, RELATIVE_PATH);

    /// <summary>Đọc (có cache theo thời điểm sửa file). Không có file ⇒ null.</summary>
    public static Doc Load()
    {
        var path = FullPath;
        if (!File.Exists(path))
            return null;
        var t = File.GetLastWriteTimeUtc(path);
        if (cached == null || t != cachedTime)
        {
            cached = JsonUtility.FromJson<Doc>(File.ReadAllText(path));
            cachedTime = t;
            byAssetPath = new Dictionary<string, Slice>();
            foreach (var s in cached.slices)
                byAssetPath[s.outDir + "/" + s.name + ".png"] = s;
        }
        return cached;
    }

    /// <summary>Mục manifest của 1 PNG (đường dẫn asset "Assets/…png") hoặc null.</summary>
    public static Slice FindSlice(string assetPath)
    {
        if (Load() == null)
            return null;
        Slice s;
        if (byAssetPath.TryGetValue(assetPath, out s))
            return s;
        return FindMap2Slice(assetPath);
    }

    static Doc map2Cached;
    static DateTime map2Time;
    static Dictionary<string, Slice> map2ByAssetPath;

    static Slice FindMap2Slice(string assetPath)
    {
        var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, MAP2_RELATIVE_PATH);
        if (!File.Exists(path))
            return null;
        var t = File.GetLastWriteTimeUtc(path);
        if (map2Cached == null || t != map2Time)
        {
            map2Cached = JsonUtility.FromJson<Doc>(File.ReadAllText(path));
            map2Time = t;
            map2ByAssetPath = new Dictionary<string, Slice>();
            foreach (var s in map2Cached.slices)
                map2ByAssetPath[s.outDir + "/" + s.name + ".png"] = s;
        }
        Slice r;
        return map2ByAssetPath.TryGetValue(assetPath, out r) ? r : null;
    }
}
