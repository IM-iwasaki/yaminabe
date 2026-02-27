using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadingUI : MonoBehaviour {
    [SerializeField]
    private GameObject loadingUI;
    [SerializeField]
    private float rotaSpeed = 5.0f;
    [SerializeField]
    private List<Sprite> tipsImages;
    [SerializeField]
    private float rotaTime = 5.0f;
    [SerializeField]
    private TextMeshProUGUI tipsCategory;
    [SerializeField]
    private TextMeshProUGUI tipsText;
    [SerializeField]
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
        yield return new WaitForSeconds(4.5f);

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
