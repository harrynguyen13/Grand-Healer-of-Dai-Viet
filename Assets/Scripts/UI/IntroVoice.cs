using UnityEngine;

public class IntroVoice : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource voiceAudioSource;

    [Header("Voice theo từng đoạn story")]
    [SerializeField] private AudioClip[] voiceClips;

    private void Awake()
    {
        if (voiceAudioSource == null)
        {
            voiceAudioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayVoiceByIndex(int index)
    {
        if (voiceAudioSource == null)
        {
            Debug.LogWarning("IntroVoice chưa có AudioSource.");
            return;
        }

        if (voiceClips == null || voiceClips.Length == 0)
        {
            Debug.LogWarning("IntroVoice chưa gán voiceClips.");
            return;
        }

        if (index < 0 || index >= voiceClips.Length)
        {
            Debug.LogWarning("Không có voice cho đoạn intro index: " + index);
            return;
        }

        voiceAudioSource.Stop();
        voiceAudioSource.clip = voiceClips[index];
        voiceAudioSource.Play();
    }

    public void StopVoice()
    {
        if (voiceAudioSource != null)
        {
            voiceAudioSource.Stop();
        }
    }
}