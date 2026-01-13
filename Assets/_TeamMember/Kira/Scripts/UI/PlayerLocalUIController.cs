using Mirror;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Player内のLocalUI管理
/// </summary>
public class PlayerLocalUIController : NetworkBehaviour {

    /// <summary>
    /// 可読性向上のためのTextの配列の列挙体
    /// </summary>
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
    //表示・非表示状態切り替え用
    [SerializeField] GameObject mpBar;

    /// <summary>
    /// バー補正用定数
    /// </summary>
    private const int FIXED_RATIO = 100;
    [SerializeField]
    private TextMeshProUGUI hpText = null;
    [SerializeField]
    private TextMeshProUGUI mpText = null;
    [SerializeField]
    private Slider hpBar_slider = null;
    [SerializeField]
    private Slider mpBar_slider = null;
    [SerializeField]
    private Image hpBarImage = null;
    [SerializeField]
    private Image mpBarImage = null;

    [SerializeField] Image[] skill_Icon;
    [SerializeField] Image skill_State;
    [SerializeField] Image[] passive_Icon;
    [SerializeField] Image passive_State;
    [SerializeField] TextMeshProUGUI passiveChains;
    [SerializeField] GeneralCharacter player;
    [SyncVar] float skillStateProgress = 0.0f;
    //[SyncVar] float passiveStateProgress = 0.0f;

    [SerializeField] GameObject interactUI;

    //  ローカルUIの本体を取得
    [SerializeField] GameObject localUIObject;

    /// <summary>
    /// ローカルUI全体の初期化
    /// </summary>
    public void Initialize() {
        hpBar_slider.interactable = false;
        mpBar_slider.interactable = false;

        if (!isLocalPlayer) {
            var LocalUI = GetComponentInChildren<Canvas>();
            LocalUI.gameObject.SetActive(false);
            return;
        }
        mainWeaponReloadIcon.enabled = false;
        interactUI.SetActive(false);
        LocalUIChanged();
    }

    void Update() {
        //スキルの表示状態管理
        if (player.action.isCanSkill) {
            skill_State.fillAmount = 1.0f;
            skill_State.color = Color.yellow;
        }
        else {
            skillStateProgress = player.parameter.skillAfterTime / player.parameter.equippedSkills[0].cooldown;
            skill_State.fillAmount = skillStateProgress;
            skill_State.color = Color.white;
        }
        //パッシブの表示状態管理
        if (player.parameter.equippedPassives[0].isPassiveActive || player.parameter.equippedPassives[0].passiveChains >= 1) {
            passiveChains.text = player.parameter.equippedPassives[0].passiveChains.ToString();
            passive_State.color = Color.yellow;
        }
        else {
            passive_State.color = Color.white;
        }

        //現在使用している武器タイプで分岐
        switch (player.weaponController_main.weaponData.type) {
            case WeaponType.Melee:
                mainWeaponText[(int)TextIndex.Current].text = "∞";
                break;
            case WeaponType.Gun:
                //メインウェポンの現在弾倉数を更新
                mainWeaponText[(int)TextIndex.Current].text = player.weaponController_main.ammo.ToString();
                break;
            case WeaponType.Magic:
                //所持している武器が魔法であるか確認。
                if (player.weaponController_main.weaponData is not MainMagicData magicData) {
                    #if UNITY_EDITOR
                    Debug.LogError("所持している魔法の詳細情報を正常に取得できませんでした。");
                    #endif
                    return;
                }
                mainWeaponText[(int)TextIndex.Partition].text = "Cost : " + magicData.MPCost.ToString();
                break;
        }
        mpText.text = player.parameter.MP.ToString();

        //サブウェポンの現在所持数を更新
        subWeaponText[(int)TextIndex.Current].text = player.weaponController_sub.currentUses.ToString();

        //パッシブの蓄積数が0だったら空欄にする
        if (player.parameter.equippedPassives[0].passiveChains == 0) passiveChains.text = "";
    }

    /// <summary>
    /// スキルとパッシブのアイコン、武器の情報の反映
    /// </summary>
    public void LocalUIChanged() {
        //MPを必要とする職業かでMPの表示非表示を分ける
        if (player.weaponController_main.weaponData.type == WeaponType.Magic) {
            mpBar.SetActive(true);
        }
        else {
            mpBar.SetActive(false);
        }

        //ステータス系の初期化
        hpText.text = player.parameter.HP.ToString();
        ChangeHPUI(player.parameter.maxHP, player.parameter.HP);
        mpText.text = player.parameter.MP.ToString();
        ChangeMPUI(player.parameter.maxMP, player.parameter.MP);


        for (int i = 0; i < skill_Icon.Length; i++) {
            skill_Icon[i].sprite = player.parameter.equippedSkills[0].skillIcon;
        }
        for (int i = 0; i < passive_Icon.Length; i++) {
            passive_Icon[i].sprite = player.parameter.equippedPassives[0].passiveIcon;
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
        if (!reloadIconRotating)
            StartCoroutine(RotateReloadIcon(player.weaponController_main.weaponData.reloadTime));
    }

    /// <summary>
    /// 体力のUI更新
    /// </summary>
    public void ChangeHPUI(int _maxHP, int _hp) {
        hpText.text = _hp.ToString();
        hpBar_slider.value = (float)_hp / _maxHP * FIXED_RATIO;
        //死亡時
        if (hpBar_slider.value < 1)
            hpBarImage.gameObject.SetActive(false);
        //2割以下
        else if (hpBar_slider.value <= _maxHP / 5 && hpBar_slider.value >= 1) {
            hpBarImage.color = Color.red;
            hpText.color = Color.red;
        }
        //5割以下
        else if (hpBar_slider.value <= _maxHP / 2) {
            hpBarImage.color = Color.yellow;
            hpText.color = Color.yellow;
        }
        //それ以外
        else {
            hpBarImage.gameObject.SetActive(true);
            hpBarImage.color = Color.green;
            hpText.color = Color.green;
        }

    }

    /// <summary>
    /// MPのUI更新
    /// </summary>
    public void ChangeMPUI(int _maxMP, int _mp) {
        mpText.text = _mp.ToString();
        mpBar_slider.value = (float)_mp / _maxMP * FIXED_RATIO;
        if (_mp <= 0)
            mpBarImage.gameObject.SetActive(false);
        else
            mpBarImage.gameObject.SetActive(true);
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

    public void OnChangeInteractUI() {
        interactUI.SetActive(true);
    }
    public void OffChangeInteractUI() {
        interactUI.SetActive(false);
    }

    public void OnLocalUIObject() {
        localUIObject.SetActive(true);
    }
    public void OffLocalUIObject() {
        localUIObject.SetActive(false);
    }

}
