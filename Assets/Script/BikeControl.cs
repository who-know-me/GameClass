using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeControl : MonoBehaviour
{
    public Rigidbody rb;
    public float tiltTorque = 0.4f;
    public float maxAngularVelocity = 5f;
    
    public float targetSpeed = 1f;              // 前进速度

    private void Awake()
    {
            
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;
        transform.rotation =  Quaternion.Euler(-10,0,90);
    }
    
    void FixedUpdate()
    {
        // 当前在前进方向上的速度分量
        Vector3 forwardDir = transform.right;  // 自行车朝向（确认是哪个轴）
        float currentForwardSpeed = Vector3.Dot(rb.velocity, forwardDir);

        // 计算要补的速度差
        float deltaSpeed = targetSpeed - currentForwardSpeed;

        // 用力来补偿这个差值（只影响前进方向，不影响侧滑/重力等）
        Vector3 force = forwardDir * deltaSpeed / Time.fixedDeltaTime;

        rb.AddForce(force);
        // 3. 倾斜控制（使用左右方向键）
        float input = Input.GetAxis("Horizontal");
        rb.AddTorque(transform.up * -input * tiltTorque);

    }

    /*private void FixedUpdate()
    {
        // 假设：自行车是子物体，空 GameObject 放在底部中心，作为父物体
        float tiltInput = Input.GetAxis("Horizontal");
        float tiltSpeed = 30f;

        transform.Rotate(Vector3.right, tiltInput * tiltSpeed * Time.deltaTime);
    }*/


}
