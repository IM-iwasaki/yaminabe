using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TPSカメラ制御スクリプト
/// 壁にめり込まないよう距離補正を行う
/// </summary>
public class PlayerCamera : MonoBehaviour {
    [Header("プレイヤー参照")]
    public Transform player;
    public Vector3 normalOffset = new Vector3(0f, 1f, -4f);

    [Header("壁判定用のレイヤー")]
    public LayerMask collisionMask;

    [Header("カメラ設定")]
    public float rotationSpeed = 120f;
    public float minPitch = -30f;
    public float maxPitch = 70f;
    public float moveSpeed = 10f;
    private float minDistance = 0.3f;  // カメラがプレイヤーに近づきすぎない距離

    // 現在と目標のオフセット
    private Vector3 currentOffset;
    private Vector3 targetOffset;
    private float yaw;
    private float pitch;
    private Vector2 lookInput;

    /// <summary>
    /// カメラオフセットを初期化。
    /// </summary>
    private void Start() {
        currentOffset = normalOffset;
        targetOffset = normalOffset;
    }

    /// <summary>
    /// PlayerInput の "Look" アクションイベントから呼ばれる関数
    /// </summary>
    public void OnLook(InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// プレイヤー位置を基準にカメラの位置・回転を更新する。
    /// </summary>
    private void LateUpdate() {
        if (!player) return;

        // 入力で回転更新
        yaw += lookInput.x * rotationSpeed * Time.deltaTime;
        pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 回転計算
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        targetOffset = rotation * normalOffset;

        // スムーズに補間
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, moveSpeed * Time.deltaTime);

        // プレイヤー位置
        Vector3 targetPos = player.position + Vector3.up * 1.0f;
        Vector3 desiredPos = targetPos + currentOffset;

        // 壁補正（距離のみ調整）
        if (Physics.SphereCast(targetPos, 0.2f, currentOffset.normalized, out RaycastHit hit,
                               normalOffset.magnitude, collisionMask)) {
            float hitDist = Mathf.Clamp(hit.distance, minDistance, normalOffset.magnitude);
            desiredPos = targetPos + currentOffset.normalized * hitDist;
        }

        // カメラ位置・回転更新
        transform.position = desiredPos;
        transform.LookAt(targetPos);
    }
}