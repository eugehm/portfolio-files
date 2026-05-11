using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TransitionManager : MonoBehaviour
{
    static TransitionManager instance;

    static bool should_darken = true;
    CanvasGroup cg_blackpanel;
    public static void RequestDarken(bool b) { should_darken = b; }

    CanvasGroup cg_whitepanel;
    public static void RequestFlash() { instance.cg_whitepanel.alpha = 1.0f; }

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

        GameObject black_panel = transform.Find("blackpanel").gameObject;
        black_panel.SetActive(true);
        cg_blackpanel = black_panel.GetComponent<CanvasGroup>();
        
        cg_blackpanel.alpha = 0.0f;

        GameObject white_panel = transform.Find("whitepanel").gameObject;
        white_panel.SetActive(true);
        cg_whitepanel = white_panel.GetComponent<CanvasGroup>();
        cg_whitepanel.alpha = 1.0f;
    }

    void Update()
    {
        if (should_darken)
        {
            if (cg_blackpanel.alpha < 1.0f)
                cg_blackpanel.alpha += Time.deltaTime;
            else
                cg_blackpanel.alpha = 1.0f;
        }
        else
        {
            if (cg_blackpanel.alpha > 0.0f)
                cg_blackpanel.alpha -= Time.deltaTime;
            else
                cg_blackpanel.alpha = 0.0f;
        }

        if (cg_blackpanel.alpha > 0.0f)
        {
            cg_blackpanel.blocksRaycasts = true;
            cg_blackpanel.interactable = true;
        }
        else
        {
            cg_blackpanel.blocksRaycasts = false;
            cg_blackpanel.interactable = false;
        }

        /* White panel flash */
        cg_whitepanel.alpha += (0.0f - cg_whitepanel.alpha) * 0.02f;
    }
}
