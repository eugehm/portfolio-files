/*
 * NOTE: added for snowball zone gameplay
 */

using UnityEngine;

public class SnowballCameraExit : MonoBehaviour
{
    public string requestId = "snowball_zone";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameplayCamera.RemoveTiltCamera(requestId);
        }
    }
}
