/*
 * NOTE: added for snowball zone gameplay
 */

using UnityEngine;

public class SnowballDestroy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (ResetTrigger.gameOver && other.CompareTag("Snowball"))
        {
            Destroy(other.gameObject);
            Debug.Log("Destroyed: " + other.name);
        }
    }
}
