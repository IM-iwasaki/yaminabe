using UnityEngine;
using Mirror;
using System.Collections;

public class SubWeaponController : NetworkBehaviour {
    [Header("Sub Weapon")]
    public SubWeaponData subWeaponData;

    private int currentUses;
    private bool isRecharging;

    private CharacterBase characterBase; // チームIDなどを取得するため

    void Start() {
        characterBase = GetComponent<CharacterBase>();

        if (subWeaponData != null)
            currentUses = subWeaponData.startFull ? subWeaponData.maxUses : 0;
    }

    void Update() {
        if (!isLocalPlayer) return; // ローカルプレイヤーのみ入力を処理
        if (Input.GetKeyDown(KeyCode.G)) {
            TryUseSubWeapon();
        }
    }

    public void TryUseSubWeapon() {
        if (subWeaponData == null || currentUses <= 0) return;
        CmdUseSubWeapon();
    }

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
                // Trap用処理（未実装）
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

    [Server]
    private void SpawnGrenade() {
        if (subWeaponData.ObjectPrefab == null) return;

        GameObject grenadeObj = ProjectilePool.Instance.SpawnFromPool(
            subWeaponData.ObjectPrefab.name,
            transform.position + transform.forward + Vector3.up,
            Quaternion.identity
        );

        if (grenadeObj.TryGetComponent(out GrenadeBase grenade)) {
            int teamID = GetComponent<CharacterBase>()?.TeamID ?? 0;
            grenade.Init(subWeaponData, subWeaponData.useEffectType, teamID, transform.forward);
        }
    }


    [Server]
    private IEnumerator ServerFuse(GrenadeBase grenade, float delay) {
        yield return new WaitForSeconds(delay);
        // GrenadeBase内の爆発を呼び出し
        var explodeMethod = grenade.GetType().GetMethod("Explode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        explodeMethod?.Invoke(grenade, null);
    }

    private IEnumerator RechargeRoutine() {
        isRecharging = true;
        while (currentUses < subWeaponData.maxUses) {
            yield return new WaitForSeconds(subWeaponData.rechargeTime);
            currentUses++;
        }
        isRecharging = false;
    }
}
