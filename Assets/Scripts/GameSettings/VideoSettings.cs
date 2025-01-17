using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoSettings : MonoBehaviour
{
    public SettingsVariable settings;

    private void Start()
    {
        Screen.SetResolution(settings.resolutions[settings.currentResolution].x, settings.resolutions[settings.currentResolution].y, settings.isFullscreen);
        QualitySettings.SetQualityLevel(settings.currentQualityLevel);
    }

    public void SetResolution(int option)
    {
        settings.currentResolution = option;
        Screen.SetResolution(settings.resolutions[option].x, settings.resolutions[option].y, settings.isFullscreen);
    }

    public void SetFullscreen(bool value)
    {
        settings.isFullscreen = value;
        Screen.fullScreen = value;
    }

    public void SetQualityPreset(int option)
    {
        settings.currentQualityLevel = option;
        if (settings.qualitySettings.Length > option)
        {
            QualitySettings.SetQualityLevel(option);
        }
        
    }
}
