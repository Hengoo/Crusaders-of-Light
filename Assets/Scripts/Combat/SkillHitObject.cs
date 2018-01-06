using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHitObject : MonoBehaviour {

    [Header("Hit Object Attributes:")]
    public float MaxTimeAlive = 0;
    private float TimeAliveCounter = 0;
    public bool CanHitSameTargetMultipleTime = false;
    public int MaxNumberOfTargets = -1;

    [Header("Hit Object - Does not need to be set in Editor!:")]
    public Character Owner;
    public SkillType SourceSkill;
    public ItemSkill SourceItemSkill;
    public int FixedLevel;
    public Character.TeamAlignment HitObjectAlignment = Character.TeamAlignment.NONE;

    private List<Character> AlreadyHitCharacters = new List<Character>();

    public void InitializeHitObject(Character _Owner, ItemSkill _SourceItemSkill, SkillType _SourceSkill, bool UseLevelAtActivationMoment)
    {
        // Link Skill User and Skill:
        Owner = _Owner;
        SourceSkill = _SourceSkill;
        SourceItemSkill = _SourceItemSkill;
        
        if (UseLevelAtActivationMoment)
        {
            FixedLevel = SourceItemSkill.GetSkillLevel();
        }
        else
        {
            FixedLevel = -1;
        }

        // Calculate which Team(s) the HitObject can hit:
        int counter = 0;

        if (SourceSkill.GetAllowTargetFriendly())
        {
            counter += (int)(Owner.GetAlignment());
        }

        if (SourceSkill.GetAllowTargetEnemy())
        {
            counter += ((int)(Owner.GetAlignment()) % 2) + 1;
        }

        HitObjectAlignment = (Character.TeamAlignment)(counter);
    }

    public void Update()
    {
        TimeAliveCounter += Time.deltaTime;

        if (TimeAliveCounter >= MaxTimeAlive)
        {
            HitObjectTimeOut();
            return;
        }
    }

    public void HitObjectTimeOut()
    {
        Destroy(this.gameObject);
    }

    protected virtual void HitTarget(Character TargetCharacter)
    {
        if (!CanHitSameTargetMultipleTime && AlreadyHitCharacters.Contains(TargetCharacter))
        {
            return;
        }

        if (HitObjectAlignment != TargetCharacter.GetAlignment()
            && HitObjectAlignment != Character.TeamAlignment.ALL)
        {
            return;
        }

        AlreadyHitCharacters.Add(TargetCharacter);

        if (FixedLevel >= 0)
        {
            SourceSkill.ApplyEffects(Owner, SourceItemSkill, TargetCharacter, FixedLevel);
        }
        else
        {
            SourceSkill.ApplyEffects(Owner, SourceItemSkill, TargetCharacter);
        }

        if (MaxNumberOfTargets > 0
            && MaxNumberOfTargets >= AlreadyHitCharacters.Count)
        {
            ReachedMaxNumberOfTargets();
        }
    }
    
    protected virtual void ReachedMaxNumberOfTargets()
    {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            HitTarget(other.gameObject.GetComponent<Character>());
        }
    }
}
