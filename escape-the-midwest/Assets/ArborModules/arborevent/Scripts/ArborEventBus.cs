using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ArborEventBus : MonoBehaviour
{
    static ArborEventBus instance;

    static EventBus bus;

    // Start is called before the first frame update
    void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        } else
        {
            instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }

        _InstantiateIfNecessary();
    }

    static bool initialized = false;
    static void _InstantiateIfNecessary()
    {
        if (initialized)
            return;

        bus = new EventBus();
        initialized = true;
    }

    public static Subscription<T> Subscribe<T>(Action<T> callback)
    {
        _InstantiateIfNecessary();

        return bus.Subscribe<T>(callback);
    }

    public static void Unsubscribe<T>(Subscription<T> subscription)
    {
        if (subscription == null)
            return;

        _InstantiateIfNecessary();

        bus.UnSubscribe<T>(subscription);
        subscription = null;
    }

    public static void Publish<T>(T message)
    {
        _InstantiateIfNecessary();

        bus.Publish<T>(message);
    }

    EventUpdate e;
    private void Update()
    {
        if (e == null)
            e = new EventUpdate();

        ArborEventBus.Publish<EventUpdate>(e);
    }

    EventFixedUpdate e_fixed;
    private void FixedUpdate()
    {
        if (e_fixed == null)
            e_fixed = new EventFixedUpdate();

        ArborEventBus.Publish<EventFixedUpdate>(e_fixed);
    }
}

public class EventUpdate { public string object_name; };
public class EventFixedUpdate { };
