using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GovernmentSpecialDiagnosisUIController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text UI")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text symptomText;

    [Header("Hiệu ứng chữ lời kể")]
    [SerializeField] private float dialogueTypeSpeed = 0.035f;

    [Header("Loading bắt mạch")]
    [SerializeField] private GameObject pulseLoadingObject;
    [SerializeField] private TMP_Text pulseLoadingText;
    [SerializeField] private RectTransform pulseSpinnerIcon;
    [SerializeField] private float pulseLoadingTime = 2f;
    [SerializeField] private float spinnerRotateSpeed = 180f;

    [Header("Thông báo bệnh mới")]
    [SerializeField] private GameObject newDiseaseNoticeRoot;
    [SerializeField] private TMP_Text newDiseaseNoticeText;

    [TextArea(2, 4)]
    [SerializeField] private string newDiseaseNoticeMessage =
        "Đây là căn bệnh mới được phát hiện.\nMời Lương Y đặt tên.";

    [SerializeField] private float newDiseaseNoticeDuration = 3.5f;

    [Header("4 nút chọn tên bệnh")]
    [SerializeField] private Button[] diseaseButtons;
    [SerializeField] private float diseaseButtonAppearDelay = 0.25f;

    [Header("Nút đóng")]
    [SerializeField] private Button closeButton;

    private SpecialDiseaseCase currentSpecialCase;
    private Action<string> onDiseaseNameSelected;

    private Coroutine diagnosisRoutine;
    private Coroutine noticeRoutine;

    private int flowId = 0;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        Hide();
    }

    private void OnDisable()
    {
        StopRunningUI();
        ClearUI();
    }

    private void Update()
    {
        RotatePulseIcon();
    }

    public void Show(SpecialDiseaseCase specialCase, Action<string> onSelected)
    {
        if (specialCase == null)
        {
            Debug.LogError("GovernmentSpecialDiagnosisUIController: SpecialDiseaseCase null.");
            return;
        }

        if (!specialCase.CanChooseDiseaseName())
        {
            Debug.LogWarning("GovernmentSpecialDiagnosisUIController: Chưa đủ điều kiện chọn tên bệnh.");
            return;
        }

        StopRunningUI();

        currentSpecialCase = specialCase;
        onDiseaseNameSelected = onSelected;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        ClearUI();

        flowId++;
        int currentFlowId = flowId;

        diagnosisRoutine = StartCoroutine(SpecialDiagnosisFlow(currentFlowId));
    }

    public void Hide()
    {
        StopRunningUI();

        ClearUI();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void StopRunningUI()
    {
        flowId++;

        if (diagnosisRoutine != null)
        {
            StopCoroutine(diagnosisRoutine);
            diagnosisRoutine = null;
        }

        if (noticeRoutine != null)
        {
            StopCoroutine(noticeRoutine);
            noticeRoutine = null;
        }
    }

    private void ClearUI()
    {
        if (dialogueText != null)
            dialogueText.text = "";

        if (symptomText != null)
            symptomText.text = "";

        if (pulseLoadingObject != null)
            pulseLoadingObject.SetActive(false);

        if (newDiseaseNoticeRoot != null)
            newDiseaseNoticeRoot.SetActive(false);

        SetDiseaseButtonsActive(false);
    }

    private IEnumerator SpecialDiagnosisFlow(int currentFlowId)
    {
        DiseaseData disease = GetCurrentDisease();

        if (disease == null)
        {
            Debug.LogError("Không có bệnh đặc biệt để hiện UI chẩn đoán.");
            yield break;
        }

        yield return StartCoroutine(TypeDialogue(GetDialogueText(disease), currentFlowId));

        if (currentFlowId != flowId)
            yield break;

        yield return new WaitForSeconds(dialogueTypeSpeed * 8f);

        if (currentFlowId != flowId)
            yield break;

        ShowAskSymptoms(disease);

        yield return new WaitForSeconds(0.5f);

        if (currentFlowId != flowId)
            yield break;

        yield return StartCoroutine(ShowPulseLoading(currentFlowId));

        if (currentFlowId != flowId)
            yield break;

        ShowPulseSymptoms(disease);

        yield return new WaitForSeconds(0.3f);

        if (currentFlowId != flowId)
            yield break;

        yield return StartCoroutine(ShowDiseaseOptionsOneByOne(currentFlowId));

        if (currentFlowId != flowId)
            yield break;

        yield return new WaitForSeconds(0.3f);

        if (currentFlowId != flowId)
            yield break;

        noticeRoutine = StartCoroutine(ShowNewDiseaseNotice(currentFlowId));

        diagnosisRoutine = null;
    }

    private IEnumerator TypeDialogue(string content, int currentFlowId)
    {
        if (dialogueText == null)
            yield break;

        dialogueText.text = "";

        if (string.IsNullOrEmpty(content))
            yield break;

        for (int i = 0; i < content.Length; i++)
        {
            if (currentFlowId != flowId)
                yield break;

            dialogueText.text = content.Substring(0, i + 1);

            yield return new WaitForSeconds(dialogueTypeSpeed);
        }
    }

    private DiseaseData GetCurrentDisease()
    {
        if (currentSpecialCase == null)
            return null;

        return currentSpecialCase.SpecialDisease;
    }

    private string GetDialogueText(DiseaseData disease)
    {
        if (disease == null)
            return "Quan Huyện đang nguy kịch, cần được chẩn bệnh cẩn thận.";

        if (!string.IsNullOrWhiteSpace(disease.patientDialogue))
            return disease.patientDialogue.Trim();

        if (!string.IsNullOrWhiteSpace(disease.description))
            return disease.description.Trim();

        return "Quan Huyện đang nguy kịch, cần được chẩn bệnh cẩn thận.";
    }

    private void ShowAskSymptoms(DiseaseData disease)
    {
        if (symptomText == null || disease == null)
            return;

        symptomText.text = "<b>Triệu chứng hỏi bệnh:</b>\n";

        if (disease.symptoms == null || disease.symptoms.Count == 0)
        {
            symptomText.text += "- Chưa có dữ liệu triệu chứng.";
            return;
        }

        bool hasAskSymptom = false;

        foreach (SymptomData symptom in disease.symptoms)
        {
            if (symptom == null)
                continue;

            if (symptom.showAtStep == ExaminationStep.Ask)
            {
                symptomText.text += "- " + symptom.symptomText + "\n";
                hasAskSymptom = true;
            }
        }

        if (!hasAskSymptom)
            symptomText.text += "- Chưa có dữ liệu triệu chứng hỏi bệnh.";
    }

    private IEnumerator ShowPulseLoading(int currentFlowId)
    {
        if (currentFlowId != flowId)
            yield break;

        if (pulseLoadingObject != null)
            pulseLoadingObject.SetActive(true);

        if (pulseLoadingText != null)
            pulseLoadingText.text = "Đang bắt mạch...";

        float timer = 0f;

        while (timer < pulseLoadingTime)
        {
            if (currentFlowId != flowId)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        if (pulseLoadingObject != null)
            pulseLoadingObject.SetActive(false);
    }

    private void ShowPulseSymptoms(DiseaseData disease)
    {
        if (symptomText == null || disease == null)
            return;

        symptomText.text += "\n<b>Triệu chứng sau bắt mạch:</b>\n";

        if (disease.symptoms == null || disease.symptoms.Count == 0)
        {
            symptomText.text += "- Chưa có dữ liệu triệu chứng.";
            return;
        }

        bool hasPulseSymptom = false;

        foreach (SymptomData symptom in disease.symptoms)
        {
            if (symptom == null)
                continue;

            if (symptom.showAtStep == ExaminationStep.PulseCheck)
            {
                symptomText.text += "- " + symptom.symptomText + "\n";
                hasPulseSymptom = true;
            }
        }

        if (!hasPulseSymptom)
            symptomText.text += "- Chưa có dữ liệu triệu chứng sau bắt mạch.";
    }

    private IEnumerator ShowDiseaseOptionsOneByOne(int currentFlowId)
    {
        if (currentSpecialCase == null)
            yield break;

        if (diseaseButtons == null || diseaseButtons.Length == 0)
        {
            Debug.LogWarning("Chưa kéo Disease Buttons vào GovernmentSpecialDiagnosisUIController.");
            yield break;
        }

        string[] options = currentSpecialCase.DiseaseNameOptions;

        SetDiseaseButtonsActive(false);

        for (int i = 0; i < diseaseButtons.Length; i++)
        {
            if (currentFlowId != flowId)
                yield break;

            Button button = diseaseButtons[i];

            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();

            if (options == null || i >= options.Length || string.IsNullOrWhiteSpace(options[i]))
            {
                button.gameObject.SetActive(false);
                continue;
            }

            string diseaseName = options[i].Trim();

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.text = diseaseName;

            button.onClick.AddListener(() =>
            {
                SelectDiseaseName(diseaseName);
            });

            button.gameObject.SetActive(true);

            yield return new WaitForSeconds(diseaseButtonAppearDelay);
        }
    }

    private IEnumerator ShowNewDiseaseNotice(int currentFlowId)
    {
        if (newDiseaseNoticeRoot == null)
            yield break;

        if (currentFlowId != flowId)
            yield break;

        if (newDiseaseNoticeText != null)
            newDiseaseNoticeText.text = newDiseaseNoticeMessage;

        newDiseaseNoticeRoot.SetActive(true);

        float timer = 0f;

        while (timer < newDiseaseNoticeDuration)
        {
            if (currentFlowId != flowId)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        if (newDiseaseNoticeRoot != null)
            newDiseaseNoticeRoot.SetActive(false);

        noticeRoutine = null;
    }

    private void SelectDiseaseName(string diseaseName)
    {
        if (currentSpecialCase == null)
            return;

        currentSpecialCase.ChooseDiseaseName(diseaseName);

        Debug.Log("Đã chọn tên bệnh cho Quan Huyện: " + diseaseName);

        Hide();

        onDiseaseNameSelected?.Invoke(diseaseName);
    }

    private void SetDiseaseButtonsActive(bool active)
    {
        if (diseaseButtons == null)
            return;

        foreach (Button button in diseaseButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(active);
            }
        }
    }

    private void RotatePulseIcon()
    {
        if (pulseLoadingObject == null)
            return;

        if (!pulseLoadingObject.activeSelf)
            return;

        if (pulseSpinnerIcon == null)
            return;

        pulseSpinnerIcon.Rotate(0f, 0f, -spinnerRotateSpeed * Time.unscaledDeltaTime);
    }
}