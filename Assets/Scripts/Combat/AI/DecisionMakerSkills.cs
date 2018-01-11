using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "decision_maker_skills", menuName = "Combat/AI/DecisionMakerSkills", order = 0)]
public class DecisionMakerSkills : DecisionMaker {

    [Header("Decision Maker Skills::")]
    public float SensibleActivationTime = -1.0f; // After this amount of time the AI stops "pressing the button". Skills with fixed Activation Times are not interrupted, Charge up skills are released. Negative: Ignore.
    public float SkillEvaluationCycle = -1.0f;

    [Header("Decision Maker Skills Move Patterns:")]
    public MovePattern[] AISkillMovePatterns;

    public float AIGetSensibleActivationTime()
    {
        return SensibleActivationTime;
    }

    public MovePattern[] AIGetSkillMovePatterns()
    {
        return AISkillMovePatterns;
    }

    public float AIGetSkillEvaluationCycle()
    {
        return SkillEvaluationCycle;
    }
}
