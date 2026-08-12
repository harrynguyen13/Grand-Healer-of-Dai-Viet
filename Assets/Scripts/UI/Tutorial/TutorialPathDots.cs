using System.Collections.Generic;
using UnityEngine;

public class TutorialPathDots : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AStarPathfinder2D pathfinder;
    [SerializeField] private GameObject dotPrefab;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Dot Settings")]
    [SerializeField] private float dotSpacing = 0.7f;
    [SerializeField] private float refreshInterval = 0.3f;

    private Transform player;
    private Transform target;

    private readonly List<GameObject> spawnedDots =
        new List<GameObject>();

    private float refreshTimer;

    private void Start()
    {
        FindPlayer();

        if (pathfinder == null)
            pathfinder = FindAnyObjectByType<AStarPathfinder2D>();
        ClearDots();
    }

    private void Update()
    {
        if (target == null)
        {
            ClearDots();
            return;
        }

        if (player == null)
        {
            FindPlayer();

            if (player == null)
                return;
        }

        refreshTimer -= Time.deltaTime;

        if (refreshTimer > 0f)
            return;

        refreshTimer = refreshInterval;

        RefreshPath();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        refreshTimer = 0f;

        if (target == null)
        {
            ClearDots();
            return;
        }

        RefreshPath();
    }

    public void ClearTarget()
    {
        target = null;
        ClearDots();
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void RefreshPath()
    {
        if (player == null)
            return;

        if (target == null)
            return;

        if (pathfinder == null)
        {
            pathfinder = FindAnyObjectByType<AStarPathfinder2D>();

            if (pathfinder == null)
            {
                Debug.LogWarning(
                    "TutorialPathDots chưa tìm thấy AStarPathfinder2D trong scene hiện tại."
                );

                return;
            }

            Debug.Log(
                "TutorialPathDots đã tự gán AStarPathfinder2D: "
                + pathfinder.gameObject.name
            );
        }

        if (dotPrefab == null)
        {
            Debug.LogWarning(
                "TutorialPathDots chưa có Dot Prefab."
            );

            return;
        }

        List<Vector2> path =
            pathfinder.FindPath(
                player.position,
                target.position
            );

        ClearDots();

        if (path == null || path.Count == 0)
            return;

        SpawnDotsAlongPath(path);
    }

    private void SpawnDotsAlongPath(List<Vector2> path)
    {
        Vector2 previousPoint = player.position;

        float distanceSinceLastDot = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2 currentPoint = path[i];

            float segmentLength =
                Vector2.Distance(
                    previousPoint,
                    currentPoint
                );

            if (segmentLength <= 0f)
            {
                previousPoint = currentPoint;
                continue;
            }

            Vector2 direction =
                (currentPoint - previousPoint).normalized;

            float travelled = 0f;

            while (
                distanceSinceLastDot +
                segmentLength - travelled
                >= dotSpacing
            )
            {
                float neededDistance =
                    dotSpacing - distanceSinceLastDot;

                travelled += neededDistance;

                Vector2 spawnPosition =
                    previousPoint +
                    direction * travelled;

                SpawnDot(spawnPosition);

                distanceSinceLastDot = 0f;
            }

            distanceSinceLastDot +=
                segmentLength - travelled;

            previousPoint = currentPoint;
        }
    }

    private void SpawnDot(Vector2 position)
    {
        GameObject dot =
            Instantiate(
                dotPrefab,
                position,
                Quaternion.identity,
                transform
            );

        spawnedDots.Add(dot);
    }

    private void ClearDots()
    {
        for (int i = 0; i < spawnedDots.Count; i++)
        {
            if (spawnedDots[i] != null)
                Destroy(spawnedDots[i]);
        }

        spawnedDots.Clear();
    }

    private void OnDisable()
    {
        ClearDots();
    }
}