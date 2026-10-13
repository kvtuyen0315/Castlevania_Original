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
    [SerializeField] private SfxLibrary sfxLibrary;

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

    /// <summary>Phát SFX theo tên (bảng SfxLibrary). Id không có clip ⇒ im lặng.</summary>
    public void PlaySfx(eSfx id)
    {
        if (sfxLibrary == null)
            return;
        var clip = sfxLibrary.Get(id);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
            CvtiDebug.Log("PlaySfx " + id);
        }
    }

    public bool IsSfxPlaying => sfxSource != null && sfxSource.isPlaying;

    /// <summary>CVGP 12 — Pause: tạm dừng / chạy tiếp nhạc (không phát lại từ đầu).</summary>
    public void PauseMusic(bool paused)
    {
        if (paused)
            musicSource.Pause();
        else
            musicSource.UnPause();
        CvtiDebug.Log(paused ? "PauseMusic" : "UnPauseMusic");
    }

    /// <summary>
    /// CVGP 12 — SFX kênh đơn: lần phát mới CẮT lần trước (NES: đếm tally kích lại mỗi ~8 tick, chỉ nghe đoạn đầu; lần cuối phát trọn —
    /// routine 02). Kênh riêng (không đụng PlayOneShot chồng của sfxSource).
    /// </summary>
    public void PlaySfxRestart(eSfx id)
    {
        if (sfxLibrary == null)
            return;
        var clip = sfxLibrary.Get(id);
        if (clip == null)
            return;
        if (sfxChannel == null)
            sfxChannel = CreateSource();
        sfxChannel.Stop();
        sfxChannel.clip = clip;
        sfxChannel.loop = false;
        sfxChannel.Play();
    }

    private AudioSource sfxChannel;

    AudioSource CreateSource()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        return src;
    }
}
