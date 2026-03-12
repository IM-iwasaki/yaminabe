using UnityEngine;
using Mirror;
using System.Collections;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// メイン武器コントローラー
/// </summary>
public class MainWeaponController : NetworkBehaviour {
    [SyncVar(hook = nameof(OnWeaponIDChanged))] public int weaponID;
    private int appearanceID;
    public WeaponData weaponData;           // メイン武器
    public Transform firePoint;
    private float lastAttackTime;
    [SyncVar, System.NonSerialized] public int ammo;
    private int currentMoney;

    private GameObject activeChargeFx;

    public CharacterEnum.CharaterType charaterType { get; private set; }

    private CharacterBase characterBase; // 名前を取得するため
    private CharacterAnimationController animCon;
    private PlayerLocalUIController playerUI;

    private void Awake() {
        characterBase = GetComponent<CharacterBase>();
        animCon = GetComponent<CharacterAnimationController>();
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        playerUI = characterBase.GetPlayerLocalUI();
    }

    [Server]
    public void RequestAmmoReset() {
        if (weaponData.type == WeaponType.Gun) {
            weaponData.AmmoReset();
            ammo = weaponData.maxAmmo;

            // クライアントのUI更新は ClientRpc で呼べよ by マツオ 
            RpcUpdateAmmoUI();
        }
    }

    [ClientRpc]
    void RpcUpdateAmmoUI() {
        if (!isLocalPlayer) return; // 自分のUIだけ更新
        playerUI?.LocalUIChanged();
    }

    public void SetCharacterType(CharacterEnum.CharaterType type) {
        charaterType = type;
    }

    public void ServerResetMainWeapon(GeneralCharacterStatus _status) {
        weaponData = _status.MainWeapon;
        CmdSetWeaponData(weaponData.WeaponID);
    }

    /// <summary>
    /// 武器チェンジ後処理
    /// </summary>
    /// <param name="_"></param>
    /// <param name="_new"></param>
    private void ChangeWeapon(WeaponData _new) {
        if (_new == null) return;

        _new.AmmoReset();

        if (!isLocalPlayer) return;
        if (playerUI == null) return;

        playerUI.LocalUIChanged();
    }

    // --- 攻撃リクエスト ---
    [Command]
    public void CmdRequestAttack(Vector3 direction) {
        switch (weaponData.type) {
            case WeaponType.Melee:
                if (weaponData is MeleeData meleeData)
                    StartCoroutine(ServerMeleeCombo(meleeData.combo, meleeData.comboDelay));
                break;
            case WeaponType.Gun:
                if (weaponData is GunData gunData) {
                    //弾がなかったら通過不可。かわりにリロードを要求する。
                    if (ammo == 0) {
                        ReloadRequest();
                        return;
                    }
                    //その他リロード中は射撃できなくする。
                    else if (characterBase.parameter.isReloading) return;

                    StartCoroutine(ServerBurstShoot(direction, gunData.multiShot, gunData.burstDelay));
                    if (ammo > 0)
                        ammo -= gunData.multiShot;


                }
                break;
            case WeaponType.MoneyGun:
                if (weaponData is GunData moneyGunData)
                    StartCoroutine(ServerBurstShoot(direction, moneyGunData.multiShot, moneyGunData.burstDelay));
                break;

            case WeaponType.Magic:
                if (weaponData is MainMagicData magicdata)
                    ServerStartMagicCast(direction);
                //ServerMagicAttack(direction);
                break;
        }
        //アニメーション開始
        RpcPlayShootAnimation();

        characterBase.parameter.RpcTriggerAttack();
    }

    /// <summary>
    /// 追加　マツオ : MoneyGun用攻撃時のお金消費処理(ローカル)
    /// </summary>
    /// <param name="direction"></param>
    public void AttemptAttack(Vector3 direction) {
        if (!isLocalPlayer) return;
        if (!CanAttack()) return;

        lastAttackTime = Time.time;

        // MoneyGunの場合、LobbyScene以外でお金を消費
        if (weaponData.type == WeaponType.MoneyGun) {
            string currentSceneName = SceneManager.GetActiveScene().name;

            // LobbySceneではお金を消費しない
            if (currentSceneName != "LobbyScene") {
                if (!PlayerWallet.Instance.SpendMoney(weaponData.cost))
                    return; // お金不足で撃てない

            }
            ammo = PlayerWallet.Instance.currentMoney; // UI更新用
        }

        // サーバーに弾撃ちをリクエスト
        CmdRequestAttack(direction);
    }
    [ClientRpc]
    private void RpcPlayShootAnimation() {
        if (animCon == null || animCon.anim == null) return;
        animCon.anim.SetBool("Shoot", true);
    }

    /// <summary>
    /// 追加攻撃用(こちらは攻撃間隔を無視して攻撃を呼び出せます)
    /// </summary>
    /// <param name="direction"></param>
    [Command]
    public void CmdRequestExtraAttack(Vector3 direction) {
        switch (weaponData.type) {
            case WeaponType.Melee:
                if (weaponData is MeleeData meleeData)
                    StartCoroutine(ServerMeleeCombo(meleeData.combo, meleeData.comboDelay));
                break;
            case WeaponType.Gun:
                //弾がなかったら通過不可。かわりにリロードを要求する。
                if (ammo == 0) {
                    ReloadRequest();
                    return;
                }
                //その他リロード中は射撃できなくする。
                else if (characterBase.parameter.isReloading) return;

                if (weaponData is GunData gunData) {
                    StartCoroutine(ServerBurstShoot(direction, gunData.multiShot, gunData.burstDelay));
                }

                break;
            case WeaponType.Magic:
                if (weaponData is MainMagicData magicdata)
                    ServerStartMagicCast(direction);
                break;
        }
        //アニメーション開始
        RpcPlayShootAnimation();
        //フレーム中攻撃した瞬間にイベントを送信
        characterBase.parameter.RpcTriggerAttack();
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
    public void CmdSetWeaponData(int _weaponID) {

        var data = WeaponDataRegistry.GetWeapon(_weaponID);

        // サーバーで SyncVar を更新
        appearanceID = data.appearanceID;
        weaponID = data.WeaponID;
        weaponData = data;
        ChangeWeapon(weaponData);
        if (weaponData.type == WeaponType.MoneyGun)
            ammo = currentMoney;
        else
            ammo = weaponData.ammo;

        // 見た目・状態は全クライアントで Hook / Rpc で反映される
        characterBase.GetComponent<GeneralCharacter>().RpcChangeWeapon(weaponData.appearanceID);
        //見た目変更
        animCon.SetWeaponLayer(GenerateWeaponIndex(weaponData.weaponName));

        Debug.LogWarning($"'{data.weaponName}' を使用します");
    }

    private void OnWeaponIDChanged(int oldID, int newID) {
        weaponData = WeaponDataRegistry.GetWeapon(newID);

        if (weaponData == null)
            return;

        ammo = weaponData.ammo;

        if (isLocalPlayer)
            playerUI?.LocalUIChanged();
    }

    /// <summary>
    /// 武器の使用可否判定
    /// </summary>
    /// <param name="character"></param>
    /// <param name="weapon"></param>
    /// <returns></returns>
    public bool CanUseWeapon(CharacterEnum.CharaterType character, WeaponType weapon) {
        return character switch {
            CharacterEnum.CharaterType.Melee => weapon == WeaponType.Melee,
            CharacterEnum.CharaterType.Gunner => weapon == WeaponType.Gun
                                              || weapon == WeaponType.MoneyGun,
            CharacterEnum.CharaterType.Wizard => weapon == WeaponType.Magic,
            _ => false
        };
    }

    // 変更　マツオ : 近接攻撃判定
    private void ServerMeleeAttack() {
        if (weaponData is not MeleeData meleeData)
            return;

        int attackLayer = LayerMask.GetMask("Character");
        int wallLayer = LayerMask.GetMask("Ground");

        Vector3 origin = firePoint.position;
        Vector3 forward = firePoint.forward;

        // 攻撃判定を前にずらす
        Vector3 center = origin + forward * (meleeData.range * 0.5f);

        Collider[] hits = Physics.OverlapSphere(center, meleeData.range, attackLayer);

        HashSet<GameObject> damagedTargets = new();

        foreach (var c in hits) {
            if (c.gameObject == gameObject) continue;
            if (damagedTargets.Contains(c.gameObject)) continue;

            var hp = c.GetComponent<CreatureBase>();
            if (hp == null) continue;
            if (hp.teamID == characterBase.teamID) continue;

            // 敵の一番近い位置を取得
            Vector3 closest = c.ClosestPoint(origin);
            Vector3 diff = closest - origin;

            float dist = diff.magnitude;

            // 密着距離なら角度無視
            if (dist > 0.3f) {
                Vector3 dir = diff.normalized;

                float dot = Vector3.Dot(forward, dir);
                float threshold = Mathf.Cos(meleeData.meleeAngle * Mathf.Deg2Rad);

                if (dot < threshold) continue;
            }

            // 壁越し防止
            if (Physics.Raycast(origin, diff.normalized, out RaycastHit hit, dist, wallLayer)) {
                if (hit.collider != c)
                    continue;
            }

            damagedTargets.Add(c.gameObject);

            hp.TakeDamage(
                meleeData.damage,
                characterBase.parameter.PlayerName,
                characterBase.parameter.playerId
            );

            RpcSpawnHitEffect(closest, meleeData.hitEffectType);
        }

        AudioManager.Instance.CmdPlayWorldSE(meleeData.se.ToString(), transform.position);

#if UNITY_EDITOR
        MeleeAttackDebugArc.Create(origin, forward, meleeData.range, meleeData.meleeAngle, Color.yellow, 0.5f);
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
                characterBase.parameter.PlayerName,
                characterBase.parameter.playerId,
                gunData.hitEffectType,
                gunData.projectileSpeed,
                gunData.damage
            );
        }
        else if (proj.TryGetComponent(out ExplosionProjectile ExpProjScript)) {
            ExpProjScript.Init(
                gameObject,
                characterBase.parameter.PlayerName,
                characterBase.parameter.playerId,
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

        //MPが不足していたら帰る
        int MPCost = characterBase.GetCurrentMPCost(magicData);
        characterBase.parameter.MP -= MPCost;


        GameObject proj;

        if (magicData.magicType == ProjectileType.DoT) {
            Vector3 spawnPos = transform.position;
            Quaternion rot = Quaternion.identity;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f)) {
                spawnPos = hit.point;
                rot = Quaternion.LookRotation(transform.forward, hit.normal);
            }

            proj = ProjectilePool.Instance.SpawnFromPool(
                magicData.projectilePrefab.name,
                spawnPos,
                rot
            );

        }
        else {
            proj = ProjectilePool.Instance.SpawnFromPool(
            magicData.projectilePrefab.name,
            firePoint.position,
            Quaternion.LookRotation(direction)
            );
        }

        if (proj == null) return;

        if (proj.TryGetComponent(out MagicProjectile projScript)) {
            projScript.Init(
                gameObject,
                characterBase.parameter.PlayerName,
                characterBase.parameter.playerId,
                magicData.magicType,
                magicData.hitEffectType,
                magicData.projectileSpeed,
                magicData.initialHeightSpeed,
                magicData.damage,
                direction
            );
        }
        else if (proj.TryGetComponent(out DoTArea dotArea)) {
            int teamID = characterBase?.parameter.TeamID ?? 0;
            dotArea.Init(
                teamID,
                characterBase.parameter.PlayerName,
                characterBase.parameter.playerId,
                magicData.hitEffectType,
                magicData.projectileSpeed,
                magicData.damage
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

        //MPが不足していたら帰る
        if (characterBase.parameter.MP < magicData.MPCost) return;

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
        if (characterBase.input.AttackPressed && characterBase.parameter.isReloading) return;
        //使っている武器が銃でなければやめる
        if (weaponData.type != WeaponType.Gun) return;

        //リロード中にする
        characterBase.parameter.isReloading = true;
        //リロードを行う
        Invoke(nameof(Reload), weaponData.reloadTime);
    }
    /// <summary>
    /// リロードの本実行
    /// </summary>
    [Server]
    void Reload() {
        ammo = weaponData.maxAmmo;
        characterBase.parameter.isReloading = false;
    }

    /// <summary>
    /// 武器毎のレイヤーのインデックスを返す
    /// </summary>
    /// <param name="_weaponName"></param>
    /// <returns></returns>
    public int GenerateWeaponIndex(string _weaponName) {
        return _weaponName switch {
            "HandGun" or "revolver" or "Punch" => 1,
            "Assult" or "BurstAssult" or "FireMagic" or "IceMagic" or "MagicRain" or "Spear" or "MagicRain" or "DarkTornado"
            or "FlameMagic" or "CrystalAttack"  => 2,
            "RPG" or "Katana" => 3,
            "Sniper" or "Knife" or "PizzaCutter" => 4,
            "Minigun" or "Lightsaver" => 5,

            _ => -1,
        };
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
