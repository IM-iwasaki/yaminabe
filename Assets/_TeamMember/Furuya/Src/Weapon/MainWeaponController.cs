using UnityEngine;
using Mirror;
using Mirror.BouncyCastle.Asn1.Pkcs;
using System.Collections;

/// <summary>
/// メイン武器コントローラー
/// </summary>
public class MainWeaponController : NetworkBehaviour {
    [SyncVar(hook = (nameof(ChangeWeapon)))] public WeaponData weaponData;           // メイン武器
    public Transform firePoint;
    private float lastAttackTime;
    [SyncVar, System.NonSerialized] public int ammo;
    
    private CharacterEnum.CharaterType charaterType;

    private CharacterBase characterBase; // 名前を取得するため
    private PlayerLocalUIController playerUI;

    //弾薬やMPを消費するか判別する列挙体
    public enum ConsumptionType {
        Consume = 0,
        NotConsumed,
    }

    public void Awake() {
        characterBase = GetComponent<CharacterBase>();
        playerUI = characterBase.GetPlayerLocalUI();
        // 追加：キラ   弾薬数を最大にする。
        weaponData.AmmoReset();
        ammo = weaponData.maxAmmo;
    }

    public void SetCharacterType(CharacterEnum.CharaterType type) {
        charaterType = type;
    }

    /// <summary>
    /// 武器チェンジ後処理
    /// </summary>
    /// <param name="_"></param>
    /// <param name="_new"></param>
    private void ChangeWeapon(WeaponData _, WeaponData _new) {
        _new.AmmoReset();
        playerUI.LocalUIChanged();
    }

    // --- 攻撃リクエスト ---
    [Command]
    public void CmdRequestAttack(Vector3 direction) {
        if (!CanAttack()) return;
        lastAttackTime = Time.time;

        switch (weaponData.type) {
            case WeaponType.Melee:
                if (weaponData is MeleeData meleeData)
                    ServerMeleeCombo(meleeData.combo, meleeData.comboDelay);
                break;
            case WeaponType.Gun:
                //弾がなかったら通過不可。かわりにリロードを要求する。
                if (ammo == 0) {
                    ReloadRequest();
                    return;
                } 
                //その他リロード中は射撃できなくする。
                else if (characterBase.isReloading) return;

                if (weaponData is GunData gunData)
                    StartCoroutine(ServerBurstShoot(direction, gunData.multiShot, gunData.burstDelay));
                break;
            case WeaponType.Magic:
                ServerMagicAttack(direction);
                break;
        }
    }

    /// <summary>
    /// 追加攻撃用(こちらは攻撃間隔を無視して攻撃を呼び出せます)
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
                //弾がなかったら通過不可。かわりにリロードを要求する。
                if (ammo == 0) {
                    ReloadRequest();
                    return;
                } 
                //その他リロード中は射撃できなくする。
                else if (characterBase.isReloading) return;

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
        ammo = weaponData.ammo;
        playerUI.LocalUIChanged();
        Debug.LogWarning($"'{data.weaponName}' を使用します");
    }

    /// <summary>
    /// 武器の使用可否判定
    /// </summary>
    /// <param name="character"></param>
    /// <param name="weapon"></param>
    /// <returns></returns>
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
                AudioManager.Instance.CmdPlayWorldSE(meleeData.se.ToString(), transform.position);
            }
        }
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

        //  追加：キラ  弾薬が必要な場合、弾薬が残っていれば銃の弾薬を消費して通過
        if (ammo > 0) ammo--;
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

        //マズルフラッシュ、SE再生
        RpcPlayMuzzleFlash(firePoint.position, gunData.muzzleFlashType);
        AudioManager.Instance.CmdPlayWorldSE(gunData.se.ToString(), transform.position);
    }

    // --- 魔法攻撃 ---
    void ServerMagicAttack(Vector3 direction) {
        if (weaponData is not MainMagicData magicData || magicData.projectilePrefab == null)
            return;

        //TODO:ここにMPの消費処理を書く。

        GameObject proj = ProjectilePool.Instance.SpawnFromPool(
            magicData.projectilePrefab.name,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

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

        //マズルフラッシュ、SE再生
        RpcPlayMuzzleFlash(firePoint.position, magicData.muzzleFlashType);
        AudioManager.Instance.CmdPlayWorldSE(magicData.se.ToString(), transform.position);
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
    /// リロード入力専用Cmd
    /// </summary>
    [Command]
    public void CmdReloadRequest() {
        ReloadRequest();
    }

    /// <summary>
    /// リロードの要求関数(リロード中だったら弾く)
    /// </summary>
    [Server]
    public void ReloadRequest() {
        //射撃中やリロード中ならやめる
        if (characterBase.isAttackPressed && characterBase.isReloading) return;
        //使っている武器が銃でなければやめる
        if (weaponData.type != WeaponType.Gun) return;

        //リロード中にする
        characterBase.isReloading = true;
        //リロードを行う
        Invoke(nameof(Reload), weaponData.reloadTime);
    }
    /// <summary>
    /// リロードの本実行
    /// </summary>
    [Server]
    void Reload() {
        ammo = weaponData.maxAmmo;
        characterBase.isReloading = false;
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
