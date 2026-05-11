using UnityEngine;
using System;

public delegate bool DelegateBoolReturnGameObjectParameter(GameObject g);

public class ToastText : MonoBehaviour
{
    [SerializeField] RectTransform rt;
    CanvasGroup cg;
    public float lifetime_seconds = 5.0f;
    float lifetime_remaining_seconds;
    public DelegateBoolReturnGameObjectParameter visibility_check = null;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        BumpAndResetLifetime();
    }

    void Update()
    {
        /* Nice bouncy scale effect */
        HookesScale();

        /* Check where we are in the Toast queue to figure out our desired position */
        int index = ToastManager.GetIndexInQueue(gameObject);
        Vector2 desired_anchored_position = new Vector2(-50, 50 + index * 100);
        rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, desired_anchored_position, 0.2f);

        /* Fade over time */
        if(visibility_check != null)
        {
            if(visibility_check(gameObject))
                lifetime_remaining_seconds -= Time.deltaTime;
        }
        else
        {
            lifetime_remaining_seconds -= Time.deltaTime;
        }

        if (lifetime_remaining_seconds < 2.0f)
        {
            cg.alpha = lifetime_remaining_seconds / 2.0f;
        }

        /* Remove from queue and destroy once fully faded */
        if (lifetime_remaining_seconds <= 0.0f)
        {
            ToastManager.RemoveFromQueueAndDestroy(gameObject);
            return;
        }
    }

    float scale_vel = 0.0f;
    public float stiffness = 0.1f;
    public float dampening_factor = 0.9f;
    void HookesScale()
    {
        float delta = 1.0f - rt.localScale.y;
        float a = delta * stiffness;
        scale_vel += a;
        scale_vel *= dampening_factor;
        rt.localScale += new Vector3(scale_vel, scale_vel, 0);
    }

    public void BumpAndResetLifetime()
    {
        rt.localScale = new Vector3(0, 0);
        scale_vel = 0.0f;
        lifetime_remaining_seconds = lifetime_seconds;
        cg.alpha = 1.0f;
    }
}
