using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTestSwarm : MonoBehaviour {

    public GameObject BoidPrefab;
    public int SpawnNumber = 30;

	// Use this for initialization
	void Start () {
        for (int i = 0; i < SpawnNumber; i++)
        {
            Instantiate(BoidPrefab, new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 40)), BoidPrefab.transform.rotation);
        }

	}
	
}
