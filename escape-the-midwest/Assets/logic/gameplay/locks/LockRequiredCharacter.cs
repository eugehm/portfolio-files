using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRequiredCharacter : Lock
{
    bool locked = true;
    [SerializeField] string allowed_characters_csv = "name1,name2";
    [SerializeField] bool once_unlocked_stay_unlocked = false;

    public override bool CheckUnlocked()
    {
        return !locked;
    }

    public override IEnumerator TryLock()
    {
        if(once_unlocked_stay_unlocked)
        {
            if (locked == false)
                yield break;
        }

        /* Split the list of characters */
        List<string> allowed_character_names = new List<string>(allowed_characters_csv.Split(",", StringSplitOptions.RemoveEmptyEntries));

        Character current_char = Character.GetCurrentSelectedCharacter();
        string current_char_name = current_char.GetCharacterName().Trim().ToLowerInvariant();

        bool using_allowed_name = false;

        foreach(string allowed_name in allowed_character_names)
        {
            if(current_char_name.Equals(allowed_name.Trim().ToLowerInvariant()))
            {
                using_allowed_name = true;
            }
        }

        locked = !using_allowed_name;

        if (locked)
        {
            string denial_message = "Wrong character. Come back as ";
            int i = 0;
            foreach(string n in allowed_character_names)
            {
                if (i < allowed_character_names.Count - 1)
                    denial_message += n + " or ";
                else
                    denial_message += n + ".";

                i++;
            }
            ToastManager.RequestToast(denial_message, ToastType.NORMAL);
        }

        yield break;
    }
}
