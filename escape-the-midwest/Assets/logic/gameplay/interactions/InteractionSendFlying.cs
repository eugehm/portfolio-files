using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InteractionSendFlying : Interaction
{
    [SerializeField] bool only_ever_run_once_pls = false;
    [SerializeField] bool thing_to_send_flying_is_player = false;
    [SerializeField] GameObject thing_to_send_flying;
    [SerializeField] bool add_gravity = true;
    [SerializeField] Vector3 launch_direction = new Vector3(0,1,0);
    [SerializeField] float launch_power = 5.0f;
    [SerializeField] Vector3 angular_velocity = new Vector3(0, 0, 0);
    [SerializeField] bool remove_all_colliders = false;

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once_pls;
    }

    public override bool IsInteractionBlocking()
    {
        return false;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return false;
    }

    private void Start()
    {
        if (thing_to_send_flying_is_player == false && thing_to_send_flying == null)
        {
            Utilities.LogErrorForStudents("InteractionSendFlying", gameObject, "You forgot to specify the thing to send flying.");
            return;
        }
    }

    protected override IEnumerator _OnInteract()
    {
        if (thing_to_send_flying_is_player)
            thing_to_send_flying = PlayerController.GetPlayerGameobject();

        if (thing_to_send_flying == null)
        {
            yield break;
        }

        if(thing_to_send_flying_is_player == false)
        {
            /* Remove all existing rigidbodies */
            foreach (Rigidbody existing_rb in thing_to_send_flying.GetComponents<Rigidbody>())
            {
                Destroy(existing_rb);
            }

            /* Remove colliders if desired */
            if (remove_all_colliders)
            {
                foreach (Collider c in thing_to_send_flying.GetComponents<Collider>())
                {
                    Destroy(c);
                }
            }

            yield return null; // Must let rigidbodies and colliders be cleaned up before adding a fresh one.
            Rigidbody new_rb = thing_to_send_flying.AddComponent<Rigidbody>();
        }


        /* Add a new rigidbody */
        Rigidbody rb = thing_to_send_flying.GetComponent<Rigidbody>();
        rb.useGravity = add_gravity;
        rb.linearVelocity = launch_direction.normalized * launch_power;
        rb.angularVelocity = angular_velocity;

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
