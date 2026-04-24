using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    [Header("Runtime Volume")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

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
                sfxSource.PlayOneShot(sound.Clip, sound.BaseVolume * sfxVolume);
                break;
        }
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
        musicSource.playOnAwake = false;
        musicSource.loop = true;
    }
}
