using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LightWisp : MonoBehaviour
{

    public float MoveSpeed = 15;
    public float PlayerDistance = 5;

    private GameObject _currentPlayer;
    private NavMeshAgent _agent;


    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = MoveSpeed;
        _agent.acceleration = 100;
    }

    void Start()
    {
        _currentPlayer = LevelController.Instance.GetActivePlayers()[0];
    }
    
    void Update()
    {
        var playerPosition = _currentPlayer.transform.position;
        _agent.isStopped = (transform.position - playerPosition).sqrMagnitude < PlayerDistance * PlayerDistance;
        _agent.SetDestination(playerPosition - (transform.position - playerPosition).normalized * PlayerDistance);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            _currentPlayer = col.gameObject;
        }
    }
}
