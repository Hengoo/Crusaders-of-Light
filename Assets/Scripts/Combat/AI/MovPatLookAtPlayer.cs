using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "move_pattern_look_at_player", menuName = "Combat/AI/MovPatLookAtPlayer", order = 4)]
public class MovPatLookAtPlayer : MovePattern {

    [Header("View Direction (Negativ : Away from Player)")]
    public int ViewDirection = 1;

    public float RotationSpeed = 5;

    public override void UpdateMovePattern(PhysicsController PhysCont, CharacterEnemy Self, Character TargetCharacter)
    {
        if (!TargetCharacter)
        {
            Self.UpdateMovePatternForMissingTarget();
            return;
        }

        Vector3 targetDir = Vector3.Normalize(
            new Vector3(TargetCharacter.transform.position.x, 0, TargetCharacter.transform.position.z) * ViewDirection
            - new Vector3(Self.transform.position.x, 0, Self.transform.position.z) * ViewDirection);

        PhysCont.SetVelRot(Vector3.zero, targetDir);

        Self.SwitchWalkingAnimation(false);
    }

    public override void UpdateMovePattern(NavMeshAgent NavAgent, CharacterEnemy Self, Character TargetCharacter)
    {
        if (!TargetCharacter)
        {
            Self.UpdateMovePatternForMissingTarget();
            return;
        }

        Vector3 targetDir = Vector3.Normalize(
            new Vector3(TargetCharacter.transform.position.x, 0, TargetCharacter.transform.position.z) * ViewDirection
            - new Vector3(Self.transform.position.x, 0, Self.transform.position.z) * ViewDirection);

        if (targetDir.sqrMagnitude > 0)
        {
            Self.transform.rotation = Quaternion.Slerp(Self.transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * RotationSpeed * 0.01f);
        }

        Self.SwitchWalkingAnimation(false);
    }
}
