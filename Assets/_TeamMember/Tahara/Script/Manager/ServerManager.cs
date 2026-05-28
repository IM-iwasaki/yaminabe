using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// ServerManager
/// サーバー側での処理を管理するクラス
/// </summary>
public class ServerManager : NetworkBehaviour {
    /// <summary>
    /// インスタンス
    /// </summary>
    public static ServerManager instance = null;
    [Header("現在接続している人数")]
    public readonly SyncList<NetworkIdentity> connectPlayer = new SyncList<NetworkIdentity>();
    [Header("チームデータの総数")]
    public List<TeamData> teams = null;

    public int teammateMax { get; private set; } = 1;
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// サーバーが生成されたタイミングで処理
    /// </summary>
    public override void OnStartServer() {
        //リストを生成して、新しいデータを追加
        int teamMax = (int)TeamData.TeamColor.ColorMax;
        teams = new List<TeamData>(teamMax);
        for (int i = 0; i < teamMax; i++) {
            teams.Add(new TeamData());
        }

        PlayerListUIManager.Instance?.ShowUI();

    }

    /// <summary>
    /// サーバー終了時処理
    /// </summary>
    public override void OnStopServer() {
        connectPlayer.Clear();
    }

    /// <summary>
    /// プレイヤーをランダムなチームに振り分ける
    /// </summary>
    /// <param name="_allRandomTeam"></param>
    private void JoinRandomTeam() {
        //チームに所属していない人を抜き出す
        List<NetworkIdentity> allPlayers = new List<NetworkIdentity>(connectPlayer);

        foreach (var player in allPlayers)
            player.GetComponent<GeneralCharacter>().parameter.TeamID = -1;

        //未所属プレイヤーを抽出
        List<NetworkIdentity> noTeamPlayer = allPlayers.Where(p => p.GetComponent<GeneralCharacter>().parameter.TeamID == -1).ToList();

        teams = new List<TeamData>();
        //ここで新たにチームを生成(PlayerのteamIDも設定しなおし)
        for (int i = 0; i < (int)TeamData.TeamColor.ColorMax; i++) {
            teams.Add(new TeamData());
            //チームリストを作り直したので既にチーム所属済みなら同じチームに設定しなおし(Redから入れ始めるので)
            foreach (var player in allPlayers) {
                if (player.GetComponent<GeneralCharacter>().parameter.TeamID == i) {
                    teams[i].teamPlayerList.Add(player);
                }
                if (teams[i].teamPlayerList.Count == teammateMax)
                    teams[i].isFullTeam = true;
            }
        }

        //未所属プレイヤーをシャッフル
        noTeamPlayer = noTeamPlayer.OrderBy(x => Random.value).ToList();
        int teamIndex = 0;
        //均等に割り振り
        if (teams[(int)TeamData.TeamColor.Red].teamPlayerList.Count > teams[(int)TeamData.TeamColor.Blue].teamPlayerList.Count)
            teamIndex = 1;
        foreach (var player in noTeamPlayer) {
            //所属しようとするチームが満員なら
            if (teams[teamIndex].isFullTeam) {
                //全員空いてるチームにぶち込む
                teams[(teamIndex + 1) % teams.Count].teamPlayerList.Add(player);
                continue;
            }
            teams[teamIndex].teamPlayerList.Add(player);
            player.GetComponent<GeneralCharacter>().parameter.TeamID = teamIndex;
            player.GetComponent<GeneralCharacter>().teamID = teamIndex;
            teamIndex = (teamIndex + 1) % teams.Count;
        }
    }
    /// <summary>
    /// ランダムチーム生成
    /// </summary>
    public void RandomTeamDecide() {
        JoinRandomTeam();
    }

    /// <summary>
    /// ホスト切断時にチームのリストをリセットする関数
    /// </summary>
    [Server]
    public void ResetTeamList() {
        foreach (var team in teams) {
            team.teamPlayerList.Clear();
        }
    }

    /// <summary>
    /// チームから除籍
    /// </summary>
    /// <param name="_identity"></param>
    [Server]
    public void RemoveTeammate(NetworkIdentity _identity)
    {
        //所属チームを抜ける
        int currentTeam = _identity.GetComponent<GeneralCharacter>().parameter.TeamID;
        //無所属なら処理しない
        if (currentTeam < 0)
            return;
        teams[currentTeam].teamPlayerList.Remove(_identity);
    }

    /// <summary>
    /// 追加 マツオ：全プレイヤーをチーム0に設定(PVE用)
    /// </summary>
    [Server]
    public void SetAllPlayersToPvETeam() {
        // チーム初期化
        teams = new List<TeamData>();
        for (int i = 0; i < (int)TeamData.TeamColor.ColorMax; i++) {
            teams.Add(new TeamData());
        }

        // 全員チーム0へ
        foreach (var player in connectPlayer) {
            var character = player.GetComponent<GeneralCharacter>();
            character.parameter.TeamID = 0;
            character.teamID = 0;
            teams[0].teamPlayerList.Add(player);
        }

        teams[0].isFullTeam = false;
    }

    /// <summary>
    /// 全員のHP、弾数状態を戻す
    /// </summary>
    [Server]
    public void ResetCharacterStatus() {
        foreach (var player in connectPlayer) {
            GeneralCharacter resetPlayer = player.GetComponent<GeneralCharacter>();
            resetPlayer.TargetResetStatus(player.connectionToClient);
            resetPlayer.parameter.TeamID = -1;
            resetPlayer.teamID = -1;
            //万が一の死亡状態解除
            resetPlayer.ResetHealth();
            // 追加 マツオ : 武器リセット用
            var param = player.GetComponent<GeneralCharacter>().parameter;
            param.ResetWeaponToDefault();
        }
    }

    /// <summary>
    /// 全員のHP、弾数状態を戻す
    /// </summary>
    [Server]
    public void ResetCharacterStatusOnGameStart() {
        foreach (var player in connectPlayer) {
            GeneralCharacter resetPlayer = player.GetComponent<GeneralCharacter>();
            resetPlayer.TargetResetStatus(player.connectionToClient);
            // 万が一の死亡状態解除
            resetPlayer.ResetHealth();
            // 追加 マツオ : 武器リセット用
            var param = player.GetComponent<GeneralCharacter>().parameter;
            param.ResetWeaponToDefault();
        }
    }

    /// <summary>
    /// 参加人数に応じて1チームの最大人数を変更
    /// </summary>
    [Server]
    public void ChangeTeammateMax() {
        teammateMax = Mathf.CeilToInt((float)connectPlayer.Count / teams.Count);
    }
}
