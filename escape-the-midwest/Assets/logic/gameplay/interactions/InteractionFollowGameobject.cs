using System.Collections;
using UnityEngine;

public class InteractionFollowGameobject : Interaction
{
    [SerializeField] bool only_ever_run_once = false;

    [SerializeField] GameObject gameobject_doing_the_following;
    [SerializeField] bool follower_gameobject_is_player = false;

    [SerializeField] GameObject gameobject_to_follow;
    [SerializeField] bool gameobject_to_follow_is_player = false;

    [SerializeField] float following_speed = 6.0f;
    [SerializeField] float back_off_distance = 2.0f;
    [SerializeField] bool look_in_xz_movement_direction = true;

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
        if (gameobject_doing_the_following == null && follower_gameobject_is_player == false)
            Utilities.LogErrorForStudents("InteractionFollowGameobject", gameObject, "You forgot to specify the follower gameobject.");

        if (gameobject_to_follow == null && gameobject_to_follow_is_player == false)
            Utilities.LogErrorForStudents("InteractionFollowGameobject", gameObject, "You forgot to specify the gameobject to follow.");
    }

    protected override IEnumerator _OnInteract()
    {
        if (follower_gameobject_is_player)
            gameobject_doing_the_following = PlayerController.GetPlayerGameobject();
        if (gameobject_to_follow_is_player)
            gameobject_to_follow = PlayerController.GetPlayerGameobject();

        if (gameobject_doing_the_following == null)
            yield break;

        if (gameobject_to_follow == null)
            yield break;

        /* Make sure the follower has no FollowTarget components already */
        FollowTargetLinear[] fts = gameobject_doing_the_following.GetComponents<FollowTargetLinear>();
        foreach (FollowTargetLinear f in fts)
            Destroy(f);

        /* Give the follower a follow target component */
        FollowTargetLinear ft = gameobject_doing_the_following.AddComponent<FollowTargetLinear>();
        ft.back_off_distance = back_off_distance;
        ft.following_speed = following_speed;
        ft.gameobject_to_follow = gameobject_to_follow;
        ft.look_in_xz_movement_direction = look_in_xz_movement_direction;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
