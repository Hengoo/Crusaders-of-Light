using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBoss : MonoBehaviour {

    public ElementItem element;

    private void OnDestroy()
    {
        if (GameController.Instance)
        {
            GameController.Instance.UnlockElement(element);
        }
    }
}
