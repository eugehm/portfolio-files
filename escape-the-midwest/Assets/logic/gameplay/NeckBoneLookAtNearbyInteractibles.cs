using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NeckBoneLookAtNearbyInteractibles : MonoBehaviour
{
    [SerializeField] float max_distance = 7;
    [SerializeField] bool look_at_nearby_players_too = true;

    NetworkPlayerView network_view = null;

    static List<NeckBoneLookAtNearbyInteractibles> looking_necks = new List<NeckBoneLookAtNearbyInteractibles>();
    static NeckBoneLookAtNearbyInteractibles GetNearestLookingNeck(Vector3 point, float max_distance, NeckBoneLookAtNearbyInteractibles neck_to_exclude)
    {
        float min_d = float.MaxValue;
        NeckBoneLookAtNearbyInteractibles nearest = null;
        foreach (NeckBoneLookAtNearbyInteractibles neck in looking_necks)
        {
            if (neck == null)
                continue;
            if (neck == neck_to_exclude)
                continue;

            float d = Vector3.Distance(neck.transform.position, point);
            if (d < max_distance && d < min_d)
            {
                min_d = d;
                nearest = neck;
            }
        }

        return nearest;
    }

    private void Start()
    {
        if(looking_necks.Contains(this) == false)
            looking_necks.Add(this);

        HashSet<ProvideAnimatorParametersNetwork> network_components = new HashSet<ProvideAnimatorParametersNetwork>();
        Utilities.GetComponentsInAncestors(transform, ref network_components);

        if (network_components.Count > 0)
        {
            using (var enumerator = network_components.GetEnumerator())
            {
                if (enumerator.MoveNext())
                    network_view = enumerator.Current.network_player_info;
            }
        }
    }

    private void OnDestroy()
    {
        if (looking_necks.Contains(this))
            looking_necks.Remove(this);
    }

    void LateUpdate()
    {
        AnimateHead();
    }

    Quaternion current_head_angle = Quaternion.identity;
    void AnimateHead()
    {
        Transform thing_to_focus_on = null;
        Transform thing_that_determines_our_direction = null;
        bool possible_to_look = false;

        /* See if there's an interactable gameobject nearby */
        InteractableGameobject go = InteractableGameobject.GetInteractableGameobjectNearestPoint(transform.position, max_distance);

        Quaternion animation_rotation = transform.rotation;
        if (current_head_angle == Quaternion.identity)
            current_head_angle = animation_rotation;

        if (go != null)
            thing_to_focus_on = go.transform;
        else
        {
            if(look_at_nearby_players_too)
            {
                /* See if there's another player nearby */
                NeckBoneLookAtNearbyInteractibles nearest_neck = GetNearestLookingNeck(transform.position, max_distance, this);
                if (nearest_neck != null)
                {
                    thing_to_focus_on = nearest_neck.transform;
                }
            }
        }

        if (network_view != null)
            thing_that_determines_our_direction = network_view.GetTransform();

        if (thing_that_determines_our_direction == null)
            thing_that_determines_our_direction = PlayerController.GetPlayerGameobject().transform;


        if (thing_to_focus_on != null)
        {
            Vector3 focus_object_point = thing_to_focus_on.position;

            Quaternion final_head_rot = transform.rotation;

            // changed this so there's less of a tilt, was looking weird
            Vector3 head_to_object = focus_object_point - transform.position;
            head_to_object.y = 0;
            head_to_object = head_to_object.normalized;
            
            final_head_rot.SetLookRotation(head_to_object, Vector3.up);

            /* Check angle between player forward and thing. */
            float angle = Quaternion.Angle(thing_that_determines_our_direction.rotation, final_head_rot);
            if(angle < 80)
            {
                current_head_angle = Quaternion.Slerp(current_head_angle, final_head_rot, 0.2f);
                possible_to_look = true;
            }
        }

        /* Don't bother if we're in a ko state or something similar */
        if (network_view == null)
        {
            if (PlayerController.GetCurrentState().Equals("gameplay") == false)
                possible_to_look = false;
        }
        else
        {
            if (network_view.GetNetworkPlayerState().Equals("gameplay") == false)
                possible_to_look = false;
        }
        
        if(possible_to_look == false)
        {
            current_head_angle = Quaternion.Slerp(current_head_angle, animation_rotation, 0.2f);
        }

        transform.rotation = current_head_angle;
    }
}
