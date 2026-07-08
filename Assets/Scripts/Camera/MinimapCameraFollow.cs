using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Camera")]
    [SerializeField] private Camera minimapCamera;

    [Header("Bounds có sẵn")]
    [SerializeField] private Transform cameraBoundsParent;

    [Header("Minimap Mode")]
    [SerializeField] private bool showWholeCurrentMap = true;
    [SerializeField] private float fixedOrthographicSize = 18f;
    [SerializeField] private float padding = 1f;

    private Collider2D[] mapBounds;
    private Collider2D currentBounds;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshReferences();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RefreshReferences();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferences();
    }

    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
        currentBounds = null;
        RefreshBounds();
    }

    private void RefreshReferences()
    {
        if (minimapCamera == null)
            minimapCamera = GetComponent<Camera>();

        FindPlayer();

        RefreshBounds();
    }

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }

    private void RefreshBounds()
    {
        if (cameraBoundsParent == null)
        {
            GameObject boundsObj = GameObject.Find("CameraBounds");

            if (boundsObj != null)
                cameraBoundsParent = boundsObj.transform;
        }

        if (cameraBoundsParent != null)
            mapBounds = cameraBoundsParent.GetComponentsInChildren<Collider2D>();

        currentBounds = null;
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            FindPlayer();

            if (player == null)
                return;
        }

        if (minimapCamera == null)
        {
            minimapCamera = GetComponent<Camera>();

            if (minimapCamera == null)
                return;
        }

        if (mapBounds == null || mapBounds.Length == 0)
        {
            RefreshBounds();

            if (mapBounds == null || mapBounds.Length == 0)
                return;
        }

        UpdateCurrentBounds();

        if (currentBounds == null)
            return;

        if (showWholeCurrentMap)
        {
            ShowWholeMap();
        }
        else
        {
            FollowPlayerInsideBounds();
        }
    }

    private void UpdateCurrentBounds()
    {
        Vector2 playerPos = player.position;

        foreach (Collider2D col in mapBounds)
        {
            if (col == null)
                continue;

            if (col.OverlapPoint(playerPos))
            {
                currentBounds = col;
                return;
            }
        }
    }

    private void ShowWholeMap()
    {
        Bounds b = currentBounds.bounds;

        float mapWidth = b.size.x;
        float mapHeight = b.size.y;

        float cameraAspect = minimapCamera.aspect;

        float sizeByHeight = mapHeight / 2f;
        float sizeByWidth = mapWidth / (2f * cameraAspect);

        minimapCamera.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth) + padding;

        transform.position = new Vector3(
            b.center.x,
            b.center.y,
            transform.position.z
        );
    }

    private void FollowPlayerInsideBounds()
    {
        Bounds b = currentBounds.bounds;

        minimapCamera.orthographicSize = fixedOrthographicSize;

        float camHeight = minimapCamera.orthographicSize;
        float camWidth = camHeight * minimapCamera.aspect;

        float minX = b.min.x + camWidth;
        float maxX = b.max.x - camWidth;
        float minY = b.min.y + camHeight;
        float maxY = b.max.y - camHeight;

        float targetX;
        float targetY;

        if (minX > maxX)
            targetX = b.center.x;
        else
            targetX = Mathf.Clamp(player.position.x, minX, maxX);

        if (minY > maxY)
            targetY = b.center.y;
        else
            targetY = Mathf.Clamp(player.position.y, minY, maxY);

        transform.position = new Vector3(
            targetX,
            targetY,
            transform.position.z
        );
    }
}