using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class HerbGardenClickTarget : MonoBehaviour
{
    [SerializeField] private HerbGardenPlot gardenPlot;

    private Collider2D clickCollider;
    private Camera mainCamera;

    private void Awake()
    {
        clickCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        if (gardenPlot == null)
            gardenPlot = GetComponentInParent<HerbGardenPlot>();

        Debug.Log("HerbGardenClickTarget đã chạy trên object: " + gameObject.name);
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // Nếu đang bấm lên UI thì không cho click xuyên xuống ô đất.
        if (IsPointerOverUI())
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("Không tìm thấy Main Camera. Kiểm tra Tag của Main Camera phải là MainCamera.");
            return;
        }

        if (clickCollider == null)
        {
            Debug.LogWarning("Object " + gameObject.name + " chưa có Collider2D.");
            return;
        }

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 mousePoint = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

        if (!clickCollider.OverlapPoint(mousePoint))
            return;

        if (gardenPlot == null)
        {
            Debug.LogWarning("Ô đất chưa gán HerbGardenPlot.");
            return;
        }

        gardenPlot.TryHarvest();
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }
}