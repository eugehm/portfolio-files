using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArborCore : MonoBehaviour
{
    static MonoBehaviour instance;

    public static MonoBehaviour Get()
    {
        if (instance != null)
            return instance;

        GameObject core = new GameObject();
        core.name = "ArborCore";
        DontDestroyOnLoad(core);
        instance = core.AddComponent<ArborCore>();
        return instance;
    }
}
