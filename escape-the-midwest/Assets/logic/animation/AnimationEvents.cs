using UnityEngine;

public enum ANIMATION_EVENT_SOUND_MODE { GLOBAL, LOCAL };

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] ANIMATION_EVENT_SOUND_MODE mode = ANIMATION_EVENT_SOUND_MODE.GLOBAL;

    public void sound(string s)
    {
        if(mode == ANIMATION_EVENT_SOUND_MODE.GLOBAL)
            ArborEventBus.Publish(new EventSoundEvent(s, Vector3.zero));
        else if (mode == ANIMATION_EVENT_SOUND_MODE.LOCAL)
            ArborEventBus.Publish(new EventSoundEvent(s, transform.position));
    }
}
