/*
 * NOTE: added for snowball zone gameplay
 */

using UnityEngine;

public class ResetTrigger : MonoBehaviour
{
    public static bool gameOver = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameOver = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameOver = false;
        }
    }
}
