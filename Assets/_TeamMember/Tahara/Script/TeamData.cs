using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamData{
    public enum teamColor {
        Invalid = -1,
        Red,
        Blue,

        ColorMax,
    }
    public List<NetworkIdentity> teamPlayerList = new List<NetworkIdentity>(TEAMMATE_MAX);
    private const int TEAMMATE_MAX = 3;
    int Score;
}
