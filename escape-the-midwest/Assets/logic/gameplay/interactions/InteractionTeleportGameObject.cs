using System.Collections;
using UnityEngine;

public class InteractionTeleportGameObject : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] bool should_move_player_gameobject = false;
    [SerializeField] GameObject thing_to_move;
    [SerializeField] Transform destination_marker;
    [SerializeField] float gradual_move_duration_sec = 0.0f;
    [SerializeField] bool disable_player_movement_during = false;
    [SerializeField] bool should_flash = false;
    [SerializeField] bool should_darken = false;
    [SerializeField] float stay_dark_pause_duration_sec = 0.0f;

    private void Start()
    {
        if (thing_to_move == null && !should_move_player_gameobject)
            Utilities.LogErrorForStudents("InteractionTeleportGameobject", gameObject, "You forgot to tell me which gameobject I should be moving!");

        if (destination_marker == null)
            Utilities.LogErrorForStudents("InteractionTeleportGameobject", gameObject, "You forgot to tell me where I should move the gameobject!");
    }

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
        if (should_move_player_gameobject)
            thing_to_move = PlayerController.GetPlayerGameobject();

        if (thing_to_move == null)
            yield break;

        if (destination_marker == null)
            yield break;

        if (should_flash)
            TransitionManager.RequestFlash();

        /* Intro */
        if(disable_player_movement_during)
            PlayerController.RegisterStayPutRequest("interaction_teleport");

        if (should_darken)
        {
            TransitionManager.RequestDarken(true);
            yield return new WaitForSeconds(1);
        }

        // Do the move
        if(gradual_move_duration_sec > 0)
        {
            Vector3 starting_position = thing_to_move.transform.position;
            Quaternion starting_rotation = thing_to_move.transform.rotation;
            float start_time = Time.time;
            float progress = (Time.time - start_time) / gradual_move_duration_sec;
            while (progress < 1.0f)
            {
                progress = (Time.time - start_time) / gradual_move_duration_sec;

                thing_to_move.transform.position = Vector3.LerpUnclamped(starting_position, destination_marker.position, progress);
                thing_to_move.transform.rotation = Quaternion.Slerp(starting_rotation, destination_marker.rotation, progress);

                yield return null;
            }
        }
        
        thing_to_move.transform.position = destination_marker.position;
        thing_to_move.transform.rotation = destination_marker.rotation;

        if(should_darken)
            yield return new WaitForSeconds(stay_dark_pause_duration_sec);

        /* Outro */
        if (should_darken)
        {
            ArborEventBus.Publish(new EventGameplayCameraWarpRequest());
            TransitionManager.RequestDarken(false);
            yield return new WaitForSeconds(1);
        }

        if (disable_player_movement_during)
            PlayerController.UnregisterStayPutRequest("interaction_teleport");

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
