using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject game_over_cam;
    public GameObject gameplay_cam;
    public GameObject char_select_cam;

    void LateUpdate()
    {
        bool on_mobile_device = WebManager.IsMobileDeviceCurrently();

        if (CharacterSelectManager.IsCharSelectActive())
        {
            if (!on_mobile_device)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
 
            SwitchToCam(char_select_cam);
        }
        else if (GameOverCam.IsContinueSequenceOngoing())
        {
            if (!on_mobile_device)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            SwitchToCam(game_over_cam);
        }
        else
        {
            if (!on_mobile_device)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            SwitchToCam(gameplay_cam);
        }
    }

    void SwitchToCam(GameObject cam)
    {
        if (Camera.main != null && Camera.main.gameObject == cam)
            return;

        if (game_over_cam.activeSelf)
            game_over_cam.SetActive(false);

        if (gameplay_cam.activeSelf)
            gameplay_cam.SetActive(false);

        if (char_select_cam.activeSelf)
            char_select_cam.SetActive(false);

        cam.SetActive(true);
        cam.tag = "MainCamera";
    }
}
