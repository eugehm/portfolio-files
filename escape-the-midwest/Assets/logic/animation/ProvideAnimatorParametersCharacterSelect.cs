using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProvideAnimatorParametersCharacterSelect : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        /* Disable any extra cameras that might be on the model */
        HashSet<Camera> cameras = new HashSet<Camera>();
        Utilities.GetComponentsInDescendents(transform, ref cameras);
        foreach (Camera c in cameras)
            c.enabled = false;

        anim = GetComponent<Animator>();
    }

    void Update()
    {
        GameObject hovered_character_object = CharacterSelectManager.GetHoveredCharacterObject();
        anim.SetBool("in_character_select", true);
        anim.SetBool("hovered_in_character_select", hovered_character_object == gameObject);
    }
}
