using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ContinueSequenceStatus { NOT_STARTED, CONTINUE_COUNTDOWN, RETRY, GAME_OVER };

public class GameOverCam : MonoBehaviour
{
    static GameOverCam instance;
    public static int continues_remaining = 2;
    public static void ShowGameOver()
    {
        instance.cam.enabled = true;
        instance.status = ContinueSequenceStatus.NOT_STARTED;
        ArborCoroutineManager.ArborStartCoroutine (instance.DoContinueSequence());
    }

    [SerializeField] CanvasGroup continue_countdown_canvas;
    [SerializeField] Text countdown_text;
    [SerializeField] Text continue_prompt_text;
    [SerializeField] Transform spotlight;
    Camera cam;
    AudioListener audio_listener;
    ContinueSequenceStatus status = ContinueSequenceStatus.NOT_STARTED;

    Quaternion initial_rotation;

    void Awake()
    {
        instance = this;

        cam = GetComponent<Camera>();
        cam.enabled = false;
        initial_rotation = transform.rotation;
        audio_listener = GetComponent<AudioListener>();
        audio_listener.enabled = false;
    }

    public static ContinueSequenceStatus GetStatus() { return instance.status; }

    public static bool IsContinueSequenceOngoing() {
        return instance.status == ContinueSequenceStatus.CONTINUE_COUNTDOWN ||
            instance.status == ContinueSequenceStatus.GAME_OVER ||
            instance.status == ContinueSequenceStatus.RETRY;
    }

    IEnumerator DoContinueSequence()
    {
        if (status == ContinueSequenceStatus.CONTINUE_COUNTDOWN || status == ContinueSequenceStatus.GAME_OVER)
            yield break;
        status = ContinueSequenceStatus.CONTINUE_COUNTDOWN;
        audio_listener.enabled = true;

        int lives = 2;
        GameSettings game_settings = GameSettings.GetGameSettings();
        if (game_settings != null)
            lives = game_settings.GetStartingLives();
        UIManager.SetLivesAmount(lives);

        transform.rotation = initial_rotation;
        TransitionManager.RequestDarken(false);

        int countdown_number = 10;
        float countdown_timer = 2.0f;

        yield return new WaitForSeconds(1.0f);

        if (continues_remaining <= 0)
        {
            yield return StartCoroutine(Conclude(false));
            yield break;
        }

        ArborEventBus.Publish(new EventSoundEvent("vocal_continue_prompt", Vector3.zero));

        continue_countdown_canvas.alpha = 1.0f;
        countdown_text.text = "";
        continue_prompt_text.text = $"Press Enter to try again.\n(continues x {continues_remaining})";

        if(WebManager.IsMobileDeviceCurrently())
        {
            continue_prompt_text.text = $"Tap two fingers to try again.\n(continues x {continues_remaining})";
        }

        bool did_retry = false;

        while (countdown_number > -1 && did_retry == false)
        {
            int current_countdown_number = countdown_number;

            countdown_timer -= Time.deltaTime;

            if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                countdown_timer = 0;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                did_retry = true;
            }

            if (WebManager.IsMobileDeviceCurrently())
            {
                if (Input.touchCount > 1)
                    did_retry = true;
            }

            if(countdown_timer <= 0.0f)
            {
                countdown_timer = 2.0f;
                countdown_number--;
            }

            /* Show new number */
            if (countdown_number != current_countdown_number)
            {
                // Audio event.

                // Show new number.
                if (countdown_number >= 0)
                {
                    if(countdown_number > 0)
                        ArborEventBus.Publish(new EventSoundEvent($"vocal_continue_{countdown_number.ToString()}", Vector3.zero));

                    countdown_text.text = countdown_number.ToString();
                }
            }

            yield return null;
        }

        yield return StartCoroutine(Conclude(did_retry));
    }

    Vector3 bonus_height = Vector3.up * 1.25f;
    IEnumerator Conclude(bool did_retry)
    {
        continue_countdown_canvas.alpha = 0.0f;

        bonus_height = Vector3.up * 1.25f;

        if (spotlight == null)
        {
            bonus_height = Vector3.up * 0.0f;
            spotlight = PlayerController.GetPlayerGameobject().transform;
        }

        /* Look at character */
        Quaternion desired_rotation = new Quaternion();
        desired_rotation.SetLookRotation((spotlight.position + bonus_height - transform.position).normalized, Vector3.up);
        float progress = 0.0f;
        while (progress < 1.0f)
        {
            progress += Time.deltaTime * 2.0f;
            transform.rotation = Quaternion.Slerp(initial_rotation, desired_rotation, progress);
            yield return null;
        }

        /* Outcome */
        if (did_retry)
        {
            yield return StartCoroutine(Retry());
        }
        else
        {
            yield return StartCoroutine(GiveUp());
        }

        continue_countdown_canvas.alpha = 0.0f;

        transform.rotation = initial_rotation;

        status = ContinueSequenceStatus.NOT_STARTED;
        instance.cam.enabled = false;
        audio_listener.enabled = false;
    }


    IEnumerator GiveUp()
    {
        ArborEventBus.Publish(new EventMusicEvent("silence_bgm"));

        continues_remaining = 2;
        status = ContinueSequenceStatus.GAME_OVER;
        yield return new WaitForSecondsRealtime(3.0f);

        /* high angle */

        Vector3 initial_pos = transform.position;
        Vector3 final_pos = transform.position + Vector3.up * 3;
        float start_time = Time.time;
        float duration_sec = 1.0f;
        float progress = (Time.time - start_time) / duration_sec;

        while(progress < 1.0f)
        {
            progress = (Time.time - start_time) / duration_sec;
            transform.position = Vector3.Lerp(initial_pos, final_pos, progress);

            Quaternion desired_rotation = new Quaternion();
            desired_rotation.SetLookRotation((spotlight.position + bonus_height - transform.position).normalized, Vector3.up);
            transform.rotation = desired_rotation;
            yield return null;
        }
        transform.position = final_pos;

        yield return new WaitForSecondsRealtime(2.0f);

        yield return StartCoroutine(BigMessageManager.ShowTheEndMessage());

        TransitionManager.RequestDarken(true);

        yield return new WaitForSecondsRealtime(3.0f);
        status = ContinueSequenceStatus.NOT_STARTED;
        SceneManager.LoadScene("main_menu");
    }

    IEnumerator Retry()
    {
        continues_remaining -= 1;

        TransitionManager.RequestFlash();
        status = ContinueSequenceStatus.RETRY;
        ArborEventBus.Publish(new EventSoundEvent($"vocal_continue_retry", Vector3.zero));

        yield return new WaitForSecondsRealtime(3.0f);

        status = ContinueSequenceStatus.NOT_STARTED;
        TransitionManager.RequestFlash();
        ArborEventBus.Publish(new EventRetry());
        yield return null;
    }
}

public class EventRetry { }