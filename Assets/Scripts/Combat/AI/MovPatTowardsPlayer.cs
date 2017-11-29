using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "move_pattern_towards_player", menuName = "Combat/AI/MovPatTowardsPlayer", order = 3)]
public class MovPatTowardsPlayer : MovePattern {

    public float MovementSpeedFactor = 10;

    public override void UpdateMovePattern(PhysicsController PhysCont, Character Self, Character TargetCharacter)
    {
        Vector3 targetDir = Vector3.Normalize(
            new Vector3(TargetCharacter.transform.position.x, 0, TargetCharacter.transform.position.z) 
            - new Vector3(Self.transform.position.x, 0, Self.transform.position.z));

        Vector3 targetVel = targetDir * MovementSpeedFactor;

        PhysCont.SetVelRot(targetVel, targetDir);
    }
}
