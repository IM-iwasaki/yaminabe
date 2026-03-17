using System.Collections;
using Mirror;
using UnityEngine;

public class Bomb : NetworkBehaviour {
    [Header("参照")]
    [SerializeField] private Renderer rend;
    [SerializeField] private GameObject explosionEffectPrefab;

    [SyncVar(hook = nameof(OnWeaponIDChanged))]
    private int weaponID;

    private MainBombData data;

    private float timer;
    private bool exploded;

    private GameObject owner;
    private string ownerName;
    private int ownerID;

    // ===== 初期化（サーバー）=====
    [Server]
    public void Init(MainBombData bombData, GameObject ownerObj, string name, int id) {
        owner = ownerObj;
        weaponID = bombData.WeaponID;

        ownerName = name;
        ownerID = id;

        timer = 0f;
        exploded = false;
    }

    // ===== クライアント側データ取得 =====
    void OnWeaponIDChanged(int _, int newID) {
        var weapon = WeaponDataRegistry.GetWeapon(newID);

        if (weapon is MainBombData bombData) {
            data = bombData;
        }
        else {
            Debug.LogError($"Bomb: ID {newID} はMainBombDataではない");
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
        // 鼓動
        float speed = Mathf.Lerp(1f, 10f, t);
        float amp = Mathf.Lerp(0.05f, 0.3f, t);
        float scale = 1f + Mathf.Sin(Time.time * speed) * amp;
        transform.localScale = Vector3.one * scale;

        // 色
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

        RpcPlayExplosionEffect(transform.position);

        switch (data.explosionType) {
            case ExplosionType.Center:
                ExplodeCenter();
                break;

            case ExplosionType.Cross:
                foreach (var dir in GetDirs())
                    StartCoroutine(ExplodeLine(dir));
                break;
        }

        StartCoroutine(DestroyAfter());
    }

    // ===== 中心爆破 =====
    [Server]
    void ExplodeCenter() {
        var hits = Physics.OverlapSphere(transform.position, data.explosionRadius);

        foreach (var c in hits) {
            ApplyDamage(c);

            if (data.chainReaction)
                TryChain(c);
        }
    }

    // ===== 十字爆破 =====
    Vector3[] GetDirs() => new Vector3[]
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right
    };

    [Server]
    IEnumerator ExplodeLine(Vector3 dir) {
        float distance = data.maxDistance;

        if (Physics.Raycast(transform.position, dir, out var hit, data.maxDistance, data.wallLayer)) {
            distance = hit.distance;
        }

        for (float d = data.interval; d <= distance; d += data.interval) {
            Vector3 pos = transform.position + dir * d;

            RpcPlayExplosionEffect(pos);

            var hits = Physics.OverlapSphere(pos, 0.5f);

            foreach (var c in hits) {
                ApplyDamage(c);

                if (data.chainReaction)
                    TryChain(c);
            }

            yield return new WaitForSeconds(data.delayBetween);
        }
    }

    // ===== ダメージ =====
    [Server]
    void ApplyDamage(Collider col) {
        var target = col.GetComponent<CharacterBase>();
        if (!target) return;

        if (!data.damageSelf && target.gameObject == owner) return;
        if (!data.damageAlly && IsAlly(target)) return;

        target.TakeDamage(data.damage, ownerName, ownerID);
    }

    bool IsAlly(CharacterBase target) {
        var ownerChar = owner.GetComponent<CharacterBase>();
        if (!ownerChar) return false;

        return ownerChar.parameter.TeamID == target.parameter.TeamID;
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
        Instantiate(explosionEffectPrefab, pos, Quaternion.identity);
    }

    // ===== 削除 =====
    [Server]
    IEnumerator DestroyAfter() {
        yield return new WaitForSeconds(1f);
        NetworkServer.Destroy(gameObject);
    }
}