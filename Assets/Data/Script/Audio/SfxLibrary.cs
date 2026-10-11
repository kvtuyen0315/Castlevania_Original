using System;
using UnityEngine;

/// <summary>
/// Bảng tra eSfx -> AudioClip (asset Assets/Data/Audio/SfxLibrary.asset, dựng bởi menu CVGP/Build SfxLibrary).
/// AudioManager giữ tham chiếu serialized, không Resources.Load.
/// </summary>
[CreateAssetMenu(fileName = "SfxLibrary", menuName = "CVGP/SfxLibrary")]
public class SfxLibrary : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public eSfx id;
        public AudioClip clip;
    }

    [SerializeField] private Entry[] entries = new Entry[0];

    public int Count => entries.Length;

    public AudioClip Get(eSfx id)
    {
        for (int i = 0; i < entries.Length; i++)
            if (entries[i].id == id)
                return entries[i].clip;
        return null;
    }

#if UNITY_EDITOR
    public void SetEntries(Entry[] value)
    {
        entries = value;
    }
#endif
}
