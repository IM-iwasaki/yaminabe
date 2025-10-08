using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TPSカメラ制御スクリプト
/// 壁にめり込まないよ
/// </summary>
public class PlayerCamera : MonoBehaviour {
    [Header("プレイヤー参照")]
    public Transform player;
    public Vector3 normalOffset = new Vector3(0.0f, 1.0f, -4.0f);

    [Header("壁判定用のレイヤー")]
    public LayerMask collisionMask;

    [Header("カメラ設定")]
    public float rotationSpeed = 120f; // 回転速度（マウス感度）
    public float minPitch = -30f;      // 下方向の制限角度
    public float maxPitch = 70f;       // 上方向の制限角度
    public float moveSpeed = 10f;      // 補間速度
    private float minDistance = 0.1f;  // 壁にめり込まない最小距離

    // 現在と目標のオフセット
    private Vector3 currentOffset;
    private Vector3 targetOffset;

    // 回転制御
    private float yaw;   // 左右回転角度
    private float pitch; // 上下回転角度

    // 入力（PlayerInputイベントで受け取る）
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

        // 入力による回転更新
        yaw += lookInput.x * rotationSpeed * Time.deltaTime;
        pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 回転計算
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // ターゲットオフセット
        targetOffset = rotation * normalOffset;
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, moveSpeed * Time.deltaTime);

        // プレイヤー基準位置
        Vector3 targetPos = player.position;
        Vector3 desiredPos = targetPos + currentOffset;

        // 壁補正
        if (Physics.Linecast(targetPos, desiredPos, out RaycastHit hit, collisionMask)) {
            float dist = Vector3.Distance(targetPos, hit.point);
            dist = Mathf.Max(dist, minDistance);
            Vector3 dir = (desiredPos - targetPos).normalized;
            desiredPos = targetPos + dir * dist;
        }

        // カメラ更新
        transform.position = desiredPos;
        transform.LookAt(player.position + Vector3.up * 1.0f);
    }
}