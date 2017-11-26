using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEnemy : Character {

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.ENEMIES;

    protected override void Update()
    {
        base.Update();
        DecideSkillUse();
    }

    public override TeamAlignment GetAlignment()
    {
        return Alignment;
    }


    [Header("Enemy AI:")]
    public List<Character> PlayersInAttentionRange = new List<Character>();
    public float IdleSkillScore = 0.2f;
    public float IdleDuration = 0.2f;
    private float IdleTimer = 0.0f;

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

    public void DecideSkillUse()
    {
        if (SkillCurrentlyActivating >= 0)
        {
            return;
        }

        if (IdleTimer > 0)
        {
            IdleTimer -= Time.deltaTime;
            return;
        }

        Debug.Log("DECIDING SKILL USE!");
        DecisionMaker.SkillApplication BestSkillApplication = new DecisionMaker.SkillApplication
        {
            Score = IdleSkillScore
        };

        DecisionMaker.SkillApplication TempSkillApplication = new DecisionMaker.SkillApplication();

        int BestSkillID = -1;

        for (int sk = 0; sk < ItemSkillSlots.Length; sk++)
        {
            if (ItemSkillSlots[sk])
            {
                TempSkillApplication = ItemSkillSlots[sk].AICalculateSkillScoreAndApplication();

                Debug.Log(TempSkillApplication.ISkill.SkillObject + " GOT SCORE: " + TempSkillApplication.Score);

                if (TempSkillApplication.Score > BestSkillApplication.Score)
                {
                    BestSkillApplication = TempSkillApplication;
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
            IdleTimer = IdleDuration;
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

    // ======================================== /AI =========================================

    // =================================== SKILL ACTIVATION ====================================

    protected override void UpdateCurrentSkillActivation()
    {
        if (SkillCurrentlyActivating < 0) { return; }

        SkillActivationTimer += Time.deltaTime;

        ItemSkillSlots[SkillCurrentlyActivating].UpdateSkillActivation(SkillActivationTimer, true);
    }

    // =================================== /SKILL ACTIVATION ====================================

}
