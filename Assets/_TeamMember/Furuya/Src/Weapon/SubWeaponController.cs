using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// サブ武器操作用
/// </summary>
public class SubWeaponController : NetworkBehaviour {
    [Header("Sub Weapon")]
    public SubWeaponData subWeaponData;

    public int currentUses { get; private set; }
    private bool isRecharging;

    private CharacterBase characterBase; // チームIDなどを取得するため

    void Start() {
        characterBase = GetComponent<CharacterBase>();

        if (subWeaponData != null)
            currentUses = subWeaponData.startFull ? subWeaponData.maxUses : 0;
    }

    /// <summary>
    /// サブ武器の使用可否判定
    /// </summary>
    public void TryUseSubWeapon() {
        if (subWeaponData == null || currentUses <= 0 || !characterBase.isLocalPlayer) return;
        CmdUseSubWeapon();
    }
    /// <summary>
    /// サブ武器使用
    /// </summary>
    [Command]
    private void CmdUseSubWeapon() {
        if (subWeaponData == null || currentUses <= 0 || !isServer) return;

        currentUses--;

        // サブ武器の種類ごとの処理
        switch (subWeaponData.type) {
            case SubWeaponType.Grenade:
                SpawnGrenade();
                break;
            case SubWeaponType.Trap:
                SpawnTrap();
                break;
            case SubWeaponType.Item:
                // アイテム処理（未実装）
                break;
            case SubWeaponType.Magic:
                // 魔法処理（未実装）
                break;
        }

        if (!isRecharging)
            StartCoroutine(RechargeRoutine());
    }

    /// <summary>
    /// グレ生成
    /// </summary>
    [Server]
    private void SpawnGrenade() {
        if (subWeaponData.ObjectPrefab == null) return;

        GameObject grenadeObj = ProjectilePool.Instance.SpawnFromPool(
            subWeaponData.ObjectPrefab.name,
            transform.position + transform.forward + Vector3.up,
            Quaternion.identity
        );

        int teamID = characterBase?.TeamID ?? 0;
        Vector3 throwDirection = transform.forward;

        // SmokeGrenade の場合
        if (subWeaponData is SmokeData smokeData && grenadeObj.TryGetComponent(out SmokeGrenade smokeGrenade)) {
            smokeGrenade.Init(smokeData, teamID, characterBase.PlayerName, throwDirection);
        }
        // 通常の Grenade の場合
        else if (subWeaponData is GrenadeData grenadeData && grenadeObj.TryGetComponent(out GrenadeBase grenade)) {
            grenade.Init(
                teamID,
                characterBase.PlayerName,
                throwDirection,
                grenadeData.throwForce,
                grenadeData.projectileSpeed,
                grenadeData.explosionRadius,
                grenadeData.damage,
                grenadeData.canDamageAllies,
                grenadeData.useEffectType,
                grenadeData.explosionDelay
            );
        }
        //アニメーション発火
        ThrowAnimation();
    }

    /// <summary>
    /// トラップ生成
    /// </summary>
    [Server]
    private void SpawnTrap() {
        if (subWeaponData.ObjectPrefab == null) return;

        Vector3 origin = transform.position + Vector3.up * 0.5f; // 少し上からRayを飛ばす
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2f, LayerMask.GetMask("Ground"))) {
            Vector3 spawnPos = hit.point;

            GameObject trapObj = ProjectilePool.Instance.SpawnFromPool(
                subWeaponData.ObjectPrefab.name,
                spawnPos,
                Quaternion.identity
            );

            if (trapObj.TryGetComponent(out LandMine mine)) {
                int teamID = characterBase?.TeamID ?? 0;
                LandMineData landMineData = subWeaponData as LandMineData;
                if (landMineData == null) return;

                TrapInitData trapInit = new TrapInitData {
                    teamID = teamID,
                    activationDelay = landMineData.activationDelay,
                    activationOnce = landMineData.activationOnce,
                    activationEffect = landMineData.activationEffect,
                    duration = landMineData.duration
                };

                mine.Init(
                    trapInit,
                    landMineData.explosionRadius,
                    landMineData.damage,
                    landMineData.canDamageAllies,
                    landMineData.useEffectType
                );
            }
        }
    }

    /// <summary>
    /// リチャージ
    /// </summary>
    /// <returns></returns>
    private IEnumerator RechargeRoutine() {
        isRecharging = true;
        while (currentUses < subWeaponData.maxUses) {
            yield return new WaitForSeconds(subWeaponData.rechargeTime);
            currentUses++;
        }
        isRecharging = false;
    }

    [ClientRpc]
    private void ThrowAnimation() {
        characterBase.anim.SetTrigger("Throw");
    }
}
