using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーUIにコンポーネント済みで、各Playerのメンバとして持たせてほしい
/// 各ローカルプレイヤーに複製してもらうので別に子オブジェクトとかにしなくていい
/// </summary>
public class PlayerUIController : NetworkBehaviour {
    /// <summary>
    /// インスタンス
    /// </summary>
    public static PlayerUIController instance = null;
    /// <summary>
    /// UIの親オブジェクト(主に戦闘用か非戦闘用かを管理)
    /// </summary>
    [SerializeField]
    private List<Transform> UIRoots = null;
    /// <summary>
    /// 親オブジェクトのタイプ
    /// </summary>
    public enum UIRootType {
        Invalid = -1,
        BattleUIRoot,
        NonBattleUIRoot,

        UIMax,
    }
    #region 戦闘用UI
    /// <summary>
    /// バー補正用定数
    /// </summary>
    private const int FIXED_RATIO = 100;
    [SerializeField, Header("数値のテキスト一覧")]
    private TextMeshProUGUI hpText = null;
    [SerializeField]
    private TextMeshProUGUI magazineText = null;
    [SerializeField]
    private TextMeshProUGUI mpText = null;
    [SerializeField, Header("バー一覧")]
    private Slider hpBar = null;
    [SerializeField]
    private Slider magazineBar = null;
    [SerializeField]
    private Slider mpBar = null;
    [SerializeField, Header("バーのイメージ")]
    private Image hpBarImage = null;
    [SerializeField]
    private Image magazineBarImage = null;
    [SerializeField]
    private Image mpBarImage = null;
    #endregion

    #region 非戦闘UI

    [SerializeField, Header("チームメイト確認用UI※人数が変わってもいいように親オブジェクトを取得")]
    Transform teamMateUIRoot = null;
    [SerializeField, Header("生成されるチームメイトUI")]
    private GameObject teammateUI = null;

    #endregion
    [Server]
    private void Awake() {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        hpBar.interactable = false;
        magazineBar.interactable = false;
        mpBar.interactable = false;
    }

    /// <summary>
    /// 初期化関数
    /// </summary>
    /// <param name="_hp"></param>
    public void Initialize(int _hp) {
        hpText.text = _hp.ToString();
        ChangeHPUI(_hp, _hp);
    }

    #region hook関数で呼ぶ想定の関数
    /// <summary>
    /// 体力のUI更新
    /// </summary>
    /// <param name="_maxHP"></param>
    /// <param name="_hp"></param>
    public void ChangeHPUI(int _maxHP, int _hp) {
        hpText.text = _hp.ToString();
        hpBar.value = (float)_hp / _maxHP * FIXED_RATIO;
        //死亡時
        if (hpBar.value < 1)
            hpBarImage.gameObject.SetActive(false);
        //2割以下
        else if (hpBar.value <= _maxHP / 5 && hpBar.value >= 1) {
            hpBarImage.color = Color.red;
            hpText.color = Color.red;
        }
        //5割以下
        else if (hpBar.value <= _maxHP / 2) {
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
    /// 残弾数のUI更新
    /// </summary>
    /// <param name="_maxMagazine"></param>
    /// <param name="_magazine"></param>
    public void ChangeMagazineUI(int _maxMagazine, int _magazine) {
        magazineText.text = _magazine.ToString();
        magazineBar.value = (float)_magazine / _maxMagazine * FIXED_RATIO;
        if (_magazine < 1)
            magazineBarImage.gameObject.SetActive(false);
        else
            magazineBarImage.gameObject.SetActive(true);
    }
    /// <summary>
    /// MPのUI更新
    /// </summary>
    /// <param name="_maxMP"></param>
    /// <param name="_mp"></param>
    public void ChangeMPUI(int _maxMP, int _mp) {
        mpText.text = _mp.ToString();
        mpBar.value = (float)_mp / _maxMP * FIXED_RATIO;
        if (_mp < 1)
            mpBarImage.gameObject.SetActive(false);
        else
            mpBarImage.gameObject.SetActive(true);
    }
    #endregion

    /// <summary>
    /// 特定の"UI群"を表示する
    /// </summary>
    /// <param name="_index"></param>
    public void ShowUIRoot(UIRootType _index) {
        UIRoots[(int)_index].gameObject.SetActive(true);
    }
    /// <summary>
    /// 特定のUI群を非表示にする
    /// </summary>
    /// <param name="_index"></param>
    public void HideUIRoot(UIRootType _index) {
        UIRoots[(int)_index].gameObject.SetActive(false);
    }

    /// <summary>
    /// 特定の"UI"を表示させる
    /// </summary>
    /// <param name="_uiName"></param>
    public void ShowUI(GameObject _UI) {
        _UI.SetActive(true);
    }

    /// <summary>
    /// 特定の"UI"を非表示させる
    /// </summary>
    /// <param name="_uiName"></param>
    public void HideUI(GameObject _UI) {
        _UI.SetActive(false);
    }



    /// <summary>
    /// チームメイトが誰なのかを表示するUIを作り出す
    /// </summary>
    /// <param name="_player"></param>
    [ClientRpc]
    public void CreateTeammateUI(NetworkIdentity _player) {
        if (!_player.isLocalPlayer || !_player.isClient) return;
        Transform createUIRoot = GameObject.Find("NonBattleUIRoot/TeammateUIRoot").transform;

        GameObject madeUI = Instantiate(teammateUI, createUIRoot);
        madeUI.GetComponent<TeammateUI>().Initialize(_player);
    }

    /// <summary>
    /// チームメイトUIを削除
    /// </summary>
    [ClientRpc]
    public void ResetTeammateUI() {
        for (int i = 0, max = teamMateUIRoot.childCount; i < max; i++) {
            Destroy(teamMateUIRoot.GetChild(i).gameObject);
        }
    }

}
