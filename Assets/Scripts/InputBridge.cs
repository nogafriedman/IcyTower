using UnityEngine;

public class InputBridge : MonoBehaviour
{
    [SerializeField] PlayerController2D controller;
    [SerializeField] MobileInputButtons mobile;

    void Update()
    {
        if (mobile) mobile.Feed(controller);
    }
}
