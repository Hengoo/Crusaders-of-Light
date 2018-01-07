﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkill : MonoBehaviour {

    [Header("Item Skill:")]
    public Item ParentItem;

    public SkillType SkillObject;

    public int Level;

    [Header("Item Skill (Do not set - Shown for testing only):")]
    public float CurrentCooldown;

    public float ActivationIntervallTimer = 0.0f;

    public bool[] EffectOnlyOnceBool = { false, false };
    public float EffectFloat = 0.0f;

    //[Header("Animation:")]
    //public string AnimationName = "no_animation";

    //public List<Character> AlreadyHitCharacters = new List<Character>();

    public bool StartSkillActivation()
    {
        if (CurrentCooldown > 0.0f) { return false; }

        ActivationIntervallTimer = 0.0f;
        for (int i = 0; i < EffectOnlyOnceBool.Length; i++)
        {
            EffectOnlyOnceBool[i] = false;
        }

        EffectFloat = 0.0f;

        bool ActivationSuccessful = SkillObject.StartSkillActivation(this, GetCurrentOwner());
        
        if (!ActivationSuccessful)
        {
            return false;
        }

        if (SkillObject.GetOverwriteAnimationSpeedScaling() > 0)
        {
            ParentItem.GetOwner().StartAnimation(SkillObject.GetAnimationName(), SkillObject.GetOverwriteAnimationSpeedScaling(), ParentItem.GetEquippedSlotID());
        }
        else
        {
            ParentItem.GetOwner().StartAnimation(SkillObject.GetAnimationName(), SkillObject.GetTotalActivationTime(), ParentItem.GetEquippedSlotID());
        }
        
        return true;
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

    public void UpdateSkillActivation(bool StillActivating)
    {
        ParentItem.UpdateSkillActivationTimer();

        if (SkillObject.GetActivationIntervall() >= 0)
        {
            ActivationIntervallTimer += Time.deltaTime;

            if (ActivationIntervallTimer >= SkillObject.GetActivationIntervall())
            {
                ActivationIntervallTimer -= SkillObject.GetActivationIntervall();
                SkillObject.UpdateSkillActivation(this, ParentItem.GetSkillActivationTimer(), StillActivating, true);
                return;
            }
        }

        SkillObject.UpdateSkillActivation(this, ParentItem.GetSkillActivationTimer(), StillActivating, false);
    }

    public void UpdateSkillActivation(float MaxActivationTime)
    {
        if (MaxActivationTime >= 0 && ParentItem.GetSkillActivationTimer() + Time.deltaTime >= MaxActivationTime)
        {
            UpdateSkillActivation(false);
        }
        else
        {
            UpdateSkillActivation(true);
        }
    }

    public void UpdateSkillActivation(bool StillActivating, float MaxActivationTime)
    {
        if (!StillActivating)
        {
            UpdateSkillActivation(false);
        }
        else if (MaxActivationTime >= 0)
        {
            UpdateSkillActivation(MaxActivationTime);
        }
        else
        {
            UpdateSkillActivation(true);
        }
    }

    public void FinishedSkillActivation()
    {
        ParentItem.SetSkillActivationTimer(0.0f);
        GetCurrentOwner().FinishedCurrentSkillActivation(ParentItem.GetCurrentEquipSlot(), -1 * SkillObject.GetHindranceLevel());
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
        if (GetCurrentOwner())
        {
            return Level + GetCurrentOwner().GetSkillLevelModifier();
        }
        return Level;
    }

    public void SetSkillLevel(int Value)
    {
        Level = Value;
    }

    public int GetParentItemEquipmentSlot()
    {
        return ParentItem.GetCurrentEquipSlot();
    }

    public float GetCurrentSkillActivationTime()
    {
        return ParentItem.GetSkillActivationTimer();
    }

    public int GetBasePowerLevel()
    {
        return SkillObject.GetPowerLevel();
    }

    public bool GetEffectOnlyOnceBool(int ID)
    {
        return EffectOnlyOnceBool[ID];
    }

    public void SetEffectOnlyOnceBool(int ID, bool state)
    {
        EffectOnlyOnceBool[ID] = state;
    }

    public float GetEffectFloat()
    {
        return EffectFloat;
    }

    public void SetEffectFloat(float value)
    {
        EffectFloat = value;
    }

    public void ChangeEffectFloat(float change)
    {
        EffectFloat += change;
    }
       

    public DecisionMaker.AIDecision AICalculateSkillScoreAndApplication()
    {
        return SkillObject.AICalculateSkillScoreAndApplication(this, GetCurrentOwner());
    }

    public float AIGetSensibleActivationTime()
    {
        return SkillObject.GetDecisionMaker().AIGetSensibleActivationTime();
    }

    public MovePattern[] AIGetSkillMovePatterns()
    {
        return SkillObject.GetDecisionMaker().AISkillMovePatterns;
    }

    public float AIGetSkillEvaluationCycle()
    {
        return SkillObject.GetDecisionMaker().SkillEvaluationCycle;
    }
}
