using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortTileInteract : TileInteract
{
    public SpawnTileController SpawnerController; // Reference to the tile spawner for pooling

    public override void OnTouchDown()
    {
        if (isInteractable)
        {
            Debug.Log("ShortTileInteract: Touch Down");
            this.Broadcast(EventID.OnTileInteract, this); // Notify listeners that the tile was interacted with
            if (SpawnerController != null)
            {
                SpawnerController.ReturnShortTileToPool(this.gameObject); // Return to pool
            }
            else
            {
                Debug.LogWarning("SpawnerController not set on ShortTileInteract. Deactivating directly.");
                this.gameObject.SetActive(false); // Fallback: Deactivate the tile on touch down
            }
        }
        else
        {
            Debug.Log("ShortTileInteract: Tile is not interactable.");
        }
    }

    public override void OnTouchUp()
    {
    }
}
