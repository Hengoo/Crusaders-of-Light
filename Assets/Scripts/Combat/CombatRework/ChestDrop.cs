using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestDrop : MonoBehaviour {

    public Weapon DroppedWeapon;
    public ElementItem DroppedElement;

    public GameObject SpawnEffect;
    private GameObject SpawnEffectInstance;
    public Transform SpawnPoint;

    private void Start()
    {
        SpawnEffectInstance = Instantiate(SpawnEffect, SpawnPoint);
    }

    public Weapon GetDroppedWeapon()
    {
        return DroppedWeapon;
    }

    public ElementItem GetDroppedElement()
    {
        return DroppedElement;
    }

}
