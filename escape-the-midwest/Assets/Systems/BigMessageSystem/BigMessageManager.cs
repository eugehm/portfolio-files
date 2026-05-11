using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum BigMessageManagerMode { GAME_OVER, THE_END, VICTORY };

public class BigMessageManager : MonoBehaviour
{
    [SerializeField] CanvasGroup black_panel_cg;

    static BigMessageManager instance;

    [SerializeField] List<BigMessageLetter> game_over_letters;
    [SerializeField] List<BigMessageLetter> the_end_letters;
    [SerializeField] List<BigMessageLetter> victory_letters;

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

        black_panel_cg.alpha = 0.0f;
    }

    public static IEnumerator ShowGameOverMessage()
    {
        yield return instance.StartCoroutine(DoBigLetterMessage(instance.game_over_letters, BigMessageManagerMode.GAME_OVER));
    }

    public static IEnumerator ShowTheEndMessage()
    {
        yield return instance.StartCoroutine(DoBigLetterMessage(instance.the_end_letters, BigMessageManagerMode.THE_END));
    }

    public static IEnumerator ShowVictoryMessage()
    {
        yield return instance.StartCoroutine(DoBigLetterMessage(instance.victory_letters, BigMessageManagerMode.VICTORY));
    }


    static bool doing_message = false;
    public static IEnumerator DoBigLetterMessage(List<BigMessageLetter> letters_to_show, BigMessageManagerMode mode)
    {
        if (doing_message)
            yield break;
        doing_message = true;

        yield return ArborCoroutineManager.ArborDoCoroutine(BlackPanelToAlpha(0.7f));
        
        foreach(BigMessageLetter letter in letters_to_show)
        {
            instance.StartCoroutine(letter.DoDelayedStart());
        }

        yield return new WaitForSeconds(1.0f);


        if (mode == BigMessageManagerMode.GAME_OVER)
        {
            ArborEventBus.Publish(new EventMusicEvent("game_over_bgm"));
            ArborEventBus.Publish(new EventSoundEvent("vocal_gameover", Vector3.zero));
        }
        else if (mode == BigMessageManagerMode.THE_END)
            ArborEventBus.Publish(new EventSoundEvent("vocal_theend", Vector3.zero));
        else if (mode == BigMessageManagerMode.VICTORY)
            ArborEventBus.Publish(new EventSoundEvent("vocal_victory", Vector3.zero));

        yield return new WaitForSeconds(2.0f);

        ArborCoroutineManager.ArborStartCoroutine(BlackPanelToAlpha(1.0f));
        TransitionManager.RequestDarken(true);

        foreach(BigMessageLetter letter in letters_to_show)
        {
            instance.StartCoroutine(letter.DoDelayedEnd());
        }

        yield return new WaitForSeconds(1.0f);
        ArborCoroutineManager.ArborStartCoroutine(BlackPanelToAlpha(0.0f));

        if(mode == BigMessageManagerMode.THE_END || mode == BigMessageManagerMode.VICTORY)
        {
            ArborEventBus.Publish(new EventTheEnd());
        }

        doing_message = false;
    }

    static IEnumerator BlackPanelToAlpha(float desired_alpha)
    {
        if (desired_alpha > instance.black_panel_cg.alpha)
        {
            while (instance.black_panel_cg.alpha < desired_alpha)
            {
                instance.black_panel_cg.alpha += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        else if (desired_alpha < instance.black_panel_cg.alpha)
        {
            while (instance.black_panel_cg.alpha > desired_alpha)
            {
                instance.black_panel_cg.alpha -= Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
