using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoSettings : MonoBehaviour
{
    private Vector2Int[] resolutions;
    private string[] qualitySettings;
    private bool isFullscreen;
    private int currentResolution;
    private int currentQualityLevel;

    private void Start()
    {
        qualitySettings = QualitySettings.names;
        isFullscreen = true;
        resolutions = new Vector2Int[4] { new Vector2Int(1920, 1080), new Vector2Int(1600, 900), new Vector2Int(1280, 720), new Vector2Int(800, 600) };
    }

    public void SetResolution(int option)
    {
        currentResolution = option;
        Screen.SetResolution(resolutions[option].x, resolutions[option].y, isFullscreen);
    }

    public void SetFullscreen(bool value)
    {
        isFullscreen = value;
        Screen.fullScreen = value;
    }

    public void SetQualityPreset(int option)
    {
        currentQualityLevel = option;
        if (qualitySettings.Length > option)
        {
            QualitySettings.SetQualityLevel(option);
        }
        
    }
}
