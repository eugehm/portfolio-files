using System.Collections;
using UnityEngine;

public class InteractionPush : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] float horizontal_power = 100.0f;
    [SerializeField] float vertical_power = 100.0f;

    public override bool IsInteractionBlocking()
    {
        return false;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return false;
    }

    protected override IEnumerator _OnInteract()
    {
        Vector3 xz_direction = transform.position - PlayerController.GetPlayerPosition();
        xz_direction = new Vector3(xz_direction.x, 0, xz_direction.z);

        /* If a rigidbody is missing, add one */
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;

        /* Force calculation */
        Vector3 final_force = xz_direction * horizontal_power + Vector3.up * vertical_power;
        rb.AddForce(final_force);

        yield break;   
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once;
    }
}
