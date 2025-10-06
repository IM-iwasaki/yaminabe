using UnityEngine;
using Mirror;

/// <summary>
/// プレイヤーのチーム情報(仮)
/// </summary>
public class PlayerTeamInfo : NetworkBehaviour {
    [SyncVar] public int teamId = 0;  // チームID
}