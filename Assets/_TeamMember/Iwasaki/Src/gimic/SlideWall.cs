using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlideWall : NetworkBehaviour {
    [Header("ˆÚ“®İ’è")]
    public Vector3 moveDirection = Vector3.right;  // ‰Šú•ûŒü
    public float moveSpeed = 3f;                   // ˆÚ“®‘¬“x
    public float bounceCooldown = 0.2f;            // Ä”½“]‚Ü‚Å‚Ì‘Ò‹@ŠÔ

    private Rigidbody rb;
    private bool canBounce = true;                 // ”½“]‰Â”\‚©‚Ç‚¤‚©

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false; // Õ“Ë‚ğ—LŒø‚É
    }

    void FixedUpdate() {
        if (!isServer) return;

        rb.MovePosition(rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter(Collision collision) {
        if (!isServer || !canBounce) return;

        // ”½“]ˆ—
        moveDirection = -moveDirection;
        rb.position += moveDirection.normalized * 0.1f; // ­‚µ‰Ÿ‚µ–ß‚·
        canBounce = false;

        // ˆê’èŠÔŒã‚ÉÄ‚Ñ”½“]‰Â”\‚É‚·‚é
        StartCoroutine(ResetBounceCooldown());
    }

    private System.Collections.IEnumerator ResetBounceCooldown() {
        yield return new WaitForSeconds(bounceCooldown);
        canBounce = true;
    }
}
