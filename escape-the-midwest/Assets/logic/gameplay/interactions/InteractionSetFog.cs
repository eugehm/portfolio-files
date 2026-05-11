using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractionSetFog : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] Color fog_color = Color.white;
    [SerializeField] float fog_density = 0.025f;
    [SerializeField] float transition_duration_sec = 2.0f;

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
        RenderSettings.fog = true;

        Color start_color = RenderSettings.fogColor;
        float start_density = RenderSettings.fogDensity;

        if(transition_duration_sec > 0.0f)
        {
            float start_time = Time.time;
            float progress = (Time.time - start_time) / transition_duration_sec;
            while (progress < 1.0f)
            {
                progress = (Time.time - start_time) / transition_duration_sec;
                RenderSettings.fogColor = Color.Lerp(start_color, fog_color, progress);
                RenderSettings.fogDensity = Mathf.Lerp(start_density, fog_density, progress);
                if (Camera.main != null)
                    Camera.main.backgroundColor = Color.Lerp(start_color, fog_color, progress);
                yield return null;
            }
        }

        RenderSettings.fogColor = fog_color;
        RenderSettings.fogDensity = fog_density;
        if (Camera.main != null)
            Camera.main.backgroundColor = fog_color;

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
