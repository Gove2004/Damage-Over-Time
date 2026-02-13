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

    [Header("BGM Clips")]
    public List<AudioClip> titleBGMClips = new List<AudioClip>();
    public List<AudioClip> battleBGMClips = new List<AudioClip>();

    [Header("Settings")]
    [Range(0.1f, 3f)] public float masterVolume = 1f;

    private Dictionary<string, AudioClip> proceduralClips = new Dictionary<string, AudioClip>();
    private bool bgmLoaded = false;

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
            EnsureBGMLoaded();
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

    private void LoadBGMResources()
    {
        // Load Title BGM
        AudioClip t1 = Resources.Load<AudioClip>("Music/标题界面bgm1");
        AudioClip t2 = Resources.Load<AudioClip>("Music/标题界面bgm2");
        if (t1 != null) titleBGMClips.Add(t1);
        else Debug.LogError("Failed to load Music/标题界面bgm1");
        
        if (t2 != null) titleBGMClips.Add(t2);
        else Debug.LogError("Failed to load Music/标题界面bgm2");

        // Load Battle BGM
        AudioClip b1 = Resources.Load<AudioClip>("Music/战斗bgm1");
        AudioClip b2 = Resources.Load<AudioClip>("Music/战斗bgm2");
        if (b1 != null) battleBGMClips.Add(b1);
        else Debug.LogError("Failed to load Music/战斗bgm1");
        
        if (b2 != null) battleBGMClips.Add(b2);
        else Debug.LogError("Failed to load Music/战斗bgm2");

        Debug.Log($"[AudioManager] Loaded {titleBGMClips.Count} Title BGMs and {battleBGMClips.Count} Battle BGMs.");
    }

    public void PlayTitleBGM()
    {
        Debug.Log("[AudioManager] Request to play Title BGM");
        EnsureBGMLoaded();
        PlayRandomBGM(titleBGMClips);
    }

    public void PlayBattleBGM()
    {
        Debug.Log("[AudioManager] Request to play Battle BGM");
        EnsureBGMLoaded();
        PlayRandomBGM(battleBGMClips);
    }

    private void PlayRandomBGM(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0)
        {
            EnsureBGMLoaded();
            if (clips == null || clips.Count == 0)
            {
            Debug.LogWarning("[AudioManager] No BGM clips to play in the list.");
            return;
            }
        }

        // Apply volume
        musicSource.volume = masterVolume;
        musicSource.loop = true;
        
        int index = Random.Range(0, clips.Count);
        AudioClip clipToPlay = clips[index];

        if (musicSource.clip == clipToPlay && musicSource.isPlaying)
        {
            Debug.Log($"[AudioManager] Already playing {clipToPlay.name}");
            return; 
        }

        Debug.Log($"[AudioManager] Playing BGM: {clipToPlay.name}");
        musicSource.clip = clipToPlay;
        musicSource.Play();
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

    private void EnsureBGMLoaded()
    {
        if (bgmLoaded) return;
        titleBGMClips.Clear();
        battleBGMClips.Clear();
        LoadBGMResources();
        bgmLoaded = true;
    }
}
