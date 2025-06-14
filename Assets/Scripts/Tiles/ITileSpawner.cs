using UnityEngine;
using System.Collections.Generic;

// Assuming TilePointData is defined elsewhere, e.g.:
// public class TilePointData { public List<Vector2> points; /* ... */ }

public interface ITileSpawner
{
    GameObject Spawn(Transform parent, GameObject prefab, TilePointData tilePointData, float[] xPositions, float speedFactor);
}
