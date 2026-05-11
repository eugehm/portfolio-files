using UnityEngine;
using System;
using System.Collections.Generic;

public class MoveOnPath : MonoBehaviour
{
    public List<Transform> path_points;
    public float cycle_duration_sec = 5.0f;
    public float cycle_offset_sec = 0.0f;
    public bool initial_position_added_to_path = true;
    public bool reconnect_to_beginning = true;

    private void Awake()
    {
        if (initial_position_added_to_path)
        {
            GameObject start_path_point = new GameObject();
            start_path_point.name = "path_marker";
            start_path_point.transform.position = transform.position;
            path_points.Insert(0, start_path_point.transform);
        }

        if (reconnect_to_beginning && path_points.Count > 0)
        {
            path_points.Add(path_points[0]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float progress = (float) (((NetworkManager.GetServerTime() + cycle_offset_sec) % cycle_duration_sec) / cycle_duration_sec);

        transform.position = GetPositionOnPath(progress);
    }

    float GetTotalPathLength()
    {
        if (path_points.Count <= 1)
            return 0.0f;

        float total_distance_result = 0.0f;

        for(int i = 0; i < path_points.Count-1; i++)
        {
            float distance_of_leg = Vector3.Distance(path_points[i].position, path_points[i + 1].position);
            total_distance_result += distance_of_leg;
        }

        return total_distance_result;
    }

    Vector3 GetPositionOnPath(float progress)
    {
        if (path_points.Count < 0)
            return transform.position;

        if (path_points.Count <= 1)
            return path_points[0].position;

        float distance_to_consume = GetTotalPathLength() * progress;

        for (int i = 0; i < path_points.Count - 1; i++)
        {
            float distance_of_leg = Vector3.Distance(path_points[i].position, path_points[i + 1].position);
            if (distance_of_leg < distance_to_consume)
                distance_to_consume -= distance_of_leg;
            else
            {
                // We've reached the desired leg.
                Vector3 desired_point = path_points[i].position + (path_points[i + 1].position - path_points[i].position).normalized * distance_to_consume;
                return desired_point;
            }
        }

        Debug.LogError("Failed to calculate position on path for some reason...");
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < path_points.Count - 1; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(path_points[i].position, 0.25f);
            Gizmos.DrawLine(path_points[i].position, path_points[i+1].position);
        }
    }
}
