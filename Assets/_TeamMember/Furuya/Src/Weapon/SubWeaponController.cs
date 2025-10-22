using UnityEngine;
using Mirror;
using System.Collections;

public class SubWeaponController : NetworkBehaviour {
    [Header("Sub Weapon")]
    public SubWeaponData currentSubWeapon;

    private int currentUses;
    private bool isRecharging;

    private CharacterBase characterBase; // チームIDなどを取得するため

    void Start() {
        characterBase = GetComponent<CharacterBase>();

        if (currentSubWeapon != null)
            currentUses = currentSubWeapon.startFull ? currentSubWeapon.maxUses : 0;
    }

    void Update() {
        if (!isLocalPlayer) return; // ローカルプレイヤーのみ入力を処理
        if (Input.GetKeyDown(KeyCode.G)) {
            TryUseSubWeapon();
        }
    }

    public void TryUseSubWeapon() {
        if (currentSubWeapon == null || currentUses <= 0) return;
        CmdUseSubWeapon();
    }

    [Command]
    private void CmdUseSubWeapon() {
        if (currentSubWeapon == null || currentUses <= 0 || !isServer) return;

        currentUses--;

        // サブ武器の種類ごとの処理
        switch (currentSubWeapon.type) {
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
        if (currentSubWeapon.ObjectPrefab == null) return;

        GameObject grenadeObj = ProjectilePool.Instance.SpawnFromPool(
            currentSubWeapon.ObjectPrefab.name,
            transform.position + transform.forward + Vector3.up,
            Quaternion.identity
        );

        if (grenadeObj.TryGetComponent(out GrenadeBase grenade)) {
            int teamID = GetComponent<CharacterBase>()?.TeamID ?? 0;
            grenade.Init(currentSubWeapon, teamID, transform.forward);
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
        while (currentUses < currentSubWeapon.maxUses) {
            yield return new WaitForSeconds(currentSubWeapon.rechargeTime);
            currentUses++;
        }
        isRecharging = false;
    }
}
