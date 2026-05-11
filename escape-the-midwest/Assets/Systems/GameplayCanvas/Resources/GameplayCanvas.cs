using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayCanvas : MonoBehaviour
{
    static GameplayCanvas instance;
    [SerializeField] RectTransform interaction_cursor_rt;
    [SerializeField] Image interaction_cursor_image;
    [SerializeField] Sprite interaction_cursor_sprite_1;
    [SerializeField] Sprite interaction_cursor_sprite_2;
    [SerializeField] CanvasGroup lives_cg;
    [SerializeField] CanvasGroup interaction_panel_cg;

    public static Vector2 GetInteractionCursorPosition()
    {
        if (instance == null || instance.interaction_cursor_rt == null)
            return Vector2.zero;

        return instance.interaction_cursor_rt.position;
    }

    public static Vector2 GetInteractionCursorAnchoredPosition()
    {
        if (instance == null || instance.interaction_cursor_rt == null)
            return Vector2.zero;

        return instance.interaction_cursor_rt.anchoredPosition;
    }


    Canvas canvas;
    CanvasGroup cg;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        canvas = GetComponent<Canvas>();
        cg = GetComponent<CanvasGroup>();
        ArborEventBus.Subscribe<EventCharacterSelectBegin>(OnEventCharacterSelectBegin);
        ArborEventBus.Subscribe<EventCharacterSelectEnd>(OnEventCharacterSelectEnd);
        transform.Find("message_raw_image").gameObject.SetActive(true);
        interaction_cursor_image.sprite = interaction_cursor_sprite_1;
        cg.alpha = 1.0f;
    }

    void OnEventCharacterSelectBegin(EventCharacterSelectBegin e)
    {
        cg.alpha = 0;
        cg.interactable = false;
    }

    void OnEventCharacterSelectEnd(EventCharacterSelectEnd e)
    {
        cg.alpha = 1;
        cg.interactable = true;
    }

    InteractableGameobject previous_interactable_gameobject;

    private void Update()
    {
        string player_state = PlayerController.GetCurrentState();
        if (SceneManager.GetActiveScene().name == "main_menu" || InteractionSystem.IsRunningNoCanvasInteraction() || CharacterSelectManager.IsCharSelectActive())
        {
            cg.alpha = 0;
            return;
        }
        else
        {
            cg.alpha = 1.0f;
        }

        if(player_state == "ko" || player_state == "falling_ko" || player_state == "victory")
        {
            interaction_panel_cg.alpha = 0.0f;
        }
        else
        {
            interaction_panel_cg.alpha = 1.0f;
        }

        if (GameOverCam.IsContinueSequenceOngoing())
        {
            lives_cg.alpha = 0.0f;
        }
        else
        {
            lives_cg.alpha = 1.0f;
        }

        InteractableGameobject hovered_gameobject = InteractionSystem.GetHoveredGameobject();
        if (hovered_gameobject == null)
            interaction_cursor_rt.anchoredPosition = Vector2.one * 9999;
        else
        {
            interaction_cursor_rt.anchoredPosition = Utilities.WorldToCanvasAnchorPosition(hovered_gameobject.gameObject, canvas, Camera.main);
        }

        if(hovered_gameobject != previous_interactable_gameobject)
        {
            ResetHookesLaw();
        }
        previous_interactable_gameobject = hovered_gameobject;

        PerformClickingAnimation();
        HookesLaw();
    }


    float seconds_until_sprite_switch = 0.25f;
    void PerformClickingAnimation()
    {
        seconds_until_sprite_switch -= Time.deltaTime;
        if(seconds_until_sprite_switch <= 0)
        {
            if (interaction_cursor_image.sprite == interaction_cursor_sprite_1)
                interaction_cursor_image.sprite = interaction_cursor_sprite_2;
            else
                interaction_cursor_image.sprite = interaction_cursor_sprite_1;

            seconds_until_sprite_switch = 0.25f;
        }
    }

    float velocity = 0.0f;
    float stiffness = 0.04f;
    float dampening_factor = 0.95f;
    void HookesLaw()
    {
        float delta = 1.0f - interaction_cursor_rt.localScale.x;
        float a = delta * stiffness;
        velocity += a;
        velocity *= dampening_factor;

        interaction_cursor_rt.localScale += Vector3.one * velocity;
    }

    void ResetHookesLaw()
    {
        interaction_cursor_rt.localScale = Vector3.zero;
        velocity = 0.0f;
    }
}
