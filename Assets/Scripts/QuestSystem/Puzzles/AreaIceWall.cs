using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/*
 * This class creates the Ice Wall area
 * 
 * Quests:
 *  -Find Ice Wall
 *  -Find Fire Mage camp
 *  -Kill Fire Mage
 *  -Pickup Fire Mage Staff
 *  -Destroy the Ice Wall
 *  
 */

[CreateAssetMenu(fileName = "Area_IceWall", menuName = "Areas/Ice wall")]
public class AreaIceWall : AreaBase
{

    public GameObject IceWallPrefab;
    public GameObject FireMage; //Must have enemy character script attached to it
    public Weapon FireStaff;

    [HideInInspector] private List<QuestBase> _questSteps = new List<QuestBase>();

    //Generate the quests to be given to the quest controller
    public QuestBase[] GenerateQuests(Transform iceWallTransform, Transform fireMageTransform)
    {
        //Create ice wall in the game world
        var iceWall = Instantiate(IceWallPrefab);
        iceWall.transform.position = iceWallTransform.position;
        iceWall.transform.rotation = iceWallTransform.rotation;
        iceWall.transform.localScale = iceWallTransform.localScale;

        //Give fire mage the special weapon
        var firestaff = FireMage.GetComponent<Character>().StartingWeapons.First(a => a.name.ToLower() == "firestaff");
        
        //Find ice wall
        _questSteps.Add(new QuestReachPlace(iceWall, 5));

        //Find fire mage camp
        _questSteps.Add(new QuestReachPlace(fireMageTransform.gameObject, 5));

        //Kill fire mage
        _questSteps.Add(new QuestKillEnemy(fireMageTransform, FireMage));

        //Pickup fire mage staff
        _questSteps.Add(new QuestPickupItem(firestaff));

        //Destroy the ice wall
        _questSteps.Add(new QuestDestroyBuilding(iceWall.GetComponent<CharacterEnemy>()));

        return _questSteps.ToArray();
    } 
}
