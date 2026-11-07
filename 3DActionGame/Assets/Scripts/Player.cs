using UnityEngine;
// UnityのInputSystemを使用する
using UnityEngine.InputSystem;

/// <summary>
/// ボールを AddForce で転がすスクリプト
/// </summary>
public class Player : MonoBehaviour
{
    [Header("転がり -- 力の強さ")]
    [SerializeField] float moveForce = 12f;
    [Header("転がり -- 速さの上限（抜け防止にも効く）")]
    [SerializeField] float maxSpeed = 10f;
    [Header("ジャンプ -- 一瞬の力（使わないなら 0）")]
    [SerializeField] float jumpImpulse = 5f;
    [Header("接地判定 -- 足元からの距離（ボール半径より少し大きめ）")]
    [SerializeField] float groundCheckDistance = 0.6f;
    [Header("接地判定 -- 地面とみなすレイヤー")]
    [SerializeField] LayerMask groundLayers;
    [Header("カメラ（力を加える前方向の基準。未設定なら Main Camera）")]
    [SerializeField] Transform cameraTransform;

    Rigidbody rigidBody;
    InputAction moveAction;
    InputAction jumpAction;
    Vector2 moveInput;
    bool isGrounded; // 地面に接しているか

    /// <summary>
    /// オブジェクトが読み込まれたとき、呼ばれる
    /// </summary>
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Player/Move");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    /// <summary>
    /// オブジェクトやスクリプトが有効になった時
    /// </summary>
    void OnEnable()
    {
        moveAction.Enable();
        if (jumpAction != null)
        {
            jumpAction.Enable();
        }
    }

    /// <summary>
    /// オブジェクトやスクリプトが無効・消えるときに呼ばれる
    /// </summary>
    void OnDisable()
    {
        moveAction.Disable();
        if (jumpAction != null)
        {
            jumpAction.Disable();
        }
    }

    /// <summary>
    /// 毎フレーム呼ばれる
    /// </summary>
    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        UpdateGrounded();
        TryJump();
    }

    /// <summary>
    /// 一定間隔で、物理の計算の前に呼ばれる
    /// </summary>
    void FixedUpdate()
    {
        // カメラの水平方向を基準に、前後左右の力の向きを作る
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 forceDir = forward * moveInput.y + right * moveInput.x;
        if (forceDir.sqrMagnitude > 1f)
        {
            forceDir.Normalize();
        }

        // 力を加えて転がす（Force = 継続的な加速）
        rigidBody.AddForce(forceDir * moveForce, ForceMode.Force);

        // 水平方向の速さだけ上限を掛ける（速すぎると抜けやすい）
        Vector3 velocity = rigidBody.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        if (horizontal.magnitude > maxSpeed)
        {
            horizontal = horizontal.normalized * maxSpeed;
            rigidBody.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
        }
    }

    /// <summary>
    /// 真下に短い線を飛ばし、地面レイヤーに当たっていれば接地とする
    /// </summary>
    void UpdateGrounded()
    {
        // 球の中心から少し下へ Ray を飛ばす（自分の Collider に当たらないよう、少し外側から）
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayers);
    }

    /// <summary>
    /// 地面にいるときだけ、ジャンプボタンの瞬間に上へ Impulse を加える（加点）
    /// </summary>
    void TryJump()
    {
        if (jumpAction == null || jumpImpulse <= 0f)
        {
            return;
        }
        if (!jumpAction.WasPressedThisFrame())
        {
            return;
        }
        // 空中では跳ばない（連続ジャンプ防止）
        if (!isGrounded)
        {
            return;
        }
        // Impulse は一瞬だけ力を加える
        rigidBody.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
    }

    /// <summary>
    /// Scene ビューで選んでいるとき、接地判定の線を緑で表示する
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}