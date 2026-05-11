using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractionStartTimer : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    public float timer_duration_sec = 60f;

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
        Timer.StartTimer(timer_duration_sec);
        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
