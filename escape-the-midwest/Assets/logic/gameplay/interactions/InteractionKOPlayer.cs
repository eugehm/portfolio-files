using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InteractionKOPlayer : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] bool falling_ko = false;

    protected override IEnumerator _OnInteract()
    {
        KO_CAUSE cause = KO_CAUSE.NORMAL;
        if (falling_ko)
            cause = KO_CAUSE.FALLING;
        ArborEventBus.Publish(new EventPlayerShouldKO(cause));
        yield break;
    }

    public override bool IsInteractionBlocking()
    {
        return true;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return true;
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
