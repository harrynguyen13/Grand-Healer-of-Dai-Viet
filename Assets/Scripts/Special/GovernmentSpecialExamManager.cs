using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class GovernmentSpecialExamManager : MonoBehaviour
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
        FindSceneReferences();
        TryUnlockSpecialQuest();
        SetupNpcBySpecialQuestState();
    }

    private void Update()
    {
        if (isExamining)
            return;

        if (!IsInteractPressed())
            return;

        if (!CanStartPlayerInteraction())
            return;

        StartCoroutine(StartSpecialExamRoutine());
    }

    private void FixedUpdate()
    {
        if (isNpcReturningToExamPoint)
            MoveNpcToExamPoint();
    }

    private void FindSceneReferences()
    {
        if (specialDiseaseCase == null)
            specialDiseaseCase = FindAnyObjectByType<SpecialDiseaseCase>();

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
    }

    private void TryUnlockSpecialQuest()
    {
        if (specialDiseaseCase == null)
            return;

        if (CanUnlockSpecialGovernmentQuest())
        {
            specialDiseaseCase.UnlockQuest();
        }
    }

    private bool CanStartPlayerInteraction()
    {
        if (specialDiseaseCase == null)
        {
            Debug.LogWarning("Không khám được: Chưa gán SpecialDiseaseCase.");
            return false;
        }

        if (!CanUnlockSpecialGovernmentQuest())
        {
            Debug.LogWarning("Không khám được: Player chưa đủ điều kiện mở nhiệm vụ Quan Huyện.");
            return false;
        }

        if (!specialDiseaseCase.CanStartExam())
        {
            PrintCannotStartReason();
            return false;
        }

        if (player == null)
            FindPlayer();

        if (player == null)
        {
            Debug.LogWarning("Không khám được: Không tìm thấy Player.");
            return false;
        }

        if (!IsPlayerInsideExamPoint())
        {
            Debug.LogWarning("Không khám được: Player chưa đứng trong vùng PlayerExamPoint.");
            return false;
        }

        return true;
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