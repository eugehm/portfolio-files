using System.Collections;
using UnityEngine;

public class InteractionStopScreenShake : Interaction
{
    [SerializeField] bool only_ever_run_once_pls = false;

    public override bool IsInteractionBlocking()
    {
        return false;
    }

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once_pls;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return false;
    }

    protected override IEnumerator _OnInteract()
    {
        ScreenShakeManager.StopShaking();

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
