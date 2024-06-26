﻿using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

public class LightWispMovement : MonoBehaviour
{
    private float _stoppingDistance;
    private GameObject _target;
    private NavMeshAgent _agent;
    private float _moveSpeed;
    private Vector3 _playerDirection;

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

    void FixedUpdate()
    {
        if (_target)
        {
            _agent.SetDestination(_target.transform.position);
            _playerDirection = _target.transform.position - _agent.transform.position;
            _playerDirection.Normalize();
        }
    }

    public Vector3 GetPlayerHeading()
    {
        return _playerDirection;
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
