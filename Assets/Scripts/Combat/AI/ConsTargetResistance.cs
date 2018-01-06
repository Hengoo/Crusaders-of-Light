using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "cons_target_resistance", menuName = "Combat/AI/ConsTargetResistance", order = 13)]
public class ConsTargetResistance : Consideration {

    [Header("Consideration Target Resistance:")]
    public Character.Resistance ResistanceType = Character.Resistance.NONE; // None always returns Input Value 0.
    public float MinDefense = 0.0f;
    public float MaxDefense = 30.0f;

    public override float CalculateScore(Context SkillContext)
    {
        float InputValue = SkillContext.Target.GetResistance(ResistanceType);

        InputValue = ClampInputValue(InputValue, MinDefense, MaxDefense);

        float score = CalculateConsideration(TypeOfCurve, InputValue, Steepness, yShift, xShift);

        return score;
    }
}
