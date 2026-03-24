using System.Collections;
using Mirror;
using UnityEngine;

public class Bomb : NetworkBehaviour {
    [Header("参照")]
    [SerializeField] private Renderer rend;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private GameObject visualRoot; // ←追加（本体＋導火線）

    [SyncVar(hook = nameof(OnWeaponIDChanged))]
    private int weaponID;

    private MainBombData data;

    private float timer;
    private bool exploded;

    private Vector3 baseScale;

    private GameObject owner;
    private string ownerName;
    private int ownerID;

    private int activeExplosionLines;

    void Awake() {
        baseScale = transform.localScale;
    }

    void OnEnable() {
        timer = 0f;
        exploded = false;
        activeExplosionLines = 0;

        // 見た目復帰
        if (visualRoot != null)
            visualRoot.SetActive(true);
    }

    // ===== 初期化（サーバー）=====
    [Server]
    public void Init(MainBombData bombData, GameObject ownerObj, string name, int id, Vector3 forward) {
        weaponID = bombData.WeaponID;

        owner = ownerObj;
        ownerName = name;
        ownerID = id;

        timer = 0f;
        exploded = false;

        var rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(forward * bombData.throwForce, ForceMode.Impulse);
    }

    // ===== クライアント側データ取得 =====
    void OnWeaponIDChanged(int _, int newID) {
        var weapon = WeaponDataRegistry.GetWeapon(newID);

        if (weapon is MainBombData bombData) {
            data = bombData;
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();

        if (data == null) {
            OnWeaponIDChanged(0, weaponID);
        }
    }

    // ===== 更新 =====
    void Update() {
        if (data == null) return;

        timer += Time.deltaTime;
        float t = timer / data.explodeTime;

        UpdateVisual(t);

        if (!isServer) return;

        if (!exploded && timer >= data.explodeTime) {
            Explode();
        }
    }

    // ===== 見た目 =====
    void UpdateVisual(float t) {
        float speed = 2f + t * 2f;
        float amp = 0.05f + t * 0.1f;
        float scale = 1f + Mathf.Sin(Time.time * speed) * amp;
        transform.localScale = baseScale * scale;

        if (t > 0.8f) {
            float blink = Mathf.Sin(Time.time * 20f);
            rend.material.color = blink > 0 ? data.dangerColor : Color.white;
        }
        else {
            rend.material.color = Color.Lerp(data.safeColor, data.dangerColor, t);
        }
    }

    // ===== 爆破 =====
    [Server]
    void Explode() {
        exploded = true;

        RpcHideVisual();
        RpcPlayExplosionEffect(transform.position);
        AudioManager.Instance.CmdPlayWorldSE("Explode", transform.position);

        switch (data.explosionType) {
            case ExplosionType.Center:
                ExplodeCenter();
                Deactivate();
                break;

            case ExplosionType.Cross:
                var dirs = GetDirs();
                activeExplosionLines = dirs.Length;

                ExplodeCenter();
                foreach (var dir in dirs)
                    StartCoroutine(ExplodeLine(dir));
                break;
        }
    }

    // ===== 見た目非表示 =====
    [ClientRpc]
    void RpcHideVisual() {
        if (visualRoot != null)
            visualRoot.SetActive(false);
    }

    // ===== 中心爆破 =====
    [Server]
    void ExplodeCenter() {
        var hits = Physics.OverlapSphere(transform.position, data.explosionRange);

        foreach (var c in hits) {
            ApplyDamage(c, transform.position);

            if (data.chainReaction)
                TryChain(c);
        }
    }

    // ===== 十字爆破 =====
    Vector3[] GetDirs() {
        int count = data.explosionLines;

        Vector3[] dirs = new Vector3[count];

        for (int i = 0; i < count; i++) {
            float angle = (360f / count) * i;
            float rad = angle * Mathf.Deg2Rad;

            dirs[i] = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        }

        return dirs;
    }

    [Server]
    IEnumerator ExplodeLine(Vector3 dir) {
        float distance = data.maxDistance;

        if (Physics.Raycast(transform.position, dir, out var hit, data.maxDistance, data.wallLayer)) {
            distance = hit.distance;
        }

        for (float d = data.interval; d <= distance; d += data.interval) {
            Vector3 pos = transform.position + dir * d;

            if (Physics.Raycast(pos + Vector3.up, Vector3.down, out var groundHit, 10f)) {
                pos = groundHit.point + Vector3.up * 0.1f;
            }

            RpcPlayExplosionEffect(pos);

            var hits = Physics.OverlapSphere(pos, 3.0f);

            foreach (var c in hits) {
                ApplyDamage(c, pos);

                if (data.chainReaction)
                    TryChain(c);
            }

            yield return new WaitForSeconds(data.delayBetween);
        }

        activeExplosionLines--;

        if (activeExplosionLines <= 0) {
            Deactivate();
        }
    }

    // ===== ダメージ =====
    [Server]
    void ApplyDamage(Collider col, Vector3 origin) {
        var target = col.GetComponent<CreatureBase>();
        if (!target) return;

        Vector3 targetPos = target.transform.position;
        if (Physics.Linecast(origin, targetPos, data.wallLayer)) return;

        if (data.damageSelf && target.gameObject == owner) {
            target.TakeDamage(data.damage / 2, ownerName, ownerID);
            return;
        }
        if (!data.damageAlly && IsAlly(target)) return;

        target.TakeDamage(data.damage, ownerName, ownerID);
    }

    bool IsAlly(CreatureBase target) {
        var ownerChar = owner.GetComponent<CreatureBase>();
        if (!ownerChar) return false;

        return ownerChar.parameter.TeamID == target.teamID;
    }

    // ===== 誘爆 =====
    [Server]
    void TryChain(Collider col) {
        var bomb = col.GetComponent<Bomb>();
        if (bomb && !bomb.exploded) {
            bomb.ForceExplode();
        }
    }

    [Server]
    public void ForceExplode() {
        if (exploded) return;
        Explode();
    }

    // ===== エフェクト同期 =====
    [ClientRpc]
    void RpcPlayExplosionEffect(Vector3 pos) {
        var obj = Instantiate(explosionEffectPrefab, pos, Quaternion.identity);
        Destroy(obj, 2f);
    }

    // ===== 削除 =====
    [Server]
    public void Deactivate() {

        foreach (var ps in GetComponentsInChildren<ParticleSystem>()) {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.DespawnToPool(gameObject);
        else
            NetworkServer.Destroy(gameObject);
    }
}