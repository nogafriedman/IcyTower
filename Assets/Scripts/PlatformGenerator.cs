using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    public GameObject platformPrefab; 
    public Transform player;       
    public int initialPlatformCount = 10;  // how many platforms to generate at the beginning of the game
    public float minGap = 0.5f; //minimum gap between platforms
    public float maxGap = 2f; //maximum gap between platforms   
    public float platformWidth = 5f;  

    private Vector3 spawnPosition;

    void Start()
    {
        spawnPosition = new Vector3(0, -6, 0);

        for (int i = 0; i < initialPlatformCount; i++)
        {
            SpawnPlatform();
        }
    }

    void Update()
    {
        while (spawnPosition.y < player.position.y + 10f)
        {
            SpawnPlatform();
        }
    }

    void SpawnPlatform()
    {
        spawnPosition.y += Random.Range(minGap, maxGap);
        spawnPosition.x = Random.Range(-platformWidth, platformWidth);
        Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
    }
}