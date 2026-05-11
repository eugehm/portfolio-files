using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameplayCamera : MonoBehaviour
{
    static GameplayCamera instance;

    public Vector3 normal_gameplay_offset;
    Camera cam;

    static List<SpecialCameraRequest> special_camera_requests = new List<SpecialCameraRequest>();
    public static void AddSpecialRequest(SpecialCameraRequest request)
    {
        special_camera_requests.Add(request);
        special_camera_requests = special_camera_requests.OrderBy(x => x.priority).ToList();
    }
    public static void RemoveSpecialRequest(string id)
    {
        for(int i = 0; i < special_camera_requests.Count; i++)
        {
            if (special_camera_requests[i].id == id)
            {
                special_camera_requests.RemoveAt(i);
                return;
            }
        }
    }

    public static Transform GetCameraTransform() { if (instance == null) return null; return instance.transform; }

    Subscription<EventGameplayCameraWarpRequest> sub_OnEventGameplayCameraWarpRequest;

    void Awake()
    {
        instance = this;

        desired_t = -Mathf.PI / 2.0f;
        t = desired_t;
        cam = transform.Find("gameplay_cam").GetComponent<Camera>();

        sub_OnEventGameplayCameraWarpRequest = ArborEventBus.Subscribe<EventGameplayCameraWarpRequest>(OnEventGameplayCameraWarpRequest);
    }

    float desired_t = 0.0f;
    float desired_radius = 12;
    float desired_altitude = 6;

    float t = 0.0f;
    float radius = 12;
    float altitude = 6.0f;

    float desired_y_pos = 0.0f;
    float player_y_pos = 0.0f;

    // Update is called once per frame
    void LateUpdate()
    {
        if (!WebManager.IsTabCurrentlyActive() || !Application.isFocused)
            return;

        if (special_camera_requests.Count > 0)
        {
            SpecialRequestMotion();
        }
        else
        {
            NormalGameplayMotion();
        }
    }


    // A mode of camera following that occurs when the player jumps too high or falls far below camera visibility.
    // This mode resets upon touching the ground.
    bool free_jumping_mode = false;

    void NormalGameplayMotion()
    {
        Vector3 player_pos = PlayerController.GetPlayerPosition();

        if (WebManager.IsMobileDeviceCurrently())
        {
            Vector2 touchscreen_dragging = MobileTouchControlsCanvas.GetNormDeltaTQuadrant();
            desired_t -= touchscreen_dragging.x * 0.075f * 3.0f;
            desired_altitude -= touchscreen_dragging.y * 0.3f * 3.0f;
        }
        else
        {
            desired_t -= Input.GetAxis("Mouse X") * 0.075f;
            desired_altitude -= Input.GetAxis("Mouse Y") * 0.3f;
            desired_radius += Input.GetAxis("Mouse ScrollWheel") * 6f;
        }

        desired_altitude = Mathf.Clamp(desired_altitude, 1.0f, 14.0f);
        desired_radius = Mathf.Clamp(desired_radius, 2.0f, 15.0f);

        // Smoothing
        t += (desired_t - t) * 0.3f * Time.deltaTime * 60.0f;
        radius += (desired_radius - radius) * Time.deltaTime * 60.0f;
        altitude = desired_altitude; // TRY this.

        Vector3 xz_pos = new Vector3(Mathf.Cos(t), 0, Mathf.Sin(t)) * radius;
        Vector3 player_xz_pos = new Vector3(player_pos.x, 0, player_pos.z);

        desired_y_pos = PlayerController.ground_point.y;
        if(IsTargetInFrustum() == false)
        {
            free_jumping_mode = true;
        }
        else if (PlayerController.on_ground)
        {
            free_jumping_mode = false;
        }

        if (free_jumping_mode)
        {
            desired_y_pos = player_pos.y;
        }

        player_y_pos += (desired_y_pos - player_y_pos) * 0.08f * Time.deltaTime * 60.0f;
        if (Mathf.Abs(desired_y_pos - player_y_pos) < 0.01f)
            player_y_pos = desired_y_pos;

        desired_pos = player_xz_pos + Vector3.up * player_y_pos + Vector3.up * 2.0f + xz_pos + Vector3.up * altitude;

        transform.position = desired_pos;
        transform.LookAt(player_xz_pos + Vector3.up * player_y_pos + Vector3.up * 2.0f, Vector3.up);
    }

    Vector3 desired_pos;
    Quaternion desired_rotation;

    // NOTE: modified from original function for snowball zone gameplay
    void SpecialRequestMotion()
    {
        SpecialCameraRequest request = special_camera_requests[0];

        if (request.object_to_track != null)
        {
            desired_pos = request.object_to_track.position + request.position;
            if (request.direction != Vector3.zero)
                desired_rotation = Quaternion.LookRotation(request.direction.normalized, Vector3.up);
            else
                desired_rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }
        else
        {
            desired_pos = request.position;
            desired_rotation = Quaternion.LookRotation(request.direction.normalized, Vector3.up);
        }

        transform.position = Vector3.Lerp(transform.position, desired_pos, request.ease_factor);
        transform.rotation = Quaternion.Slerp(transform.rotation, desired_rotation, request.ease_factor);
    }

    void OnEventGameplayCameraWarpRequest(EventGameplayCameraWarpRequest e)
    {
        transform.position = desired_pos;
        transform.rotation = desired_rotation;
    }

    bool IsTargetInFrustum()
    {
        Vector3 vp = cam.WorldToViewportPoint(PlayerController.GetPlayerPosition());
        const float margin = -0.1f;
        // vp is in [0,1]x[0,1] when on screen. z > 0 means in front of the camera.
        return vp.z > 0f &&
               vp.x > -margin && vp.x < 1f + margin &&
               vp.y > -margin && vp.y < 1f + margin;
    }

    private void OnDestroy()
    {
        if (sub_OnEventGameplayCameraWarpRequest != null)
            ArborEventBus.Unsubscribe(sub_OnEventGameplayCameraWarpRequest);
    }

    // NOTE: added function for snowball zone gameplay
    public static void TiltCamera(string requestId, Vector3 direction, Vector3 offset, float ease = 0.2f)
    {
        Transform playerTransform = PlayerController.instance.transform;
        if (playerTransform == null) return;

        SpecialCameraRequest request = new SpecialCameraRequest()
        {
            id = requestId,
            priority = 100,
            position = offset,
            object_to_track = playerTransform,
            direction = direction,
            ease_factor = ease
        };

        AddSpecialRequest(request);
    }

    // NOTE: added function for snowball zone gameplay
    public static void RemoveTiltCamera(string requestId)
    {
        RemoveSpecialRequest(requestId);
    }
}

public class SpecialCameraRequest
{
    public string id;
    public int priority = 0; // higher comes first.
    public Vector3 position;
    public Vector3 direction;
    public Transform object_to_track;
    public float ease_factor = 1.0f;
}

public class EventGameplayCameraWarpRequest { }