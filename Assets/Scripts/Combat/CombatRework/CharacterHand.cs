using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHand : MonoBehaviour {

    public bool[] ActivateEffect = new bool[2];

    public bool TriggerActivateEffect(int ID)
    {
        bool temp = ActivateEffect[ID];
        ActivateEffect[ID] = false;
        return temp;
    }

    public void StartActivateEffect(int ID)
    {
        Debug.Log("startActivationeffect");
        ActivateEffect[ID] = true;
    }
	
}
