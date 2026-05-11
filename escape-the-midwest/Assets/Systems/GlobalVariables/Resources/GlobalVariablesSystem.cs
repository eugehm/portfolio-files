using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GlobalVariablesSystem : MonoBehaviour
{
    [SerializeField] SerializableDictionary<string, string> data_store = new SerializableDictionary<string, string>();

    /* Replace variables represented by {variable_name} with the actual values. */
    public static string ConvertVariablesInString(string content)
    {
        if (content == null || content == "")
            return content;

        return Regex.Replace(content, @"\{(.*?)\}", match =>
        {
            var key = match.Groups[1].Value.Trim();
            return instance.data_store.TryGetValue(key, out var value) ? value : "???";
        });
    }

    static GlobalVariablesSystem instance;

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

        ArborEventBus.Subscribe<EventPlayerKO>(OnEventPlayerKO);
        ArborEventBus.Subscribe<EventGameOver>(OnEventGameOver);
        ArborEventBus.Subscribe<EventGameplayStart>(OnEventGameplayStart);
    }

    static int number_of_deaths = 0;
    static int number_of_gameovers = 0;
    static float gameplay_start_timestamp = 0;
    void OnEventPlayerKO(EventPlayerKO e)
    {
        number_of_deaths++;
    }

    void OnEventGameOver(EventGameOver e)
    {
        number_of_gameovers++;
    }

    void OnEventGameplayStart(EventGameplayStart e)
    {
        gameplay_start_timestamp = Time.time;
    }


    public static void Clear()
    {
        instance.data_store.Clear();
        number_of_deaths = 0;
        number_of_gameovers = 0;
    }

    public static void SetVariable(string variable_name, string variable_value)
    {
        if (variable_name == null)
            return;
        if (variable_value == null)
            variable_value = "";

        variable_name = variable_name.ToLowerInvariant().Trim();

        instance.data_store[variable_name] = variable_value;
    }

    public static void SetVariable(string variable_name, float variable_value)
    {
        if (variable_name == null)
            return;

        variable_name = variable_name.ToLowerInvariant().Trim();

        instance.data_store[variable_name] = variable_value.ToString();
    }

    public static void SetVariable(string variable_name, bool variable_value)
    {
        if (variable_name == null)
            return;

        variable_name = variable_name.ToLowerInvariant().Trim();

        instance.data_store[variable_name] = variable_value.ToString();
    }

    public static float GetVariableNumber(string variable_name)
    {
        variable_name = variable_name.ToLowerInvariant().Trim();

        if(instance.data_store.ContainsKey(variable_name) == false)
        {
            Utilities.LogErrorForStudents("GlobalVariableSystem", null, $"Failed to find variable [{variable_name}]");
            return 0;
        }

        string val = instance.data_store[variable_name];

        float result;
        if(float.TryParse(val, out result))
        {
            return result;
        }
        else
        {
            Utilities.LogErrorForStudents("GlobalVariableSystem", null, $"Failed to float-parse variable [{variable_name}] = [{val}]");
            return 0;
        }    
    }

    public static string GetVariableString(string variable_name)
    {
        variable_name = variable_name.ToLowerInvariant().Trim();

        if (instance.data_store.ContainsKey(variable_name) == false)
        {
            Utilities.LogErrorForStudents("GlobalVariableSystem", null, $"Failed to find variable [{variable_name}]");
            return "error";
        }

        string val = instance.data_store[variable_name];
        return val;
    }

    public static bool GetVariableBool(string variable_name)
    {
        variable_name = variable_name.ToLowerInvariant().Trim();

        if (instance.data_store.ContainsKey(variable_name) == false)
        {
            Utilities.LogErrorForStudents("GlobalVariableSystem", null, $"Failed to find variable [{variable_name}]");
            return false;
        }

        string val = instance.data_store[variable_name];

        bool result;
        if(bool.TryParse(val, out result))
        {
            return result;
        }
        else
        {
            Utilities.LogErrorForStudents("GlobalVariableSystem", null, $"Failed to bool-parse variable [{variable_name}] = [{val}]");
            return false;
        }
    }

    public static Type GetVariableType(string variable_name)
    {
        variable_name = variable_name.ToLowerInvariant().Trim();

        if (instance.data_store.ContainsKey(variable_name) == false)
        {
            Utilities.LogErrorForStudents("GlobalVariableSystem", null, $"Failed to find variable [{variable_name}]");
            return null;
        }

        string val = instance.data_store[variable_name];

        float f_result;
        if (float.TryParse(val, out f_result))
        {
            return typeof(float);
        }

        bool b_result;
        if (bool.TryParse(val, out b_result))
        {
            return typeof(bool);
        }

        return typeof(string);
    }

    private void Update()
    {
        if (PlayerController.GetPlayerGameobject() == null)
            return;

        /* Player status */
        GlobalVariablesSystem.SetVariable("state", PlayerController.GetCurrentState());
        GlobalVariablesSystem.SetVariable("lives", UIManager.GetRemainingLives());
        GlobalVariablesSystem.SetVariable("continues", GameOverCam.continues_remaining);
        GlobalVariablesSystem.SetVariable("on_ground", PlayerController.on_ground);
        GlobalVariablesSystem.SetVariable("dialogue", InteractionDialogue.IsAnyDialogueRunning());
        GlobalVariablesSystem.SetVariable("character", Character.GetCurrentSelectedCharacter().GetCharacterName());

        /* Player position */
        Vector3 player_pos = PlayerController.GetPlayerPosition();
        GlobalVariablesSystem.SetVariable("x", player_pos.x);
        GlobalVariablesSystem.SetVariable("y", player_pos.y);
        GlobalVariablesSystem.SetVariable("z", player_pos.z);

        /* Player Direction (euler) */
        GlobalVariablesSystem.SetVariable("dir_x", player_pos.x);
        GlobalVariablesSystem.SetVariable("dir_y", player_pos.y);
        GlobalVariablesSystem.SetVariable("dir_z", player_pos.z);

        /* Stats */
        GlobalVariablesSystem.SetVariable("deaths", number_of_deaths);
        GlobalVariablesSystem.SetVariable("gameovers", number_of_gameovers);
        GlobalVariablesSystem.SetVariable("network_players", NetworkManager.GetNumberOfNetworkPlayers());
        GlobalVariablesSystem.SetVariable("gametime_in_seconds", GetTimeSinceGameplayStartInSeconds());
        GlobalVariablesSystem.SetVariable("gametime_in_minutes", GetTimeSinceGameplayStartInMinutes());
    }

    float GetTimeSinceGameplayStartInSeconds()
    {
        float duration_seconds = Time.time - gameplay_start_timestamp;
        return duration_seconds;
    }

    float GetTimeSinceGameplayStartInMinutes()
    {
        return GetTimeSinceGameplayStartInSeconds() / 60.0f;
    }
}
