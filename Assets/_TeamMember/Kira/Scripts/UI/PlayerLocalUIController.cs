using Mirror;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Player内のLocalUI管理
/// </summary>
public class PlayerLocalUIController : NetworkBehaviour {

    enum TextIndex {
        Current = 0,
        Max,
        Partition,
        WeaponName,
    }

    [SerializeField] TextMeshProUGUI[] mainWeaponText;
    [SerializeField] Image mainWeaponReloadIcon;
    private bool reloadIconRotating = false;
    [SerializeField] TextMeshProUGUI[] subWeaponText;

    [SerializeField] Image[] skill_Icon;
    [SerializeField] Image skill_State;
    [SerializeField] Image[] passive_Icon;
    [SerializeField] Image passive_State;
    [SerializeField] TextMeshProUGUI passiveChains;
    [SerializeField] GeneralCharacter player;
    [SyncVar] float skillStateProgress = 0.0f;
    [SyncVar] float passiveStateProgress = 0.0f;

    public void Initialize() {
        if (!isLocalPlayer) {
            var LocalUI = GetComponentInChildren<Canvas>();
            LocalUI.gameObject.SetActive(false);
            return;
        }
        mainWeaponReloadIcon.enabled = false;
        LocalUIChanged();
    }

    void Update() {
        //スキルの表示状態管理
        if (player.isCanSkill) {
            skill_State.fillAmount = 1.0f;
            skill_State.color = Color.yellow;
        }
        else {
            skillStateProgress = player.skillAfterTime / player.equippedSkills[0].cooldown;
            skill_State.fillAmount = skillStateProgress;
            skill_State.color = Color.white;
        }
        //パッシブの表示状態管理
        if (player.equippedPassives[0].isPassiveActive || player.equippedPassives[0].passiveChains >= 1) {
            passiveChains.text = player.equippedPassives[0].passiveChains.ToString();
            passive_State.color = Color.yellow;
        }
        else {
            passive_State.color = Color.white;
        }

        //現在使用している武器タイプで分岐
        switch(player.weaponController_main.weaponData.type) {
            case WeaponType.Melee:
                mainWeaponText[(int)TextIndex.Current].text = "∞";
                break;
            case WeaponType.Gun:
                //メインウェポンの現在弾倉数を更新
                mainWeaponText[(int)TextIndex.Current].text = player.weaponController_main.ammo.ToString();
                break;
            case WeaponType.Magic:
                break;
        }        
        
        //サブウェポンの現在所持数を更新
        subWeaponText[(int)TextIndex.Current].text = player.weaponController_sub.currentUses.ToString();

        //パッシブの蓄積数が0だったら空欄にする
        if(player.equippedPassives[0].passiveChains == 0) passiveChains.text = "";
    }

    /// <summary>
    /// スキルとパッシブのアイコン、武器の情報の反映
    /// </summary>
    public void LocalUIChanged() {
        for (int i = 0; i < skill_Icon.Length; i++) {
            skill_Icon[i].sprite = player.equippedSkills[0].skillIcon;
        }
        for (int i = 0; i < passive_Icon.Length; i++) {
            passive_Icon[i].sprite = player.equippedPassives[0].passiveIcon;
        }

        mainWeaponText[(int)TextIndex.WeaponName].text = player.weaponController_main.weaponData.weaponName;
        //プレイヤーの弾倉が存在すればメインウェポンの弾倉UIを有効化する
        if (player.weaponController_main.weaponData.type == WeaponType.Gun) {
            mainWeaponText[(int)TextIndex.Partition].text = "/";
            mainWeaponText[(int)TextIndex.Current].text = player.weaponController_main.ammo.ToString();
            mainWeaponText[(int)TextIndex.Max].text = player.weaponController_main.weaponData.maxAmmo.ToString();            
        }
        else {
            mainWeaponText[(int)TextIndex.Partition].text = "";
            mainWeaponText[(int)TextIndex.Current].text = "";
            mainWeaponText[(int)TextIndex.Max].text = "";
        }
        //プレイヤーのサブウェポンUIを反映
        subWeaponText[(int)TextIndex.Current].text = player.weaponController_sub.currentUses.ToString();
        subWeaponText[(int)TextIndex.Max].text = player.weaponController_sub.subWeaponData.maxUses.ToString();
        subWeaponText[(int)TextIndex.WeaponName].text = player.weaponController_sub.subWeaponData.WeaponName;
    }

    /// <summary>
    /// hook関数で自動的に呼べるよう一度かませる関数
    /// player.isReloadingがtrueで自動発火
    /// </summary>
    public void StartRotateReloadIcon() {
        if(!reloadIconRotating)
            StartCoroutine(RotateReloadIcon(player.weaponController_main.weaponData.reloadTime));
    }


    /// <summary>
    /// リロードアイコンを1回転させる ( float _duration = 1回転するまでにかかる時間 )
    /// </summary>
    public IEnumerator RotateReloadIcon(float _duration) {
        reloadIconRotating = true;
        mainWeaponReloadIcon.enabled = true;
        float start = 0f;
        float end = -360f;
        float time = 0f;

        while (time < _duration) {
            float t = time / _duration;
            float angle = Mathf.Lerp(start, end, t);
            mainWeaponReloadIcon.transform.localRotation = Quaternion.Euler(0, 0, angle);
            time += Time.deltaTime;
            yield return null;
        }

        // 最後に角度をリセットしてアイコンを非表示にする
        mainWeaponReloadIcon.transform.localRotation = Quaternion.Euler(0, 0, 0);
        reloadIconRotating = false;
        mainWeaponReloadIcon.enabled = false;
    }
}
