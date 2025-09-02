using UnityEngine;

public interface IPlayerInput
{
    float MoveX { get; }       // -1..1
    bool JumpPressed { get; }  // edge-trigger
    bool JumpHeld { get; }     // hold
    void UpdateInput();        // call once per Update
}

