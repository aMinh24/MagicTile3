using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileInteract : MonoBehaviour
{
    public bool isInteractable = true; // Flag to check if the tile can be interacted with
    public abstract void OnTouchDown();
    public abstract void OnTouchUp();
}
