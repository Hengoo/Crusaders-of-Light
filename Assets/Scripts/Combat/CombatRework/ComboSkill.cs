using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboSkill : ScriptableObject {

    public ItemSkill ISkill;

    [Header("Next Combo Skills:")]
    public ComboSkill[] NextSkills = new ComboSkill[0];

    public ItemSkill GetISkill()
    {
        return ISkill;
    }

    public ComboSkill[] GetNextSkills()
    {
        return NextSkills;
    }
}
