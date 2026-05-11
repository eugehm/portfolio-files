/*
 * NOTE: added for snowball zone gameplay
 */

using UnityEngine;

public class MeltingTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SnowballMelter melter = other.GetComponent<SnowballMelter>();
        if (melter != null)
        {
            melter.StartMelting();
        }
    }
}
