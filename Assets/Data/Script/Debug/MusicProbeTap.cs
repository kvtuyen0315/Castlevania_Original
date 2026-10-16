#if UNITY_EDITOR
using System.IO;
using UnityEngine;

/// <summary>
/// CVM2 03 — ghi âm ĐẦU RA thật (gắn cạnh AudioListener, OnAudioFilterRead) vào WAV 16-bit để kiểm điểm lặp nhạc liền mạch
/// bằng dạng sóng (không khoảng lặng, không nhảy). Chỉ Editor; tự gỡ sau khi ghi đủ. MusicProbeTap.Begin(seconds, path).
/// </summary>
public class MusicProbeTap : MonoBehaviour
{
    private float[] buf;
    private int pos, channels, rate;
    private string path;
    private volatile bool full;

    public static MusicProbeTap Begin(float seconds, string wavPath)
    {
        var listener = Object.FindObjectOfType<AudioListener>();
        if (listener == null)
            return null;
        var t = listener.gameObject.AddComponent<MusicProbeTap>();
        t.rate = AudioSettings.outputSampleRate;
        t.channels = AudioSettings.speakerMode == AudioSpeakerMode.Mono ? 1 : 2;
        t.buf = new float[(int)(seconds * t.rate) * 8];
        t.path = wavPath;
        return t;
    }

    public bool Done => full;

    private void OnAudioFilterRead(float[] data, int ch)
    {
        if (full)
            return;
        channels = ch;
        int n = Mathf.Min(data.Length, buf.Length - pos);
        System.Array.Copy(data, 0, buf, pos, n);
        pos += n;
        if (pos >= buf.Length / 8 * channels)
            full = true;
    }

    private void Update()
    {
        if (!full || path == null)
            return;
        Write();
        path = null;
        Destroy(this);
    }

    private void Write()
    {
        int n = pos;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var w = new BinaryWriter(File.Create(path)))
        {
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + n * 2);
            w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            w.Write(16);
            w.Write((short)1);
            w.Write((short)channels);
            w.Write(rate);
            w.Write(rate * channels * 2);
            w.Write((short)(channels * 2));
            w.Write((short)16);
            w.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            w.Write(n * 2);
            for (int i = 0; i < n; i++)
                w.Write((short)Mathf.Clamp(Mathf.RoundToInt(buf[i] * 32767f), -32768, 32767));
        }
    }
}
#endif
