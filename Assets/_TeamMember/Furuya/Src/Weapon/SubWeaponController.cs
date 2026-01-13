using UnityEngine;
using Mirror;
using System.Collections;
using Mirror.Examples.Basic;
using static CharacterEnum;
using Mirror.Examples.Benchmark;

/// <summary>
/// サブ武器操作用
/// </summary>
public class SubWeaponController : NetworkBehaviour {
    [Header("Sub Weapon")]
    public SubWeaponData subWeaponData;

    public int currentUses { get; private set; }
    private bool isRecharging;

    private CharacterBase characterBase; // チームIDなどを取得するため
    private CharacterAnimationController characterAnimationController;
    private PlayerLocalUIController playerUI;

    public void Awake() {
        characterBase = GetComponent<CharacterBase>();
        characterAnimationController = GetComponent<CharacterAnimationController>();
        playerUI = characterBase.GetPlayerLocalUI();

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
                UseItem();
                break;
            case SubWeaponType.Magic:
                // 魔法処理（未実装）
                break;
        }

        if (!isRecharging)
            StartCoroutine(RechargeRoutine());
    }

    /// <summary>
    /// 武器データセット
    /// </summary>
    /// <param name="name"></param>
    //[Command]
    public void SetWeaponData(string name) {
        var data = WeaponDataRegistry.GetSubWeapon(name);

        subWeaponData = data;
        playerUI.LocalUIChanged();
        Debug.LogWarning($"'{data.subWeaponName}' を使用します");
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

        int teamID = characterBase?.parameter.TeamID ?? 0;
        Vector3 throwDirection = transform.forward;

        // SmokeGrenade の場合
        if (subWeaponData is SmokeData smokeData && grenadeObj.TryGetComponent(out SmokeGrenade smokeGrenade)) {
            smokeGrenade.Init(smokeData, teamID, characterBase.parameter.PlayerName, throwDirection);
        }
        // 通常の Grenade の場合
        else if (subWeaponData is GrenadeData grenadeData && grenadeObj.TryGetComponent(out GrenadeBase grenade)) {
            grenade.Init(
                teamID,
                characterBase.parameter.PlayerName,
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
                int teamID = characterBase?.parameter.TeamID ?? 0;
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

    [Server]
    private void UseItem() {
        if (subWeaponData is not ItemData itemData) return;

        switch (itemData.itemType) {

            case ItemType.HealthPack: {
                if (itemData is HealthPackData hpData )
                    characterBase.Heal(hpData.healAmount, 1);
                break;
            }

            case ItemType.Shield: {
                if (itemData is ShieldData shieldData) {
                    SpawnShieldBarricade(shieldData);
                }
                break;
            }

            case ItemType.SpeedBoost: {
                if (itemData is SpeedBoostData speedData) {
                    characterBase.MoveSpeedBuff(
                        speedData.speedMultiplier,
                        speedData.duration
                    );
                }
                break;
            }
        }
    }

    /// <summary>
    /// バリケード生成
    /// </summary>
    /// <param name="data"></param>
    [Server]
    private void SpawnShieldBarricade(ShieldData data) {
        if (data.barricadePrefab == null) return;

        Vector3 spawnPos =
            transform.position +
            transform.forward * data.distanceFromPlayer;

        Quaternion rot = Quaternion.LookRotation(transform.forward);

        GameObject obj = Instantiate(
            data.barricadePrefab,
            spawnPos,
            rot
        );

        NetworkServer.Spawn(obj);

        // 一定時間後に消す
        StartCoroutine(DestroyAfterTime(obj, data.duration));
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

    private IEnumerator DestroyAfterTime(GameObject obj, float time) {
        yield return new WaitForSeconds(time);
        if (obj != null)
            NetworkServer.Destroy(obj);
    }

    [ClientRpc]
    private void ThrowAnimation() {
        characterAnimationController.anim.SetTrigger("Throw");
    }
}
