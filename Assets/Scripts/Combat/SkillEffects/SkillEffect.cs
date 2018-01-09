using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect : ScriptableObject {

	public virtual void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {

    }

    public virtual void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        ApplyEffect(Owner, SourceItemSkill, Target);
    }
}
