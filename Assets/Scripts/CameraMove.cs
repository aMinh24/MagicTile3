using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private float maxHeight; // Maximum height the camera will reach
    public float speed = 2.0f;     // Speed of the camera movement

    private int direction = 1; // 1 for up, -1 for down
    private float initialY;
    public bool canMove = false;

    // Start is called before the first frame update
    void Start()
    {
        initialY = transform.position.y; // Store the initial Y position
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return; // If movement is not allowed, exit early
        transform.Translate(Vector3.up * speed * direction * Time.deltaTime);

        if (transform.position.y >= initialY + this.maxHeight)
        {
            transform.position = new Vector3(transform.position.x, initialY, transform.position.z);
        }
    }

    public void SetMaxHeightAndResetPosition(float newMaxHeight)
    {
        this.maxHeight = newMaxHeight;
        transform.position = new Vector3(transform.position.x, initialY, transform.position.z); // Reset position
        canMove = true; // Enable movement
    }

    public void UpdateSpeed()
    {
        speed = GameManager.Instance.cameraSpeedFactor* CONST.CAMERA_SPEED;
    }
}
