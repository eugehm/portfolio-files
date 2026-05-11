using UnityEngine;
using System.Runtime.InteropServices;

public class WebManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern bool IsMobileDevice();
    [DllImport("__Internal")]
    private static extern bool IsTabActive();

    public static bool IsMobileDeviceCurrently() {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            return IsMobileDevice();
        else
            return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
    }
    public static bool IsTabCurrentlyActive() {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            return IsTabActive();
        else
            return Application.isFocused;
    }

    public static Vector2 GetNormalizedMousePosition()
    {
        return new Vector2(Input.mousePosition.x / (float)Screen.width, Input.mousePosition.y / (float)Screen.height);
    }

    static WebManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}

