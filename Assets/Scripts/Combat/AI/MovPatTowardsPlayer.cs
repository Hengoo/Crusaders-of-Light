using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "move_pattern_towards_player", menuName = "Combat/AI/MovPatTowardsPlayer", order = 3)]
public class MovPatTowardsPlayer : MovePattern {

    public override void UpdateMovePattern(Character Self, Character TargetCharacter)
    {
        Debug.Log("Move Pattern: " + Self + " : MOVE TOWARDS: " + TargetCharacter);
    }
}
