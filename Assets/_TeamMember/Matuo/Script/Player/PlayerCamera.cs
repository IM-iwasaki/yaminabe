using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TPSカメラ制御スクリプト
/// 壁にめり込まないよ
/// <summary>
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
    private float minDistance = 0.5f;  // 壁にめり込まない最小距離

    // 現在と目標のオフセット
    private Vector3 currentOffset;
    private Vector3 targetOffset;

    // 回転制御
    private float yaw;   // 左右回転角度
    private float pitch; // 上下回転角度

    // 入力
    private PlayerInputActions input;
    private Vector2 lookInput;

    /// <summary>
    /// InputActionsを初期化
    /// </summary>
    private void Awake() {
        input = new PlayerInputActions();
        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += _ => lookInput = Vector2.zero;
    }

    /// <summary>
    /// Input Systemを有効化。
    /// </summary>
    private void OnEnable() {
        input.Enable();
    }

    /// <summary>
    /// Input Systemを無効化。
    /// </summary>
    private void OnDisable() {
        input.Disable();
    }

    /// <summary>
    /// カメラオフセットを初期化。
    /// </summary>
    private void Start() {
        currentOffset = normalOffset;
        targetOffset = normalOffset;
    }

    /// <summary>
    /// プレイヤー位置を基準にカメラの位置・回転を更新する。
    /// 入力に応じてyaw・pitchを更新
    /// オフセットを回転させて目標位置を決定
    /// 壁との衝突を補正
    /// カメラの位置と向きを反映
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
        transform.LookAt(player.position + Vector3.up * 1.0f); // プレイヤーを見る
    }
}