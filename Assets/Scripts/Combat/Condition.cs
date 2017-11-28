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
    public float Duration = 0.0f;
    public float TickTime = 0.5f;

    public int StackMaximum = 1;
    public int InstanceMaximum = 1;


    public void ApplyCondition(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        for (int i = 0; i < OnApply.Length; i++)
        {
            OnApply[i].ApplyEffect(Owner, SourceItemSkill, Target);
        }
    }

    public void ApplyEffectsOnTick(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        for (int i = 0; i < OnTick.Length; i++)
        {
            OnTick[i].ApplyEffect(Owner, SourceItemSkill, Target);
        }
    }

    public void EndCondition(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        for (int i = 0; i < OnEnd.Length; i++)
        {
            OnEnd[i].ApplyEffect(Owner, SourceItemSkill, Target);
        }
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
        if (TimeCounter >= Duration)
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

    public int GetStackMaximum()
    {
        return StackMaximum;
    }
}
