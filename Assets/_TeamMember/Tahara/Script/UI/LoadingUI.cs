using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingUI : MonoBehaviour {
    public static LoadingUI instance = null;
    public bool isLoading { get; private set; }
    [SerializeField, Header("ローディングUI(回転させるやつ)")]
    private GameObject loadingUI;
    [SerializeField, Header("回転スピード")]
    private float rotaSpeed = 5.0f;
    [SerializeField, Header("tipsの種類")]
    private TextMeshProUGUI tipsCategory;
    [SerializeField, Header("ルール説明")]
    private TextMeshProUGUI ruleDiscription;
    [SerializeField, Header("tipsの中身")]
    private TextMeshProUGUI tipsText;
    [SerializeField, Header("チームカラー表示用UI")]
    private TextMeshProUGUI teamColorUI;
    [SerializeField, Header("tips文章リスト")]
    private List<ExplaneScentences> explaneDatas;
    [SerializeField, Header("ルール説明用文章リスト")]
    private ExplaneScentences ruleDiscriptionSentences;
    private float rotaZ = 0f;


    private void Awake() {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        teamColorUI.text = "";
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }
    /// <summary>
    /// ロード画面出力非同期処理
    /// ロード画面中の全ての処理の発火場所
    /// </summary>
    /// <returns></returns>
    void Update() {
        if (!isLoading) return;

        rotaZ -= rotaSpeed;
        loadingUI.transform.rotation = Quaternion.Euler(0, 0, rotaZ);
    }

    public void ShowLoading(GameRuleType _rule = GameRuleType.Hoko) {
        if (tipsCategory == null || tipsText == null || ruleDiscription == null)
            return;
        UpdateTips(_rule);
        isLoading = true;
        gameObject.SetActive(true);
    }

    public IEnumerator HideLoading(float _waittime = 3.0f) {
        yield return new WaitForSeconds(_waittime);

        isLoading = false;
        gameObject.SetActive(false);
        FadeManager.Instance.StartFadeIn(1.0f);
    }

    /// <summary>
    /// Tipsの更新
    /// </summary>
    /// <param name="_index"></param>
    private void UpdateTips(GameRuleType _rule) {
        //とりあえず決め打ち(クライアント側で変更がかかるので全員違うのが出るはず)
        int tipsIndex = Random.Range(0, 3);
        switch (_rule) {
            case GameRuleType.Hoko:
                tipsCategory.text = "クラウン";
                break;
            case GameRuleType.Area:
                tipsCategory.text = "エリア";
                break;
            case GameRuleType.DeathMatch:
                tipsCategory.text = "デスマッチ";
                break;
            case GameRuleType.PvE:
                tipsCategory.text = "PvE";
                break;
            default:
                break;
        }
        //メインのTipsを変更
        tipsText.text = explaneDatas[(int)_rule].explanes[tipsIndex];
        //ルール説明も変更
        ruleDiscription.text = ruleDiscriptionSentences.explanes[(int)_rule];
    }

    public void UpdateTeamColorUI(int _teamID = -1) {
        if(_teamID == 0) {
            teamColorUI.color = Color.red;
            teamColorUI.text = "チーム:Red";
        }
        else if(_teamID == 1) {
            teamColorUI.color = Color.blue;
            teamColorUI.text = "チーム:Blue";
        }
        else {
            teamColorUI.text = "";
        }
    }

    public void SetIsLoading(bool _isLoading) {
        isLoading = _isLoading;
    }
}
