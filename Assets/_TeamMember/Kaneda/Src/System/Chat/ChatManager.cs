using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    private readonly string TEXT_OBJECT_NAME = "UserName";
    private readonly string IMAGE_OBJECT_NAME = "StampImage";

    public static ChatManager instance = null;

    [Header("チャットログ用の親オブジェクト")]
    [SerializeField] private Transform chatRoot;

    [Header("スタンプの基盤プレハブ")]
    [SerializeField] private GameObject stamp;

    [Header("システムメッセージ基盤プレハブ")]
    [SerializeField] private GameObject systemText;

    [Header("表示可能な高さ上限")]
    [SerializeField] private float maxHeight = 650f;

    [Header("フェードアウトまでの時間")]
    [SerializeField] private float fadeTime = 1.0f;

    [Header("表示する時間")]
    [SerializeField] private float viewTime = 3.0f;

    private void Awake() {
        instance = this;
    }

    /// <summary>
    /// スタンプをチャットに生成
    /// </summary>
    /// <param name="stampImage"></param>
    /// <param name="userName"></param>
    public void AddStamp(Sprite stampImage = null, string userName = "player") {
        //  プレハブを生成する
        GameObject stampObj = Instantiate(stamp, chatRoot);

        //  生成したプレハブの子オブジェクトのImage,Textを取得
        Transform nameObj = stampObj.transform.Find(TEXT_OBJECT_NAME);
        Transform imageObj = stampObj.transform.Find(IMAGE_OBJECT_NAME);

        //  ユーザー名を設定
        SetUserName(nameObj, userName);
        //  スタンプの画像を設定
        SetImage(imageObj, stampImage);

        //  高さ制限チェック
        CheckHeightLimit();

        //  フェードアウト処理
        StartCoroutine(FadeAndDestroy(stampObj));
    }

    //  ユーザー名を設定する関数
    private void SetUserName(Transform nameObj, string userName) {
        //  取得できなければ空で通す 
        if (nameObj == null) return;
        //  コンポーネントを取得
        var tmp = nameObj.GetComponent<TextMeshProUGUI>();
        //  取得できなければ空で通す
        if (tmp == null) return;
        //  userNameを入れ込む
        tmp.SetText("[" + userName + "]");
    }

    //  スタンプの画像を設定
    private void SetImage(Transform imageObj, Sprite stampImage) {
        //  取得できなければスルー 
        if (imageObj == null) return;
        //  コンポーネントを取得
        var image = imageObj.GetComponent<Image>();
        //  取得できなければスルー
        if (image == null) return;
        //  画像を差し替える
        image.sprite = stampImage;
    }

    /// <summary>
    /// システムメッセージをチャットに生成
    /// </summary>
    /// <param name="message"></param>
    public void AddSystemMessage(string message = "") {
        //  プレハブを生成
        GameObject textObj = Instantiate(systemText, chatRoot);
        //  コンポーネントを取得
        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        //  取得できなければスルー
        if (tmp == null) return;
        //  テキストを書き換える
        tmp.SetText("<System> " + message);

        //  高さ制限チェック
        CheckHeightLimit();

        //  フェードアウト処理
        StartCoroutine(FadeAndDestroy(textObj));
    }

    /// <summary>
    /// 高さ制限チェック、超えたら古いものから削除
    /// </summary>
    private void CheckHeightLimit() {
        RectTransform rootRect = chatRoot as RectTransform;
        //  現在の高さを取得
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
        float rootHeight = rootRect.rect.height;

        bool check = rootHeight > maxHeight && chatRoot.childCount > 0;

        // 超えている場合のみ処理
        if (rootHeight <= maxHeight) return;

        // 古いものから順に削除
        int index = 0;
        while (index < chatRoot.childCount && rootHeight > maxHeight) {
            RectTransform oldest = chatRoot.GetChild(index).GetComponent<RectTransform>();
            if (oldest != null) {
                // 削除前に高さを取得
                float h = oldest.rect.height;
                Destroy(oldest.gameObject);
                // 高さを減算
                rootHeight -= h;
            }
            index++;
        }
    }

    /// <summary>
    /// フェードアウト、削除
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private IEnumerator FadeAndDestroy(GameObject obj) {
        CanvasGroup groupe = obj.GetComponent<CanvasGroup>();
        if(groupe == null) groupe = obj.AddComponent<CanvasGroup>();

        //  表示する時間待機
        yield return new WaitForSeconds(viewTime);

        //  フェードアウト処理
        float timer = 0f;
        while(timer < fadeTime) {
            timer += Time.deltaTime;
            groupe.alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
            yield return null;
        }
        //  削除する
        Destroy(obj);
    }

}
