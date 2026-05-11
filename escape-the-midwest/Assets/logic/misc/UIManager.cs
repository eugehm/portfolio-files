using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static int currency = 0;
    static int lives = 2;
    Camera cam;

    public static void ChangeLivesAmount(int delta)
    {
        lives += delta;
        ArborEventBus.Publish(new EventLivesChanged());
    }

    public static void SetLivesAmount(int number)
    {
        lives = number;
        ArborEventBus.Publish(new EventLivesChanged());
    }

    public static int GetRemainingLives()
    {
        return lives;
    }

    List<LifeIcon> existing_icons = new List<LifeIcon>();

    Subscription<EventLivesChanged> sub_EventLivesChanged;
    Subscription<EventCurrencyChanged> sub_EventCurrencyChanged;
    Subscription<EventCharacterChanged> sub_EventCharacterChanged;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ArborCoroutineManager.ArborStartCoroutine(DoDelayedStart());

        sub_EventLivesChanged = ArborEventBus.Subscribe<EventLivesChanged>(OnEventLivesChanged);
        sub_EventCurrencyChanged = ArborEventBus.Subscribe<EventCurrencyChanged>(OnEventCurrencyChanged);
        sub_EventCharacterChanged = ArborEventBus.Subscribe<EventCharacterChanged>(OnEventCharacterChanged);
    }

    void OnEventLivesChanged(EventLivesChanged e)
    {
        CheckExistingLives();
    }

    void OnEventCurrencyChanged(EventCurrencyChanged e)
    {

    }

    void OnEventCharacterChanged(EventCharacterChanged e)
    {
        PurgeIcons();
    }

    private void OnDestroy()
    {
        if (sub_EventLivesChanged != null)
            ArborEventBus.Unsubscribe(sub_EventLivesChanged);
        if (sub_EventCurrencyChanged != null)
            ArborEventBus.Unsubscribe(sub_EventCurrencyChanged);
    }

    IEnumerator DoDelayedStart()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        CheckExistingLives();
    }

    void PurgeIcons()
    {
        foreach(LifeIcon icon in existing_icons)
        {
            Destroy(icon.gameObject);
        }

        existing_icons.Clear();

        CheckExistingLives();
    }

    void CheckExistingLives()
    {
        int max = Math.Max(lives, existing_icons.Count);
        List<int> icons_to_remove = new List<int>();

        for(int i = 0; i < max; i++)
        {
            // We need to spawn some new icons
            if(i >= existing_icons.Count)
            {
                Character current_selected_char = Character.GetCurrentSelectedCharacter();
                GameObject new_icon = null;

                if (current_selected_char.GetFaceUILivesPrefab() == null)
                {
                    new_icon = GameObject.Instantiate(Resources.Load<GameObject>("objects/default_life_icon/default_life_icon"));
                }
                else
                {
                    new_icon = GameObject.Instantiate(current_selected_char.GetFaceUILivesPrefab());
                }
                 
                new_icon.transform.SetParent(transform);
                new_icon.transform.localPosition = new Vector3(-7 + i, 0, 2);

                Collectible coll = new_icon.GetComponent<Collectible>();
                if (coll != null)
                    Destroy(coll);

                LifeIcon icon = new_icon.GetComponent<LifeIcon>();
                if (icon == null)
                    icon = new_icon.AddComponent<LifeIcon>();

                icon.index = i;
                icon.BecomeVisible();
                existing_icons.Add(icon);
            }

            // We need to deactivate and remove some icons
            else if (i >= lives)
            {
                LifeIcon icon = existing_icons[i].GetComponent<LifeIcon>();
                ArborCoroutineManager.ArborStartCoroutine(icon.DoOutroAnimation());
                icons_to_remove.Add(i);
            }
        }

        icons_to_remove.Reverse();
        foreach (int icon_index in icons_to_remove)
            existing_icons.RemoveAt(icon_index);
    }
}

public class EventLivesChanged { }
public class EventCurrencyChanged { }