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

    [Header("Skill Effect Damage Ignore Armor:")]
    public int IgnoreDefenseValueBase = 0;
    public int IgnoreDefenseValuePerLevel = 0;
    public int IgnoreResistanceValueBase = 0;
    public int IgnoreResistanceValuePerLevel = 0;

    [Header("Skill Effect Damage Ignore Armor Value Modifier:")]
    public SkillEffectValueModifier[] IgnoreValueModifiers = new SkillEffectValueModifier[0];


    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        int FinalDamageValue = DamageValueBase;

        FinalDamageValue += DamageValuePerLevel * SourceItemSkill.GetSkillLevel();

        for (int i = 0; i < DamageValueModifiers.Length; i++)
        {
            FinalDamageValue = Mathf.RoundToInt(DamageValueModifiers[i].ModifyValue(FinalDamageValue, Owner, SourceItemSkill, Target));
        }

        int FinalIgnoreDefenseValue = CalculateIgnoreDefense(Owner, SourceItemSkill, Target, SourceItemSkill.GetSkillLevel());
        int FinalIgnoreResistanceValue = CalculateIgnoreResistance(Owner, SourceItemSkill, Target, SourceItemSkill.GetSkillLevel());

        Target.InflictDamage(DefenseType, DamageType, FinalDamageValue, FinalIgnoreDefenseValue, FinalIgnoreResistanceValue);
    }

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        int FinalDamageValue = DamageValueBase;

        FinalDamageValue += DamageValuePerLevel * FixedLevel;

        for (int i = 0; i < DamageValueModifiers.Length; i++)
        {
            FinalDamageValue = Mathf.RoundToInt(DamageValueModifiers[i].ModifyValue(FinalDamageValue, Owner, SourceItemSkill, Target));
        }

        int FinalIgnoreDefenseValue = CalculateIgnoreDefense(Owner, SourceItemSkill, Target, FixedLevel);
        int FinalIgnoreResistanceValue = CalculateIgnoreResistance(Owner, SourceItemSkill, Target, FixedLevel);

        Target.InflictDamage(DefenseType, DamageType, FinalDamageValue, FinalIgnoreDefenseValue, FinalIgnoreResistanceValue);
    }

    private int CalculateIgnoreDefense(Character Owner, ItemSkill SourceItemSkill, Character Target, int Level)
    {       
        int FinalIgnoreDefenseValue = IgnoreDefenseValueBase;

        FinalIgnoreDefenseValue += IgnoreDefenseValuePerLevel * Level;

        for (int i = 0; i < IgnoreValueModifiers.Length; i++)
        {
            FinalIgnoreDefenseValue = Mathf.RoundToInt(IgnoreValueModifiers[i].ModifyValue(FinalIgnoreDefenseValue, Owner, SourceItemSkill, Target));
        }

        return FinalIgnoreDefenseValue;
    }

    private int CalculateIgnoreResistance(Character Owner, ItemSkill SourceItemSkill, Character Target, int Level)
    {
        int FinalIgnoreResistanceValue = IgnoreResistanceValueBase;

        FinalIgnoreResistanceValue += IgnoreResistanceValuePerLevel * Level;

        for (int i = 0; i < IgnoreValueModifiers.Length; i++)
        {
            FinalIgnoreResistanceValue = Mathf.RoundToInt(IgnoreValueModifiers[i].ModifyValue(FinalIgnoreResistanceValue, Owner, SourceItemSkill, Target));
        }

        return FinalIgnoreResistanceValue;
    }
}
