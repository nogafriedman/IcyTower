using UnityEngine;

public class MobileInputButtons : MonoBehaviour
{
    public bool leftHeld, rightHeld, jumpHeld;
    bool jumpDownThisFrame;

    public void LeftDown()  => leftHeld = true;
    public void LeftUp()    => leftHeld = false;
    public void RightDown() => rightHeld = true;
    public void RightUp()   => rightHeld = false;
    public void JumpDown()  { jumpHeld = true; jumpDownThisFrame = true; }
    public void JumpUp()    => jumpHeld = false;

    public void Feed(PlayerController2D controller)
    {
        float mx = (leftHeld ? -1f : 0f) + (rightHeld ? 1f : 0f);
        controller.SetInput(mx, jumpDownThisFrame, jumpHeld);
        jumpDownThisFrame = false; // consume
    }
}
