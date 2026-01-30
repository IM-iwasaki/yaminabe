using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListUIManager : MonoBehaviour
{
    public static PlayerListUIManager Instance;

    //  サーバーマネージャーを取得
    private ServerManager server = null;

    [Header("生成させるプレイヤーリストプレハブ")]
    [SerializeField] private GameObject playerListUI;

    [Header("親ルート取得")]
    [SerializeField] private GameObject playerListRoot;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        playerListRoot.SetActive(false);
    }

    private void Start() {
        server = ServerManager.instance;

        Debug.Log($"[PlayerListUI] server instanceID={server.GetInstanceID()} scene={server.gameObject.scene.name}");
        Debug.Log($"connectPlayer Count = {server.connectPlayer.Count}");

        if (NetworkServer.active && NetworkClient.active) ShowUI();
        UpdatePlayerList();
    }

    //  ホストだけ表示するUI
    public void ShowUI() {
        playerListRoot.SetActive(true);
    }

    /// <summary>
    /// プレイヤーリストの更新
    /// </summary>
    /// <param name="server"></param>
    public void UpdatePlayerList() {
        //  現在のコネクト数をログに流す
        Debug.Log($"connectPlayer Count = {server.connectPlayer.Count}");
        //  一度プレイヤーリストを初期化
        ResetPlayerList();
        //  プレイヤー1人1人のプレハブを作成
        foreach (var conn in server.connectPlayer) {
            //  キャラクターパラメータの情報をキャッシュ
            var player = conn.GetComponent<CharacterParameter>();
            //  子オブジェクトとして生成
            GameObject nameText = Instantiate(playerListUI, playerListRoot.transform);
            //  プレイヤーの名前をチームカラー含めセット]
            ChangePlayerTextAndColor(player, nameText);
            //  チェックボックス判定
            CanReadyPlayerUI(player, nameText);
        }

    }

    /// <summary>
    /// プレイヤーが準備完了か否かをUIで見せる
    /// </summary>
    /// <param name="player"></param>
    public void CanReadyPlayerUI(CharacterParameter player, GameObject nameText) {
        Transform checkBox = nameText.transform.GetChild(0);
        checkBox.gameObject.SetActive(player.ready);
    }

    /// <summary>
    /// テキストをプレイヤーのチームの色に変更する
    /// </summary>
    /// <param name="player"></param>
    /// <param name="nameText"></param>
    private void ChangePlayerTextAndColor(CharacterParameter player, GameObject nameText) {
        //  名前を入れる
        TextMeshProUGUI text = nameText.GetComponent<TextMeshProUGUI>();
        text.SetText(player.PlayerName);
        switch (player.TeamID) {
            //  未所属
            case -1:
                text.color = Color.white;
                break;
            //  赤チーム
            case 0:
                text.color = Color.red;
                break;
            //  青チーム
            case 1:
                text.color = Color.blue;
                break;
        }
    }

    /// <summary>
    /// プレイヤーリストのリセット
    /// </summary>
    private void ResetPlayerList() {
        DestroyAllChildren(playerListRoot.transform);
    }

    /// <summary>
    /// 指定の親オブジェクトの子オブジェクトを全部削除する
    /// </summary>
    /// <param name="parent"></param>
    private void DestroyAllChildren(Transform parent) {
        foreach (Transform child in parent) {
            Destroy(child.gameObject);
        }
    }

}
