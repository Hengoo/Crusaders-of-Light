using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LightWispMovement : MonoBehaviour
{
    private GameObject _target;
    private NavMeshAgent _agent;


    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        _target = LevelController.Instance.GetActivePlayers()[0];
    }

    void Update()
    {
        _agent.SetDestination(_target.transform.position);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            _target = col.gameObject;
        }
    }
}
