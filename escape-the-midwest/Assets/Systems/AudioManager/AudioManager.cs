/* USAGE : Find the SoundBank asset in the project pane and fill it out. */
/* If no SoundBank asset exists, you'll see errors. Use right click in the project pane to create a new one. */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    static AudioManager instance;
    static List<AudioSource> all_audio_sources = new List<AudioSource>();

    [SerializeField] AudioSource bgm_source_1;
    [SerializeField] AudioSource bgm_source_2;

    AudioSource active_audio_source;
    AudioSource backup_audio_source;


    void Awake()
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

        active_audio_source = bgm_source_1;
        backup_audio_source = bgm_source_2;

        active_audio_source.loop = true;
        backup_audio_source.loop = true;

        backup_audio_source.Stop();
        active_audio_source.Stop();

        ArborEventBus.Subscribe<EventSoundEvent>(OnEventSoundEvent);
        ArborEventBus.Subscribe<EventMusicEvent>(OnEventMusicEvent);
    }

    void OnEventSoundEvent(EventSoundEvent e)
    {
        /* Find Audio Clip */
        AudioClip clip = SoundBank.GetClipForEventName(e.sound_event_name);

        /* Set up and play audio source */
        // If an audio event has an associated location / position, it's a 3D sound. Otherwise 2D.
        bool three_d_sound = e.location.Equals(Vector3.zero) == false;

        // Prepare an audio source to play our sound.
        AudioSource audio_source = GetAvailableAudioSource();
        audio_source.Stop();
        audio_source.loop = false;
        audio_source.volume = 1.0f;

        if (three_d_sound)
            audio_source.spatialBlend = 1.0f;
        else
            audio_source.spatialBlend = 0.0f;

        audio_source.clip = clip;
        audio_source.Play();
    }

    void OnEventMusicEvent(EventMusicEvent e)
    {
        /* Find Audio Clip */
        AudioClip clip = MusicBank.GetClipForEventName(e.music_event_name);

        if (clip == active_audio_source.clip)
            return;

        float max_volume = MusicBank.GetMaxVolumeForEventName(e.music_event_name);

        StartCoroutine(PlayBGMClip(clip, max_volume));
    }

    IEnumerator PlayBGMClip(AudioClip clip, float max_volume)
    {
        backup_audio_source.clip = clip;

        StartCoroutine(FadeAudioSource(active_audio_source));
        yield return StartCoroutine(RiseAudioSource(backup_audio_source, max_volume));

        // Switch audio sources
        AudioSource temp = active_audio_source;
        active_audio_source = backup_audio_source;
        backup_audio_source = temp;
    }

    IEnumerator RiseAudioSource(AudioSource source, float max_volume)
    {
        source.volume = 0.0f;
        source.Play();

        float duration_Sec = 2.0f;
        float start_time = Time.time;
        float progress = (Time.time - start_time) / duration_Sec;
        
        while(progress < 1.0f)
        {
            progress = (Time.time - start_time) / duration_Sec;
            source.volume = Mathf.Lerp(0.0f, max_volume, progress);
            yield return null;
        }
        source.volume = max_volume;
    }

    IEnumerator FadeAudioSource(AudioSource source)
    {
        float starting_volume = source.volume;
        float duration_Sec = 2.0f;
        float start_time = Time.time;
        float progress = (Time.time - start_time) / duration_Sec;

        while (progress < 1.0f)
        {
            progress = (Time.time - start_time) / duration_Sec;
            source.volume = Mathf.Lerp(starting_volume, 0.0f, progress);
            yield return null;
        }
        source.volume = 0.0f;
        source.Stop();
    }

    AudioSource GetAvailableAudioSource()
    {
        /* Find an existing audio source */
        foreach(AudioSource source in all_audio_sources)
        {
            if(source.isPlaying == false)
            {
                return source;
            }
        }

        /* Create a new one, as none were available */
        AudioSource new_source = gameObject.AddComponent<AudioSource>();
        all_audio_sources.Add(new_source);

        return new_source;
    } 
}

public class EventSoundEvent
{
    public string sound_event_name;
    public Vector3 location;

    public EventSoundEvent(string _audio_event_name, Vector3 _location)
    {
        sound_event_name = _audio_event_name.ToLowerInvariant().Trim();
        location = _location;
    }
}

public class EventMusicEvent
{
    public string music_event_name;

    public EventMusicEvent(string _music_event_name)
    {
        music_event_name = _music_event_name;
    }
}
