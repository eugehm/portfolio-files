using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArborCoroutineManager : MonoBehaviour
{
    static ArborCoroutineManager instance;

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
    }

    public static void ArborStartCoroutine(IEnumerator c)
    {
        instance.StartCoroutine(c);
    }

    public static IEnumerator ArborDoCoroutine(IEnumerator c)
    {
        yield return instance.StartCoroutine(c);
    }
}
