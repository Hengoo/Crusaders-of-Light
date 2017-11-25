using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "cons_target_distance", menuName = "Combat/AI/ConsTargetDistance", order = 1)]
public class ConsTargetDistance : Consideration {

    [Header("Consideration Target Distance:")]
    public float MinDistance = 0.0f;
    public float MaxDistance = 30.0f;

    public override float CalculateScore(Context SkillContext)
    {
        float InputValue = Vector3.Distance(SkillContext.user.transform.position, SkillContext.target.transform.position);

        InputValue = ClampInputValue(InputValue, MinDistance, MaxDistance);

        float score = CalculateConsideration(TypeOfCurve, InputValue, Steepness, yShift, xShift);

        return score;
    }

    private float CalcInputValue(Context SkillContext)
    {
        float Distance = Vector3.Distance(SkillContext.user.transform.position, SkillContext.target.transform.position);

        return Distance;
    }
}
