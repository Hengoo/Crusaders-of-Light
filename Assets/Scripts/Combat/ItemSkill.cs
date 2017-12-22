using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkill : MonoBehaviour {

    [Header("Item Skill:")]
    public Item ParentItem;

    public SkillType SkillObject;

    public float CurrentCooldown;

    public int Level;

    private float ActivationIntervallTimer = 0.0f;

    private bool EffectOnlyOnceBool = false;

    [Header("Animation:")]
    public string AnimationName = "no_animation";

    //public List<Character> AlreadyHitCharacters = new List<Character>();

    public bool StartSkillActivation()
    {
        if (CurrentCooldown > 0.0f) { return false; }
        
        ParentItem.GetOwner().StartAnimation(AnimationName, SkillObject.GetTotalActivationTime(), ParentItem.GetEquippedSlotID());

        ActivationIntervallTimer = 0.0f;
        EffectOnlyOnceBool = false;

        return SkillObject.StartSkillActivation(this, GetCurrentOwner());
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
        if (SkillObject.GetActivationIntervall() >= 0)
        {
            ActivationIntervallTimer += Time.deltaTime;

            if (ActivationIntervallTimer >= SkillObject.GetActivationIntervall())
            {
                ActivationIntervallTimer -= SkillObject.GetActivationIntervall();
                SkillObject.UpdateSkillActivation(this, ActivationTimer, StillActivating, true);
                return;
            }
        }

        SkillObject.UpdateSkillActivation(this, ActivationTimer, StillActivating, false);
    }

    public void FinishedSkillActivation()
    {
        GetCurrentOwner().FinishedCurrentSkillActivation();
    }

    public bool CheckIfSkillIsUsingHitBox(ItemSkill SkillToCheck)
    {
        return ParentItem.CheckIfSkillIsUsingHitBox(SkillToCheck);
    }

    public void InterruptSkill(bool ResetCooldown)
    {
        if (ResetCooldown)
        {
            CurrentCooldown = 0;
        }

        ParentItem.EndSkillCurrentlyUsingItemHitBox();
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
        return ParentItem.GetOwner();
    }

    // Note: If a Character can have buffs/changes to Skill Levels, then this function has to include those changes.
    public int GetSkillLevel()
    {
        return Level;
    }

    public bool GetEffectOnlyOnceBool()
    {
        return EffectOnlyOnceBool;
    }

    public void SetEffectOnlyOnceBool(bool state)
    {
        EffectOnlyOnceBool = state;
    }

    public DecisionMaker.AIDecision AICalculateSkillScoreAndApplication()
    {
        return SkillObject.AICalculateSkillScoreAndApplication(this, GetCurrentOwner());
    }
}
