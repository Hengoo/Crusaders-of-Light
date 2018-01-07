using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner_Order", menuName = "Spawner/SpawnerOrder", order = 4)]
public class SpawnerOrder : ScriptableObject {

    public enum Mode
    {
        FORCE_ONE,
        FORBID_ALL
    }

    [Header("Spawner Order:")]
    public Mode OrderMode = Mode.FORBID_ALL;
    public Weapon[] Weapons;

    public SpawnerEnemyObject.SpawnItem[] RemoveWeaponsFromList(SpawnerEnemyObject.SpawnItem[] OriginalWeaponList)
    {
        List<SpawnerEnemyObject.SpawnItem> ModifiedList = new List<SpawnerEnemyObject.SpawnItem>();

        for (int i = 0; i < OriginalWeaponList.Length; i++)
        {
            if (!ContainsWeapon(OriginalWeaponList[i].SpawnWeapon))
            {
                ModifiedList.Add(OriginalWeaponList[i]);
            }
        }

        return ModifiedList.ToArray();
    }

    private bool ContainsWeapon(Weapon WeaponToCheck)
    {
        for (int i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i] == WeaponToCheck)
            {
                return true;
            }
        }
        return false;
    }

    public Weapon RollForWeapon()
    {
        int Roll = Random.Range(0, Weapons.Length - 1);
        return Weapons[Roll];
    }

    public Mode GetMode()
    {
        return OrderMode;
    }

    public Weapon[] GetWeapons()
    {
        return Weapons;
    }
    
}
