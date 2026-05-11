using System.Collections;
using UnityEngine;

public class InteractionBGM : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] string audio_event_name = "";

    private void Start()
    {
        if (audio_event_name == null)
            Utilities.LogErrorForStudents("InteractionBGM", gameObject, "You forgot to specify the BGM event name.");
    }

    public override bool IsInteractionBlocking()
    {
        return false;
    }

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return false;
    }

    protected override IEnumerator _OnInteract()
    {
        ArborEventBus.Publish(new EventMusicEvent(audio_event_name));
        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
