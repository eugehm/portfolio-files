using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MusicTrack { BACKGROUND_MUSIC, AMBIENT_AUDIO };

[CreateAssetMenu(fileName = "music_bank", menuName = "3D Platformer Testbed/Create New MusicBank")]
public class MusicBank : ScriptableObject
{
    public SerializableDictionary<string, Music> database = new SerializableDictionary<string, Music>();
    static MusicBank active_music_bank = null;

    public static AudioClip GetClipForEventName(string event_name)
    {
        if (event_name == null)
            return null;
        event_name = event_name.ToLowerInvariant().Trim();

        /* Check music bank */
        if (active_music_bank == null)
        {
            MusicBank[] music_banks = Resources.LoadAll<MusicBank>("");
            if (music_banks.Length > 1)
                Utilities.LogErrorForStudents("MusicBank", null, "You have more than one musicbank asset in the project view. Please remove them until you have one.");
            if (music_banks.Length <= 0)
                Utilities.LogErrorForStudents("MusicBank", null, "You do not have a musicbank asset in the project view. Please use the right click menu inside a Resources/ folder to create one.");
            active_music_bank = music_banks[0];
        }

        if (active_music_bank == null)
            return null;

        /* Randomly choose an audio clip */
        AudioClip clip = active_music_bank.GetAudioClip(event_name);
        if(clip == null)
            Utilities.LogErrorForStudents("MusicBank", null, $"Your music bank does not include any music for the event [{event_name}]");

        return clip;
    }

    public static float GetMaxVolumeForEventName(string event_name)
    {
        if (event_name == null)
            return -1.0f;
        event_name = event_name.ToLowerInvariant().Trim();

        if (active_music_bank == null)
        {
            MusicBank[] music_banks = Resources.LoadAll<MusicBank>("");
            if (music_banks.Length > 1)
                Utilities.LogErrorForStudents("MusicBank", null, "You have more than one musicbank asset in the project view. Please remove them until you have one.");
            if (music_banks.Length <= 0)
                Utilities.LogErrorForStudents("MusicBank", null, "You do not have a musicbank asset in the project view. Please use the right click menu to create one.");
            active_music_bank = music_banks[0];
        }

        if (active_music_bank == null)
            return -1.0f;

        return active_music_bank.GetMaxVolume(event_name);
    }

    public float GetMaxVolume(string event_name)
    {
        InitIfNecessary();

        if (database.ContainsKey(event_name) == false)
            return -1.0f;

        Music music_event = database[event_name];
        return music_event.max_volume;
    }

    public AudioClip GetAudioClip(string event_name)
    {
        InitIfNecessary();

        if (database.ContainsKey(event_name) == false)
            return null;

        Music music_event = database[event_name];
        if (music_event.possible_clips.Count <= 0)
            return null;

        int random_index = UnityEngine.Random.Range(0, music_event.possible_clips.Count);
        return music_event.possible_clips[random_index];
    }

    bool is_initialized = false;
    public void InitIfNecessary()
    {
        if (is_initialized)
            return;

        /* Ensure all the dictionary keys are lower case and trimmed. */
        SerializableDictionary<string, Music> temp_database = new SerializableDictionary<string, Music>();
        foreach(var kvp in database)
        {
            var new_key = kvp.Key.Trim().ToLowerInvariant();
            temp_database[new_key] = kvp.Value;
        }

        database.Clear();

        foreach(var kvp in temp_database)
        {
            database[kvp.Key] = kvp.Value;
        }

        is_initialized = true;
    }
}

[Serializable]
public class Music
{
    /* Music with the same track will end / cancel any other music playing on the same track */
    public MusicTrack track = MusicTrack.BACKGROUND_MUSIC;
    public float max_volume = 1.0f;
    public List<AudioClip> possible_clips = new List<AudioClip>();
}