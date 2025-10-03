using UnityEngine;

// プレイヤーに追従するカメラのスクリプト
// プレイヤーの子にカメラを入れてそのカメラにアタッチして使う
public class PlayerCamera : MonoBehaviour {
    // プレイヤーとカメラの位置
    public Transform player;
    public Vector3 normalOffset = new Vector3(0.0f, 1.0f, -4.0f);

    // 壁判定用のレイヤー(これに指定されたレイヤーの物はカメラが貫通しない)
    public LayerMask collisionMask;

    // カメラの補間速度と距離
    private float minDistance = 0.5f;
    private float moveSpeed = 10f;

    // カメラのオフセット
    private Vector3 currentOffset;
    private Vector3 targetOffset;

    void Start() {
        currentOffset = normalOffset;
        targetOffset = normalOffset;
    }

    private void LateUpdate() {
        if (!player) return;

        // カメラの動きをスムーズに補間
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, moveSpeed * Time.deltaTime);

        Vector3 targetPos = player.position;
        Vector3 desiredPos = player.TransformPoint(currentOffset);

        // 壁補正
        if (Physics.Linecast(targetPos, desiredPos, out RaycastHit hit, collisionMask)) {
            float dist = Vector3.Distance(targetPos, hit.point);
            dist = Mathf.Max(dist, minDistance);
            Vector3 dir = (desiredPos - targetPos).normalized;
            desiredPos = targetPos + dir * dist;
        }

        transform.position = desiredPos;
    }   
}