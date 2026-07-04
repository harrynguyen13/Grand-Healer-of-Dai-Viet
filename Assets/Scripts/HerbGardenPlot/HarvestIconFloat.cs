using UnityEngine;

public class HarvestIconFloat : MonoBehaviour
{
    [Header("Độ cao nhấp nhô")]
    [SerializeField] private float floatHeight = 0.12f;

    [Header("Tốc độ nhấp nhô")]
    [SerializeField] private float floatSpeed = 2.5f;

    [Header("Có xoay nhẹ không")]
    [SerializeField] private bool useSmallRotation = true;

    [SerializeField] private float rotationAngle = 4f;
    [SerializeField] private float rotationSpeed = 2f;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    private void OnEnable()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.localPosition = startLocalPosition + new Vector3(0f, yOffset, 0f);

        if (useSmallRotation)
        {
            float zRotation = Mathf.Sin(Time.time * rotationSpeed) * rotationAngle;
            transform.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, zRotation);
        }
    }

    private void OnDisable()
    {
        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;
    }
}