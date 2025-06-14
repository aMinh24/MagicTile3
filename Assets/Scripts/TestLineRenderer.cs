using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TestLineRenderer : MonoBehaviour
{
    public float speed = 1.0f; // Speed at which the line extends
    public Vector2[] pathPoints = new Vector2[] {
        new Vector2(0, 0),
        new Vector2(0, 4),
        new Vector2(1, 7)
    };

    private LineRenderer lineRenderer;
    private Vector3 currentHeadPosition;
    private int numFixedPoints; // Number of points in pathPoints that are already fixed in the LineRenderer
    private int nextTargetIndex; // Index in pathPoints for the current animation target

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component not found on this GameObject.");
            enabled = false; 
            return;
        }

        if (pathPoints == null || pathPoints.Length < 2)
        {
            Debug.LogError("PathPoints must contain at least two points.");
            enabled = false;
            return;
        }

        InitializePath();
    }

    void InitializePath()
    {
        numFixedPoints = 1;
        nextTargetIndex = 1;

        Vector3 initialPointV3 = new Vector3(pathPoints[0].x, pathPoints[0].y, 0);
        currentHeadPosition = initialPointV3;

        lineRenderer.positionCount = 2; // First fixed point + animating head
        lineRenderer.SetPosition(0, initialPointV3);
        lineRenderer.SetPosition(1, currentHeadPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if (lineRenderer == null || pathPoints == null || pathPoints.Length < 2) return;

        if (nextTargetIndex >= pathPoints.Length) // All points reached, path complete for one cycle
        {
            // Option: Do nothing, or reset/loop
            // For looping:
            InitializePath(); 
            return;
        }

        Vector3 targetPointV3 = new Vector3(pathPoints[nextTargetIndex].x, pathPoints[nextTargetIndex].y, 0);

        // Move currentHeadPosition towards the targetPointV3
        currentHeadPosition = Vector3.MoveTowards(currentHeadPosition, targetPointV3, speed * Time.deltaTime);
        lineRenderer.SetPosition(numFixedPoints, currentHeadPosition); // Update the animating head's position

        // If the head reaches the target point
        if (Vector3.Distance(currentHeadPosition, targetPointV3) < 0.01f)
        {
            // Fix the reached point
            lineRenderer.SetPosition(numFixedPoints, targetPointV3);
            
            numFixedPoints++;
            nextTargetIndex++;

            if (nextTargetIndex < pathPoints.Length)
            {
                // Add a new point to the line renderer for the next segment's animation
                lineRenderer.positionCount = numFixedPoints + 1;
                // The new animating head starts from the point just reached
                currentHeadPosition = targetPointV3; 
                lineRenderer.SetPosition(numFixedPoints, currentHeadPosition);
            }
            else
            {
                // All points in the current path have been rendered.
                // The loop will be handled by the check at the beginning of Update.
            }
        }
    }
}
