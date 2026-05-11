using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public static Vector3 GetPlayerPosition() { return instance.transform.position; }
    public static Vector3 GetPlayerDirection() { return instance.transform.rotation.eulerAngles; }
    public static Quaternion GetPlayerRotation() { return instance.transform.rotation; }

    static List<Vector3> ground_raycast_offsets = new List<Vector3>();
    public static List<Vector3> GetGroundRaycastOffsets() { return ground_raycast_offsets; }

    public HashSet<string> stay_put_requests = new HashSet<string>();
    public static void RegisterStayPutRequest(string request_name) {
        request_name = request_name.Trim().ToLowerInvariant();
        instance.stay_put_requests.Add(request_name); 
    }
    public static void UnregisterStayPutRequest(string request_name) {
        request_name = request_name.Trim().ToLowerInvariant();

        if (instance.stay_put_requests.Contains(request_name))
            instance.stay_put_requests.Remove(request_name); 
    }
    public static bool StayPutRequestExists() { return instance == null || instance.stay_put_requests.Count > 0; }

    GameSettings game_settings;

    PlayerControllerState current_state;

    public static Vector3 ground_point;
    public static bool on_ground = true;
    public Vector3 initial_scale = Vector3.one;
    
    // Hack : needed to prevent 3D model from freaking out on first SetParent()
    public float ground_transform_cooldown = 0.25f;

    public static string GetCurrentState()
    {
        if (instance == null || instance.current_state == null)
            return "???";

        return instance.current_state.GetStateName();
    }

    public static GameObject GetPlayerGameobject()
    {
        if (instance == null)
            return null;
        return instance.gameObject;
    }

    public static Vector3 GetPlayerInput()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        /* Mobile controls */
        if (WebManager.IsMobileDeviceCurrently())
        {
            Vector2 delta = MobileTouchControlsCanvas.GetNormDeltaBottomLeftQuadrant();
            input = (new Vector3(delta.x, 0, delta.y));
            if (input.magnitude > 1.0f)
                input = input.normalized;
        }

        if (PlayerController.StayPutRequestExists())
            input = Vector3.zero;

        return input;
    }

    void Awake()
    {
        instance = this;

        initial_scale = transform.localScale;

        ground_raycast_offsets.Add(Vector3.zero);
        ground_raycast_offsets.Add(Vector3.right * 0.5f);
        ground_raycast_offsets.Add(Vector3.right * -0.5f);
        ground_raycast_offsets.Add(Vector3.forward * 0.5f);
        ground_raycast_offsets.Add(Vector3.forward * -0.5f);

        transform.position += UnityEngine.Random.insideUnitSphere;

        GetComponent<Rigidbody>().linearVelocity = transform.forward;

        SwitchToState(new PlayerGameplayState());
    }

    public static void SwitchToState(PlayerControllerState new_state)
    {
        if (instance.current_state != null)
            instance.current_state.OnDestroy();

        instance.current_state = new_state;
        instance.current_state.OnStart(instance);
    }

    void FixedUpdate()
    {
        if (current_state != null)
            current_state.OnFixedUpdate();
    }

    private void Update()
    {
        if (current_state != null)
            current_state.OnUpdate();
    }
}

public abstract class PlayerControllerState
{
    public abstract string GetStateName();
    public virtual void OnStart(PlayerController _player_controller) {}
    public virtual void OnUpdate() {}
    public virtual void OnFixedUpdate() { }
    public virtual void OnDestroy() { }
}

public class PlayerGameplayState : PlayerControllerState
{
    Quaternion desired_rotation;
    float coyote_timer = 0.3f;
    float jump_desire_timer = 0.0f;

    GameSettings game_settings;
    Rigidbody rb;

    PlayerController player_controller;

    Subscription<EventPlayerShouldKO> sub_EventPlayerShouldKO;
    Subscription<EventPlayerVictory> sub_EventPlayerVictory;

    public override string GetStateName()
    {
        return "gameplay";
    }

    public override void OnStart(PlayerController _player_controller)
    {
        player_controller = _player_controller;
        player_controller.transform.SetParent(null);
        player_controller.transform.localScale = Vector3.one;

        game_settings = GameSettings.GetGameSettings();
        rb = player_controller.transform.GetComponent<Rigidbody>();
        PlayerController.ground_point = player_controller.transform.position;

        player_controller.stay_put_requests.Clear();
        GameplayCamera.RemoveSpecialRequest("ko");

        sub_EventPlayerShouldKO = ArborEventBus.Subscribe<EventPlayerShouldKO>(OnEventPlayerShouldKO);
        sub_EventPlayerVictory = ArborEventBus.Subscribe<EventPlayerVictory>(OnEventPlayerVictory);
        TransitionManager.RequestDarken(false);

        player_controller.transform.rotation = Quaternion.identity;
        rb.isKinematic = false;
        Vector3 desired_respawn_point = CheckpointFlag.GetActiveCheckpointFlagPosition();
        Vector3 random_offset = UnityEngine.Random.insideUnitSphere * 0.5f;
        random_offset = new Vector3(random_offset.x, 0.0f, random_offset.z);
        player_controller.transform.position = desired_respawn_point + Vector3.right + random_offset;
        ArborEventBus.Publish(new EventPlayerRespawned());
        ArborEventBus.Publish(new EventGameplayCameraWarpRequest());
        rb.linearVelocity = Vector3.zero;
    }

    void OnEventPlayerShouldKO (EventPlayerShouldKO e)
    {
        PlayerController.SwitchToState(new PlayerKOState(e.cause));
    }

    void OnEventPlayerVictory (EventPlayerVictory e)
    {
        if(PlayerController.GetCurrentState() == "gameplay")
            PlayerController.SwitchToState(new PlayerVictoryState());
    }

    public override void OnUpdate()
    {
        if (Camera.main == null)
            return;

        if (PlayerController.StayPutRequestExists())
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }

        if(game_settings == null || game_settings.GetShouldAllowCharacterChangeWithEscapeKey())
        {
            if ((Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Escape)) && PlayerController.on_ground && !PlayerController.StayPutRequestExists())
                ArborCoroutineManager.ArborStartCoroutine(CharacterSelectManager.DoCharacterSelection());
        }

        if (CharacterSelectManager.IsCharSelectActive())
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        RaycastHit hit;
        List<Vector3> ground_raycast_offsets = PlayerController.GetGroundRaycastOffsets();

        Transform ground_transform_found = null;

        for (int i = 0; i < ground_raycast_offsets.Count; i++)
        {
            if (Physics.Raycast(player_controller.transform.position + ground_raycast_offsets[i], Vector3.down, out hit, 1.05f))
            {
                PlayerController.ground_point = hit.point;
                PlayerController.on_ground = true;

                if (ground_transform_found == null)
                    ground_transform_found = hit.transform;
                break;
            }
            else
            {
                PlayerController.on_ground = false;
            }
        }

        Vector3 input = PlayerController.GetPlayerInput();

        // Hack : needed to prevent 3D model from freaking out on first SetParent()
        // This is the issue. It goes away if player moves around a bit : 
        // https://f002.backblazeb2.com/file/sharex-hN8T5vpN8wZGmmwU/2025/01/29/09/46/02/0f62062f-3a8c-40d8-8b5e-c0c1cca0ef9c/Unity_NSuOjXRR25.mp4
        if (player_controller.ground_transform_cooldown > 0) {
            if (input.x != 0.0f || input.z != 0.0f)
                player_controller.ground_transform_cooldown -= Time.deltaTime;
        }
        else
        {
            bool should_set_parent = true;

            if (ground_transform_found != null)
            {
                ShrinkOnTouch shrink_on_touch = ground_transform_found.GetComponent<ShrinkOnTouch>();
                PreventPlayerParenting prevent_player_parenting = ground_transform_found.GetComponent<PreventPlayerParenting>();

                if (shrink_on_touch != null)
                    should_set_parent = false;
                if (prevent_player_parenting != null)
                    should_set_parent = false;
            }

            if(should_set_parent)
                player_controller.transform.SetParent(ground_transform_found);
        }

        if (PlayerController.on_ground && rb.linearVelocity.y < 3.0f)
            coyote_timer = 0.3f;
        else
            coyote_timer -= Time.deltaTime;

        jump_desire_timer -= Time.deltaTime;

        /* Jump Desire */
        if (Input.GetKeyDown(KeyCode.Space) || MobileTouchControlsCanvas.IsBottomRightQuadrantPressedThisFrame())
        {
            if(!PlayerController.StayPutRequestExists())
            {
                jump_desire_timer = 0.3f;
            }
        }

        /* Actual Jump */
        if (coyote_timer > 0.0f && jump_desire_timer > 0.0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Character.GetCurrentSelectedCharacter().GetJumpPower(), rb.linearVelocity.z);
            coyote_timer = 0.0f;
            jump_desire_timer = 0.0f;

            //ArborEventBus.Publish(new EventSoundEvent($"vocal_{Character.GetCurrentSelectedCharacter().GetCharacterName()}_jump", Vector3.zero));
        }

        if (rb.linearVelocity.y > 0 && !PlayerController.StayPutRequestExists() && (Input.GetKeyUp(KeyCode.Space) || MobileTouchControlsCanvas.IsBottomRightQuadrantReleasedThisFrame()))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
        }

        // Rotate Aesthetic
        Vector3 cam_forward_xz = (new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z)).normalized;
        Vector3 cam_right_xz = (new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z)).normalized;

        Vector3 xz_acceleration_vector = cam_right_xz * input.x + cam_forward_xz * input.z;
        if (xz_acceleration_vector.magnitude > 0.01f)
        {
            Quaternion new_rotation = Quaternion.identity;
            new_rotation.SetLookRotation(xz_acceleration_vector.normalized, Vector3.up);
            desired_rotation = new_rotation;
        }

        player_controller.transform.rotation = Quaternion.Slerp(player_controller.transform.rotation, desired_rotation, 0.2f * Time.deltaTime * 60.0f);
    }

    public override void OnFixedUpdate()
    {
        if(Camera.main == null)
            return;

        if (PlayerController.StayPutRequestExists())
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            return;
        }

        if (CharacterSelectManager.IsCharSelectActive())
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 input = PlayerController.GetPlayerInput();
        Vector3 cam_forward_xz = (new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z)).normalized;
        Vector3 cam_right_xz = (new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z)).normalized;

        Vector3 xz_acceleration_vector = cam_right_xz * input.x + cam_forward_xz * input.z;

        // Acceleration
        Character current_character = Character.GetCurrentSelectedCharacter();
        float max_speed = current_character.GetMaxRunningSpeed();
        float running_acceleration_factor = current_character.GetRunningAcceleration();
        float in_air_acceleration_factor = current_character.GetInAirAcceleration();
        float running_friction_factor = current_character.GetRunningFrictionFactor();
        float in_air_friction_factor = current_character.GetInAirFrictionFactor();

        if (PlayerController.on_ground == false)
        {
            max_speed += 1;
            xz_acceleration_vector *= in_air_acceleration_factor;
        }
        else
        {
            xz_acceleration_vector *= running_acceleration_factor;
        }

        rb.AddForce(xz_acceleration_vector, ForceMode.Acceleration);

        // constraints
        Vector3 xz_velocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // friction
        if (PlayerController.on_ground)
            xz_velocity *= Mathf.Pow(running_friction_factor, Time.fixedDeltaTime * 40.0f);
        else
            xz_velocity *= Mathf.Pow(in_air_friction_factor, Time.fixedDeltaTime * 40.0f);

        if (xz_velocity.magnitude > max_speed)
        {
            xz_velocity = xz_velocity.normalized * max_speed;
        }

        rb.linearVelocity = new Vector3(xz_velocity.x, rb.linearVelocity.y, xz_velocity.z);
    }

    public override void OnDestroy()
    {
        if (sub_EventPlayerShouldKO != null)
            ArborEventBus.Unsubscribe(sub_EventPlayerShouldKO);
        if (sub_EventPlayerVictory != null)
            ArborEventBus.Unsubscribe(sub_EventPlayerVictory);
    }
}

public class PlayerIntroState : PlayerControllerState
{
    public override string GetStateName()
    {
        return "intro";
    }
}

public class PlayerKOState : PlayerControllerState
{
    PlayerController pc;
    Rigidbody rb;

    float timer = 0.75f;

    KO_CAUSE cause = KO_CAUSE.NORMAL;

    public PlayerKOState(KO_CAUSE _cause)
    {
        cause = _cause;
    }

    public override string GetStateName()
    {
        if (cause == KO_CAUSE.FALLING)
            return "falling_ko";

        return "ko";
    }

    public override void OnStart(PlayerController _player_controller)
    {
        pc = _player_controller;
        rb = pc.GetComponent<Rigidbody>();
        ArborEventBus.Publish(new EventPlayerKO());
        rb.isKinematic = true;
        pc.ground_transform_cooldown = 0.25f;

        Vector3 current_camera_xz = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        current_camera_xz = current_camera_xz.normalized;

        Vector3 cam_dir = current_camera_xz - Vector3.up * 0.25f;
        Vector3 cam_pos = PlayerController.GetPlayerPosition() - cam_dir * 6;

        GameplayCamera.AddSpecialRequest(new SpecialCameraRequest()
        {
            id = "ko",
            position = cam_pos,
            direction = cam_dir,
            ease_factor = 1.0f,
            priority = 10000
        });

        if(cause == KO_CAUSE.TIMEOVER)
            ArborEventBus.Publish(new EventSoundEvent("vocal_timeover", Vector3.zero));
        else
            ArborEventBus.Publish(new EventSoundEvent("vocal_lifelost", Vector3.zero));

        ArborCoroutineManager.ArborStartCoroutine(DoAnimation());
    }

    IEnumerator DoAnimation()
    {
        while (CharacterSelectManager.IsCharSelectActive())
            yield return null;

        // Wait for KO animation to end
        yield return new WaitForSeconds(3.0f);

        if (UIManager.GetRemainingLives() > 0)
        {
            UIManager.ChangeLivesAmount(-1);
            ArborEventBus.Publish(new EventLivesChanged());
            TransitionManager.RequestDarken(true);

            yield return new WaitForSecondsRealtime(1.0f);
            PlayerController.SwitchToState(new PlayerGameplayState());
        }
        else
        {
            yield return ArborCoroutineManager.ArborDoCoroutine(BigMessageManager.ShowGameOverMessage());
            PlayerController.SwitchToState(new PlayerGameOverState());
        }
    }

    public override void OnUpdate()
    {
        if (pc.transform.parent != null)
        {
            pc.transform.SetParent(null);
            pc.transform.localScale = pc.initial_scale;
        }
    }

    public override void OnDestroy()
    {
        
    }
}

public class PlayerGameOverState : PlayerControllerState
{
    public override string GetStateName()
    {
        ContinueSequenceStatus continue_sequence_status = GameOverCam.GetStatus();
        if (continue_sequence_status == ContinueSequenceStatus.GAME_OVER)
            return "game_over";
        else if (continue_sequence_status == ContinueSequenceStatus.CONTINUE_COUNTDOWN)
            return "continue";
        else if (continue_sequence_status == ContinueSequenceStatus.RETRY)
            return "retry";

        return "game_over";
    }

    Subscription<EventRetry> sub_OnEventRetry;

    PlayerController pc;

    public override void OnStart(PlayerController _player_controller)
    {
        pc = _player_controller;
        sub_OnEventRetry = ArborEventBus.Subscribe<EventRetry>(OnEventRetry);
        ArborEventBus.Publish(new EventGameOver());

        DoGameOverSequence();
    }

    void OnEventRetry(EventRetry e)
    {
        PlayerController.SwitchToState(new PlayerGameplayState());
    }

    public override void OnDestroy()
    {
        if (sub_OnEventRetry != null)
            ArborEventBus.Unsubscribe(sub_OnEventRetry);
    }

    void DoGameOverSequence()
    {
        // Move player character into spotlight
        pc.transform.position = new Vector3(-999, 997.75f, 1003.87f);
        pc.transform.rotation = Quaternion.identity;
        pc.GetComponent<Rigidbody>().isKinematic = true;

        GameOverCam.ShowGameOver();
    }
}

public class PlayerVictoryState : PlayerControllerState
{
    PlayerController pc;
    Rigidbody rb;

    float timer = 0.75f;

    public PlayerVictoryState()
    {

    }

    public override string GetStateName()
    {
        return "victory";
    }

    public override void OnStart(PlayerController _player_controller)
    {
        pc = _player_controller;
        rb = pc.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        pc.ground_transform_cooldown = 0.25f;

        Vector3 current_camera_xz = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        current_camera_xz = current_camera_xz.normalized;

        Vector3 cam_dir = current_camera_xz - Vector3.up * 0.25f;
        Vector3 cam_pos = PlayerController.GetPlayerPosition() - cam_dir * 6;

        GameplayCamera.AddSpecialRequest(new SpecialCameraRequest()
        {
            id = "victory",
            position = cam_pos,
            direction = cam_dir,
            ease_factor = 1.0f,
            priority = 10000
        });

        //ArborEventBus.Publish(new EventSoundEvent("vocal_victory", Vector3.zero));


        ArborCoroutineManager.ArborStartCoroutine(DoAnimation());
    }

    IEnumerator DoAnimation()
    {
        while (CharacterSelectManager.IsCharSelectActive())
            yield return null;

        TransitionManager.RequestFlash();

        ArborEventBus.Publish(new EventSoundEvent($"vocal_victory", Vector3.zero));

        yield return null;
        pc.transform.LookAt(Camera.main.transform, Vector3.up);

        // Wait for KO animation to end
        yield return new WaitForSeconds(3.0f);

        yield return ArborCoroutineManager.ArborDoCoroutine(BigMessageManager.ShowTheEndMessage());

        TransitionManager.RequestDarken(true);

        yield return new WaitForSeconds(4.0f);
        GameplayCamera.RemoveSpecialRequest("victory");

        SceneManager.LoadScene("main_menu");
    }

    public override void OnUpdate()
    {
        if (pc.transform.parent != null)
        {
            pc.transform.SetParent(null);
            pc.transform.localScale = pc.initial_scale;
        }
    }

    public override void OnDestroy()
    {
        GameplayCamera.RemoveSpecialRequest("victory");
    }
}

public enum KO_CAUSE { NORMAL, FALLING, TIMEOVER };
public class EventPlayerShouldKO {

    public KO_CAUSE cause = KO_CAUSE.NORMAL;
    public EventPlayerShouldKO(KO_CAUSE _cause) { cause = _cause; }

}
public class EventPlayerRespawned { }
public class EventPlayerVictory { }

public class EventPlayerKO { }
public class EventGameOver { }
public class EventTheEnd { }