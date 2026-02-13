using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip drawClip;
    public AudioClip playClip;
    public AudioClip damageClip;
    public AudioClip healClip;
    public AudioClip manaClip;

    [Header("Settings")]
    [Range(0.1f, 3f)] public float masterVolume = 1f;

    private Dictionary<string, AudioClip> proceduralClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure AudioSources exist
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            
            RegisterClips();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to events
        EventCenter.Register("Player_DrawCard", (obj) => PlaySFX("Draw"));
        EventCenter.Register("Player_PlayCard", (obj) => PlaySFX("Play"));
        EventCenter.Register("CardDrawn", (obj) => PlaySFX("Draw")); // Enemy draw
        EventCenter.Register("CardPlayed", (obj) => PlaySFX("Play")); // Generic play
        
        // Damage/Heal events are handled via DamageEffectManager usually, but we can listen globally if we had a global event.
        // Or we can let DamageEffectManager call us.
        // But since we want to be decoupled, let's expose public methods and let DamageEffectManager call them, 
        // or hook into BattleManager if possible.
        // Actually, DamageEffectManager already listens to DamageTaken/HealTaken. 
        // We can hook there or let DamageEffectManager invoke audio.
        // For simplicity, let's expose PlayDamage and PlayHeal and modify DamageEffectManager to call them.
    }

    public void PlaySFX(string clipName)
    {
        if (proceduralClips.ContainsKey(clipName))
        {
            // Debug.Log($"Playing SFX: {clipName}");
            sfxSource.PlayOneShot(proceduralClips[clipName], masterVolume);
        }
        else
        {
            Debug.LogWarning($"Audio clip not found: {clipName}");
        }
    }

    public void RegisterClips()
    {
        Debug.Log($"[AudioManager] Registering Clips. Draw:{drawClip!=null}, Play:{playClip!=null}, Slash:{damageClip!=null}, Heal:{healClip!=null}, Mana:{manaClip!=null}");

        if (drawClip != null) proceduralClips["Draw"] = drawClip;
        if (playClip != null) proceduralClips["Play"] = playClip;
        if (damageClip != null) proceduralClips["Slash"] = damageClip; // Key matches "Slash" usage in DamageEffectManager
        if (healClip != null) proceduralClips["Heal"] = healClip;
        if (manaClip != null) proceduralClips["Mana"] = manaClip;
    }
}
