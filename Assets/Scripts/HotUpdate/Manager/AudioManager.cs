using System.Collections.Generic;
using GoveKits.Runtime.Core;
using GoveKits.Runtime.Storage;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

    public AudioClip MustGetAudioClip(string name)
    {
        if (!audioClips.ContainsKey(name))
        {
            AudioClip clip = ResCore.LoadAssetSync<AudioClip>(name).GetAssetObject<AudioClip>();
            if (clip != null)
            {
                audioClips[name] = clip;
                return clip;
            }
            else
            {
                LogCore.Error("AudioManager", $"Audio clip '{name}' not found!");
                return null;
            }
        }
        return audioClips[name];
    }


    public void PlaySound(string soundName)
    {
        AudioClip clip = MustGetAudioClip(soundName);
        if (clip != null)
        {
            AudioCore.PlayDynamic(AudioChannel.SFX, clip);
        }
    }


    public void PlayBGM(string bgmName)
    {
        AudioClip clip = MustGetAudioClip(bgmName);
        if (clip != null)
        {
            AudioCore.PlayBGM(clip);
        }
    }

    public UnityEngine.Events.UnityAction PlayOnClick(UnityEngine.Events.UnityAction callback)
    {
        return () =>
        {
            PlaySound("UI");
            callback();
        };
    }

}