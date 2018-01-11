using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePattern : ScriptableObject {

    public DecisionMakerMoves AIDecisionMaker;

    public DecisionMaker.AIDecision AICalculateMovePatternScore(Character Self)
    {
        return AIDecisionMaker.CalculateTotalScore(Self);
    }

    public virtual void UpdateMovePattern(PhysicsController PhysCont, CharacterEnemy Self, Character TargetCharacter)
    {

    }
}
