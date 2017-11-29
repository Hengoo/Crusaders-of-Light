using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_affect_source_character", menuName = "Combat/SkillEffectsSpecial/AffectSourceCharacter", order = 1)]
public class SkillEffectAffectSourceCharacter : SkillEffect {

    [Header("Skill Effect Affect Source Character:")]
    public SkillEffect[] SkillEffectsToOwner = new SkillEffect[0];

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        for (int i = 0; i < SkillEffectsToOwner.Length; i++)
        {
            SkillEffectsToOwner[i].ApplyEffect(Owner, SourceItemSkill, Owner);
        }
    }

}
