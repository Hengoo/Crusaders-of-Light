using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "cons_target_defense", menuName = "Combat/AI/ConsTargetDefense", order = 12)]
public class ConsTargetDefense : Consideration {

    [Header("Consideration Target Defense:")]
    public Character.Defense DefenseType = Character.Defense.NONE; // None always returns Input Value 0.
    public float MinDefense = 0.0f;
    public float MaxDefense = 30.0f;

    public override float CalculateScore(Context SkillContext)
    {
        float InputValue = SkillContext.Target.GetDefense(DefenseType);

        InputValue = ClampInputValue(InputValue, MinDefense, MaxDefense);

        float score = CalculateConsideration(TypeOfCurve, InputValue, Steepness, yShift, xShift);

        return score;
    }
}
