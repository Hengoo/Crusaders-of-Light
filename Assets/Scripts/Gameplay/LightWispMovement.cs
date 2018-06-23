using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LightWispMovement : MonoBehaviour
{
    private float _stoppingDistance;
    private GameObject _target;
    private NavMeshAgent _agent;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _stoppingDistance = _agent.stoppingDistance;
    }

    void Start()
    {
        _target = LevelController.Instance.GetActivePlayers()[0];
    }

    void Update()
    {
        if(_target)
            _agent.SetDestination(_target.transform.position);
    }

    public void StopPlayerFollow()
    {
        _target = null;
        GetComponent<Collider>().enabled = false;
        
    }

    public void ResumePlayerFollow()
    {
        GetComponent<Collider>().enabled = true;
        _agent.stoppingDistance = _stoppingDistance;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            _target = col.gameObject;
        }
    }
}
