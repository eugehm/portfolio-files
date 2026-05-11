using System.Collections;
using UnityEngine;

public class InteractionStartScreenShake : Interaction
{
    [SerializeField] bool only_ever_run_once_pls = false;

    [SerializeField] float shake_magnitude = 6.0f;
    [SerializeField] float shake_interval_sec = 0.5f;

    public override bool IsInteractionBlocking()
    {
        return false;
    }

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once_pls;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return false;
    }

    Subscription<EventPlayerRespawned> sub_EventPlayerRespawned;
    Subscription<EventGameOver> sub_EventGameOver;

    private void Start()
    {
        sub_EventPlayerRespawned = ArborEventBus.Subscribe<EventPlayerRespawned>(OnEventPlayerRespawned);
        sub_EventGameOver = ArborEventBus.Subscribe<EventGameOver>(OnEventGameOver);
    }

    void OnEventPlayerRespawned (EventPlayerRespawned e)
    {
        ScreenShakeManager.StopShaking();
    }

    void OnEventGameOver(EventGameOver e)
    {
        ScreenShakeManager.StopShaking();
    }

    protected override IEnumerator _OnInteract()
    {
        ScreenShakeManager.StartShaking(shake_magnitude, shake_interval_sec);

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }

    private void OnDestroy()
    {
        ArborEventBus.Unsubscribe(sub_EventPlayerRespawned);
        ArborEventBus.Unsubscribe(sub_EventGameOver);
    }
}
