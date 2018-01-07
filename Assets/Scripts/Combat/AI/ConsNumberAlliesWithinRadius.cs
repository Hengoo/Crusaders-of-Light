using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "cons_number_allies_within_radius", menuName = "Combat/AI/ConsNumberAlliesWithinRadius", order = 15)]
public class ConsNumberAlliesWithinRadius : Consideration {

    [Header("Consideration Number Allies Within Radius:")]
    public float MinDistance = 0.0f;
    public float MaxDistance = 30.0f;

    public float MinNumber = 0.0f;
    public float MaxNumber = 0.0f;

    public override float CalculateScore(Context SkillContext)
    {
        float InputValue = SkillContext.User.GetAttention().GetEnemiesInAttentionRange(MinDistance, MaxDistance).Count;

        InputValue = ClampInputValue(InputValue, MinNumber, MaxNumber);

        float score = CalculateConsideration(TypeOfCurve, InputValue, Steepness, yShift, xShift);

        return score;
    }
}
