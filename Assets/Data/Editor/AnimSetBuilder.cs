using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dựng Assets/Data/Config/Anim/&lt;Set&gt;AnimSet.asset từ mục "anims" + "palettes" của manifest
/// Tools/cvgp/slices_gameplay.json (thứ tự anim = thứ tự manifest).
/// Chạy lại được: asset sẵn có chỉ ghi khi nội dung khác (CopySerialized, giữ GUID); có lỗi ⇒ không ghi asset nào.
/// </summary>
public static class AnimSetBuilder
{
    [MenuItem("CVGP/Build AnimSets")]
    public static void BuildMenu()
    {
        Debug.Log(Build());
    }

    /// <summary>Trả chuỗi tóm tắt; lỗi được log bằng Debug.LogError.</summary>
    public static string Build()
    {
        var doc = CvgpSpriteManifest.Load();
        if (doc == null)
        {
            Debug.LogError("AnimSetBuilder: không thấy " + CvgpSpriteManifest.FullPath);
            return "AnimSet: LỖI — không có manifest";
        }

        var errors = new List<string>();
        var order = new List<string>();
        var sets = new Dictionary<string, List<AnimSet.Anim>>();
        var names = new HashSet<string>();
        foreach (var a in doc.anims)
        {
            string ctx = a.set + "/" + a.name;
            if (!names.Add(ctx))
                errors.Add(ctx + ": trùng tên anim");
            if (!sets.ContainsKey(a.set))
            {
                sets[a.set] = new List<AnimSet.Anim>();
                order.Add(a.set);
            }
            int n = a.frames == null ? 0 : a.frames.Length;
            if (n == 0)
                errors.Add(ctx + ": không có khung");
            if (a.durations == null || a.durations.Length != n)
                errors.Add(ctx + ": durations phải có " + n + " phần tử");
            else
                foreach (var d in a.durations)
                    if (d < 1)
                        errors.Add(ctx + ": duration < 1");
            int no = a.overlays == null ? 0 : a.overlays.Length;
            if (no != 0 && (no != n || a.overlayOffsets == null || a.overlayOffsets.Length != 2 * n
                            || a.overlayOnTop == null || a.overlayOnTop.Length != n))
                errors.Add(ctx + ": overlays/overlayOffsets/overlayOnTop không khớp số khung");

            var anim = new AnimSet.Anim
            {
                name = a.name,
                frames = LoadSprites(a.frames, ctx, errors),
                durations = a.durations ?? new int[0],
                loop = a.loop,
                measured = a.measured,
                overlays = no == 0 ? new Sprite[0] : LoadSprites(a.overlays, ctx, errors),
                overlayOffsets = new Vector2Int[no],
                overlayOnTop = no == 0 ? new bool[0] : a.overlayOnTop,
                note = a.note ?? ""
            };
            for (int i = 0; i < no && a.overlayOffsets != null && 2 * i + 1 < a.overlayOffsets.Length; i++)
                anim.overlayOffsets[i] = new Vector2Int(a.overlayOffsets[2 * i], a.overlayOffsets[2 * i + 1]);
            sets[a.set].Add(anim);
        }

        var palettes = new Dictionary<string, List<AnimSet.Palette>>();
        if (doc.palettes != null)
            foreach (var p in doc.palettes)
            {
                if (!sets.ContainsKey(p.set))
                    errors.Add("palette " + p.name + ": set '" + p.set + "' không có anim");
                if (p.from == null || p.to == null || p.from.Length != p.to.Length || p.from.Length % 3 != 0)
                {
                    errors.Add("palette " + p.set + "/" + p.name + ": from/to phải cùng độ dài, bội 3");
                    continue;
                }
                if (!palettes.ContainsKey(p.set))
                    palettes[p.set] = new List<AnimSet.Palette>();
                palettes[p.set].Add(new AnimSet.Palette { name = p.name, from = ToColors(p.from), to = ToColors(p.to), note = p.note ?? "" });
            }

        if (errors.Count > 0)
        {
            foreach (var e in errors)
                Debug.LogError("AnimSetBuilder: " + e);
            return "AnimSet: " + errors.Count + " lỗi — KHÔNG ghi asset nào";
        }

        string dir = string.IsNullOrEmpty(doc.animDir) ? "Assets/Data/Config/Anim" : doc.animDir;
        EnsureFolder(dir);
        int written = 0, same = 0, frames = 0;
        var sb = new StringBuilder();
        foreach (var set in order)
        {
            var path = dir + "/" + set + "AnimSet.asset";
            var temp = ScriptableObject.CreateInstance<AnimSet>();
            temp.name = set + "AnimSet";
            List<AnimSet.Palette> pal;
            palettes.TryGetValue(set, out pal);
            temp.SetData(sets[set].ToArray(), pal == null ? null : pal.ToArray());
            foreach (var a in sets[set])
                frames += a.frames.Length;

            var existing = AssetDatabase.LoadAssetAtPath<AnimSet>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(temp, path);
                written++;
                continue;
            }
            if (EditorJsonUtility.ToJson(existing) == EditorJsonUtility.ToJson(temp))
                same++;
            else
            {
                EditorUtility.CopySerialized(temp, existing);
                EditorUtility.SetDirty(existing);
                written++;
            }
            Object.DestroyImmediate(temp);
        }
        AssetDatabase.SaveAssets();
        sb.Append("AnimSet: ").Append(order.Count).Append(" bộ, ").Append(doc.anims.Length).Append(" anim, ")
          .Append(frames).Append(" khung; ghi lại ").Append(written).Append(", không đổi ").Append(same);
        return sb.ToString();
    }

    static Sprite[] LoadSprites(string[] paths, string ctx, List<string> errors)
    {
        var result = new Sprite[paths == null ? 0 : paths.Length];
        for (int i = 0; i < result.Length; i++)
        {
            var p = CvgpSpriteManifest.ART_ROOT + paths[i] + ".png";
            result[i] = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (result[i] == null)
                errors.Add(ctx + ": không tải được sprite " + p);
        }
        return result;
    }

    static Color32[] ToColors(int[] rgb)
    {
        var c = new Color32[rgb.Length / 3];
        for (int i = 0; i < c.Length; i++)
            c[i] = new Color32((byte)rgb[3 * i], (byte)rgb[3 * i + 1], (byte)rgb[3 * i + 2], 255);
        return c;
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
