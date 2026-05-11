using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int currency_delta = 1;
    public int lives_delta = 0;

    Vector3 initial_position;
    Vector3 initial_scale;

    public enum CollectibleState { BOUNCING, SEEKING, COLLECTED };
    CollectibleState current_state = CollectibleState.BOUNCING;

    // Start is called before the first frame update
    void Start()
    {
        initial_position = transform.position;
        initial_scale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerController.GetPlayerGameobject() == null)
            return;

        if (current_state == CollectibleState.BOUNCING)
            BounceState();
        else if (current_state == CollectibleState.SEEKING)
            SeekingState();
        else if (current_state == CollectibleState.COLLECTED)
            CollectedState();
    }

    void BounceState()
    {
        /* Motion */
        float t = Time.time * 4.0f + initial_position.x * 0.2f + initial_position.z * 0.2f + initial_position.y * 0.2f;

        Vector3 vert_pos = Vector3.up * Mathf.Abs(Mathf.Sin(t) * 0.5f);
        transform.position = initial_position + vert_pos;
        transform.Rotate(0, 1.0f, 0);

        /* Collection */
        Vector3 delta = PlayerController.GetPlayerPosition() - transform.position;
        if (delta.magnitude < 5 && delta.y >= -1.0f)
            current_state = CollectibleState.SEEKING;
    }

    float seeking_speed = 15.0f;
    void SeekingState()
    {
        // Collectibles will speed up over time to catch the player
        seeking_speed += Time.deltaTime * 2.0f;

        Vector3 delta = PlayerController.GetPlayerPosition() - transform.position;
        Vector3 direction = delta.normalized;

        float distance_to_move = seeking_speed * Time.deltaTime;

        if (delta.magnitude > 0.75f)
            transform.position += direction * seeking_speed * Time.deltaTime;
        else
        {
            current_state = CollectibleState.COLLECTED;

            // Use delta, not direction, to calculate entry_offset on the X-Z plane
            entry_offset = Mathf.Atan2(delta.z, delta.x) + Mathf.PI;

            /* Apply collection effect */
            if(currency_delta != 0)
            {
                UIManager.currency += currency_delta;
                ArborEventBus.Publish(new EventCurrencyChanged());
            }
            if (lives_delta != 0)
            {
                UIManager.ChangeLivesAmount(lives_delta);
                ArborEventBus.Publish(new EventLivesChanged());
            }
        }
    }

    float collected_progress = 0.0f;
    float entry_offset = 0.0f;
    const float speed_factor = 10.0f;
    void CollectedState()
    {
        collected_progress += Time.deltaTime;

        // Calculate the circular offset
        Vector3 circle_offset = new Vector3(
            Mathf.Cos(entry_offset + collected_progress * speed_factor),
            collected_progress * 2.0f, // Y-axis progression
            Mathf.Sin(entry_offset + collected_progress * speed_factor)
        );

        // Update position and scale
        transform.position = PlayerController.GetPlayerPosition() + circle_offset;
        transform.localScale = initial_scale * (1.0f - collected_progress);

        // Destroy the object when done
        if (collected_progress >= 1.0f)
            Destroy(gameObject);
    }
}
