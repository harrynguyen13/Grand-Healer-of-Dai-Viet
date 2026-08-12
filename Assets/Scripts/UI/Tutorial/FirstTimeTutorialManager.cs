using System.Collections.Generic;
using UnityEngine;

public class FirstTimeTutorialManager : MonoBehaviour
{
    public static FirstTimeTutorialManager Instance { get; private set; }

    private const string TutorialStepKey =
        "FirstTimeTutorial_CurrentStep";

    private const string TutorialCompletedKey =
        "FirstTimeTutorial_Completed";

    [Header("Arrow")]
    [SerializeField] private TutorialArrowUI arrowUI;
    [SerializeField] private TutorialPathDots pathDots;

    private int currentStep;

    private readonly Dictionary<int, TutorialTargetData>
        registeredTargets =
            new Dictionary<int, TutorialTargetData>();

    public int CurrentStep
    {
        get { return currentStep; }
    }

    public bool IsTutorialCompleted
    {
        get
        {
            return PlayerPrefs.GetInt(
                TutorialCompletedKey,
                0
            ) == 1;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        currentStep =
            PlayerPrefs.GetInt(
                TutorialStepKey,
                0
            );

        if (arrowUI != null)
            arrowUI.HideArrow();
    }

    public void RegisterStepTarget(
        int stepIndex,
        Transform target,
        Vector2 screenOffset,
        float rotationZ
    )
    {
        if (IsTutorialCompleted)
            return;

        if (target == null)
            return;

        TutorialTargetData data =
            new TutorialTargetData(
                target,
                screenOffset,
                rotationZ
            );

        registeredTargets[stepIndex] = data;

        Debug.Log(
            "Đăng ký Tutorial Step "
            + stepIndex
            + ": "
            + target.name
        );

        if (stepIndex == currentStep)
        {
            ShowTarget(data);
        }
    }

    public void CompleteStep(int stepIndex)
    {
        if (IsTutorialCompleted)
            return;

        if (stepIndex != currentStep)
            return;

        Debug.Log(
            "Hoàn thành Tutorial Step "
            + stepIndex
        );

        currentStep++;

        PlayerPrefs.SetInt(
            TutorialStepKey,
            currentStep
        );

        PlayerPrefs.Save();

        if (arrowUI != null)
            arrowUI.HideArrow();
            
        if (pathDots != null)
            pathDots.ClearTarget();

        ShowCurrentRegisteredTarget();
    }

    private void ShowCurrentRegisteredTarget()
    {
        TutorialTargetData data;

        if (!registeredTargets.TryGetValue(
                currentStep,
                out data))
        {
            return;
        }

        if (data.target == null)
            return;

        ShowTarget(data);
    }

    private void ShowTarget(
        TutorialTargetData data
    )
    {
        if (arrowUI == null)
            return;

        if (data.target == null)
            return;

        arrowUI.SetTarget(
            data.target,
            data.screenOffset,
            data.rotationZ
        );

        if (pathDots != null)
        {
            pathDots.SetTarget(data.target);
        }

        Debug.Log(
            "Hiển thị Tutorial Step "
            + currentStep
            + ": "
            + data.target.name
        );
    }

    public void CompleteTutorial()
    {
        PlayerPrefs.SetInt(
            TutorialCompletedKey,
            1
        );

        PlayerPrefs.DeleteKey(
            TutorialStepKey
        );

        PlayerPrefs.Save();

        if (arrowUI != null)
            arrowUI.HideArrow();
        
        if (pathDots != null)
            pathDots.ClearTarget();
        

        registeredTargets.Clear();

        Debug.Log(
            "Đã hoàn thành toàn bộ First Time Tutorial."
        );
    }

    public static void ResetTutorialForNewGame()
    {
        PlayerPrefs.DeleteKey(
            TutorialStepKey
        );

        PlayerPrefs.DeleteKey(
            TutorialCompletedKey
        );

        PlayerPrefs.Save();

        if (Instance != null)
        {
            Instance.currentStep = 0;

            if (Instance.arrowUI != null)
                Instance.arrowUI.HideArrow();

            if (Instance.pathDots != null)
                Instance.pathDots.ClearTarget();

            Instance.registeredTargets.Clear();
        }

        Debug.Log(
            "Đã reset First Time Tutorial về Step 0."
        );
    }

    private class TutorialTargetData
    {
        public Transform target;
        public Vector2 screenOffset;
        public float rotationZ;

        public TutorialTargetData(
            Transform target,
            Vector2 screenOffset,
            float rotationZ
        )
        {
            this.target = target;
            this.screenOffset = screenOffset;
            this.rotationZ = rotationZ;
        }
    }
}