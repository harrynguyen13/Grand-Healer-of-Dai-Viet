using UnityEngine;

public class LevelUpSoundPlayer : MonoBehaviour
{
    public static LevelUpSoundPlayer Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelUpClip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.8f;

    private void Awake()
    {
        Instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    public void PlayLevelUpSound()
    {
        if (audioSource == null || levelUpClip == null)
            return;

        audioSource.PlayOneShot(levelUpClip, volume);
    }
}