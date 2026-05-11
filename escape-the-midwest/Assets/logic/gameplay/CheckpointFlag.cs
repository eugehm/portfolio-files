using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointFlag : MonoBehaviour
{
    static CheckpointFlag current_active_checkpoint = null;
    static List<CheckpointFlag> all_checkpoint_flags = new List<CheckpointFlag>();

    public static Vector3 GetActiveCheckpointFlagPosition()
    {
        if (current_active_checkpoint == null)
        {
            foreach (CheckpointFlag flag in all_checkpoint_flags)
            {
                if (flag.is_stage_start_flag)
                {
                    current_active_checkpoint = flag;
                    break;
                }
            }

            if (current_active_checkpoint == null && all_checkpoint_flags.Count > 0)
                current_active_checkpoint = all_checkpoint_flags[0];
        }

        return current_active_checkpoint.transform.position;
    }

    public static Vector3 GetStartingCheckpointFlagPosition()
    {
        Vector3 result = Vector3.zero;

        foreach(CheckpointFlag flag in all_checkpoint_flags)
        {
            if (flag.is_stage_start_flag)
            {
                result = flag.transform.position;
                break;
            }
        }

        return result;
    }

    public bool is_stage_start_flag = false;

    bool close_to_player = false;
    const float close_distance = 4.0f;

    private void Awake()
    {
        all_checkpoint_flags.Add(this);
        if (is_stage_start_flag)
        {
            ClaimCheckpoint();
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        all_checkpoint_flags.Remove(this);
    }

    void Update()
    {
        if (PlayerController.GetPlayerGameobject() == null)
            return;

        float distance = Vector3.Distance(transform.position, PlayerController.GetPlayerPosition());
        bool currently_close_to_player = distance <= close_distance;

        if (close_to_player == false && currently_close_to_player == true)
        {
            close_to_player = true;
            ClaimCheckpoint();
        }
        else if (close_to_player == true && currently_close_to_player == false)
        {
            close_to_player = false;
        }

        HookesScale();
    }

    void ClaimCheckpoint()
    {
        if(current_active_checkpoint != this && !is_stage_start_flag)
        {
            ArborEventBus.Publish(new EventSoundEvent("vocal_checkpoint", Vector3.zero));
        }
        current_active_checkpoint = this;
    }

    float desired_scale = 1.0f;
    float velocity = 0.0f;
    void HookesScale()
    {
        if (WebManager.IsTabCurrentlyActive() == false || Application.isFocused == false)
            return;

        if (current_active_checkpoint == this)
            desired_scale = 2.0f;
        else desired_scale = 1.0f;

        float k = 0.04f;
        float dampening_factor = 0.92f;
        // a = kx
        float x = desired_scale - transform.localScale.x;
        float a = k * x;
        velocity += a * Time.deltaTime * 60.0f;
        velocity *= Mathf.Pow(dampening_factor, Time.deltaTime * 60.0f);
        transform.localScale += Vector3.one * velocity * Time.deltaTime * 60.0f;
    }
}
