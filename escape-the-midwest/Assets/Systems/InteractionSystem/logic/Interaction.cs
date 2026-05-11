using UnityEngine;
using System.Collections;

public abstract class Interaction : GameplayLogicComponent
{
    public override GameplayLogicComponentType GetGameplayLogicComponentType()
    {
        return GameplayLogicComponentType.INTERACTION;
    }

    public abstract bool IsInteractionBlocking();
    public abstract bool ShouldHideCanvasDuring();

    public IEnumerator OnInteract()
    {
        run_count++;

        /* ArborStartCoroutine is needed, as otherwise Interact() calls will fail on inactive gameobjects */
        /* This would prevent inactive gameobjects from running interactions to re-enable themselves. */
        /* Collectibles also tend to despawn a bit early as well. */
        yield return ArborCoroutineManager.ArborDoCoroutine(_OnInteract());
    }

    protected abstract IEnumerator _OnInteract();

    public abstract IEnumerator OnFinished();

    public abstract bool OnlyEverRunOnce();


    private int run_count = 0;
    public int GetRunCount() { return run_count; }
}
