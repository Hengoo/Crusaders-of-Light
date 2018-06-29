using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmlingWinged : EnemySwarm {

    [Header("Swarmling Wings:")]
    public float AttackDistance = 1;
    public float AttackDistanceMin = 5;
    public float AttackTargetMoveRadius = 3;

    public float AttackCooldown = 1f;
    public float AttackCooldownCounter = -1f;

    public float TargetSearchTime = 1f;
    public float TargetSearchTimeCounter = -1f;

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
    public Transform TargetTransform;
    //public Vector3 FlyingVector = Vector3.zero;
    //public Vector3 FlyingPosition = Vector3.zero;
    //public float FlyingTime = 1f;


    public bool HasAttacked = false;

    [Header("Animation:")]
    public string AnimFlyUp = "Fly_Up";
    public string AnimFly = "Fly";

    public override void SwarmlingUpdate()
    {
        base.SwarmlingUpdate();

        UpdateWingAttack();
    }

    // This is updated every few frames, along with the other basic and special rules:
    public override void SwarmlingAttackRuleCalculation()
    {
        // Check if normal Movement is allowed (= not currently attacking):
        if (!DoNotMove)
        {
            if (!TargetTransform && AttackCooldownCounter <= 0 && ClosestPlayer && ClosestPlayerSqrDistance < Mathf.Pow(AttackDistance, 2) && ClosestPlayerSqrDistance >= Mathf.Pow(AttackDistanceMin, 2))
            {
                TargetTransform = ClosestPlayer.transform;
                TargetPoint = TargetTransform.position;
                TargetSearchTimeCounter = TargetSearchTime;
                DoNotMove = true;

                IgnoreThisSwarmlingForOthers = true;
            }
        }
        // Observe the current target, and stop if it moves too far away from its original location:
        else if (TargetSearchTimeCounter > 0)
        {
            //Debug.Log("Radius: " + (TargetTransform.position - TargetPoint).sqrMagnitude);
            if ((TargetTransform.position - TargetPoint).sqrMagnitude > Mathf.Pow(AttackTargetMoveRadius, 2))
            {
                TargetTransform = null;
                //TargetPoint = Vector3.zero;
                TargetSearchTimeCounter = -1f;
                DoNotMove = false;
                SwarmlingBodyTransform.localRotation = Quaternion.identity;
                IgnoreThisSwarmlingForOthers = false;
            }
        }
    }

    // This is updated every frame (required, as movement is done through this):
    private void UpdateWingAttack()
    {
        // Check if normal Movement is allowed (= not currently attacking):
        if (!DoNotMove)
        {
            // Count down Attack Cooldown:
            if (AttackCooldownCounter > 0)
            {
                AttackCooldownCounter -= Time.deltaTime;
            }
            return;
        }

        // Count down the Observation Time:
        if (TargetSearchTimeCounter > 0)
        {
            TargetSearchTimeCounter -= Time.deltaTime;

            SwarmlingBodyTransform.rotation = Quaternion.Slerp(SwarmlingBodyTransform.rotation, Quaternion.LookRotation(SwarmlingBodyTransform.position - ClosestPlayer.transform.position), 5f * Time.deltaTime);

            if (TargetSearchTimeCounter <= 0)
            {            
                //DoNotMove = true;

                //ThisSwarmlingCharacter.SwarmlingStartSkillActivation();

                SwarmlingTransform.rotation = Quaternion.LookRotation(ClosestPlayer.transform.position - SwarmlingTransform.position);

                NMAgent.enabled = false;

                ThisSwarmlingCharacter.StartAnimation(AnimFlyUp, SwarmlingFlightUpwardsTimeEnd, 0);

                AttackCooldownCounter = AttackCooldown; // Cooldown only starts counting down if !DoNotMove / Another Attack could be started.
            }

            return;
        }
        
        SwarmlingFlightCounter += Time.deltaTime;

        if (SwarmlingFlightCounter <= SwarmlingFlightUpwardsTimeEnd)
        {
            SwarmlingBodyTransform.position += Vector3.up * SwarmlingFlightUpwardsSpeed * Time.deltaTime;
            //SwarmlingTransform.rotation = Quaternion.LookRotation(ClosestPlayer.transform.position - SwarmlingTransform.position);

            SwarmlingBodyTransform.rotation = Quaternion.Slerp(SwarmlingBodyTransform.rotation, Quaternion.LookRotation(SwarmlingBodyTransform.position - ClosestPlayer.transform.position), 5f * Time.deltaTime);
        }
        else if (SwarmlingFlightTowardsCounter <= 1)
        {
            if (!TargetPointCalculated)
            {
                TargetPointCalculated = true;
                TargetPoint = ClosestPlayer.transform.position; // - SwarmlingBodyTransform.position;
                StartingPoint = SwarmlingBodyTransform.position;
                ThisSwarmlingCharacter.StartAnimation(AnimFly, 1, 0);
                //TargetPoint = TargetPoint.normalized * SwarmlingFlightTowardsTargetSpeed;
                //FlyingVector = TargetPoint - StartingPoint;
                //FlyingTime = Vector3.Distance(StartingPoint, TargetPoint) / SwarmlingFlightTowardsTargetSpeed;
                //FlyingPosition = SwarmlingBodyTransform.position;

                //SwarmlingTransform.rotation = Quaternion.LookRotation(ClosestPlayer.transform.position - SwarmlingTransform.position);
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
        NMAgent.enabled = true;

        DoNotMove = false;
        HasAttacked = false;
        TargetPointCalculated = false;

        SwarmlingFlightCounter = 0;
        SwarmlingFlightTowardsCounter = 0;

        NMAgent.Warp(TargetPoint);
        SwarmlingBodyTransform.localPosition = Vector3.zero;
        SwarmlingBodyTransform.localRotation = Quaternion.identity;

        TargetTransform = null;
        TargetSearchTimeCounter = -1f;

        IgnoreThisSwarmlingForOthers = false;

        //AttractionDistance = AttractionDistanceMin;
    }
}
