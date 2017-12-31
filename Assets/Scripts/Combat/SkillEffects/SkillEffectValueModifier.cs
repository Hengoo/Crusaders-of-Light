using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffectValueModifier : ScriptableObject {

	public virtual float ModifyValue(float Value, Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        return Value;
    }
}
