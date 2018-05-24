using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTestSwarm : Singleton<EnemyTestSwarm> {

    public GameObject BoidPrefab1;
    public int SpawnNumber1 = 30;
    public GameObject BoidPrefab2;
    public int SpawnNumber2 = 30;
    public GameObject BoidPrefab3;
    public int SpawnNumber3 = 30;

    public float SeperationFactor = 1f;
    public float AlignmentFactor = 1f;
    public float CohesionFactor = 1f;
    public float DangerFactor = 1f;
    

    // Use this for initialization
    void Start () {
        for (int i = 0; i < SpawnNumber1; i++)
        {
            Instantiate(BoidPrefab1, new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 40)), BoidPrefab1.transform.rotation);
        }
        for (int i = 0; i < SpawnNumber2; i++)
        {
            Instantiate(BoidPrefab2, new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 40)), BoidPrefab2.transform.rotation);
        }
        for (int i = 0; i < SpawnNumber3; i++)
        {
            Instantiate(BoidPrefab3, new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 40)), BoidPrefab3.transform.rotation);
        }

    }
	
}
