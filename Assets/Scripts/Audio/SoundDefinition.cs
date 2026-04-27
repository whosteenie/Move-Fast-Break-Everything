using UnityEngine;

[CreateAssetMenu(fileName = "SoundDefinition", menuName = "Audio/Sound Definition")]
public class SoundDefinition : ScriptableObject
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private SoundCategory category = SoundCategory.Sfx;
    [SerializeField, Range(0f, 1f)] private float baseVolume = 1f;
    [SerializeField, Min(1)] private int maxSimultaneousVoices = 8;
    [SerializeField, Min(0f)] private float minReplayDelay = 0f;

    public AudioClip Clip => clip;
    public SoundCategory Category => category;
    public float BaseVolume => baseVolume;
    public int MaxSimultaneousVoices => maxSimultaneousVoices;
    public float MinReplayDelay => minReplayDelay;
}
