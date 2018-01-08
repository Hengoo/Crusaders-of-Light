using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "cons_allies_average_health_within_radius", menuName = "Combat/AI/ConsAlliesAverageHealthWithinRadius", order = 16)]
public class ConsAlliesAverageHealthWithinRadius : Consideration {

    [Header("Consideration Allies Average Health Within Radius:")]
    public float MinDistance = 0.0f;
    public float MaxDistance = 30.0f;

    public float MinHealth = 0.0f;
    public float MaxHealth = 0.0f;

    public override float CalculateScore(Context SkillContext)
    {
        List<Character> AlliesInRange = SkillContext.User.GetAttention().GetEnemiesInAttentionRange(MinDistance, MaxDistance);

        float InputValue = 0;

        for (int i = 0; i < AlliesInRange.Count; i++)
        {
            InputValue += AlliesInRange[i].GetHealthCurrentPercentage();
        }

        InputValue = InputValue / AlliesInRange.Count;

        InputValue = ClampInputValue(InputValue, MinHealth, MaxHealth);

        float score = CalculateConsideration(TypeOfCurve, InputValue, Steepness, yShift, xShift);

        return score;
    }
}
