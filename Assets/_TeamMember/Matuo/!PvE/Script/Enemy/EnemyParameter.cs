using UnityEngine;
using Mirror;

public class EnemyParameter : NetworkBehaviour {
    [SyncVar]
    public int HP;

    [SyncVar]
    public int attack;

    [SyncVar]
    public int moveSpeed;

    [SyncVar]
    public bool isDead = false;

    [SyncVar] public int TeamID = -1;
}