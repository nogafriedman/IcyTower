using UnityEngine;

public class KeyboardFeeder : MonoBehaviour
{
    [SerializeField] PlayerController2D controller;

    void Update()
    {
        float mx = Input.GetAxisRaw("Horizontal");       // old Input axis
        bool jumpDown = Input.GetButtonDown("Jump");     // Space by default
        bool jumpHeld = Input.GetButton("Jump");
        controller.SetInput(mx, jumpDown, jumpHeld);
    }
}
