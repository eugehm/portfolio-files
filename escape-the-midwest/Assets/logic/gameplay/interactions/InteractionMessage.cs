using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractionMessage : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] List<string> messages = new List<string>();
    public float delay_after_message_seconds = 1.5f;

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
        if (messages.Count <= 0)
        {
            Utilities.LogErrorForStudents("InteractionMessage", gameObject, "You forgot to provide messages.");
            yield break;
        }

        foreach(string message in messages)
        {
            ToastManager.RequestToast(GlobalVariablesSystem.ConvertVariablesInString(message), ToastType.NORMAL);
            yield return new WaitForSeconds(delay_after_message_seconds);
        }
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
