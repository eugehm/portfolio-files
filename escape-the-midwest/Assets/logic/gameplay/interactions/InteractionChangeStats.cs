using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractionChangeStats : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] bool change_is_relative = false;

    [SerializeField] float max_running_speed_value = 30;
    [SerializeField] float running_acceleration_value = 30;
    [SerializeField] float in_air_acceleration_value = 30;
    [SerializeField] float jumping_power_value = 8;
    [SerializeField] float running_friction_factor = 0.93f;
    [SerializeField] float in_air_friction_factor = 0.93f;

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
        Character current_char = Character.GetCurrentSelectedCharacter();

        if(change_is_relative)
        {
            current_char.SetMaxRunningSpeed(current_char.GetMaxRunningSpeed() + max_running_speed_value);
            current_char.SetRunningAcceleration(current_char.GetRunningAcceleration() + running_acceleration_value);
            current_char.SetInAirAcceleration(current_char.GetInAirAcceleration() + in_air_acceleration_value);
            current_char.SetJumpPower(current_char.GetJumpPower() + jumping_power_value);
            current_char.SetRunningFrictionFactor(current_char.GetRunningFrictionFactor() + running_friction_factor);
            current_char.SetInAirFrictionFactor(current_char.GetInAirFrictionFactor() + in_air_friction_factor);
        }
        else
        {
            current_char.SetMaxRunningSpeed(max_running_speed_value);
            current_char.SetRunningAcceleration(running_acceleration_value);
            current_char.SetInAirAcceleration(in_air_acceleration_value);
            current_char.SetJumpPower(jumping_power_value);
            current_char.SetRunningFrictionFactor(running_friction_factor);
            current_char.SetInAirFrictionFactor(in_air_friction_factor);
        }

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
