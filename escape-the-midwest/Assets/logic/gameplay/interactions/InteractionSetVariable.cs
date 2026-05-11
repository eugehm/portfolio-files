using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractionSetVariable : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] string variable_name = "variable_name";
    [SerializeField] string variable_value = "";

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
        GlobalVariablesSystem.SetVariable(variable_name, variable_value);
        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
