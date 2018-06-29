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
        }
    }

    public override void SwarmlingFinishedAttack()
    {
        DoNotMove = false;

      //  AttractionDistance = AttractionDistanceMax / 2;
      //  AttractionDistanceMax = AttractionDistance;
    }

}
