using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArenaGateTrigger : MonoBehaviour
{
    public Vector3 ArenaCenter;
    private NavMeshAgent _wispAgent;
    private NavMeshObstacle _obstacle;
    private ParticleSystem _particles;

    void Awake()
    {
        _particles = GetComponent<ParticleSystem>();
        _obstacle = GetComponent<NavMeshObstacle>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("LightWisp"))
        {
            GetComponent<Collider>().enabled = false;
            Destroy(GetComponent<Rigidbody>());
            var lightWispMovement = col.GetComponent<LightWispMovement>();
            lightWispMovement.StopPlayerFollow();
            _obstacle.enabled = false;
            _wispAgent = col.GetComponent<NavMeshAgent>();
            _wispAgent.stoppingDistance = 1;
            _wispAgent.SetDestination(transform.position);
            _wispAgent.autoBraking = true;
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            StartCoroutine(CheckWallReached());
        }
    }

    IEnumerator CheckWallReached()
    {
        // Wait until agent is stopped
        yield return new WaitUntil(DestinationReached);
        _wispAgent.SetDestination(ArenaCenter);
        yield return new WaitForSeconds(3);
        LevelController.Instance.SwarmlingSpawner.EnteredBossArena();
        yield return new WaitUntil(DestinationReached);
        _particles.Play(true);
        _obstacle.enabled = true;
    }

    private bool DestinationReached()
    {
        if (_wispAgent.remainingDistance != Mathf.Infinity &&
            _wispAgent.pathStatus == NavMeshPathStatus.PathComplete &&
            _wispAgent.remainingDistance <= _wispAgent.stoppingDistance)
            return true;

        return false;
    }
}
