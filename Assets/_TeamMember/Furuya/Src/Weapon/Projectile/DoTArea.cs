using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DoTエリア用　古谷
/// </summary>

public class DoTArea : NetworkBehaviour {
    [SyncVar] private int ownerTeamID;
    private string ownerName;
    private float lifetime = 5f;
    private Rigidbody rb;
    private float speed = 20f;
    private bool initialized;

    [Header("DoT Settings")]
    private int damage = 10;
    [SerializeField] private float interval = 1f;
    [SerializeField] private string targetTag = "Player";

    private Dictionary<GameObject, float> timers = new();


    /// <summary>
    /// 弾の初期化（発射時に呼ぶ）
    /// </summary>
    public void Init(int teamID, string _name, float _speed, int _damage, Vector3 direction) {
        ownerTeamID = teamID;
        ownerName = _name;
        speed = _speed;
        damage = _damage;

        if (rb == null) rb = GetComponent<Rigidbody>();

        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
        }

        initialized = true;

        if (isServer) {
            StopAllCoroutines();
            StartCoroutine(AutoDisable()); // 自動で非アクティブ化
        }
    }

    void FixedUpdate() {
        if (!isServer) return;
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (!initialized || !isServer) return;
        if (!other.CompareTag(targetTag)) return;
        // 自分自身の発射元には当たらない
        var target = other.GetComponent<CharacterBase>();
        if (target.parameter.TeamID == ownerTeamID) return;

        var character = other.GetComponent<CharacterBase>();
        if (character != null) {
           character.TakeDamage(damage, ownerName);
        }
    }

    [ServerCallback]
    private void OnTriggerStay(Collider other) {
        if (!initialized || !isServer) return;
        if (!other.CompareTag(targetTag)) return;
        // 自分自身の発射元には当たらない
        var target = other.GetComponent<CharacterBase>();
        if (target.parameter.TeamID == ownerTeamID) return;

        if (!timers.ContainsKey(other.gameObject))
            timers[other.gameObject] = 0f;

        timers[other.gameObject] += Time.deltaTime;

        if (timers[other.gameObject] >= interval) {
            timers[other.gameObject] = 0f;

            var character = other.GetComponent<CharacterBase>();
            if (character != null) {
                character.TakeDamage(damage, ownerName);
            }
        }
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other) {
        if (timers.ContainsKey(other.gameObject))
            timers.Remove(other.gameObject);
    }

    /// <summary>
    /// 自動で不可視
    /// </summary>
    /// <returns></returns>
    IEnumerator AutoDisable() {
        yield return new WaitForSeconds(lifetime);
        Deactivate();
    }

    /// <summary>
    /// 非アクティブ化
    /// </summary>
    [Server]
    private void Deactivate() {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        initialized = false;
        timers.Clear();

        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.DespawnToPool(gameObject);
        else
            NetworkServer.Destroy(gameObject);
    }
}
