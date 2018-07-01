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

    [Header("Enemy Swarm:")]
    public NavMeshAgent NMAgent;
    public SwarmType SType = SwarmType.STANDARD;
    public CharacterSwarm ThisSwarmlingCharacter;

    [Header("Core Rules:")]
    // Note: Seperation, Alignment, Cohesion, and Danger Factors are currently pulled from EnemyTestSwarm for at runtime testing.
    // Thus, the Factors here for these 4 rules are currently unused, but should be used later!
    [Header("Seperation:")]
    public float SeperationDistance = 2;
    public float SeperationFactor = 4;
    protected bool NoSeperationThisUpdate = false;

    [Header("Alignment:")]
    public float AlignmentDistance = 4;
    public float AlignmentFactor = 0.75f;

    [Header("Cohesion:")]
    public float CohesionDistance = 3;
    public float CohesionFactor = 0.35f;

    [Header("Advanced Rules:")]
    [Header("Danger Avoidance:")]
    public float DangerDistance = 6;
    public float DangerFactor = 10;

    [Header("Attraction:")]
    public float AttractionDistanceMin = 2;
    public float AttractionDistanceMax = 20;
    public float AttractionDistance = 10;
    public float AttractionFactor = 0.6f;
    public float AttractionTooCloseDistance = 0f;

    public Vector3 TempPlayerPosition = Vector3.zero;

    [Header("Movement:")]
    public Vector3 Velocity = Vector3.zero;
    public Vector3 Acceleration = Vector3.forward;

    public float Friction = 0.1f;

    public float GoalFactor;

    [Header("Speed::")]
    public float DesiredBaseSpeed = 6;
    public float DesiredRunSpeed = 8;

    [Header("Optimization:")]
    public float UpdateTimer = 0.06f;
    public float UpdateCounter = 0;

    [Header("Lists:")]
    public SwarmAttention SAttention;
    public CharacterAttention CAttention;
    public List<EnemySwarm> EnemiesInRange = new List<EnemySwarm>();
    public List<GameObject> DangerInRange = new List<GameObject>();
    public CharacterPlayer[] Players;

    [Header("FOR TESTING:")]
    public bool PlayerDanger = true;

    [Header("New Variables:")]
    public Transform SwarmlingTransform;
    public int NeighbourRadiusBase = 7;
    public float NeighbourRadiusCurrent = 2f;
    public float NeighbourRadiusMin = 2f;
    public float NeighbourRadiusMax = 7f;
    public float NeighbourRadiusStep = 1;
    public Collider[] NeighbourColliders = new Collider[6];
    public int NeighbourCount = 0;
    public int NeighbourLayerMask = 17;

    public EnemySwarm CurrentSwarmling;



    public Vector3 CohesionVec = Vector3.zero;
    public Vector3 SeperationVec = Vector3.zero;
    public Vector3 AlignmentVec = Vector3.zero;

    public int CohesionNumber = 0;
    public int SeperationNumber = 0;
    public int AlignmentNumber = 0;

    public Vector3 DistanceVec = Vector3.zero;
    public float DistanceVecMag = 0;

    public Vector3 DistanceVec2 = Vector3.zero;
    public float DistanceVecMag2 = 0;
    public float DistanceAngle = 0;

    public float NewNeighbourTimer = 1f;
    public float NewNeighbourCounter = 0f;


    public Vector3 DangerAvoidanceVec = Vector3.zero;
    public Vector3 PlayerAttractionVec = Vector3.zero;

    public int AttractionNumber = 0;
    public int DangerNumber = 0;
    [Header("Player Danger:")]
    public float PlayerDangerDistance = 4;  // Must be smaller than Player Attraction Range for optimization reasons!
    public int PlayerDangerThreatLevelCheck = 2;
    public float PlayerDangerAngle = 0.3f;


    public Vector3 PlayerDangerVec = Vector3.zero;
    public int PlayerDangerNumber = 0;
    public float PlayerDangerFactor = 6;

    [Header("Spawn Stuff:")]
    public int SwarmlingID = -1;
    public SwarmSpawner SpawnedBy;

    public bool DoNotMove = false;
    public CharacterPlayer ClosestPlayer;
    public float ClosestPlayerSqrDistance = 9;
    public float ClosestPlayerSqrDistanceBase = 9;

    public bool IgnoreThisSwarmlingForOthers = false;

    public bool IgnoreThisSwarmlingForOthersWhenInDanger = false;

    //public float TempAngle = 0;
    //public Vector3 TempAxis = Vector3.zero;

    [Header("Light Orb Attraction:")]
    public NavMeshPath NavPathLightOrb;
    public float MinLightOrbAttractionDistance;
    public float LightOrbAttractionReachDistance;
    public float LightOrbAttractionSelfDestroyDistance;

    public bool LightOrbAttractionMode = false;
    public float LightOrbAttractionTimer = 8;
    public float LightOrbAttractionCounter = -1;

    [Header("Confidence:")]
    public float ConfidenceCurrent = 1;
    public bool UseConfidenceForPlayerDanger = true;

    [Header("Spawn Effect:")]
    public GameObject SpawnEffectPrefab;


    public bool ScaredOfPlayer = false;


    [Header("Home Area:")]
    public Vector3 SwarmlingHomeAreaCenter = Vector3.zero;
    public float SwarmlingHomeAreaRadius = 40;
    public float SwarmlingHomeAreaGoToRadius = 10;
    public bool SwarmlingIsGoingHome = false;

    // ================================================================================================================

    public void SwarmlingLightOrbAttractionCalculation()
    {
        if (LightOrbAttractionCounter > 0)
        {
            LightOrbAttractionCounter -= Time.deltaTime;

            if (LightOrbAttractionCounter > 0)
            {
                return;
            }
            else
            {
                LightOrbAttractionCounter += LightOrbAttractionTimer;
            }
        }

        if (!NMAgent.enabled)
        {
            return;
        }

        DistanceVec = SpawnedBy.transform.position - SwarmlingTransform.position;
        DistanceVecMag = DistanceVec.sqrMagnitude;

        if (LightOrbAttractionMode)
        {
            if (DistanceVecMag > Mathf.Pow(LightOrbAttractionSelfDestroyDistance, 2))
            {
                SwarmlingSuicide();
                return;
            }
            else if (DistanceVecMag <= Mathf.Pow(LightOrbAttractionReachDistance, 2))
            {
                NMAgent.ResetPath();
                LightOrbAttractionMode = false;
                LightOrbAttractionCounter += LightOrbAttractionTimer;
                return;
            }
            else
            {
                NMAgent.CalculatePath(SpawnedBy.transform.position, NavPathLightOrb);
                if (NavPathLightOrb.status != NavMeshPathStatus.PathComplete)
                {
                    SwarmlingSuicide();
                }
                NMAgent.SetPath(NavPathLightOrb);
            }
        }
        else
        {
            if (DistanceVecMag >= Mathf.Pow(MinLightOrbAttractionDistance, 2))
            {
                NMAgent.CalculatePath(SpawnedBy.transform.position, NavPathLightOrb);
                if (NavPathLightOrb.status != NavMeshPathStatus.PathComplete
                    || DistanceVecMag >= Mathf.Pow(LightOrbAttractionSelfDestroyDistance, 2))
                {
                    LightOrbAttractionMode = true; // To prevent swarmlings from continuing their update until they are destroyed
                    SwarmlingSuicide();
                    return;
                }
                NMAgent.SetPath(NavPathLightOrb);
                //NMAgent.SetDestination(SpawnedBy.transform.position);
                LightOrbAttractionMode = true;
                return;
            }
            else
            {
                LightOrbAttractionCounter += LightOrbAttractionTimer;
            }
        }
    }

    public void SwarmlingAttractionAndDangerRuleCalculation()
    {
        // Go through all Players for Attraction and Player Danger Avoidance:
        ScaredOfPlayer = false;

        PlayerAttractionVec = Vector3.zero;
        DangerAvoidanceVec = Vector3.zero;

        AttractionNumber = 0;
        DangerNumber = 0;

        DistanceVec = Vector3.zero;
        DistanceVecMag = 0;

        ClosestPlayer = null;
        ClosestPlayerSqrDistance = ClosestPlayerSqrDistanceBase;

        if (IgnoreThisSwarmlingForOthersWhenInDanger && IgnoreThisSwarmlingForOthers)
        {
            IgnoreThisSwarmlingForOthers = false;
        }

        // Go through all Players:
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].GetCharacterIsDead())
            {
                continue;
            }

            // Player Attraction:
            TempPlayerPosition = Players[i].transform.position + Players[i].GetTargetVelocity() * 3;// + Players[i].transform.rotation * Vector3.forward * 2;
            DistanceVec = TempPlayerPosition - SwarmlingTransform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            // Save Closest Player for Attack Rule later:
            if (ClosestPlayerSqrDistance > DistanceVecMag)
            {
                ClosestPlayerSqrDistance = DistanceVecMag;
                ClosestPlayer = Players[i];
            }
        }

        if (ClosestPlayer)
        {
            // Player Attraction:
            TempPlayerPosition = ClosestPlayer.transform.position + ClosestPlayer.GetTargetVelocity() * 3;// + Players[i].transform.rotation * Vector3.forward * 2;
            DistanceVec = TempPlayerPosition - SwarmlingTransform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= Mathf.Pow(AttractionDistance, 2))
            {
                PlayerAttractionVec += DistanceVec;
                //PlayerAttractionVec +=  * 100;
                AttractionNumber++;
/*
                // If in Player Attraction Range, check if in Player Danger Range:
                if (DistanceVecMag <= Mathf.Pow(PlayerDangerDistance, 2)
                && ClosestPlayer.GetCurrentThreatLevel(true, false) >= PlayerDangerThreatLevelCheck
                && (Vector3.Dot(ClosestPlayer.transform.forward, (TempPlayerPosition - transform.position).normalized) < PlayerDangerAngle))
                {
                    /*   TempAxis = Vector3.Cross(Velocity, DistanceVec);
                       TempAngle = Vector3.SignedAngle(Velocity, DistanceVec, TempAxis);
                       //if (TempAngle > 90)
                       //{
                       if (TempAngle <= 0)
                       {
                           DangerAvoidanceVec += Quaternion.AngleAxis(-1 * (90 + TempAngle), TempAxis) * (DistanceVec / DistanceVecMag);
                       }
                       else
                       {
                           DangerAvoidanceVec += Quaternion.AngleAxis(90 - TempAngle, TempAxis) * (DistanceVec / DistanceVecMag);
                       }
                       DangerNumber++;*//*
                    if (!ScaredOfPlayer) { ScaredOfPlayer = true; }

                    /* if (DistanceVecMag <= Mathf.Pow(PlayerDangerDistance-1, 2))
                     {
                         DistanceVec2 = ClosestPlayer.transform.position - SwarmlingTransform.position;
                         DistanceVecMag2 = DistanceVec2.sqrMagnitude;

                         if (DistanceVecMag2 <= DistanceVecMag) // Real Position closer than Pos+Vel
                         {
                             DistanceAngle = Vector3.SignedAngle(DistanceVec2, DistanceVec, Vector3.Cross(DistanceVec2, DistanceVec));

                             DangerAvoidanceVec += Quaternion.AngleAxis((DistanceAngle / Mathf.Abs(DistanceAngle)) * 90, Vector3.Cross(DistanceVec2, DistanceVec)) * (DistanceVec2 / DistanceVecMag2);
                         }
                         else
                         {
                             DangerAvoidanceVec += -1 * DistanceVec / DistanceVecMag;
                             DangerNumber++;
                         }
                     }*//*
                    DangerAvoidanceVec += -1 * DistanceVec / DistanceVecMag;
                    DangerNumber++;
                    if (IgnoreThisSwarmlingForOthersWhenInDanger && !IgnoreThisSwarmlingForOthers)
                    {
                        IgnoreThisSwarmlingForOthers = true;
                    }
                }*/
            }
        }
/*
        // Go through all Danger Objects:
        for (int i = 0; i < DangerInRange.Count; i++)
        {
            if (!DangerInRange[i])
            {
                continue;
            }

            // Danger Avoidance:
            DistanceVec = transform.position - DangerInRange[i].transform.position;
            //DistanceVec =  DangerInRange[i].transform.position - SwarmlingTransform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= Mathf.Pow(DangerDistance, 2))
            {
                
              /*  TempAxis = Vector3.Cross(Velocity, DistanceVec);
                TempAngle = Vector3.SignedAngle(Velocity, DistanceVec, TempAxis);
        //if (TempAngle > 90)
        //{
                if (TempAngle <= 0)
                {
                    DangerAvoidanceVec += Quaternion.AngleAxis(-1 * (90 + TempAngle), TempAxis) * (DistanceVec / DistanceVecMag);                 
                }
                else
                {
                    DangerAvoidanceVec += Quaternion.AngleAxis(90 - TempAngle, TempAxis) * (DistanceVec / DistanceVecMag);
                }
                DangerNumber++;
                // }
                *//*


                DangerAvoidanceVec += DistanceVec / DistanceVecMag;
                DangerNumber++;
            }
        }*/

        // Player Attraction:
        if (AttractionNumber > 0)
        {
            PlayerAttractionVec = PlayerAttractionVec.normalized * GetDesiredSpeed();

            PlayerAttractionVec = Steer(PlayerAttractionVec);
            Acceleration += PlayerAttractionVec * AttractionFactor;
            GoalFactor += AttractionFactor;
        }
        else
        {
            AttractionDistance = Mathf.Min(AttractionDistance + UpdateTimer * 8, AttractionDistanceMax);
        }
        /*
        // Total Danger:
        if (DangerNumber > 0)
        {
            DangerAvoidanceVec = DangerAvoidanceVec.normalized * GetDesiredRunSpeed();

            DangerAvoidanceVec = Steer(DangerAvoidanceVec);
            Acceleration += DangerAvoidanceVec * DangerFactor * ConfidenceCurrent;
            GoalFactor += DangerFactor * ConfidenceCurrent;
        }

           */
       
    }

    public void SwarmlingBaseRulesCalculation()
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

        ConfidenceCurrent = 0;

        for (int i = 0; i < NeighbourCount; i++)
        {
            if (!NeighbourColliders[i])
            {
                continue;
            }

            CurrentSwarmling = NeighbourColliders[i].GetComponent<EnemySwarm>();

            if (CurrentSwarmling.IgnoreThisSwarmlingForOthers)
            {
                continue;
            }

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
                ConfidenceCurrent++;
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

        ConfidenceCurrent = 1 - ConfidenceCurrent / NeighbourColliders.Length;
    }

    public void SwarmlingHomeRuleCalculation()
    {
        DistanceVec = SwarmlingHomeAreaCenter - SwarmlingTransform.position;
        DistanceVecMag = DistanceVec.sqrMagnitude;

        if (DistanceVecMag >= Mathf.Pow(SwarmlingHomeAreaRadius,2))
        {
            if (!SwarmlingIsGoingHome && NMAgent.enabled)
            {
                SwarmlingIsGoingHome = true;
                NMAgent.SetDestination(SwarmlingHomeAreaCenter);
            }
        }
        else if (SwarmlingIsGoingHome && DistanceVecMag <= Mathf.Pow(SwarmlingHomeAreaGoToRadius,2))
        {
            SwarmlingIsGoingHome = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(SwarmlingHomeAreaCenter, SwarmlingHomeAreaRadius);
    }

    public virtual void SwarmlingSpecialRuleCalculation() { }

    public virtual void SwarmlingAttackRuleCalculation() { }

    public virtual void SwarmlingFinishedAttack() { }


    // ================================================================================================================

    private void Start()
    {
        UpdateCounter = Random.Range(0, UpdateTimer);
        NewNeighbourCounter = Random.Range(0, NewNeighbourTimer);
        NMAgent.updateRotation = false;

        Instantiate(SpawnEffectPrefab, SwarmlingTransform.position, Quaternion.identity);
        //Players = EnemyTestSwarm.Instance.PlayerCharacters;
    }

    public void InitializeSwarmling(SwarmSpawner _SpawnedBy, int _SwarmlingID, CharacterPlayer[] _Players, int _NeighbourLayerMask, float HealthFactor)
    {
        SpawnedBy = _SpawnedBy;
        Players = _Players;
        SwarmlingID = _SwarmlingID;
        NeighbourLayerMask = _NeighbourLayerMask;

        SwarmlingHomeAreaCenter = SwarmlingTransform.position;

        ThisSwarmlingCharacter.SetHealthMax(Mathf.RoundToInt(ThisSwarmlingCharacter.GetHealthMax() * HealthFactor));

        NavPathLightOrb = new NavMeshPath();
    }

    public virtual void SwarmlingUpdate()
    {
        if (ThisSwarmlingCharacter.GetCharacterIsDead())
        {
            return;
        }

        GoalFactor = 0;

        // Get List of Neighbours:
        if (NewNeighbourCounter <= 0)
        {
            //NeighbourRadiusCurrent = NeighbourRadiusBase + NeighbourCount * -0.3f;

            if (NeighbourCount == NeighbourColliders.Length)
            {
                NeighbourRadiusCurrent = Mathf.Max(NeighbourRadiusCurrent - NeighbourRadiusStep, NeighbourRadiusMin);
            }
            else
            {
                NeighbourRadiusCurrent = Mathf.Min(NeighbourRadiusCurrent + NeighbourRadiusStep, NeighbourRadiusMax);
            }

            NeighbourCount = Physics.OverlapSphereNonAlloc(SwarmlingTransform.position, NeighbourRadiusCurrent, NeighbourColliders, NeighbourLayerMask);

            for (int i = NeighbourColliders.Length - 1; i > NeighbourCount - 1; i--)
            {
                NeighbourColliders[i] = null;
            }

            NewNeighbourCounter = Random.Range(0, NewNeighbourTimer) + Mathf.Pow(NeighbourCount * 0.3f, 2);
        }
        else
        {
            NewNeighbourCounter -= Time.deltaTime;
        }


        SwarmlingLightOrbAttractionCalculation();
        

        if (LightOrbAttractionMode)
        {
            return;
        }

        SwarmlingHomeRuleCalculation();

        if (SwarmlingIsGoingHome)
        {
            return;
        }

        UpdateCounter += Time.deltaTime;

        if (UpdateCounter >= UpdateTimer)
        {
            UpdateCounter -= UpdateTimer;

            // Reset Acceleration:
            Acceleration = Vector3.zero;

            if (!DoNotMove)
            {
                SwarmlingBaseRulesCalculation();
                SwarmlingAttractionAndDangerRuleCalculation();
                SwarmlingSpecialRuleCalculation();
            }

            SwarmlingAttackRuleCalculation();
            
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

        if (DoNotMove)
        {
            return;
        }

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
            SwarmlingTransform.rotation = Quaternion.Slerp(SwarmlingTransform.rotation, Quaternion.LookRotation(Velocity), 5f * Time.deltaTime);
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
/*
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
    }*/

    // ===========================================/ RULE: DANGER AVOIDANCE /============================================

    // =============================================== RULE: ATTRACTION ================================================
    // Enemies steer towards the nearest attraction (in this case Player Characters, so far).
/*
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
    */
    // ==============================================/ RULE: ATTRACTION /===============================================

    // ============================================== RULE: GO TO BORDER ===============================================
    // Tank Enemies should steer towards the outside of the swarm.
    /*
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
    */
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
/*
    public void AddToPlayersInRange(GameObject AddPlayer)
    {
        PlayersInRange.Add(AddPlayer.GetComponent<CharacterAttention>().GetOwner());
    }

    public void RemoveFromPlayersInRanger(GameObject RemovePlayer)
    {
        PlayersInRange.Remove(RemovePlayer.GetComponent<CharacterAttention>().GetOwner());
    }
    */
    public void OnDestroy()
    {
        /*for (int i = 0; i < EnemiesInRange.Count; i++)
        {
            EnemiesInRange[i].RemoveFromEnemiesInRange(this);
        }*/
        SpawnedBy.SwarmlingDied(SwarmlingID);
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

    public void ChangeSwarmlingSpeed(float SpeedChange)
    {
        DesiredBaseSpeed += SpeedChange;
        DesiredRunSpeed += SpeedChange;
    }

    public void SwarmlingSuicide()
    {
        ThisSwarmlingCharacter.ChangeHealthCurrent(-13370); // This ensures that the Swarmling is properly removed, as it forces the system to go through all the steps of defeating and removing a character.
    }

    // ===============================================/ GETTERS/SETTERS /================================================
}
