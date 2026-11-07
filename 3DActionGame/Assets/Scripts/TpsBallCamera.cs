using UnityEngine;
// UnityのInputSystemを使用する
using UnityEngine.InputSystem;

/// <summary>
/// ボールを斜め上から追従し、JIKL／右スティックで視点を回す TPS 風カメラ
/// （Main Camera に付ける。ボールの移動は WASD／左スティックで Player 側が行う）
/// </summary>
public class TpsBallCamera : MonoBehaviour
{
    [Header("追う対象（ボール）")]
    [SerializeField] Transform target;
    [Header("カメラ -- 後ろへの距離")]
    [SerializeField] float distance = 8f;
    [Header("カメラ -- 高さ")]
    [SerializeField] float height = 5f;
    [Header("カメラ -- 視点の左右の速さ（JIKL・右スティック用）")]
    [SerializeField] float lookSensitivityX = 120f;
    [Header("カメラ -- 視点の上下の速さ（JIKL・右スティック用）")]
    [SerializeField] float lookSensitivityY = 90f;
    [Header("カメラ -- 上下の下限（度）")]
    [SerializeField] float pitchMin = 10f;
    [Header("カメラ -- 上下の上限（度）")]
    [SerializeField] float pitchMax = 70f;
    [Header("カメラ -- 位置の追従のなめらかさ（大きいほど遅い）")]
    [SerializeField] float positionSmooth = 10f;

    InputAction lookAction; // Player/Look（JIKL・右スティック）
    float yaw;   // 左右の角度
    float pitch = 25f; // 上下の角度（斜め上から見る初期値）

    /// <summary>
    /// オブジェクトが読み込まれたとき、呼ばれる
    /// </summary>
    void Awake()
    {
        lookAction = InputSystem.actions.FindAction("Player/Look");
    }

    /// <summary>
    /// オブジェクトやスクリプトが有効になった時
    /// </summary>
    void OnEnable()
    {
        if (lookAction != null)
        {
            lookAction.Enable();
        }
    }

    /// <summary>
    /// オブジェクトやスクリプトが無効・消えるときに呼ばれる
    /// </summary>
    void OnDisable()
    {
        if (lookAction != null)
        {
            lookAction.Disable();
        }
    }

    /// <summary>
    /// 全ての Update が終わった後に、毎フレーム呼ばれる
    /// </summary>
    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // JIKL／右スティックで視点を回す
        UpdateLookAngles();

        // ボールの位置を基準に、斜め後ろ上の座標を計算する（ボールの回転は使わない）
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = orbit * new Vector3(0f, 0f, -distance) + Vector3.up * height;
        Vector3 desiredPosition = target.position + offset;

        // 位置をなめらかに寄せる
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            1f - Mathf.Exp(-positionSmooth * Time.deltaTime));

        // ボールを見る（上方向は世界の上のまま）
        transform.LookAt(target.position, Vector3.up);
    }

    /// <summary>
    /// Look 入力から、左右・上下の角度を更新する
    /// </summary>
    void UpdateLookAngles()
    {
        if (lookAction == null)
        {
            return;
        }

        // JIKL・スティックはだいたい -1〜1。毎秒何度回すか、として deltaTime を掛ける
        Vector2 look = lookAction.ReadValue<Vector2>();
        yaw += look.x * lookSensitivityX * Time.deltaTime;
        pitch -= look.y * lookSensitivityY * Time.deltaTime;
        // 真上・真下まで回りすぎないようにする
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }
}