using UnityEngine;

public class ShortTileSpawner : ITileSpawner
{
    public GameObject Spawn(Transform parent, GameObject prefab, TilePointData tilePointData, float[] xPositions, float speedFactor)
    {
        if (prefab == null)
        {
            Debug.LogError("ShortTilePrefab is not assigned for ShortTileSpawner.");
            return null;
        }
        if (tilePointData.points == null || tilePointData.points.Count == 0)
        {
            Debug.LogWarning("No points data for short tile.");
            return null;
        }

        Vector2 pointData = tilePointData.points[0];
        float time = pointData.x;
        int columnIndex = (int)pointData.y;

        if (columnIndex < 0 || columnIndex >= xPositions.Length)
        {
            Debug.LogError($"Invalid columnIndex {columnIndex} for short tile. Time: {time}");
            return null;
        }

        float xPos = xPositions[columnIndex];
        float yPos = time * speedFactor;

        GameObject instance = Object.Instantiate(prefab, new Vector3(xPos, yPos, 0), Quaternion.identity, parent);
        return instance;
    }
}
