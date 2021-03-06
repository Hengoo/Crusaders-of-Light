﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmlingSpiky : EnemySwarm {

    [Header("Swarmling Spiky:")]
    public float AttackDistance = 1;

    [Header("Go To Border:")]
    public bool BorderOn = false;
    //public float OutsideAcceleration = 1;
    public float BorderDistance = 10;
    public float BorderFactor = 0.4f;
    public Vector3 BorderVec = Vector3.zero;
    public int BorderNumber = 0;

    public float AttackDelayTimeMax = 3;
    public float AttackDelayCounter = 3;

    public float AttackAngle = 0.3f;
    public float DesiredGoToBorderSpeed = 16;

    public override void SwarmlingAttackRuleCalculation()
    {
        if (DoNotMove) { return; }

        if (ClosestPlayer && ClosestPlayerSqrDistance < Mathf.Pow(AttackDistance, 2))
        {
            // Attack if player is really close, or if AttackDelayCounter is reached and Player is moving in the Beetles direction.
            if ((ClosestPlayerSqrDistance <= 4)
                || (AttackDelayCounter <= 0 && (ClosestPlayerSqrDistance < ((ClosestPlayer.transform.position - SwarmlingTransform.position).sqrMagnitude)))) //ClosestPlayerSqrDistance < ((ClosestPlayer.transform.position + ClosestPlayer.GetTargetVelocity()) - SwarmlingTransform.position).sqrMagnitude)     //&& (Vector3.Dot(ClosestPlayer.GetTargetVelocity(), (ClosestPlayer.transform.position - transform.position).normalized) < AttackAngle))
            {
                DoNotMove = true;

                ThisSwarmlingCharacter.SwarmlingStartSkillActivation();

                SwarmlingTransform.rotation = Quaternion.LookRotation(ClosestPlayer.transform.position - SwarmlingTransform.position);

                NMAgent.avoidancePriority = 40;
            }
            else
            {
                AttackDelayCounter = Mathf.Max(AttackDelayCounter - UpdateTimer, 0);
            }
        }
        else
        {
            AttackDelayCounter = Mathf.Min(AttackDelayCounter + UpdateTimer, AttackDelayTimeMax);
        }
    }

    public override void SwarmlingFinishedAttack()
    {
        DoNotMove = false;

        NMAgent.avoidancePriority = 60;
        //AttractionDistance = AttractionDistanceMin;
    }

    public override void SwarmlingSpecialRuleCalculation()
    {
        // Go to Border:
        BorderVec = Vector3.zero;
        BorderNumber = 0;

        DistanceVec = Vector3.zero;
        DistanceVecMag = 0;

        for (int i = 0; i < NeighbourCount; i++)
        {
            if (!NeighbourColliders[i])
            {
                continue;
            }

            CurrentSwarmling = NeighbourColliders[i].GetComponent<EnemySwarm>();

            DistanceVec = SwarmlingTransform.position - CurrentSwarmling.SwarmlingTransform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= 0) continue;

            if (DistanceVecMag <= Mathf.Pow(BorderDistance, 2) && CurrentSwarmling.SType != SType)
            {
                BorderVec += CurrentSwarmling.SwarmlingTransform.position;
                BorderNumber++;
            }
        }

        if (BorderNumber >= 2)
        {
            BorderVec = BorderVec / BorderNumber;
            BorderVec = SwarmlingTransform.position - BorderVec;

            BorderVec = BorderVec.normalized * DesiredGoToBorderSpeed * Mathf.Max((Mathf.Pow(BorderDistance, 2) - BorderVec.sqrMagnitude) / Mathf.Pow(BorderDistance, 2), 0);

            BorderVec = Steer(BorderVec);
            Acceleration += BorderVec * BorderFactor;
            GoalFactor += BorderFactor;

            NoSeperationThisUpdate = true;
            IgnoreThisSwarmlingForOthers = true;
            //NMAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
        }
        else if (IgnoreThisSwarmlingForOthers)
        {
            IgnoreThisSwarmlingForOthers = false;
            //NMAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }
    }
}
