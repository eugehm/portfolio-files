/*
 * NOTE: added for snowball zone gameplay
 */

using UnityEngine;

public class SnowballMelter : MonoBehaviour
{
    private bool isMelting = false;
    public float meltSpeed = 10f;

    void Update()
    {
        if (isMelting)
        {
            transform.localScale -= Vector3.one * meltSpeed * Time.deltaTime;
            if (transform.localScale.x <= 0.1f)
            {
                Destroy(gameObject);
            }
        }
    }

    public void StartMelting()
    {
        isMelting = true;
    }
}
