using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionChangeActive : Interaction
{
    public enum InteractionChangeActiveOptions { DISABLE_THE_OBJECT, ENABLE_THE_OBJECT };

    [SerializeField] bool only_ever_run_once_pls = false;
    [SerializeField] GameObject thing_to_change;
    [SerializeField] List<GameObject> more_things_to_change;
    [SerializeField] InteractionChangeActiveOptions choice;

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once_pls;
    }

    public override bool IsInteractionBlocking()
    {
        return false;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return false;
    }

    private void Start()
    {
        if (thing_to_change == null && more_things_to_change.Count <= 0)
            Utilities.LogErrorForStudents("InteractionChangeActive", gameObject, "You forgot to specify which gameobject should be affected by the InteractionChangeActive component.");
    }

    protected override IEnumerator _OnInteract()
    {
        bool should_be_active = false;
        if (choice == InteractionChangeActiveOptions.ENABLE_THE_OBJECT)
            should_be_active = true;

        if(thing_to_change != null)
            thing_to_change.SetActive(should_be_active);

        foreach (GameObject thing in more_things_to_change)
        {
            if(thing != null)
                thing.SetActive(should_be_active);
        }

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
