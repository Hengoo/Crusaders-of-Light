﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MiniBoss : MonoBehaviour {

    public ChestBossTrigger GuardedChest;

    public ChestDrop GuardedChestDrop;

    private NavMeshHit hit;

    public void InitializeMiniBoss(ChestBossTrigger NewGuardedChest)
    {
        GuardedChest = NewGuardedChest;

       // NavMesh.SamplePosition(transform.position, out hit, 3, NavMesh.AllAreas);
       // transform.position = hit.position;

        if (LevelController.Instance)
        {
            LevelController.Instance.GetSwarmSpawner().ArenaInBossAreaStart(false);
        }
    }

    private void OnDestroy()
    {
        if (LevelController.Instance)
        {
            LevelController.Instance.GetSwarmSpawner().ArenaFinishedBossFight();
        }

        GuardedChest.OnMiniBossGuardDeath(GuardedChestDrop);
    }

}
