using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_value_mod_activation_time", menuName = "Combat/SkillEffectsModifiers/ActivationTime", order = 1)]
public class SkillEffectValueModActivationTime : SkillEffectValueModifier {

    [Header("Value Modifier Activation Time:")]

    public float MinTime = 0.0f;
    public float MaxTime = 1.0f;

    public float MinModifier = 1.0f;
    public float MaxModifier = 1.0f;

    public override float ModifyValue(float Value, Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        float TimePerc = Mathf.Clamp01((SourceItemSkill.GetCurrentSkillActivationTime() - MinTime) / (MaxTime - MinTime));
        return Value * Mathf.Lerp(MinModifier, MaxModifier, TimePerc);
    }
}
