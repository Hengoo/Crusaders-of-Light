using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "condition", menuName = "Combat/Condition/Condition", order = 1)]
public class Condition : ScriptableObject {
    [Header("Condition:")]
    public string Name = "unnamed_condition";

    [Header("Condition Effects:")]
    public SkillEffect[] OnApply = new SkillEffect[0];
    public SkillEffect[] OnTick = new SkillEffect[0];
    public SkillEffect[] OnEnd = new SkillEffect[0];

    [Header("Condition Duration:")]
    private float Duration = 0.0f; // Currently unsused!
    public float TickTime = 0.5f;

    //public int StackMaximum = 1;
    public int InstanceMaximum = 1;

    [Header("Condition Visual:")]
    public GameObject VisualEffectObject;


    public void ApplyCondition(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        for (int i = 0; i < OnApply.Length; i++)
        {
            OnApply[i].ApplyEffect(Owner, SourceItemSkill, Target, FixedLevel);
        }
    }

    public void ApplyEffectsOnTick(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        for (int i = 0; i < OnTick.Length; i++)
        {
            OnTick[i].ApplyEffect(Owner, SourceItemSkill, Target, FixedLevel);
        }
    }

    public void EndCondition(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        for (int i = 0; i < OnEnd.Length; i++)
        {
            OnEnd[i].ApplyEffect(Owner, SourceItemSkill, Target, FixedLevel);
        }
    }

    public bool HasTicks()
    {
        if (TickTime >= 0)
        {
            return true;
        }
        return false;
    }

    public bool ReachedTick (float TickCounter)
    {
        if (TickCounter >= TickTime)
        {
            return true;
        }
        return false;
    }

    public bool ReachedEnd (float TimeCounter)
    {
        if (Duration >= 0 && TimeCounter >= Duration)
        {
            return true;
        }
        return false;
    }

    public float GetDuration()
    {
        return Duration;
    }

    public float GetTickTime()
    {
        return TickTime;
    }

    public int GetInstanceMaximum()
    {
        return InstanceMaximum;
    }

    public GameObject GetVisualEffectObject()
    {
        return VisualEffectObject;
    }
/*
    public int GetStackMaximum()
    {
        return StackMaximum;
    }*/
}
