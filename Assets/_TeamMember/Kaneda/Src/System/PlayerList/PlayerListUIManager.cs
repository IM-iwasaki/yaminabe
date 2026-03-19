using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListUIManager : NetworkBehaviour {
    public static PlayerListUIManager Instance;

    //  サーバーマネージャーを取得
#pragma warning disable CS0414
    private ServerManager server = null;
#pragma warning restore CS0414

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

    /// <summary>
    /// 追加 マツオ : スタート時クライアントもUI出す
    /// </summary>
    public override void OnStartClient() {
        base.OnStartClient();

        ShowUI();

        InvokeRepeating(nameof(UpdatePlayerList), 0.5f, 0.5f);
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
        ResetPlayerList();
        // 変更 : マツオ
        foreach (var identity in NetworkClient.spawned.Values) {
            if (!identity.TryGetComponent<CharacterParameter>(out var player))
                continue;

            GameObject nameText = Instantiate(playerListUI, playerListRoot.transform);

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
        checkBox.gameObject.SetActive(player.isReady);
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
                text.color = Color.cyan;
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
    /// <summary>
    /// 追加 マツオ : Listの内容が切り替わった時に呼ばれる
    /// </summary>
    private void OnPlayerListChanged(SyncList<NetworkIdentity>.Operation op,int index,NetworkIdentity oldItem,NetworkIdentity newItem) {
        UpdatePlayerList();
    }

}
