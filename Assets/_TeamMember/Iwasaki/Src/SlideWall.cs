using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlideWall : NetworkBehaviour {
    [Header("移動設定")]
    public Vector3 moveDirection = Vector3.right;  // 初期方向
    public float moveSpeed = 3f;                   // 移動速度

    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true; // MovePositionではなくTransformを直接操作
    }

    void FixedUpdate() {
        // サーバーだけ動かす
        if (!isServer) return;

        transform.position += moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;
    }

    void OnCollisionEnter(Collision collision) {
        if (!isServer) return;

        // 当たったら進行方向を真逆にする
        moveDirection = -moveDirection;
    }
}
