using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_value_mod_target_health_percentage", menuName = "Combat/SkillEffectsModifiers/TargetHealthPercentage", order = 2)]
public class SkillEffectValueModTargetHealthPercentage : SkillEffectValueModifier {

    [Header("Value Modifier Target Health Percentage:")]

    public float MinHealth = 0.0f;
    public float MaxHealth = 1.0f;

    public float MinModifier = 1.0f;
    public float MaxModifier = 1.0f;

    public override float ModifyValue(float Value, Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        float HealthPerc = Mathf.Clamp01((Target.GetHealthCurrentPercentage() - MinHealth) / (MaxHealth - MinHealth));
        return Value * Mathf.Lerp(MinModifier, MaxModifier, HealthPerc);
    }
}

