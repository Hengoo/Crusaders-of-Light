using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_damage", menuName = "Combat/SkillEffects/Damage", order = 1)]
public class SkillEffectDamage : SkillEffect
{
    [Header("Skill Effect Damage:")]
    public int DamageValueBase = 0;
    public int DamageValuePerLevel = 0;
    public Character.Defense DefenseType = Character.Defense.NONE;
    public Character.Resistance DamageType = Character.Resistance.NONE;

    [Header("Skill Effect Damage Value Modifier:")]
    public SkillEffectValueModifier[] DamageValueModifiers = new SkillEffectValueModifier[0];


    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        int FinalDamageValue = DamageValueBase;

        FinalDamageValue += DamageValuePerLevel * SourceItemSkill.GetSkillLevel();

        for (int i = 0; i < DamageValueModifiers.Length; i++)
        {
            FinalDamageValue = Mathf.RoundToInt(DamageValueModifiers[i].ModifyValue(FinalDamageValue, Owner, SourceItemSkill, Target));
        }

        Target.InflictDamage(DefenseType, DamageType, FinalDamageValue);
    }

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        int FinalDamageValue = DamageValueBase;

        FinalDamageValue += DamageValuePerLevel * FixedLevel;

        for (int i = 0; i < DamageValueModifiers.Length; i++)
        {
            FinalDamageValue = Mathf.RoundToInt(DamageValueModifiers[i].ModifyValue(FinalDamageValue, Owner, SourceItemSkill, Target));
        }

        Target.InflictDamage(DefenseType, DamageType, FinalDamageValue);
    }

}
