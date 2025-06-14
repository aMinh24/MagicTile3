using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

// Ensure DataSong and TilePointData are defined or imported
// public class DataSong { public List<TilePointData> tilePointDatas; }
// public class TilePointData { public List<Vector2> points; }
// Ensure CONST and GameManager are defined or accessible
// public static class CONST { public static float CAMERA_SPEED = 10f; /* example */ }
// public class GameManager : MonoBehaviour { 
//     public static GameManager Instance { get; private set; }
//     public float multiplerCameraSpeed = 1f; // Corrected to cameraSpeedFactor based on GameManager.cs
//     void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }
// }


public class SpawnTileController : MonoBehaviour
{
    private readonly float[] _xPositions = { -1.682f, -0.56f, 0.56f, 1.682f }; // X positions for the tiles
    public GameObject shortTilePrefab; // Prefab for the short tile
    public GameObject longTilePrefab; // Prefab for the long tile
    public SpriteRenderer errorTile;
    public int initialShortTilePoolSize = 20; // Initial size of the short tile pool
    public float spawnAheadDistance = 20f; // How far ahead of the camera to spawn tiles

    private ITileSpawner _longTileSpawner;
    // _shortTileSpawner is kept for interface consistency but won't be used for pooled short tiles.

    private List<GameObject> _spawnedTiles = new List<GameObject>(); // List to keep track of spawned tiles
    private DataSong _songData; // Reference to the song data
    [SerializeField]
    private int _currentTileIndex = 0;

    private Queue<GameObject> _shortTilePool = new Queue<GameObject>();
    private List<TilePointData> _pendingShortTileData = new List<TilePointData>();
    private int _nextShortTileToSpawnIndex = 0;
    private float _speedFactor;

    void Awake()
    {
        // _shortTileSpawner = new ShortTileSpawner(); // Not strictly needed if all short tiles are pooled
        _longTileSpawner = new LongTileSpawner();
        InitializeShortTilePool();
    }

    private void InitializeShortTilePool()
    {
        if (shortTilePrefab == null)
        {
            Debug.LogError("ShortTilePrefab is not assigned. Cannot initialize short tile pool.");
            return;
        }
        for (int i = 0; i < initialShortTilePoolSize; i++)
        {
            GameObject tileInstance = Instantiate(shortTilePrefab, this.transform);
            tileInstance.SetActive(false);
            _shortTilePool.Enqueue(tileInstance);
        }
    }

    public void ReturnShortTileToPool(GameObject tileInstance)
    {
        if (tileInstance != null)
        {
            tileInstance.SetActive(false);
            // Optionally reset tile state here if needed (e.g., visual state)
            if (!_shortTilePool.Contains(tileInstance)) // Avoid duplicates if logic error elsewhere
            {
                _shortTilePool.Enqueue(tileInstance);
            }
            _spawnedTiles.Remove(tileInstance); // Remove from active list
        }
    }

    private void Start()
    {
        this.Register(EventID.TouchWrong, OnTouchWrong);
        this.Register(EventID.OnTileInteract, UpdateTileIndex); // Register to listen for tile interaction events
    }
    private void UpdateTileIndex(object data)
    {
        _currentTileIndex++;
    }

    void Update()
    {
        if (_songData == null) return;

        TrySpawnNextShortTile();

        if (_currentTileIndex >= _songData.tilePointDatas.Count)
        {
            // All tiles successfully interacted with or sequence finished
            return;
        }

        // Lose condition: checks the next expected tile in the song sequence
        TilePointData expectedTileData = _songData.tilePointDatas[_currentTileIndex];
        float yPositionOfExpectedTile = expectedTileData.points.Last().x * _speedFactor;

        if (yPositionOfExpectedTile - Camera.main.transform.position.y < -6f) // Check if the expected tile is missed
        {
            Debug.Log($"Lose: Expected tile at Y:{yPositionOfExpectedTile} missed. Camera Y: {Camera.main.transform.position.y}. CurrentTileIndex: {_currentTileIndex}");
            _currentTileIndex = _songData.tilePointDatas.Count + 1; // Effectively stop further processing by setting index out of bounds
            this.Broadcast(EventID.OnTileToEnd);
            GameManager.Instance?.EndGame(); // Notify GameManager to end the game
        }
    }

    private void TrySpawnNextShortTile()
    {
        if (_pendingShortTileData == null || _nextShortTileToSpawnIndex >= _pendingShortTileData.Count)
        {
            return;
        }

        TilePointData nextShortTileInfo = _pendingShortTileData[_nextShortTileToSpawnIndex];
        float spawnYPos = nextShortTileInfo.points[0].x * _speedFactor;

        if (spawnYPos - Camera.main.transform.position.y < spawnAheadDistance)
        {
            GameObject tileInstance;
            if (_shortTilePool.Count > 0)
            {
                tileInstance = _shortTilePool.Dequeue();
                tileInstance.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Short tile pool empty, instantiating new tile.");
                if (shortTilePrefab == null)
                { // Should not happen if pending list was populated
                    Debug.LogError("ShortTilePrefab is null, cannot instantiate fallback short tile.");
                    _nextShortTileToSpawnIndex++; // Skip this tile to avoid infinite loop
                    return;
                }
                tileInstance = Instantiate(shortTilePrefab);
            }

            tileInstance.transform.SetParent(this.transform); // Ensure parent is set
            Vector2 pointData = nextShortTileInfo.points[0];
            int columnIndex = (int)pointData.y;
            if (columnIndex < 0 || columnIndex >= _xPositions.Length)
            {
                Debug.LogError($"Invalid columnIndex {columnIndex} for short tile. Time: {pointData.x}");
                ReturnShortTileToPool(tileInstance); // Return problematic tile
                _nextShortTileToSpawnIndex++;
                return;
            }
            float xPos = _xPositions[columnIndex];
            tileInstance.transform.position = new Vector3(xPos, spawnYPos, 0);
            tileInstance.transform.rotation = Quaternion.identity;

            ShortTileInteract interactScript = tileInstance.GetComponent<ShortTileInteract>();
            if (interactScript != null)
            {
                interactScript.SpawnerController = this;
            }

            _spawnedTiles.Add(tileInstance);
            _nextShortTileToSpawnIndex++;
        }
    }

    private void ClearPreviousTilesAndResetState()
    {
        // Return pooled tiles to pool, destroy others
        List<GameObject> tilesToProcess = new List<GameObject>(_spawnedTiles);
        _spawnedTiles.Clear();
        foreach (var tile in tilesToProcess)
        {
            if (tile == null) continue;
            ShortTileInteract sti = tile.GetComponent<ShortTileInteract>();
            // Check if it's a short tile that should be returned to the pool
            if (sti != null && shortTilePrefab != null && tile.name.StartsWith(shortTilePrefab.name)) // A way to identify pooled tiles
            {
                ReturnShortTileToPool(tile);
            }
            else
            {
                Destroy(tile);
            }
        }

        // Ensure all tiles in the pool are inactive and parented correctly
        Queue<GameObject> tempQueue = new Queue<GameObject>();
        while (_shortTilePool.Count > 0)
        {
            GameObject pooledTile = _shortTilePool.Dequeue();
            if (pooledTile != null)
            {
                pooledTile.SetActive(false);
                pooledTile.transform.SetParent(this.transform); // Ensure parent
                tempQueue.Enqueue(pooledTile);
            }
        }
        _shortTilePool = tempQueue;

        _pendingShortTileData.Clear();
        _nextShortTileToSpawnIndex = 0;
        _currentTileIndex = 0;
    }


    [Button("Spawn Tiles From Song Data")] // Odin Inspector button for easy testing
    public void SpawnTilesFromData(DataSong songData)
    {
        ClearPreviousTilesAndResetState();

        if (songData == null)
        {
            Debug.LogError("SongData is null. Cannot spawn tiles.");
            return;
        }
        _songData = songData;

        // Prefab checks
        bool canSpawnShort = shortTilePrefab != null;
        bool canSpawnLong = longTilePrefab != null;

        if (!canSpawnShort) Debug.LogWarning("ShortTilePrefab is not assigned. Short tiles will not be spawned.");
        if (!canSpawnLong) Debug.LogWarning("LongTilePrefab is not assigned. Long tiles will not be spawned.");


        if (GameManager.Instance != null && CONST.CAMERA_SPEED != 0)
        {
            _speedFactor = GameManager.Instance.cameraSpeedFactor * CONST.CAMERA_SPEED;
        }
        else
        {
            Debug.LogWarning("GameManager.Instance or CONST.CAMERA_SPEED is not properly set. Using default speedFactor = 10f.");
            _speedFactor = 10f;
        }


        if (songData.tilePointDatas == null)
        {
            Debug.LogWarning("SongData.tilePointDatas is null.");
            return;
        }

        foreach (var tilePoint in songData.tilePointDatas)
        {
            if (tilePoint.points == null || tilePoint.points.Count == 0)
            {
                continue;
            }

            if (tilePoint.points.Count == 1) // Short tile
            {
                if (canSpawnShort)
                {
                    _pendingShortTileData.Add(tilePoint); // Add to pending list for pooled spawning
                }
                // Error already logged if prefab is null
            }
            else // Long tile (2 or more points)
            {
                if (canSpawnLong)
                {
                    GameObject spawnedLongTile = _longTileSpawner.Spawn(this.transform, longTilePrefab, tilePoint, _xPositions, _speedFactor);
                    if (spawnedLongTile != null) _spawnedTiles.Add(spawnedLongTile);
                }
                // Error already logged if prefab is null
            }
        }
    }
    private void OnTouchWrong(object data)
    {
        if (data is Vector2 position)
        {
            Debug.Log($"Touch wrong at position: {position}");
            if (errorTile != null)
            {
                errorTile.transform.position = new Vector3(position.x, position.y, 0);
                errorTile.gameObject.SetActive(true);
                errorTile.DOFade(0, 1f).SetEase(Ease.InOutSine);
            }
            else
            {
                Debug.LogWarning("ErrorTile SpriteRenderer is not assigned.");
            }
        }

    }
}
