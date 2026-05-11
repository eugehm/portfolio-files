using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum BigMessageLetterState { NORMAL, FALLING_MIDDLE, FALLING_OUT };

public class BigMessageLetter : MonoBehaviour
{
    public int index = 0;

    Vector3 initial_local_position;

    BigMessageLetterState current_state = BigMessageLetterState.NORMAL;

    void Awake()
    {
        initial_local_position = transform.localPosition;
        transform.localPosition = initial_local_position + Vector3.up * 30;
    }

    public IEnumerator DoDelayedStart()
    {
        already_bounced = false;
        velocity = 0.0f;
        yield return new WaitForSecondsRealtime(0.1f * index);

        current_state = BigMessageLetterState.FALLING_MIDDLE;
    }

    public IEnumerator DoDelayedEnd()
    {
        yield return new WaitForSecondsRealtime(0.1f * index);

        velocity = 0.5f;
        current_state = BigMessageLetterState.FALLING_OUT;
    }

    float velocity = 0.0f;

    bool already_bounced = false;
    void Update()
    {
        if (current_state == BigMessageLetterState.NORMAL)
        {
            transform.localPosition = initial_local_position + Vector3.up * 30;
            return;
        }

        /* Fall */
        velocity -= 0.02f * Time.deltaTime * 60.0f;
        transform.localPosition += Vector3.up * velocity;

        if (current_state == BigMessageLetterState.FALLING_MIDDLE)
        {
            /* Bounce in middle */
            if (transform.localPosition.y <= initial_local_position.y)
            {
                if (already_bounced == false)
                {
                    already_bounced = true;
                    transform.localPosition = initial_local_position;
                    velocity *= -0.5f;
                }
                else
                {
                    transform.localPosition = initial_local_position;
                    velocity = 0.0f;
                }
            }
        }
        else if (current_state == BigMessageLetterState.FALLING_OUT)
        {
            if (transform.localPosition.y < initial_local_position.y - 30)
            {
                current_state = BigMessageLetterState.NORMAL;
            }
        }
        
        /* Spin */
        transform.Rotate(Vector3.right * Mathf.Cos(Time.time * 3 + index) * 0.6f);
    }
}
