using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTestSwarm : Singleton<EnemyTestSwarm> {

    public bool Spawn = false;

    public EnemySwarm BoidPrefab1;
    public int SpawnNumber1 = 30;
    public GameObject BoidPrefab2;
    public int SpawnNumber2 = 30;
    public GameObject BoidPrefab3;
    public int SpawnNumber3 = 30;

    public float SeperationFactor = 1f;
    public float AlignmentFactor = 1f;
    public float CohesionFactor = 1f;
    public float DangerFactor = 1f;

    public GameObject GlobalAttractionTarget;
    public Terrain Terr;
    private Vector3 spawnPos = Vector3.zero;
    private NavMeshHit hit;
    private EnemySwarm[] SpawnedSwarmlings = new EnemySwarm[0];

    private void Update()
    {
        for (int i = 0; i < SpawnedSwarmlings.Length; i++)
        {
            if (SpawnedSwarmlings[i])
            {
                SpawnedSwarmlings[i].SwarmlingFixedUpdate();
            }
        }
    }

    public EnemySwarm GetSwarmling(int id)
    {
        return SpawnedSwarmlings[id];
    }
    

    // Use this for initialization
    void Start () {
        if (!Spawn)
        {
            return;
        }
        SpawnedSwarmlings = new EnemySwarm[SpawnNumber1];
        
        

        for (int i = 0; i < SpawnNumber1; i++)
        {
            GenerateSpawnPosition();
            SpawnedSwarmlings[i] = Instantiate(BoidPrefab1, spawnPos, BoidPrefab1.transform.rotation);
            SpawnedSwarmlings[i].InitializeSwarmling(i);
        }
        for (int i = 0; i < SpawnNumber2; i++)
        {
            GenerateSpawnPosition();
            Instantiate(BoidPrefab2, spawnPos, BoidPrefab2.transform.rotation);
        }
        for (int i = 0; i < SpawnNumber3; i++)
        {
            GenerateSpawnPosition();
            Instantiate(BoidPrefab3, spawnPos, BoidPrefab3.transform.rotation);
        }

    }

    private void GenerateSpawnPosition()
    {
        spawnPos.x = Random.Range(-30, 30) + transform.position.x;
        spawnPos.z = Random.Range(-30, 30) + transform.position.z;
        spawnPos.y = 0;
        spawnPos.y = Terr.SampleHeight(spawnPos);

        NavMesh.SamplePosition(spawnPos, out hit, 10, NavMesh.AllAreas);

        spawnPos = hit.position;

        if (spawnPos.x == Mathf.Infinity)
        {
            GenerateSpawnPosition();
        }
    }
	
}
