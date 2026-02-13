using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Procedural Settings")]
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
            
            GenerateProceduralSounds();
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

    private void GenerateProceduralSounds()
    {
        // 1. Draw Card (Slide/Paper sound) - White noise burst with filter
        proceduralClips["Draw"] = GenerateNoiseClip("Draw", 0.2f, true);

        // 2. Play Card (Place/Click) - Short punchy noise
        proceduralClips["Play"] = GenerateToneClip("Play", 400f, 0.15f, true);

        // 3. Slash (Damage) - Sawtooth/Noise sweep
        proceduralClips["Slash"] = GenerateSlashClip();

        // 4. Heal (Magic chime) - Sine wave arpeggio
        proceduralClips["Heal"] = GenerateHealClip();
    }

    private AudioClip GenerateNoiseClip(string name, float length, bool fadeOut)
    {
        int sampleRate = 44100;
        int samples = (int)(length * sampleRate);
        float[] data = new float[samples];
        
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float noise = Random.Range(-1f, 1f);
            float envelope = fadeOut ? 1f - t : 1f;
            data[i] = noise * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateToneClip(string name, float freq, float length, bool decay)
    {
        int sampleRate = 44100;
        int samples = (int)(length * sampleRate);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = decay ? 1f - ((float)i / samples) : 1f;
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateSlashClip()
    {
        int sampleRate = 44100;
        float length = 0.25f;
        int samples = (int)(length * sampleRate);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / samples;
            
            // Noise part (High frequency hiss)
            float noise = Random.Range(-1f, 1f);
            
            // Tone part (Low frequency sweep for impact)
            // 300Hz down to 50Hz
            float freq = Mathf.Lerp(300f, 50f, progress);
            float tone = Mathf.Sin(2 * Mathf.PI * freq * t);
            
            // Envelope: Sharp attack, exponential decay
            float envelope = Mathf.Exp(-5f * progress);
            
            // Mix
            float val = (noise * 0.6f + tone * 0.4f) * envelope;
            
            data[i] = val;
        }

        AudioClip clip = AudioClip.Create("Slash", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateHealClip()
    {
        int sampleRate = 44100;
        float length = 1.0f;
        int samples = (int)(length * sampleRate);
        float[] data = new float[samples];

        // Chord: C major (C, E, G) ascending
        float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f };
        float noteDur = length / freqs.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.FloorToInt(t / noteDur);
            if (noteIndex >= freqs.Length) noteIndex = freqs.Length - 1;
            
            float freq = freqs[noteIndex];
            
            // Sine wave + slight envelope per note
            float localT = t % noteDur;
            float envelope = 1f - (localT / noteDur); // Saw envelope
            
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.3f * envelope;
        }

        AudioClip clip = AudioClip.Create("Heal", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
