using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionMaker : MonoBehaviour {

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

    public void CalculateTotalScore()
    {

    }
}
