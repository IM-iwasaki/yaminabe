using Mirror;
using UnityEngine;
using System.Collections.Generic;
using static TeamData;
using TMPro;
/// <summary>
/// ServerManager
/// サーバー側での処理を管理するクラス(Playerのステータス更新やオブジェクトの生成等)
/// </summary>
public class ServerManager : NetworkBehaviour {
    public static ServerManager instance = null;
    public List<NetworkIdentity> connectPlayer = null;
    public List<TeamData> teams = null;
    bool isSkip = false;
    [SerializeField]
    TextMeshProUGUI text = null;

    bool joinRedTeam = false;

    private void Awake() {
        instance = this;
    }

    /// <summary>
    /// このタイミングで必要なマネージャーを全て生成する
    /// </summary>
    public override void OnStartServer() {
        //リストを生成して、新しいデータを追加
        int teamMax = (int)teamColor.ColorMax;
        teams = new List<TeamData>(teamMax);
        for(int i = 0;i < teamMax; i++) {
            teams.Add(new TeamData());
        }
    }

    public void Update() {
        text.text = connectPlayer.Count.ToString();

        if (joinRedTeam) {
            joinRedTeam = false;
        }
    }

    /// <summary>
    /// チーム生成
    /// </summary>
    private void CreateTeam() {
        //ランダム
        if (isSkip) {
            JoinRandomTeam();
        }
        //任意のチーム

    }

    private void JoinRandomTeam(int _teamCount = 2) {
        //まずはチームを全てリセット
        foreach (var resetTeam in teams) {
            for (int i = 0, max = resetTeam.teamPlayerList.Count; i < max; i++) {
                resetTeam.teamPlayerList[i].GetComponent<DemoPlayer>().TeamID = -1;
            }
            resetTeam.teamPlayerList.Clear();
        }
        teams.Clear();
        //ここで新たにチームを生成(PlayerのteamIDも設定しなおし)
        for (int i = 0; i < _teamCount; i++) {
            teams.Add(new TeamData());
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
            //プレイヤーのチームIDを設定
            player.GetComponent<DemoPlayer>().TeamID = teamIndex;
            Debug.Log(player + "は" + teamIndex + "番目のチームに入りました!");
        }

    }

    [Command]
    public void CmdJoinTeam(NetworkIdentity _player, teamColor _color) {
        CharacterBase player = _player.GetComponent<CharacterBase>();
        int currentTeam = player.TeamID;
        int newTeam = (int)_color;

        //加入しようとしてるチームが埋まっていたら
        if (teams[(newTeam)].teamPlayerList.Count >= TEAMMATE_MAX) {
            Debug.Log("チームの人数が最大です！");
            return;
        }
        //既に同じチームに入っていたら
        if (newTeam == currentTeam) {
            Debug.Log("今そのチームにいます!");
            return;
        }
        //新たなチームに加入する時
        //今加入しているチームから抜けてIDをリセット
        teams[_player.GetComponent<CharacterBase>().TeamID].teamPlayerList.Remove(_player);
       player.TeamID = -1;
        //新しいチームに加入
        teams[newTeam].teamPlayerList.Add(_player);
        player.TeamID = newTeam;
        //ログを表示
        Debug.Log(_player.ToString() + "は" + newTeam + "番目のチームに加入しました！");
    }

    //デバッグ用チーム振り分け確認
    public void RandomTeamDecide() {
        isSkip = true;
        CreateTeam();
    }




}
