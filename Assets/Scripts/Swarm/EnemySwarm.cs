using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySwarm : MonoBehaviour {

    public NavMeshAgent NMAgent;

    public float UpdatePercentage = 0.5f;

    [Header("Core Rules:")]
    public float SeperationDistance = 3;
    public float SeperationFactor = 1;
    private float SeperationTurnSpeed = 10;

    public float AlignmentDistance = 3;
    public float AlignmentFactor = 1;
    private float AlignmentTurnSpeed = 10;

    public float CohesionDistance = 3;
    public float CohesionFactor = 1;
    private float CohesionTurnSpeed = 10;

    [Header("Advanced Rules:")]
    public float DangerDistance = 3;
    public float DangerFactor = 1;
    private float DangerTurnSpeed = 1;


    [Header("Movement:")]
    public float Speed = 1f;

    public Vector3 MovVector;

    public Vector3 GoalVector;
    public float GoalFactor;

    public Vector3 GoalPosition;

    [Header("Lists:")]
    public List<EnemySwarm> EnemiesInRange = new List<EnemySwarm>();
    public List<GameObject> DangerInRange = new List<GameObject>();

    private void FixedUpdate()
    {
        GoalVector = transform.rotation * Vector3.forward;
        GoalFactor = 1;

        RandomMove();
        if (Random.Range(0f, 1f) <= UpdatePercentage)
        {
            Swarm();
        }

        DangerAvoidance();

        //GoalPosition = transform.position + transform.rotation * Vector3.forward;
        //GoalPosition = transform.position + GoalVector;

        GoalVector = GoalVector / GoalFactor;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(GoalVector), Time.deltaTime * 360);
        

        // NMAgent.Move(Vector3.forward * Time.deltaTime * Speed);
        NMAgent.Move(transform.rotation * Vector3.forward * Time.deltaTime * Speed);
        //NMAgent.SetDestination(GoalPosition);
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
        int CohesionNumber = 0;
        
        Vector3 SeperationVec = Vector3.zero;
        int SeperationNumber = 0;

        Vector3 AlignmentVec = Vector3.zero;
        int AlignmentNumber = 0;
        float OthersSpeed = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = transform.position - EnemiesInRange[i].transform.position;
            DistanceVecMag = DistanceVec.magnitude;

            // Cohesion:
            if (DistanceVecMag <= CohesionDistance)
            {
                CohesionVec += EnemiesInRange[i].transform.position;
                CohesionNumber++;
            }
  
            // Seperation:
            if (DistanceVecMag <= SeperationDistance)
            {
                SeperationVec += DistanceVec;
                SeperationNumber++;
            }

            // Alignment:
            if (DistanceVecMag <= AlignmentDistance)
            {
                AlignmentVec += EnemiesInRange[i].transform.rotation * Vector3.forward;
                AlignmentNumber++;
                OthersSpeed += EnemiesInRange[i].GetCurrentSpeed();
            }
        }
        // Cohesion:
        if (CohesionNumber >= 1)
        {
            CohesionVec = CohesionVec / CohesionNumber;
            CohesionVec = CohesionVec - this.transform.position;

            if (CohesionVec.magnitude > 0.00001)
            {
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(CohesionVec), CohesionTurnSpeed * Time.deltaTime * CohesionFactor);
                //GoalVector = Vector3.Slerp(GoalVector, CohesionVec, CohesionTurnSpeed * Time.deltaTime * CohesionFactor);
                GoalVector += CohesionVec * CohesionFactor;
                GoalFactor += CohesionFactor;
            }
        }
        
        // Seperation:
        if (SeperationNumber >= 1)
        {
            SeperationVec = SeperationVec / SeperationNumber;

            if (SeperationVec.magnitude > 0.00001)
            {
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(SeperationVec), SeperationTurnSpeed * Time.deltaTime * SeperationFactor);
                //GoalVector = Vector3.Slerp(GoalVector, SeperationVec, SeperationTurnSpeed * Time.deltaTime * SeperationFactor);
                GoalVector += SeperationVec * SeperationFactor;
                GoalFactor += SeperationFactor;
            }
        }

        // Alignment:
        if (AlignmentNumber >= 1)
        {
            AlignmentVec = AlignmentVec / AlignmentNumber;
            //AlignmentVec *= (Speed * (OthersSpeed/AlignmentNumber));
            Speed = Mathf.Lerp(Speed, (OthersSpeed / AlignmentNumber), Time.deltaTime);

            if (AlignmentVec.magnitude > 0.00001)
            {
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(AlignmentVec), AlignmentTurnSpeed * Time.deltaTime * AlignmentFactor);
                //GoalVector = Vector3.Slerp(GoalVector, AlignmentVec, AlignmentTurnSpeed * Time.deltaTime * AlignmentFactor);
                GoalVector += AlignmentVec * AlignmentFactor;
                GoalFactor += AlignmentFactor;
            }
        }

        //GoalPosition = transform.position + transform.rotation * Vector3.forward;
    }

    private void DangerAvoidance()
    {
        int NumberOfDangers = DangerInRange.Count;

        Vector3 DangerVec = Vector3.zero;
        Vector3 DistanceVec = Vector3.zero;
        int DistanceVecNumber = 0;

        for (int i = 0; i < NumberOfDangers; i++)
        {
            DistanceVec = transform.position - DangerInRange[i].transform.position;
            if (DistanceVec.magnitude < DangerDistance)
            {
                DangerVec += DistanceVec;
                DistanceVecNumber++;
            }
        }

        if (DistanceVecNumber >= 1)
        {
            DangerVec = DangerVec / NumberOfDangers;

            if (DangerVec.magnitude > 0.00001)
            {
                // transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(DangerVec), DangerTurnSpeed * Time.deltaTime * DangerFactor);
                //GoalVector = Vector3.Slerp(GoalVector, DangerVec, DangerTurnSpeed * Time.deltaTime * DangerFactor);
                GoalVector += DangerVec * DangerFactor;
                GoalFactor += DangerFactor;
            }
        }
       
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EnemySwarm")
        {
            EnemiesInRange.Add(other.GetComponent<EnemySwarm>());
        }
        else if (other.tag == "SwarmDanger")
        {
            DangerInRange.Add(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "EnemySwarm")
        {
            EnemiesInRange.Remove(other.GetComponent<EnemySwarm>());
        }
        else if (other.tag == "SwarmDanger")
        {
            DangerInRange.Remove(other.gameObject);
        }
    }

    public void OnDestroy()
    {
      /*  for (int i = 0; i < EnemiesInRange.Count; i++)
        {
            EnemiesInRange[i].GetComponent<EnemySwarm>().RemoveFromList(this.gameObject);
        }*/
    }

    public void RemoveFromList(EnemySwarm SwarmObject)
    {
        EnemiesInRange.Remove(SwarmObject);
    }

    public float GetCurrentSpeed()
    {
        return Speed;
    }
}
