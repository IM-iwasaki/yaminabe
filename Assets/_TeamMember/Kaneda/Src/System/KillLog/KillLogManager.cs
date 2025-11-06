using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class KillLogManager : NetworkBehaviour {
    //  オブジェクト名の定数
    private readonly string IMAGE_OBJECT_NAME = "KillBannerImg";
    private readonly string TEXT_KILLER_OBJECT_NAME = "KillerName";
    private readonly string TEXT_KILL_OBJECT_NAME = "KillName";

    //  instance
    public static KillLogManager instance = null;

    [Header("キルログ用の親オブジェクト")]
    [SerializeField] private Transform killLogRoot;

    [Header("キルログの基盤プレハブ")]
    [SerializeField] private GameObject killLogPrefab;

    [Header("バナー一覧データ")]
    [SerializeField] private BannerData bannerData;

    [Header("フェードアウトまでの時間")]
    [SerializeField] private float fadeTime = 1.0f;

    [Header("表示する時間")]
    [SerializeField] private float viewTime = 3.0f;

    private void Awake() {
        instance = this;
    }

    /// <summary>
    /// クライアントからサーバーへ送信
    /// </summary>
    /// <param name="bannerId"></param>
    /// <param name="killerName"></param>
    /// <param name="killName"></param>
    [Command(requiresAuthority = false)]
    public void CmdSendKillLog(int bannerId, string killerName, string killName) {
        //  サーバーが全員に通知
        RpcAddKillLog(bannerId, killerName, killName);
    }

    /// <summary>
    /// サーバーから全員へ同期
    /// </summary>
    /// <param name="bannerId"></param>
    /// <param name="killerName"></param>
    /// <param name="killName"></param>
    [ClientRpc]
    private void RpcAddKillLog(int bannerId, string killerName, string killName) {
        //  番号を越したら0に設定
        if (bannerId >= bannerData.bannerInfos.Count) bannerId = 0;
        //  バナーデータから○番目のデータを保存
        BannerData.BannerInfo bannerImages = bannerData.bannerInfos[bannerId];
        //  番号で画像を入れ込む
        Sprite bannerImage = bannerImages.bannerImage;

        //  キルログ生成
        CreateKillLog(bannerImage, killerName, killName);
    }

    /// <summary>
    /// キルログを生成する関数
    /// </summary>
    /// <param name="bannerImage"></param>
    /// <param name="killerName"></param>
    /// <param name="killName"></param>
    private void CreateKillLog(Sprite bannerImage, string killerName = "player", string killName = "player") {
        //  プレハブの生成
        GameObject killLogObj = Instantiate(killLogPrefab, killLogRoot);

        //  生成したプレハブから子オブジェクトを取得する
        Transform bannerObj = killLogObj.transform.Find(IMAGE_OBJECT_NAME);
        Transform killerTextObj = bannerObj.transform.Find(TEXT_KILLER_OBJECT_NAME);
        Transform killTextObj = bannerObj.transform.Find(TEXT_KILL_OBJECT_NAME);
        

        //  画像にバナーをセット
        SetImage(bannerObj, bannerImage);
        //  プレイヤー名をセット
        SetName(killerTextObj, killerName);
        SetName(killTextObj, killName);

        //  フェードアウトして消す
        FadeAndDestroy(killLogObj);
    }

    /// <summary>
    /// 画像を変更する関数
    /// </summary>
    /// <param name="imageObj"></param>
    /// <param name="setImage"></param>
    private void SetImage(Transform imageObj, Sprite setImage) {
        //  取得できなければスルー 
        if (imageObj == null) return;
        //  コンポーネントを取得
        var image = imageObj.GetComponent<Image>();
        //  取得できなければスルー 
        if(image == null) return;
        //  画像を差し替える
        image.sprite = setImage;
    }

    /// <summary>
    /// 名前テキストを変更する関数
    /// </summary>
    /// <param name="nameObj"></param>
    /// <param name="setName"></param>
    private void SetName(Transform nameObj, string setName) {
        //  取得できなければスルー 
        if (nameObj == null) return;
        //  コンポーネントを取得
        var name = nameObj.GetComponent<TextMeshProUGUI>();
        //  取得できなければスルー 
        if (name == null) return;
        //  テキストを差し替える
        name.text = setName;
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
