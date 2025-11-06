using Mirror;
using UnityEngine;
using System.Collections.Generic;
using static TeamData;
/// <summary>
/// ServerManager
/// サーバー側での処理を管理するクラス(Playerのステータス更新やオブジェクトの生成等)
/// </summary>
public class ServerManager : NetworkBehaviour {
    public static ServerManager instance = null;
    [Header("現在接続している人数")]
    public List<NetworkIdentity> connectPlayer = null;
    [Header("チームデータの総数")]
    public List<TeamData> teams = null;
    private void Awake() {
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// このタイミングで必要なマネージャーを全て生成する
    /// </summary>
    public override void OnStartServer() {
        //リストを生成して、新しいデータを追加
        int teamMax = (int)TeamColor.ColorMax;
        teams = new List<TeamData>(teamMax);
        for (int i = 0; i < teamMax; i++) {
            teams.Add(new TeamData());
        }
    }

    /// <summary>
    /// 参加者をランダムなチームに振り分ける
    /// </summary>
    /// <param name="_teamCount"></param>
    private void JoinRandomTeam(int _teamCount = 2) {
        if (teams.Capacity != 0) {
            //まずはチームを全てリセット
            foreach (var resetTeam in teams) {
                for (int i = 0, max = resetTeam.teamPlayerList.Count; i < max; i++) {
                    resetTeam.teamPlayerList[i].GetComponent<GeneralCharacter>().TeamID = -1;
                }
                resetTeam.teamPlayerList.Clear();
                PlayerUIController.instance.ResetTeammateUI();
            }

        }
        teams = new List<TeamData>(TEAMMATE_MAX);
        //ここで新たにチームを生成(PlayerのteamIDも設定しなおし)
        for (int i = 0; i < _teamCount; i++) {
            TeamData newTeam = new TeamData();
            teams.Add(newTeam);
        }
        for (int i = 0, max = connectPlayer.Count; i < max; i++) {
            var player = connectPlayer[i];
            int teamIndex = Random.Range(0, teams.Count);
            //チームが既定の人数を超えていたら一個次のチームに入れる(チームの最後尾なら先頭のチームに加入)
            if (teams[teamIndex].teamPlayerList.Count >= TEAMMATE_MAX) {
                teams[(teamIndex + 1) % teams.Count].teamPlayerList.Add(connectPlayer[i]);

            }
            //それ以外はランダムにぶち込む
            else {
                teams[teamIndex].teamPlayerList.Add(connectPlayer[i]);
            }
            //プレイヤーのチームIDやUIを設定
            player.GetComponent<GeneralCharacter>().TeamID = teamIndex;
            ChatManager.instance.CmdSendSystemMessage(player.GetComponent<GeneralCharacter>().PlayerName + "は" + teamIndex + "番目のチームに入りました!");
        }

    }
    /// <summary>
    /// ランダムチーム生成
    /// </summary>
    public void RandomTeamDecide() {
        JoinRandomTeam();
    }
}
