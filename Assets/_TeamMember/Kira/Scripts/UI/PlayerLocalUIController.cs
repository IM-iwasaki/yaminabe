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
    [SerializeField] GameObject mpUnderBar;

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

    //裏のバーの参照
    [SerializeField] Slider hpUnderBar_slider = null;
    [SerializeField] Slider mpUnderBar_slider = null;
    //裏のバーの減少開始遅延
    private readonly float underBar_Delay = 0.5f;

    /// <summary>
    /// ローカルUI全体の初期化
    /// </summary>
    public void Initialize() {
        hpBar_slider.interactable = false;
        mpBar_slider.interactable = false;
        hpUnderBar_slider.interactable = false;
        mpUnderBar_slider.interactable = false;

        if (!isLocalPlayer) {
            var LocalUI = GetComponentInChildren<Canvas>();
            LocalUI.gameObject.SetActive(false);
            return;
        }
        mainWeaponReloadIcon.enabled = false;
        interactUI.SetActive(false);
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();
        Initialize();
    }

    void Update() {
        // ローカルプレイヤー以外は一切処理しない
        if (!isLocalPlayer) return;

        // null ガード（Spawn直後・切断直前対策）
        if (player == null) return;
        if (player.parameter == null) return;
        if (player.weaponController_main == null) return;
        if (player.weaponController_sub == null) return;

        var mainWeapon = player.weaponController_main.weaponData;
        if (mainWeapon == null) return;

        // スキル / パッシブ状態更新
        UpdateSkillState();
        UpdatePassiveState();

        // 裏バー更新
        UpdateUnderBar();

        // 武器タイプ別UI更新
        switch (mainWeapon.type) {
            case WeaponType.Melee:
                // 近接は弾数無限表示
                mainWeaponText[(int)TextIndex.Current].text = "∞";
                break;

            case WeaponType.Gun:
                // 銃は現在弾数表示
                mainWeaponText[(int)TextIndex.Current].text =
                    player.weaponController_main.ammo.ToString();
                break;

            case WeaponType.Magic:
                // 魔法はMPコスト表示
                if (WeaponDataRegistry.GetWeapon(mainWeapon.WeaponName) is MainMagicData magicData) {
                    mainWeaponText[(int)TextIndex.Partition].text =
                        "Cost : " + magicData.MPCost.ToString();
                }
                break;
        }

        // MP表示更新
        mpText.text = player.parameter.MP.ToString();

        // サブ武器表示更新
        subWeaponText[(int)TextIndex.Current].text =
            player.weaponController_sub.currentUses.ToString();
    }

    /// <summary>
    /// 裏バーの更新関数
    /// </summary>
    private void UpdateUnderBar() {
        //表バー値が裏バー値より低かったら
        if (hpBar_slider.value < hpUnderBar_slider.value) {
            //裏バー値と表バー値の差分を算出
            float valueDiscrepancy = hpUnderBar_slider.value - hpBar_slider.value;
            //差分が一定以下になったらバー同士の値を合わせる
            if(valueDiscrepancy <= 0.2f) {
                hpUnderBar_slider.value = hpBar_slider.value;
            }
            //指数関数的に速度を落としながら裏バーの値を減少させる
            hpUnderBar_slider.value -= valueDiscrepancy / 60;
        }
        //表バーが裏バーの値を超える時値を合わせる
        if (hpBar_slider.value > hpUnderBar_slider.value) {
            hpUnderBar_slider.value = hpBar_slider.value;
        }
        //表バー値が裏バー値より低かったら
        if (mpBar_slider.value < mpUnderBar_slider.value) {
            //裏バー値と表バー値の差分を算出
            float valueDiscrepancy = mpUnderBar_slider.value - mpBar_slider.value;
            //差分が一定以下になったらバー同士の値を合わせる
            if(valueDiscrepancy <= 0.2f) {
                mpUnderBar_slider.value = mpBar_slider.value;
            }
            //指数関数的に速度を落としながら裏バーの値を減少させる
            mpUnderBar_slider.value -= valueDiscrepancy / 60;
        }
        //表バーが裏バーの値を超える時値を合わせる
        if (mpBar_slider.value > mpUnderBar_slider.value) {
            mpUnderBar_slider.value = mpBar_slider.value;
        }
    }

    /// <summary>
    /// スキルの表示状態管理
    /// </summary>
    private void UpdateSkillState() {
        if (!isLocalPlayer) return;
        //現在のスキルの状態をキャッシュ
        var skillParam = player.parameter.equippedSkills[0];

        //スキルが使用可能な場合
        if (player.action.isCanSkill) {
            //ゲージの端数を捨て、色を黄色にする
            skill_State.fillAmount = 1.0f;
            skill_State.color = Color.yellow;
        }
        //スキルが使用不可だった場合
        else {
            //ゲージを更新・反映する、色を白に変更する
            skillStateProgress = player.parameter.skillAfterTime / skillParam.cooldown;
            skill_State.fillAmount = skillStateProgress;
            skill_State.color = Color.white;
        }
    }

    /// <summary>
    /// パッシブの表示状態管理
    /// </summary>
    private void UpdatePassiveState() {
        if (!isLocalPlayer) return;
        //現在のパッシブの状態をキャッシュ
        var passiveParam = player.parameter.equippedPassives[0];

        //パッシブが発動中、またはパッシブの蓄積数が1以上ある場合
        if (passiveParam.isPassiveActive || passiveParam.passiveChains >= 1) {
            //蓄積数をテキストに反映、アイコンを黄色に変える
            passiveChains.text = passiveParam.passiveChains.ToString();
            passive_State.color = Color.yellow;
        }
        else {
            //アイコンを白色に変える
            passive_State.color = Color.white;
        }

        //パッシブの蓄積数が0だったら空欄にする
        if (passiveParam.passiveChains == 0) passiveChains.text = "";
    }

    /// <summary>
    /// スキルとパッシブのアイコン、武器の情報の反映
    /// </summary>
    public void LocalUIChanged() {
        // ローカルプレイヤー以外は処理しない
        if (!isLocalPlayer) return;

        // null ガード（Mirrorでは必須）
        if (player == null) return;
        if (player.weaponController_main == null) return;
        if (player.weaponController_sub == null) return;

        var mainWeapon = player.weaponController_main.weaponData;
        var subWeapon = player.weaponController_sub.subWeaponData;

        if (mainWeapon == null || subWeapon == null) return;
        if (player.parameter == null) return;

        // MPバーの表示制御
        if (mainWeapon.type == WeaponType.Magic) {
            mpBar.SetActive(true);
            mpUnderBar.SetActive(true);
        } else {
            mpBar.SetActive(false);
            mpUnderBar.SetActive(false);
        }

        // HP / MP UI
        hpText.text = player.parameter.HP.ToString();
        ChangeHPUI(player.parameter.maxHP, player.parameter.HP);

        mpText.text = player.parameter.MP.ToString();
        ChangeMPUI(player.parameter.maxMP, player.parameter.MP);

        // スキル / パッシブアイコン
        if (player.parameter.equippedSkills != null && player.parameter.equippedSkills.Length > 0) {
            for (int i = 0; i < skill_Icon.Length; i++) {
                skill_Icon[i].sprite = player.parameter.equippedSkills[0].skillIcon;
            }
        }

        if (player.parameter.equippedPassives != null && player.parameter.equippedPassives.Length > 0) {
            for (int i = 0; i < passive_Icon.Length; i++) {
                passive_Icon[i].sprite = player.parameter.equippedPassives[0].passiveIcon;
            }
        }

        // メイン武器UI
        mainWeaponText[(int)TextIndex.WeaponName].text = mainWeapon.weaponName;

        if (mainWeapon.type == WeaponType.Gun) {
            mainWeaponText[(int)TextIndex.Partition].text = "/";
            mainWeaponText[(int)TextIndex.Current].text = player.weaponController_main.ammo.ToString();
            mainWeaponText[(int)TextIndex.Max].text = mainWeapon.maxAmmo.ToString();
        } else {
            mainWeaponText[(int)TextIndex.Partition].text = "";
            mainWeaponText[(int)TextIndex.Current].text = "";
            mainWeaponText[(int)TextIndex.Max].text = "";
        }

        // サブ武器UI
        subWeaponText[(int)TextIndex.Current].text = player.weaponController_sub.currentUses.ToString();
        subWeaponText[(int)TextIndex.Max].text = subWeapon.maxUses.ToString();
        subWeaponText[(int)TextIndex.WeaponName].text = subWeapon.WeaponName;
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
        Debug.Log("value:"+_hp + "/ slider.value:" + hpBar_slider.value);
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
    /// リロードアイコンを1回転させる (float _duration = 1回転するまでにかかる時間 )
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

    /// <summary>
    /// インタラクト用UIの表示
    /// </summary>
    public void OnChangeInteractUI() {
        interactUI.SetActive(true);
    }
    /// <summary>
    /// インタラクト用UIの非表示
    /// </summary>
    public void OffChangeInteractUI() {
        interactUI.SetActive(false);
    }
    /// <summary>
    /// プレイヤーローカルUIの表示
    /// </summary>
    public void OnLocalUIObject() {
        localUIObject.SetActive(true);
    }
    /// <summary>
    /// プレイヤーローカルUIの非表示
    /// </summary>
    public void OffLocalUIObject() {
        localUIObject.SetActive(false);
    }
}