using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttention : MonoBehaviour {

    [Header("Character Attention:")]
    public Character Owner;
    public string Tag = "Attention";
    public string TagHitObjects = "Skills";

    public List<Character> PlayersInAttentionRange = new List<Character>();
    public List<Character> EnemiesInAttentionRange = new List<Character>();

    public List<SkillHitObject> PlayerHitObjectsInRange = new List<SkillHitObject>();

    public Character GetOwner()
    {
        return Owner;
    }

    public void OwnerDied()
    {
        for (int i = 0; i < PlayersInAttentionRange.Count; i++)
        {
            PlayersInAttentionRange[i].AttentionCharacterDied(Owner);
        }

        for (int i = 0; i < EnemiesInAttentionRange.Count; i++)
        {
            EnemiesInAttentionRange[i].AttentionCharacterDied(Owner);
        }

    }

    public void CharacterDied(Character CharDied)
    {
        if (CharDied.GetAlignment() == Character.TeamAlignment.PLAYERS)
        {
            PlayerLeavesAttentionRange(CharDied);
        }
        else
        {
            EnemyLeavesAttentionRange(CharDied);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == Tag)
        {
            CharacterAttention OtherAttention = other.gameObject.GetComponent<CharacterAttention>();

            if (OtherAttention.GetOwner().GetAlignment() == Character.TeamAlignment.PLAYERS)
            {
                PlayerEntersAttentionRange(OtherAttention.GetOwner());
            }
            else
            {
                EnemyEntersAttentionRange(OtherAttention.GetOwner());
            }
        }
        else if (other.tag == TagHitObjects)
        {
            SkillHitObject OtherHitObject = other.gameObject.GetComponent<SkillHitObject>();

            if (OtherHitObject.GetAlignment() == Character.TeamAlignment.ALL
                || OtherHitObject.GetAlignment() == Character.TeamAlignment.PLAYERS)
            {
                PlayerHitObjectsInRange.Add(OtherHitObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == Tag)
        {
            CharacterAttention OtherAttention = other.gameObject.GetComponent<CharacterAttention>();

            if (OtherAttention.GetOwner().GetAlignment() == Character.TeamAlignment.PLAYERS)
            {
                PlayerLeavesAttentionRange(OtherAttention.GetOwner());
            }
            else
            {
                EnemyLeavesAttentionRange(OtherAttention.GetOwner());
            }
        }
        else if (other.tag == TagHitObjects)
        {
            SkillHitObject OtherHitObject = other.gameObject.GetComponent<SkillHitObject>();

            if (OtherHitObject.GetAlignment() == Character.TeamAlignment.ALL
                || OtherHitObject.GetAlignment() == Character.TeamAlignment.PLAYERS)
            {
                PlayerHitObjectsInRange.Remove(OtherHitObject);
            }
        }
    }

    public void PlayerEntersAttentionRange(Character PlayerCharacter)
    {
        if (PlayersInAttentionRange.Contains(PlayerCharacter))
        {
            return;
        }

        PlayersInAttentionRange.Add(PlayerCharacter);
    }

    public void PlayerLeavesAttentionRange(Character PlayerCharacter)
    {
        PlayersInAttentionRange.Remove(PlayerCharacter);
    }

    public List<Character> GetPlayersInAttentionRange()
    {
        return PlayersInAttentionRange;
    }

    public List<Character> GetPlayersInAttentionRange(float MinDistance, float MaxDistance)
    {
        List<Character> CharactersInRange = new List<Character>();
        float DistanceToOwner = 0.0f;

        for (int i = 0; i < PlayersInAttentionRange.Count; i++)
        {
            if (CharactersInRange[i])
            {
                DistanceToOwner = Vector3.Distance(Owner.transform.position, PlayersInAttentionRange[i].transform.position);
                if (DistanceToOwner >= MinDistance && DistanceToOwner <= MaxDistance)
                {
                    CharactersInRange.Add(PlayersInAttentionRange[i]);
                }
            }
        }

        return CharactersInRange;
    }

    public void EnemyEntersAttentionRange(Character EnemyCharacter)
    {
        if (EnemiesInAttentionRange.Contains(EnemyCharacter))
        {
            return;
        }

        EnemiesInAttentionRange.Add(EnemyCharacter);
    }

    public void EnemyLeavesAttentionRange(Character EnemyCharacter)
    {
        EnemiesInAttentionRange.Remove(EnemyCharacter);
    }

    public List<Character> GetEnemiesInAttentionRange()
    {
        return EnemiesInAttentionRange;
    }

    public List<Character> GetEnemiesInAttentionRange(float MinDistance, float MaxDistance)
    {
        List<Character> CharactersInRange = new List<Character>();
        float DistanceToOwner = 0.0f;

        for (int i = 0; i < EnemiesInAttentionRange.Count; i++)
        {
            if (CharactersInRange[i])
            {
                DistanceToOwner = Vector3.Distance(Owner.transform.position, EnemiesInAttentionRange[i].transform.position);
                if (DistanceToOwner >= MinDistance && DistanceToOwner <= MaxDistance)
                {
                    CharactersInRange.Add(EnemiesInAttentionRange[i]);
                }
            }
        }

        return CharactersInRange;
    }
}
