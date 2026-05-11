using UnityEngine;

public class FollowTargetLinear : MonoBehaviour
{
    public GameObject gameobject_to_follow;
    public float following_speed = 6.0f;
    public float back_off_distance = 2.0f;
    public bool look_in_xz_movement_direction = true;

    bool following = true;

    void Update()
    {
        if (gameobject_to_follow == null)
            return;

        float distance = Vector3.Distance(transform.position, gameobject_to_follow.transform.position);

        if(following)
        {
            if (distance < back_off_distance)
                following = false;
        }
        else
        {
            if (distance > back_off_distance + 1.0f)
                following = true;
        }

        Vector3 direction = (gameobject_to_follow.transform.position - transform.position).normalized;
        
        if(following)
            transform.position += direction * Time.deltaTime * following_speed;


        if (look_in_xz_movement_direction)
        {
            Quaternion desired_rotation = new Quaternion();
            Vector3 towards_target = (gameobject_to_follow.transform.position - transform.position).normalized;
            towards_target = new Vector3(towards_target.x, 0.0f, towards_target.z);
            desired_rotation.SetLookRotation(towards_target, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired_rotation, 0.1f);
        }
    }
}
