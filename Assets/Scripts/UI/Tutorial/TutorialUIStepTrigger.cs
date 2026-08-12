using UnityEngine;
using UnityEngine.UI;

public class TutorialUIStepTrigger : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private int stepIndex;

    [Header("Vị trí mũi tên")]
    [SerializeField] private Vector2 screenOffset =
        new Vector2(0f, 80f);

    [Header("Hướng mũi tên")]
    [SerializeField] private float rotationZ = -90f;

    [Header("Hoàn thành khi bấm UI")]
    [SerializeField] private bool completeOnClick = true;

    private Button button;
    private TutorialArrowUI arrowUI;

    private bool registered;

    private void Start()
    {
        button = GetComponent<Button>();

        arrowUI =
            FindAnyObjectByType<TutorialArrowUI>();

        if (button != null && completeOnClick)
        {
            button.onClick.AddListener(
                CompleteStep
            );
        }
    }

    private void Update()
    {
        if (FirstTimeTutorialManager.Instance == null)
            return;

        // Chưa tới step của UI này.
        if (FirstTimeTutorialManager.Instance.CurrentStep != stepIndex)
        {
            registered = false;
            return;
        }

        // Step này đã hiện rồi thì không gọi lại mỗi frame.
        if (registered)
            return;

        if (arrowUI == null)
        {
            arrowUI =
                FindAnyObjectByType<TutorialArrowUI>();

            if (arrowUI == null)
                return;
        }

        RectTransform rect =
            transform as RectTransform;

        if (rect == null)
            return;

        arrowUI.SetUITarget(
            rect,
            screenOffset,
            rotationZ
        );

        registered = true;

        Debug.Log(
            "Hiển thị UI Tutorial Step "
            + stepIndex
            + ": "
            + gameObject.name
        );
    }

    private void CompleteStep()
    {
        if (FirstTimeTutorialManager.Instance == null)
            return;

        if (FirstTimeTutorialManager.Instance.CurrentStep != stepIndex)
            return;

        FirstTimeTutorialManager.Instance.CompleteStep(
            stepIndex
        );

        registered = false;
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                CompleteStep
            );
        }
    }
}