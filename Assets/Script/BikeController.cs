using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BikeController : MonoBehaviour
{
    public enum BikeState { Normal, SingleWheel, Chagring, Jumping }
    public BikeState state = BikeState.Normal;
    public GameObject camera;
    public GameObject panel;

    [Header("平衡状态设置")]
    public float balanceThreshold; // 倾斜角小于此值才可进入状态2
    public Transform backWheel;
    public Transform frontWheel;


    [Header("蓄力跳跃设置")]
    public float maxChargeTime;
    public float jumpForce;
    public AnimationCurve chargeCurve; // 蓄力曲线（0~1）
    private float chargeTimer = 0f;
    private bool isCharging = false;

    [Header("组件")]
    public Rigidbody rb;

    
    [Header("前进相关")]
    public float forwardForce;         // 基础前进力
    public float steerSensitivity;      // 倾斜转向敏感度（度/角度）

    [Header("平衡相关")]
    public float balanceTorque;         // 玩家输入纠正力矩

    [Header("限制")]
    public float maxTiltAngle;          // 最大倾斜角（防止无限翻车）

    private CameraController _cameraController;

    // 输入标志变量
    private bool spacePressed = false;
    private bool spaceHeld = false;
    private bool spaceReleased = false;
    private bool cPressed = false;
    private bool vPressed = false;
    
    
    void Start()
    {        
        camera = GameObject.FindGameObjectWithTag("MainCamera");
        _cameraController = camera.GetComponent<CameraController>();
        rb = GetComponent<Rigidbody>();
        
        rb.maxAngularVelocity = 10f; // 限制最大角速度，防止旋转过快
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        backWheel = transform.Find("BackWheel");
        frontWheel = transform.Find("FrontWheel");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            spacePressed = true;
        if(Input.GetKey(KeyCode.Space))
            spaceHeld = true;
        if(Input.GetKeyUp(KeyCode.Space))
            spaceReleased = true;
        if(Input.GetKeyDown(KeyCode.C))
            cPressed = true;
        if(Input.GetKeyDown(KeyCode.V))
            vPressed = true;
        /*spacePressed = Input.GetKeyDown(KeyCode.Space);
        spaceHeld = Input.GetKey(KeyCode.Space);
        spaceReleased = Input.GetKeyUp(KeyCode.Space);
        cPressed = Input.GetKeyDown(KeyCode.C);*/
    }

    
    
    void FixedUpdate()
    {
        switch (state)
        {
            case BikeState.Normal:
                CheckEnterSingleWheelMode();
                ApplyForwardMovement();
                ApplyBalanceControl();
                break;

            case BikeState.SingleWheel:
                HandleBalanceControl();
                break;
            
            case BikeState.Chagring:
                HandleCharging();
                break;
            
            case BikeState.Jumping:
                JumpingControl();
                break;
        }
        CheckFail();
        
        spacePressed = false;
        spaceHeld = false;
        spaceReleased = false;
        cPressed = false;
        vPressed = false;
    }

    private void CheckFail()
    {
        if(transform.position.y < -11.4514)
            Fail();
        float angle = Mathf.Abs(Vector3.Angle(transform.forward, Vector3.up));
        if (angle < 10 || angle > 180 - 10)
            Fail();
    }

    private void Fail()
    {
        panel.SetActive(true);
    }

    //检查是否进入单轮状态
    void CheckEnterSingleWheelMode()
    {
        float tilt = Vector3.Dot(transform.forward, Vector3.down); // 如果是X轴前进，forward表示左右方向
        if (Mathf.Abs(tilt) < balanceThreshold && spacePressed)
        {
            // 锁定当前姿态
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.useGravity = false; // 暂停重力
            
            // 进入单轮状态
            state = BikeState.SingleWheel;
            _cameraController.isFree = true;
        }
    }

    //状态一：控制前进
    void ApplyForwardMovement()
    {
        // 计算倾斜角（车体右方向 与 世界重力方向夹角）
        float tilt = Vector3.Dot(transform.forward, Vector3.down); // 向左倾为正

        // 计算当前倾斜影响的前进方向（绕Y轴旋转）
        Vector3 forwardDir = Quaternion.AngleAxis(tilt * steerSensitivity, Vector3.down) * Vector3.right;

        // 施加前进力（始终向前）
        //if (Input.GetKey(KeyCode.W))
            rb.AddForce(forwardDir.normalized * forwardForce);
    }

    //状态一：控制人为倾斜
    void ApplyBalanceControl()
    {
        //检测落地
        var coll1 = Physics.OverlapSphere(frontWheel.position, 0.2f, LayerMask.GetMask("Ground"));
        var coll2  = Physics.OverlapSphere(backWheel.position, 0.2f, LayerMask.GetMask("Ground"));
        bool isGround = coll1.Length > 0 && coll2.Length > 0;
        if (!isGround)
        {
            return;
        }
        
        
        // 倾斜过度直接不允许矫正
        float angle = Mathf.Abs(Vector3.Angle(transform.forward, Vector3.up));
        if (angle < maxTiltAngle || angle > 180-maxTiltAngle)
            return;
        
        // 玩家输入（A/D键）左右控制
        float input = Input.GetAxis("Horizontal");

        // 施加玩家纠正力矩（绕Z轴旋转来平衡左右）
        rb.AddTorque(-input * balanceTorque * transform.right);

        // 获取当前倾斜程度（车体右方向 与 向下方向夹角）
        float tilt = Vector3.Dot(transform.forward, Vector3.down);
    }
    
    
    


    //状态二：控制前后平衡
    void HandleBalanceControl()
    {
        // 按 C 进入蓄力状态
        if (cPressed)
        {
            // 锁定当前姿态
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            
            state = BikeState.Chagring;
        }
        
        Vector3 downForce = -transform.up * 30f; // 增加吸地力
        rb.AddForceAtPosition(downForce, backWheel.position, ForceMode.Force);
        
        float angle = Mathf.Abs(Vector3.Angle(transform.up, Vector3.up));
        if (angle > 45)
        {
            Vector3 force =  -50f * transform.up ;
            rb.AddForceAtPosition(force, frontWheel.position);
            return;
        }
            
        
        float input = Input.GetAxis("Vertical"); // W/S 控制
        
        Vector3 forceDir = input * 10f * transform.up ;

        // 在车体的“前方”施加力以模拟绕后轮旋转
        rb.AddForceAtPosition(forceDir, frontWheel.position);
    }

    //状态二：跳跃蓄力
    void HandleCharging()
    {
        rb.freezeRotation = true;
        
        // 按 V 退出蓄力状态
        if (vPressed)
        {
            // 锁定当前姿态
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            rb.freezeRotation = false;
            
            _cameraController.isFree = false;
            state = BikeState.Normal;
        }
        
        if (spaceHeld)
        {
            if (!isCharging)
            {
                isCharging = true;
                chargeTimer = 0f;
            }

            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, maxChargeTime);
        }

        if (spaceReleased && isCharging)
        {
            isCharging = false;
            rb.useGravity = true;
            float chargePercent = chargeTimer / maxChargeTime;
            float actualForce = jumpForce * chargeCurve.Evaluate(chargePercent);

            // 按当前前后角跳跃
            
            Vector3 jumpDir = transform.right.normalized;
            //rb.AddForce(jumpDir * actualForce, ForceMode.Impulse);
            rb.velocity = jumpDir * actualForce;
            //state = BikeState.Jumping;
            //Debug.Log(jumpForce);
            
            //_cameraController.isFree = false;
        }
    }
    
    private void JumpingControl()
    {
        float input = Input.GetAxis("Vertical"); // W/S 控制
        
        Vector3 forceDir = input * 10f * transform.up ;
        
        rb.AddForceAtPosition(-forceDir, frontWheel.position);
        rb.AddForceAtPosition(forceDir, backWheel.position);
        
        //检测落地
        var coll1 = Physics.OverlapSphere(frontWheel.position, 0.2f, LayerMask.GetMask("Ground"));
        var coll2  = Physics.OverlapSphere(backWheel.position, 0.2f, LayerMask.GetMask("Ground"));
        bool isGround = coll1.Length > 0 && coll2.Length > 0;
        if (isGround)
        {
            state = BikeState.Normal;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        
            
    }
}