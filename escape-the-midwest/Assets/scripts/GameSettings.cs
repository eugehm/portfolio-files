using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "game_settings", menuName = "3D Platformer Testbed/Create New Game Settings")]
public class GameSettings : ScriptableObject
{
    [SerializeField] Character starting_character;
    [Range(0, 9)] [SerializeField] int starting_extra_lives = 2;
    [Range(0, 5)] [SerializeField] int starting_extra_continues = 2;
    [SerializeField] float starting_gravity = -9.81f;
    [SerializeField] bool allow_character_change_with_escape_key = false;
    [SerializeField] Color starting_fog_color = Color.white;
    [SerializeField] float starting_fog_density = 0.0f;

    public Character GetStartingCharacter() { return starting_character; }
    public int GetStartingLives() { return Mathf.Min(9, starting_extra_lives); }
    public int GetStartingContinues() { return Mathf.Min(5, starting_extra_continues); }
    public float GetStartingGravity() { return starting_gravity; }
    public bool GetShouldAllowCharacterChangeWithEscapeKey() { return allow_character_change_with_escape_key; }

    public Color GetStartingFogColor() { return starting_fog_color; }
    public float GetStartingFogDensity() { return starting_fog_density; }

    public static GameSettings GetGameSettings()
    {
        GameSettings[] game_settings_assets = Resources.LoadAll<GameSettings>("");

        if(game_settings_assets.Length <= 0)
        {
            Utilities.LogErrorForStudents("GameSettings", null, "You don't have a game_settings file in your project. Use right-click inside of a Resources/ folder to create one!");
            return null;
        }

        if (game_settings_assets.Length > 1)
        {
            Utilities.LogErrorForStudents("GameSettings", null, "You have more than one game_settings file in your project, which might lead to confusion or bugs. Please delete some of them until you have only one left in your project.");
        }

        return game_settings_assets[0];
    }
}
