using System.Collections;
using UnityEngine;

public class InteractionStopFollowing : Interaction
{
    [SerializeField] bool only_ever_run_once = false;

    [SerializeField] GameObject gameobject_that_should_stop_following;
    [SerializeField] bool gameobject_that_should_stop_following_is_player = false;

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

    private void Start()
    {
        if (gameobject_that_should_stop_following == null && gameobject_that_should_stop_following_is_player == false)
            Utilities.LogErrorForStudents("InteractionStopFollowing", gameObject, "You forgot to specify the gameobject that should stop following.");
    }

    protected override IEnumerator _OnInteract()
    {
        if (gameobject_that_should_stop_following_is_player)
            gameobject_that_should_stop_following = PlayerController.GetPlayerGameobject();
       
        if (gameobject_that_should_stop_following == null)
            yield break;

        FollowTargetLinear[] fts = gameobject_that_should_stop_following.GetComponents<FollowTargetLinear>();
        foreach (FollowTargetLinear f in fts)
            Destroy(f);
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
