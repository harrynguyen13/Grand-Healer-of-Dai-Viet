using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiagnosisUIController : MonoBehaviour
{
    [Header("Root UI")]
    [SerializeField] private GameObject diagnosisPanel;

    [Header("Text hiển thị")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text symptomText;

    [Header("Loading bắt mạch")]
    [SerializeField] private GameObject pulseLoadingObject;
    [SerializeField] private TMP_Text pulseLoadingText;
    [SerializeField] private RectTransform pulseSpinnerIcon;
    [SerializeField] private float spinnerRotateSpeed = 180f;

    [Header("Nút chọn bệnh")]
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text[] optionButtonTexts;

    [Header("Thời gian")]
    [SerializeField] private float typeSpeed = 0.035f;
    [SerializeField] private float pulseLoadingTime = 2f;

    private PatientCase currentPatientCase;
    private MedicalDatabase medicalDatabase;
    private int clinicLevel;
    private Action<DiseaseData> onDiseaseSelected;

    private Coroutine diagnosisCoroutine;

    private void Awake()
    {
        Hide();
    }

    private void Update()
    {
        RotatePulseIcon();
    }

    public void Show(
        PatientCase patientCase,
        MedicalDatabase database,
        int currentClinicLevel,
        Action<DiseaseData> selectedCallback
    )
    {
        currentPatientCase = patientCase;
        medicalDatabase = database;
        clinicLevel = currentClinicLevel;
        onDiseaseSelected = selectedCallback;

        if (diagnosisPanel != null)
            diagnosisPanel.SetActive(true);

        if (diagnosisCoroutine != null)
        {
            StopCoroutine(diagnosisCoroutine);
            diagnosisCoroutine = null;
        }

        ClearUI();

        diagnosisCoroutine = StartCoroutine(DiagnosisFlow());
    }

    public void Hide()
    {
        if (diagnosisCoroutine != null)
        {
            StopCoroutine(diagnosisCoroutine);
            diagnosisCoroutine = null;
        }

        ClearUI();

        if (diagnosisPanel != null)
            diagnosisPanel.SetActive(false);
    }

    private void ClearUI()
    {
        if (dialogueText != null)
            dialogueText.text = "";

        if (symptomText != null)
            symptomText.text = "";

        if (pulseLoadingObject != null)
            pulseLoadingObject.SetActive(false);

        SetOptionButtonsActive(false);
    }

    private IEnumerator DiagnosisFlow()
    {
        if (currentPatientCase == null || currentPatientCase.realDisease == null)
        {
            Debug.LogError("Không có ca bệnh để hiện UI chẩn đoán.");
            yield break;
        }

        DiseaseData disease = currentPatientCase.realDisease;

        currentPatientCase.hasAsked = true;

        // 1. Chạy lời kể bệnh nhân trước
        yield return StartCoroutine(TypeDialogue(disease.patientDialogue));

        yield return new WaitForSeconds(typeSpeed * 8f);

        // 2. Hiện triệu chứng hỏi bệnh
        ShowAskSymptoms(disease);

        yield return new WaitForSeconds(0.5f);

        // 3. Sau đó mới hiện loading bắt mạch + icon xoay
        yield return StartCoroutine(ShowPulseLoading());

        currentPatientCase.hasPulseChecked = true;

        // 4. Hiện triệu chứng sau bắt mạch
        ShowPulseSymptoms(disease);

        yield return new WaitForSeconds(0.3f);

        // 5. Cuối cùng mới hiện 4 lựa chọn bệnh
        ShowDiseaseOptions(disease);
    }

    private IEnumerator TypeDialogue(string content)
    {
        if (dialogueText == null)
            yield break;

        dialogueText.text = "";

        if (string.IsNullOrEmpty(content))
            yield break;

        for (int i = 0; i < content.Length; i++)
        {
            dialogueText.text += content[i];
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private void ShowAskSymptoms(DiseaseData disease)
    {
        if (symptomText == null || disease == null)
            return;

        symptomText.text = "<b>Triệu chứng hỏi bệnh:</b>\n";

        foreach (SymptomData symptom in disease.symptoms)
        {
            if (symptom == null)
                continue;

            if (symptom.showAtStep == ExaminationStep.Ask)
            {
                symptomText.text += "- " + symptom.symptomText + "\n";
            }
        }
    }

    private IEnumerator ShowPulseLoading()
    {
        if (pulseLoadingObject != null)
            pulseLoadingObject.SetActive(true);

        if (pulseLoadingText != null)
            pulseLoadingText.text = "Đang bắt mạch...";

        yield return new WaitForSeconds(pulseLoadingTime);

        if (pulseLoadingObject != null)
            pulseLoadingObject.SetActive(false);
    }

    private void ShowPulseSymptoms(DiseaseData disease)
    {
        if (symptomText == null || disease == null)
            return;

        symptomText.text += "\n<b>Triệu chứng sau bắt mạch:</b>\n";

        foreach (SymptomData symptom in disease.symptoms)
        {
            if (symptom == null)
                continue;

            if (symptom.showAtStep == ExaminationStep.PulseCheck)
            {
                symptomText.text += "- " + symptom.symptomText + "\n";
            }
        }
    }

    private void ShowDiseaseOptions(DiseaseData realDisease)
    {
        if (medicalDatabase == null)
        {
            Debug.LogError("Chưa có MedicalDatabase.");
            return;
        }

        if (optionButtons == null || optionButtons.Length == 0)
        {
            Debug.LogWarning("Chưa kéo Option Buttons vào DiagnosisUIController.");
            return;
        }

        List<DiseaseData> options = medicalDatabase.GetDiagnosisOptions(
            realDisease,
            optionButtons.Length,
            clinicLevel
        );

        SetOptionButtonsActive(false);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null)
                continue;

            if (i >= options.Count)
            {
                optionButtons[i].gameObject.SetActive(false);
                continue;
            }

            DiseaseData optionDisease = options[i];

            optionButtons[i].gameObject.SetActive(true);

            if (optionButtonTexts != null && i < optionButtonTexts.Length && optionButtonTexts[i] != null)
            {
                optionButtonTexts[i].text = optionDisease.diseaseName;
            }

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                SelectDisease(optionDisease);
            });
        }
    }

    private void SelectDisease(DiseaseData selectedDisease)
    {
        if (currentPatientCase == null || selectedDisease == null)
            return;

        currentPatientCase.selectedDisease = selectedDisease;

        Debug.Log("Player chọn bệnh: " + selectedDisease.diseaseName);
        Debug.Log("Bệnh thật: " + currentPatientCase.realDisease.diseaseName);

        Hide();

        onDiseaseSelected?.Invoke(selectedDisease);
    }

    private void SetOptionButtonsActive(bool active)
    {
        if (optionButtons == null)
            return;

        foreach (Button button in optionButtons)
        {
            if (button != null)
                button.gameObject.SetActive(active);
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