using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InteractionDialogue : Interaction
{
    static int number_of_dialogues_running = 0;
    public static bool IsAnyDialogueRunning() { return number_of_dialogues_running > 0; }


    [SerializeField] bool only_ever_run_once = false;

    [SerializeField] bool look_towards_player_when_speaking = true;
    [SerializeField] Camera forced_camera = null;
    [SerializeField] List<string> dialogue_lines = new List<string>();

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

    private void Awake()
    {
        if (forced_camera != null)
        {
            forced_camera.enabled = false;
            if (forced_camera.GetComponent<AudioListener>() != null)
                Destroy(forced_camera.GetComponent<AudioListener>());
        }
    }

    private void Update()
    {
        if (look_towards_player_when_speaking == false)
            return;

        float distance = Vector3.Distance(PlayerController.GetPlayerPosition(), transform.position);
        if (distance > 7)
            return;

        Quaternion desired_rotation = new Quaternion();
        Vector3 towards_player = (PlayerController.GetPlayerPosition() - transform.position).normalized;
        towards_player = new Vector3(towards_player.x, 0.0f, towards_player.z);
        desired_rotation.SetLookRotation(towards_player, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desired_rotation, 0.1f);
    }

    protected override IEnumerator _OnInteract()
    {
        if(dialogue_lines.Count <= 0)
        {
            Utilities.LogErrorForStudents("InteractionDialogue", gameObject, "You forgot to give me any lines.");
            yield break;
        }

        number_of_dialogues_running++;
        PlayerController.RegisterStayPutRequest("interaction_dialogue");

        Vector3 object_to_player = PlayerController.GetPlayerPosition() - transform.position;
        Vector3 direction = -(Vector3.Cross(object_to_player.normalized, Vector3.up));
        Vector3 cam_pos = object_to_player * 0.5f + transform.position + direction * -5.0f + Vector3.up;

        if(forced_camera != null)
        {
            cam_pos = forced_camera.transform.position;
            direction = forced_camera.transform.forward;
        }

        SpecialCameraRequest request = new SpecialCameraRequest()
        {
            position = cam_pos,
            direction = direction,
            ease_factor = 0.1f,
            id = "dialogue_cam"
        };
        GameplayCamera.AddSpecialRequest(request);

        if (forced_camera != null)
            ArborEventBus.Publish(new EventGameplayCameraWarpRequest());

        /* Look towards player */
        if(look_towards_player_when_speaking)
        {
            Quaternion desired_rotation = new Quaternion();
            Vector3 towards_player = (PlayerController.GetPlayerPosition() - transform.position).normalized;
            towards_player = new Vector3(towards_player.x, 0.0f, towards_player.z);
            desired_rotation.SetLookRotation(towards_player, Vector3.zero);
            float start_time = Time.time;
            float progress = (Time.time - start_time) / 1.0f;
            while(progress <= 1.0f)
            {
                progress = (Time.time - start_time) / 1.0f;
                
                transform.rotation = Quaternion.Slerp(transform.rotation, desired_rotation, progress);
                yield return null;
            }
            transform.rotation = desired_rotation;
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }


        int index = 0;
        foreach(string dialogue_line in dialogue_lines)
        {
            ToastManager.RequestToast($"{GlobalVariablesSystem.ConvertVariablesInString(dialogue_line)}", ToastType.NORMAL, dialogue_line_visibility_check);
            yield return StartCoroutine(Utilities.WaitForMouseClickSpacebarOrEnter());
            index++;
        }

        GameplayCamera.RemoveSpecialRequest("dialogue_cam");

        PlayerController.UnregisterStayPutRequest("interaction_dialogue");
        number_of_dialogues_running--;

        yield return null;
    }

    bool dialogue_line_visibility_check(GameObject g)
    {
        return IsAnyDialogueRunning() == false;
    }

    public override IEnumerator OnFinished()
    {
        yield return null;
    }
}
