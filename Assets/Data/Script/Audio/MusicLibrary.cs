using System;
using UnityEngine;

/// <summary>
/// Bảng tra eMusic -> clip + điểm lặp (asset Assets/Data/Audio/MusicLibrary.asset, dựng bởi menu CVGP/Build MusicLibrary từ
/// Tools/cvm2/music_loops.json — CVM2 03). Mẫu tính theo clip Unity (44.1 kHz; = mẫu ffmpeg + 2257 độ trễ bộ mã MP3).
/// AudioManager giữ tham chiếu serialized, không Resources.Load.
/// </summary>
[CreateAssetMenu(fileName = "MusicLibrary", menuName = "CVGP/MusicLibrary")]
public class MusicLibrary : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public eMusic id;
        public AudioClip clip;
        [Tooltip("Mẫu bắt đầu phát (bỏ độ trễ bộ mã MP3 Unity giữ lại).")]
        public int startSample;
        public bool loop;
        [Tooltip("Tới loopEnd ⇒ lùi về loopStart (mẫu). loopEnd 0 + loop ⇒ lặp cả clip.")]
        public int loopStart;
        public int loopEnd;
        [TextArea] public string source;
    }

    [SerializeField] private Entry[] entries = new Entry[0];

    public int Count => entries.Length;

    public bool TryGet(eMusic id, out Entry entry)
    {
        for (int i = 0; i < entries.Length; i++)
            if (entries[i].id == id)
            {
                entry = entries[i];
                return entry.clip != null;
            }
        entry = default(Entry);
        return false;
    }

#if UNITY_EDITOR
    public void SetEntries(Entry[] value)
    {
        entries = value;
    }
#endif
}
