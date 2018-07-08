using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AreaArenaTrigger : MonoBehaviour
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

        // Get Spawner from Wisp agent with GetComponent
        _wispAgent.GetComponent<SwarmSpawner>().ArenaStartSpawning(this, _insidePolygon);

        // Call Spawn Function
        // -> This should start the spawning progress as it is so far, but with the new Polygon Check. Also, a counter so it only spawns until a total number is reached.

        // For Beetle AI: Increase Player Search Radius over time (currently done, but maybe slower and to a larger radius?)
        // Also, no more Kill Range, instead reenable them walking towards the wisp. But only to the outer general area, at which point the player attention should trigger!
        // With player attention and wisp attention, both should have a certain time until they trigger to give the bugs some time to formate.
    }

    public void Initialize(List<GameObject> walls, Vector2[] polygon)
    {
        _walls = walls;
        _insidePolygon = polygon;

        foreach (var wall in _walls)
        {
            wall.GetComponent<NavMeshObstacle>().enabled = false;
            wall.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
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
