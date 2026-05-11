/* A generic container for a character.
 * Supports the player character as well as networked characters */

using UnityEngine;

[DisallowMultipleComponent]
public class HasCharacterView : MonoBehaviour
{
    public bool networked_character_view = false;
    public NetworkPlayerView network_player_info = null;

    Subscription<EventCharacterChanged> sub_EventCharacterChanged;

    void Start()
    {
        if (networked_character_view == false)
            InitPlayerCharacter();
    }

    void InitPlayerCharacter()
    {
        Character current_character = Character.GetCurrentSelectedCharacter();
        ArborEventBus.Subscribe<EventCharacterChanged>(OnEventCharacterChanged);
        EstablishCharacterView(current_character);
    }

    public void UseCharacterWithName(string character_name)
    {

    }

    void OnEventCharacterChanged(EventCharacterChanged e)
    {
        EstablishCharacterView(e.new_character);
    }

    public void EstablishCharacterViewByName(string character_name)
    {
        if (current_character_view_character != null && current_character_view_character.GetCharacterName() == character_name)
            return;

        Character c = Character.GetCharacterByName(character_name);
        EstablishCharacterView(c);
    }

    Character current_character_view_character = null;
    void EstablishCharacterView(Character c)
    {
        // No point in setting up a character view again if we are already using that character.
        if (c == current_character_view_character)
            return;

        current_character_view_character = c;

        /* Destroy character_view if it exists */
        Transform current_char_view = transform.Find("character_view");
        if (current_char_view != null)
            Destroy(current_char_view.gameObject);

        /* Set up character view within player */
        GameObject new_character_view = GameObject.Instantiate(c.GetPlayableCharacterPrefab());
        new_character_view.name = "character_view";

        new_character_view.transform.SetParent(transform, false);
        new_character_view.transform.localPosition = new Vector3(0, -1, 0);
        new_character_view.transform.localRotation = Quaternion.identity;

        /* Add Animation Logic if it doesn't already exist */
        ProvideAnimatorParametersNetwork net_anim = new_character_view.GetComponent<ProvideAnimatorParametersNetwork>();
        ProvideAnimatorParameters player_anim = new_character_view.GetComponent<ProvideAnimatorParameters>();

        if (networked_character_view)
        {
            if (net_anim == null)
                net_anim = new_character_view.AddComponent<ProvideAnimatorParametersNetwork>();
            if (player_anim != null)
                Destroy(player_anim);

            net_anim.network_player_info = network_player_info;
        }
        else
        {
            if (player_anim == null)
                player_anim = new_character_view.AddComponent<ProvideAnimatorParameters>();
            if (net_anim != null)
                Destroy(net_anim);
        }
    }

    private void OnDestroy()
    {
        if (sub_EventCharacterChanged != null)
            ArborEventBus.Unsubscribe(sub_EventCharacterChanged);
    }
}
