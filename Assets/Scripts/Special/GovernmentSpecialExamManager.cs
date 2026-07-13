using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GovernmentSpecialExamManager : MonoBehaviour
{
    [Header("Ca bệnh đặc biệt")]
    [SerializeField] private SpecialDiseaseCase specialDiseaseCase;

    [Header("NPC Quan Huyện")]
    [SerializeField] private NpcAIController specialNpcAI;

    [Header("Vùng Player đứng để khám")]
    [SerializeField] private Collider2D playerExamPoint;

    [Header("Điểm Quan Huyện đứng khi khám")]
    [SerializeField] private Transform npcExamPoint;

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
            if (SpecialQuestMailBridge.Instance != null && SpecialQuestMailBridge.Instance.HasSentMail())
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

        StartReturnNpcToExamPoint();
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

        if (!specialDiseaseCase.CanStartExam())
        {
            Debug.LogWarning(
                "Không khám được: CanStartExam = false"
                + " | QuestUnlocked = " + specialDiseaseCase.QuestUnlocked
                + " | Disease = " + (specialDiseaseCase.SpecialDisease != null)
                + " | IsCured = " + specialDiseaseCase.IsCured
            );

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

        Vector2 currentPosition = npcRb != null ? npcRb.position : (Vector2)npcTransform.position;
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

        if (specialNpcAI != null)
        {
            specialNpcAI.SetBusy(true);
            specialNpcAI.ForceStopMovement();
            specialNpcAI.FaceTarget(player.position);
        }

        yield return null;

        specialDiseaseCase.MarkExamined();

        Debug.Log("Đã bắt đầu khám bệnh đặc biệt.");
        PrintSpecialDiseaseInfo();

        isExamining = false;
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

    private void PrintSpecialDiseaseInfo()
    {
        DiseaseData disease = specialDiseaseCase.SpecialDisease;

        if (disease == null)
        {
            Debug.LogWarning("GovernmentSpecialExamManager: Bệnh đặc biệt chưa được gán.");
            return;
        }

        Debug.Log("===== TRIỆU CHỨNG BỆNH ĐẶC BIỆT =====");

        if (disease.symptoms != null)
        {
            for (int i = 0; i < disease.symptoms.Count; i++)
            {
                if (disease.symptoms[i] == null)
                    continue;

                Debug.Log("- " + disease.symptoms[i].symptomText);
            }
        }

        Debug.Log("===== 4 TÊN BỆNH ĐỂ NGƯỜI CHƠI CHỌN =====");

        string[] options = specialDiseaseCase.DiseaseNameOptions;

        if (options != null)
        {
            for (int i = 0; i < options.Length; i++)
            {
                Debug.Log((i + 1) + ". " + options[i]);
            }
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