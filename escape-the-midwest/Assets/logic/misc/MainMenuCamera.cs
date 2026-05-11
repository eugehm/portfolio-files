using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public enum MAIN_MENU_ACTION { STORY, SETTINGS, CREDITS, MORE_GAMES, QUIT };

public class MainMenuCamera : MonoBehaviour
{
    [SerializeField] Camera title_camera;
    [SerializeField] List<MainMenuChoice> menu_choices;

    [SerializeField] Text bottom_bar_text;
    [SerializeField] Image bottom_bar_right_im;
    [SerializeField] Image bottom_bar_left_im;

    bool title_logo_mode = true;
    public static bool InTitleLogoMode() { return instance.title_logo_mode; }
    int current_choice_index = 0;

    public static MainMenuCamera instance;
    public static bool DoesMainMenuCameraExist()
    {
        return instance != null;
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializeMainMenu();
        InitializeMainMenuCameras();

        /* Initialize camera */
        transition_start_position = transform.position;
        transition_start_rotation = transform.rotation;
        transition_destination = title_camera;

        bottom_bar_text.text = "";
    }

    void InitializeMainMenu()
    {
        /* Initialize Game Settings */
        GlobalVariablesSystem.Clear();
        InventoryItem.ResetInventory();

        GameSettings game_settings = GameSettings.GetGameSettings();
        Color starting_fog_color = Color.white;
        float starting_fog_density = 0.0f;
        if (game_settings != null)
        {
            starting_fog_color = game_settings.GetStartingFogColor();
            starting_fog_density = game_settings.GetStartingFogDensity();
        }
        RenderSettings.fog = true;
        RenderSettings.fogColor = starting_fog_color;
        RenderSettings.fogDensity = starting_fog_density;
        if (Camera.main != null)
            Camera.main.backgroundColor = starting_fog_color;
    }

    void InitializeMainMenuCameras()
    {
        if(title_camera == null)
        {
            Utilities.LogErrorForStudents("MainMenuCamera", gameObject, "Your \"title_camera variable is null. Please find in the inspector and fix!\"");
        }
        title_camera.enabled = false;

        /* Disable all menu choice cameras */
        foreach (MainMenuChoice choice in menu_choices)
        {
            if (choice.camera == null)
            {
                Utilities.LogErrorForStudents("MainMenuCamera", gameObject, "One of your main menu choices does not have a camera assigned. Please fix.");
                continue;
            }

            choice.camera.enabled = false;
        }
    }

    void Update()
    {
        if(title_logo_mode)
            TitleLogoMode();
        else
            MenuOptionMode();
    }

    void TitleLogoMode()
    {
        if (MainMenuCanvas.IsIntroCompleted())
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                title_logo_mode = false;

                /* Initialize first choice */
                current_choice_index = 0;
                transition_start_position = transform.position;
                transition_start_rotation = transform.rotation;
                transition_progress = 0.0f;
                MainMenuChoice current_hovered_choice = menu_choices[current_choice_index];
                transition_destination = current_hovered_choice.camera;

                bottom_bar_text.text = current_hovered_choice.display_name;
            }
        }
    }

    void MenuOptionMode()
    {
        if (InformationViewer.IsVisible())
            return;

        int previous_choice_index = current_choice_index;

        /* Selection */
        bool mouse_cursor_wants_right = Input.mousePosition.x > Screen.width * 0.65f;
        bool mouse_cursor_wants_left = Input.mousePosition.x < Screen.width * 0.35f;
        bool mouse_cursor_wants_middle = Input.mousePosition.x <= Screen.width * 0.65f && Input.mousePosition.x >= Screen.width * 0.35f;

        int player_wants_to_move = 0;
        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) || (Input.GetMouseButtonDown(0) && mouse_cursor_wants_right))
            player_wants_to_move = 1;
        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) || (Input.GetMouseButtonDown(0) && mouse_cursor_wants_left))
            player_wants_to_move = -1;

        current_choice_index += player_wants_to_move;

        /* Arrow keys and text */
        if (mouse_cursor_wants_right)
            bottom_bar_right_im.color = Color.red;
        else
            bottom_bar_right_im.color = Color.white;

        if (mouse_cursor_wants_left)
            bottom_bar_left_im.color = Color.red;
        else
            bottom_bar_left_im.color = Color.white;

        if (mouse_cursor_wants_middle)
            bottom_bar_text.color = Color.red;
        else
            bottom_bar_text.color = Color.white;
        

        /* Wrap */
        if (current_choice_index < 0)
            current_choice_index = menu_choices.Count - 1;
        current_choice_index = current_choice_index % menu_choices.Count;

        MainMenuChoice current_choice = menu_choices[current_choice_index];

        /* Confirmation */
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || (mouse_cursor_wants_middle && Input.GetMouseButtonDown(0)))
        {
            TransitionManager.RequestFlash();

            if(current_choice.action == MAIN_MENU_ACTION.STORY)
            {
                SceneManager.LoadScene("level1");
            }
            else if(current_choice.action == MAIN_MENU_ACTION.SETTINGS)
            {
                // Show settings prefab
                InformationViewer.Show("No settings are available (yet)");
            }
            else if (current_choice.action == MAIN_MENU_ACTION.CREDITS)
            {
                // Show credits view
                TextAsset ta = Resources.Load<TextAsset>("credits");
                if(ta != null)
                    InformationViewer.Show(ta.text);
            }
            else if(current_choice.action == MAIN_MENU_ACTION.MORE_GAMES)
            {
                Application.OpenURL(@"https://eecs298.com/galleries/database.html");
            }
        }

        /* Ready the camera transition */
        if (current_choice_index != previous_choice_index)
        {
            transition_start_position = transform.position;
            transition_start_rotation = transform.rotation;
            transition_progress = 0.0f;
            transition_destination = current_choice.camera;
            bottom_bar_text.text = current_choice.display_name;
        }
    }

    float transition_progress = 1.0f;
    Vector3 transition_start_position;
    Quaternion transition_start_rotation;
    Camera transition_destination;
    private void LateUpdate()
    {
        Transform cam_t = transition_destination.transform;

        if(transition_progress >= 1.0f)
        {
            transition_progress = 1.0f;
        }
        else
        {
            transition_progress += Time.deltaTime * 2.0f;
        }

        transform.position = Vector3.Lerp(transition_start_position, cam_t.position, transition_progress);
        transform.rotation = Quaternion.Slerp(transition_start_rotation, cam_t.rotation, transition_progress);
    }
}

[Serializable]
public class MainMenuChoice
{
    public MAIN_MENU_ACTION action;
    public Camera camera;
    public string display_name = "";
}
