using System.Collections;
using UnityEngine;

public class InteractionVictory : Interaction
{
    [SerializeField] bool only_ever_run_once_pls = false;

    public override bool IsInteractionBlocking()
    {
        return true;
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
        ArborEventBus.Publish(new EventPlayerVictory());

        yield return new WaitForSeconds(1);
        while (PlayerController.GetCurrentState() == "victory")
            yield return null;

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
