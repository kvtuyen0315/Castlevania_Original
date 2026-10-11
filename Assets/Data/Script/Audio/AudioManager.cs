using afsdk;
using UnityEngine;

/// <summary>
/// 1 kênh nhạc + 1 kênh SFX (CVTI_MEMORY §4.3). Clip do màn hình giữ (serialized trên prefab màn), không Resources.Load.
/// Gọi PlayMusic trong cùng tick bật màn (OnEnter) để nhạc khớp frame cắt cảnh. Title im lặng ⇒ Title gọi StopMusic.
/// </summary>
public class AudioManager : ManualSingletonMono<AudioManager>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;

    protected override void Awake()
    {
        base.Awake();
        if (musicSource == null)
            musicSource = CreateSource();
        if (sfxSource == null)
            sfxSource = CreateSource();
    }

    /// <summary>Phát nhạc từ đầu (dừng bài cũ). clip null ⇒ chỉ dừng nhạc (chưa có file nhạc, CVTI_MEMORY §5 #3).</summary>
    public void PlayMusic(AudioClip clip, bool loop)
    {
        musicSource.Stop();
        if (clip == null)
        {
            CvtiDebug.Log("PlayMusic: chưa có clip — im lặng");
            return;
        }
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
        CvtiDebug.Log("PlayMusic " + clip.name + (loop ? " (loop)" : ""));
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            CvtiDebug.Log("StopMusic");
        musicSource.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    AudioSource CreateSource()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        return src;
    }
}
