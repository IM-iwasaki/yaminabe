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
    public List<NetworkIdentity> connectPlayer = null;
    public List<TeamData> teams = null;
    private int maxTeamPlayer;
    bool isSkip = false;
    public override void OnStartServer() {
        instance = this;
        CreateTeam();
    }

    /// <summary>
    /// チーム生成
    /// </summary>
    private void CreateTeam() {
        teams = new List<TeamData>((int)teamColor.ColorMax);
        if (teams.Count == 0) maxTeamPlayer = 1;
        else
            maxTeamPlayer = connectPlayer.Count / teams.Count;
        //ランダム
        if (isSkip) {
            JoinRandomTeam();
        }
        //任意のチーム
        else {

        }
    }

    private void JoinRandomTeam(int _teamCount = 2) {
        //まずはチームを全てリセット
        foreach (var resetTeam in teams) {
            for (int i = 0, max = resetTeam.teamPlayerList.Count; i < max; i++) {
                //resetTeam.teamPlayerList[i].GetComponent<PlayerBase>().teamID = -1;
            }
            resetTeam.teamPlayerList.Clear();
        }
        teams.Clear();
        //ここで新たにチームを生成(PlayerのteamIDも設定しなおし)
        for(int i = 0; i < _teamCount; i++) {
            teams.Add(new TeamData());
        }
        for (int i = 0, max = connectPlayer.Count; i < max; i++) {
            int teamIndex = Random.Range(0, teams.Count);
            //チームが既定の人数を超えていたら一個次のチームに入れる
            if (teams[teamIndex].teamPlayerList.Count >= maxTeamPlayer) {
                teams[(teamIndex + 1) % teams.Count].teamPlayerList.Add(connectPlayer[i]);
            }
            //それ以外はランダムにぶち込む
            else {
                teams[teamIndex].teamPlayerList.Add(connectPlayer[i]);
            }
                
            //teams[teamIndex].teamPlayerList[i].GetComponent<PlayerBase>().teamID = teamIndex;
        }

    }

    [Command]
    private void CmdJoinTeam(teamColor _color, NetworkIdentity _player) {
        teams[(int)_color].teamPlayerList.Add(_player);
    }
}
