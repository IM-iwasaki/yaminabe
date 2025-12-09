using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// プレイヤーのレーティングランキング管理
/// 表示と数値の変更を扱う
/// </summary>
public class PlayerRankingManager : NetworkBehaviour {
    /// <summary>
    /// インスタンス
    /// </summary>
    public static PlayerRankingManager instance { get; private set; }

    /// <summary>
    /// レート表示用UI
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI rateUI = null;

    /// <summary>
    /// レートを変更するプレイヤーのデータ
    /// </summary>
    private PlayerData playerData;


    private void Awake() {
        // シングルトンの設定
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPlayerData();
    }

    private void LoadPlayerData() {
        //データをロード
        playerData = PlayerSaveData.Load();
    }

    /// <summary>
    /// レートの計算
    /// 実装初期段階では固定値を加えるが試合内容によって変動できるように計算機能も持たせる
    /// 値の変更なのでサーバーで処理
    /// </summary>
    /// <param name="_teamID"></param>
    [Server]
    public int CalculatePersonalRate(NetworkConnectionToClient _conn,int _winnerTeamID) {
        //プレイヤーを取得
        var playerObj = _conn.identity;
        int addRate;
        int teamID = playerObj.gameObject.GetComponent<CharacterBase>().TeamID;
        switch (teamID == _winnerTeamID) {
            case true:
                addRate = 10;
                break;
            case false:
                addRate = -10;
                break;
        }

        return addRate;
    }

    /// <summary>
    /// 対象に正しくレートを反映する
    /// </summary>
    /// <param name="_playerConn"></param>
    [Server]
    public void ApplyRateAllPlayers(int _winnerTeamID) {
        //全員のコネクションを保持するリストを生成しサーバーから接続者を加える
        List<NetworkConnectionToClient> playerConn = new List<NetworkConnectionToClient>();
        foreach (var addConn in ServerManager.instance.connectPlayer) {
            if (addConn == null || addConn.connectionToClient == null) continue;
            //ホスト分もスキップ
            if (addConn.connectionToClient.connectionId == 0) continue;

            playerConn.Add(addConn.connectionToClient);
        }
        //全員に対してレート加算&保存を通知
        foreach (var player in playerConn) {
            int addRate = CalculatePersonalRate(player,_winnerTeamID);
            AddAndSaveRate(player, addRate);
        }
    }

    /// <summary>
    /// データのセーブ
    /// </summary>
    /// <param name="_newRate"></param>
    private void SavePlayerData(int _newRate) {
        playerData.currentRate = _newRate;
        PlayerSaveData.Save(playerData);
    }

    /// <summary>
    /// 計算したレートをデータに加え、保存までを対象に通知
    /// </summary>
    /// <param name="_addRate">加える値</param>
    [TargetRpc]
    private void AddAndSaveRate(NetworkConnectionToClient _winnerConn, int _addRate) {
        SavePlayerData(playerData.currentRate + _addRate);
        //テスト
        ChatManager.instance.CmdSendSystemMessage($"{playerData.playerName}'s Rate : {playerData.currentRate}");
        
    }
}
