using System;
using afsdk;
using UnityEngine;

/// <summary>
/// 1 kênh nhạc + 1 kênh SFX (CVTI_MEMORY §4.3). Gọi PlayMusic trong cùng tick bật màn / sự kiện (OnEnter, hồi sinh, quả cầu…)
/// để nhạc khớp frame cắt cảnh. Title im lặng ⇒ Title gọi StopMusic.
/// CVM2 03: nhạc theo tên (eMusic → MusicLibrary: clip + mẫu bắt đầu + điểm lặp đo bằng tương quan dạng sóng). Lặp theo mẫu:
/// Update đọc timeSamples, vượt loopEnd ⇒ lùi đúng (loopEnd − loopStart) mẫu — đoạn sau loopEnd trong file = vòng 2 (cùng nội dung)
/// nên trễ ≤ 1 khung không nghe ra. Update ở đây là NGOẠI LỆ hợp lệ của H4 (chỉ âm thanh, không đụng trạng thái gameplay/hash —
/// CVM2_MEMORY §4).
/// </summary>
public class AudioManager : ManualSingletonMono<AudioManager>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private SfxLibrary sfxLibrary;
    [SerializeField] private MusicLibrary musicLibrary;

    /// <summary>Probe/log: (sự kiện, bài) — "play", "stop", "pause", "unpause", "loop" (lùi điểm lặp), "end" (bài không lặp hết).</summary>
    public static event Action<string, eMusic> MusicEvent;

    private eMusic curMusic;
    private int curLoopStart, curLoopEnd;
    private bool curWasPlaying;

    public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;
    public eMusic CurrentMusic => curMusic;
    public AudioSource MusicSource => musicSource;

    protected override void Awake()
    {
        base.Awake();
        if (musicSource == null)
            musicSource = CreateSource();
        if (sfxSource == null)
            sfxSource = CreateSource();
    }

    /// <summary>Phát bài theo tên từ đầu (dừng bài cũ). None / không có trong thư viện ⇒ chỉ dừng nhạc + log.</summary>
    public void PlayMusic(eMusic id)
    {
        musicSource.Stop();
        curMusic = eMusic.None;
        curLoopStart = curLoopEnd = 0;
        MusicLibrary.Entry e;
        if (id == eMusic.None || musicLibrary == null || !musicLibrary.TryGet(id, out e))
        {
            CvtiDebug.Log("PlayMusic " + id + ": không có clip — im lặng");
            Raise("stop", id);
            return;
        }
        musicSource.clip = e.clip;
        // tự lặp theo điểm đo (loopEnd > 0) ⇒ tắt loop của AudioSource; loopEnd 0 + loop ⇒ lặp cả clip
        musicSource.loop = e.loop && e.loopEnd <= 0;
        musicSource.timeSamples = Mathf.Clamp(e.startSample, 0, e.clip.samples - 1);
        musicSource.Play();
        curMusic = id;
        if (e.loop && e.loopEnd > e.loopStart)
        {
            curLoopStart = e.loopStart;
            curLoopEnd = e.loopEnd;
        }
        curWasPlaying = true;
        paused = false;
        CvtiDebug.Log("PlayMusic " + id + " (" + e.clip.name + (e.loop ? ", lặp " + e.loopStart + "–" + e.loopEnd : "") + ")");
        Raise("play", id);
    }

    /// <summary>Lặp theo mẫu + báo bài không lặp đã hết (âm thanh — không phải gameplay).</summary>
    private void Update()
    {
        if (musicSource == null || curMusic == eMusic.None)
            return;
        if (curLoopEnd > 0 && musicSource.isPlaying && musicSource.timeSamples >= curLoopEnd)
        {
            int before = musicSource.timeSamples;
            musicSource.timeSamples = before - (curLoopEnd - curLoopStart);
            CvtiDebug.Log("Music loop " + curMusic + " " + before + " → " + musicSource.timeSamples);
            Raise("loop", curMusic);
        }
        bool playing = musicSource.isPlaying;
        if (curWasPlaying && !playing && !paused && !musicSource.loop && curLoopEnd == 0)
            Raise("end", curMusic);
        curWasPlaying = playing;
    }

    private bool paused;

    private static void Raise(string what, eMusic id)
    {
        if (MusicEvent != null)
            MusicEvent(what, id);
    }

    /// <summary>Phát clip từ đầu (API CVTI cũ — dừng bài cũ). clip null ⇒ chỉ dừng nhạc.</summary>
    public void PlayMusic(AudioClip clip, bool loop)
    {
        musicSource.Stop();
        curMusic = eMusic.None;
        curLoopStart = curLoopEnd = 0;
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
        var was = curMusic;
        curMusic = eMusic.None;
        curLoopStart = curLoopEnd = 0;
        paused = false;
        Raise("stop", was);
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
        this.paused = paused;
        CvtiDebug.Log(paused ? "PauseMusic" : "UnPauseMusic");
        Raise(paused ? "pause" : "unpause", curMusic);
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
