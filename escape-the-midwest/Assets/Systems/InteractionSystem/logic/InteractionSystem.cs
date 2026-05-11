using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InteractionSystem : MonoBehaviour
{
    static InteractionSystem instance;

    InteractableGameobject hovered_gameobject;

    public static InteractableGameobject GetHoveredGameobject() { return instance.hovered_gameobject; }

    Subscription<EventPlayerShouldKO> sub_EventPlayerShouldKO;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        sub_EventPlayerShouldKO = ArborEventBus.Subscribe<EventPlayerShouldKO>(OnEventPlayerShouldKO);
    }

    void OnEventPlayerShouldKO(EventPlayerShouldKO e)
    {
        ResetSystem();
    }

    void ResetSystem()
    {
        StopAllCoroutines();
        currently_blocking_interactions = false;
        number_of_interactions_running = 0;
        number_of_interactions_requesting_no_canvas = 0;
        gameobjects_running_interactions.Clear();
    }

    void Update()
    {
        /* The InteractionSystem is not active in the main menu. Best way to tell if
         *  we're in the main menu is whether the MainMenuCamera exists (as students may rename the scene). */
        if (MainMenuCamera.DoesMainMenuCameraExist())
            return;

        DetermineHoveredGameobject();
        PerformPlayerControls();
    }

    void DetermineHoveredGameobject()
    {
        /* Find closest interactable gameobject */
        List<InteractableGameobject> interactable_gameobjects = InteractableGameobject.GetAllClickToInteractGameobjects();
        float closest_distance = float.MaxValue;
        InteractableGameobject closest_gameobject = null;
        Vector3 player_position = PlayerController.GetPlayerPosition();

        foreach(InteractableGameobject go in interactable_gameobjects)
        {
            if (CanInteractableGameobjectLaunchInteractions(go) == false)
                continue;

            float d = Vector3.Distance(go.transform.position, player_position);
            if (d < closest_distance)
            {
                closest_distance = d;
                closest_gameobject = go;
            }
        }

        /* Decide if an interaction is possible */
        const float MAX_DISTANCE = 5.0f;
        if (closest_distance > MAX_DISTANCE)
            closest_gameobject = null;

        hovered_gameobject = closest_gameobject;
    }

    void PerformPlayerControls()
    {
        if (hovered_gameobject != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(WebManager.IsMobileDeviceCurrently())
                {
                    // Mobile controls
                    Vector2 interaction_cursor_pos = GameplayCanvas.GetInteractionCursorPosition();
                    float distance = Vector2.Distance(Input.mousePosition, interaction_cursor_pos);

                    float max_distance = Mathf.Max((float)Screen.width * (1.0f / 16.0f), (float)Screen.height * (1.0f / 16.0f));

                    if(distance <= max_distance)
                    {
                        ExecuteInteractionsOnGameobject(hovered_gameobject);
                    }
                }
                else
                {
                    // Normal controls
                    ExecuteInteractionsOnGameobject(hovered_gameobject);
                }
            }
        }
    }

    public static bool CanInteractableGameobjectLaunchInteractions(InteractableGameobject go)
    {
        if (go == null)
            return false;

        if (go.enabled == false)
            return false;

        if (go.gameObject.activeSelf == false)
            return false;

        if (go.gameObject.activeInHierarchy == false)
            return false;

        if (go.AllowsFurtherSelfInteractionsBeforeFinished())
            return true;

        if (gameobjects_running_interactions.ContainsKey(go) == false)
            return true;

        int number_of_running_interactions_on_this_object = gameobjects_running_interactions[go];
        if (number_of_running_interactions_on_this_object <= 0)
            return true;

        return false;
    }

    public static bool IsRunningInteraction()
    {
        return number_of_interactions_running > 0;
    }

    public static bool IsRunningNoCanvasInteraction()
    {
        return number_of_interactions_requesting_no_canvas > 0;
    }

    public static void ExecuteInteractionsOnGameobject(InteractableGameobject go)
    {
        instance.StartCoroutine (DoExecuteInteractionsOnGameobject(go));
    }

    static bool currently_blocking_interactions = false;
    public static bool IsBlockingCurrently() { return currently_blocking_interactions; }
    static int number_of_interactions_running = 0;
    static int number_of_interactions_requesting_no_canvas = 0;
    static Dictionary<InteractableGameobject, int> gameobjects_running_interactions = new Dictionary<InteractableGameobject, int>();
    static IEnumerator DoExecuteInteractionsOnGameobject(InteractableGameobject go)
    {
        /* Check if interaction is possible at the moment */
        string current_state = PlayerController.GetCurrentState();
        if (current_state == "ko" || current_state == "falling_ko" || current_state == "continue" || current_state == "game_over" || current_state == "retry" || current_state == "victory")
            yield break;

        if (currently_blocking_interactions) // TODO : Should we allow multiple interactions to happen at once? Should we allow some interactions to prevent new ones from launching (dialogue)?
            yield break;

        /* Most interactable objects do not allow multiple interactions chains to be running on it at once. */
        if (CanInteractableGameobjectLaunchInteractions(go) == false)
            yield break;

        /* Process Interactions and Locks */
        try
        {
            List<GameplayLogicComponent> gameplay_logic_components = new List<GameplayLogicComponent>(go.gameObject.GetComponents<GameplayLogicComponent>());

            foreach (GameplayLogicComponent comp in gameplay_logic_components)
            {
                /* Check if an interaction or lock.*/
                /* If we run into a lock, we need to try the lock and pause if it fails. */
                Type comp_type = comp.GetType();

                if (comp_type.IsSubclassOf(typeof(Interaction)))
                {
                    Interaction interaction = (Interaction)comp;
                    if (interaction.OnlyEverRunOnce())
                    {
                        if (interaction.GetRunCount() > 0)
                            continue;
                    }

                    number_of_interactions_running++;
                    if (interaction.ShouldHideCanvasDuring())
                        number_of_interactions_requesting_no_canvas++;

                    if (!gameobjects_running_interactions.ContainsKey(go))
                        gameobjects_running_interactions[go] = 0;

                    gameobjects_running_interactions[go]++;

                    bool should_block_further_interactions = interaction.IsInteractionBlocking();
                    if (should_block_further_interactions)
                        currently_blocking_interactions = true;

                    try
                    {
                        yield return instance.StartCoroutine(interaction.OnInteract());
                        yield return instance.StartCoroutine(interaction.OnFinished());
                    }
                    finally { }

                    gameobjects_running_interactions[go]--;

                    if (should_block_further_interactions)
                        currently_blocking_interactions = false;
                    
                    number_of_interactions_running--;
                    if (interaction.ShouldHideCanvasDuring())
                        number_of_interactions_requesting_no_canvas--;
                }
                else if (comp_type.IsSubclassOf(typeof(Lock)))
                {
                    /* Locks must be tried, then checked for success */
                    Lock lock_to_check = (Lock)comp;
                    yield return instance.StartCoroutine(lock_to_check.TryLock());

                    bool success = lock_to_check.CheckUnlocked();

                    if(success == false)
                    {
                        break;
                    }
                }
            }
        }
        finally { } // Coroutines do not allow us to have a catch() clause -_-
    }

    private void OnDestroy()
    {
        ArborEventBus.Unsubscribe(sub_EventPlayerShouldKO);
    }
}
