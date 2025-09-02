using UnityEngine;
using UnityEngine;

public class KeyboardInput : IPlayerInput
{
    public float MoveX { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }

    public void UpdateInput()
    {
        MoveX = Input.GetAxisRaw("Horizontal");
        bool down = Input.GetKeyDown(KeyCode.Space);
        bool held = Input.GetKey(KeyCode.Space);

        JumpPressed = down;
        JumpHeld = held;
    }
}

