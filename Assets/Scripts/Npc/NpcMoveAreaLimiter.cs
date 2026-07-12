using UnityEngine;

[RequireComponent(typeof(NpcAIController))]
public class NpcMoveAreaLimiter : MonoBehaviour
{
    [Header("Vùng được phép di chuyển")]
    [SerializeField] private Collider2D moveArea;

    [Header("Cấu hình")]
    [SerializeField] private float pushBackTime = 0.5f;
    [SerializeField] private float boundaryPadding = 0.1f;

    private NpcAIController npcAI;

    private void Awake()
    {
        npcAI = GetComponent<NpcAIController>();
    }

    private void LateUpdate()
    {
        if (moveArea == null)
            return;

        Vector3 currentPosition = transform.position;
        Bounds bounds = moveArea.bounds;

        float minX = bounds.min.x + boundaryPadding;
        float maxX = bounds.max.x - boundaryPadding;
        float minY = bounds.min.y + boundaryPadding;
        float maxY = bounds.max.y - boundaryPadding;

        bool isOutside =
            currentPosition.x < minX ||
            currentPosition.x > maxX ||
            currentPosition.y < minY ||
            currentPosition.y > maxY;

        if (!isOutside)
            return;

        float clampedX = Mathf.Clamp(currentPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(currentPosition.y, minY, maxY);

        transform.position = new Vector3(clampedX, clampedY, currentPosition.z);

        Vector2 moveToCenter = bounds.center - transform.position;

        if (moveToCenter.sqrMagnitude < 0.01f)
            moveToCenter = Vector2.down;

        npcAI.MoveDirectionForSeconds(moveToCenter.normalized, pushBackTime);
    }

    public void SetMoveArea(Collider2D newMoveArea)
    {
        moveArea = newMoveArea;
    }
}