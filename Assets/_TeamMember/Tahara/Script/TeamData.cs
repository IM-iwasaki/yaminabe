using Mirror;
using System.Collections.Generic;

public class TeamData{
    //ƒ`[ƒ€”»•Ê—p—ñ‹“’è”
    public enum TeamColor {
        Invalid = -1,
        Red,
        Blue,

        ColorMax,
    }
    public List<NetworkIdentity> teamPlayerList = new List<NetworkIdentity>(TEAMMATE_MAX);
    public const int TEAMMATE_MAX = 3;
}
