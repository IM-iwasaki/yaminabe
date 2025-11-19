using UnityEngine;
using Mirror;
using Mirror.BouncyCastle.Asn1.Pkcs;
using System.Collections;

/// <summary>
/// メイン武器コントローラー
/// </summary>
public class MainWeaponController : NetworkBehaviour {
    public WeaponData weaponData;           // メイン武器
    public Transform firePoint;
    private float lastAttackTime;

    private CharacterEnum.CharaterType charaterType;

    private CharacterBase characterBase; // 名前を取得するため

    void Start() {
        characterBase = GetComponent<CharacterBase>();
        // 追加：キラ   弾薬数を最大にする。
        weaponData.AmmoReset();
    }

    public void SetCharacterType(CharacterEnum.CharaterType type) {
        charaterType = type;
    }

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
                if (weaponData is GunData gunData)
                    StartCoroutine(ServerBurstShoot(direction, gunData.multiShot, gunData.burstDelay));
                break;
            case WeaponType.Magic:
                ServerMagicAttack(direction);
                break;
        }
    }

    /// <summary>
    /// 追加攻撃用
    /// </summary>
    /// <param name="direction"></param>
    [Command]
    public void CmdRequestExtraAttack(Vector3 direction) {
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

    /// <summary>
    /// 攻撃可否判定
    /// </summary>
    /// <returns></returns>
    bool CanAttack() {
        // サブ武器も別クールダウンを持たせる場合は拡張可能
        return weaponData != null && Time.time >= lastAttackTime + weaponData.cooldown;
    }

    /// <summary>
    /// 武器データセット
    /// </summary>
    /// <param name="name"></param>
    [Command]
    public void SetWeaponData(string name) {
        var data = WeaponDataRegistry.GetWeapon(name);

        if (!CanUseWeapon(charaterType, data.type)) {
            Debug.LogWarning($"{charaterType} は {data.weaponName} を装備できません");
            return;
        }

        weaponData = data;
        Debug.LogWarning($"'{data.weaponName}' を使用します");
    }

    private bool CanUseWeapon(CharacterEnum.CharaterType character, WeaponType weapon) {
        return character switch {
            CharacterEnum.CharaterType.Melee => weapon == WeaponType.Melee,
            CharacterEnum.CharaterType.Gunner => weapon == WeaponType.Gun,
            CharacterEnum.CharaterType.Wizard => weapon == WeaponType.Magic,
            _ => false
        };
    }

    // --- 近接攻撃 ---
    void ServerMeleeAttack() {
        if (weaponData is not MeleeData meleeData)
            return;

        int attackLayer = LayerMask.GetMask("Character");
        Collider[] hits = Physics.OverlapSphere(firePoint.position, meleeData.range, attackLayer);
        // プレイヤーの前方ベクトル（視線や武器の向き）
        Vector3 forward = firePoint.forward;

        foreach (var c in hits) {
            var hp = c.GetComponent<CharacterBase>();
            if (hp == null || !IsValidTarget(hp.gameObject)) continue;

            // 対象との方向ベクトル
            Vector3 dir = (c.transform.position - firePoint.position).normalized;

            // forwardとの角度を計算
            float angle = Vector3.Angle(forward, dir);

            if (angle <= meleeData.meleeAngle) {
                hp.TakeDamage(meleeData.damage, characterBase.PlayerName);
                RpcSpawnHitEffect(c.transform.position, meleeData.hitEffectType);
            }
        }
#if UNITY_EDITOR
                MeleeAttackDebugArc.Create(firePoint.position, firePoint.forward, meleeData.range, meleeData.meleeAngle, Color.yellow, 0.5f);
#endif
    }

    // --- 銃撃処理（TPSレティクル方向） ---
    IEnumerator ServerBurstShoot(Vector3 direction, int multiShot, float shootDelay) {
        int count = Mathf.Max(1, multiShot);
        float delay = shootDelay;

        for (int i = 0; i < count; i++) {
            ServerGunAttack(direction);

            // 最後の弾以外は待機
            if (i < count - 1)
                yield return new WaitForSeconds(delay);
        }
    }

    void ServerGunAttack(Vector3 direction) {
        if (weaponData is not GunData gunData || gunData.projectilePrefab == null)
            return;

        //  追加：キラ   弾薬が残っていれば銃の弾薬を消費して通過
        if (weaponData.ammo > 0) weaponData.ammo--;
        else return;

        // 弾をネットワークプールから取得
        GameObject proj = ProjectilePool.Instance.SpawnFromPool(
            gunData.projectilePrefab.name, // プール名で取得
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        if (proj == null) return;

        if (proj.TryGetComponent(out Projectile projScript)) {
            projScript.Init(
                gameObject,
                characterBase.PlayerName,
                gunData.hitEffectType,
                gunData.projectileSpeed,
                gunData.damage
            );
        }
        else if (proj.TryGetComponent(out ExplosionProjectile ExpProjScript)) {
            ExpProjScript.Init(
                gameObject,
                characterBase.PlayerName,
                gunData.hitEffectType,
                gunData.projectileSpeed,
                gunData.damage,
                gunData.explosionRange
            );
        }

        if (proj.TryGetComponent(out Rigidbody rb)) {
            rb.velocity = direction * gunData.projectileSpeed;
        }

        RpcPlayMuzzleFlash(firePoint.position, gunData.muzzleFlashType);
    }

    // --- 魔法攻撃 ---
    void ServerMagicAttack(Vector3 direction) {
        if (weaponData is not MainMagicData magicData || magicData.projectilePrefab == null)
            return;

        GameObject proj = ProjectilePool.Instance.SpawnFromPool(
            magicData.projectilePrefab.name,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );
        AudioManager.Instance.CmdPlayWorldSE("FireBall", transform.position);

        if (proj == null) return;

        if (proj.TryGetComponent(out MagicProjectile projScript)) {
            projScript.Init(
                gameObject,
                characterBase.PlayerName,
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
            var fx = EffectPool.Instance.GetFromPool(prefab, pos, transform.rotation);
            EffectPool.Instance.ReturnToPool(fx, 1.5f);
        }
    }

    // --- クライアントでマズルフラッシュ再生 ---
    [ClientRpc]
    void RpcPlayMuzzleFlash(Vector3 pos, EffectType type) {
        GameObject prefab = EffectPoolRegistry.Instance.GetMuzzleFlash(type);
        if (prefab != null) {
            var fx = EffectPool.Instance.GetFromPool(prefab, pos, transform.rotation);
            EffectPool.Instance.ReturnToPool(fx, 0.8f);
        }
    }

    bool IsValidTarget(GameObject obj) {
        return obj != gameObject; // 自分以外
    }
}


/// <summary>
/// 近接用ヒット判定可視化
/// </summary>
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
