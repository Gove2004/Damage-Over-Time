using GoveKits.Runtime.Storage;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void OnEnable()
    {
        if (bgmSlider != null)
        {
            bgmSlider.value = AudioCore.GetVolume(AudioChannel.BGM);
            bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioCore.GetVolume(AudioChannel.SFX);
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
    }

    private void OnDisable()
    {
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        }
    }

    private void OnBgmVolumeChanged(float value)
    {
        AudioCore.SetVolume(AudioChannel.BGM, value);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        AudioCore.SetVolume(AudioChannel.SFX, value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public static void LoadVolume()
    {
        AudioCore.SetVolume(AudioChannel.BGM, PlayerPrefs.GetFloat("BGMVolume", 1f));
        AudioCore.SetVolume(AudioChannel.SFX, PlayerPrefs.GetFloat("SFXVolume", 1f));
    }
}
