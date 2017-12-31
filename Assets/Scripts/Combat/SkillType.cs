using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillType : ScriptableObject {

    [Header("Skill Base:")]
    public int Cost;
    public float ActivationTime;
    public float ActivationIntervall = -1; // Negative: No Intervall Action
    public float Cooldown = -1; // Negative: No Cooldown

    [Header("Skill Targeting:")]
    public bool AllowTargetFriendly;
    public bool AllowTargetEnemy;

    [Header("Skill Effects:")]
    public SkillEffect[] Effects;

    [Header("Skill Animation:")]
    public float OverwriteAnimationSpeedScaling = -1; // If > 0 : Use other Speed scaling. Which one, depends on the exact Skill Type! Base: This Value.

    [Header("Skill Enemy AI Decision Maker:")]
    public DecisionMaker AIDecisionMaker;


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

    public DecisionMaker GetDecisionMaker()
    {
        return AIDecisionMaker;
    }

    public virtual float GetOverwriteAnimationSpeedScaling()
    {
        return OverwriteAnimationSpeedScaling;
    }

    public bool StartSkillActivation(ItemSkill SourceItemSkill, Character Owner)
    {

        CheckIfSkillCouldBeActivated(SourceItemSkill, Owner);
        
        // Skill succesfully activated if this point is reached:

        // Pay Activation Cost:
        Owner.ChangeEnergyCurrent(-1 * Cost);

        // Start Cooldown:      (Note: The current Cooldown is saved in the SourceWeapon)
        if (Cooldown > 0)
        {
            SourceItemSkill.SetCurrentCooldown(Cooldown);
        }

        return true;
    }

    private bool CheckIfSkillCouldBeActivated(ItemSkill SourceItemSkill, Character Owner)
    {
        // Can Activation Cost be paid?
        if (Owner.GetEnergyCurrent() < Cost)
        {
            Debug.Log(Owner + " can not activate Skill " + this + "! Reason: Not enough Energy!");
            return false;
        }

        if (SourceItemSkill.IsCurrentlyOnCooldown())
        {
            Debug.Log(Owner + " can not activate Skill " + this + "! Reason: Skill is on Cooldown!");
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
}
