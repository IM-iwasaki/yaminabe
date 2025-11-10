using Mirror;
using System.Collections.Generic;
/// <summary>
/// チームデータ
/// </summary>
public class TeamData{
    /// <summary>
    /// チーム判別用列挙定数
    /// </summary>
    public enum TeamColor {
        Invalid = -1,
        Red,
        Blue,

        ColorMax,
    }
    public List<NetworkIdentity> teamPlayerList = new List<NetworkIdentity>(TEAMMATE_MAX);
    public const int TEAMMATE_MAX = 3;
}
