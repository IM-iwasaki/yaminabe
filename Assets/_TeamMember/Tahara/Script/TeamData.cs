using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamData{
    public enum teamColor {
        Invalid = -1,
        Red,
        Blue,
        Green,
        Yellow,
        Black,
        White,

        ColorMax,
    }
    public List<NetworkIdentity> teamPlayerList;

    int Score;
}
