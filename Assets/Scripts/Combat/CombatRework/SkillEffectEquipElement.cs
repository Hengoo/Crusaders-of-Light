using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "skill_effect_equip_element", menuName = "Combat/SkillEffects/EquipElement", order = 40)]
public class SkillEffectEquipElement : SkillEffect {

    public ElementItem ElementToEquip;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        if (Owner.GetEquippedElement())
        {
            return;
        }

        Owner.SpawnAndEquipElement(ElementToEquip);
    }
}
