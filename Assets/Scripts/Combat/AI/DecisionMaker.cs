using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionMaker : ScriptableObject {

    public struct AIDecision
    {
        public Character TargetCharacter;
        public float Score;
        public int ConsiderationsCounter;
    }

    [Header("Decision Maker Considerations:")]
    public float Weight = 1.0f;

    public Consideration[] ConsiderationsTargeted = new Consideration[0];
    public Consideration[] ConsiderationsSelf = new Consideration[0];



    public AIDecision CalculateTotalScore(Character Self)
    {
        return CalculateTotalScore(Self, null);
    }

    public AIDecision CalculateTotalScore(Character Self, ItemSkill SkillToScore)   // Note : The Skill Part of Context is optional. Beware, that Considerations that use the Skill cannot be used for Move Pattern Scoring!
    {
        // Base Score:
        AIDecision Decision = new AIDecision
        {
            TargetCharacter = null,
            Score = 1,
            ConsiderationsCounter = 0
        };

        CharacterEnemy User = (CharacterEnemy)(Self);
        List<Character> PlayersInAttentionRange = User.GetAttention().GetPlayersInAttentionRange();

        if (PlayersInAttentionRange.Count <= 0)
        {
            Decision.Score = 0;
            return Decision;
        }

        // Score of Self Targeted Considerations:
        Consideration.Context TempContext = new Consideration.Context
        {
            Skill = SkillToScore,
            User = Self,
            Target = Self
        };

        for (int i = 0; i < ConsiderationsSelf.Length; i++)
        {
            Decision.Score = Decision.Score * ConsiderationsSelf[i].CalculateScore(TempContext);
        }

        Decision.ConsiderationsCounter += ConsiderationsSelf.Length;

        // Score of Targeted Considerations:

        float TempScore = 1;
        float TempBestScore = 0;

        // Calculate Score of Skill with each Player as target:
        for (int plT = 0; plT < PlayersInAttentionRange.Count; plT++)
        {
            TempScore = 1;
            TempContext.Target = PlayersInAttentionRange[plT];

            for (int i = 0; i < ConsiderationsTargeted.Length; i++)
            {
                TempScore = TempScore * ConsiderationsTargeted[i].CalculateScore(TempContext);
            }

            if (TempScore > TempBestScore)
            {
                TempBestScore = TempScore;
                Decision.TargetCharacter = TempContext.Target;
            }
        }

        Decision.Score *= TempBestScore;
        Decision.ConsiderationsCounter += ConsiderationsTargeted.Length;

        // Compensation based on number of multiplications:

        if (Decision.ConsiderationsCounter > 0)
        {
            float ModFactor = 1 - (1 / (float)(Decision.ConsiderationsCounter));
            float CompensationValue = (1 - Decision.Score) * ModFactor;
            Decision.Score += CompensationValue * Decision.Score;
        }

        Decision.Score *= Weight;

        //Debug.Log("TOTAL SCORE: " + Decision.Score);

        return Decision;
    }
}
