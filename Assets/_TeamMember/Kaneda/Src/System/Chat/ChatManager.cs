using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : NetworkBehaviour {
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

    [Header("スタンプ一覧データ")]
    [SerializeField] private StampData stampData;

    private void Awake() {
        instance = this;
    }

    #region スタンプ生成
    //  クライアントからサーバーへ送信
    [Command(requiresAuthority = false)]
    public void CmdSendStamp(int stampId, string userName) {
        //  サーバーが全員に通知
        RpcAddStamp(stampId, userName);
    }
    //  サーバーから全員へ同期
    [ClientRpc]
    private void RpcAddStamp(int stampId, string userName) {

        //  番号が越していたら0に設定
        if (stampId >= stampData.stampInfos.Count) stampId = 0;
        //  スタンプデータから○番目のデータを保存
        StampData.StampInfo stampImages = stampData.stampInfos[stampId];
        //  番号で画像を入れ込む
        Sprite stampImage = stampImages.stampImage;

        //  スタンプを生成
        CreateStamp(stampImage, userName);
    }

    /// <summary>
    /// スタンプをチャットに生成
    /// </summary>
    /// <param name="stampImage"></param>
    /// <param name="userName"></param>
    private void CreateStamp(Sprite stampImage, string userName = "player") {
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

    /// <summary>
    ///  ユーザー名を設定する関数
    /// </summary>
    /// <param name="nameObj"></param>
    /// <param name="userName"></param>
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

    /// <summary>
    ///  スタンプの画像を設定
    /// </summary>
    /// <param name="imageObj"></param>
    /// <param name="stampImage"></param>
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
    #endregion

    #region システムメッセージ
    //  クライアントからサーバーへ送信
    [Command(requiresAuthority = false)]
    public void CmdSendSystemMessage(string message) {
        RpcAddMessage(message);
    }
    //  サーバーから全員へ同期
    [ClientRpc]
    private void RpcAddMessage(string message) {
        //  システムメッセージを生成
        CreateSystemMessage(message);
    }

    /// <summary>
    /// システムメッセージをチャットに生成
    /// </summary>
    /// <param name="message"></param>
    private void CreateSystemMessage(string message = "") {
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
    #endregion

    /// <summary>
    /// 高さ制限チェック、超えたら古いものから削除
    /// </summary>
    private void CheckHeightLimit() {
        RectTransform rootRect = chatRoot as RectTransform;
        //  現在の高さを取得
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
        float rootHeight = rootRect.rect.height;

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
        if (groupe == null) groupe = obj.AddComponent<CanvasGroup>();

        //  表示する時間待機
        yield return new WaitForSeconds(viewTime);

        //  フェードアウト処理
        float timer = 0f;
        while (timer < fadeTime) {
            timer += Time.deltaTime;
            groupe.alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
            yield return null;
        }
        //  削除する
        Destroy(obj);
    }

}
