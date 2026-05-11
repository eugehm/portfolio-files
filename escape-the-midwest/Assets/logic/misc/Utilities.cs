using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Utilities
{
    public static void LogErrorForStudents(string component_name, GameObject gameobject, string message)
    {
        string message_to_student = $"⛔ {component_name} Component on Gameobject {gameobject} : {message}";
        Debug.LogError(message_to_student);
        ToastManager.RequestToast(message_to_student, ToastType.EMERGENCY);
    }

    public static List<LevelConfiguration> GetAvailableLevels()
    {
        LevelConfiguration[] levels = Resources.LoadAll<LevelConfiguration>("");

        return new List<LevelConfiguration>(levels);
    }

    public static IEnumerator WaitForMouseClickSpacebarOrEnter()
    {
        yield return null;

        while(true)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                yield break;

            yield return null;
        }

        yield return null;
    }

    public static Vector2 WorldToCanvasAnchorPosition(GameObject worldObject, Canvas canvas, Camera camera = null)
    {
        if (canvas == null)
        {
            Debug.LogError("Canvas is null!");
            return Vector2.zero;
        }

        // Convert world position to screen point
        Vector3 screenPoint;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && camera != null)
        {
            screenPoint = camera.WorldToScreenPoint(worldObject.transform.position);
        }
        else
        {
            screenPoint = Camera.main.WorldToScreenPoint(worldObject.transform.position);
        }

        // Convert screen point to local UI point
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera,
            out localPoint
        );

        return localPoint;
    }

    public static void GetComponentsInDescendents<T>(Transform t, ref HashSet<T> result) where T : Component
    {
        if (result == null)
        {
            Debug.LogError("You forgot to provide a non-null result array to Utilities.GetComponentsInDescendents()");
            return;
        }

        result.UnionWith(t.GetComponents<T>());

        /* Go through all children */
        foreach(Transform child in t)
        {
            GetComponentsInDescendents<T>(child, ref result);
        }
    }

    public static void GetComponentsInAncestors<T>(Transform t, ref HashSet<T> result) where T : Component
    {
        if (result == null)
        {
            Debug.LogError("You forgot to provide a non-null result array to Utilities.GetComponentsInAncestors()");
            return;
        }

        result.UnionWith(t.GetComponents<T>());

        /* Go up through ancestors */
        if(t.parent != null)
            GetComponentsInAncestors<T>(t.parent, ref result);
    }
}

