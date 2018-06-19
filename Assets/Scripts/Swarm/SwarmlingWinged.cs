using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmlingWinged : EnemySwarm {

    [Header("Swarmling Wings:")]
    public float AttackDistance = 1;

    [Header("Swarmling Wings Special Movement:")]
    public Transform SwarmlingBodyTransform;

    public float SwarmlingFlightTimer;
    public float SwarmlingFlightCounter;

    public float SwarmlingFlightUpwardsTimeEnd = 0.5f;
    public float SwarmlingFlightUpwardsSpeed = 10f;

    public float SwarmlingFlightTowardsTargetTimeEnd = 1.5f;
    public float SwarmlingFlightTowardsTargetSpeed = 10f;
    public float SwarmlingFlightTowardsCounter = 0f;

    public bool TargetPointCalculated = false;
    public Vector3 TargetPoint = Vector3.zero;
    public Vector3 StartingPoint = Vector3.zero;
    //public Vector3 FlyingVector = Vector3.zero;
    //public Vector3 FlyingPosition = Vector3.zero;
    //public float FlyingTime = 1f;

    public bool HasAttacked = false;

    public override void SwarmlingAttackRuleCalculation()
    {
        if (!DoNotMove)
        {
            if (ClosestPlayer && ClosestPlayerSqrDistance < Mathf.Pow(AttackDistance, 2))
            {
                DoNotMove = true;

                //ThisSwarmlingCharacter.SwarmlingStartSkillActivation();

                SwarmlingTransform.rotation = Quaternion.LookRotation(ClosestPlayer.transform.position - SwarmlingTransform.position);
            }
            return;
        }

        SwarmlingFlightCounter += Time.deltaTime;

        if (SwarmlingFlightCounter <= SwarmlingFlightUpwardsTimeEnd)
        {
            SwarmlingBodyTransform.position += Vector3.up * SwarmlingFlightUpwardsSpeed * Time.deltaTime;
            //SwarmlingTransform.rotation = Quaternion.LookRotation(ClosestPlayer.transform.position - SwarmlingTransform.position);
        }
        else if (SwarmlingFlightTowardsCounter <= 1)
        {
            if (!TargetPointCalculated)
            {
                TargetPointCalculated = true;
                TargetPoint = ClosestPlayer.transform.position; // - SwarmlingBodyTransform.position;
                StartingPoint = SwarmlingBodyTransform.position;
                //TargetPoint = TargetPoint.normalized * SwarmlingFlightTowardsTargetSpeed;
                //FlyingVector = TargetPoint - StartingPoint;
                //FlyingTime = Vector3.Distance(StartingPoint, TargetPoint) / SwarmlingFlightTowardsTargetSpeed;
                //FlyingPosition = SwarmlingBodyTransform.position;

                SwarmlingTransform.rotation = Quaternion.LookRotation(ClosestPlayer.transform.position - SwarmlingTransform.position);
            }

            SwarmlingFlightTowardsCounter += Time.deltaTime * SwarmlingFlightTowardsTargetSpeed;

            //SwarmlingBodyTransform.position = Vector3.Lerp(StartingPoint, TargetPoint, SwarmlingFlightTowardsCounter);

            //FlyingPosition = Vector3.MoveTowards(FlyingPosition, TargetPoint, Time.deltaTime * SwarmlingFlightTowardsTargetSpeed);

            SwarmlingBodyTransform.position = Vector3.Lerp(StartingPoint, TargetPoint, SwarmlingFlightTowardsCounter);
            SwarmlingBodyTransform.position += Vector3.up * Mathf.Sin((SwarmlingFlightTowardsCounter) * Mathf.PI) * 2;

            // Debug.Log("SIN: " + Mathf.Sin(SwarmlingFlightTowardsCounter * Mathf.PI) + " at: " + SwarmlingFlightTowardsCounter);
        }
        else if (!HasAttacked)
        {
            HasAttacked = true;
            ThisSwarmlingCharacter.SwarmlingStartSkillActivation();
        }

    }

    public override void SwarmlingFinishedAttack()
    {
        DoNotMove = false;
        HasAttacked = false;
        TargetPointCalculated = false;

        SwarmlingFlightCounter = 0;
        SwarmlingFlightTowardsCounter = 0;

        NMAgent.Warp(TargetPoint);
        SwarmlingBodyTransform.localPosition = Vector3.zero;
    }
}
