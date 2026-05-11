using UnityEngine;

[CreateAssetMenu(fileName = "level_configuration", menuName = "3D Platformer Testbed/Create New Level")]
public class LevelConfiguration : ScriptableObject
{
    public string level_name = "???";
    public AudioClip background_music;
}
