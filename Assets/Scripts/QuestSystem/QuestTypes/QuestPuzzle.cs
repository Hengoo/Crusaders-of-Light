using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleQuest", menuName = "Quests/Puzzle")]
public class QuestPuzzle : QuestBase
{
    public PuzzleBase Puzzle;

    protected override void QuestStarted()
    {
        Puzzle.SubscribeAction(OnQuestCompleted);
        //TODO: ??
    }

    protected override void QuestCompleted()
    {
        //TODO: ??
    }
}
