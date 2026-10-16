#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Probe nhạc (CVM2 03) — CHỈ Editor, đang Play. Ghi mọi sự kiện AudioManager.MusicEvent (play/stop/pause/unpause/loop/end) kèm
/// NesClock.Frame tuyệt đối, màn hình, stage, clip, timeSamples (trước/sau), isPlaying; thêm 1 dòng mẫu mỗi 60 tick và cảnh báo
/// "đứng" khi đang phát mà timeSamples không tăng 10 tick liền. Chỉ đọc — không đổi trạng thái game (hash luồng không đổi).
///   MusicProbe.Start() → chạy luồng (vd RegressionRunner "flow") → MusicProbe.Stop(path).
/// </summary>
public static class MusicProbe
{
    public static readonly List<string> Lines = new List<string>();
    public static bool Running { get; private set; }

    private static int lastSamples, stallTicks;
    private static string lastClip;

    public static string Start()
    {
        Stop(null);
        Lines.Clear();
        lastSamples = 0;
        stallTicks = 0;
        lastClip = null;
        AudioManager.MusicEvent += OnEvent;
        NesClock.OnTick += OnTick;
        Running = true;
        Lines.Add("# frame\tsự kiện\tbài\tclip\ttimeSamples\ttrước\tisPlaying\tmàn\tstage");
        return "MusicProbe bắt đầu ở frame " + NesClock.Frame;
    }

    public static string Stop(string path)
    {
        if (Running)
        {
            AudioManager.MusicEvent -= OnEvent;
            NesClock.OnTick -= OnTick;
            Running = false;
        }
        if (string.IsNullOrEmpty(path))
            return "MusicProbe dừng (" + Lines.Count + " dòng)";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, string.Join("\n", Lines.ToArray()) + "\n", new UTF8Encoding(false));
        return path + " (" + Lines.Count + " dòng)";
    }

    private static AudioSource Src => AudioManager.I != null ? AudioManager.I.MusicSource : null;

    private static void OnEvent(string what, eMusic id)
    {
        Add(what, id);
        var s = Src;
        if (s != null)
            lastSamples = s.timeSamples;
    }

    private static void OnTick(int frame)
    {
        var s = Src;
        if (s == null)
            return;
        int ts = s.timeSamples;
        string clip = s.clip != null ? s.clip.name : "-";
        if (s.isPlaying && clip == lastClip && ts <= lastSamples)
        {
            if (++stallTicks == 10)
                Add("ĐỨNG 10 tick", AudioManager.I.CurrentMusic);
        }
        else
            stallTicks = 0;
        if (frame % 60 == 0)
            Add("mẫu", AudioManager.I.CurrentMusic);
        lastSamples = ts;
        lastClip = clip;
    }

    private static void Add(string what, eMusic id)
    {
        var s = Src;
        var gm = GameManager.I;
        var gs = GameSession.Current;
        Lines.Add(NesClock.Frame + "\t" + what + "\t" + id + "\t" + (s != null && s.clip != null ? s.clip.name : "-") + "\t"
                  + (s != null ? s.timeSamples : -1) + "\t" + lastSamples + "\t" + (s != null && s.isPlaying) + "\t"
                  + (gm != null ? gm.State.ToString() : "?") + "\t" + (gs != null ? gs.stage : 0));
    }
}
#endif
