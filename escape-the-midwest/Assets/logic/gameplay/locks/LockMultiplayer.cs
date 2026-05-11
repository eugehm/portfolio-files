using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockMultiplayer : Lock
{
    bool locked = true;
    [SerializeField] int required_nearby_players = 2;
    [SerializeField] float nearby_distance = 6;
    [SerializeField] bool once_unlocked_stay_unlocked = false;

    public override bool CheckUnlocked()
    {
        return !locked;
    }

    public override IEnumerator TryLock()
    {
        if (once_unlocked_stay_unlocked)
        {
            if (locked == false)
                yield break;
        }

        /* Gather nearby players */
        int num_nearby_players = 1; // count the interacting player.
        List<Vector3> player_positions = NetworkManager.GetAllNetworkPlayerPositions();

        foreach (Vector3 player_pos in player_positions)
        {
            float d = Vector3.Distance(transform.position, player_pos);
            if (d <= nearby_distance)
            {
                num_nearby_players++;
            }
        }

        if (num_nearby_players >= required_nearby_players)
            locked = false;
        else
        {
            locked = true;
            ToastManager.RequestToast($"Need more players nearby ({num_nearby_players}/{required_nearby_players})", ToastType.NORMAL);
        }

        yield break;
    }
}
