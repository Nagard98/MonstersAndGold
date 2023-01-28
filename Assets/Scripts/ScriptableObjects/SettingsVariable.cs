using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SettingsVariable : ScriptableObject
{
    public Vector2Int[] resolutions;
    public string[] qualitySettings;
    public bool isFullscreen;
    [Range(0, 3)]public int currentResolution;
    [Range(0, 5)] public int currentQualityLevel;

    [Range(-80, 20)] public float masterVolume;
    [Range(-80, 20)] public float musicVolume;
    [Range(-80, 20)] public float sfxVolume;
    public bool isMusicMuted;
    public bool isSFXMuted;

    private void OnEnable()
    {
        qualitySettings = QualitySettings.names;
        currentResolution = Mathf.Clamp(currentResolution, 0, resolutions.Length);
        currentQualityLevel = Mathf.Clamp(currentQualityLevel, 0, qualitySettings.Length);
    }
}
