using UnityEditor;
using UnityEngine;

/// <summary>
/// Preset import nhạc cho Assets/Data/Audio/Music/** (CVM2 03): giữ stereo + sample rate gốc (điểm lặp tính theo mẫu 44.1 kHz),
/// Vorbis 70 %. Bài dài: Compressed In Memory (đặt timeSamples để lặp chính xác — Streaming nhảy vị trí không chuẩn);
/// jingle ngắn (≤ 7 s: m02 Prologue, m12 Stage Clear, m13 Game Complete, m15 Death, m16 Game Over): Decompress On Load.
/// Preload sẵn, không nạp nền (phát đúng tick gọi). Chỉ đặt lần đầu import (importSettingsMissing) — không đè chỉnh tay.
/// </summary>
public class MusicImporter : AssetPostprocessor
{
    public const string MUSIC_ROOT = "Assets/Data/Audio/Music/";
    static readonly string[] JINGLES = { "m02_", "m12_", "m13_", "m15_", "m16_" };

    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith(MUSIC_ROOT) || !assetImporter.importSettingsMissing)
            return;
        var importer = (AudioImporter)assetImporter;
        importer.forceToMono = false;
        importer.loadInBackground = false;
        var settings = importer.defaultSampleSettings;
        settings.preloadAudioData = true;
        settings.loadType = IsJingle(assetPath) ? AudioClipLoadType.DecompressOnLoad : AudioClipLoadType.CompressedInMemory;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.7f;
        settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        importer.defaultSampleSettings = settings;
    }

    static bool IsJingle(string path)
    {
        string file = System.IO.Path.GetFileName(path);
        foreach (var j in JINGLES)
            if (file.StartsWith(j))
                return true;
        return false;
    }
}
