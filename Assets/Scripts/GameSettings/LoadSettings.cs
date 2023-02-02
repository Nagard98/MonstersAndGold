using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//On awake applies the settings
public class LoadSettings : MonoBehaviour
{
    public SettingsVariable settings;

    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    public Slider masterVolume;
    public Slider musicVolume;
    public Slider sfxVolume;

    public Toggle muteSfx;
    public Toggle muteMusic;

    private void Awake()
    {
        qualityDropdown.value = settings.currentQualityLevel;
        resolutionDropdown.value = settings.currentResolution;
        fullscreenToggle.isOn = settings.isFullscreen;

        masterVolume.value = settings.masterVolume;
        musicVolume.value = settings.musicVolume;
        sfxVolume.value = settings.sfxVolume;

        muteMusic.SetIsOnWithoutNotify(settings.isMusicMuted);
        muteSfx.SetIsOnWithoutNotify(settings.isSFXMuted);
    }
}
