using UnityEngine;
using System;
using System.Collections.Generic;

public enum InteractibleGameobjectType { CLICK_TO_INTERACT, TOUCH_TO_INTERACT, APPROACH_TO_INTERACT, PLAYER_RESPAWN_TO_INTERACT, EVERY_TICK_INTERACT, INTERACT_ON_START_GAME, INTERACT_ON_VARIABLE_CHECK }

[DisallowMultipleComponent]
public class InteractableGameobject : MonoBehaviour
{
    static List<InteractableGameobject> click_to_interact_gameobjects = new List<InteractableGameobject>();
    public static List<InteractableGameobject> GetAllClickToInteractGameobjects() { return click_to_interact_gameobjects; }
    public static InteractableGameobject GetInteractableGameobjectNearestPoint(Vector3 point, float max_distance)
    {
        float min_d = float.MaxValue;
        InteractableGameobject nearest = null;
        foreach(InteractableGameobject go in click_to_interact_gameobjects)
        {
            if (go == null)
                continue;
            float d = Vector3.Distance(go.transform.position, point);
            if(d < max_distance && d < min_d)
            {
                min_d = d;
                nearest = go;
            }
        }

        return nearest;
    }

    [SerializeField] InteractibleGameobjectType interaction_type;
    [SerializeField] string touchable_tags_comma_separated = "Player,";
    [SerializeField] bool allow_further_self_interactions_before_finished = false;
    public bool AllowsFurtherSelfInteractionsBeforeFinished()
    {
        return allow_further_self_interactions_before_finished;
    }

    Subscription<EventPlayerRespawned> sub_EventPlayerRespawned;

    void Awake()
    {
        if (interaction_type == InteractibleGameobjectType.CLICK_TO_INTERACT)
            click_to_interact_gameobjects.Add(this);

        if (interaction_type == InteractibleGameobjectType.PLAYER_RESPAWN_TO_INTERACT)
        {
            sub_EventPlayerRespawned = ArborEventBus.Subscribe<EventPlayerRespawned>(OnEventPlayerRespawned);
        }

        RunChecks();
    }

    private void Start()
    {
        variable_name = variable_name.ToLowerInvariant().Trim();
        variable_value_needed = variable_value_needed.ToLowerInvariant().Trim();

        if (interaction_type == InteractibleGameobjectType.INTERACT_ON_START_GAME)
        {
            InteractionSystem.ExecuteInteractionsOnGameobject(this);
        }
    }

    [SerializeField] string variable_name = "variable_name";
    [SerializeField] string variable_value_needed = "variable_value";
    [SerializeField] bool perform_every_frame_variable_succeeds = false;
    bool currently_succeeding = false;
    private void Update()
    {
        PerformUpdateLogic();
    }

    void PerformUpdateLogic()
    {
        if (interaction_type == InteractibleGameobjectType.EVERY_TICK_INTERACT)
        {
            InteractionSystem.ExecuteInteractionsOnGameobject(this);
        }

        /* Check variable */
        else if (interaction_type == InteractibleGameobjectType.INTERACT_ON_VARIABLE_CHECK)
        {
            string current_var_value_str = GlobalVariablesSystem.GetVariableString(variable_name);

            if (current_var_value_str.Equals(variable_value_needed))
            {

                /* SUCCESS */
                if (perform_every_frame_variable_succeeds)
                {
                    InteractionSystem.ExecuteInteractionsOnGameobject(this);
                }
                else
                {
                    if (currently_succeeding == false)
                    {
                        InteractionSystem.ExecuteInteractionsOnGameobject(this);
                    }
                }

                currently_succeeding = true;
            }
            else
            {
                currently_succeeding = false;
            }
        }
    }

    void OnEventPlayerRespawned(EventPlayerRespawned e)
    {
        InteractionSystem.ExecuteInteractionsOnGameobject(this);
    }

    void RunChecks()
    {
        /* Touch to interact requires colliders */
        if (interaction_type == InteractibleGameobjectType.TOUCH_TO_INTERACT)
        {
            Collider c = GetComponent<Collider>();
            if (c == null)
            {
                Utilities.LogErrorForStudents("InteractibleGameobject", gameObject, "I've been set to the \"TOUCH_TO_INTERACT\" interaction type, but I need a collider component to make this work. Please give me one.");
                return;
            }
        }
    }

    private void OnDestroy()
    {
        if(interaction_type == InteractibleGameobjectType.CLICK_TO_INTERACT)
            click_to_interact_gameobjects.Remove(this);

        if (sub_EventPlayerRespawned != null)
            ArborEventBus.Unsubscribe(sub_EventPlayerRespawned);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (interaction_type == InteractibleGameobjectType.TOUCH_TO_INTERACT)
        {
            if (CheckTagForTouchPossible(other.gameObject.tag))
                InteractionSystem.ExecuteInteractionsOnGameobject(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (interaction_type == InteractibleGameobjectType.TOUCH_TO_INTERACT)
        {
            if(CheckTagForTouchPossible(collision.gameObject.tag))
                InteractionSystem.ExecuteInteractionsOnGameobject(this);
        }
    }

    bool CheckTagForTouchPossible(string tag)
    {
        if (tag == null)
            return false;

        tag = tag.Trim().ToLowerInvariant();

        string[] tags_with_touch_enabled = touchable_tags_comma_separated.Split(',');
        foreach(string touchable_tag in tags_with_touch_enabled)
        {
            if (tag.Equals(touchable_tag.Trim().ToLowerInvariant()))
                return true;
        }

        return false;
    }
}
