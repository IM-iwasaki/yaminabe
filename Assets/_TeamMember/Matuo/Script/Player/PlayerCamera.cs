using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TPSカメラ制御スクリプト
/// 壁にめり込まないよう距離補正を行う
/// </summary>
public class PlayerCamera : MonoBehaviour {
    [Header("プレイヤー参照")]
    public Transform player;

    public Vector3 normalOffset = new Vector3(0f, 0f, -4f);

    [Header("カメラ設定")]
    public float rotationSpeed = 120f;
    public float minPitch = -20f;
    public float maxPitch = 60f;
    public float moveSpeed = 10f;
    public float minDistance = 0.3f;
    public float upOffsetAmount = 0f;

    [Header("壁判定用")]
    public LayerMask collisionMask;

    [Header("画面左寄せ設定")]
    public float leftOffsetAmount = 1.0f;

    private float yaw;
    private float pitch;
    private Vector2 lookInput;
    private Vector3 currentOffset;
    private Vector3 targetOffset;

    private void Start() {
        currentOffset = normalOffset;
        targetOffset = normalOffset;
    }

    public void OnLook(InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2>();
    }

    private void LateUpdate() {
        if (!player) return;

        // 回転入力
        yaw += lookInput.x * rotationSpeed * Time.deltaTime;
        pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        targetOffset = rotation * normalOffset;
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, moveSpeed * Time.deltaTime);

        // プレイヤーのワールド位置
        Vector3 playerPos = player.position + Vector3.up * 1.5f;

        // 左寄せ・高さオフセットをインスペクターで調整可能
        Vector3 leftScreenOffset = player.right * -leftOffsetAmount + player.up * upOffsetAmount;
        Vector3 lookTarget = playerPos + leftScreenOffset;

        // 壁補正
        Vector3 desiredPos = playerPos + currentOffset;
        if (Physics.SphereCast(playerPos, 0.2f, currentOffset.normalized, out RaycastHit hit,
                               normalOffset.magnitude, collisionMask)) {
            float hitDist = Mathf.Clamp(hit.distance, minDistance, normalOffset.magnitude);
            desiredPos = playerPos + currentOffset.normalized * hitDist;
        }

        // カメラ更新
        transform.position = desiredPos;
        transform.LookAt(lookTarget);
    }
}