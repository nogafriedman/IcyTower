using Unity.VisualScripting;
using UnityEngine;

public class powerUpScript : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.name + " entered the trigger zone!");
    }
}
