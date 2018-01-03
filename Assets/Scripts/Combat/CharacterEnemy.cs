using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEnemy : Character {

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.ENEMIES;

    [Header("Enemy AI:")]
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

    private float[] SensibleSkillActivationTimes = new float[2];

    private float[] SkillEvaluationCycleTimers = { 0, 0 };
    private bool[] SkillContinueActivation = { false, false };

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

        int BestSkillID = EvaluateBestSkillToUse(WeaponSlotID);

        if (BestSkillID >= 0)
        {
            SkillEvaluationCycleTimers[WeaponSlotID] = ItemSkillSlots[BestSkillID].AIGetSkillEvaluationCycle();
            SkillContinueActivation[WeaponSlotID] = true;

            SensibleSkillActivationTimes[WeaponSlotID] = ItemSkillSlots[BestSkillID].AIGetSensibleActivationTime();
            StartSkillActivation(BestSkillID);
        }
        else
        {
            IdleTimer[WeaponSlotID] = IdleDuration;
        }
    }

    private int EvaluateBestSkillToUse(int WeaponSlotID)
    {
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

        return BestSkillID;
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

        MovePattern[] WeaponMovePatterns;

        for (int i = 0; i < SkillCurrentlyActivating.Length; i++)
        {
            if (SkillCurrentlyActivating[i] >= 0)
            {
                WeaponMovePatterns = ItemSkillSlots[SkillCurrentlyActivating[i]].AIGetSkillMovePatterns();

                for (int mp = 0; mp < WeaponMovePatterns.Length; mp++)
                {
                    TempMovePatternDecision = WeaponMovePatterns[mp].AICalculateMovePatternScore(this);

                    if (TempMovePatternDecision.Score > BestMovePatternDecision.Score)
                    {
                        BestMovePatternDecision = TempMovePatternDecision;
                        ActiveMovePattern = WeaponMovePatterns[mp];
                        TargetCharacter = BestMovePatternDecision.TargetCharacter;
                    }
                }
            }
        }   
    }

    private void UpdateMovePattern()
    {
        if (CharAttention.GetPlayersInAttentionRange().Count <= 0)
        {
            return;
        }

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

    public override void FinishedCurrentSkillActivation(int WeaponSlotID, int Hindrance)
    {
        SkillContinueActivation[WeaponSlotID] = false;

        base.FinishedCurrentSkillActivation(WeaponSlotID, Hindrance);
    }

    protected override void UpdateCurrentSkillActivation()
    {
        for (int i = 0; i < SkillCurrentlyActivating.Length; i++)
        {
            if (SkillCurrentlyActivating[i] >= 0)
            {
                if (SkillContinueActivation[i] && ItemSkillSlots[SkillCurrentlyActivating[i]].AIGetSkillEvaluationCycle() >= 0)
                {
                    if (!EvaluateContinuedActivationOfSkill(i))
                    {
                        SkillContinueActivation[i] = false;
                    }
                }

                ItemSkillSlots[SkillCurrentlyActivating[i]].UpdateSkillActivation(SkillContinueActivation[i], SensibleSkillActivationTimes[i]);
            }
        }
    }

    private bool EvaluateContinuedActivationOfSkill(int WeaponSlotID)
    {
        SkillEvaluationCycleTimers[WeaponSlotID] -= Time.deltaTime;

        if (SkillEvaluationCycleTimers[WeaponSlotID] <= 0)
        {
            if (SkillCurrentlyActivating[WeaponSlotID] != EvaluateBestSkillToUse(WeaponSlotID))
            {
                return false;
            }

            SkillEvaluationCycleTimers[WeaponSlotID] = ItemSkillSlots[WeaponSlotID].AIGetSkillEvaluationCycle();
        }

        return true;
    }

    // =================================== /SKILL ACTIVATION ====================================

}
