using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class MainMenuCanvas : MonoBehaviour
{
    [SerializeField] RectTransform logo_rt;
    [SerializeField] CanvasGroup logo_cg;
    [SerializeField] CanvasGroup course_logo_cg;
    [SerializeField] CanvasGroup black_bars_cg;
    [SerializeField] CanvasGroup white_cover_cg;
    [SerializeField] CanvasGroup press_start_cg;
    [SerializeField] CanvasGroup click_here_cg;

    static bool intro_completed = false;
    public static bool IsIntroCompleted() { return intro_completed; }

    void Start()
    {
        GetComponent<CanvasGroup>().alpha = 1.0f;
        intro_completed = false;
        StartCoroutine(DoIntro());
    }

    /* Hack : in web browser-based games, we need the player to click the screen before any audio may be heard. */
    /* We use this variable to pause until the player clicks */
    static bool did_click_screen = false;

    IEnumerator DoIntro()
    {
        press_start_cg.alpha = 0.0f;
        course_logo_cg.alpha = 0.0f;
        logo_rt.anchoredPosition = Vector2.zero;
        logo_rt.localScale = Vector2.one;
        click_here_cg.alpha = 0.0f;

        TransitionManager.RequestDarken(false);

        /* If game is not yet focused, make player click the screen */
        if(did_click_screen == false)
        {
            while (Input.GetMouseButtonDown(0) == false || WebManager.IsTabCurrentlyActive() == false)
            {
                click_here_cg.alpha = (Mathf.Sin(Time.time * 4.0f) + 1.0f) / 2.0f;
                yield return null;
            }

            click_here_cg.alpha = 0.0f;
            did_click_screen = true;

            yield return null;
        }

        yield return new WaitForSeconds(0.25f);

        ArborEventBus.Publish(new EventSoundEvent("vocal_eecs298", Vector3.zero));

        /* Show Course logo */
        ArborEventBus.Publish(new EventMusicEvent("main_menu_bgm"));
        while (course_logo_cg.alpha < 1.0f)
        {
            course_logo_cg.alpha += Time.deltaTime;
            yield return null;
        }
        course_logo_cg.alpha = 1.0f;

        yield return new WaitForSeconds(1.0f);

        while (course_logo_cg.alpha > 0.0f)
        {
            course_logo_cg.alpha -= Time.deltaTime;
            yield return null;
        }
        course_logo_cg.alpha = 0.0f;

        /* Show logo */
        ArborEventBus.Publish(new EventSoundEvent("vocal_title", Vector3.zero));
        while (logo_cg.alpha < 1.0f)
        {
            logo_cg.alpha += Time.deltaTime;
            yield return null;
        }
        logo_cg.alpha = 1.0f;


        yield return new WaitForSeconds(0.5f);

        /* Move Logo to corner */
        SetAnchorAndPivotWithoutMoving(logo_rt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

        Vector2 initial_position = logo_rt.anchoredPosition;
        Vector2 final_position = new Vector2(40, -40);
        Vector2 final_scale = new Vector3(0.5f, 0.5f);

        float progress = 0.0f;
        while(progress < 1.0f)
        {
            progress += Time.deltaTime;
            
            // Move logo
            logo_rt.anchoredPosition = Vector2.Lerp(initial_position, final_position, progress);
            logo_rt.localScale = Vector2.Lerp(Vector2.one, final_scale, progress);

            // Reveal scene
            white_cover_cg.alpha = 1.0f - progress;
            yield return null;
        }
        logo_rt.anchoredPosition = final_position;
        logo_rt.localScale = final_scale;
        white_cover_cg.alpha = 0.0f;

        intro_completed = true;

        /* Wait until the title screen passes, then show bars */
        while (MainMenuCamera.InTitleLogoMode())
        {
            press_start_cg.alpha = (Mathf.Sin(Time.time * 3.0f) + 1.0f) / 2.0f;
            yield return null;
        }

        press_start_cg.alpha = 0.0f;

        while (black_bars_cg.alpha < 1.0f)
        {
            black_bars_cg.alpha += Time.deltaTime;
            yield return null;
        }

        menu_mode = true;
    }

    bool menu_mode = false;

    private void Update()
    {
        if (menu_mode == false)
            return;

        if (InformationViewer.IsVisible())
        {
            GetComponent<CanvasGroup>().alpha = 0.0f;
        }
        else
            GetComponent<CanvasGroup>().alpha = 1.0f;
    }

    void SetAnchorAndPivotWithoutMoving(
        RectTransform rectTransform,
        Vector2 newAnchorMin,
        Vector2 newAnchorMax,
        Vector2 newPivot)
    {
        // Cache sizes and transforms
        RectTransform parent = rectTransform.parent as RectTransform;

        if (parent == null)
        {
            Debug.LogWarning("RectTransform parent is null. This method assumes the RectTransform is parented to another RectTransform.");
            return;
        }

        // Get the world position of the corners BEFORE the change
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        Vector3 worldPositionBefore = (worldCorners[0] + worldCorners[2]) * 0.5f; // Center

        // Apply anchor and pivot changes
        rectTransform.anchorMin = newAnchorMin;
        rectTransform.anchorMax = newAnchorMax;
        rectTransform.pivot = newPivot;

        // Get world position of the corners AFTER the change
        rectTransform.GetWorldCorners(worldCorners);
        Vector3 worldPositionAfter = (worldCorners[0] + worldCorners[2]) * 0.5f;

        // Calculate the difference and move anchoredPosition to compensate
        Vector3 worldDelta = worldPositionBefore - worldPositionAfter;
        // Transform worldDelta to local space of the parent
        Vector2 localDelta = (Vector2)parent.InverseTransformVector(worldDelta);
        rectTransform.anchoredPosition += localDelta;
    }
}
