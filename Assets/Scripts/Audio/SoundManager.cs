using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    private const string SfxVolumeKey = "Options.Audio.SfxVolume";
    private const string MusicVolumeKey = "Options.Audio.MusicVolume";

    private static SoundManager _instance;

    private sealed class ActiveSfxVoice
    {
        public SoundDefinition Sound;
        public AudioSource Source;
    }

    [Header("Runtime Volume")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    private readonly List<AudioSource> _sfxSourcePool = new();
    private readonly List<ActiveSfxVoice> _activeSfxVoices = new();
    private readonly Dictionary<SoundDefinition, float> _lastPlayTimes = new();
    private SoundDefinition _currentMusic;

    private static SoundManager Instance
    {
        get
        {
            if(_instance != null) return _instance;
            _instance = FindAnyObjectByType<SoundManager>();

            if(_instance != null) return _instance;
            var managerObject = new GameObject("SoundManager");
            _instance = managerObject.AddComponent<SoundManager>();

            return _instance;
        }
    }

    public float SfxVolume => sfxVolume;
    public float MusicVolume => musicVolume;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureAudioSourcesConfigured();

        LoadSavedVolumes();
        EnsureAudioSources();

        if (sfxSource != null && !_sfxSourcePool.Contains(sfxSource))
        {
            _sfxSourcePool.Add(sfxSource);
        }

        ApplyMusicVolume();
    }

    private void EnsureAudioSourcesConfigured()
    {
        var attachedSources = GetComponents<AudioSource>();

        if (sfxSource == null && attachedSources.Length > 0)
        {
            sfxSource = attachedSources[0];
        }

        if (musicSource == null)
        {
            if (attachedSources.Length > 1)
            {
                musicSource = attachedSources[1];
            }
            else if (sfxSource != null && attachedSources.Length == 1)
            {
                musicSource = CreateAttachedAudioSource("MusicSource");
            }
        }

        if (sfxSource == null)
        {
            sfxSource = CreateAttachedAudioSource("SfxSource");
        }

        if (musicSource == null)
        {
            musicSource = CreateAttachedAudioSource("MusicSource");
        }

        ConfigureSfxSource(sfxSource);
        ConfigureMusicSource(musicSource);
    }

    private AudioSource CreateAttachedAudioSource(string sourceName)
    {
        var sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform, false);
        return sourceObject.AddComponent<AudioSource>();
    }

    private static void ConfigureSfxSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
    }

    private static void ConfigureMusicSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
    }

    public static void Play(SoundDefinition sound)
    {
        Instance.PlayInternal(sound);
    }

    public static float GetSfxVolume()
    {
        return Instance.sfxVolume;
    }

    public static float GetMusicVolume()
    {
        return Instance.musicVolume;
    }

    public static void SetSfxVolume(float volume)
    {
        Instance.SetSfxVolumeInternal(volume, save: true);
    }

    public static void SetMusicVolume(float volume)
    {
        Instance.SetMusicVolumeInternal(volume, save: true);
    }

    private void SetSfxVolumeInternal(float volume, bool save)
    {
        sfxVolume = Mathf.Clamp01(volume);

        foreach (var voice in _activeSfxVoices)
        {
            if (voice.Source != null && voice.Sound != null)
            {
                voice.Source.volume = voice.Sound.BaseVolume * sfxVolume;
            }
        }

        if (save)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            PlayerPrefs.Save();
        }
    }

    private void SetMusicVolumeInternal(float volume, bool save)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();

        if (save)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
            PlayerPrefs.Save();
        }
    }

    private void PlayInternal(SoundDefinition sound)
    {
        if (sound == null || sound.Clip == null)
        {
            return;
        }

        switch (sound.Category)
        {
            case SoundCategory.Music:
                PlayMusic(sound);
                break;
            default:
                PlaySfx(sound);
                break;
        }
    }

    private void PlaySfx(SoundDefinition sound)
    {
        CleanupFinishedVoices();

        if (_lastPlayTimes.TryGetValue(sound, out var lastPlayTime))
        {
            if (Time.unscaledTime - lastPlayTime < sound.MinReplayDelay)
            {
                return;
            }
        }

        var activeVoiceCount = 0;
        foreach(var v in _activeSfxVoices) {
            if (v.Sound == sound)
            {
                activeVoiceCount++;
            }
        }

        if (activeVoiceCount >= sound.MaxSimultaneousVoices)
        {
            return;
        }

        var source = GetAvailableSfxSource();
        source.clip = sound.Clip;
        source.volume = sound.BaseVolume * sfxVolume;
        source.loop = false;
        source.Play();

        _lastPlayTimes[sound] = Time.unscaledTime;
        _activeSfxVoices.Add(new ActiveSfxVoice
        {
            Sound = sound,
            Source = source
        });
    }

    private void PlayMusic(SoundDefinition sound)
    {
        if (musicSource == null)
        {
            return;
        }

        if (_currentMusic == sound && musicSource.clip == sound.Clip && musicSource.isPlaying)
        {
            return;
        }

        _currentMusic = sound;
        musicSource.clip = sound.Clip;
        ApplyMusicVolume();
        musicSource.loop = true;
        musicSource.Play();
    }

    private void ApplyMusicVolume()
    {
        if (musicSource == null)
        {
            return;
        }

        var baseVolume = _currentMusic != null ? _currentMusic.BaseVolume : 1f;
        musicSource.volume = baseVolume * musicVolume;
    }

    private void LoadSavedVolumes()
    {
        SetSfxVolumeInternal(PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume), save: false);
        SetMusicVolumeInternal(PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume), save: false);
    }

    private void EnsureAudioSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
        }
    }

    private void CleanupFinishedVoices()
    {
        for (var i = _activeSfxVoices.Count - 1; i >= 0; i--)
        {
            var voice = _activeSfxVoices[i];
            if (voice.Source == null || voice.Source.isPlaying)
            {
                continue;
            }

            voice.Source.clip = null;
            _activeSfxVoices.RemoveAt(i);
        }
    }

    private AudioSource GetAvailableSfxSource()
    {
        CleanupFinishedVoices();

        foreach(var pooledSource in _sfxSourcePool) {
            if (pooledSource != null && !pooledSource.isPlaying)
            {
                return pooledSource;
            }
        }

        var sourceObject = new GameObject($"SfxSource_{_sfxSourcePool.Count}");
        sourceObject.transform.SetParent(transform, false);

        var newSource = sourceObject.AddComponent<AudioSource>();
        ConfigureSfxSource(newSource);

        _sfxSourcePool.Add(newSource);
        return newSource;
    }
}
