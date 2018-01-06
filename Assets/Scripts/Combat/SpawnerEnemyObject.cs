using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner_Enemy", menuName = "Spawner/SpawnerEnemy", order = 2)]
public class SpawnerEnemyObject : ScriptableObject {

    [System.Serializable]
    public struct SpawnItem
    {
        public Weapon SpawnWeapon;
        public int SpawnWeight;
    }

    [System.Serializable]
    public struct SpawnEnemy
    {
        public CharacterEnemy CharacterBase;
        public Weapon[] Weapons;
        public int[] WeaponLevels;
        public int PowerLevel;
    }

    [Header("Spawner Set Characters:")]
    public CharacterEnemy EnemyPrefab;

    [Header("Spawner Set Weapons:")]
    public SpawnItem[] ListWeaponsLeft = new SpawnItem[0];
    public SpawnItem[] ListWeaponsRight = new SpawnItem[0];

    public SpawnEnemy GenerateEnemy(Spawner MainSpawner)
    {
        SpawnEnemy GeneratedEnemy = new SpawnEnemy();

        GeneratedEnemy.CharacterBase = EnemyPrefab;
  
        GeneratedEnemy.Weapons = new Weapon[2];
        GeneratedEnemy.Weapons[0] = RollForWeapon(MainSpawner, 0).SpawnWeapon;

        if (!GeneratedEnemy.Weapons[0] || (GeneratedEnemy.Weapons[0] && !GeneratedEnemy.Weapons[0].IsTwoHanded()))
        {
            GeneratedEnemy.Weapons[1] = RollForWeapon(MainSpawner, 1).SpawnWeapon;
        }

        GeneratedEnemy.WeaponLevels = new int[GeneratedEnemy.Weapons.Length];
        for (int i = 0; i < GeneratedEnemy.Weapons.Length; i++)
        {
            if (GeneratedEnemy.Weapons[i])
            {
                GeneratedEnemy.WeaponLevels[i] = Random.Range(MainSpawner.GetMinLevel(), MainSpawner.GetMaxLevel());
            }
            else
            {
                GeneratedEnemy.WeaponLevels[i] = 0;
            }
        }

        int TotalPowerLevel = GeneratedEnemy.CharacterBase.GetBasePowerLevel();
        for (int i = 0; i < GeneratedEnemy.Weapons.Length; i++)
        {
            if (GeneratedEnemy.Weapons[i])
            {
                TotalPowerLevel += GeneratedEnemy.Weapons[i].GetTotalPowerLevel();
                TotalPowerLevel += GeneratedEnemy.WeaponLevels[i] * MainSpawner.GetPowerLevelCostPerLevelUp();
            }
        }

        GeneratedEnemy.PowerLevel = TotalPowerLevel;

        return GeneratedEnemy;
    }

    private SpawnItem RollForWeapon(Spawner MainSpawner, int HandID)
    {
        // Currently forces to spawn a weapon from the force orders in the main hand. Does not care whether the weapon makes sense on the character.
        if (HandID == 0 && MainSpawner.GetSpawnOrdersForce().Count > 0)
        {
            SpawnerOrder OrderForce = MainSpawner.GetSpawnOrdersForce()[0];

            SpawnItem ForcedItem = new SpawnItem
            {
                SpawnWeight = 1,
                SpawnWeapon = OrderForce.RollForWeapon()
            };

            MainSpawner.SpawnOrderForceFulfilled(OrderForce);

            return ForcedItem;
        }

        SpawnItem[] AllowedWeaponList = Weapons(HandID);

        SpawnerOrder[] OrdersForbid = MainSpawner.GetSpawnOrderForbid();

        for (int i = 0; i < OrdersForbid.Length; i++)
        {
            AllowedWeaponList = OrdersForbid[i].RemoveWeaponsFromList(AllowedWeaponList);
        }

        int NumberOfWeapons = AllowedWeaponList.Length;
        int TotalWeight = 0;

        for (int i = 0; i < AllowedWeaponList.Length; i++)
        {
            TotalWeight += AllowedWeaponList[i].SpawnWeight;
        }

        int RolledNumber = Random.Range(0, TotalWeight);
        int WeightCounter = 0;

        for (int i = 0; i < AllowedWeaponList.Length; i++)
        {
            WeightCounter += AllowedWeaponList[i].SpawnWeight;

            if (RolledNumber < WeightCounter)
            {
                return AllowedWeaponList[i];
            }
        }

        return new SpawnItem(); // Should not be reachable!
    }

    private SpawnItem[] Weapons(int HandID)
    {
        if (HandID == 0)
        {
            return ListWeaponsLeft;
        }
        return ListWeaponsRight;
    }
}
