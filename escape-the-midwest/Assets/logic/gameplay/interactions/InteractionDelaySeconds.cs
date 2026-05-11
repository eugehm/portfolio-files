using System.Collections;
using UnityEngine;

public class InteractionDelaySeconds : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] bool blocking = true;
    [SerializeField] float seconds_to_delay = 3.0f;

    protected override IEnumerator _OnInteract()
    {
        yield return new WaitForSeconds(seconds_to_delay);
    }

    public override bool IsInteractionBlocking()
    {
        return blocking;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return false;
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
