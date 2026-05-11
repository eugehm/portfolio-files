using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraLookInteractionMode { MATCH_TARGET, LOOK_AT_TARGET };

public class InteractionCameraLook : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] Transform target;
    [SerializeField] CameraLookInteractionMode look_mode = CameraLookInteractionMode.MATCH_TARGET;
    [SerializeField] float duration_sec = 2.0f;
    [SerializeField] bool disable_player_controls_during = false;

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
        if(target != null)
        {
            Camera cam = target.GetComponent<Camera>();
            if(cam != null)
            {
                cam.enabled = false;
            }

            AudioListener listener = target.GetComponent<AudioListener>();
            if(listener != null)
            {
                listener.enabled = false;
            }
        }
    }

    protected override IEnumerator _OnInteract()
    {
        if (target == null)
        {
            Utilities.LogErrorForStudents("InteractionCameraLook", gameObject, "You forgot to provide a target to move to or look at.");
            yield break;
        }

        if (duration_sec <= 0)
        {
            Utilities.LogErrorForStudents("InteractionCameraLook", gameObject, "Your duration is 0 or less seconds.");
            yield break;
        }

        const string REQUEST_ID = "interaction_camera_look";

        if (disable_player_controls_during)
            PlayerController.RegisterStayPutRequest(REQUEST_ID);

        SpecialCameraRequest request = new SpecialCameraRequest()
        {
            position = target.position,
            direction = target.forward,
            ease_factor = 0.1f,
            id = REQUEST_ID
        };

        if(look_mode == CameraLookInteractionMode.LOOK_AT_TARGET)
        {
            Transform gameplay_cam_t = GameplayCamera.GetCameraTransform();
            if(gameplay_cam_t != null)
            {
                Vector3 direction = (target.position - gameplay_cam_t.position).normalized;
                request = new SpecialCameraRequest()
                {
                    position = gameplay_cam_t.position,
                    direction = direction,
                    ease_factor = 0.1f,
                    id = REQUEST_ID
                };
            }
        }

        GameplayCamera.AddSpecialRequest(request);

        yield return new WaitForSeconds(duration_sec);

        GameplayCamera.RemoveSpecialRequest(REQUEST_ID);

        if (disable_player_controls_during)
            PlayerController.UnregisterStayPutRequest(REQUEST_ID);
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
