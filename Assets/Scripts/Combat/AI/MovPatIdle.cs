using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "move_pattern_idle", menuName = "Combat/AI/MovPatIdle", order = 2)]
public class MovPatIdle : MovePattern {

    public override void UpdateMovePattern(PhysicsController PhysCont, CharacterEnemy Self, Character TargetCharacter)
    {
        PhysCont.SetVelRot(Vector3.zero, Vector3.zero);

        Self.SwitchWalkingAnimation(false);
    }

    public override void UpdateMovePattern(NavMeshAgent NavAgent, CharacterEnemy Self, Character TargetCharacter)
    {
        Self.SwitchWalkingAnimation(false);
    }

}
