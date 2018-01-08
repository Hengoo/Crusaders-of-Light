using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "cons_current_threat", menuName = "Combat/AI/ConsCurrentThreat", order = 30)]
public class ConsOwnThreatLevel : Consideration {

    [Header("Consideration Current Threat:")]
    public float MinDistance = 0.0f;
    public float MaxDistance = 30.0f;

    public float MaxDistanceMelee = 5.0f;

    public float MinThreat = 0.0f;
    public float MaxThreat = 20.0f;

    public override float CalculateScore(Context SkillContext)
    {
        List<Character> PlayersInRange = SkillContext.User.GetAttention().GetPlayersInAttentionRange(MinDistance, MaxDistance);
        List<SkillHitObject> PlayerSkillHitObjectsInRange = SkillContext.User.GetAttention().GetPlayerHitObjectsInAttentionRange(MinDistance, MaxDistanceMelee);

        float InputValue = 0;

        for (int i = 0; i < PlayersInRange.Count; i++)
        {
            if (Vector3.Distance(PlayersInRange[i].transform.position, SkillContext.User.transform.position) <= MaxDistanceMelee)
            {
                InputValue += PlayersInRange[i].GetCurrentThreatLevel(true, false);
            }
            else
            {
                InputValue += PlayersInRange[i].GetCurrentThreatLevel(false, false);
            }
        }

        for (int i = 0; i < PlayerSkillHitObjectsInRange.Count; i++)
        {
            InputValue += PlayerSkillHitObjectsInRange[i].GetCurrentThreat();
        }

        InputValue = ClampInputValue(InputValue, MinThreat, MaxThreat);

        float score = CalculateConsideration(TypeOfCurve, InputValue, Steepness, yShift, xShift);

        return score;
    }

}
