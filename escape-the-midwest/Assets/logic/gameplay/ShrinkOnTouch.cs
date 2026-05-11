using UnityEngine;

public enum ShrinkObjectState { NORMAL, SHRINKING, SHRUNK, GROWING };

public class ShrinkOnTouch : MonoBehaviour 
{
    public float delay_before_shrink_begins_sec = 0.35f;
    public float shrink_duration_sec = 2.0f;
    public float respawns_after_time_delay_sec = 20.0f;
    public float growing_duration_sec = 0.5f;
    public bool telegraph_via_twitching_visual_effect = true;

    Vector3 initial_scale = Vector3.one;
    ShrinkObjectState current_state = ShrinkObjectState.NORMAL;
    float shrink_begin_timestamp = 0.0f;
    float shrunk_begin_timestamp = 0.0f;
    float growing_begin_timestamp = 0.0f;

    // Tilting animation to telegraph shrinking behavior.
    bool tilted = false;
    Vector3 previous_tilt = Vector3.zero;
    float tilt_timer = 0.5f;

    Subscription<EventPlayerRespawned> sub_EventPlayerRespawned;

    private void Awake()
    {
        initial_scale = transform.localScale;

        sub_EventPlayerRespawned = ArborEventBus.Subscribe<EventPlayerRespawned>(OnEventPlayerRespawned);
    }

    private void Start()
    {
        initial_scale = transform.localScale;
    }

    void OnEventPlayerRespawned(EventPlayerRespawned e)
    {
        transform.localScale = initial_scale;
        current_state = ShrinkObjectState.NORMAL;

        /* Re-enable all colliders just-in-case */
        //Collider[] colliders = gameObject.GetComponents<Collider>();
        //foreach (Collider c in colliders)
        //    c.enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player == null)
            return;

        if (current_state == ShrinkObjectState.NORMAL)
        {
            current_state = ShrinkObjectState.SHRINKING;
            shrink_begin_timestamp = Time.time;
            initial_scale = transform.localScale;
        }
    }

    void Update()
    {
        if (current_state == ShrinkObjectState.NORMAL)
        {
            if (telegraph_via_twitching_visual_effect)
            {
                tilt_timer -= Time.deltaTime;
                if (tilt_timer <= 0.0f)
                {
                    tilt_timer = 0.05f + UnityEngine.Random.Range(-0.0f, 0.2f);

                    if (tilted)
                    {
                        // un-tilt ourselves.
                        transform.Rotate(-previous_tilt);
                        tilted = false;
                    }
                    else
                    {
                        // tilt ourselves to spook the player.
                        Vector3 random_tilt = UnityEngine.Random.onUnitSphere * 2;
                        transform.Rotate(random_tilt);
                        previous_tilt = random_tilt;
                        tilted = true;
                    }
                }
            }
        }

        else if (current_state == ShrinkObjectState.SHRINKING)
        {
            float time_since_shrink_begin = Time.time - shrink_begin_timestamp;
            
            if (time_since_shrink_begin < delay_before_shrink_begins_sec)
            {
                // Delay for a moment before we begin shrinking.
            }
            else
            {
                // Begin shrinking
                float progress = (time_since_shrink_begin - delay_before_shrink_begins_sec) / shrink_duration_sec;
                transform.localScale = Vector3.Lerp(initial_scale, Vector3.zero, progress);

                if (progress >= 1.0f)
                {
                    current_state = ShrinkObjectState.SHRUNK;
                    transform.localScale = Vector3.zero;
                    shrunk_begin_timestamp = Time.time;
                }
            }
        }
        
        else if (current_state == ShrinkObjectState.SHRUNK)
        {
            float progress = (Time.time - shrunk_begin_timestamp) / respawns_after_time_delay_sec;
            if (progress >= 1.0f)
            {
                current_state = ShrinkObjectState.GROWING;
                growing_begin_timestamp = Time.time;
            }
        }

        else if (current_state == ShrinkObjectState.GROWING)
        {
            float progress = (Time.time - growing_begin_timestamp) / growing_duration_sec;

            transform.localScale = Vector3.Lerp(Vector3.zero, initial_scale, progress);

            if (progress >= 1.0f)
            {
                current_state = ShrinkObjectState.NORMAL;
                transform.localScale = initial_scale;
            }
        }
    }

    private void OnDestroy()
    {
        if (sub_EventPlayerRespawned != null)
            ArborEventBus.Unsubscribe(sub_EventPlayerRespawned);
    }
}
