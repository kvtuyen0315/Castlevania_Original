using UnityEditor;
using UnityEngine;

/// <summary>
/// Preset import SFX NES cho Assets/Data/Audio/SFX/** (CVGP_MEMORY §3): mono, PCM, Decompress On Load, giữ sample rate.
/// Chỉ đặt giá trị lần đầu import (importSettingsMissing) để không đè chỉnh tay về sau. AudioSource 2D sẵn có nên không cần cờ 3D.
/// </summary>
public class SfxImporter : AssetPostprocessor
{
    const string SFX_ROOT = "Assets/Data/Audio/SFX/";

    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith(SFX_ROOT) || !assetImporter.importSettingsMissing)
            return;

        var importer = (AudioImporter)assetImporter;
        importer.forceToMono = true;
        importer.loadInBackground = false;

        var settings = importer.defaultSampleSettings;
        settings.preloadAudioData = true;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.PCM;
        settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        importer.defaultSampleSettings = settings;
    }
}
