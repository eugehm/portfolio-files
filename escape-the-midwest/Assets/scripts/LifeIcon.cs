using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeIcon : MonoBehaviour
{
    public int index = 0;

    Vector3 initial_scale;
    Vector3 initial_pos;

    bool hidden = true;

    void Start()
    {
        initial_pos = transform.position;
        initial_scale = transform.localScale;

        transform.localScale = Vector3.zero;
    }

    public void BecomeVisible()
    {
        hidden = false;
    }

    bool performing_outro = false;
    public IEnumerator DoOutroAnimation()
    {
        if (performing_outro)
            yield break;
        performing_outro = true;

        float scale_step = transform.localScale.x;

        while (transform.localScale.x > 0)
        {
            transform.localScale -= Time.unscaledDeltaTime * scale_step * Vector3.one;
            yield return null;
        }

        Destroy(gameObject);
    }

    void Update()
    {
        float t_offset_by_index = Time.time + index * 0.4f;

        // position
        Vector3 desired_pos = initial_pos + Vector3.up * 0.1f * Mathf.Sin(t_offset_by_index * 1.5f);
        transform.position = desired_pos;

        // rotation
        Quaternion desired_rotation = Quaternion.identity;
        desired_rotation.eulerAngles = new Vector3(0, -120 - Mathf.Sin(t_offset_by_index * 0.5f) * 30, 0);
        transform.rotation = desired_rotation;

        // scale
        if (performing_outro == false)
            HookesScale();
    }

    float velocity = 0.0f;
    float k = 0.05f;
    float dampening_factor = 0.92f;

    void HookesScale()
    {
        if (WebManager.IsTabCurrentlyActive() == false || Application.isFocused == false)
            return;

        float desired_scale = initial_scale.x;
        if (hidden)
            desired_scale = 0.0f;

        float delta = desired_scale - transform.localScale.x;
        float a = delta * k;
        velocity += a * Time.deltaTime * 60.0f; // Scale acceleration by deltaTime
        velocity *= Mathf.Pow(dampening_factor, Time.deltaTime * 60.0f); // Adjust dampening factor for deltaTime

        transform.localScale += velocity * Vector3.one * Time.deltaTime * 60.0f; // Scale velocity by deltaTime
    }
}
