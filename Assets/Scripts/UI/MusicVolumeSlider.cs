using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Music UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicPercentText;

    [Header("SFX UI")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxPercentText;

    [Header("Âm test SFX khi kéo thanh")]
    [SerializeField] private AudioSource previewSfxSource;
    [SerializeField] private AudioClip previewSfxClip;

    [Tooltip("Khoảng nghỉ giữa 2 lần phát test khi đang kéo slider.")]
    [SerializeField] private float previewCooldown = 0.18f;

    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private float lastPreviewTime;
    private bool isInitializing;

    private void Start()
    {
        SetupPreviewSource();

        float savedMusicValue = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.45f);
        float savedSfxValue = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.45f);

        isInitializing = true;

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(savedMusicValue);
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(savedSfxValue);
            sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        SetMusicVolume(savedMusicValue);
        SetSFXVolumeWithoutPreview(savedSfxValue);

        isInitializing = false;
    }

    private void SetupPreviewSource()
    {
        if (previewSfxSource == null)
        {
            previewSfxSource = GetComponent<AudioSource>();
        }

        if (previewSfxSource == null)
        {
            previewSfxSource = gameObject.AddComponent<AudioSource>();
        }

        previewSfxSource.playOnAwake = false;
        previewSfxSource.loop = false;
        previewSfxSource.spatialBlend = 0f;
    }

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (audioMixer != null)
        {
            float dbValue = VolumeToDb(value);
            audioMixer.SetFloat(MUSIC_VOLUME_PARAM, dbValue);
        }

        if (musicPercentText != null)
        {
            musicPercentText.text = Mathf.RoundToInt(value * 100f) + "%";
        }

        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp01(value);

        SetSFXVolumeWithoutPreview(value);

        if (!isInitializing)
        {
            PlayPreviewSFX(value);
        }
    }

    private void SetSFXVolumeWithoutPreview(float value)
    {
        value = Mathf.Clamp01(value);

        if (audioMixer != null)
        {
            float dbValue = VolumeToDb(value);
            audioMixer.SetFloat(SFX_VOLUME_PARAM, dbValue);
        }

        if (sfxPercentText != null)
        {
            sfxPercentText.text = Mathf.RoundToInt(value * 100f) + "%";
        }

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    private void PlayPreviewSFX(float sliderValue)
    {
        if (previewSfxSource == null || previewSfxClip == null)
            return;

        if (Time.unscaledTime - lastPreviewTime < previewCooldown)
            return;

        lastPreviewTime = Time.unscaledTime;

        previewSfxSource.Stop();
        previewSfxSource.PlayOneShot(previewSfxClip, Mathf.Clamp01(sliderValue));
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