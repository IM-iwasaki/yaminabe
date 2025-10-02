using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    // プレイヤーとカメラの位置
    public Transform player;
    public Vector3 normalOffset = new Vector3(1.7f, 0.6f, -3.1f);

    // 壁判定用
    public LayerMask collisionMask;
    public float minDistance = 0.5f;
    public float moveSpeed = 10f;

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