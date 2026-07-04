using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text percentText;

    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    private void Start()
    {
        float savedValue = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.45f);

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(savedValue);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        SetMusicVolume(savedValue);
    }

    public void SetMusicVolume(float value)
    {
        if (audioMixer != null)
        {
            float dbValue = VolumeToDb(value);
            audioMixer.SetFloat(MUSIC_VOLUME_PARAM, dbValue);
        }

        if (percentText != null)
        {
            percentText.text = Mathf.RoundToInt(value * 100f) + "%";
        }

        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    private float VolumeToDb(float value)
    {
        if (value <= 0.0001f)
        {
            return -80f;
        }

        return Mathf.Log10(value) * 20f;
    }
}