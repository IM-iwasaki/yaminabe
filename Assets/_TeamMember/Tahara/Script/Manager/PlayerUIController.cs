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
    public static PlayerUIController instance = null;
    [SerializeField]
    private List<Transform> UIRoots = null;

    #region 戦闘用UI
    #region プレイヤー体力管理用UI
    private const int FIXED_RATIO = 100;
    [SerializeField, Header("体力のテキスト※デフォルトで設定済み")]
    private TextMeshProUGUI hpText = null;
    [SerializeField, Header("体力のバー※デフォルトで設定済み")]
    private Slider hpBar = null;
    [SerializeField, Header("体力のバーのイメージ※デフォルトで設定済み")]
    private Image hpBarImage = null;
    [SerializeField, Header("残弾数のテキスト※デフォルトで設定済み")]
    private TextMeshProUGUI magazineText = null;
    [SerializeField, Header("残弾数のバー※デフォルトで設定済み")]
    private Slider magazineBar = null;
    [SerializeField, Header("残弾数のバーのイメージ※デフォルトで設定済み")]
    private Image magazineBarImage = null;
    [SerializeField, Header("MPのテキスト※デフォルトで設定済み")]
    private TextMeshProUGUI mpText = null;
    [SerializeField, Header("MPのバー※デフォルトで設定済み")]
    private Slider mpBar = null;
    [SerializeField, Header("MPのバーのイメージ※デフォルトで設定済み")]
    private Image mpBarImage = null;
    #endregion

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
        if (_hp < 1)
            hpBarImage.gameObject.SetActive(false);
        else if (hpBar.value <= _maxHP / 5 && _hp >= 1) {
            hpBarImage.color = Color.red;
        }

        else if (hpBar.value <= _maxHP / 2) {
            hpBarImage.color = Color.yellow;
        }


        else {
            hpBarImage.gameObject.SetActive(true);
            hpBarImage.color = Color.green;
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
    public void ShowUIRoot(int _index) {
        UIRoots[_index].gameObject.SetActive(true);
    }
    /// <summary>
    /// 特定のUI群を非表示にする
    /// </summary>
    /// <param name="_index"></param>
    public void HideUIRoot(int _index) {
        UIRoots[_index].gameObject.SetActive(false);
    }

    /// <summary>
    /// 特定の"UI"を表示させる
    /// </summary>
    /// <param name="_uiName"></param>
    public void ShowUI(string _uiName) {
        GameObject.Find(_uiName).SetActive(true);
    }

    /// <summary>
    /// 特定の"UI"を非表示させる
    /// </summary>
    /// <param name="_uiName"></param>
    public void HideUI(string _uiName) {
        GameObject.Find(_uiName).SetActive(false);
    }



    /// <summary>
    /// チームメイトが誰なのかを表示するUIを作り出す
    /// </summary>
    /// <param name="_player"></param>
    public void CreateTeammateUI(NetworkIdentity _player) {
        if (!_player.isLocalPlayer || !_player.isClient) return;
        Transform createUIRoot = GameObject.Find("NonBattleUIRoot/TeammateUIRoot").transform;

        GameObject madeUI = Instantiate(teammateUI, createUIRoot);
        madeUI.GetComponent<TeammateUI>().Initialize(_player);
    }

    /// <summary>
    /// チームメイトUIを削除
    /// </summary>
    public void ResetTeammateUI() {
        for (int i = 0, max = teamMateUIRoot.childCount; i < max; i++) {
            Destroy(teamMateUIRoot.GetChild(i).gameObject);
        }
    }

}
