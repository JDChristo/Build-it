using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    private Vector3 startOffset;

    private void Start()
    {
        SetTarget();
    }

    private void Update()
    {
        if (target == null)
        {
            SetTarget();
        }
        else
        {
            transform.position = target.position + startOffset + Vector3.up;
            transform.LookAt(target);
        }
    }

    private void SetTarget()
    {
        if (target != null)
        {
            startOffset = transform.position - target.position;
        }
    }
}