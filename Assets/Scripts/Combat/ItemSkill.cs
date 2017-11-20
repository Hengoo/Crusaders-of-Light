using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkill : MonoBehaviour {

    public SkillType SkillObject;

    public float CurrentCooldown;

    public int Level;

    public Character CurrentOwner;

    public bool StartSkillActivation()
    {
        if (CurrentCooldown > 0.0f) { return false; }

        return SkillObject.StartSkillActivation(this, CurrentOwner);
    }

    public void SetCurrentCooldown(float NewCooldown)
    {
        CurrentCooldown = NewCooldown;
    }

    public void UpdateSkillActivation(float ActivationTimer, bool StillActivating)
    {
        SkillObject.UpdateSkillActivation(this, ActivationTimer, StillActivating);
    }

    public void FinishedSkillActivation()
    {
        CurrentOwner.FinishedCurrentSkillActivation();
    }

    public Character GetCurrentOwner()
    {
        return CurrentOwner;
    }

    // Note: If a Character can have buffs/changes to Skill Levels, then this function has to include those changes.
    public int GetSkillLevel()
    {
        return Level;
    }
}
