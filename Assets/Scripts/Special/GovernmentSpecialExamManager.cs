using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GovernmentSpecialExamManager : MonoBehaviour
{
    private const string OfficialQuestCompletedKey = "OfficialQuestCompleted";
    private const string OfficialQuestFailedKey = "OfficialQuestFailed";

    [Header("Ca bệnh đặc biệt")]
    [SerializeField] private SpecialDiseaseCase specialDiseaseCase;

    [Header("NPC Quan Huyện")]
    [SerializeField] private NpcAIController specialNpcAI;

    [Header("Vùng Player đứng để khám")]
    [SerializeField] private Collider2D playerExamPoint;

    [Header("Điểm Quan Huyện đứng khi khám")]
    [SerializeField] private Transform npcExamPoint;

    [Header("UI khám bệnh đặc biệt")]
    [SerializeField] private GovernmentSpecialDiagnosisUIController specialDiagnosisUIController;

    [Header("UI bốc thuốc đặc biệt")]
    [SerializeField] private GovernmentSpecialPrescriptionUIController specialPrescriptionUIController;

    [Header("Cấu hình tương tác")]
    [SerializeField] private Key interactKey = Key.F;

    [Header("Cấu hình đưa NPC về điểm khám")]
    [SerializeField] private float npcReturnSpeed = 2.5f;
    [SerializeField] private float npcArriveDistance = 0.05f;
    [SerializeField] private bool lockNpcAtExamPoint = true;

    private Transform player;
    private Rigidbody2D npcRb;
    private Animator npcAnimator;

    private bool isNpcReturningToExamPoint;
    private bool isExamining;

    private IEnumerator Start()
    {
        yield return null;
        yield return null;

        FindPlayer();

        if (specialDiseaseCase == null)
            specialDiseaseCase = FindAnyObjectByType<SpecialDiseaseCase>();

        if (specialDiseaseCase != null)
        {
            if (CanUnlockSpecialGovernmentQuest())
            {
                specialDiseaseCase.UnlockQuest();
            }
        }

        if (specialNpcAI == null && specialDiseaseCase != null)
            specialNpcAI = specialDiseaseCase.GetComponent<NpcAIController>();

        if (specialNpcAI != null)
        {
            npcRb = specialNpcAI.GetComponent<Rigidbody2D>();
            npcAnimator = specialNpcAI.GetComponent<Animator>();
        }

        if (specialDiagnosisUIController == null)
            specialDiagnosisUIController = FindAnyObjectByType<GovernmentSpecialDiagnosisUIController>();

        if (specialPrescriptionUIController == null)
            specialPrescriptionUIController = FindAnyObjectByType<GovernmentSpecialPrescriptionUIController>();

        SetupNpcBySpecialQuestState();
    }

    private void Update()
    {
        if (isExamining)
            return;

        if (!IsInteractPressed())
            return;

        if (specialDiseaseCase == null)
        {
            Debug.LogWarning("Không khám được: Chưa gán SpecialDiseaseCase.");
            return;
        }

        if (!CanUnlockSpecialGovernmentQuest())
        {
            Debug.LogWarning("Không khám được: Player chưa đủ điều kiện mở nhiệm vụ Quan Huyện.");
            return;
        }

        if (!specialDiseaseCase.CanStartExam())
        {
            PrintCannotStartReason();
            return;
        }

        if (player == null)
            FindPlayer();

        if (player == null)
        {
            Debug.LogWarning("Không khám được: Không tìm thấy Player.");
            return;
        }

        if (!IsPlayerInsideExamPoint())
        {
            Debug.LogWarning("Không khám được: Player chưa đứng trong vùng PlayerExamPoint.");
            return;
        }

        StartCoroutine(StartSpecialExamRoutine());
    }

    private bool CanUnlockSpecialGovernmentQuest()
    {
        int currentStage = PlayerLevelService.GetCurrentStage();

        if (currentStage < 5)
        {
            Debug.Log(
                "Chưa mở nhiệm vụ Quan Huyện: Player chưa đạt Chương 5."
                + " | CurrentStage = " + currentStage
                + " | Rank = " + PlayerLevelService.GetCurrentRankName()
            );

            return false;
        }

        if (SpecialQuestMailBridge.Instance == null)
        {
            Debug.LogWarning("Chưa mở nhiệm vụ Quan Huyện: Không tìm thấy SpecialQuestMailBridge.");
            return false;
        }

        if (!SpecialQuestMailBridge.Instance.HasSentMail())
        {
            Debug.Log("Chưa mở nhiệm vụ Quan Huyện: Thư nhiệm vụ chưa được gửi.");
            return false;
        }

        return true;
    }

    private void PrintCannotStartReason()
    {
        if (specialDiseaseCase == null)
            return;

        if (PlayerLevelService.GetCurrentStage() < 5)
        {
            Debug.LogWarning(
                "Không khám được: Player chưa đạt Chương 5."
                + " | CurrentStage = " + PlayerLevelService.GetCurrentStage()
                + " | Rank = " + PlayerLevelService.GetCurrentRankName()
            );
            return;
        }

        if (!specialDiseaseCase.QuestUnlocked)
        {
            Debug.LogWarning("Không khám được: Chưa mở nhiệm vụ Quan Huyện.");
            return;
        }

        if (specialDiseaseCase.SpecialDisease == null)
        {
            Debug.LogWarning("Không khám được: Chưa gán DiseaseData bệnh đặc biệt.");
            return;
        }

        if (specialDiseaseCase.IsCured)
        {
            Debug.Log("Quan Huyện đã khỏi bệnh. Nhiệm vụ Quan Huyện đã hoàn thành.");
            return;
        }

        if (specialDiseaseCase.IsFailed)
        {
            Debug.LogWarning("Nhiệm vụ Quan Huyện đã thất bại. Không thể khám hoặc bốc thuốc tiếp.");
            return;
        }

        Debug.LogWarning(
            "Không khám được: CanStartExam = false"
            + " | QuestUnlocked = " + specialDiseaseCase.QuestUnlocked
            + " | Disease = " + (specialDiseaseCase.SpecialDisease != null)
            + " | IsCured = " + specialDiseaseCase.IsCured
            + " | IsFailed = " + specialDiseaseCase.IsFailed
            + " | RemainingAttempts = " + specialDiseaseCase.RemainingAttempts
        );
    }

    private void SetupNpcBySpecialQuestState()
    {
        if (specialDiseaseCase == null)
        {
            Debug.LogWarning("GovernmentSpecialExamManager: Không có SpecialDiseaseCase, Quan Huyện được đi lại tự do.");

            SetNpcFree();
            return;
        }

        if (PlayerLevelService.GetCurrentStage() < 5)
        {
            Debug.Log(
                "Player chưa đạt Chương 5, Quan Huyện đi lại tự do."
                + " | CurrentStage = " + PlayerLevelService.GetCurrentStage()
                + " | Rank = " + PlayerLevelService.GetCurrentRankName()
            );

            SetNpcFree();
            return;
        }

        if (!CanUnlockSpecialGovernmentQuest())
        {
            Debug.Log("Nhiệm vụ Quan Huyện chưa được kích hoạt, Quan Huyện đi lại tự do.");
            SetNpcFree();
            return;
        }

        if (!specialDiseaseCase.CanStartExam())
        {
            Debug.Log(
                "Nhiệm vụ Quan Huyện chưa cần khóa NPC."
                + " | QuestUnlocked = " + specialDiseaseCase.QuestUnlocked
                + " | IsCured = " + specialDiseaseCase.IsCured
                + " | IsFailed = " + specialDiseaseCase.IsFailed
                + " | Quan Huyện được đi lại tự do."
            );

            SetNpcFree();
            return;
        }

        StartReturnNpcToExamPoint();
    }

    private void SetNpcFree()
    {
        if (specialNpcAI != null)
            specialNpcAI.SetBusy(false);

        StopNpcPhysics();

        isNpcReturningToExamPoint = false;
    }

    private bool IsInteractPressed()
    {
        if (Keyboard.current == null)
            return false;

        return Keyboard.current[interactKey].wasPressedThisFrame;
    }

    private bool IsPlayerInsideExamPoint()
    {
        if (playerExamPoint == null)
        {
            Debug.LogWarning("GovernmentSpecialExamManager: Chưa gán PlayerExamPoint.");
            return false;
        }

        if (player == null)
            return false;

        Collider2D playerCollider = player.GetComponent<Collider2D>();

        if (playerCollider == null)
        {
            Debug.LogWarning("GovernmentSpecialExamManager: Player không có Collider2D.");
            return false;
        }

        return playerExamPoint.bounds.Intersects(playerCollider.bounds);
    }

    private void FixedUpdate()
    {
        if (isNpcReturningToExamPoint)
            MoveNpcToExamPoint();
    }

    private void StartReturnNpcToExamPoint()
    {
        if (specialNpcAI == null)
        {
            Debug.LogWarning("GovernmentSpecialExamManager: Chưa gán Special NPC AI.");
            return;
        }

        if (npcExamPoint == null)
        {
            Debug.LogWarning("GovernmentSpecialExamManager: Chưa gán NpcExamPoint.");
            return;
        }

        specialNpcAI.SetBusy(true);
        specialNpcAI.ForceStopMovement();

        StopNpcPhysics();

        isNpcReturningToExamPoint = true;

        Debug.Log("GovernmentSpecialExamManager: Đang đưa Quan Huyện về điểm khám.");
    }

    private void MoveNpcToExamPoint()
    {
        if (specialNpcAI == null || npcExamPoint == null)
        {
            isNpcReturningToExamPoint = false;
            return;
        }

        Transform npcTransform = specialNpcAI.transform;

        Vector2 currentPosition = npcRb != null
            ? npcRb.position
            : (Vector2)npcTransform.position;

        Vector2 targetPosition = npcExamPoint.position;

        Vector2 direction = targetPosition - currentPosition;
        float distance = direction.magnitude;

        if (distance <= npcArriveDistance)
        {
            SnapNpcToExamPoint();
            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            npcReturnSpeed * Time.fixedDeltaTime
        );

        if (npcRb != null)
            npcRb.MovePosition(nextPosition);
        else
            npcTransform.position = nextPosition;

        UpdateNpcMoveAnimation(direction.normalized);
    }

    private void SnapNpcToExamPoint()
    {
        isNpcReturningToExamPoint = false;

        if (npcRb != null)
            npcRb.position = npcExamPoint.position;
        else if (specialNpcAI != null)
            specialNpcAI.transform.position = npcExamPoint.position;

        StopNpcPhysics();
        UpdateNpcIdleAnimation(Vector2.down);

        if (specialNpcAI != null)
        {
            if (lockNpcAtExamPoint)
            {
                specialNpcAI.SetBusy(true);
                specialNpcAI.ForceStopMovement();
            }
            else
            {
                specialNpcAI.SetBusy(false);
            }
        }

        Debug.Log("GovernmentSpecialExamManager: Quan Huyện đã về đúng điểm khám.");
    }

    private IEnumerator StartSpecialExamRoutine()
    {
        isExamining = true;

        StopPlayerMovementOnly();

        if (specialNpcAI != null && player != null)
        {
            specialNpcAI.SetBusy(true);
            specialNpcAI.ForceStopMovement();
            specialNpcAI.FaceTarget(player.position);
        }

        yield return null;

        StartSpecialCaseUI();

        isExamining = false;
    }

    private void StartSpecialCaseUI()
    {
        if (specialDiseaseCase == null)
        {
            Debug.LogError("GovernmentSpecialExamManager: Chưa kéo SpecialDiseaseCase.");
            return;
        }

        if (!CanUnlockSpecialGovernmentQuest())
        {
            Debug.LogWarning("GovernmentSpecialExamManager: Chưa đủ điều kiện mở nhiệm vụ Quan Huyện.");
            return;
        }

        if (!specialDiseaseCase.CanStartExam())
        {
            PrintCannotStartReason();
            return;
        }

        if (specialDiseaseCase.HasExamined &&
            specialDiseaseCase.HasChosenDiseaseName &&
            specialDiseaseCase.HasAddedToYThu)
        {
            StartSpecialPrescriptionUI();
            return;
        }

        specialDiseaseCase.MarkExamined();
        StartSpecialDiagnosisUI();
    }

    private void StartSpecialDiagnosisUI()
    {
        if (specialDiseaseCase == null)
        {
            Debug.LogError("GovernmentSpecialExamManager: Chưa có SpecialDiseaseCase.");
            return;
        }

        if (!specialDiseaseCase.CanChooseDiseaseName())
        {
            Debug.LogWarning(
                "Chưa thể mở UI chọn tên bệnh."
                + " | HasExamined = " + specialDiseaseCase.HasExamined
                + " | HasChosenDiseaseName = " + specialDiseaseCase.HasChosenDiseaseName
                + " | HasAddedToYThu = " + specialDiseaseCase.HasAddedToYThu
                + " | IsCured = " + specialDiseaseCase.IsCured
                + " | IsFailed = " + specialDiseaseCase.IsFailed
            );

            return;
        }

        if (specialDiagnosisUIController == null)
        {
            Debug.LogError("GovernmentSpecialExamManager: Chưa kéo GovernmentSpecialDiagnosisUIController.");
            return;
        }

        specialDiagnosisUIController.Show(
            specialDiseaseCase,
            OnSpecialDiseaseNameSelected
        );

        Debug.Log("Đã mở UI khám bệnh đặc biệt cho Quan Huyện.");
    }

    private void StartSpecialPrescriptionUI()
    {
        if (specialDiseaseCase == null)
        {
            Debug.LogError("GovernmentSpecialExamManager: Chưa có SpecialDiseaseCase.");
            return;
        }

        if (!CanUnlockSpecialGovernmentQuest())
        {
            Debug.LogWarning("Chưa thể bốc thuốc: nhiệm vụ Quan Huyện chưa đủ điều kiện kích hoạt.");
            return;
        }

        if (!specialDiseaseCase.CanTryTreatment())
        {
            Debug.LogWarning(
                "Chưa thể bốc thuốc cho Quan Huyện."
                + " | HasExamined = " + specialDiseaseCase.HasExamined
                + " | HasChosenDiseaseName = " + specialDiseaseCase.HasChosenDiseaseName
                + " | HasAddedToYThu = " + specialDiseaseCase.HasAddedToYThu
                + " | RemainingAttempts = " + specialDiseaseCase.RemainingAttempts
                + " | IsCured = " + specialDiseaseCase.IsCured
                + " | IsFailed = " + specialDiseaseCase.IsFailed
            );

            return;
        }

        if (specialPrescriptionUIController == null)
        {
            Debug.LogError("Chưa kéo GovernmentSpecialPrescriptionUIController.");
            return;
        }

        specialPrescriptionUIController.Show(
            specialDiseaseCase,
            OnSpecialTreatmentFinished
        );

        Debug.Log("Đã mở UI bốc thuốc đặc biệt cho Quan Huyện.");
    }

    private void OnSpecialDiseaseNameSelected(string selectedDiseaseName)
    {
        Debug.Log("Người chơi chọn tên bệnh cho Quan Huyện: " + selectedDiseaseName);

        StartSpecialPrescriptionUI();
    }

    private void OnSpecialTreatmentFinished(SpecialPrescriptionEvaluationResult result)
    {
        if (result == null)
            return;

        if (specialDiseaseCase == null)
            return;

        if (result.isCorrect)
        {
            HandleTreatmentSuccess(result);
            return;
        }

        if (specialDiseaseCase.IsFailed || specialDiseaseCase.RemainingAttempts <= 0)
        {
            HandleTreatmentFailed(result);
            return;
        }

        HandleTreatmentWrongButCanRetry(result);
    }

    private void HandleTreatmentSuccess(SpecialPrescriptionEvaluationResult result)
    {
        Debug.Log(
            "Quan Huyện đã được chữa khỏi bằng đơn thuốc đặc biệt."
            + " | Message = " + result.message
        );

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.CompleteOfficialQuest();
        }
        else
        {
            PlayerPrefs.SetInt(OfficialQuestCompletedKey, 1);
            PlayerPrefs.SetInt(OfficialQuestFailedKey, 0);
            PlayerPrefs.Save();
        }

        Debug.Log("Đã đánh dấu nhiệm vụ Quan Huyện hoàn thành. Điều kiện Truyền Nhân Y Đạo đã mở.");
    }

    private void HandleTreatmentWrongButCanRetry(SpecialPrescriptionEvaluationResult result)
    {
        int attempt = specialDiseaseCase.TreatmentAttemptCount;
        int remaining = specialDiseaseCase.RemainingAttempts;

        SendWrongTreatmentMail(attempt, remaining);

        Debug.LogWarning(
            "Đơn thuốc chưa phù hợp."
            + " | Lý do: " + result.message
            + " | Lần sai = " + attempt
            + " | Còn " + remaining + " lần thử."
        );
    }

    private void HandleTreatmentFailed(SpecialPrescriptionEvaluationResult result)
    {
        int attempt = specialDiseaseCase.TreatmentAttemptCount;

        SendFailedTreatmentMail(attempt);

        Debug.LogWarning(
            "Nhiệm vụ Quan Huyện thất bại."
            + " | Lý do đơn cuối: " + result.message
            + " | Player sẽ ở lại cấp Lương Y Đại Việt."
        );

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.FailOfficialQuest();
        }
        else
        {
            PlayerPrefs.SetInt(OfficialQuestFailedKey, 1);
            PlayerPrefs.SetInt(OfficialQuestCompletedKey, 0);
            PlayerPrefs.Save();
        }
    }

    private void SendWrongTreatmentMail(int attempt, int remaining)
    {
        if (MailboxManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy MailboxManager để gửi thư Quan Huyện.");
            return;
        }

        string content = "";

        if (attempt <= 1)
        {
            content =
                "Lương y,\n\n" +
                "Sau khi dùng đơn thuốc vừa rồi, bệnh tình của ta vẫn chưa có chuyển biến rõ rệt. " +
                "Hơi thở vẫn nặng, trong ngực còn đau tức, thân thể vẫn suy yếu.\n\n" +
                "Ta nghĩ ngươi nên nghiên cứu thêm về căn bệnh này trước khi kê đơn lần nữa.\n\n" +
                "Số lần còn lại: " + remaining;
        }
        else
        {
            content =
                "Lương y,\n\n" +
                "Bệnh của ta vẫn chưa thuyên giảm. Đơn thuốc lần này xem ra vẫn chưa chạm đúng căn nguyên của bệnh.\n\n" +
                "Ta sẽ cho ngươi thêm một cơ hội cuối cùng. Hãy suy xét thật cẩn trọng trước khi kê đơn.\n\n" +
                "Số lần còn lại: " + remaining;
        }

        MailboxManager.Instance.AddMail(
            MailType.PatientFail,
            "Quan Huyện",
            content,
            0,
            0
        );

        Debug.Log("Đã gửi thư Quan Huyện báo kê đơn chưa phù hợp. Lần sai: " + attempt);
    }

    private void SendFailedTreatmentMail(int attempt)
    {
        if (MailboxManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy MailboxManager để gửi thư thất bại Quan Huyện.");
            return;
        }

        string content =
            "Lương y,\n\n" +
            "Ta đã đặt niềm tin vào ngươi, nhưng qua nhiều lần dùng thuốc, bệnh tình vẫn không hề thuyên giảm. " +
            "Thân thể ta ngày một suy kiệt, lòng tin cũng chẳng còn như trước.\n\n" +
            "Ta quá thất vọng về ngươi. Có lẽ y thuật của ngươi vẫn chưa đủ để chữa căn bệnh này. " +
            "Hãy tiếp tục rèn luyện nhiều hơn trước khi gánh lấy những ca bệnh lớn như vậy.\n\n" +
            "Nhiệm vụ Quan Huyện đã thất bại.";

        MailboxManager.Instance.AddMail(
            MailType.PatientFail,
            "Quan Huyện",
            content,
            0,
            0
        );

        Debug.Log("Đã gửi thư thất bại nhiệm vụ Quan Huyện. Lần sai cuối: " + attempt);
    }

    private void StopPlayerMovementOnly()
    {
        if (player == null)
            return;

        BaseMove baseMove = player.GetComponent<BaseMove>();

        if (baseMove != null)
        {
            baseMove.StopImmediately();
            return;
        }

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
    }

    private void StopNpcPhysics()
    {
        if (npcRb == null)
            return;

        npcRb.linearVelocity = Vector2.zero;
        npcRb.angularVelocity = 0f;
    }

    private void UpdateNpcMoveAnimation(Vector2 direction)
    {
        if (npcAnimator == null)
            return;

        npcAnimator.SetBool("isMoving", true);
        npcAnimator.SetFloat("x", direction.x);
        npcAnimator.SetFloat("y", direction.y);
        npcAnimator.SetFloat("speed", npcReturnSpeed);
    }

    private void UpdateNpcIdleAnimation(Vector2 direction)
    {
        if (npcAnimator == null)
            return;

        npcAnimator.SetBool("isMoving", false);
        npcAnimator.SetFloat("x", direction.x);
        npcAnimator.SetFloat("y", direction.y);
        npcAnimator.SetFloat("speed", 0f);
    }

    private void FindPlayer()
    {
        if (PlayerSceneKeeper.Instance != null)
        {
            player = PlayerSceneKeeper.Instance.transform;
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void OnDisable()
    {
        if (specialNpcAI != null)
        {
            specialNpcAI.SetBusy(false);
        }

        isNpcReturningToExamPoint = false;
    }
}