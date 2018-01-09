using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_change_skill_level_modifier", menuName = "Combat/SkillEffects/ChangeSkillLevelModifier", order = 8)]
public class SkillEffectChangeSkillLevelModifier : SkillEffect {

    [Header("Skill Effect Change Skill Level Modifier:")]
    public int LevelValueBase = 0;
    public float LevelValuePerLevel = 0;

    [Header("Skill Effect Damage Value Modifier:")]
    public SkillEffectValueModifier[] LevelValueModifiers = new SkillEffectValueModifier[0];

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        ApplyEffect(Owner, SourceItemSkill, Target, SourceItemSkill.GetSkillLevel());
    }

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        float LevelValue = LevelValueBase;

        LevelValue += LevelValuePerLevel * FixedLevel;

        for (int i = 0; i < LevelValueModifiers.Length; i++)
        {
            LevelValue = LevelValueModifiers[i].ModifyValue(LevelValue, Owner, SourceItemSkill, Target);
        }

        Target.ChangeSkillLevelModifier(Mathf.RoundToInt(LevelValue));
    }
}
