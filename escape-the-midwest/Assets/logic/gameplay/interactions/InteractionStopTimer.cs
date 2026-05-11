using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractionStopTimer : Interaction
{
    [SerializeField] bool only_ever_run_once = false;

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
        Timer.StopTimer();
        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
