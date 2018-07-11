using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillType : ScriptableObject
{

    public enum Hindrance
    {
        NONE = 0,
        OWN_SIDE = 1,
        MIDDLE_SIDE = 2,
        OTHER_SIDE = 3,
        NO_OTHER_SKILLS = 4
    }

    [Header("Skill Base:")]
    public int Cost;
    public float ActivationTime;
    public float ActivationIntervall = -1; // Negative: No Intervall Action
    public float Cooldown = -1; // Negative: No Cooldown
    public float ActivationMovementModifier = 0.0f;

    [Header("Skill Targeting:")]
    public bool AllowTargetFriendly;
    public bool AllowTargetEnemy;

    [Header("Skill Effects:")]
    public SkillEffect[] Effects;

    [Header("Skill Interaction With Other Skills:")]
    public Hindrance HindranceLevel = Hindrance.NONE;   // Hindrance is added while using this skill.

    [Header("Skill Animation:")]
    public string AnimationName = "no_animation";
    public float OverwriteAnimationSpeedScaling = -1; // If > 0 : Use other Speed scaling. Which one, depends on the exact Skill Type! Base: This Value.

    [Header("Skill Enemy AI (Threat Array has to have Size 3!):")]
    public DecisionMakerSkills AIDecisionMaker;
    public int PowerLevel = 0;
    public float[] Threat = new float[3]; // 0: Threat Active, 1: Threat Active Close Range, 2: Threat Long Range




    public bool GetAllowTargetFriendly()
    {
        return AllowTargetFriendly;
    }

    public bool GetAllowTargetEnemy()
    {
        return AllowTargetEnemy;
    }

    public float GetActivationIntervall()
    {
        return ActivationIntervall;
    }

    public DecisionMakerSkills GetDecisionMaker()
    {
        return AIDecisionMaker;
    }

    public int GetPowerLevel()
    {
        return PowerLevel;
    }

    public string GetAnimationName()
    {
        return AnimationName;
    }

    public virtual float GetOverwriteAnimationSpeedScaling()
    {
        return OverwriteAnimationSpeedScaling;
    }

    public int GetHindranceLevel()
    {
        return (int)(HindranceLevel);
    }

    public virtual bool StartSkillActivation(ItemSkill SourceItemSkill, Character Owner)
    {
        if (!Owner.CheckHindrance(HindranceLevel))
        {
            return false;
        }


        //CheckIfSkillCouldBeActivated(SourceItemSkill, Owner);

        // Skill succesfully activated if this point is reached:

        // Pay Activation Cost:
        Owner.ChangeEnergyCurrent(-1 * Cost);

        // Add Hindrance Level:
        Owner.ChangeHindranceLevel(HindranceLevel);

        // Add Movement Modifier:
        Owner.ChangeMovementRateModifier(ActivationMovementModifier);

        // Start Cooldown:      (Note: The current Cooldown is saved in the SourceWeapon)
        /*      if (Cooldown > 0)
              {
                  SourceItemSkill.SetCurrentCooldown(Cooldown);
              }*/

        return true;
    }

    public void RemoveActivationMovementRateModifier(ItemSkill SourceItemSkill, Character Owner)
    {
        Owner.ChangeMovementRateModifier(-1 * ActivationMovementModifier);
        Owner.SetOverrideMovement(false);
        Owner.SetOverrideRotation(false);
    }


    private bool CheckIfSkillCouldBeActivated(ItemSkill SourceItemSkill, Character Owner)
    {
        // Can Activation Cost be paid?
        if (Owner.GetEnergyCurrent() < Cost)
        {
           // Debug.Log(Owner + " can not activate Skill " + this + "! Reason: Not enough Energy!");
            return false;
        }

        if (SourceItemSkill.IsCurrentlyOnCooldown())
        {
           // Debug.Log(Owner + " can not activate Skill " + this + "! Reason: Skill is on Cooldown!");
            return false;
        }


        return true;
    }

    public virtual void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {

    }

    public virtual void ApplyEffects(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        for (int i = 0; i < Effects.Length; i++)
        {
            Effects[i].ApplyEffect(Owner, SourceItemSkill, Target);
        }
    }

    public virtual void ApplyEffects(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        for (int i = 0; i < Effects.Length; i++)
        {
            Effects[i].ApplyEffect(Owner, SourceItemSkill, Target, FixedLevel);
        }
    }

    public DecisionMaker.AIDecision AICalculateSkillScoreAndApplication(ItemSkill SourceItemSkill, Character Owner)
    {
        DecisionMaker.AIDecision SkillApp;

        if (!CheckIfSkillCouldBeActivated(SourceItemSkill, Owner))
        {
            SkillApp = new DecisionMaker.AIDecision
            {
                Score = -1
            };

            return SkillApp;
        }

        SkillApp = AIDecisionMaker.CalculateTotalScore(Owner, SourceItemSkill);

        return SkillApp;
    }

    public float GetTotalActivationTime()
    {
        return ActivationTime;
    }

    public float[] GetThreat()
    {
        return Threat;
    }
}
