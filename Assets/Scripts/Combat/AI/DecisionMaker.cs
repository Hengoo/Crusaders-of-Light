using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "decision_maker", menuName = "Combat/AI/DecisionMaker", order = 0)]
public class DecisionMaker : ScriptableObject {

    // Go through all Skills:

    // Go through all Considerations per Skill:

    // This can have two results:
    // Some Considerations are mostly deciding wether the Skill is used or not at all.
    // Some others, need to be checked for different targets (for example distance).
    // This way, sometimes a Skill is (not) used because no one is in range for example...

    // For this: Keep List of all Players that are in general legit targets:
    // 1: Within a certain Range (This represents the distance an enemy could "see")
    // 2: Line of Sight (So that enemies can't notice the player if there is a large obstacle in the way)
    // (3): If there is no Line of Sight, but there is Line of Sight to another Enemy that does have Line of Sight to a Player. (Could be nice, implement later maybe)

    // Then, check all Considerations against all Targets:

    // Calculate total Skill from those Considerations for that Skill:

    // Choose Skill with best Score:

    // Start Skill Activation:

    public struct SkillApplication
    {
        public ItemSkill ISkill;
        public Character TargetCharacter;
        public float Score;
        public int ConsiderationsCounter;
    }

    public float Weight = 1.0f;

    public Consideration[] ConsiderationsTargeted = new Consideration[0];
    public Consideration[] ConsiderationsSelf = new Consideration[0];

    
    public SkillApplication CalculateTotalScore(ItemSkill SkillToScore)
    {
        // Base Score:
        SkillApplication SkillApp = new SkillApplication
        {
            ISkill = SkillToScore,
            TargetCharacter = null,
            Score = 1,
            ConsiderationsCounter = 0
        };

        // Score of Self Targeted Considerations:
        Consideration.Context TempContext = new Consideration.Context
        {
            Skill = SkillApp.ISkill,
            User = SkillApp.ISkill.GetCurrentOwner(),
            Target = SkillApp.ISkill.GetCurrentOwner()
        };

        for (int i = 0; i < ConsiderationsSelf.Length; i++)
        {
            SkillApp.Score = SkillApp.Score * ConsiderationsSelf[i].CalculateScore(TempContext);
        }

        SkillApp.ConsiderationsCounter += ConsiderationsSelf.Length;

        // Score of Targeted Considerations:

        CharacterEnemy User = (CharacterEnemy)(SkillApp.ISkill.GetCurrentOwner());

        List<Character> PlayersInAttentionRange = User.GetPlayersInAttentionRange();

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
                SkillApp.TargetCharacter = TempContext.Target;
            }
        }

        SkillApp.Score *= TempBestScore;
        SkillApp.ConsiderationsCounter += ConsiderationsTargeted.Length;

        // Compensation based on number of multiplications:

        if (SkillApp.ConsiderationsCounter > 0)
        {
            float ModFactor = 1 - (1 / (float)(SkillApp.ConsiderationsCounter));
            float CompensationValue = (1 - SkillApp.Score) * ModFactor;
            SkillApp.Score += CompensationValue * SkillApp.Score;
        }

        SkillApp.Score *= Weight;

        return SkillApp;
    }
}
