using UnityEngine;

public class TutorialStepTrigger : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private int stepIndex = 0;

    [Header("Vị trí mũi tên")]
    [SerializeField] private Vector2 screenOffset =
        new Vector2(0f, 80f);

    [Header("Hướng mũi tên")]
    [SerializeField] private float rotationZ = -90f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Hoàn thành khi Player chạm trigger")]
    [SerializeField] private bool completeOnPlayerEnter = true;

    [Header("Điều kiện Vườn thuốc")]
    [SerializeField] private HerbGardenUnlockZone gardenUnlockZone;

    [Header("Điều kiện nhiệm vụ")]
    [SerializeField] private string requiredActiveQuestId = "";

    private bool triggered;
    private bool registered;

    private void Start()
    {
        TryRegisterTarget();
    }

    private void Update()
    {
        if (registered)
            return;

        TryRegisterTarget();
    }

    private void TryRegisterTarget()
    {
        if (registered)
            return;

        if (gardenUnlockZone != null &&
            !gardenUnlockZone.IsUnlocked)
        {
            return;
        }

        if (!string.IsNullOrEmpty(requiredActiveQuestId))
        {
            if (QuestRuntimeManager.Instance == null)
                return;

            if (!QuestRuntimeManager.Instance.IsQuestActive(
                    requiredActiveQuestId))
            {
                return;
            }
        }

        if (FirstTimeTutorialManager.Instance == null)
            return;

        FirstTimeTutorialManager.Instance.RegisterStepTarget(
            stepIndex,
            transform,
            screenOffset,
            rotationZ
        );

        registered = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!completeOnPlayerEnter)
            return;

        if (triggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (FirstTimeTutorialManager.Instance == null)
            return;

        FirstTimeTutorialManager.Instance.CompleteStep(
            stepIndex
        );

        triggered = true;
    }
}