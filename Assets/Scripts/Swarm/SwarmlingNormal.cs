using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmlingNormal : EnemySwarm {

    [Header("Swarmling Normal:")]
    public float AttackDistance = 1;

    public override void SwarmlingAttackRuleCalculation()
    {
        if (!DoNotMove && !ScaredOfPlayer && ClosestPlayer && ClosestPlayerSqrDistance < Mathf.Pow(AttackDistance,2))
        {
            DoNotMove = true;

            ThisSwarmlingCharacter.SwarmlingStartSkillActivation();

            SwarmlingTransform.rotation = Quaternion.LookRotation(ClosestPlayer.transform.position - SwarmlingTransform.position);

            NMAgent.avoidancePriority = 40;
        }
    }

    public override void SwarmlingFinishedAttack()
    {
        DoNotMove = false;
        NMAgent.avoidancePriority = 60;
        //  AttractionDistance = AttractionDistanceMax / 2;
        //  AttractionDistanceMax = AttractionDistance;
    }

}
