using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "sound_bank", menuName = "3D Platformer Testbed/Create New SoundBank")]
public class SoundBank : ScriptableObject
{
    public SerializableDictionary<string, SoundEvent> database = new SerializableDictionary<string, SoundEvent>();
    static SoundBank active_sound_bank = null;

    public static AudioClip GetClipForEventName(string event_name)
    {
        if (event_name == null)
            return null;
        event_name = event_name.ToLowerInvariant().Trim();

        /* Check sound bank */
        if (active_sound_bank == null)
        {
            SoundBank[] sound_banks = Resources.LoadAll<SoundBank>("");
            if (sound_banks.Length > 1)
                Utilities.LogErrorForStudents("SoundBank", null, "You have more than one soundbank asset in the project view. Please remove them until you have one.");
            if (sound_banks.Length <= 0)
                Utilities.LogErrorForStudents("SoundBank", null, "You do not have a soundbank asset in the project view. Please use the right click menu inside a Resources/ folder to create one.");
            active_sound_bank = sound_banks[0];
        }

        if (active_sound_bank == null)
            return null;

        /* Randomly choose an audio clip */
        AudioClip clip = active_sound_bank.GetAudioClip(event_name);
        if(clip == null)
            Utilities.LogErrorForStudents("SoundBank", null, $"Your sound bank does not include any sounds for the event [{event_name}]");

        return clip;
    }

    Dictionary<string, HashSet<int>> previously_chosen_clips = new Dictionary<string, HashSet<int>>();

    public AudioClip GetAudioClip(string event_name)
    {
        InitIfNecessary();

        if (database.ContainsKey(event_name) == false)
            return null;

        SoundEvent sound_event = database[event_name];
        if (sound_event.possible_clips.Count <= 0)
            return null;

        int random_index = UnityEngine.Random.Range(0, sound_event.possible_clips.Count);

        if (previously_chosen_clips.ContainsKey(event_name) == false)
            previously_chosen_clips[event_name] = new HashSet<int>();

        /* Check if we should try a different index. This is kind of a hacky solution. */
        if(sound_event.possible_clips.Count > 1)
        {
            int limit = 30;
            int attempts = 0;
            while(previously_chosen_clips[event_name].Contains(random_index) && attempts < limit)
            {
                random_index = UnityEngine.Random.Range(0, sound_event.possible_clips.Count);
                attempts++;
            }
        }

        previously_chosen_clips[event_name].Add(random_index);
        if (previously_chosen_clips[event_name].Count >= sound_event.possible_clips.Count)
            previously_chosen_clips[event_name].Clear();

        return sound_event.possible_clips[random_index];
    }

    bool is_initialized = false;
    public void InitIfNecessary()
    {
        if (is_initialized)
            return;

        /* Ensure all the dictionary keys are lower case and trimmed. */
        SerializableDictionary<string, SoundEvent> temp_database = new SerializableDictionary<string, SoundEvent>();
        foreach(var kvp in database)
        {
            if (kvp.Key == null || kvp.Value == null || kvp.Key.Trim() == "")
                continue;

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
public class SoundEvent
{
    public List<AudioClip> possible_clips = new List<AudioClip>();
}