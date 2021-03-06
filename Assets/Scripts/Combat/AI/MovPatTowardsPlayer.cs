﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "move_pattern_towards_player", menuName = "Combat/AI/MovPatTowardsPlayer", order = 3)]
public class MovPatTowardsPlayer : MovePattern {

    [Header("View Direction (Negativ : Away from Player)")]
    public int ViewDirection = 1;
    [Header("Movement Speed (Negativ : Backwards)")]
    public float MovementSpeedFactor = 10;
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

        Vector3 targetVel = targetDir * MovementSpeedFactor * Self.GetMovementRateModifier();

        PhysCont.SetVelRot(targetVel, targetDir);

        Self.SwitchWalkingAnimation(true);
        Self.StartBodyAnimation(Self.GetMovementRateModifier());
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

        Vector3 targetVel = targetDir * MovementSpeedFactor * Self.GetMovementRateModifier();

        //NavAgent.Move(targetVel * Time.deltaTime);
        NavAgent.speed = MovementSpeedFactor * Self.GetMovementRateModifier();
        NavAgent.SetDestination(TargetCharacter.transform.position);


        Self.SwitchWalkingAnimation(true);
        Self.StartBodyAnimation(Self.GetMovementRateModifier());
    }
}
