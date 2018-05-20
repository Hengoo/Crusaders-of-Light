using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySwarm : MonoBehaviour {

    public NavMeshAgent NMAgent;

    public float SeperationFactor = 1;
    public float SeperationTurnSpeed = 10;
    public float AlignmentFactor = 1;
    public float AlignmentTurnSpeed = 10;
    public float CohesionFactor = 1;
    public float CohesionTurnSpeed = 10;

    public float Speed = 1f;

    public Vector3 MovVector;

    public Vector3 GoalPosition;

    public List<GameObject> EnemiesInRange = new List<GameObject>();

    private void Update()
    {
        RandomMove();
        Swarm();
        // NMAgent.Move(Vector3.forward * Time.deltaTime * Speed);
        NMAgent.SetDestination(GoalPosition);
    }

    private void RandomMove()
    {
        if (Random.Range(0, 1000) <= 5)
        {
            MovVector = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            //MovVector.Normalize();    
        }
        if (MovVector != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(MovVector), Time.deltaTime);
        }
        
    }

    private void Swarm()
    {
        int NumberOfOthers = EnemiesInRange.Count;

        
        Vector3 CohesionVec = Vector3.zero;
        
        Vector3 SeperationVec = Vector3.zero;
        Vector3 TempVec = Vector3.zero;

        Vector3 AlignmentVec = Vector3.zero;

        for (int i = 0; i < NumberOfOthers; i++)
        {
            // Cohesion:
            CohesionVec += EnemiesInRange[i].transform.position;

            // Seperation:
            TempVec = transform.position - EnemiesInRange[i].transform.position;
            if (TempVec.magnitude < 1)
            {
                SeperationVec += TempVec;
            }

            // Alignment:
            TempVec = EnemiesInRange[i].transform.rotation * Vector3.forward;
            AlignmentVec += TempVec;
        }
        // Cohesion:
        CohesionVec = CohesionVec / NumberOfOthers;
        CohesionVec = CohesionVec - this.transform.position;

        if (CohesionVec.magnitude > 0.00001)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(CohesionVec), CohesionTurnSpeed * Time.deltaTime * CohesionFactor);
        }

        // Seperation:
        SeperationVec = SeperationVec / NumberOfOthers;
        
        if (SeperationVec.magnitude > 0.00001)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(SeperationVec), SeperationTurnSpeed * Time.deltaTime * SeperationFactor);
        }

        // Alignment:
        AlignmentVec = AlignmentVec / NumberOfOthers;

        if (AlignmentVec.magnitude > 0.00001)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(AlignmentVec), AlignmentTurnSpeed * Time.deltaTime * AlignmentFactor);
        }



        GoalPosition = transform.position + transform.rotation * Vector3.forward;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EnemySwarm")
        {
            EnemiesInRange.Add(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "EnemySwarm")
        {
            EnemiesInRange.Remove(other.gameObject);
        }
    }

    public void OnDestroy()
    {
        for (int i = 0; i < EnemiesInRange.Count; i++)
        {
            EnemiesInRange[i].GetComponent<EnemySwarm>().RemoveFromList(this.gameObject);
        }
    }

    public void RemoveFromList(GameObject SwarmObject)
    {
        EnemiesInRange.Remove(SwarmObject);
    }
}
