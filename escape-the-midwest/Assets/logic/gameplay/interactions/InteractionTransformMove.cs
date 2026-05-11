using System.Collections;
using UnityEngine;

public class InteractionTransformMove : Interaction
{
    [SerializeField] bool only_ever_execute_once = false;
    [SerializeField] GameObject thing_to_move = null;
    [SerializeField] Vector3 movement = Vector3.right;
    [SerializeField] bool multiply_by_deltatime = false; // for framerate independence on every-frame movement.
    [SerializeField] bool use_localposition = false;

    /* Limits */
    [SerializeField] bool limit_x_axis = false;
    [SerializeField] float max_x = 0;
    [SerializeField] float min_x = 0;

    [SerializeField] bool limit_y_axis = false;
    [SerializeField] float max_y = 0;
    [SerializeField] float min_y = 0;

    [SerializeField] bool limit_z_axis = false;
    [SerializeField] float max_z = 0;
    [SerializeField] float min_z = 0;

    private void Start()
    {
        if (thing_to_move == null)
            Utilities.LogErrorForStudents("InteractionTransformMove", gameObject, "You forgot to specify which gameobject we're moving.");
    }

    public override bool OnlyEverRunOnce()
    {
        return only_ever_execute_once;
    }

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
        if (thing_to_move == null)
            yield break;

        Vector3 amount_to_move = movement;
        if (multiply_by_deltatime)
            amount_to_move *= Time.deltaTime;

        if(use_localposition)
            thing_to_move.transform.localPosition += amount_to_move;
        else
            thing_to_move.transform.position += amount_to_move;

        /* Enforce limits */
        if(limit_x_axis)
        {
            if (use_localposition)
                thing_to_move.transform.localPosition = new Vector3(Mathf.Clamp(thing_to_move.transform.localPosition.x, min_x, max_x), thing_to_move.transform.localPosition.y, thing_to_move.transform.localPosition.z);
            else
                thing_to_move.transform.position = new Vector3(Mathf.Clamp(thing_to_move.transform.position.x, min_x, max_x), thing_to_move.transform.position.y, thing_to_move.transform.position.z);
        }

        if (limit_y_axis)
        {
            if (use_localposition)
                thing_to_move.transform.localPosition = new Vector3(thing_to_move.transform.localPosition.x, Mathf.Clamp(thing_to_move.transform.localPosition.y, min_y, max_y), thing_to_move.transform.localPosition.z);
            else
                thing_to_move.transform.position = new Vector3(thing_to_move.transform.position.x, Mathf.Clamp(thing_to_move.transform.position.y, min_y, max_y), thing_to_move.transform.position.z);
        }

        if(limit_z_axis)
        {
            if (use_localposition)
                thing_to_move.transform.localPosition = new Vector3(thing_to_move.transform.localPosition.x, thing_to_move.transform.localPosition.y, Mathf.Clamp(thing_to_move.transform.localPosition.z, min_z, max_z));
            else
                thing_to_move.transform.position = new Vector3(thing_to_move.transform.position.x, thing_to_move.transform.position.y, Mathf.Clamp(thing_to_move.transform.position.z, min_z, max_z));
        }
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
