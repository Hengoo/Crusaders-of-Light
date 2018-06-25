using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LightWispMovement : MonoBehaviour
{
    private float _stoppingDistance;
    private GameObject _target;
    private NavMeshAgent _agent;
    private float _moveSpeed;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _stoppingDistance = _agent.stoppingDistance;
        _moveSpeed = _agent.speed;
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
        _agent.speed = 4;
    }

    public void ResumePlayerFollow()
    {
        GetComponent<Collider>().enabled = true;
        _agent.stoppingDistance = _stoppingDistance;
        _agent.speed = _moveSpeed;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            _target = col.gameObject;
        }
    }
}
