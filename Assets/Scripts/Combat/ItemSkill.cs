using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkill : MonoBehaviour {

    public Item ParentItem;

    public SkillType SkillObject;

    public float CurrentCooldown;

    public int Level;

    public Character CurrentOwner;

    public List<Character> AlreadyHitCharacters = new List<Character>();

    public bool StartSkillActivation()
    {
        if (CurrentCooldown > 0.0f) { return false; }

        return SkillObject.StartSkillActivation(this, CurrentOwner);
    }

    public void SetCurrentCooldown(float NewCooldown)
    {
        CurrentCooldown = NewCooldown;
    }

    public bool IsCurrentlyOnCooldown()
    {
        if (CurrentCooldown > 0)
        {
            return true;
        }
        return false;
    }

    public void UpdateCooldown(float PassedTime)
    {
        if (!IsCurrentlyOnCooldown())
        {
            return;
        }
        CurrentCooldown = Mathf.Max(CurrentCooldown - PassedTime, 0);
    }

    public void UpdateSkillActivation(float ActivationTimer, bool StillActivating)
    {
        SkillObject.UpdateSkillActivation(this, ActivationTimer, StillActivating);
    }

    public void FinishedSkillActivation()
    {
        CurrentOwner.FinishedCurrentSkillActivation();
    }

    public bool CheckIfSkillIsUsingHitBox(ItemSkill SkillToCheck)
    {
        return ParentItem.CheckIfSkillIsUsingHitBox(SkillToCheck);
    }

    public List<Character> GetAllCurrentlyCollidingCharacters()
    {
        return ParentItem.GetAllCurrentlyCollidingCharacters();
    }

    public void StartSkillCurrentlyUsingItemHitBox(bool HitEachCharacterOnce)
    {
        ParentItem.StartSkillCurrentlyUsingItemHitBox(this, SkillObject, HitEachCharacterOnce);
    }

    public void EndSkillCurrentlyUsingItemHitBox()
    {
        ParentItem.EndSkillCurrentlyUsingItemHitBox();
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

    public DecisionMaker.AIDecision AICalculateSkillScoreAndApplication()
    {
        return SkillObject.AICalculateSkillScoreAndApplication(this, CurrentOwner);
    }
}
