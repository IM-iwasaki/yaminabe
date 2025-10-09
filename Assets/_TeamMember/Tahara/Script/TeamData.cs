using Mirror;
using System.Collections.Generic;

public class TeamData{
    public enum teamColor {
        Invalid = -1,
        Red,
        Blue,

        ColorMax,
    }
    
    public List<NetworkIdentity> teamPlayerList = new List<NetworkIdentity>(TEAMMATE_MAX);
    public const int TEAMMATE_MAX = 3;
    int Score;
    private void ChangedTeammate(List<NetworkIdentity> _oldList,List<NetworkIdentity> _newList) {
        for(int i = 0,max = _newList.Count; i < max; i++) {
            PlayerUIManager.instance.CreateTeammateUI(teamPlayerList[i]);
        }
    }
}
