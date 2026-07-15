using UnityEngine;

public partial class GovernmentSpecialExamManager
{
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
}