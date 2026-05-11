using UnityEngine;

public class ScreenShakeManager : MonoBehaviour
{
    static ScreenShakeManager instance;

    [SerializeField] float shake_magnitude = 6f;
    [SerializeField] float shake_interval_sec = 0.5f;
    [SerializeField] bool should_shake = false;
    float time_until_next_shake = 0;

    public static void StartShaking(float magnitude, float interval_sec)
    {
        instance.shake_magnitude = magnitude;
        instance.shake_interval_sec = interval_sec;
        instance.time_until_next_shake = 0;
        instance.should_shake = true;
    }

    public static void StopShaking()
    {
        instance.should_shake = false;
    }

    Transform inner_cam_t;

    // Meant to address a common camera-related issue on lower-framerate systems.
    // The issue manifests in a gameplay camera oscillating wildly on game start until eventually settling down.
    // To address the issue, this "hack" forces the inner camera to remain at 0,0,0 for a brief period at the beginning of gameplay.
    float gameplay_start_calming_timer = 1.0f;
    void Start()
    {
        instance = this;
        should_shake = false;
        inner_cam_t = transform.Find("gameplay_cam");
        inner_cam_t.localPosition = Vector3.zero;
    }

    void Bump()
    {
        velocity += UnityEngine.Random.insideUnitSphere * shake_magnitude;
    }

    void Update()
    {
        if(gameplay_start_calming_timer > 0.0f)
        {
            inner_cam_t.localPosition = Vector3.zero;
            gameplay_start_calming_timer -= Time.deltaTime;
            return;
        }

        if(should_shake)
        {
            time_until_next_shake -= Time.deltaTime;
            if (time_until_next_shake <= 0)
            {
                Bump();
                time_until_next_shake = shake_interval_sec + UnityEngine.Random.Range(0.0f, shake_interval_sec);
            }
        }

        HookesPosition();
    }

    Vector3 velocity = Vector3.zero;
    [SerializeField] float stiffness = 100.0f;
    [SerializeField] float dampening_factor = 0.2f;
    void HookesPosition()
    {
        Vector3 delta = -inner_cam_t.localPosition;
        Vector3 a = stiffness * delta;
        velocity += a * Time.deltaTime;
        velocity *= Mathf.Pow(dampening_factor, Time.deltaTime);

        inner_cam_t.localPosition += velocity * Time.deltaTime;
    }
}
