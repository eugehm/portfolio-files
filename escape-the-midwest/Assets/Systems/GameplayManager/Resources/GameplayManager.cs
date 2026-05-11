using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    static GameplayManager instance;

    GameplayManagerState current_state = null;

    void Awake()
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

        InitializeGame();
        ArborEventBus.Subscribe<EventGameplayStart>(OnEventGameplayStart);
    }

    void OnEventGameplayStart(EventGameplayStart e)
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        Character.ResetAllCharacters();

        GameSettings game_settings = GameSettings.GetGameSettings();
        int continues = 2;
        int lives = 2;
        Color starting_fog_color = Color.white;
        float starting_fog_density = 0.0f;
        if (game_settings != null)
        {
            continues = game_settings.GetStartingContinues();
            lives = game_settings.GetStartingLives();
            Physics.gravity = new Vector3(0.0f, game_settings.GetStartingGravity(), 0.0f);
            starting_fog_color = game_settings.GetStartingFogColor();
            starting_fog_density = game_settings.GetStartingFogDensity();
        }
        GameOverCam.continues_remaining = continues;
        UIManager.SetLivesAmount(lives);
        ChangeState(new GameplayManagerStateNormal());
        RenderSettings.fog = true;
        RenderSettings.fogColor = starting_fog_color;
        RenderSettings.fogDensity = starting_fog_density;
        if (Camera.main != null)
            Camera.main.backgroundColor = starting_fog_color;
    }

    void Update()
    {
        if (current_state != null)
            current_state.Update();

        ArborEventBus.Publish(new EventTick());
    }

    void ChangeState(GameplayManagerState new_state)
    {
        if (current_state == new_state)
            return;

        if (current_state != null)
        {
            current_state.OnEnd();
        }

        current_state = new_state;

        current_state.Update();
    }
}

public abstract class GameplayManagerState
{
    public abstract void Init();
    public abstract void Update();
    public abstract void OnEnd();
}

public class GameplayManagerStateNormal : GameplayManagerState
{
    public override void Init()
    {
        
    }

    public override void Update()
    {
        // We need to spawn the player
        if (PlayerController.instance == null && SceneManager.GetActiveScene().name != "main_menu")
        {
            GameObject new_player = GameObject.Instantiate(Resources.Load<GameObject>("Player"));
            ArborEventBus.Publish(new EventGameplayStart());

            Vector3 spawn_point = CheckpointFlag.GetStartingCheckpointFlagPosition() + Vector3.up * 2.0f + Vector3.right + UnityEngine.Random.insideUnitSphere * 0.5f;
            new_player.transform.position = spawn_point;
        }

        // Check if the player has been deactivated.
        // This often happens because the player stood atop (and became parented to) another gameobject that was then deactivated.
        GameObject player_go = PlayerController.GetPlayerGameobject();
        if (player_go != null)
        {
            if (player_go.activeInHierarchy == false)
            {
                player_go.transform.SetParent(null);
                player_go.SetActive(true);
            }

            // Check if player has encountered the weird "squash" bug that sometimes occurs when jumping on objects.
            if (player_go.transform.parent == null && !player_go.transform.localScale.Equals(Vector3.one))
                player_go.transform.localScale = Vector3.one;
        }


    }

    public override void OnEnd()
    {
        
    }
}

public class EventGameplayStart { }
public class EventTick { }