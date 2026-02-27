using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadingUI : MonoBehaviour {
    [SerializeField,Header("ローディングUI(回転させるやつ)")]
    private GameObject loadingUI;
    [SerializeField,Header("回転スピード")]
    private float rotaSpeed = 5.0f;
    [SerializeField,Header("tipsのイメージ画像※現状使う予定なし")]
    private List<Sprite> tipsImages;
    [SerializeField,Header("回転する長さ")]
    private float rotaTime = 5.0f;
    [SerializeField,Header("tipsの種類")]
    private TextMeshProUGUI tipsCategory;
    [SerializeField,Header("tipsの中身")]
    private TextMeshProUGUI tipsText;
    [SerializeField,Header("tips文章リスト")]
    private ExplaneScentences explaneDatas;

    private bool isLoading = false;
    private float rotaZ = 0f;

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


    public void ShowLoading(GameRuleType _rule) {
        UpdateTips(_rule);
        isLoading = true;
        gameObject.SetActive(true);
    }

    public IEnumerator HideLoading() {
        yield return new WaitForSeconds(rotaTime);

        isLoading = false;
        gameObject.SetActive(false);
        FadeManager.Instance.StartFadeIn(1.0f);
    }

    /// <summary>
    /// Tipsの更新
    /// </summary>
    /// <param name="_index"></param>
    private void UpdateTips(GameRuleType _rule) {

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
            default:
                break;
        }
        tipsText.text = explaneDatas.explanes[(int)_rule];
    }
}
