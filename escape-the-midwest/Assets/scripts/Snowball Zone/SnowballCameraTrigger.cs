/*
 * NOTE: added for snowball zone gameplay
 */

using UnityEngine;

public class SnowballCameraTrigger : MonoBehaviour
{
    public string requestId = "snowball_zone";
    public Vector3 offset = new Vector3(0, -3f, -12f);
    public float ease = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 lookDir = (Vector3.forward + Vector3.up * 0.2f).normalized;
            GameplayCamera.TiltCamera(requestId, lookDir, offset, ease);
        }
    }
}
