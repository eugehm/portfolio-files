using System.Collections;
using UnityEngine;

public class InteractionCharacterSelect : Interaction
{
    [SerializeField] bool only_ever_run_once = false;

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once;
    }

    public override bool IsInteractionBlocking()
    {
        return true;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return true;
    }

    protected override IEnumerator _OnInteract()
    {
        StartCoroutine(CharacterSelectManager.DoCharacterSelection());

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
