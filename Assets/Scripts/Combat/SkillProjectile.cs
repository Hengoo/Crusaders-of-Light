using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    [Header("Projectile Attributes:")]
    public bool PierceTargets = false;
    public float Speed = 0;
    public float MaxTimeAlive = 0;
    private float TimeAliveCounter = 0;

    [Header("Debug Information - Does not need to be set in Editor!:")]
    public Character Owner;
    public SkillType SourceSkill;
    public ItemSkill SourceItemSkill;
    public Vector3 FlyDirection = Vector3.forward;
    public Character.TeamAlignment ProjectileAlignment = Character.TeamAlignment.NONE;

    public void InitializeProjectile(Character _Owner, ItemSkill _SourceItemSkill, SkillType _SourceSkill)
    {
        // Link Skill User and Skill:
        Owner = _Owner;
        SourceSkill = _SourceSkill;
        SourceItemSkill = _SourceItemSkill;

        // Calculate which Team(s) the Projectile can hit:
        int counter = 0;

        if (SourceSkill.GetAllowTargetFriendly())
        {
            counter += (int)(Owner.GetAlignment());
        }

        if (SourceSkill.GetAllowTargetEnemy())
        {
            counter += ((int)(Owner.GetAlignment()) % 2) + 1;
        }

        ProjectileAlignment = (Character.TeamAlignment)(counter);

        // Calculate the direction in which the projectile flies:
        FlyDirection = Vector3.forward;//Owner.transform.forward;
    }

    public void Update()
    {
        TimeAliveCounter += Time.deltaTime;

        if (TimeAliveCounter >= MaxTimeAlive)
        {
            ProjectileTimeOut();
            return;
        }

        gameObject.transform.Translate(FlyDirection * Speed * Time.deltaTime);
    }

    public void ProjectileTimeOut()
    {
        Destroy(this.gameObject);
    }

    private void CheckIfTargetLegit(Character TargetCharacter)
    {
        if (ProjectileAlignment == Character.TeamAlignment.ALL
            || ProjectileAlignment == TargetCharacter.GetAlignment())
        {
            SourceSkill.ApplyEffects(Owner, SourceItemSkill, TargetCharacter);
            // TODO : PIERCE TARGETS : SAVE ALL HIT CHARACTERS SO THEY CAN NOT BE HIT AGAIN!
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("COLLIDED WITH : " + other.gameObject);
        // TODO : PIERCE TARGETS / CRASH INTO OBSTACLES!
        if (other.gameObject.tag == "Character")
        {
            CheckIfTargetLegit(other.gameObject.GetComponent<Character>());
        }
    }
}