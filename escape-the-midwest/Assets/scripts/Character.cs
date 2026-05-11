using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "character_configuration", menuName = "3D Platformer Testbed/Create New Character")]
public class Character : ScriptableObject
{
    [SerializeField] bool character_available = true;
    bool currently_available;

    [SerializeField] string character_name = "???";
    [SerializeField] float max_running_speed2 = 30.0f;
    float current_max_running_speed;

    [SerializeField] float running_acceleration2 = 30f;
    float current_running_acceleration;

    [SerializeField] float in_air_acceleration = 30f;
    float current_in_air_acceleration;

    [SerializeField] float jumping_power = 8.0f;
    float current_jumping_power;

    [SerializeField] float running_friction_factor = 0.93f;
    float current_running_friction_factor;

    [SerializeField] float in_air_friction_factor = 0.93f;
    float current_in_air_friction_factor;

    [SerializeField] GameObject playable_character_prefab;
    [SerializeField] GameObject face_ui_icon_prefab;
    [SerializeField] GameObject common_collectible_prefab;
    [SerializeField] GameObject life_collectible_prefab;

    public static void ResetAllCharacters()
    {
        List<Character> all_chars = GetAllCharacters();

        foreach(Character c in all_chars)
        {
            c.currently_available = c.character_available;
            c.current_max_running_speed = c.max_running_speed2;
            c.current_running_acceleration = c.running_acceleration2;
            c.current_in_air_acceleration = c.in_air_acceleration;
            c.current_jumping_power = c.jumping_power;
            c.current_running_friction_factor = c.running_friction_factor;
            c.current_in_air_friction_factor = c.in_air_friction_factor;
        }
    }

    static Character current_selected_character;

    public static List<Character> GetAllCharacters()
    {
        return new List<Character>(Resources.LoadAll<Character>(""));
    }

    public static List<Character> GetAllAvailableCharacters()
    {
        Character[] characters = Resources.LoadAll<Character>("");

        List<Character> result = new List<Character>();
        foreach(Character c in characters)
        {
            if (c.GetCharacterAvailable())
            {
                result.Add(c);
            }
        }

        return result;
    }

    public static Character GetCurrentSelectedCharacter()
    {
        InitIfNecessary();

        return current_selected_character;
    }

    public static void ChangeCharacter(Character new_selected_character)
    {
        bool new_char = new_selected_character != current_selected_character;
        current_selected_character = new_selected_character;
        TransitionManager.RequestFlash();

        if (new_char)
            ArborEventBus.Publish(new EventCharacterChanged(new_selected_character));
    }

    public static Character GetCharacterByName(string character_name)
    {
        if (character_name == null)
            return null;

        character_name = character_name.ToLowerInvariant().Trim();

        List<Character> all_characters = GetAllAvailableCharacters();
        foreach (Character c in all_characters)
        {
            if (c.GetCharacterAvailable() && c.character_name.Trim().ToLowerInvariant().Equals(character_name.Trim().ToLowerInvariant()))
                return c;
        }

        return null;
    }

    public static void ChangeCharacterByName(string character_name)
    {
        Character c = GetCharacterByName(character_name);
        if (c == null)
            return;

        ChangeCharacter(c);
    }

    public static bool DoesCharacterWithNameExist(string character_name, bool check_unavailable_characters)
    {
        List<Character> all_characters = GetAllCharacters();
        foreach(Character c in all_characters)
        {
            if (check_unavailable_characters == false)
            {
                if (c.GetCharacterAvailable() == false)
                    continue;
            }

            if (c.character_name.Trim().ToLowerInvariant().Equals(character_name.Trim().ToLowerInvariant()))
                return true;
        }

        return false;
    }

    public static void ChangeAvailabilityOfCharacter(string character_name, bool should_be_available)
    {
        List<Character> all_characters = GetAllCharacters();
        foreach (Character c in all_characters)
        {
            if (c.character_name.Trim().ToLowerInvariant().Equals(character_name.Trim().ToLowerInvariant()))
            {
                c.SetCharacterAvailable(should_be_available);
            }
        }
    }

    public static string GetPrettyStringOfAllCharacters(bool even_unavailable_characters)
    {
        string result = "";
        List<Character> all_characters = GetAllAvailableCharacters();
        if (even_unavailable_characters)
            all_characters = GetAllCharacters();

        foreach (Character c in all_characters)
        {
            result += c.character_name + ",";
        }

        return result;
    }

    public static void InitIfNecessary()
    {
        if (current_selected_character != null)
            return;

        GameSettings game_settings = GameSettings.GetGameSettings();
        if(game_settings != null)
        {
            if (game_settings.GetStartingCharacter() != null)
            {
                current_selected_character = game_settings.GetStartingCharacter();
            }
        }

        if(current_selected_character == null)
        {
            List<Character> available_chars = GetAllAvailableCharacters();
            if (available_chars.Count > 0)
            {
                //current_selected_character = available_chars[UnityEngine.Random.Range(0, available_chars.Count)];
                current_selected_character = available_chars[0];
            }
            else
                Debug.LogError("FAILED to find any available playable characters. Please create a new character by watching this video (" + @"https://f002.backblazeb2.com/file/sharex-hN8T5vpN8wZGmmwU/2025/February/13/14/28/22/708/453a3037-313d-4ffc-8040-5d8f39b51129/Unity_OgenFNNAGX.mp4" + ")");
        }  
    }

    public bool IsValid()
    {
        if (playable_character_prefab == null)
            return false;

        return true;
    }

    public float GetJumpPower()
    {
        return current_jumping_power;
    }

    public void SetJumpPower(float j)
    {
        current_jumping_power = j;
    }

    public float GetRunningFrictionFactor()
    {
        return current_running_friction_factor;
    }

    public void SetRunningFrictionFactor(float f)
    {
        current_running_friction_factor = f;
    }

    public float GetInAirFrictionFactor()
    {
        return current_in_air_friction_factor;
    }

    public void SetInAirFrictionFactor(float f)
    {
        current_in_air_friction_factor = f;
    }

    public string GetCharacterName()
    {
        return character_name;
    }

    public bool GetCharacterAvailable()
    {
        return currently_available;
    }

    public void SetCharacterAvailable(bool available)
    {
        currently_available = available;
    }

    public float GetMaxRunningSpeed()
    {
        return current_max_running_speed;
    }

    public void SetMaxRunningSpeed(float s)
    {
        current_max_running_speed = s;
    }

    public float GetRunningAcceleration()
    {
        return current_running_acceleration;
    }

    public void SetRunningAcceleration(float a)
    {
        current_running_acceleration = a;
    }

    public float GetInAirAcceleration()
    {
        return current_in_air_acceleration;
    }

    public void SetInAirAcceleration(float a)
    {
        current_in_air_acceleration = a;
    }

    public GameObject GetPlayableCharacterPrefab()
    {
        return playable_character_prefab;
    }

    public GameObject GetFaceUILivesPrefab()
    {
        return face_ui_icon_prefab;
    }
}

public class EventCharacterChanged
{
    public Character new_character;
    public EventCharacterChanged(Character _new_char) { new_character = _new_char; }
}