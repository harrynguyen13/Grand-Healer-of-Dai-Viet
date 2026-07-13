using UnityEngine;

public class PlayerLevelUpWatcher : MonoBehaviour
{
    private const string LastKnownStageKey = "LastKnownPlayerStage";

    [Header("Cấu hình")]
    [SerializeField] private bool playSoundOnLevelUp = true;

    private int lastKnownStage;

    private void Start()
    {
        int currentStage = PlayerLevelService.GetCurrentStage();

        if (!PlayerPrefs.HasKey(LastKnownStageKey))
        {
            lastKnownStage = currentStage;
            SaveLastKnownStage();
            return;
        }

        lastKnownStage = PlayerPrefs.GetInt(LastKnownStageKey, currentStage);

        if (currentStage > lastKnownStage)
        {
            HandleLevelUp(currentStage);
        }
        else if (currentStage < lastKnownStage)
        {
            lastKnownStage = currentStage;
            SaveLastKnownStage();
        }
    }

    private void Update()
    {
        int currentStage = PlayerLevelService.GetCurrentStage();

        if (currentStage > lastKnownStage)
        {
            HandleLevelUp(currentStage);
            return;
        }

        if (currentStage < lastKnownStage)
        {
            lastKnownStage = currentStage;
            SaveLastKnownStage();
        }
    }

    private void HandleLevelUp(int newStage)
    {
        lastKnownStage = newStage;
        SaveLastKnownStage();

        if (playSoundOnLevelUp)
        {
            PlayLevelUpSound();
        }
    }

    private void PlayLevelUpSound()
    {
        if (LevelUpSoundPlayer.Instance == null)
            return;

        LevelUpSoundPlayer.Instance.PlayLevelUpSound();
    }

    private void SaveLastKnownStage()
    {
        PlayerPrefs.SetInt(LastKnownStageKey, lastKnownStage);
        PlayerPrefs.Save();
    }
}