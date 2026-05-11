using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum InventoryItemState { NOT_COLLECTED, COLLECTING, COLLECTED };

public class InventoryItem : MonoBehaviour
{
    Vector3 initial_position;
    Quaternion initial_rotation;

    InventoryItemState current_state = InventoryItemState.NOT_COLLECTED;

    public string item_name = "";
    public bool collect_by_coming_near = true;
    public float collection_distance2 = 4.0f;
    public float back_off_distance2 = 2.0f;
    public float follow_speed3 = 9.0f;
    public bool reset_on_death2 = true;
    public bool reset_on_gameover2 = false;
    public bool destroy_on_usage2 = true;

    Subscription<EventPlayerKO> sub_EventPlayerKO;
    Subscription<EventGameOver> sub_EventGameOver;

    static List<InventoryItem> inventory = new List<InventoryItem>();
    public static void ResetInventory()
    {
        inventory.Clear();
    }

    public static bool ConsumeItem(string item_name)
    {
        if(item_name == null || item_name.Trim().ToLowerInvariant() == "")
        {
            return false;
        }

        item_name = item_name.Trim().ToLowerInvariant();

        InventoryItem found_item = null;

        foreach(InventoryItem item in inventory)
        {
            if (item == null)
                continue;

            if (item.item_name.Trim().ToLowerInvariant() == item_name)
            {
                found_item = item;
                break;
            }   
        }

        if(found_item != null)
        {
            found_item.Consume();
            return true;
        }

        return false;
    }

    public static int GetIndexInInventory(InventoryItem item)
    {
        return inventory.IndexOf(item);
    }

    void Start()
    {
        if(item_name == null || item_name.Trim() == "")
        {
            Utilities.LogErrorForStudents("InventoryItem", gameObject, "You forgot to give the InventoryItem component a name.");
        }

        initial_position = transform.position;
        initial_rotation = transform.rotation;

        if (reset_on_death2)
            sub_EventPlayerKO = ArborEventBus.Subscribe<EventPlayerKO>(OnEventPlayerKO);
        if (reset_on_gameover2)
            sub_EventGameOver = ArborEventBus.Subscribe<EventGameOver>(OnEventGameOver);

        CleanOffGameobject();
    }

    void ResetItem()
    {
        if (inventory.Contains(this))
            inventory.Remove(this);
        
        transform.position = initial_position;
        transform.rotation = initial_rotation;
        current_state = InventoryItemState.NOT_COLLECTED;
    }

    void OnEventPlayerKO(EventPlayerKO e)
    {
        ResetItem();
    }

    void OnEventGameOver(EventGameOver e)
    {
        ResetItem();
    }

    void CleanOffGameobject()
    {
        /* Remove colliders */
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider c in colliders)
            Destroy(c);

        /* Remove interactions */
        GameplayLogicComponent[] logic_components = GetComponents<GameplayLogicComponent>();
        foreach (GameplayLogicComponent g in logic_components)
            Destroy(g);
        InteractableGameobject interactable_gameobject = GetComponent<InteractableGameobject>();
        if (interactable_gameobject != null)
            Destroy(interactable_gameobject);
    }

    void Update()
    {
        if (PlayerController.GetPlayerGameobject() == null)
            return;

        if (current_state == InventoryItemState.NOT_COLLECTED)
            PerformNotCollectedState();
        else if (current_state == InventoryItemState.COLLECTING)
            PerformCollectingState();
        else if (current_state == InventoryItemState.COLLECTED)
            PerformCollectedState();
    }

    void Consume()
    {
        inventory.Remove(this);
        if (destroy_on_usage2)
            Destroy(gameObject);
        else
        {
            transform.position = initial_position;
            transform.rotation = initial_rotation;
        }
    }

    void AddToInventory()
    {
        if(inventory.Contains(this) == false)
        {
            inventory.Add(this);
        }
    }

    void PerformNotCollectedState()
    {
        if (collect_by_coming_near)
        {
            /* Check distance from player */
            float distance = Vector3.Distance(transform.position, PlayerController.GetPlayerPosition());
            if (distance <= collection_distance2)
            {
                current_state = InventoryItemState.COLLECTING;
            }
        }

        /* Animation */
        transform.Rotate(new Vector3(0, Time.deltaTime * 60.0f, 0));
        transform.position = initial_position + new Vector3(0, Mathf.Abs(Mathf.Sin(Time.time * 4.0f)), 0);
    }

    void PerformCollectingState()
    {

        Vector3 to_player = PlayerController.GetPlayerPosition() - transform.position;
        to_player.Normalize();

        transform.position += to_player * follow_speed3 * 2.0f * Time.deltaTime;
        float distance = Vector3.Distance(PlayerController.GetPlayerPosition(), transform.position);
        if(distance <= 0.5f)
        {
            AddToInventory();
            current_state = InventoryItemState.COLLECTED;
        }
    }

    Transform GetThingInFrontOfUsInLine()
    {
        int our_index = inventory.IndexOf(this);
        int index_of_thing_to_follow = our_index - 1;
        while(index_of_thing_to_follow >= 0)
        {
            InventoryItem thing = inventory[index_of_thing_to_follow];
            if (thing != null)
                return thing.transform;

            index_of_thing_to_follow--;
        }

        return PlayerController.GetPlayerGameobject().transform;
    }

    void PerformCollectedState()
    {
        Transform thing_to_follow = GetThingInFrontOfUsInLine();
        if (thing_to_follow == null)
            return;

        Vector3 movement_direction_xz = thing_to_follow.transform.position - transform.position;
        movement_direction_xz = new Vector3(movement_direction_xz.x, 0, movement_direction_xz.z);
        float distance_xz = movement_direction_xz.magnitude;
        movement_direction_xz = movement_direction_xz.normalized;

        if (distance_xz <= back_off_distance2)
        {
            /* We're close enough. Do nothing. */

        }
        else
        {
            /* Follow the next thing in line */
            float movement_amount = follow_speed3;
            transform.position += movement_direction_xz * movement_amount * Time.deltaTime;
        }

        /* Animation */
        float bounce_y = Mathf.Abs(Mathf.Sin(Time.time * 4.0f + GetIndexInInventory(this) * 0.5f));

        float current_y = transform.position.y;
        current_y = Mathf.Max(desired_y_position, PlayerController.GetPlayerPosition().y - 2.0f);// Mathf.Lerp(current_y, desired_y_position, 0.1f);
        transform.position = new Vector3(transform.position.x, current_y + bounce_y, transform.position.z);
    }

    float desired_y_position = 0;

    private void FixedUpdate()
    {
        if (PlayerController.GetPlayerGameobject() == null)
            return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 8.0f))
        {
            desired_y_position = hit.point.y;
        }
        else
        {
            if(desired_y_position > PlayerController.GetPlayerPosition().y)
                desired_y_position = PlayerController.GetPlayerPosition().y;
        }
    }

    private void OnDestroy()
    {
        if (sub_EventPlayerKO != null)
            ArborEventBus.Unsubscribe(sub_EventPlayerKO);
        if (sub_EventGameOver != null)
            ArborEventBus.Unsubscribe(sub_EventGameOver);
    }
}
