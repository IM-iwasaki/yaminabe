using Mirror;
using UnityEngine;
using System.Collections.Generic;
using static TeamData;
/// <summary>
/// ServerManager
/// サーバー側での処理を管理するクラス
/// </summary>
public class ServerManager : NetworkBehaviour {
    public static ServerManager instance = null;
    [Header("現在接続している人数")]
    public List<NetworkIdentity> connectPlayer = null;
    [Header("チームデータの総数")]
    public List<TeamData> teams = null;

    [System.NonSerialized] public bool isRandom = false;
    private void Awake() {
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// サーバーが生成されたタイミングで処理
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
    /// プレイヤーをランダムなチームに振り分ける
    /// </summary>
    /// <param name="_allRandomTeam"></param>
    private void JoinRandomTeam(bool _allRandomTeam = false) {
        //チームに所属していない人だけver
        List<NetworkIdentity> notInTeamPlayer = new List<NetworkIdentity>();
        if (!_allRandomTeam) {
            //チームに所属していない人を抜き出す
            foreach (var player in connectPlayer) {
                if (player.GetComponent<GeneralCharacter>().TeamID != -1)
                    continue;
                notInTeamPlayer.Add(player);
            }
        }
        //全員ランダムver
        else {
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
            for (int i = 0; i < (int)TeamColor.ColorMax; i++) {
                TeamData newTeam = new TeamData();
                teams.Add(newTeam);
            }
        }
        for (int i = 0, max = notInTeamPlayer.Count; i < max; i++) {
            var player = notInTeamPlayer[i];
            int teamIndex = Random.Range(0, teams.Count);
            //チームが既定の人数を超えていたら一個次のチームに入れる(チームの最後尾なら先頭のチームに加入)
            if (teams[teamIndex].teamPlayerList.Count >= TEAMMATE_MAX) {
                teams[(teamIndex + 1) % teams.Count].teamPlayerList.Add(player);
                teamIndex = (teamIndex + 1) % teams.Count;
            }
            //それ以外はランダムにふりわける
            else {
                teams[teamIndex].teamPlayerList.Add(player);
            }
            //プレイヤーのチームIDやUIを設定
            player.GetComponent<GeneralCharacter>().TeamID = teamIndex;
            ChatManager.instance.CmdSendSystemMessage(player.GetComponent<GeneralCharacter>().PlayerName + " is " + teamIndex + "Team");
        }

    }
    /// <summary>
    /// ランダムチーム生成
    /// </summary>
    public void RandomTeamDecide() {
        JoinRandomTeam(isRandom);
    }
}
