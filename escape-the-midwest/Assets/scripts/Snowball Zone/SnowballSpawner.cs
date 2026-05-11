/*
 * NOTE: added for snowball zone gameplay
 */

using UnityEngine;

public class SnowballSpawner : MonoBehaviour
{
    public GameObject snowballPrefab;
    public float spawnInterval = 1f;
    public float spawnRangeX = 37.36f;
    public float spawnHeight = 10f;
    private float timer = 0f;
    private bool spawningEnabled = false;
    public Transform spawnParent;

    // Update is called once per frame
    void Update()
    {
        if (!spawningEnabled) return;
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnSnowball();
            timer = 0f;
        }
    }

    void SpawnSnowball()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX) + transform.position.x;
        Vector3 spawnPos = new Vector3(randomX, spawnHeight, transform.position.z);
        Instantiate(snowballPrefab, spawnPos, Quaternion.identity, spawnParent);
    }

    public void StartSpawning()
    {
        spawningEnabled = true;
        Debug.Log("Spawning activated.");
    }

    public void StopSpawning()
    {
        spawningEnabled = false;
        Debug.Log("Spawning deactivated.");
    }
}
