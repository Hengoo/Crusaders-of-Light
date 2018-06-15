using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Jobs;
using Unity.Collections;

public class EnemySwarm : MonoBehaviour {



    /*  public struct NeighbourJob : IJob
      {
          public Vector3 SwarmlingPos;
          public float NeighbourRadius;
         // public Collider[] NeighbourColliders;
          public int NeighbourLayerMask;

          public NativeArray<int> NeighbourCount;
          public NativeArray<int> NeighbourIDs;

          public void Execute()
          {
              // NeighbourCount[0] = Physics.OverlapSphereNonAlloc(SwarmlingPos, NeighbourRadius, NeighbourColliders, NeighbourLayerMask);
              Physics.OverlapSphere(SwarmlingPos, NeighbourRadius, NeighbourLayerMask);

              /*  for (int i = 0; i < NeighbourColliders.Length; i++)
                {
                    NeighbourIDs[i] = NeighbourColliders[i].GetComponent<EnemySwarm>().SwarmlingID;
                }*/
    //}
    //} 

    public struct MoveJob : IJob
    {
        public NativeArray<float> CohesionVec;
        public NativeArray<float> SeperationVec;
        public NativeArray<float> AlignmentVec;

        public int CohesionNumber;
        public int SeperationNumber;
        public int AlignmentNumber;

        public NativeArray<float> DistanceVec;
        public float DistanceVecMag;

        public float SeperationDistance;
        public float SeperationFactor;
        

        public float AlignmentDistance;
        public float AlignmentFactor;

        public float CohesionDistance;
        public float CohesionFactor;

        public float DangerDistance;
        public float DangerFactor;

        public float AttractionDistance;
        public float AttractionFactor;

        public bool BorderOn;
        //public float OutsideAcceleration = 1;
        public float BorderDistance;
        public float BorderFactor;
        public float DesiredBaseSpeed;
        public float DesiredRunSpeed;

        // New Assign per Schedule:
        public int NeighbourCount;
        public NativeArray<float> SwarmlingPos;
        public NativeArray<float> OtherSwarmlingPos;
        public NativeArray<float> OtherSwarmlingVelocity;
        public bool NoSeperationThisUpdate;
        //public float[] Velocity;

        public void Execute()
        {
            
            // NeighbourColliders = Physics.OverlapSphere(SwarmlingTransform.position, NeighbourRadius, NeighbourLayerMask);
            // Stop if not enough Neighbours:
            if (NeighbourCount < 2) return;

            for (int v = 0; v < CohesionVec.Length; v++)
            {
                CohesionVec[v] = 0;
                SeperationVec[v] = 0;
                AlignmentVec[v] = 0;
            }

            CohesionNumber = 0;
            SeperationNumber = 0;
            AlignmentNumber = 0;

            for (int v = 0; v < DistanceVec.Length; v++)
            {
                DistanceVec[v] = 0;
            }

            DistanceVecMag = 0;

            for (int i = 0; i < NeighbourCount; i++)
            {
               /* CurrentPosition[0] = OtherSwarmlingPos[i * 3];
                CurrentPosition[1] = OtherSwarmlingPos[i * 3 + 1];
                CurrentPosition[2] = OtherSwarmlingPos[i * 3 + 2];
                CurrentVelocity[0] = OtherSwarmlingVelocity[i * 3];
                CurrentVelocity[1] = OtherSwarmlingVelocity[i * 3 + 1];
                CurrentVelocity[2] = OtherSwarmlingVelocity[i * 3 + 2];*/

                //DistanceVec = VectorSub(SwarmlingPos, CurrentPosition);
                for (int v = 0; v < 3; v++)
                {
                    DistanceVec[v] = SwarmlingPos[v] - OtherSwarmlingPos[i * 3 + v];
                }


                //DistanceVecMag = DistanceVec.sqrMagnitude;
                DistanceVecMag = DistanceVec[0] * DistanceVec[0] + DistanceVec[1] * DistanceVec[1] + DistanceVec[2] * DistanceVec[2];

                if (DistanceVecMag <= 0) continue;

                // Cohesion:
                if (DistanceVecMag <= Mathf.Pow(CohesionDistance, 2)) // Could be optimized by storing the pow2 distance!
                {
                    //CohesionVec += CurrentPosition;
                    for (int v = 0; v < 3; v++)
                    {
                        CohesionVec[v] += OtherSwarmlingPos[i * 3 + v];
                    }

                    //CohesionNumber++;
                    CohesionVec[3]++;
                }

                // Seperation:
                if (DistanceVecMag <= Mathf.Pow(SeperationDistance, 2)) // Could be optimized by storing the pow2 distance!
                {
                    //SeperationVec += DistanceVec / DistanceVecMag;
                    for (int v = 0; v < 3; v++)
                    {
                        SeperationVec[v] += DistanceVec[v] / DistanceVecMag;
                    }

                    //SeperationNumber++;
                    SeperationVec[3]++;
                }

                // Alignment:
                if (DistanceVecMag <= Mathf.Pow(AlignmentDistance, 2)) // Could be optimized by storing the pow2 distance!
                {
                    //AlignmentVec += CurrentVelocity;
                    for (int v = 0; v < 3; v++)
                    {
                        AlignmentVec[v] += OtherSwarmlingVelocity[i * 3 + v];
                    }
                    //AlignmentNumber++;
                    AlignmentVec[3]++;
                }
            }
            /*
            // Cohesion:
            if (CohesionNumber > 0)
            {
                //CohesionVec = Vector3.ClampMagnitude(((CohesionVec / CohesionNumber) - SwarmlingTransform.position), DesiredBaseSpeed);
                //Debug.Log("Cohesion: " + CohesionVec);
                CohesionVec = CohesionVec / CohesionNumber;
                CohesionVec = CohesionVec - SwarmlingPos;
                CohesionVec = CohesionVec.normalized * DesiredBaseSpeed;

                CohesionVec = Steer(CohesionVec);

                Acceleration[0] += CohesionVec * CohesionFactor;
                GoalFactor[0] += CohesionFactor;
            }

            // Seperation:
            if (SeperationNumber > 0)
            {
                if (NoSeperationThisUpdate)
                {
                    NoSeperationThisUpdate = false;
                }
                else
                {
                    //SeperationVec = Vector3.ClampMagnitude((SeperationVec / SeperationNumber), DesiredBaseSpeed);
                    //Debug.Log("SeperationVec: " + SeperationVec);
                    SeperationVec = SeperationVec.normalized * DesiredBaseSpeed;

                    SeperationVec = Steer(SeperationVec);
                    Acceleration[0] += SeperationVec * SeperationFactor;

                    GoalFactor[0] += SeperationFactor;
                }
            }

            // Alignment:
            if (AlignmentNumber > 0)
            {
                //AlignmentVec = Vector3.ClampMagnitude((AlignmentVec/AlignmentNumber), DesiredBaseSpeed);
                //Debug.Log("AlignmentVec: " + AlignmentVec);

                AlignmentVec = AlignmentVec.normalized * DesiredBaseSpeed;

                //AlignmentVec = AlignmentVec / AlignmentNumber;

                AlignmentVec = Steer(AlignmentVec);

                Acceleration[0] += AlignmentVec * AlignmentFactor;
                GoalFactor[0] += AlignmentFactor;
            }*/
        }
    }

 /*   public struct MovJob : IJobParallelFor
    {
        public void Execute(int index)
        {
            CurrentSwarmling = NeighbourColliders[index].GetComponent<EnemySwarm>();
            //CurrentSwarmling = NeighbourSwarmlings[i];
            DistanceVec = SwarmlingTransform.position - CurrentSwarmling.SwarmlingTransform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= 0) continue;

            // Cohesion:
            if (DistanceVecMag <= Mathf.Pow(CohesionDistance, 2)) // Could be optimized by storing the pow2 distance!
            {
                CohesionVec += CurrentSwarmling.SwarmlingTransform.position;
                CohesionNumber++;
            }

            // Seperation:
            if (DistanceVecMag <= Mathf.Pow(SeperationDistance, 2)) // Could be optimized by storing the pow2 distance!
            {
                SeperationVec += DistanceVec / DistanceVecMag;

                SeperationNumber++;
            }

            // Alignment:
            if (DistanceVecMag <= Mathf.Pow(AlignmentDistance, 2)) // Could be optimized by storing the pow2 distance!
            {
                AlignmentVec += CurrentSwarmling.Velocity;
                AlignmentNumber++;
            }


        }
    }*/

    public enum SwarmType
    {
        STANDARD = 1,
        SPIKE = 2,
        RANGED = 3
    }

    [Header("Enemy Swarm:")]
    public NavMeshAgent NMAgent;
    public SwarmType SType = SwarmType.STANDARD;

    [Header("Core Rules:")]
    // Note: Seperation, Alignment, Cohesion, and Danger Factors are currently pulled from EnemyTestSwarm for at runtime testing.
    // Thus, the Factors here for these 4 rules are currently unused, but should be used later!
    [Header("Seperation:")]
    public float SeperationDistance = 3;
    public float SeperationFactor = 1;
    private bool NoSeperationThisUpdate = false;
    
    [Header("Alignment:")]
    public float AlignmentDistance = 3;
    public float AlignmentFactor = 1;
    
    [Header("Cohesion:")]
    public float CohesionDistance = 3;
    public float CohesionFactor = 1;

    [Header("Advanced Rules:")]
    [Header("Danger Avoidance:")]
    public float DangerDistance = 3;
    public float DangerFactor = 1;

    [Header("Attraction:")]
    public float AttractionDistance = 4;
    public float AttractionFactor = 1;

    [Header("Go To Border:")]
    public bool BorderOn = false;
    //public float OutsideAcceleration = 1;
    public float BorderDistance = 3;
    public float BorderFactor = 1;

    [Header("Movement:")]
    public Vector3 Velocity = Vector3.zero;
    public Vector3 Acceleration = Vector3.forward;

    public float Friction = 0.1f;

    public float GoalFactor;

    [Header("Speed::")]
    public float DesiredBaseSpeed = 6;
    public float DesiredRunSpeed = 12;

    [Header("Optimization:")]
    public float UpdateTimer = 0.5f;
    public float UpdateCounter = 0;

    [Header("Lists:")]
    public SwarmAttention SAttention;
    public CharacterAttention CAttention;
    public List<EnemySwarm> EnemiesInRange = new List<EnemySwarm>();
    public List<GameObject> DangerInRange = new List<GameObject>();
    public List<Character> PlayersInRange = new List<Character>();

    [Header("FOR TESTING:")]
    public bool PlayerDanger = true;

    [Header("New Variables:")]
    public Transform SwarmlingTransform;
    public int NeighbourRadius = 7;
    public Collider[] NeighbourColliders = new Collider[6];
    public int NeighbourCount = 0;
    public int NeighbourLayerMask = 0;

    public EnemySwarm CurrentSwarmling;

    public Vector3 CohesionVec = Vector3.zero;
    public Vector3 SeperationVec = Vector3.zero;
    public Vector3 AlignmentVec = Vector3.zero;

    public int CohesionNumber = 0;
    public int SeperationNumber = 0;
    public int AlignmentNumber = 0;

    public Vector3 DistanceVec = Vector3.zero;
    public float DistanceVecMag = 0;

    public float NewNeighbourTimer = 1f;
    public float NewNeighbourCounter = 0f;

    public int SwarmlingID = -1;
    public int NeighbourMax = 30;

    public JobHandle MJobHandle;
    public MoveJob MJobData;


    public NativeArray<float> CohesionVecNA;
    public NativeArray<float> SeperationVecNA;
    public NativeArray<float> AlignmentVecNA;

    public bool MJobRunning = false;

    public NativeArray<float> OtherSwarmlingPos;
    public NativeArray<float> OtherSwarmlingVelocity;
    public NativeArray<float> DistanceVecNA;
    public NativeArray<float> SwarmlingPosNA;

    // ================================================================================================================

    public void UpdateSwarmling()
    {
       // NeighbourColliders = Physics.OverlapSphere(SwarmlingTransform.position, NeighbourRadius, NeighbourLayerMask);
        // Stop if not enough Neighbours:
        if (NeighbourCount < 2) return;

        CohesionVec = Vector3.zero;
        SeperationVec = Vector3.zero;
        AlignmentVec = Vector3.zero;

        CohesionNumber = 0;
        SeperationNumber = 0;
        AlignmentNumber = 0;

        DistanceVec = Vector3.zero;
        DistanceVecMag = 0;

        for (int i = 0; i < NeighbourCount; i++)
        {
            CurrentSwarmling = NeighbourColliders[i].GetComponent<EnemySwarm>();
            //CurrentSwarmling = NeighbourSwarmlings[i];
            DistanceVec = SwarmlingTransform.position - CurrentSwarmling.SwarmlingTransform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= 0) continue;

            // Cohesion:
            if (DistanceVecMag <= Mathf.Pow(CohesionDistance, 2)) // Could be optimized by storing the pow2 distance!
            {
                CohesionVec += CurrentSwarmling.SwarmlingTransform.position;
                CohesionNumber++;
            }

            // Seperation:
            if (DistanceVecMag <= Mathf.Pow(SeperationDistance, 2)) // Could be optimized by storing the pow2 distance!
            {
                SeperationVec += DistanceVec / DistanceVecMag;
                
                SeperationNumber++;
            }

            // Alignment:
            if (DistanceVecMag <= Mathf.Pow(AlignmentDistance, 2)) // Could be optimized by storing the pow2 distance!
            {
                AlignmentVec += CurrentSwarmling.Velocity;
                AlignmentNumber++;
            }
        }
        // Cohesion:
        if (CohesionNumber > 0)
        {
            //CohesionVec = Vector3.ClampMagnitude(((CohesionVec / CohesionNumber) - SwarmlingTransform.position), DesiredBaseSpeed);
            //Debug.Log("Cohesion: " + CohesionVec);
            CohesionVec = CohesionVec / CohesionNumber;
            CohesionVec = CohesionVec - SwarmlingTransform.position;
            CohesionVec = CohesionVec.normalized * DesiredBaseSpeed;

            CohesionVec = Steer(CohesionVec);
            
            Acceleration += CohesionVec * CohesionFactor;
            GoalFactor += CohesionFactor;
        }

        // Seperation:
        if (SeperationNumber > 0)
        {
            if (NoSeperationThisUpdate)
            {
                NoSeperationThisUpdate = false;
            }
            else
            {
                //SeperationVec = Vector3.ClampMagnitude((SeperationVec / SeperationNumber), DesiredBaseSpeed);
                //Debug.Log("SeperationVec: " + SeperationVec);
                SeperationVec = SeperationVec.normalized * DesiredBaseSpeed;

                SeperationVec = Steer(SeperationVec);
                Acceleration += SeperationVec * SeperationFactor;
                
                GoalFactor += SeperationFactor;
            }    
        }

        // Alignment:
        if (AlignmentNumber > 0)
        {
            //AlignmentVec = Vector3.ClampMagnitude((AlignmentVec/AlignmentNumber), DesiredBaseSpeed);
            //Debug.Log("AlignmentVec: " + AlignmentVec);

            AlignmentVec = AlignmentVec.normalized * DesiredBaseSpeed;

            //AlignmentVec = AlignmentVec / AlignmentNumber;

            AlignmentVec = Steer(AlignmentVec);
            
            Acceleration += AlignmentVec * AlignmentFactor;
            GoalFactor += AlignmentFactor;
        }

    }

    // ================================================================================================================

    private void Start()
    {
        UpdateCounter = Random.Range(0, UpdateTimer);
        NewNeighbourCounter = Random.Range(0, NewNeighbourTimer);
        NeighbourLayerMask = 1 << NeighbourLayerMask;

        /*    var jobData = new TestJob();

            jobData.NumA = new NativeArray<float>(1, Allocator.Temp);
            jobData.NumA[0] = 10;

            jobData.NumB = new NativeArray<float>(1, Allocator.Temp);
            jobData.NumB[0] = 12;

            NativeArray<float> result = new NativeArray<float>(10, Allocator.Temp);
            jobData.result = result;

            JobHandle handle = jobData.Schedule(result.Length, 1);

            handle.Complete();

            string debugMess = "Result: ";

            for (int i = 0; i < result.Length; i++)
            {
                debugMess += "/ " + i + ": " + result[i] + " ";
            }

            Debug.Log(debugMess);

            result.Dispose();
            jobData.NumA.Dispose();
            jobData.NumB.Dispose();*/
    }


    public void InitializeSwarmling(int NewSwarmlingID)
    {
        SwarmlingID = NewSwarmlingID;

        OtherSwarmlingPos = new NativeArray<float>(NeighbourColliders.Length * 3, Allocator.Persistent);
        OtherSwarmlingVelocity = new NativeArray<float>(NeighbourColliders.Length * 3, Allocator.Persistent);

        MJobData = new MoveJob();

       // MJobHandle = MJobData.Schedule();

        MJobData.CohesionDistance = CohesionDistance;
        MJobData.CohesionFactor = CohesionFactor;
        MJobData.CohesionNumber = 0;
        CohesionVecNA = new NativeArray<float>(4, Allocator.Persistent);
        MJobData.CohesionVec = CohesionVecNA;

        MJobData.SeperationDistance = SeperationDistance;
        MJobData.SeperationFactor = SeperationFactor;
        MJobData.SeperationNumber = 0;
        SeperationVecNA = new NativeArray<float>(4, Allocator.Persistent);
        MJobData.SeperationVec = SeperationVecNA;

        MJobData.AlignmentDistance = AlignmentDistance;
        MJobData.AlignmentFactor = AlignmentFactor;
        MJobData.AlignmentNumber = 0;
        AlignmentVecNA = new NativeArray<float>(4, Allocator.Persistent);
        MJobData.AlignmentVec = AlignmentVecNA;

        DistanceVecNA = new NativeArray<float>(3, Allocator.Persistent);
        MJobData.DistanceVec = DistanceVecNA;
        MJobData.DistanceVecMag = 0;

        MJobData.DesiredBaseSpeed = DesiredBaseSpeed;
        MJobData.DesiredRunSpeed = DesiredRunSpeed;

        SwarmlingPosNA = new NativeArray<float>(3, Allocator.Persistent);
        MJobData.SwarmlingPos = SwarmlingPosNA;
/*
   

    public float DangerDistance;
    public float DangerFactor;

    public float AttractionDistance;
    public float AttractionFactor;

    public bool BorderOn;
    //public float OutsideAcceleration = 1;
    public float BorderDistance;
    public float BorderFactor;

    // New Assign per Schedule:
    public int NeighbourCount;
    public Vector3 SwarmlingPos;
    public Vector3[] OtherSwarmlingPos;
    public Vector3[] OtherSwarmlingVelocity;
    public bool NoSeperationThisUpdate;
    public Vector3 Velocity;
    */



}

    public void SwarmlingFixedUpdate()
    {
        GoalFactor = 0;

        UpdateCounter += Time.deltaTime;

        // Get List of Neighbours:
        if (NewNeighbourCounter <= 0)
        {
            /*
             if (!NJobCurrentlyRunning)
             {
                 NJobCurrentlyRunning = true;

                // NJobData.NeighbourColliders = new Collider[NeighbourMax];

                 NJobData.NeighbourLayerMask = NeighbourLayerMask;
                 NJobData.NeighbourRadius = NeighbourRadius;

                 NJobData.NeighbourCount = NeighbourCountNA;
                 NJobData.NeighbourIDs = NeighbourIDsNA;
                 NJobHandle = NJobData.Schedule();
             }
             else if (NJobHandle.IsCompleted)
             {
                NewNeighbourCounter += Random.Range(0, NewNeighbourTimer) + Mathf.Pow((NeighbourCount * 0.15f), 2);

                for (int i = 0; i < NeighbourIDsNA.Length; i++)
                {
                    NeighbourSwarmlings[i] = EnemyTestSwarm.Instance.GetSwarmling(NeighbourIDsNA[i]);
                }

                NJobCurrentlyRunning = false;
            }*/
             /*
            NeighbourCountNA = new NativeArray<int>(1, Allocator.Temp);
            NeighbourIDsNA = new NativeArray<int> (30, Allocator.Temp);


            NJobData.NeighbourCount = NeighbourCountNA;
            NJobData.NeighbourIDs = NeighbourIDsNA;

            NJobData.NeighbourColliders = new Collider[NeighbourMax];

            NJobData.NeighbourLayerMask = NeighbourLayerMask;
            NJobData.NeighbourRadius = NeighbourRadius;

            //NJobData.NeighbourCount = NeighbourCountNA;
           // NJobData.NeighbourIDs = NeighbourIDsNA;
            NJobHandle = NJobData.Schedule();

            NJobHandle.Complete();

            NewNeighbourCounter += Random.Range(0, NewNeighbourTimer) + Mathf.Pow((NeighbourCount * 0.15f), 2);

            for (int i = 0; i < NeighbourIDsNA.Length; i++)
            {
                NeighbourSwarmlings[i] = EnemyTestSwarm.Instance.GetSwarmling(NeighbourIDsNA[i]);
            }

            NeighbourIDsNA.Dispose();
            NeighbourCountNA.Dispose();
            */


            NeighbourCount = Physics.OverlapSphereNonAlloc(SwarmlingTransform.position, NeighbourRadius, NeighbourColliders, NeighbourLayerMask);
            //NewNeighbourCounter += NewNeighbourTimer;
            NewNeighbourCounter += Random.Range(0, NewNeighbourTimer) + Mathf.Pow((NeighbourCount * 0.15f), 2);
        }
        else
        {
            NewNeighbourCounter -= Time.deltaTime;
        }

        if (UpdateCounter >= UpdateTimer)
        {
            if (!MJobRunning)
            {
                MJobData.NeighbourCount = NeighbourCount;
                MJobData.SwarmlingPos[0] = SwarmlingTransform.position.x;
                MJobData.SwarmlingPos[1] = SwarmlingTransform.position.y;
                MJobData.SwarmlingPos[2] = SwarmlingTransform.position.z;
               // MJobData.Velocity = Velocity;
                MJobData.NoSeperationThisUpdate = NoSeperationThisUpdate;

                for (int i = 0; i < NeighbourCount; i++)
                {
                    CurrentSwarmling = NeighbourColliders[i].GetComponent<EnemySwarm>();
                    OtherSwarmlingPos[i * 3] = CurrentSwarmling.transform.position.x;
                    OtherSwarmlingPos[i * 3 + 1] = CurrentSwarmling.transform.position.y;
                    OtherSwarmlingPos[i * 3 + 2] = CurrentSwarmling.transform.position.z;
                    OtherSwarmlingVelocity[i * 3] = CurrentSwarmling.Velocity.x;
                    OtherSwarmlingVelocity[i * 3 + 1] = CurrentSwarmling.Velocity.y;
                    OtherSwarmlingVelocity[i * 3 + 2] = CurrentSwarmling.Velocity.z;
                }

                MJobData.OtherSwarmlingPos = OtherSwarmlingPos;
                MJobData.OtherSwarmlingVelocity = OtherSwarmlingVelocity;



                MJobHandle = MJobData.Schedule();
                MJobRunning = true;
            }
            else if (MJobHandle.IsCompleted)
            {
                MJobHandle.Complete();

                CohesionNumber = Mathf.FloorToInt(CohesionVecNA[3]);
                SeperationNumber = Mathf.FloorToInt(SeperationVecNA[3]);
                AlignmentNumber = Mathf.FloorToInt(AlignmentVecNA[3]);

                CohesionVec = new Vector3(CohesionVecNA[0], CohesionVecNA[1], CohesionVecNA[2]);
                SeperationVec = new Vector3(SeperationVecNA[0], SeperationVecNA[1], SeperationVecNA[2]);
                AlignmentVec = new Vector3(AlignmentVecNA[0], AlignmentVecNA[1], AlignmentVecNA[2]);

                // Cohesion:
                if (CohesionNumber > 0)
                {
                    //CohesionVec = Vector3.ClampMagnitude(((CohesionVec / CohesionNumber) - SwarmlingTransform.position), DesiredBaseSpeed);
                    //Debug.Log("Cohesion: " + CohesionVec);
                    CohesionVec = CohesionVec / CohesionNumber;
                    CohesionVec = CohesionVec - SwarmlingTransform.position;
                    CohesionVec = CohesionVec.normalized * DesiredBaseSpeed;

                    CohesionVec = Steer(CohesionVec);

                    Acceleration += CohesionVec * CohesionFactor;
                    GoalFactor += CohesionFactor;
                }

                // Seperation:
                if (SeperationNumber > 0)
                {
                    if (NoSeperationThisUpdate)
                    {
                        NoSeperationThisUpdate = false;
                    }
                    else
                    {
                        //SeperationVec = Vector3.ClampMagnitude((SeperationVec / SeperationNumber), DesiredBaseSpeed);
                        //Debug.Log("SeperationVec: " + SeperationVec);
                        SeperationVec = SeperationVec.normalized * DesiredBaseSpeed;

                        SeperationVec = Steer(SeperationVec);
                        Acceleration += SeperationVec * SeperationFactor;

                        GoalFactor += SeperationFactor;
                    }
                }

                // Alignment:
                if (AlignmentNumber > 0)
                {
                    //AlignmentVec = Vector3.ClampMagnitude((AlignmentVec/AlignmentNumber), DesiredBaseSpeed);
                    //Debug.Log("AlignmentVec: " + AlignmentVec);

                    AlignmentVec = AlignmentVec.normalized * DesiredBaseSpeed;

                    //AlignmentVec = AlignmentVec / AlignmentNumber;

                    AlignmentVec = Steer(AlignmentVec);

                    Acceleration += AlignmentVec * AlignmentFactor;
                    GoalFactor += AlignmentFactor;
                }









                UpdateCounter = 0;

                //Acceleration = AccelerationNA[0];
                //GoalFactor = GoalFactorNA[0];

                if (GoalFactor > 0)
                {
                    Acceleration = Acceleration / GoalFactor;
                }

                MJobRunning = false;
            }









            

            // Reset Acceleration:
            //Acceleration = Vector3.zero;

            //UpdateSwarmling();

            // ====

            

            // ====


            /*if (BorderOn)
            {
                RuleGoToBorder();
            }*/

            //RuleAttraction();
           // RuleCohesion();
           // RuleSeperation();
            //RuleAlignment();

            /*if (PlayerDanger)
            {
                RuleDangerAvoidanceEnhanced();
            }
            else
            {
                RuleDangerAvoidance();
            }*/
            
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
    }

    public Vector3 Steer(Vector3 VelDesired)
    {
        return VelDesired - Velocity;
    }

   

    // ===================================================== RULES =====================================================

    // ================================================ RULE: COHESION ================================================
    // Enemies should steer to the center of all nearby enemies.

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
            DistanceVecMag = DistanceVec.sqrMagnitude;

            // Cohesion:
            if (DistanceVecMag <= CohesionDistance * CohesionDistance)
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

    // ===============================================/ RULE: COHESION /===============================================

    // =============================================== RULE: SEPERATION ===============================================
    // Enemies should steer away from other very close enemies.

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

    // ==============================================/ RULE: SEPERATION /==============================================

    // ================================================ RULE: ALIGNMENT ================================================
    // Enemies should steer towards the average direction of nearby enemies.

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
            DistanceVecMag = DistanceVec.sqrMagnitude;

            // Alignment:
            if (DistanceVecMag <= AlignmentDistance * AlignmentDistance)
            {
                AlignmentVec += EnemiesInRange[i].GetCurrenVelocity();
                AlignmentNumber++;
            }
        }

        // Alignment:
        if (AlignmentNumber >= 1)
        {
            AlignmentVec = AlignmentVec.normalized * GetDesiredSpeed();
            //AlignmentVec = AlignmentVec / AlignmentNumber;

            AlignmentVec = Steer(AlignmentVec);
            Acceleration += AlignmentVec * EnemyTestSwarm.Instance.AlignmentFactor;
            GoalFactor += EnemyTestSwarm.Instance.AlignmentFactor;
        }

    }

    // ===============================================/ RULE: ALIGNMENT /===============================================

    // ============================================ RULE: DANGER AVOIDANCE =============================================
    // Enemies should steer away from nearby dangers.

    private void RuleDangerAvoidance()
    {
        // Dangers in Range:
        int NumberOfDangers = DangerInRange.Count;

        Vector3 DangerVec = Vector3.zero;
        int DangerVecNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;
        
        for (int i = 0; i < NumberOfDangers; i++)
        {
            DistanceVec = transform.position - DangerInRange[i].transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= DangerDistance * DangerDistance)
            {
                DangerVec += DistanceVec.normalized / Mathf.Sqrt(DistanceVecMag);
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

    private void RuleDangerAvoidanceEnhanced()
    {
        // Dangers in Range:
        int NumberOfDangers = DangerInRange.Count;

        Vector3 DangerVec = Vector3.zero;
        int DangerVecNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfDangers; i++)
        {
            DistanceVec = transform.position - DangerInRange[i].transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= DangerDistance * DangerDistance)
            {
                DangerVec += DistanceVec.normalized / Mathf.Sqrt(DistanceVecMag);
                DangerVecNumber++;
            }
        }

        // Player Hit Objects In Range:
        List<SkillHitObject> PlayerHitObjects = CAttention.GetPlayerHitObjectsInAttentionRange();
        NumberOfDangers = PlayerHitObjects.Count;

        for (int i = 0; i < NumberOfDangers; i++)
        {
            if (PlayerHitObjects[i])
            {
                DistanceVec = transform.position - PlayerHitObjects[i].transform.position;
                DistanceVecMag = DistanceVec.sqrMagnitude;

                if (DistanceVecMag <= DangerDistance * DangerDistance)
                {
                    DangerVec += DistanceVec.normalized / Mathf.Sqrt(DistanceVecMag);
                    DangerVecNumber++;
                }
            }
        }

        // Players in Range:
        NumberOfDangers = PlayersInRange.Count;

        for (int i = 0; i < NumberOfDangers; i++)
        {
            DistanceVec = transform.position - PlayersInRange[i].transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= (DangerDistance * 0.6f) * (DangerDistance * 0.6f)
                && PlayersInRange[i].GetCurrentThreatLevel(true, false) >= 2
                && (Vector3.Dot(PlayersInRange[i].transform.forward, (PlayersInRange[i].transform.position - transform.position).normalized) < 0.3f))
            {
                DangerVec += DistanceVec.normalized / Mathf.Sqrt(DistanceVecMag);
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

    // ===========================================/ RULE: DANGER AVOIDANCE /============================================

    // =============================================== RULE: ATTRACTION ================================================
    // Enemies steer towards the nearest attraction (in this case Player Characters, so far).

    private void RuleAttraction()
    {
        int NumberOfOthers = PlayersInRange.Count;

        Vector3 AttractionVec = Vector3.zero;
        float AttractionVecMag = AttractionDistance * AttractionDistance + 1;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;
        
        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = PlayersInRange[i].transform.position - transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= AttractionDistance * AttractionDistance
                && DistanceVecMag < AttractionVecMag)
            {
                AttractionVec = DistanceVec;
                AttractionVecMag = DistanceVecMag;
            }
        }

        if (AttractionVecMag < AttractionDistance * AttractionDistance + 1)
        {
            AttractionVec = AttractionVec.normalized * GetDesiredSpeed();

            AttractionVec = Steer(AttractionVec);
            Acceleration += AttractionVec * AttractionFactor;
            GoalFactor += AttractionFactor;
        }
    }

    // ==============================================/ RULE: ATTRACTION /===============================================

    // ============================================== RULE: GO TO BORDER ===============================================
    // Tank Enemies should steer towards the outside of the swarm.

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
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= BorderDistance * BorderDistance && EnemiesInRange[i].GetSwarmType() != SType)
            {
                BorderVec += EnemiesInRange[i].transform.position;
                BorderNumber++;
            }
        }

        if (BorderNumber >= 2)
        {
            BorderVec = BorderVec / BorderNumber;
            BorderVec = this.transform.position - BorderVec;
           
            float NewFactor = Mathf.Max(BorderDistance - BorderVec.magnitude, 0);
            BorderVec = BorderVec.normalized * GetDesiredRunSpeed() * NewFactor;

            BorderVec = Steer(BorderVec);
            Acceleration += BorderVec * BorderFactor;
            GoalFactor += BorderFactor;

            NoSeperationThisUpdate = true;
        }
    }

    // =============================================/ RULE: GO TO BORDER /==============================================

    // ====================================================/ RULES /====================================================


    // ================================================== NEARBY LISTS ==================================================

    public void AddToEnemiesInRange(EnemySwarm AddEnemySwarm)
    {
        EnemiesInRange.Add(AddEnemySwarm);
    }

    public void RemoveFromEnemiesInRange(EnemySwarm RemoveEnemySwarm)
    {
        EnemiesInRange.Remove(RemoveEnemySwarm);
    }

    public void AddToDangersInRange(GameObject AddDanger)
    {
        DangerInRange.Add(AddDanger);
    }

    public void RemoveFromDangersInRange(GameObject RemoveDanger)
    {
        DangerInRange.Remove(RemoveDanger);
    }

    public void AddToPlayersInRange(GameObject AddPlayer)
    {
        PlayersInRange.Add(AddPlayer.GetComponent<CharacterAttention>().GetOwner());
    }

    public void RemoveFromPlayersInRanger(GameObject RemovePlayer)
    {
        PlayersInRange.Remove(RemovePlayer.GetComponent<CharacterAttention>().GetOwner());
    }

    public void OnDestroy()
    {
        for (int i = 0; i < EnemiesInRange.Count; i++)
        {
            EnemiesInRange[i].RemoveFromEnemiesInRange(this);
        }

    }

    private void OnApplicationQuit()
    {
        MJobHandle.Complete();

        AlignmentVecNA.Dispose();
        SeperationVecNA.Dispose();
        CohesionVecNA.Dispose();

        SwarmlingPosNA.Dispose();
        DistanceVecNA.Dispose();
        OtherSwarmlingPos.Dispose();
        OtherSwarmlingVelocity.Dispose();

    }

    // =================================================/ NEARBY LISTS /=================================================


    // ================================================ GETTERS/SETTERS =================================================

    public float GetDesiredSpeed()
    {
        return DesiredBaseSpeed;
    }

    public float GetDesiredRunSpeed()
    {
        return DesiredRunSpeed;
    }

    public Vector3 GetCurrenVelocity()
    {
        return Velocity;
    }

    public SwarmType GetSwarmType()
    {
        return SType;
    }

    // ===============================================/ GETTERS/SETTERS /================================================
}
