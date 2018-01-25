using UnityEngine;
using UnityEngine.Events;

/*
 * This class serves as a base for all area challenges and puzzle recipies
 */
public abstract class AreaBase : ScriptableObject
{
    public abstract QuestBase[] GenerateQuests(sceneryStructure sceneryStructure, int assignedArea);
}