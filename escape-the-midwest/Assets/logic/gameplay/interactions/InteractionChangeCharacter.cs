using System.Collections;
using UnityEngine;

public class InteractionChangeCharacter : Interaction
{
    [SerializeField] bool only_ever_run_once = false;

    [SerializeField] string character_to_switch_to = "";

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once;
    }

    public override bool IsInteractionBlocking()
    {
        return true;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return true;
    }

    protected override IEnumerator _OnInteract()
    {
        if (character_to_switch_to.Trim() == "")
        {
            Utilities.LogErrorForStudents("InteractionChangeCharacter", gameObject, "You forgot to specify a character the player will switch to.");
            yield break;
        }

        bool does_character_exist = Character.DoesCharacterWithNameExist(character_to_switch_to, false);
        if(!does_character_exist)
        {
            string characters_that_do_exist = Character.GetPrettyStringOfAllCharacters(false);
            Utilities.LogErrorForStudents("InteractionChangeCharacter", gameObject, $"Character {character_to_switch_to} does not exist. Here are the ones that are unlocked : {characters_that_do_exist}");
            yield break;
        }

        Character current_character = Character.GetCurrentSelectedCharacter();
        if (current_character.GetCharacterName().Trim().ToLowerInvariant() == character_to_switch_to.Trim().ToLowerInvariant())
        {
            ToastManager.RequestToast($"You are already {character_to_switch_to}", ToastType.NORMAL);
            yield break;
        }

        Character.ChangeCharacterByName(character_to_switch_to);

        yield break;
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
