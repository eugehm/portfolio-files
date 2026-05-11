using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    public Vector3 rotation_amount = new Vector3(0, 50, 0);

    Vector3 initial_rotation;

    private void Awake()
    {
        initial_rotation = transform.eulerAngles;
    }

    void Update()
    {
        float secondsOfDay = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;

        transform.eulerAngles = initial_rotation + rotation_amount * secondsOfDay;
    }
}
