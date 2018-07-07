using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BiomeArenaTrigger : MonoBehaviour
{
    private List<GameObject> _walls;
    private NavMeshAgent _wispAgent;
    private Vector2[] _insidePolygon;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("LightWisp"))
        {
            // Grab wisp agent
            _wispAgent = col.GetComponent<NavMeshAgent>();
            col.GetComponent<LightWispMovement>().StopPlayerFollow();

            // Disable this trigger
            GetComponent<Collider>().enabled = false;
            Destroy(GetComponent<Rigidbody>());

            // Teleport all players outside of the arena to the center;
            foreach (var player in LevelController.Instance.GetActivePlayers())
            {
                Vector2 position2D = new Vector2(player.transform.position.x, player.transform.position.z);
                if (!position2D.IsInsidePolygon(_insidePolygon))
                    player.transform.position = transform.position;
            }
            
            // Enable all walls to start blocking
            foreach (var wall in _walls)
            {
                wall.GetComponent<NavMeshObstacle>().enabled = true;
                wall.GetComponent<ParticleSystem>().Play();

                _wispAgent.stoppingDistance = 1;
                _wispAgent.SetDestination(transform.position);
                _wispAgent.autoBraking = true;
            }

            // Start spawning bugs!
            StartArena();
        }
    }

    private void StartArena()
    {
        // TODO: florian
        // BTW: you can check if a bug is inside the arena in the same way I did with the players in the "OnTriggerEnter" function
    }

    public void Initialize(List<GameObject> walls, Vector2[] polygon)
    {
        _walls = walls;
        _insidePolygon = polygon;
    }

    public void OpenArena()
    {
        foreach (var wall in _walls)
        {
            wall.GetComponent<NavMeshObstacle>().enabled = false;
            wall.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);

            _wispAgent.GetComponent<LightWispMovement>().ResumePlayerFollow();
        }
    }

}
