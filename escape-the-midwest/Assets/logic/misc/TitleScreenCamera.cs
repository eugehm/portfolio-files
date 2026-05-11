using UnityEngine;

public class TitleScreenCamera : MonoBehaviour
{
    [SerializeField] float height = 2.0f;
    [SerializeField] float xz_distance = 4.0f;
    [SerializeField] float speed_factor = 0.2f;
    [SerializeField] float side_skew_factor = 2.0f;
    [SerializeField] float vertical_skew_factor = 0.0f;
    [SerializeField] float vertical_wave_factor = 0.2f;

    float t = 0.0f;

    void Update()
    {
        t += Time.deltaTime;

        Vector3 xz_position = new Vector3(Mathf.Cos(t * speed_factor) * xz_distance, 0, Mathf.Sin(t * speed_factor) * xz_distance);
        Vector3 y_position = new Vector3(0, height + Mathf.Sin(t * speed_factor) * vertical_wave_factor, 0);
        Vector3 desired_position = PlayerController.GetPlayerPosition() + xz_position + y_position;

        transform.position = Vector3.Lerp(transform.position, desired_position, 0.1f);

        Vector3 character_to_camera_xz = transform.position - PlayerController.GetPlayerPosition();
        character_to_camera_xz = (new Vector3(character_to_camera_xz.x, 0, character_to_camera_xz.z)).normalized;
        Vector3 side_skew_direction = Vector3.Cross(character_to_camera_xz, Vector3.up);

        transform.LookAt(PlayerController.GetPlayerPosition() + side_skew_direction * side_skew_factor + Vector3.up * vertical_skew_factor, Vector3.up);
    }
}
