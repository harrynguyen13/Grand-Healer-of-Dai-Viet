using UnityEngine;

[RequireComponent(typeof(NpcAIController))]
public class NpcMarketBuyerController : MonoBehaviour
{
    [Header("Bong bóng thoại")]
    [SerializeField] private NpcSpeechBubble speechBubble;

    private NpcAIController movement;
    private bool isTalking;
    private float nextTalkTime;

    public bool CanTalk
    {
        get
        {
            if (isTalking)
                return false;

            if (Time.time < nextTalkTime)
                return false;

            if (movement != null && movement.IsBusy)
                return false;

            return true;
        }
    }

    private void Awake()
    {
        movement = GetComponent<NpcAIController>();

        if (speechBubble == null)
            speechBubble = GetComponent<NpcSpeechBubble>();

        HideBubble();
    }

    public void BeginTalk(float cooldown)
    {
        isTalking = true;
        nextTalkTime = Time.time + cooldown;

        if (movement != null)
        {
            movement.SetBusy(true);
            movement.ForceStopMovement();
        }
    }

    public void EndTalk()
    {
        isTalking = false;
        HideBubble();

        if (movement != null)
            movement.SetBusy(false);
    }

    public void ShowBubble(string line)
    {
        if (speechBubble != null)
            speechBubble.Show(line);
    }

    public void HideBubble()
    {
        if (speechBubble != null)
            speechBubble.Hide();
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        if (movement != null)
            movement.FaceTarget(targetPosition);
    }

    public void MoveAwayFrom(Vector3 targetPosition, float duration)
    {
        if (movement != null)
            movement.MoveAwayFrom(targetPosition, duration);
    }
}