/* Provide a network player with the Animator Parameters needed to function */
/* This component will replace the ProvideAnimatorParameters component on network players */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvideAnimatorParametersNetwork : MonoBehaviour
{
    Animator anim;
    Vector3 previous_position;

    public NetworkPlayerView network_player_info = null;

    void Start()
    {
        /* Disable any extra cameras that might be on the model */
        HashSet<Camera> cameras = new HashSet<Camera>();
        Utilities.GetComponentsInDescendents(transform, ref cameras);
        foreach (Camera c in cameras)
            c.enabled = false;

        anim = GetComponent<Animator>();
        previous_position = transform.position;
    }

    void Update()
    {
        LookForNetworkPlayerInfo();
        ProvideVariablesToAnimator();
    }

    void LookForNetworkPlayerInfo()
    {
        if (network_player_info != null)
            return;

        if (transform.parent == null)
            return;

        HasCharacterView hcv = transform.parent.GetComponent<HasCharacterView>();
        if (hcv == null)
            return;

        network_player_info = hcv.network_player_info;
    }

    float time_at_idle_vertical_speed = 0.0f;
    void ProvideVariablesToAnimator()
    {
        /* The values of required animator parameters cannot be known exactly for network players. */
        /* Fortunately, we can estimate these parameters by observing how the network player moves over time. */
        Vector3 movement_delta = transform.position - previous_position;
        float running_input = movement_delta.magnitude * 5.0f;
        float vertical_movement = movement_delta.y * 10.0f;
        float abs_vertical_movement = Mathf.Abs(vertical_movement);

        /* Deciding if the player is on the ground is tricky without performing raycasts */
        /* We can approximate this status based on the player's vertical movement, and how long it remains near zero. */
        bool on_ground = false;
        if (abs_vertical_movement <= 0.1f)
            time_at_idle_vertical_speed -= Time.deltaTime;
        else
            time_at_idle_vertical_speed = 0.15f;

        if (time_at_idle_vertical_speed <= 0.0f)
            on_ground = true;

        /* Communicate parameter values with Animator */
        anim.SetFloat("running_input", running_input);
        anim.SetFloat("vertical_speed", vertical_movement);
        anim.SetFloat("absolute_vertical_speed", abs_vertical_movement);
        anim.SetBool("on_ground", on_ground);

        string current_player_state = "";
        if(network_player_info != null)
        {
            current_player_state = network_player_info.GetNetworkPlayerState();
        }
        anim.SetBool("game_over", current_player_state == "game_over");
        anim.SetBool("continue", current_player_state == "continue");
        anim.SetBool("retry", current_player_state == "retry");
        anim.SetBool("ko", current_player_state == "ko");
        anim.SetBool("falling_ko", current_player_state == "falling_ko");
        anim.SetBool("victory", current_player_state == "victory");
    }

    private void LateUpdate()
    {
        previous_position = transform.position;
    }
}
