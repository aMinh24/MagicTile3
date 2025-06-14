using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleTouchController : MonoBehaviour
{
    public TouchController touchController;
    public ParticleSystem tapVfx;
    private Dictionary<int, TileInteract> _activeTiles = new Dictionary<int, TileInteract>(); 
    void Start()
    {
        if (touchController != null)
        {
            touchController.OnTouchDown += HandleTouchDown;
            // If you need to handle touch up for other reasons:
            touchController.OnTouchUp += HandleTouchUp;
        }
        else
        {
            Debug.LogError("TouchController not found. Please assign it in the Inspector.");
        }
    }

    void OnDestroy()
    {
        if (touchController != null)
        {
            touchController.OnTouchDown -= HandleTouchDown;
            touchController.OnTouchUp -= HandleTouchUp;
        }
    }

    private void HandleTouchDown(int touchId, Vector2 screenPosition)
    {
        Debug.Log("Touch Down with ID: " + touchId + " at screen position: " + screenPosition);
        InteractWithTileAtPosition(touchId, screenPosition);
    }
    private void HandleTouchUp(int touchId, Vector2 screenPosition)
    {
        Debug.Log("Touch Up with ID: " + touchId + " at screen position: " + screenPosition);
        // If you need to handle touch up logic, you can implement it here.
        // For example, you could call a method on the TileInteract component if needed.
        if (_activeTiles.TryGetValue(touchId, out TileInteract tileInteract))
        {
            tileInteract.OnTouchUp();
            _activeTiles.Remove(touchId);
        }
    }
    private void InteractWithTileAtPosition(int touchId, Vector2 screenPosition)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);

        if (hitCollider != null)
        {
            TileInteract tileInteract = hitCollider.GetComponent<TileInteract>();
            if (tileInteract != null)
            {
                ProcessTileHit(tileInteract, screenPosition, worldPosition, touchId);
            }
            else
            {
                Debug.Log("Collider hit, but no TileInteract component found on: " + hitCollider.gameObject.name);
                ProcessMiss(worldPosition); // Treat as a miss if collider has no TileInteract
            }
        }
        else
        {
            ProcessMiss(worldPosition);
        }
    }

    private void ProcessTileHit(TileInteract tileInteract, Vector2 screenPosition, Vector2 worldPosition, int touchId)
    {
        tileInteract.OnTouchDown();

        // Check if the y-position of the click is in the middle of the screen
        float screenHeight = Screen.height;
        float lowerBound = screenHeight * 2 / 5f; // 40% of screen height
        float upperBound = screenHeight * 3 / 5f; // 60% of screen height

        bool isPerfectHit = screenPosition.y >= lowerBound && screenPosition.y <= upperBound;

        if (isPerfectHit)
        {
            Debug.Log("Perfect!");
        }
        else
        {
            Debug.Log("Good");
        }

        // Notify ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RecordHit(isPerfectHit);
        }
        else
        {
            Debug.LogError("ScoreManager instance not found. Ensure ScoreManager is in the scene and initialized.");
        }

        if (!_activeTiles.ContainsKey(touchId))
            _activeTiles.Add(touchId, tileInteract);
        
        if (tapVfx != null)
        {
            tapVfx.transform.position = worldPosition; // Set the position of the tap VFX
            tapVfx.Play(); // Play the tap VFX
        }
        else
        {
            Debug.LogWarning("Tap VFX not assigned in HandleTouchController.");
        }
    }

    private void ProcessMiss(Vector2 worldPosition)
    {
        this.Broadcast(EventID.TouchWrong, worldPosition); // Broadcast the event if no collider is hit
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame(); // End the game if no collider is hit
        }
        else
        {
            Debug.LogError("GameManager instance not found. Cannot end game.");
        }
        Debug.Log("No valid tile found at position: " + worldPosition + ". Game Over.");
    }
}
