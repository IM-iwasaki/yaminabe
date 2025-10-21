using Mono.CecilX.Cil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    private readonly string TEXT_OBJECT_NAME = "UserName";
    private readonly string IMAGE_OBJECT_NAME = "StampImage";

    private static ChatManager instance;

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

    }

    /// <summary>
    /// 高さ制限チェック、超えたら古いものから削除
    /// </summary>
    private void CheckHeightLimit() {
        RectTransform rootRect = chatRoot as RectTransform;

        //  現在の高さを取得
        float rootHeight = rootRect.rect.height;

        //  高さ上限を超えた場合、古いものから削除
        while(rootHeight > maxHeight && chatRoot.childCount > 0) {
            //  一番古いの子オブジェクトを取得、削除
            Transform oldChatObj = chatRoot.GetChild(0);
            Destroy(oldChatObj.gameObject);

            //  削除した後、高さが更新されるまで待機
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
            rootHeight = rootRect.rect.height;
        }
    }

    private IEnumerator FadeAndDestroy(GameObject obj) {

    }

}
