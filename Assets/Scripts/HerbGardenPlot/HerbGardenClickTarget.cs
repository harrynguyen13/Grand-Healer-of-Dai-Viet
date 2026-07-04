using UnityEngine;
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

        Debug.Log("Đã bấm chuột trái.");

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

        Debug.Log("Mouse World Position: " + mousePoint);

        if (!clickCollider.OverlapPoint(mousePoint))
        {
            Debug.Log("Bấm chuột nhưng chưa trúng icon thu hoạch.");
            return;
        }

        Debug.Log("ĐÃ CLICK TRÚNG ICON THU HOẠCH.");

        if (gardenPlot == null)
        {
            Debug.LogWarning("HarvestReadyIcon chưa gán Garden Plot.");
            return;
        }

        gardenPlot.TryHarvest();
    }
}