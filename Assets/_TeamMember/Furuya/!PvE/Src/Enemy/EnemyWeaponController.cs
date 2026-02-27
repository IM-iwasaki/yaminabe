using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// 敵用武器コントローラー
/// </summary>
public class EnemyWeaponController : NetworkBehaviour {
    public WeaponData weaponData;           // メイン武器
    public WeaponData SkillWeaponData;
    public Transform firePoint;
    private GameObject activeChargeFx;
    private EnemyStatusBase enemyBase;

    private void Awake() {
        enemyBase = GetComponent<EnemyStatusBase>();
    }

    // --- 攻撃リクエスト ---
    [Server]
    public void ServerRequestAttack(Vector3 direction) {
        if (weaponData.type == WeaponType.Enemy) {
            ExecuteWeapon(weaponData, direction);
        }
    }

    // --- 攻撃リクエスト ---
    [Server]
    public void ServerRequestSkill(Vector3 direction) {
        if (SkillWeaponData.type == WeaponType.Enemy) {
            ExecuteWeapon(SkillWeaponData, direction);
        }
    }

    [Server]
    void ExecuteWeapon(WeaponData data, Vector3 direction) {
        if (data == null || data.type != WeaponType.Enemy)
            return;

        if (data is MeleeData meleeData)
            StartCoroutine(ServerMeleeCombo(meleeData.combo, meleeData.comboDelay));

        else if (data is MainMagicData magicData)
            ServerStartMagicCast(magicData, direction);
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
            if (hp == null || !IsValidTarget(hp.gameObject) || hp.teamID == -1) continue;
            hp.TakeDamage(meleeData.damage, enemyBase.statusData.enemyName, -1);
            RpcSpawnHitEffect(c.transform.position, meleeData.hitEffectType);
        }
        AudioManager.Instance.CmdPlayWorldSE(meleeData.se.ToString(), transform.position);
#if UNITY_EDITOR
        MeleeAttackDebugArc.Create(firePoint.position, firePoint.forward, meleeData.range, meleeData.meleeAngle, Color.yellow, 0.5f);
#endif
    }

    IEnumerator ServerMeleeCombo(int combo, float comboDelay) {
        int count = Mathf.Max(1, combo);
        float delay = comboDelay;

        for (int i = 0; i < count; i++) {
            ServerMeleeAttack();

            // 最後の以外は待機
            if (i < count - 1)
                yield return new WaitForSeconds(delay);
        }
    }

    // --- 魔法攻撃 ---
    void ServerMagicAttack(MainMagicData magic, Vector3 direction) {
        if (magic == null || magic.projectilePrefab == null)
            return;

        Vector3 safeDir = direction;
        safeDir.y = 0f;

        if (safeDir.sqrMagnitude < 0.0001f) {
            safeDir = firePoint.forward;
        }

        safeDir.Normalize();

        Quaternion rot = Quaternion.LookRotation(safeDir);

        GameObject proj = null;

        if (magic.magicType == ProjectileType.DoT) {
            Vector3 spawnPos = transform.position;
            Quaternion dotRot = rot;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f)) {
                spawnPos = hit.point;
                dotRot = Quaternion.LookRotation(safeDir, hit.normal);
            }

            proj = ProjectilePool.Instance.SpawnFromPool(
                magic.projectilePrefab.name,
                spawnPos,
                dotRot
            );
        }
        else {
            proj = ProjectilePool.Instance.SpawnFromPool(
                magic.projectilePrefab.name,
                firePoint.position,
                rot
            );
        }

        if (proj == null)
            return;

        if (proj.TryGetComponent(out MagicProjectile mp)) {
            mp.Init(
                gameObject,
                "Boss",
                -1,
                magic.magicType,
                magic.hitEffectType,
                magic.projectileSpeed,
                magic.initialHeightSpeed,
                magic.damage,
                safeDir
            );
        } 
        else if (proj.TryGetComponent(out DoTArea dot)) {
            dot.Init(
                -2,
                "Boss",
                -2,
                magic.hitEffectType,
                magic.projectileSpeed,
                magic.damage
            );
        }
    }


    /// <summary>
    /// 詠唱開始
    /// </summary>
    /// <param name="direction"></param>
    [Server]
    public void ServerStartMagicCast(MainMagicData magic, Vector3 direction) {
        if (magic.chargeTime > 0)
            RpcPlayChargeEffect(firePoint.position, magic.chargeEffectType);

        StartCoroutine(CastAfterDelay(magic, direction));
    }

    [Server]
    private IEnumerator CastAfterDelay(MainMagicData magicData, Vector3 direction) {
        yield return new WaitForSeconds(magicData.chargeTime);

        // 発射エフェクト (チャージ停止＆マズルフラッシュ)
        RpcCastMagic(firePoint.position, magicData.muzzleFlashType);

        // 弾の生成
        ServerMagicAttack(magicData, direction);

        // SE はここでサーバー再生
        AudioManager.Instance.CmdPlayWorldSE(magicData.se.ToString(), transform.position);
    }

    // --- チャージエフェクト再生 ---
    [ClientRpc]
    void RpcPlayChargeEffect(Vector3 pos, EffectType type) {
        GameObject prefab = EffectPoolRegistry.Instance.GetChargeEffect(type);
        if (prefab != null) {
            activeChargeFx = EffectPool.Instance.GetFromPool(prefab, pos, transform.rotation);

            //シュートポイントに追従
            activeChargeFx.transform.SetParent(firePoint);
            activeChargeFx.transform.localPosition = Vector3.zero;
            activeChargeFx.transform.localRotation = Quaternion.identity;
        }
    }

    [ClientRpc]
    void RpcCastMagic(Vector3 pos, EffectType muzzleFlashType) {
        // チャージ停止
        if (activeChargeFx != null) {
            EffectPool.Instance.ReturnToPool(activeChargeFx, 0.01f);
            activeChargeFx = null;
        }

        // マズルフラッシュ
        GameObject prefab = EffectPoolRegistry.Instance.GetMuzzleFlash(muzzleFlashType);
        if (prefab != null) {
            var fx = EffectPool.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            EffectPool.Instance.ReturnToPool(fx, 0.8f);
        }
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

    bool IsValidTarget(GameObject obj) {
        return obj != gameObject; // 自分以外
    }
}