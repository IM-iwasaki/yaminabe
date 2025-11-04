using UnityEngine;
using Mirror;
using Mirror.BouncyCastle.Asn1.Pkcs;

public class MainWeaponController : NetworkBehaviour {
    public WeaponData weaponData;           // メイン武器
    public Transform firePoint;
    float lastAttackTime;

    // --- 攻撃リクエスト ---
    [Command]
    public void CmdRequestAttack(Vector3 direction) {
        if (!CanAttack()) return;
        lastAttackTime = Time.time;

        switch (weaponData.type) {
            case WeaponType.Melee:
                ServerMeleeAttack();
                break;
            case WeaponType.Gun:
                ServerGunAttack(direction);
                break;
            case WeaponType.Magic:
                ServerMagicAttack(direction);
                break;
        }
    }

    [Command]
    public void CmdRequestSkillAttack(Vector3 direction) {
        lastAttackTime = Time.time;

        switch (weaponData.type) {
            case WeaponType.Melee:
                ServerMeleeAttack();
                break;
            case WeaponType.Gun:
                ServerGunAttack(direction);
                break;
            case WeaponType.Magic:
                ServerMagicAttack(direction);
                break;
        }
    }

    bool CanAttack() {
        // サブ武器も別クールダウンを持たせる場合は拡張可能
        return weaponData != null && Time.time >= lastAttackTime + weaponData.cooldown;
    }

    [Command]
    public void SetWeaponData(string name) {
        var data = WeaponDataRegistry.GetWeapon(name);

        Debug.LogWarning($"'{data.weaponName}'　を使用します");
        weaponData = data;
    }

    // --- 近接攻撃 ---
    void ServerMeleeAttack() {
        int attackLayer = LayerMask.GetMask("Character");
        Collider[] hits = Physics.OverlapSphere(firePoint.position, weaponData.range, attackLayer);
        // プレイヤーの前方ベクトル（視線や武器の向き）
        Vector3 forward = firePoint.forward;

        foreach (var c in hits) {
            var hp = c.GetComponent<CharacterBase>();
            if (hp == null || !IsValidTarget(hp.gameObject)) continue;

            // 対象との方向ベクトル
            Vector3 dir = (c.transform.position - firePoint.position).normalized;

            // forwardとの角度を計算
            float angle = Vector3.Angle(forward, dir);

            // 半円判定：前方180度（つまり90°以内なら当たり）
            if (angle <= weaponData.meleeAngle) {
                hp.TakeDamage(weaponData.damage);
                RpcSpawnHitEffect(c.transform.position, weaponData.hitEffectType);
            }
        }
#if UNITY_EDITOR
                MeleeAttackDebugArc.Create(firePoint.position, firePoint.forward, weaponData.range, weaponData.meleeAngle, Color.yellow, 0.5f);
#endif
    }

    // --- 銃撃処理（TPSレティクル方向） ---
    void ServerGunAttack(Vector3 direction) {
        if (weaponData.projectilePrefab == null) return;

        // 弾をネットワークプールから取得
        GameObject proj = ProjectilePool.Instance.SpawnFromPool(
            weaponData.projectilePrefab.name, // プール名で取得
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        if (proj == null) return;

        if (proj.TryGetComponent(out Projectile projScript)) {
            projScript.Init(
                gameObject,
                weaponData.hitEffectType,
                weaponData.projectileSpeed,
                weaponData.damage
            );
        }

        if (proj.TryGetComponent(out Rigidbody rb)) {
            rb.velocity = direction * weaponData.projectileSpeed;
        }

        RpcPlayMuzzleFlash(firePoint.position, weaponData.muzzleFlashType);
    }

    // --- 魔法攻撃 ---
    void ServerMagicAttack(Vector3 direction) {
        if (weaponData is not MagicWeaponData magicData || magicData.projectilePrefab == null)
            return;

        GameObject proj = ProjectilePool.Instance.SpawnFromPool(
            magicData.projectilePrefab.name,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        if (proj == null) return;

        if (proj.TryGetComponent(out MagicProjectile projScript)) {
            projScript.Init(
                gameObject,
                magicData.magicType,
                magicData.hitEffectType,
                magicData.projectileSpeed,
                magicData.initialHeightSpeed,
                magicData.damage,
                direction
            );
        }

        RpcPlayMuzzleFlash(firePoint.position, magicData.muzzleFlashType);
    }

    // --- クライアントでヒットエフェクト再生 ---
    [ClientRpc]
    void RpcSpawnHitEffect(Vector3 pos, EffectType type) {
        GameObject prefab = EffectPoolRegistry.Instance.GetHitEffect(type);
        if (prefab != null) {
            var fx = WeaponEffectPool.Instance.GetFromPool(prefab, pos, transform.rotation);
            WeaponEffectPool.Instance.ReturnToPool(fx, 1.5f);
        }
    }

    // --- クライアントでマズルフラッシュ再生 ---
    [ClientRpc]
    void RpcPlayMuzzleFlash(Vector3 pos, EffectType type) {
        GameObject prefab = EffectPoolRegistry.Instance.GetMuzzleFlash(type);
        if (prefab != null) {
            var fx = WeaponEffectPool.Instance.GetFromPool(prefab, pos, transform.rotation);
            WeaponEffectPool.Instance.ReturnToPool(fx, 0.8f);
        }
    }

    bool IsValidTarget(GameObject obj) {
        return obj != gameObject; // 自分以外
    }
}

#if UNITY_EDITOR
public class MeleeAttackDebugArc : MonoBehaviour {
    private float range;
    private float angle;
    private Color color;
    private float duration;
    private float timer;
    private Vector3 forward;

    public static void Create(Vector3 pos, Vector3 forward, float range, float angle, Color color, float duration) {
        var obj = new GameObject("MeleeAttackDebugArc");
        var arc = obj.AddComponent<MeleeAttackDebugArc>();
        arc.range = range;
        arc.angle = angle;
        arc.color = color;
        arc.duration = duration;
        arc.forward = forward;
        obj.transform.position = pos;
    }

    private void Update() {
        timer += Time.deltaTime;
        if (timer >= duration) Destroy(gameObject);
    }

    private void OnDrawGizmos() {
        Gizmos.color = color;
        int segments = 20;
        Vector3 leftDir = Quaternion.Euler(0, -angle, 0) * forward;
        Vector3 prevPoint = transform.position + leftDir * range;

        for (int i = 1; i <= segments; i++) {
            float currentAngle = -angle + (angle * 2f / segments) * i;
            Vector3 nextPoint = transform.position + (Quaternion.Euler(0, currentAngle, 0) * forward) * range;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        Gizmos.DrawRay(transform.position, leftDir * range);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, angle, 0) * forward * range);
    }
}
#endif
