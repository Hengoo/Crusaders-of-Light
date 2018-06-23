using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterEnemy : Character {

    public struct BestSkill
    {
        public int SkillID;
        public float Score;
    }

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.ENEMIES;

    [Header("Enemy AI:")]
    public float IdleSkillScore = 0.2f;
    public float IdleDuration = 0.2f;
    private float[] IdleTimer = { 0.0f, 0.0f };

    public MovePattern ActiveMovePattern;
    private MovePattern BaseMovePattern;
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

    public int BasePowerLevel = 0;

    public Spawner SpawnedBy;

    [Header("Enemy Nav Mesh Movement:")]
    public NavMeshAgent NavAgent;

    [Header("Enemy Testing:")]
    public bool SpawnStartingWeaponsOnStart = false;

    protected override void Start()
    {
        PhysCont = new PhysicsController(gameObject);
        base.Start();
        if (SpawnStartingWeaponsOnStart)
        {
            SpawnAndEquipStartingWeapons();
        }

        BaseMovePattern = ActiveMovePattern;
    }

    protected override void Update()
    {
        base.Update();

        DecideSkillUse();
        if (!TwoHandedWeaponEquipped)
        {
            DecideSkillUse();
        }
        UpdateMovePattern();
    }
    

    private void FixedUpdate()
    {
        //ActiveMovePattern.UpdateMovePattern(PhysCont, this, TargetCharacter);
        ActiveMovePattern.UpdateMovePattern(NavAgent, this, TargetCharacter);
    }

    public override TeamAlignment GetAlignment()
    {
        return Alignment;
    }

    public void InitializeEnemy(Spawner SpawnedFrom, Weapon[] SpawnEquippedWeapons, int[] SpawnWeaponsLevel)
    {
        SetSpawner(SpawnedFrom);

        StartingWeapons = SpawnEquippedWeapons;

        SpawnAndEquipStartingWeapons();

        for (int i = 0; i < WeaponSlots.Length; i++)
        {
            if (WeaponSlots[i])
            {
                WeaponSlots[i].SetAllItemSkillsLevel(SpawnWeaponsLevel[i]);
            }
        }
    }

    public void SetSpawner(Spawner SpawnedFrom)
    {
        SpawnedBy = SpawnedFrom;
    }

    protected override void CharacterDied()
    {
        if(SpawnedBy)
            SpawnedBy.SpawnedCharacterDied(this);

        base.CharacterDied();
    }

    // ========================================= AI =========================================

    public void DecideSkillUse()
    {
        if (SkillCurrentlyActivating[0] >= 0 || !WeaponSlots[0])
        {
            if (SkillCurrentlyActivating[1] < 0)
            {
                DecideSkillUse(1);
                return;
            }
            return;
        }
        else if (SkillCurrentlyActivating[1] >= 0 || !WeaponSlots[1])
        {
            if (SkillCurrentlyActivating[0] < 0)
            {
                DecideSkillUse(0);
                return;
            }
            return;
        }

        if (IdleTimer[0] > 0)
        {
            IdleTimer[0] -= Time.deltaTime;
        }
        if (IdleTimer[1] > 0)
        {
            IdleTimer[1] -= Time.deltaTime;
        }
        if (IdleTimer[0] > 0 || IdleTimer[1] > 0)
        {
            return;
        }

        BestSkill[] BestSkills = new BestSkill[2];
        BestSkill CurrentBestSkill = new BestSkill
        {
            Score = -1
        };

        int WeaponSlotID = -1;

        for (int i = 0; i < BestSkills.Length; i++)
        {
            BestSkills[i] = EvaluateBestSkillToUse(i);
        }

        for (int i = 0; i < BestSkills.Length; i++)
        {
            if (BestSkills[i].Score > CurrentBestSkill.Score)
            {
                CurrentBestSkill = BestSkills[i];
                WeaponSlotID = i;
            }
        }

        if (CurrentBestSkill.SkillID >= 0)
        {
            SkillEvaluationCycleTimers[WeaponSlotID] = ItemSkillSlots[CurrentBestSkill.SkillID].AIGetSkillEvaluationCycle();
            SkillContinueActivation[WeaponSlotID] = true;

            SensibleSkillActivationTimes[WeaponSlotID] = ItemSkillSlots[CurrentBestSkill.SkillID].AIGetSensibleActivationTime();
            StartSkillActivation(CurrentBestSkill.SkillID);
        }
        else
        {
            IdleTimer[0] = IdleDuration;
            IdleTimer[1] = IdleDuration;
        }
    }

    public void DecideSkillUse(int WeaponSlotID)
    {
        if (!WeaponSlots[WeaponSlotID])
        {
            return;
        }

        if (SkillCurrentlyActivating[WeaponSlotID] >= 0)
        {
            return;
        }

        if (IdleTimer[WeaponSlotID] > 0)
        {
            IdleTimer[WeaponSlotID] -= Time.deltaTime;
            return;
        }

        int BestSkillID = EvaluateBestSkillToUse(WeaponSlotID).SkillID;

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

    private BestSkill EvaluateBestSkillToUse(int WeaponSlotID)
    {
        DecisionMaker.AIDecision BestSkillDecision = new DecisionMaker.AIDecision
        {
            Score = IdleSkillScore
        };

        DecisionMaker.AIDecision TempSkillDecision = new DecisionMaker.AIDecision();

        BestSkill BestSkillID = new BestSkill();
        BestSkillID.SkillID = -1;

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
                    BestSkillID.SkillID = sk;
                    BestSkillID.Score = BestSkillDecision.Score;
                }
            }
        }

        return BestSkillID;
    }

    public void DecideMovePattern()
    {
 /*       if (MovePatternEvaluationCycleTimer > 0)
        {
            MovePatternEvaluationCycleTimer -= Time.deltaTime;
            return;
        }*/
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

        // Go through basic move patterns:
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

        // Go through weapon move patterns:
        MovePattern[] WeaponMovePatterns;

        for (int i = 0; i < WeaponSlots.Length; i++)
        {
            if (WeaponSlots[i])
            {
                WeaponMovePatterns = WeaponSlots[i].GetMovePatterns();

                for (int j = 0; j < WeaponMovePatterns.Length; j++)
                {
                    TempMovePatternDecision = WeaponMovePatterns[j].AICalculateMovePatternScore(this);

                    if (TempMovePatternDecision.Score > BestMovePatternDecision.Score)
                    {
                        BestMovePatternDecision = TempMovePatternDecision;
                        ActiveMovePattern = WeaponMovePatterns[j];
                        TargetCharacter = BestMovePatternDecision.TargetCharacter;
                    }
                }
            }
        }

        // Go through skill move patterns:
        MovePattern[] SkillMovePatterns;

        for (int i = 0; i < SkillCurrentlyActivating.Length; i++)
        {
            if (SkillCurrentlyActivating[i] >= 0)
            {
                SkillMovePatterns = ItemSkillSlots[SkillCurrentlyActivating[i]].AIGetSkillMovePatterns();

                for (int mp = 0; mp < SkillMovePatterns.Length; mp++)
                {
                    TempMovePatternDecision = SkillMovePatterns[mp].AICalculateMovePatternScore(this);

                    if (TempMovePatternDecision.Score > BestMovePatternDecision.Score)
                    {
                        BestMovePatternDecision = TempMovePatternDecision;
                        ActiveMovePattern = SkillMovePatterns[mp];
                        TargetCharacter = BestMovePatternDecision.TargetCharacter;
                    }
                }
            }
        }

        MovePatternEvaluationCycleTimer = MovePatternEvaluationCycle;
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
          //  MovePatternEvaluationCycleTimer = MovePatternEvaluationCycle;
        }
        else
        {
            MovePatternEvaluationCycleTimer -= Time.deltaTime;
        }
    }

    public void UpdateMovePatternForMissingTarget()
    {
        if (CharAttention.GetPlayersInAttentionRange().Count <= 0)
        {
            ActiveMovePattern = BaseMovePattern;
        }
        else
        {
            DecideMovePattern();
        }

    }

    public int GetBasePowerLevel()
    {
        return BasePowerLevel;
    }

    // ======================================== /AI =========================================

    // =================================== SKILL ACTIVATION ====================================

    public override void FinishedCurrentSkillActivation(int WeaponSlotID, int Hindrance)
    {
        SkillContinueActivation[WeaponSlotID] = false;
        HandAnimators[0].SetTrigger(Anim_BreakAnim);
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
            if ((SkillCurrentlyActivating[WeaponSlotID] != EvaluateBestSkillToUse(0).SkillID)
                && (SkillCurrentlyActivating[WeaponSlotID] != EvaluateBestSkillToUse(1).SkillID))
            {
                return false;
            }

            SkillEvaluationCycleTimers[WeaponSlotID] = ItemSkillSlots[SkillCurrentlyActivating[WeaponSlotID]].AIGetSkillEvaluationCycle();
        }

        return true;
    }

    // =================================== /SKILL ACTIVATION ====================================

}
