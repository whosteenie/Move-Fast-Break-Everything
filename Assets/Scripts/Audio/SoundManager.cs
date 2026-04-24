using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
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
        EnsureAudioSources();
        ApplyMusicVolume();
    }

    public static void Play(SoundDefinition sound)
    {
        Instance.PlayInternal(sound);
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();
    }

    private void PlayInternal(SoundDefinition sound)
    {
        if (sound == null || sound.Clip == null)
        {
            return;
        }

        EnsureAudioSources();

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
        for (var i = 0; i < _activeSfxVoices.Count; i++)
        {
            if (_activeSfxVoices[i].Sound == sound)
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
        if (musicSource.clip == sound.Clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = sound.Clip;
        musicSource.volume = sound.BaseVolume * musicVolume;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void ApplyMusicVolume()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.volume = musicVolume;
    }

    private void EnsureAudioSources()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;

        if (!_sfxSourcePool.Contains(sfxSource))
        {
            _sfxSourcePool.Add(sfxSource);
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

        for (var i = 0; i < _sfxSourcePool.Count; i++)
        {
            var pooledSource = _sfxSourcePool[i];
            if (pooledSource != null && !pooledSource.isPlaying)
            {
                return pooledSource;
            }
        }

        var sourceObject = new GameObject($"SfxSource_{_sfxSourcePool.Count}");
        sourceObject.transform.SetParent(transform, false);

        var newSource = sourceObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.loop = false;
        newSource.spatialBlend = 0f;

        _sfxSourcePool.Add(newSource);
        return newSource;
    }
}
