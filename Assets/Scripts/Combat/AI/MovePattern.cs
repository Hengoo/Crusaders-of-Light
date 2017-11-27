using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePattern : ScriptableObject {

    public DecisionMaker AIDecisionMaker;

    public DecisionMaker.AIDecision AICalculateMovePatternScore(Character Self)
    {
        return AIDecisionMaker.CalculateTotalScore(Self);
    }

    public virtual void UpdateMovePattern(Character Self, Character TargetCharacter)
    {

    }
}
