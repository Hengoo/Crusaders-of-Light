using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEnemy : Character {

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.ENEMIES;

    [Header("Enemy AI:")]
    public List<Character> PlayersInAttentionRange = new List<Character>();
    public float IdleSkillScore = 0.2f;
    public float IdleDuration = 0.2f;
    private float[] IdleTimer = { 0.0f, 0.0f };

    public MovePattern ActiveMovePattern;
    private Character TargetCharacter;
    public MovePattern[] MovePatterns = new MovePattern[0];

    public float MovePatternEvaluationCycle = 0.5f;
    public float MovePatternEvaluationCycleTimer = 0.0f;

    public float IdleMoveScore = 0.2f;
    public float IdleMoveDuration = 0.2f;
    private float IdleMoveTimer = 0.0f;

    protected override void Update()
    {
        base.Update();
        DecideSkillUse(0);
        if (!TwoHandedWeaponEquipped)
        {
            DecideSkillUse(1);
        }
        UpdateMovePattern();
    }

    private void FixedUpdate()
    {
        ActiveMovePattern.UpdateMovePattern(PhysCont, this, TargetCharacter);
    }

    public override TeamAlignment GetAlignment()
    {
        return Alignment;
    }

    // ========================================= AI =========================================

    public void PlayerEntersAttentionRange(Character PlayerCharacter)
    {
        if (PlayersInAttentionRange.Contains(PlayerCharacter))
        {
            return;
        }

        PlayersInAttentionRange.Add(PlayerCharacter);
    }

    public void PlayerLeavesAttentionRange(Character PlayerCharacter)
    {
        PlayersInAttentionRange.Remove(PlayerCharacter);
    }

    public List<Character> GetPlayersInAttentionRange()
    {
        return PlayersInAttentionRange;
    }

    public void DecideSkillUse(int WeaponSlotID)
    {
        if (SkillCurrentlyActivating[WeaponSlotID] >= 0)
        {
            return;
        }

        if (IdleTimer[WeaponSlotID] > 0)
        {
            IdleTimer[WeaponSlotID] -= Time.deltaTime;
            return;
        }

        DecisionMaker.AIDecision BestSkillDecision = new DecisionMaker.AIDecision
        {
            Score = IdleSkillScore
        };

        DecisionMaker.AIDecision TempSkillDecision = new DecisionMaker.AIDecision();

        int BestSkillID = -1;

        int TwoHandedModifier = 0;

        if (TwoHandedWeaponEquipped)
        {
            TwoHandedModifier = 1;
        }

        for (int sk = WeaponSlotID * SkillsPerWeapon; sk < (WeaponSlotID + 1 + TwoHandedModifier) * SkillsPerWeapon; sk++)
        {
            if (ItemSkillSlots[sk])
            {
                TempSkillDecision = ItemSkillSlots[sk].AICalculateSkillScoreAndApplication();

                if (TempSkillDecision.Score > BestSkillDecision.Score)
                {
                    BestSkillDecision = TempSkillDecision;
                    BestSkillID = sk;
                }
            }
        }

        if (BestSkillID >= 0)
        {
            StartSkillActivation(BestSkillID);
        }
        else
        {
            IdleTimer[WeaponSlotID] = IdleDuration;
        }
    }
    /*
        protected override void StartSkillActivation(int WeaponSkillSlotID)
        {
            base.StartSkillActivation(WeaponSkillSlotID);

            SkillActivationButtonsPressed[WeaponSkillSlotID] = true;
        }

        public override void FinishedCurrentSkillActivation()
        {
            base.FinishedCurrentSkillActivation();


        }*/

    public void DecideMovePattern()
    {
        if (MovePatternEvaluationCycleTimer > 0)
        {
            MovePatternEvaluationCycleTimer -= Time.deltaTime;
            return;
        }
/*
        if (IdleMoveTimer > 0)
        {
            IdleMoveTimer -= Time.deltaTime;
            return;
        }*/

        DecisionMaker.AIDecision BestMovePatternDecision = new DecisionMaker.AIDecision
        {
            Score = IdleSkillScore
        };

        DecisionMaker.AIDecision TempMovePatternDecision = new DecisionMaker.AIDecision();

        for (int mp = 0; mp < MovePatterns.Length; mp++)
        {
            TempMovePatternDecision = MovePatterns[mp].AICalculateMovePatternScore(this);

            if (TempMovePatternDecision.Score > BestMovePatternDecision.Score)
            {
                BestMovePatternDecision = TempMovePatternDecision;
                ActiveMovePattern = MovePatterns[mp];
                TargetCharacter = BestMovePatternDecision.TargetCharacter;
            }
        }
    }

    private void UpdateMovePattern()
    {
        if (MovePatternEvaluationCycleTimer <= 0)
        {
            DecideMovePattern();
            MovePatternEvaluationCycleTimer = MovePatternEvaluationCycle;
        }
        else
        {
            MovePatternEvaluationCycleTimer -= Time.deltaTime;
        }
    }


    // ======================================== /AI =========================================

    // =================================== SKILL ACTIVATION ====================================

    protected override void UpdateCurrentSkillActivation()
    {
        for (int i = 0; i < SkillCurrentlyActivating.Length; i++)
        {
            if (SkillCurrentlyActivating[i] >= 0)
            {
                ItemSkillSlots[SkillCurrentlyActivating[i]].UpdateSkillActivation(true);
            }
        }
    }

    // =================================== /SKILL ACTIVATION ====================================

}
