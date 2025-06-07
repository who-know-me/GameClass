using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BikeController : MonoBehaviour
{
    [Header("前进相关")]
    public float forwardForce = 100f;         // 基础前进力
    public float steerSensitivity = 30f;      // 倾斜转向敏感度（度/角度）

    [Header("平衡相关")]
    public float balanceTorque = 50f;         // 玩家输入纠正力矩
    public float autoLeanTorque = 30f;        // 自动倾倒（重心不正时）

    [Header("限制")]
    public float maxTiltAngle = 45f;          // 最大倾斜角（防止无限翻车）

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 10f; // 限制最大角速度，防止旋转过快
    }

    void FixedUpdate()
    {
        ApplyForwardMovement();
        ApplyBalanceControl();
    }

    void ApplyForwardMovement()
    {
        // 计算倾斜角（车体右方向 与 世界重力方向夹角）
        float tilt = Vector3.Dot(transform.forward, Vector3.down); // 向左倾为正

        // 计算当前倾斜影响的前进方向（绕Y轴旋转）
        Vector3 forwardDir = Quaternion.AngleAxis(tilt * steerSensitivity, Vector3.down) * transform.right;

        // 施加前进力（始终向前）
        rb.AddForce(forwardDir.normalized * forwardForce);
    }

    void ApplyBalanceControl()
    {
        // 倾斜过度直接不允许矫正
        float angle = Mathf.Abs(Vector3.Angle(transform.forward, Vector3.up));
        if (angle < maxTiltAngle || angle > 180-maxTiltAngle)
            return;
        
        // 玩家输入（A/D键）左右控制
        float input = Input.GetAxis("Horizontal");

        // 施加玩家纠正力矩（绕Z轴旋转来平衡左右）
        rb.AddTorque(transform.right * -input * balanceTorque);

        // 获取当前倾斜程度（车体右方向 与 向下方向夹角）
        float tilt = Vector3.Dot(transform.forward, Vector3.down);

        // 模拟自然倾倒（让车更偏向当前不平衡方向）
        rb.AddTorque(transform.right * tilt * autoLeanTorque);
    }
}