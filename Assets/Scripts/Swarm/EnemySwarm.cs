using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySwarm : MonoBehaviour {

    public enum SwarmType
    {
        STANDARD = 1,
        SPIKE = 2,
        RANGED = 3
    }

    public NavMeshAgent NMAgent;

    public float UpdatePercentage = 0.5f;

    public SwarmType SType = SwarmType.STANDARD;

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

    public bool BorderOn = false;
    //public float OutsideAcceleration = 1;
    public float BorderDistance = 3;
    public float BorderFactor = 1;


    [Header("Movement:")]
    public Vector3 Velocity = Vector3.zero;
    public Vector3 Acceleration = Vector3.forward;
    public float Resistance = 0.1f;
    public float Friction = 0.1f;


    public float BaseSpeed = 1f;
    public float Speed = 1f;
    public float BonusSpeedThisFrame = 0f;

    public Vector3 MovVector;

    public Vector3 GoalVector;
    public float GoalFactor;

    public Vector3 GoalPosition;

    public float TurnSpeed = 360;
    public float BaseAcceleration = 10f;

    [Header("Movement 2:")]
    public float DesiredBaseSpeed = 6;
    public float DesiredRunSpeed = 12;

    public bool NoSeperationThisUpdate = false;

    public float AttractionDistance = 4;
    public float AttractionFactor = 1;

    [Header("Lists:")]
    public List<EnemySwarm> EnemiesInRange = new List<EnemySwarm>();
    public List<GameObject> DangerInRange = new List<GameObject>();
    public List<GameObject> PlayersInRange = new List<GameObject>();

    private void FixedUpdate()
    {
        

        BonusSpeedThisFrame = 0f;
        Speed = Mathf.Lerp(Speed, BaseSpeed, Time.deltaTime);

        GoalVector = Vector3.zero;
        GoalFactor = 0;

        //RandomMove();
        if (Random.Range(0f, 1f) <= UpdatePercentage)
        {
            // Reset Acceleration:
            Acceleration = Vector3.zero;

            if (BorderOn)
            {
                RuleGoToBorder();
            }

            RuleAttraction();
            RuleCohesion();
            RuleSeperation();
            RuleAlignment();
            RuleDangerAvoidance();
        }

       

       

        //GoalPosition = transform.position + transform.rotation * Vector3.forward;
        //GoalPosition = transform.position + GoalVector;

        /*if (GoalFactor != 0)
        {
            GoalVector = GoalVector / GoalFactor;      
            Acceleration = Vector3.Lerp(Acceleration, GoalVector, Time.deltaTime * TurnSpeed);
        }*/

        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Velocity), Time.deltaTime * TurnSpeed);

        //Velocity += Acceleration * Time.deltaTime;
        //Velocity *= Resistance;
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Velocity), Time.deltaTime * TurnSpeed);

        if (GoalFactor > 0)
        {
            Acceleration = Acceleration / GoalFactor;
        }
        

        // Update Velocity:
        Velocity += Acceleration * Time.deltaTime * 10;
        Velocity *= (1 - Friction * Time.deltaTime);

        // Move:
        NMAgent.Move(Velocity * Time.deltaTime);

        // Rotate towards Velocity Direction:
        if (Velocity.sqrMagnitude > 0)
        {
            transform.rotation = Quaternion.LookRotation(Velocity);
        }




        // NMAgent.Move(Vector3.forward * Time.deltaTime * Speed);
        //NMAgent.Move(transform.rotation * Vector3.forward * Time.deltaTime * (Speed + BonusSpeedThisFrame));
        //NMAgent.SetDestination(GoalPosition);
    }

    public Vector3 Steer(Vector3 VelDesired)
    {
        return VelDesired - Velocity;
    }

    public float GetDesiredSpeed()
    {
        return DesiredBaseSpeed;
    }

    public float GetDesiredRunSpeed()
    {
        return DesiredRunSpeed;
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

    private void RuleCohesion()
    {
        int NumberOfOthers = EnemiesInRange.Count;

        Vector3 CohesionVec = Vector3.zero;
        int CohesionNumber = 0;

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
        }
        // Cohesion:
        if (CohesionNumber >= 1)
        {
            CohesionVec = CohesionVec / CohesionNumber;
            CohesionVec = CohesionVec - this.transform.position;
            CohesionVec = CohesionVec.normalized * GetDesiredSpeed();

            CohesionVec = Steer(CohesionVec);
            Acceleration += CohesionVec * EnemyTestSwarm.Instance.CohesionFactor;
            GoalFactor += EnemyTestSwarm.Instance.CohesionFactor;
        }
    }

    private void RuleSeperation()
    {
        if (NoSeperationThisUpdate)
        {
            NoSeperationThisUpdate = false;
            return;
        }

        int NumberOfOthers = EnemiesInRange.Count;

        Vector3 SeperationVec = Vector3.zero;
        int SeperationNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = transform.position - EnemiesInRange[i].transform.position;
            DistanceVecMag = DistanceVec.magnitude;

            // Seperation:
            if (DistanceVecMag <= SeperationDistance)
            {
                SeperationVec += DistanceVec.normalized / DistanceVecMag;
                SeperationNumber++;
            }
        }

        // Seperation:
        if (SeperationNumber >= 1)
        {
            SeperationVec = SeperationVec.normalized * GetDesiredSpeed();

            SeperationVec = Steer(SeperationVec);
            Acceleration += SeperationVec * EnemyTestSwarm.Instance.SeperationFactor;
            GoalFactor += EnemyTestSwarm.Instance.SeperationFactor;
        }
    }

    private void RuleAlignment()
    {
        int NumberOfOthers = EnemiesInRange.Count;

        Vector3 AlignmentVec = Vector3.zero;
        int AlignmentNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = transform.position - EnemiesInRange[i].transform.position;
            DistanceVecMag = DistanceVec.magnitude;

            // Alignment:
            if (DistanceVecMag <= AlignmentDistance)
            {
                AlignmentVec += EnemiesInRange[i].GetCurrenVelocity();
                AlignmentNumber++;
            }
        }

        // Alignment:
        if (AlignmentNumber >= 1)
        {
            AlignmentVec = AlignmentVec.normalized * GetDesiredSpeed();

            AlignmentVec = Steer(AlignmentVec);
            Acceleration += AlignmentVec * EnemyTestSwarm.Instance.AlignmentFactor;
            GoalFactor += EnemyTestSwarm.Instance.AlignmentFactor;
        }

    }

    private void RuleDangerAvoidance()
    {
        int NumberOfDangers = DangerInRange.Count;

        Vector3 DangerVec = Vector3.zero;
        int DangerVecNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;
        
        for (int i = 0; i < NumberOfDangers; i++)
        {
            DistanceVec = transform.position - DangerInRange[i].transform.position;
            DistanceVecMag = DistanceVec.magnitude;

            if (DistanceVecMag <= DangerDistance)
            {
                DangerVec += DistanceVec.normalized / DistanceVecMag;
                DangerVecNumber++;
            }
        }

        if (DangerVecNumber >= 1)
        {
            DangerVec = DangerVec.normalized * GetDesiredRunSpeed();

            DangerVec = Steer(DangerVec);
            Acceleration += DangerVec * EnemyTestSwarm.Instance.DangerFactor;
            GoalFactor += EnemyTestSwarm.Instance.DangerFactor;
        }
    }

    private void RuleAttraction()
    {
        int NumberOfOthers = PlayersInRange.Count;

        Vector3 AttractionVec = Vector3.zero;
        float AttractionVecMag = AttractionDistance + 1;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;
        
        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = PlayersInRange[i].transform.position - transform.position;
            DistanceVecMag = DistanceVec.magnitude;

            if (DistanceVecMag <= AttractionDistance
                && DistanceVecMag < AttractionVecMag)
            {
                AttractionVec = DistanceVec;
                AttractionVecMag = DistanceVecMag;
            }
        }

        if (AttractionVecMag < AttractionDistance + 1)
        {
            AttractionVec = AttractionVec.normalized * GetDesiredSpeed();

            AttractionVec = Steer(AttractionVec);
            Acceleration += AttractionVec * AttractionFactor;
            GoalFactor += AttractionFactor;
        }
    }

    private void RuleGoToBorder()
    {
        int NumberOfOthers = EnemiesInRange.Count;

        Vector3 BorderVec = Vector3.zero;
        int BorderNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = transform.position - EnemiesInRange[i].transform.position;
            DistanceVecMag = DistanceVec.magnitude;

            if (DistanceVecMag <= BorderDistance && EnemiesInRange[i].GetSwarmType() != SType)
            {
                BorderVec += EnemiesInRange[i].transform.position;
                BorderNumber++;
            }
        }

        if (BorderNumber >= 2)
        {
            BorderVec = BorderVec / BorderNumber;
            BorderVec = this.transform.position - BorderVec;
           
            float NewFactor = BorderFactor * Mathf.Max(BorderDistance - BorderVec.magnitude, 0);
            BorderVec = BorderVec.normalized * GetDesiredRunSpeed() * NewFactor;

            BorderVec = Steer(BorderVec);
            Acceleration += BorderVec * BorderFactor;
            GoalFactor += BorderFactor;

            NoSeperationThisUpdate = true;
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
        else if (other.tag == "Character")
        {
            /*Character otherChar = other.GetComponent<Character>();
            if (otherChar.GetAlignment() == Character.TeamAlignment.PLAYERS)
            {
                PlayersInRange.Add(otherChar);
            }*/
            PlayersInRange.Add(other.gameObject);
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
        else if (other.tag == "Character")
        {
            /*Character otherChar = other.GetComponent<Character>();
            if (otherChar.GetAlignment() == Character.TeamAlignment.PLAYERS)
            {
                PlayersInRange.Remove(otherChar);
            }*/
            PlayersInRange.Remove(other.gameObject);
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

    public Vector3 GetCurrenVelocity()
    {
        return Velocity;
    }

    public SwarmType GetSwarmType()
    {
        return SType;
    }
}
