using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

// Placeholder for GameManager and CONST if they are structured this way.
// If GameManager.Instance or CONST.CAMERA_SPEED are not available,
// you might need to adjust the speed calculation.
// Example GameManager:
// public class GameManager : MonoBehaviour {
//     public static GameManager Instance { get; private set; }
//     public float cameraSpeedFactor = 1.0f;
//     void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }
// }
// Example CONST:
// public static class CONST { public const float CAMERA_SPEED = 1.0f; }


public class LongTileInteract : TileInteract
{
    public LineRenderer animationLineRenderer;
    public float animationBaseSpeed = 1.0f; // This will act like CONST.CAMERA_SPEED

    private LineRenderer originalLineRenderer;
    private Vector3[] targetPoints;
    private int currentTargetPointIndex; // The index of the point in targetPoints we are currently animating towards.
    private bool isAnimating = false;

    public override void OnTouchDown()
    {
        Debug.Log("LongTileInteract: Touch Down");
        this.Broadcast(EventID.OnTileInteract, this); // Notify listeners that this tile was interacted with
        this.isInteractable = false; // Disable interaction after the first touch
        StartLineAnimation();
    }

    public override void OnTouchUp()
    {
        Debug.Log("LongTileInteract: Touch Up");
        isAnimating = false; // Stop the animation if it was in progress
        // Deactivation is now handled at the end of the animation.
        // this.gameObject.SetActive(false); 
    }
    [Button("Start Animation")]
    void StartLineAnimation()
    {
        originalLineRenderer = GetComponent<LineRenderer>();

        if (originalLineRenderer == null)
        {
            Debug.LogError("LongTileInteract: No LineRenderer component found on this GameObject to use as a source.");
            this.gameObject.SetActive(false); // Deactivate if setup is invalid
            return;
        }

        if (animationLineRenderer == null)
        {
            Debug.LogError("LongTileInteract: AnimationLineRenderer is not assigned.");
            this.gameObject.SetActive(false); // Deactivate if setup is invalid
            return;
        }
        animationLineRenderer.gameObject.SetActive(true); // Ensure the animation line is active
        // Consider copying other relevant properties like useWorldSpace, alignment, textureMode, etc.
        animationLineRenderer.useWorldSpace = originalLineRenderer.useWorldSpace;


        int pointCount = originalLineRenderer.positionCount;
        if (pointCount < 2)
        {
            Debug.LogWarning("LongTileInteract: Original LineRenderer has less than 2 points. Cannot animate.");
            // Deactivate or handle as an instant tile
            if (pointCount == 1) { // If only one point, just show it and finish
                targetPoints = new Vector3[1];
                originalLineRenderer.GetPositions(targetPoints);
                animationLineRenderer.positionCount = 1;
                animationLineRenderer.SetPosition(0, targetPoints[0]);
            } else {
                animationLineRenderer.positionCount = 0; // No points to draw
            }
            isAnimating = false; 
            // Potentially deactivate gameObject if no animation is possible or desired
            // this.gameObject.SetActive(false); 
            return;
        }

        targetPoints = new Vector3[pointCount];
        originalLineRenderer.GetPositions(targetPoints);

        // Initialize with the first segment (zero length initially)
        animationLineRenderer.positionCount = 2; 
        animationLineRenderer.SetPosition(0, targetPoints[0]);
        animationLineRenderer.SetPosition(1, targetPoints[0]); // Second point starts at the first point's position

        currentTargetPointIndex = 1; // Start animating the second point of animationLineRenderer towards targetPoints[1]
        isAnimating = true;
        
        // originalLineRenderer.enabled = false; // Hide the original line
    }

    void Update()
    {
        if (!isAnimating)
        {
            return;
        }

        float effectiveSpeed = GameManager.Instance.cameraSpeedFactor* CONST.CAMERA_SPEED;
        
        Vector3 currentAnimatedPointLocation = animationLineRenderer.GetPosition(animationLineRenderer.positionCount - 1);
        Vector3 nextTargetPos = targetPoints[currentTargetPointIndex];

        float distanceToNextTarget = Vector3.Distance(currentAnimatedPointLocation, nextTargetPos);
        
        if (distanceToNextTarget < 0.001f) // Already at the target or very close
        {
            // Snap to target
            animationLineRenderer.SetPosition(animationLineRenderer.positionCount - 1, nextTargetPos);
        }
        else
        {
            Vector3 direction = (nextTargetPos - currentAnimatedPointLocation).normalized;
            float moveDistance = effectiveSpeed * Time.deltaTime;

            if (moveDistance >= distanceToNextTarget)
            {
                // Reached the target point
                animationLineRenderer.SetPosition(animationLineRenderer.positionCount - 1, nextTargetPos);
            }
            else
            {
                // Move towards the target point
                animationLineRenderer.SetPosition(animationLineRenderer.positionCount - 1, currentAnimatedPointLocation + direction * moveDistance);
                return; // Continue animating this segment in the next frame
            }
        }

        // If we've reached here, it means the current segment (ending at targetPoints[currentTargetPointIndex]) is complete.
        currentTargetPointIndex++;

        if (currentTargetPointIndex < targetPoints.Length)
        {
            // Add new point to start the next segment. The new point starts at the position of the previous target.
            animationLineRenderer.positionCount++;
            animationLineRenderer.SetPosition(animationLineRenderer.positionCount - 1, targetPoints[currentTargetPointIndex -1]); 
        }
        else
        {
            // Animation finished
            isAnimating = false;
            Debug.Log("LongTileInteract: Animation Finished.");
            // this.gameObject.SetActive(false); // Deactivate the tile
        }
    }
    // Removed StartAnimation() as it's replaced by StartLineAnimation()
}
