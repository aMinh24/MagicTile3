using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TouchController : MonoBehaviour
{
    public delegate void TouchDownAction(int touchId, Vector2 position);
    public event TouchDownAction OnTouchDown;

    public delegate void TouchUpAction(int touchId, Vector2 position);
    public event TouchUpAction OnTouchUp;
    
    private class ActiveTouch
    {
        public int touchId;
        public Vector2 startPosition; // Store start position if needed, or just use current

        public ActiveTouch(int id, Vector2 pos)
        {
            touchId = id;
            startPosition = pos;
        }
    }

    private Dictionary<int, ActiveTouch> activeTouches = new Dictionary<int, ActiveTouch>();

    void Update()
    {
        if (!GameManager.Instance.isGameStarted) return;
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (!activeTouches.ContainsKey(touch.fingerId))
                        {
                            ActiveTouch newTouch = new ActiveTouch(touch.fingerId, touch.position);
                            activeTouches.Add(touch.fingerId, newTouch);
                            OnTouchDown?.Invoke(newTouch.touchId, touch.position);
                        }
                        break;

                    // Moved and Stationary phases are not explicitly handled for down/up events
                    // but are part of the touch lifecycle.
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        // Optionally, you could add an OnTouchMove event here if needed in the future
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (activeTouches.TryGetValue(touch.fingerId, out ActiveTouch endedTouch))
                        {
                            OnTouchUp?.Invoke(endedTouch.touchId, touch.position);
                            activeTouches.Remove(touch.fingerId);
                        }
                        break;
                }
            }
        }
    }
}
