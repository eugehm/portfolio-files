using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum QuadrantPressState { DOWN, UP, JUST_DOWN, JUST_UP };

public class MobileTouchControlsCanvas : MonoBehaviour
{
    static MobileTouchControlsCanvas instance;

    Canvas canvas;

    bool running = true;

    public GameObject touch_start_indicator_prefab;
    public GameObject touch_indicator_prefab;

    static Dictionary<int, GameObject> finger_to_touch_start_indicator = new Dictionary<int, GameObject>();
    static Dictionary<int, GameObject> finger_to_touch_indicator = new Dictionary<int, GameObject>();
    static Dictionary<int, Touch> finger_id_to_touch = new Dictionary<int, Touch>();

    static int finger_id_bottom_right_quadrant = -1;
    static int finger_id_bottom_left_quadrant = -1;
    static int finger_id_t_quadrant = -1;

    public static Vector2 GetNormDeltaBottomRightQuadrant()
    {
        if (finger_id_bottom_right_quadrant == -1)
            return Vector2.zero;

        Vector2 screen_pixel_delta = finger_to_touch_indicator[finger_id_bottom_right_quadrant].transform.position - finger_to_touch_start_indicator[finger_id_bottom_right_quadrant].transform.position;
        return new Vector2(screen_pixel_delta.x / (float)Screen.width, screen_pixel_delta.y / (float)Screen.height);
    }

    static QuadrantPressState bottom_quadrant_state = QuadrantPressState.UP;
    public static bool IsBottomRightQuadrantPressedThisFrame()
    {
        return bottom_quadrant_state == QuadrantPressState.JUST_DOWN;
    }

    public static bool IsBottomRightQuadrantReleasedThisFrame()
    {
        return bottom_quadrant_state == QuadrantPressState.JUST_UP;
    }

    public static Vector2 GetNormDeltaBottomLeftQuadrant()
    {
        if (finger_id_bottom_left_quadrant == -1)
            return Vector2.zero;

        Vector2 screen_pixel_delta = finger_to_touch_indicator[finger_id_bottom_left_quadrant].transform.position - finger_to_touch_start_indicator[finger_id_bottom_left_quadrant].transform.position;
        return new Vector2(screen_pixel_delta.x / (float)Screen.width, screen_pixel_delta.y / (float)Screen.height);
    }

    public static Vector2 GetNormDeltaTQuadrant() // camera -- uses top of screen as well as center-bottom in a "T" shape.
    {
        if (finger_id_t_quadrant == -1)
            return Vector2.zero;

        Vector2 screen_pixel_delta = finger_to_touch_indicator[finger_id_t_quadrant].transform.position - finger_to_touch_start_indicator[finger_id_t_quadrant].transform.position;
        return new Vector2(screen_pixel_delta.x / (float)Screen.width, screen_pixel_delta.y / (float)Screen.height);
    }

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

        if (WebManager.IsMobileDeviceCurrently() == false)
        {
            canvas.enabled = false;
            running = false;
            return;
        }
    }



    void Update()
    {
        if (!running)
            return;

        if (bottom_quadrant_state == QuadrantPressState.JUST_DOWN)
            bottom_quadrant_state = QuadrantPressState.DOWN;

        if (bottom_quadrant_state == QuadrantPressState.JUST_UP)
            bottom_quadrant_state = QuadrantPressState.UP;

        for(int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                GameObject new_touch_start_indicator = GameObject.Instantiate(touch_start_indicator_prefab);
                new_touch_start_indicator.transform.SetParent(transform, false);
                new_touch_start_indicator.GetComponent<RectTransform>().position = touch.position;
                finger_to_touch_start_indicator[touch.fingerId] = new_touch_start_indicator;

                GameObject new_touch_indicator = GameObject.Instantiate(touch_indicator_prefab);
                new_touch_indicator.transform.SetParent(transform, false);
                new_touch_indicator.GetComponent<RectTransform>().position = touch.position;
                finger_to_touch_indicator[touch.fingerId] = new_touch_indicator;

                finger_id_to_touch[touch.fingerId] = touch;

                /* Determine touch area */
                Vector2 norm_touch_position = new Vector2(touch.position.x / (float)Screen.width, touch.position.y / (float)Screen.height);

                if (norm_touch_position.y < 0.5f && norm_touch_position.x < 0.4f) // bottom left
                {
                    finger_id_bottom_left_quadrant = touch.fingerId;
                }
                else if (norm_touch_position.y < 0.5f && norm_touch_position.x > 0.6f) // bottom_right
                {
                    finger_id_bottom_right_quadrant = touch.fingerId;
                }
                else // T area
                {
                    finger_id_t_quadrant = touch.fingerId;
                }
            }

            else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
            {
                Destroy(finger_to_touch_start_indicator[touch.fingerId]);
                finger_to_touch_start_indicator.Remove(touch.fingerId);

                Destroy(finger_to_touch_indicator[touch.fingerId]);
                finger_to_touch_indicator.Remove(touch.fingerId);

                finger_id_to_touch.Remove(touch.fingerId);

                if (finger_id_bottom_left_quadrant == touch.fingerId)
                    finger_id_bottom_left_quadrant = -1;
                if (finger_id_bottom_right_quadrant == touch.fingerId)
                    finger_id_bottom_right_quadrant = -1;
                if (finger_id_t_quadrant == touch.fingerId)
                    finger_id_t_quadrant = -1;
            }

            else if (touch.phase == TouchPhase.Moved)
            {
                finger_to_touch_indicator[touch.fingerId].GetComponent<RectTransform>().position = touch.position;
            }
        }

        /* determine state of bottom-right quad */
        if (finger_id_bottom_right_quadrant != -1) // is currently down
        {
            if (bottom_quadrant_state == QuadrantPressState.JUST_UP || bottom_quadrant_state == QuadrantPressState.UP)
                bottom_quadrant_state = QuadrantPressState.JUST_DOWN;
        }
        else
        {
            if (bottom_quadrant_state == QuadrantPressState.JUST_DOWN || bottom_quadrant_state == QuadrantPressState.DOWN)
                bottom_quadrant_state = QuadrantPressState.JUST_UP;
        }
    }
}
