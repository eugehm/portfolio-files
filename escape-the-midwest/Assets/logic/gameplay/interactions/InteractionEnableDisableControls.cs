using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnableDisableMode { ENABLE, DISABLE };

public class InteractionEnableDisableControls : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] EnableDisableMode mode = EnableDisableMode.ENABLE; // Remember to re-enable later if you disable them now!
    [SerializeField] string request_id = "enable_disable_interaction";

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
        if (request_id == null || request_id.Trim() == "")
        {
            Utilities.LogErrorForStudents("InteractionEnableDisableControls", gameObject, "You forgot to provide a request_id. This is used to register or unregister stay-put requests for the player controller. More than one system may be requesting the player stay put.");
            yield break;
        }

        if(mode == EnableDisableMode.DISABLE)
            PlayerController.RegisterStayPutRequest(request_id);
        else if (mode == EnableDisableMode.ENABLE)
            PlayerController.UnregisterStayPutRequest(request_id);
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
