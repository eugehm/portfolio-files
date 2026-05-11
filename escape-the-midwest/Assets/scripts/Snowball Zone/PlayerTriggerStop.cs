/*
 * NOTE: added for snowball zone gameplay
 */

using UnityEngine;

public class PlayerTriggerStop : MonoBehaviour
{
    public SnowballSpawner[] spawners;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (SnowballSpawner spawner in spawners)
            {
                spawner.StopSpawning();
            }
        }
    }
}
