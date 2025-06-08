using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public bool isFree;
    
    public float distance = 5.0f;     // 摄像机距离目标的初始距离
    public float zoomSpeed = 2.0f;    // 缩放速度
    public float rotationSpeed = 5.0f;// 旋转速度
    public float minDistance = 2f;    // 最小缩放距离
    public float maxDistance = 15f;   // 最大缩放距离

    private float currentX = 0f;
    private float currentY = 0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private void Start()
    {
        isFree = false;
    }

    private void LateUpdate()
    {
        if (!isFree)
            transform.position = target.position + offset;
        else
        {
            // 鼠标右键控制旋转
            if (Input.GetMouseButton(1))
            {
                currentX += Input.GetAxis("Mouse X") * rotationSpeed;
                currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
                currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);
            }

            // 鼠标滚轮控制缩放
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            // 计算旋转后的摄像机位置
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 dir = new Vector3(0, 0, -distance);
            transform.position = target.position + rotation * dir;
        }
        
        transform.LookAt(target); // 始终看向目标
    }

    public void IsFree()
    {
        isFree = true;
    }
    public void NotFree()
    {
        isFree = false;
    }
    
}
