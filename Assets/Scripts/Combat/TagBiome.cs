using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner_Tag_Biome", menuName = "Spawner/SpawnerTagBiome", order = 9)]
public class TagBiome : ScriptableObject {

    public bool IsSameAs(TagBiome OtherTag)
    {
        if (OtherTag == this)
        {
            return true;
        }
        return false;
    }


}
