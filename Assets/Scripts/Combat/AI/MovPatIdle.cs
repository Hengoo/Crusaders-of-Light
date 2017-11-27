using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "move_pattern_idle", menuName = "Combat/AI/MovPatIdle", order = 2)]
public class MovPatIdle : MovePattern {

    public override void UpdateMovePattern(Character Self, Character Target)
    {
        Debug.Log("Move Pattern: " + Self + " : IDLE");
    }
}
