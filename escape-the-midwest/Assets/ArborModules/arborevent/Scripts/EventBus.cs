// https://www.c-sharpcorner.com/UploadFile/pranayamr/publisher-or-subscriber-pattern-with-event-or-delegate-and-e/

using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class EventBus
{
    private Dictionary<Type, IList> _topics = new Dictionary<Type, IList>();

    public EventBus()
    {
        _topics = new Dictionary<Type, IList>();
    }

    public void Publish<TMessageType>(TMessageType message)
    {
        Type t = typeof(TMessageType);
        IList actionlst;

        if (_topics.ContainsKey(t))
        {
            actionlst = new List<Subscription<TMessageType>>(_topics[t].Cast<Subscription<TMessageType>>());

            foreach (Subscription<TMessageType> a in actionlst)
            {
                try
                {
                    a.Action(message);
                } catch(Exception e)
                {
                    Debug.LogError("[EventBus.Publish] Error publishing event : " + message + " Error: " + e.Message + " stacktrace : " + e.StackTrace);
                }
            }
        }
    }

    public Subscription<TMessageType> Subscribe<TMessageType>(Action<TMessageType> action)
    {
        Type t = typeof(TMessageType);
        IList actionlst;
        var actiondetail = new Subscription<TMessageType>(action, this);

        if (!_topics.TryGetValue(t, out actionlst))
        {
            actionlst = new List<Subscription<TMessageType>>();
            _topics.Add(t, actionlst);
        }

        _topics[t].Add(actiondetail);

        return actiondetail;
    }

    public void UnSubscribe<TMessageType>(Subscription<TMessageType> subscription)
    {
        Type t = typeof(TMessageType);
        if (_topics.ContainsKey(t))
        {
            _topics[t].Remove(subscription);
        }
    }
}

//Does used by EventBus to reserve subscription  
public class Subscription<Tmessage> : IDisposable
{
    public Action<Tmessage> Action { get; private set; }
    private readonly EventBus EventAggregator;
    private bool isDisposed;
    public Subscription(Action<Tmessage> action, EventBus eventAggregator)
    {
        Action = action;
        EventAggregator = eventAggregator;
    }

    ~Subscription()
    {
        if (!isDisposed)
            Dispose();
    }

    public void Dispose()
    {
        EventAggregator.UnSubscribe(this);
        isDisposed = true;
    }
}


public class Subscriber<T>
{
    public IPublisher<T> Publisher { get; private set; }
    public Subscriber(IPublisher<T> publisher)
    {
        Publisher = publisher;
    }
}


public interface IPublisher<T>
{
    event EventHandler<MessageArgument<T>> DataPublisher;
    void OnDataPublisher(MessageArgument<T> args);
    void PublishData(T data);
}

public class Publisher<T> : IPublisher<T>
{
    //Defined datapublisher event  
    public event EventHandler<MessageArgument<T>> DataPublisher;

    public void OnDataPublisher(MessageArgument<T> args)
    {
        var handler = DataPublisher;
        if (handler != null)
            handler(this, args);
    }

    public void PublishData(T data)
    {
        MessageArgument<T> message = (MessageArgument<T>)Activator.CreateInstance(typeof(MessageArgument<T>), new object[] { data });
        OnDataPublisher(message);
    }
}


public class MessageArgument<T> : EventArgs
{
    public T Message { get; set; }
    public MessageArgument(T message)
    {
        Message = message;
    }
}
