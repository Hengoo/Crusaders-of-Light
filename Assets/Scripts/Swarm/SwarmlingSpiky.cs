using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmlingSpiky : EnemySwarm {

    [Header("Swarmling Spiky:")]
    public float AttackDistance = 1;

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
}
