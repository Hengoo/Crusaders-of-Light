using System.Collections;
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

    public float DesiredGoToBorderSpeed = 16;

    public override void SwarmlingAttackRuleCalculation()
    {
        if (!DoNotMove && ClosestPlayer && ClosestPlayerSqrDistance < Mathf.Pow(AttackDistance, 2))
        {
            DoNotMove = true;

            ThisSwarmlingCharacter.SwarmlingStartSkillActivation();

            SwarmlingTransform.rotation = Quaternion.LookRotation(ClosestPlayer.transform.position - SwarmlingTransform.position);
        }
    }

    public override void SwarmlingFinishedAttack()
    {
        DoNotMove = false;
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
