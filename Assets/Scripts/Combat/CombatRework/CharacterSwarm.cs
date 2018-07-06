using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSwarm : Character {

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.ENEMIES;

    [Header("Enemy Testing:")]
    public bool SpawnStartingWeaponsOnStart = false;

    [Header("Swarmling:")]
    public EnemySwarm Swarmling;

    // ===================================== Override Functions =====================================

    protected override void Start()
    {
        base.Start();
        if (SpawnStartingWeaponsOnStart)
        {
            SpawnAndEquipStartingWeapons();
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    public override TeamAlignment GetAlignment()
    {
        return Alignment;
    }

    protected override void CharacterDied()
    {
        // If enemies are spawned by a spawner class, update that spawner class here!

        Swarmling.HealPlayersOnDeath();
        base.CharacterDied();
    }

    protected override void StartSkillActivation(int WeaponSkillSlotID)
    {
        // Probably no special case.
        base.StartSkillActivation(WeaponSkillSlotID);
    }

    protected override void UpdateCurrentSkillActivation()
    {
        // CharacterEnemy updates Activation here and re-evaluates it.
        // For Swarm enemies that have only one attack, the re-evaluation is mostly pointless.
        if (SkillCurrentlyActivating[0] >= 0)
        {
            ItemSkillSlots[SkillCurrentlyActivating[0]].UpdateSkillActivation(true);
        }
    }

    public override void FinishedCurrentSkillActivation(int WeaponSlotID, int Hindrance)
    {
        // CharacterEnemy overrode this for AI reasons.
        base.FinishedCurrentSkillActivation(WeaponSlotID, Hindrance);

        Swarmling.SwarmlingFinishedAttack();
    }

    // Attention Stuff: Used by Player and old Enemies, not used by the Swarm.
    // Could be optimized by changing inheritance. For now, basically ignore these calls for Swarmlings:
    public override void AttentionCharacterDied(Character CharDied)
    {

    }

    public override void AttentionPlayerRespawned(Character CharRespawned)
    {

    }

    public override void AttentionThisCharacterDied()
    {

    }

    public override CharacterAttention GetAttention()
    {
        return null;
    }

    public override void ChangeMovementRateModifier(float Change)
    {
        //base.ChangeMovementRateModifier(Change);

        Swarmling.ChangeSwarmlingSpeed(Change);
    }

    protected override void CreateCharacterFollowGUI()
    {
        /*if (!GUIChar)
        {
            GUIChar = GUIController.Instance.GenerateGUICharacterFollow();
            GUIChar.Initialize(this);
        }*/
    }

    // ====================================/ Override Functions /====================================


    // ============================================= AI =============================================

    // AI for Swarm Enemies:
    //      Melee: Attack Closest Player if in Range.
    //      Ranged: Wait for good attack oportunity, then Attack if in range.
    //      Tank: Attack Closest if in Range.

    public void SwarmlingStartSkillActivation()
    {
        StartSkillActivation(0); // Since Swarmlings are only supposed to have 1 Skill, it should always be in Slot 0!
    }

    // ============================================/ AI /============================================
}
