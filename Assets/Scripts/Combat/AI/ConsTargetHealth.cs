using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "cons_target_health", menuName = "Combat/AI/ConsTargetHealth", order = 11)]
public class ConsTargetHealth : Consideration {

    [Header("Consideration Target Distance:")]
    public float MinHealthPerc = 0.0f;
    public float MaxHealthPerc = 1.0f;

    public override float CalculateScore(Context SkillContext)
    {
        float InputValue = SkillContext.Target.GetHealthCurrentPercentage();

        InputValue = ClampInputValue(InputValue, MinHealthPerc, MaxHealthPerc);

        float score = CalculateConsideration(TypeOfCurve, InputValue, Steepness, yShift, xShift);

        return score;
    }
}
