using System.Collections.Generic;
using UnityEngine;

public enum SFXType
{
    None,

    PlayerShoot,
    EnemyShoot,

    EnemyHit,
    EnemyCritHit,
    EnemyDeath,

    PlayerDamaged,

    LevelUp,
    GameOver,

    ButtonClick
}

[System.Serializable]
public class SFXClipGroup
{
    public SFXType sfxType;

    [Tooltip("You can assign more than one clip. The AudioManager will randomly pick one.")]
    public AudioClip[] clips;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.25f, 3f)]
    public float pitchMin = 1f;

    [Range(0.25f, 3f)]
    public float pitchMax = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public static bool HasInstance
    {
        get { return Instance != null; }
    }

    [Header("Persistence")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Master SFX")]
    [Range(0f, 1f)]
    [SerializeField] private float masterSFXVolume = 1f;

    [Header("Audio Source Pool")]
    [SerializeField] private int startingAudioSources = 20;
    [SerializeField] private int maxAudioSources = 50;

    [Header("SFX Clips")]
    [SerializeField] private SFXClipGroup[] sfxClipGroups;

    private readonly List<AudioSource> audioSources = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        CreateStartingAudioSources();
    }

    public void PlaySFX(SFXType sfxType)
    {
        PlaySFX(sfxType, Vector3.zero, 1f);
    }

    public void PlaySFX(SFXType sfxType, Vector3 worldPosition)
    {
        PlaySFX(sfxType, worldPosition, 1f);
    }

    public void PlaySFX(SFXType sfxType, Vector3 worldPosition, float volumeMultiplier)
    {
        if (sfxType == SFXType.None)
            return;

        SFXClipGroup group = GetClipGroup(sfxType);

        if (group == null)
            return;

        if (group.clips == null || group.clips.Length == 0)
            return;

        AudioClip clip = GetRandomClip(group.clips);

        if (clip == null)
            return;

        AudioSource audioSource = GetAvailableAudioSource();

        if (audioSource == null)
            return;

        audioSource.transform.position = worldPosition;
        audioSource.clip = clip;
        audioSource.volume = Mathf.Clamp01(group.volume * masterSFXVolume * volumeMultiplier);
        audioSource.pitch = Random.Range(group.pitchMin, group.pitchMax);
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;

        audioSource.Play();
    }

    public void SetMasterSFXVolume(float value)
    {
        masterSFXVolume = Mathf.Clamp01(value);
    }

    public float GetMasterSFXVolume()
    {
        return masterSFXVolume;
    }

    private void CreateStartingAudioSources()
    {
        for (int i = 0; i < startingAudioSources; i++)
        {
            CreateAudioSource();
        }
    }

    private AudioSource CreateAudioSource()
    {
        if (audioSources.Count >= maxAudioSources)
            return null;

        GameObject sourceObject = new GameObject("Pooled Audio Source");
        sourceObject.transform.SetParent(transform);

        AudioSource audioSource = sourceObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        audioSources.Add(audioSource);

        return audioSource;
    }

    private AudioSource GetAvailableAudioSource()
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
                return audioSources[i];
        }

        return CreateAudioSource();
    }

    private SFXClipGroup GetClipGroup(SFXType sfxType)
    {
        for (int i = 0; i < sfxClipGroups.Length; i++)
        {
            if (sfxClipGroups[i].sfxType == sfxType)
                return sfxClipGroups[i];
        }

        return null;
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips.Length == 0)
            return null;

        int randomIndex = Random.Range(0, clips.Length);
        return clips[randomIndex];
    }
}