using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    static Timer instance;
    
    Text text;
    bool running = false;
    float lifetime = 60.0f;

    Subscription<EventPlayerKO> sub_EventPlayerKO;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        sub_EventPlayerKO = ArborEventBus.Subscribe<EventPlayerKO>(OnEventPlayerKO);
    }

    void OnEventPlayerKO(EventPlayerKO e)
    {
        StopTimer();
    }

    private void OnDestroy()
    {
        if (sub_EventPlayerKO != null)
            ArborEventBus.Unsubscribe(sub_EventPlayerKO);
    }

    public static void StartTimer(float duration_sec)
    {
        instance.lifetime = duration_sec;
        if(duration_sec > 0.0f)
        {
            instance.running = true;
        }
    }

    public static void StopTimer()
    {
        instance.running = false;
        instance.lifetime = -1.0f;
    }

    void Start()
    {
        text = GetComponent<Text>();
        text.text = "";
    }

    void Update()
    {
        if (running)
        {
            /* Make visible */
            if (InteractionSystem.IsBlockingCurrently() == false)
            {
                lifetime -= Time.deltaTime;
            }

            if(lifetime <= 0.0f && running)
            {
                running = false;
                ArborEventBus.Publish(new EventPlayerShouldKO(KO_CAUSE.TIMEOVER));
            }

            text.color = Color.Lerp(Color.white, Color.red, (Mathf.Sin(Time.time * 4.0f) + 1.0f) / 2.0f);
            text.text = lifetime.ToString("F2");
        }
        else
        {
            /* Make invisible */
            text.text = "";
        }
    }
}
