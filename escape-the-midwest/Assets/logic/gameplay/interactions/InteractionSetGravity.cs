using System.Collections;
using UnityEngine;

public class InteractionSetGravity : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] Vector3 new_gravity = new Vector3(0, -4.5f, 0);

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once;
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
        Physics.gravity = new_gravity;

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
