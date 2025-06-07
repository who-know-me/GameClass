using UnityEngine;

public class BikeJumpController : MonoBehaviour
{
    enum BikeState { Normal, SingleWheelPrep, SingleWheel, Jumping }
    BikeState state = BikeState.Normal;

    [Header("平衡状态设置")]
    public float balanceThreshold = 0.1f; // 倾斜角小于此值才可进入状态2

    [Header("蓄力跳跃设置")]
    public float maxChargeTime = 1.5f;
    public float jumpForce = 10f;
    public AnimationCurve chargeCurve; // 蓄力曲线（0~1）

    [Header("组件")]
    public Rigidbody rb;
    public Transform model; // 车体视觉对象

    private float chargeTimer = 0f;
    private bool isCharging = false;

    void Start()
    {
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case BikeState.Normal:
                CheckEnterSingleWheelMode();
                break;

            case BikeState.SingleWheelPrep:
                HandleSingleWheelPrep();
                break;

            case BikeState.SingleWheel:
                HandleBalanceControl();
                HandleCharging();
                break;
        }
    }

    void CheckEnterSingleWheelMode()
    {
        float tilt = Vector3.Dot(transform.forward, Vector3.down); // 如果是X轴前进，forward表示左右方向
        if (Mathf.Abs(tilt) < balanceThreshold && Input.GetKeyDown(KeyCode.Space))
        {
            // 锁定当前姿态
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.isKinematic = true; // 暂停物理
            state = BikeState.SingleWheelPrep;
        }
    }

    void HandleSingleWheelPrep()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            // 进入单轮状态
            rb.isKinematic = false;
            state = BikeState.SingleWheel;
        }
    }

    void HandleBalanceControl()
    {
        float input = Input.GetAxis("Vertical"); // W/S 控制
        rb.AddTorque(transform.up * input * 10f);
    }

    void HandleCharging()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (!isCharging)
            {
                isCharging = true;
                chargeTimer = 0f;
            }

            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, maxChargeTime);
        }

        if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            isCharging = false;
            float chargePercent = chargeTimer / maxChargeTime;
            float actualForce = jumpForce * chargeCurve.Evaluate(chargePercent);

            // 按当前前后角跳跃
            Vector3 jumpDir = (transform.up + transform.right * 0.5f).normalized;
            rb.AddForce(jumpDir * actualForce, ForceMode.Impulse);
            state = BikeState.Jumping;
        }
    }
}

