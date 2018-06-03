using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmAttention : MonoBehaviour {

    public EnemySwarm Owner;

    // ================================================== NEARBY LISTS ==================================================

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EnemySwarm")
        {
            Owner.AddToEnemiesInRange(other.GetComponent<SwarmAttention>().GetOwner());
        }
        else if (other.tag == "SwarmDanger")
        {
            Owner.AddToDangersInRange(other.gameObject);
        }
        else if (other.tag == "Attention")
        {
            /*Character otherChar = other.GetComponent<Character>();
            if (otherChar.GetAlignment() == Character.TeamAlignment.PLAYERS)
            {
                PlayersInRange.Add(otherChar);
            }*/
            Owner.AddToPlayersInRange(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "EnemySwarm")
        {
            Owner.RemoveFromEnemiesInRange(other.GetComponent<SwarmAttention>().GetOwner());
        }
        else if (other.tag == "SwarmDanger")
        {
            Owner.RemoveFromDangersInRange(other.gameObject);
        }
        else if (other.tag == "Attention")
        {
            /*Character otherChar = other.GetComponent<Character>();
            if (otherChar.GetAlignment() == Character.TeamAlignment.PLAYERS)
            {
                PlayersInRange.Remove(otherChar);
            }*/
            Owner.RemoveFromPlayersInRanger(other.gameObject);
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
        Owner.RemoveFromEnemiesInRange(SwarmObject);
    }

    // =================================================/ NEARBY LISTS /=================================================

    public EnemySwarm GetOwner()
    {
        return Owner;
    }

}
