using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    static CharacterSelectManager instance;

    GameObject contents;

    public Camera cam;
    public GameObject character_pillar_prefab;
    public Canvas canvas;
    public Text text;

    static bool char_select_active = false;
    public static bool IsCharSelectActive() { return char_select_active; }

    static int current_hovered_slot = 0;
    static int number_of_slots = 8;
    List<GameObject> character_view_objects = new List<GameObject>();
    public static GameObject GetHoveredCharacterObject()
    {
        if (current_hovered_slot >= instance.character_view_objects.Count)
            return null;

        return instance.character_view_objects[current_hovered_slot % number_of_slots];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;

        contents = GameObject.Find("contents");
        contents.SetActive(false);
    }

    void Update()
    {
        if (char_select_active == false)
        {
            cam.enabled = false;
            canvas.enabled = false;
            return;
        }
        else
        {
            cam.enabled = true;
            canvas.enabled = true;
        }

        /* Text */
        List<Character> available_characters = Character.GetAllAvailableCharacters();
        Character current_hovered_char = available_characters[current_hovered_slot];
        text.text = current_hovered_char.GetCharacterName();

        /* Camera movement */
        float t_delta = (2 * Mathf.PI) / available_characters.Count;
        float t_pos = current_hovered_slot * t_delta;

        Vector3 xz_position = new Vector3(Mathf.Cos(t_pos), 0, Mathf.Sin(t_pos));
        /* Hack to deal with annoying slerp math */
        /* Otherwise we get this : https://f002.backblazeb2.com/file/sharex-hN8T5vpN8wZGmmwU/2025/February/13/13/41/14/070/012f0bed-d4f1-448b-932a-5e526cf204b4/Unity_5EAKnET7Vd.mp4 */
        if (number_of_slots == 2)
        {
            float hack_t = 1.2f * current_hovered_slot;
            xz_position = new Vector3(Mathf.Cos(hack_t), 0, Mathf.Sin(hack_t));
        }

        float radius = 7.0f;
        Vector3 final_pos = xz_position * radius + Vector3.up * 1.8f;
        cam.transform.localPosition = Vector3.Slerp(cam.transform.localPosition, final_pos, 0.05f);
        cam.transform.LookAt(new Vector3(-888, 889, -888), Vector3.up);    
    }


    public static IEnumerator DoCharacterSelection()
    {
        if (char_select_active)
            yield break;

        List<Character> available_characters = Character.GetAllAvailableCharacters();

        /* Configure */
        number_of_slots = available_characters.Count;
        if (number_of_slots <= 1)
        {
            ToastManager.RequestToast("There are no other characters...", ToastType.NORMAL);
            char_select_active = false;
            yield break;
        }

        char_select_active = true;
        TransitionManager.RequestFlash();

        current_hovered_slot = current_hovered_slot % number_of_slots;
        instance.character_view_objects.Clear();
        instance.cam.transform.localPosition = Vector3.up * 10.0f;
        List<GameObject> pillars = new List<GameObject>();

        float t_delta = (2 * Mathf.PI) / number_of_slots;

        for (int i = 0; i < number_of_slots; i++)
        {
            Character character = available_characters[i];

            float t_pos = i * t_delta;
            Vector3 xz_position = new Vector3(Mathf.Cos(t_pos) * 3.0f, 0, Mathf.Sin(t_pos) * 3.0f);

            /* Hack to deal with annoying slerp math */
            /* Otherwise we get this : https://f002.backblazeb2.com/file/sharex-hN8T5vpN8wZGmmwU/2025/February/13/13/41/14/070/012f0bed-d4f1-448b-932a-5e526cf204b4/Unity_5EAKnET7Vd.mp4 */
            if (number_of_slots == 2)
            {
                float hack_t = 1.2f * i;
                xz_position = new Vector3(Mathf.Cos(hack_t) * 3.0f, 0, Mathf.Sin(hack_t) * 3.0f);
            }

            /* Pillar */
            GameObject pillar = GameObject.Instantiate(instance.character_pillar_prefab, instance.contents.transform);
            pillar.transform.localPosition = xz_position + Vector3.up * 2.4f;
            pillars.Add(pillar);

            /* Character */
            GameObject character_view = GameObject.Instantiate(character.GetPlayableCharacterPrefab(), instance.contents.transform);
            character_view.transform.localPosition = xz_position;
            character_view.transform.LookAt(character_view.transform.position + xz_position, Vector3.up);

            ProvideAnimatorParameters playable_anims = character_view.GetComponent<ProvideAnimatorParameters>();
            if(playable_anims != null )
                Destroy(playable_anims);

            ProvideAnimatorParametersCharacterSelect char_select_anims = character_view.GetComponent<ProvideAnimatorParametersCharacterSelect>();
            if (char_select_anims == null)
                char_select_anims = character_view.AddComponent<ProvideAnimatorParametersCharacterSelect>();

            instance.character_view_objects.Add(character_view);
        }

        ArborEventBus.Publish(new EventCharacterSelectBegin());
        instance.contents.SetActive(true);
        
        while(true)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                current_hovered_slot++;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                current_hovered_slot--;
            }

            if (current_hovered_slot < 0)
                current_hovered_slot = number_of_slots - 1;
            current_hovered_slot = current_hovered_slot % number_of_slots;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                break;
            yield return null;
        }

        instance.contents.SetActive(false);
        foreach (GameObject g in instance.character_view_objects)
            Destroy(g);
        instance.character_view_objects.Clear();

        foreach (GameObject g in pillars)
            Destroy(g);
        pillars.Clear();

        Character chosen_character = available_characters[current_hovered_slot];
        Character.ChangeCharacter(chosen_character);

        TransitionManager.RequestFlash();

        ArborEventBus.Publish(new EventCharacterSelectEnd());
        char_select_active = false;
    }
}

public class EventCharacterSelectBegin { };
public class EventCharacterSelectEnd { };