using UnityEngine;
using Mirror;
using System.Collections;
using static UnityEngine.UI.GridLayoutGroup;

/// <summary>
/// メイン武器コントローラー
/// </summary>
public class EnemyWeaponController : NetworkBehaviour {
    public WeaponData weaponData;           // メイン武器
    public Transform firePoint;

    private GameObject activeChargeFx;

    private EnemyStatusBase enemyBase;

    private void Awake() {
        enemyBase = GetComponent<EnemyStatusBase>();
    }

    // --- 攻撃リクエスト ---
    [Command]
    public void CmdRequestAttack(Vector3 direction) {
        if(weaponData.type == WeaponType.Enemy) {
            if (weaponData is MeleeData meleeData)
                StartCoroutine(ServerMeleeCombo(meleeData.combo, meleeData.comboDelay));
            else if(weaponData is MainMagicData magicData)
                ServerStartMagicCast(direction);
        }
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
            if (hp == null || !IsValidTarget(hp.gameObject) || hp.parameter.TeamID == -1) continue;
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
    void ServerMagicAttack(Vector3 direction) {
        if (weaponData is not MainMagicData magicData || magicData.projectilePrefab == null)
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
                "Boss",
                -1,
                magicData.magicType,
                magicData.hitEffectType,
                magicData.projectileSpeed,
                magicData.initialHeightSpeed,
                magicData.damage,
                direction
            );
        }
        else if (proj.TryGetComponent(out DoTArea dotArea)) {
            int teamID = -1;
            Vector3 Direction = transform.forward;
            dotArea.Init(
                teamID,
                "Boss",
                -1,
                magicData.projectileSpeed,
                magicData.damage,
                Direction
                );
        }
    }

    /// <summary>
    /// 詠唱開始
    /// </summary>
    /// <param name="direction"></param>
    [Server]
    public void ServerStartMagicCast(Vector3 direction) {
        if (weaponData is not MainMagicData magicData) return;

        //クライアント側にチャージエフェクトを出させる
        if (magicData.chargeTime > 0)
            RpcPlayChargeEffect(firePoint.position, magicData.chargeEffectType);
        StartCoroutine(CastAfterDelay(direction, magicData));
    }

    [Server]
    private IEnumerator CastAfterDelay(Vector3 direction, MainMagicData magicData) {
        yield return new WaitForSeconds(magicData.chargeTime);

        // 発射エフェクト (チャージ停止＆マズルフラッシュ)
        RpcCastMagic(firePoint.position, magicData.muzzleFlashType);

        // 弾の生成
        ServerMagicAttack(direction);

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

    /// <summary>
    /// 武器毎のレイヤーのインデックスを返す
    /// </summary>
    /// <param name="_weaponName"></param>
    /// <returns></returns>
    public int GenerateWeaponIndex(string _weaponName) {
        return _weaponName switch {
            "HandGun" or "revolver" or "Punch" => 1,
            "Assult" or "BurstAssult" or "FireMagic" or "IceMagic" or "MagicRain" or "Spear" or "IceMagic" or "Katana" or "Lightsaver"
             or "Knife" or "PizzaCutter" => 2,
            "RPG" => 3,
            "Sniper" => 4,
            "Minigun" => 5,

            _ => -1,
        };
    }
}