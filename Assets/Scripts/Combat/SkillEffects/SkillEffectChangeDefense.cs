using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_change_defense", menuName = "Combat/SkillEffects/ChangeDefense", order = 6)]
public class SkillEffectChangeDefense : SkillEffect {

    [Header("Skill Effect Change Defense:")]
    public float DefenseValueBase = 0;
    public float DefenseValuePerLevel = 0;
    public Character.Defense DefenseType = Character.Defense.NONE;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        float FinalResistanceValue = DefenseValueBase;

        FinalResistanceValue += DefenseValuePerLevel * SourceItemSkill.GetSkillLevel();

        Target.ChangeDefense(DefenseType, FinalResistanceValue);
    }

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        float FinalResistanceValue = DefenseValueBase;

        FinalResistanceValue += DefenseValuePerLevel * FixedLevel;

        Target.ChangeDefense(DefenseType, FinalResistanceValue);
    }
}
