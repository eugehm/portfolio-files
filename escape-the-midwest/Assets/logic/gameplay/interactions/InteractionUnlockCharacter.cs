using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnlockOperation { UNLOCK_CHARACTER, LOCK_CHARACTER };

public class InteractionUnlockCharacter : Interaction
{
    [SerializeField] bool only_ever_run_once = false;
    [SerializeField] string name_of_character = "";
    [SerializeField] UnlockOperation operation = UnlockOperation.UNLOCK_CHARACTER;

    public override bool OnlyEverRunOnce()
    {
        return only_ever_run_once;
    }

    public override bool IsInteractionBlocking()
    {
        return false;
    }

    public override bool ShouldHideCanvasDuring()
    {
        return false;
    }

    protected override IEnumerator _OnInteract()
    {
        if (name_of_character == null || name_of_character.Trim() == "")
        {
            Utilities.LogErrorForStudents("InteractionUnlockCharacter", gameObject, "You forgot to provide the name of a character to unlock.");
            yield break;
        }

        name_of_character = name_of_character.ToLower().Trim();

        if (Character.DoesCharacterWithNameExist(name_of_character, true) == false)
        {
            string message = $"The character with name [{name_of_character}] does not exist. Available chars include {Character.GetPrettyStringOfAllCharacters(true)}";
            
            Utilities.LogErrorForStudents("InteractionUnlockCharacter", gameObject, message);
            yield break;
        }

        Character.ChangeAvailabilityOfCharacter(name_of_character, operation == UnlockOperation.UNLOCK_CHARACTER);
    }

    public override IEnumerator OnFinished()
    {
        yield break;
    }
}
