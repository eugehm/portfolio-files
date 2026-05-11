/* Feed important parameters to the students' animator / animation controller */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProvideAnimatorParameters : MonoBehaviour
{
    Animator anim;
    Rigidbody controller_rb;

    void Start()
    {
        /* Disable any extra cameras that might be on the model */
        HashSet<Camera> cameras = new HashSet<Camera>();
        Utilities.GetComponentsInDescendents(transform, ref cameras);
        foreach (Camera c in cameras)
            c.enabled = false;

        anim = GetComponent<Animator>();
        if (anim == null)
            Utilities.LogErrorForStudents("PlayableCharacterAnimations", gameObject, "I tried to find the Animator component on my gameobject, but I could not. Please set up an animator component. Read this guide : http://bit.ly/4nTM0Gj");

        controller_rb = transform.parent.GetComponent<Rigidbody>();

        CheckAnimatorVariables();
    }

    void CheckAnimatorVariables()
    {
        /* For students' sake, let them know if they've forgotten any required parameters */
        if (anim == null)
            anim = GetComponent<Animator>();

        List<RequiredAnimationControllerParameter> required_animator_variables = new List<RequiredAnimationControllerParameter>() 
        { 
            new RequiredAnimationControllerParameter("running_input", AnimatorControllerParameterType.Float),
            new RequiredAnimationControllerParameter("vertical_speed", AnimatorControllerParameterType.Float),
            new RequiredAnimationControllerParameter("absolute_vertical_speed", AnimatorControllerParameterType.Float),
            new RequiredAnimationControllerParameter("on_ground", AnimatorControllerParameterType.Bool),
            new RequiredAnimationControllerParameter("ko", AnimatorControllerParameterType.Bool),
            new RequiredAnimationControllerParameter("falling_ko", AnimatorControllerParameterType.Bool),
            new RequiredAnimationControllerParameter("continue", AnimatorControllerParameterType.Bool),
            new RequiredAnimationControllerParameter("game_over", AnimatorControllerParameterType.Bool),
            new RequiredAnimationControllerParameter("in_character_select", AnimatorControllerParameterType.Bool),
            new RequiredAnimationControllerParameter("hovered_in_character_select", AnimatorControllerParameterType.Bool),
            new RequiredAnimationControllerParameter("victory", AnimatorControllerParameterType.Bool),
            new RequiredAnimationControllerParameter("retry", AnimatorControllerParameterType.Bool)
        };

        List<RequiredAnimationControllerParameter> found_variables = new List<RequiredAnimationControllerParameter>();

        foreach (var parameter in anim.parameters)
        {
            foreach(RequiredAnimationControllerParameter required_var in required_animator_variables)
            {
                if(required_var.name.Equals(parameter.name) && required_var.type.Equals(parameter.type))
                {
                    found_variables.Add(required_var);
                }
            }
        }

        foreach(RequiredAnimationControllerParameter found_var in found_variables)
        {
            required_animator_variables.Remove(found_var);
        }

        foreach(RequiredAnimationControllerParameter missed_parameter in required_animator_variables)
        {
            Utilities.LogErrorForStudents("PlayableCharacterAnimations", gameObject, $"I found the animator component, but it is missing required parameter / variable {missed_parameter.name} of type {missed_parameter.type}. Please add it.");
        }
    }

    void Update()
    {
        Vector3 input = PlayerController.GetPlayerInput();
        if (PlayerController.StayPutRequestExists())
            input = Vector3.zero;

        ContinueSequenceStatus continue_sequence_status = GameOverCam.GetStatus();
        string current_player_state = PlayerController.GetCurrentState();

        anim.SetBool("game_over", continue_sequence_status == ContinueSequenceStatus.GAME_OVER);
        anim.SetBool("continue", continue_sequence_status == ContinueSequenceStatus.CONTINUE_COUNTDOWN);
        anim.SetBool("retry", continue_sequence_status == ContinueSequenceStatus.RETRY);
        anim.SetBool("ko", current_player_state == "ko");
        anim.SetBool("falling_ko", current_player_state == "falling_ko");
        anim.SetBool("victory", current_player_state == "victory");
        anim.SetFloat("running_input", input.magnitude);
        anim.SetFloat("vertical_speed", controller_rb.linearVelocity.y);
        anim.SetFloat("absolute_vertical_speed", Mathf.Abs(controller_rb.linearVelocity.y));
        anim.SetBool("on_ground", PlayerController.on_ground);
    }
}

public class RequiredAnimationControllerParameter
{
    public string name;
    public AnimatorControllerParameterType type;

    public RequiredAnimationControllerParameter(string _name, AnimatorControllerParameterType _type)
    {
        name = _name;
        type = _type;
    }
}