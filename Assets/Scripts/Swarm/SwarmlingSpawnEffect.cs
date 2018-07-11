using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmlingSpawnEffect : MonoBehaviour {

    public ParticleSystem ParticleSystemPrefab;
    public GameObject ParticleSystemSpawnPoint;

    private void Start()
    {
        Instantiate(ParticleSystemPrefab, ParticleSystemSpawnPoint.transform);

        Destroy(gameObject, ParticleSystemPrefab.main.duration);
    }

}
