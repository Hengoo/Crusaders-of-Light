using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDeathTimer : MonoBehaviour {

    [Header("Character Death Timer:")]

    public float DeathDuration = 1f;
    private float DeathCounter = 0f;

    private void Update()
    {
        DeathCounter += Time.deltaTime;

        if (DeathCounter >= DeathDuration)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            this.enabled = false;
        }
    }

    public void StartDeathTimer()
    {
        DeathCounter = 0f;
        this.enabled = true;
    }
}
