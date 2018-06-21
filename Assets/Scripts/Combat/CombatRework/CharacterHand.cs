using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHand : MonoBehaviour {

    public bool ActivateEffect = false;

    public bool TriggerActivateEffect()
    {
        bool temp = ActivateEffect;
        ActivateEffect = false;
        return temp;
    }

    public void StartActivateEffect()
    {
        ActivateEffect = true;
    }
	
}
