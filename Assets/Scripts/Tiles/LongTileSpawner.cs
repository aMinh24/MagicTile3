using UnityEngine;
using System.Collections.Generic;

public class LongTileSpawner : ITileSpawner
{
    private const float Y_MULTIPLIER = 670f;
    private const float X_OFFSET = 230.0f;
    private static readonly float[] COLLIDER_BASE_X_POSITIONS = new float[] { -520f, -170f, 230f, 630f };
    private const float COLLIDER_CAP_OFFSET = 250f;

    public GameObject Spawn(Transform parent, GameObject prefab, TilePointData tilePointData, float[] xPositions, float speedFactor)
    {
        if (prefab == null)
        {
            Debug.LogError("LongTilePrefab is not assigned for LongTileSpawner.");
            return null;
        }
        if (tilePointData.points == null || tilePointData.points.Count < 2) // Long tiles need at least 2 points
        {
            Debug.LogWarning("Not enough points data for long tile.");
            return null;
        }

        // Determine the initial position for the long tile object (e.g., first point)
        Vector2 firstPointRaw = tilePointData.points[0];
        int firstColumnIndex = (int)firstPointRaw.y;
        if (firstColumnIndex < 0 || firstColumnIndex >= xPositions.Length)
        {
            Debug.LogError($"Invalid columnIndex {firstColumnIndex} for the first point of a long tile. Time: {firstPointRaw.x}");
            return null;
        }

        GameObject longTileInstance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
        longTileInstance.SetActive(true); // Ensure the long tile is active
        LineRenderer lr = longTileInstance.GetComponent<LineRenderer>();

        if (lr != null)
        {
            lr.positionCount = tilePointData.points.Count;
            Vector3[] linePositions = new Vector3[tilePointData.points.Count];

            for (int i = 0; i < tilePointData.points.Count; i++)
            {
                Vector2 currentPointData = tilePointData.points[i];
                float currentTime = currentPointData.x;
                int currentColumnIndex = (int)currentPointData.y;

                if (currentColumnIndex < 0 || currentColumnIndex >= xPositions.Length)
                {
                    Debug.LogError($"Invalid columnIndex {currentColumnIndex} for point {i} in long tile. Time: {currentTime}");
                    if (i > 0 && (int)tilePointData.points[i - 1].y >= 0 && (int)tilePointData.points[i - 1].y < xPositions.Length)
                    {
                        currentColumnIndex = (int)tilePointData.points[i - 1].y; // use previous valid
                    }
                    else
                    {
                        currentColumnIndex = 0; // fallback to first column
                    }
                }

                float currentXPos = xPositions[currentColumnIndex];
                float currentYPos = currentTime * speedFactor;

                // Assuming LineRenderer.useWorldSpace = true.
                // If false, positions should be local to longTileInstance.
                // linePositions[i] = new Vector3(currentXPos, currentYPos, 0) - initialLongTilePosition; (if local)
                linePositions[i] = new Vector3(currentXPos, currentYPos, 0);
            }
            lr.SetPositions(linePositions);

            // Setup PolygonCollider2D
            PolygonCollider2D polyCollider = longTileInstance.GetComponent<PolygonCollider2D>();
            if (polyCollider == null)
            {
                polyCollider = longTileInstance.AddComponent<PolygonCollider2D>();
            }
            SetupPolygonCollider(polyCollider, tilePointData);
        }
        else
        {
            Debug.LogError("LongTilePrefab is missing a LineRenderer component.");
        }
        return longTileInstance;
    }

    private void SetupPolygonCollider(PolygonCollider2D polyCollider, TilePointData tilePointData)
    {
        if (tilePointData == null || tilePointData.points == null || tilePointData.points.Count < 1) // Needs at least one point for a shape
        {
            polyCollider.pathCount = 0;
            Debug.LogWarning("Not enough points in TilePointData for PolygonCollider.");
            return;
        }

        int pointCount = tilePointData.points.Count;
        // Capacity for: pointCount for +X side, pointCount for -X side, 2 cap points
        List<Vector2> colliderPathPoints = new List<Vector2>(pointCount * 2 + 2);

        // --- Add first cap point ---
        Vector2 firstOriginalPointData = tilePointData.points[0];
        float firstRawTime = firstOriginalPointData.x;
        int firstColumnIndex = (int)firstOriginalPointData.y;

        if (firstColumnIndex < 0 || firstColumnIndex >= COLLIDER_BASE_X_POSITIONS.Length)
        {
            Debug.LogWarning($"Invalid columnIndex {firstColumnIndex} for collider start cap (time: {firstRawTime}). Clamping to range [0, {COLLIDER_BASE_X_POSITIONS.Length - 1}].");
            firstColumnIndex = Mathf.Clamp(firstColumnIndex, 0, COLLIDER_BASE_X_POSITIONS.Length - 1);
        }

        float firstBaseX = COLLIDER_BASE_X_POSITIONS[firstColumnIndex];
        float firstLocalY = firstRawTime * GameManager.Instance.cameraSpeedFactor * Y_MULTIPLIER;
        float firstLocalXPlus = firstBaseX;
        colliderPathPoints.Add(new Vector2(firstLocalXPlus, firstLocalY - COLLIDER_CAP_OFFSET));

        // Calculate points for the first side (+X_OFFSET)
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 currentPointData = tilePointData.points[i];
            float currentRawTime = currentPointData.x;
            int currentColumnIndex = (int)currentPointData.y;

            if (currentColumnIndex < 0 || currentColumnIndex >= COLLIDER_BASE_X_POSITIONS.Length)
            {
                Debug.LogWarning($"Invalid columnIndex {currentColumnIndex} for collider point {i} (time: {currentRawTime}). Clamping to range [0, {COLLIDER_BASE_X_POSITIONS.Length - 1}].");
                currentColumnIndex = Mathf.Clamp(currentColumnIndex, 0, COLLIDER_BASE_X_POSITIONS.Length - 1);
            }

            float baseX = COLLIDER_BASE_X_POSITIONS[currentColumnIndex];
            float localY = currentRawTime * GameManager.Instance.cameraSpeedFactor * Y_MULTIPLIER;
            float localX = baseX + X_OFFSET;
            
            colliderPathPoints.Add(new Vector2(localX, localY));
        }

        // --- Add second cap point ---
        Vector2 lastOriginalPointData = tilePointData.points[pointCount - 1];
        float lastRawTime = lastOriginalPointData.x;
        int lastColumnIndex = (int)lastOriginalPointData.y;

        if (lastColumnIndex < 0 || lastColumnIndex >= COLLIDER_BASE_X_POSITIONS.Length)
        {
            Debug.LogWarning($"Invalid columnIndex {lastColumnIndex} for collider end cap (time: {lastRawTime}). Clamping to range [0, {COLLIDER_BASE_X_POSITIONS.Length - 1}].");
            lastColumnIndex = Mathf.Clamp(lastColumnIndex, 0, COLLIDER_BASE_X_POSITIONS.Length - 1);
        }
        
        float lastBaseX = COLLIDER_BASE_X_POSITIONS[lastColumnIndex];
        float lastLocalY = lastRawTime * GameManager.Instance.cameraSpeedFactor * Y_MULTIPLIER;
        float lastLocalXMinus = lastBaseX;
        colliderPathPoints.Add(new Vector2(lastLocalXMinus, lastLocalY + COLLIDER_CAP_OFFSET));

        // Calculate points for the second side (-X_OFFSET), in reverse order
        for (int i = pointCount - 1; i >= 0; i--)
        {
            Vector2 currentPointData = tilePointData.points[i];
            float currentRawTime = currentPointData.x;
            int currentColumnIndex = (int)currentPointData.y;

            if (currentColumnIndex < 0 || currentColumnIndex >= COLLIDER_BASE_X_POSITIONS.Length)
            {
                // Warning already issued in the first loop if problematic, but good for safety
                currentColumnIndex = Mathf.Clamp(currentColumnIndex, 0, COLLIDER_BASE_X_POSITIONS.Length - 1);
            }
            
            float baseX = COLLIDER_BASE_X_POSITIONS[currentColumnIndex];
            float localY = currentRawTime * GameManager.Instance.cameraSpeedFactor * Y_MULTIPLIER;
            float localX = baseX - X_OFFSET;

            colliderPathPoints.Add(new Vector2(localX, localY));
        }

        polyCollider.SetPath(0, colliderPathPoints);
    }
}
