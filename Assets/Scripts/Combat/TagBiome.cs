using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
