using Mirror;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;

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

    [SerializeField]
    private GameObject LocalUICanvas;

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
#pragma warning disable CS0414
    private Image mpBarImage = null;
#pragma warning restore CS0414

    [SerializeField] Image[] skill_Icon;
    [SerializeField] Image skill_State;
    [SerializeField] Image[] passive_Icon;
    [SerializeField] Image passive_State;
    [SerializeField] TextMeshProUGUI passiveChains;
    [SerializeField] CharacterBase player;
    [SyncVar] float skillStateProgress = 0.0f;

    [SerializeField] GameObject interactUI;
    //  ローカルUIの本体を取得
    [SerializeField] GameObject localUIObject;

    //裏のバーの参照
    [SerializeField] Slider hpUnderBar_slider = null;
    [SerializeField] Slider mpUnderBar_slider = null;

    /// <summary>
    /// ローカルUIの安全な初期化
    /// </summary>
    public void InitializeLocalUI(CharacterBase _player) {
        if (!isLocalPlayer) return;

        player = _player;
        if (player == null) return;

        // UIオブジェクトが無効なら有効化
        if (localUIObject != null && !localUIObject.activeSelf)
            localUIObject.SetActive(true);

        Initialize();

        // UI参照が揃っていなければ中断
        if (!IsUIReady()) return;

        LocalUIChanged();
    }

    /// <summary>
    /// ローカルUI全体の初期化
    /// </summary>
    public void Initialize() {
        hpBar_slider.interactable = false;
        mpBar_slider.interactable = false;
        hpUnderBar_slider.interactable = false;
        mpUnderBar_slider.interactable = false;
        hpBar_slider.maxValue = FIXED_RATIO;
        mpBar_slider.maxValue = FIXED_RATIO;

        localUIObject.SetActive(true);

        mainWeaponReloadIcon.enabled = false;
        interactUI.SetActive(false);
    }

    void Update() {
        if (!isLocalPlayer) return;

        //表示状態管理関数の呼び出し
        UpdateSkillState();
        UpdatePassiveState();

        //裏バーの更新
        UpdateUnderBar();
        //現在使用している武器タイプで分岐
        switch (player.weaponController_main.weaponData.type) {
            case WeaponType.Melee:
                //近接攻撃に弾数やMPは存在しないので表示を切り替える
                mainWeaponText[(int)TextIndex.Current].text = "∞";
                break;
            case WeaponType.Gun:
                //メインウェポンの現在弾倉数を更新
                mainWeaponText[(int)TextIndex.Current].text = player.weaponController_main.ammo.ToString();
                break;
            case WeaponType.MoneyGun:
                //メインウェポンの現在弾倉数を更新
                mainWeaponText[(int) TextIndex.Current].text = player.weaponController_main.ammo.ToString();
                break;
            case WeaponType.Magic:
                //所持している武器が魔法であるか確認。
                if (WeaponDataRegistry.GetWeapon(player.weaponController_main.weaponData.WeaponID) is not MainMagicData magicData) {
#if UNITY_EDITOR
                    Debug.LogError("所持している魔法の詳細情報を正常に取得できませんでした。");
#endif
                    return;
                }
                //MP消費量をテキストに反映
                mainWeaponText[(int)TextIndex.Partition].text = "Cost : " + magicData.MPCost.ToString();
                break;
        }
        //現在のMPをテキストに反映
        mpText.text = player.parameter.MP.ToString();
        //サブウェポンの現在所持数を更新
        subWeaponText[(int)TextIndex.Current].text = player.weaponController_sub.CurrentUses.ToString();
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
            if (valueDiscrepancy <= 0.2f) {
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
            if (valueDiscrepancy <= 0.2f) {
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
        } else {
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
        // 自分のみ処理する
        if (!isLocalPlayer) return;
        // プレイヤーが存在しないかUIの準備が整っていない時は変える
        if (player == null || !IsUIReady()) return;

        // メイン武器のキャッシュ
        var main = player.weaponController_main;
        // 武器データがない時帰る
        if (main == null || main.weaponData == null) return;

        // 取得した武器データが魔法であるか判定、魔法だったらMPバー表示
        bool isMagic = main.weaponData.type == WeaponType.Magic;
        mpBar.SetActive(isMagic);
        mpUnderBar.SetActive(isMagic);

        // HPとMPのUIを強制的に更新
        ChangeHPUI(player.parameter.maxHP, player.parameter.HP);
        ChangeMPUI(player.parameter.maxMP, player.parameter.MP);

        // 可読性向上のためキャッシュ
        var skills = player.parameter.equippedSkills;
        // スキル
        if (skills != null && skills.Length > 0 && skills[0] != null) {
            // スキルアイコンの反映
            for (int i = 0; i < skill_Icon.Length; i++)
                skill_Icon[i].sprite = skills[0].skillIcon;
        }

        // 可読性向上のためキャッシュ
        var passives = player.parameter.equippedPassives;
        // パッシブ
        if (passives != null && passives.Length > 0 && passives[0] != null) {
            // パッシブアイコンの表示
            for (int i = 0; i < passive_Icon.Length; i++)
                passive_Icon[i].sprite = passives[0].passiveIcon;
        }

        // メイン武器の名前をUIに反映
        mainWeaponText[(int)TextIndex.WeaponName].text = main.weaponData.weaponName;

        if (main.weaponData.type == WeaponType.Gun || main.weaponData.type == WeaponType.MoneyGun) {
            mainWeaponText[(int)TextIndex.Partition].text = "/";
            mainWeaponText[(int)TextIndex.Current].text = main.ammo.ToString();
            mainWeaponText[(int)TextIndex.Max].text = main.weaponData.maxAmmo.ToString();
        } else {
            mainWeaponText[(int)TextIndex.Partition].text = "";
            mainWeaponText[(int)TextIndex.Current].text = "";
            mainWeaponText[(int)TextIndex.Max].text = "";
        }

        // サブ武器
        var sub = player.weaponController_sub;
        if (sub != null && sub.subWeaponData != null) {
            //サブ武器の現在数、最大数、名前をテキストに反映
            subWeaponText[(int)TextIndex.Current].text = sub.CurrentUses.ToString();
            subWeaponText[(int)TextIndex.Max].text = sub.subWeaponData.maxUses.ToString();
            subWeaponText[(int)TextIndex.WeaponName].text = sub.subWeaponData.WeaponName;
        }
    }

    /// <summary>
    /// UIが正常に取得できるか確認する関数
    /// </summary>
    private bool IsUIReady() {
        return
            mpBar != null &&
            mpUnderBar != null &&
            hpText != null &&
            mpText != null &&
            hpBar_slider != null &&
            mpBar_slider != null &&
            hpUnderBar_slider != null &&
            mpUnderBar_slider != null &&
            mainWeaponText != null &&
            mainWeaponText.Length >= 4 &&
            subWeaponText != null &&
            subWeaponText.Length >= 4;
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
        //HP現在値をテキストに反映
        hpText.text = _hp.ToString();
        //HP最大値が0の時は帰る(_maxHPが0の時に発生する0除算の防止)
        if (_maxHP <= 0) return;
        //現在のHP割合をSliderのvalueに反映
        hpBar_slider.value = (float)_hp / _maxHP * FIXED_RATIO;
        //死亡時
        if (_hp <= 0)
            hpBarImage.gameObject.SetActive(false);
        //2割以下
        else if (_hp <= _maxHP / 5 && _hp >= 1) {
            hpBarImage.color = Color.red;
            hpText.color = Color.red;
        }
        //5割以下
        else if (_hp <= _maxHP / 2) {
            hpBarImage.color = Color.yellow;
            hpText.color = Color.yellow;
        }
        //5割超
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
        //MP現在値をテキストに反映
        mpText.text = _mp.ToString();
        //MP最大値が0の時は帰る(_maxMPが0の時に発生する0除算の防止)
        if (_maxMP <= 0) return;
        //現在のMP割合をSliderのvalueに反映
        mpBar_slider.value = (float)_mp / _maxMP * FIXED_RATIO;
        //if (_mp <= 0)
        //    mpBarImage.gameObject.SetActive(false);
        //else {
        //    mpBarImage.gameObject.SetActive(true);
        //}            
    }

    /// <summary>
    /// リロードアイコンを1回転させる ( float _duration = 1回転するまでにかかる時間)
    /// </summary>
    public IEnumerator RotateReloadIcon(float _duration) {
        //リロードアイコン表示状態にする
        reloadIconRotating = true;
        //アイコンを有効化
        mainWeaponReloadIcon.enabled = true;
        //角度と経過時間の初期化
        float start = 0f;
        float end = -360f;
        float time = 0f;

        //回転が完了するまでループ
        while (time < _duration) {
            //設定時間に対する現在経過時間を計算
            float t = time / _duration;
            //このフレームで到達する角度を計算
            float angle = Mathf.Lerp(start, end, t);
            //計算した角度をアイコンに反映
            mainWeaponReloadIcon.transform.localRotation = Quaternion.Euler(0, 0, angle);
            //時間経過の加算
            time += Time.deltaTime;
            //1フレーム待つ
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