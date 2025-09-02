using UnityEngine;
using UnityEngine;

public class TouchInput : IPlayerInput
{
    public float MoveX { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }

    const float tapMaxDuration = 0.18f;
    const float tapMaxMove = 30f; // pixels

    float _lastJumpTime = -999f;

    public void UpdateInput()
    {
        MoveX = 0f;
        JumpPressed = false;
        JumpHeld = false;

        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            var pos = t.position;

            // Horizontal input by screen halves
            if (pos.x < Screen.width * 0.4f) MoveX = Mathf.Min(MoveX, -1f);
            else if (pos.x > Screen.width * 0.6f) MoveX = Mathf.Max(MoveX, 1f);

            if (t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved)
            {
                // Holding any touch = "jump held" (for variable jump height)
                JumpHeld = true;
            }

            if (t.phase == TouchPhase.Ended)
            {
                float dur = t.deltaTime; // time between last frames; we need full gesture duration
                // Better: track duration manually. Quick-and-dirty:
                // We'll approximate by using Touch.tapCount or small movement heuristic.
                bool smallMove = (t.deltaPosition.magnitude <= tapMaxMove);
                bool upperHalf = (pos.y > Screen.height * 0.5f);

                if (t.tapCount > 0 && upperHalf)      // typical quick tap
                {
                    JumpPressed = true;
                }
                else if (dur <= tapMaxDuration && smallMove && upperHalf)
                {
                    JumpPressed = true;
                }
            }
        }
    }
}

