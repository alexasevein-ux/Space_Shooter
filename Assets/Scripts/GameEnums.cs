using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnums : MonoBehaviour
{
    public enum PowerUpType
    {
        None,
        TripleShot,
        SpeedBoost,
        HomingShots,
        Shield,
        Health,
        Ammo,
    }

    public enum DebuffType
    {
        Slow,
        Freeze,
        ReverseControls,
    }
}