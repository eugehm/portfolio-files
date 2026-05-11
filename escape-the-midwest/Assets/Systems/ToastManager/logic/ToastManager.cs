using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum ToastType { NORMAL, EMERGENCY }

public class ToastManager : MonoBehaviour
{
    [SerializeField] RectTransform toast_panel_rt;
    [SerializeField] GameObject toast_text_prefab;
    [SerializeField] CanvasGroup toast_panel_cg;

    static ToastManager instance;

    static List<GameObject> toast_text_queue = new List<GameObject>();
    public static int GetIndexInQueue(GameObject g) { return toast_text_queue.IndexOf(g); }
    public static void RemoveFromQueueAndDestroy(GameObject g) { toast_text_queue.Remove(g); GameObject.Destroy(g); }


    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        GetComponent<CanvasGroup>().alpha = 1.0f;
        toast_panel_cg.alpha = 0.0f;
    }

    public static void RequestToast(string message, ToastType toast_type, DelegateBoolReturnGameObjectParameter visibility_check = null)
    {
        if (message == null)
            message = "";
        message = message.Trim();

        /* Check to make sure we aren't spamming the same exact message */
        if (toast_text_queue.Count > 0)
        {
            TextMeshProUGUI tmp = toast_text_queue[0].GetComponent<TextMeshProUGUI>();
            if(tmp.text.Equals(message))
            {
                /* Make the previous message "pop" so the user sees it */
                ToastText tt = toast_text_queue[0].GetComponent<ToastText>();
                tt.BumpAndResetLifetime();
                return;
            }
        }

        /* Create a new message */
        GameObject new_toast_text = GameObject.Instantiate(instance.toast_text_prefab);
        new_toast_text.GetComponent<ToastText>().visibility_check = visibility_check;
        RectTransform rt = new_toast_text.GetComponent<RectTransform>();
        rt.SetParent(instance.toast_panel_rt, true);
        rt.anchoredPosition = new Vector2(-50, 0);
        toast_text_queue.Insert(0, new_toast_text);
        TextMeshProUGUI text = new_toast_text.GetComponent<TextMeshProUGUI>();
        text.SetText(message);
        if (toast_type == ToastType.EMERGENCY)
            text.faceColor = Color.red;
    }

    private void Update()
    {
        HandleAlpha();
    }

    void HandleAlpha()
    {
        /* Become visible when we have toasts. */
        if (toast_text_queue.Count > 0)
            toast_panel_cg.alpha += Time.deltaTime * 2.0f;

        /* Become invisible when we do not. */
        else
            toast_panel_cg.alpha -= Time.deltaTime * 2.0f;

        toast_panel_cg.alpha = Mathf.Clamp01(toast_panel_cg.alpha);
    }
}
