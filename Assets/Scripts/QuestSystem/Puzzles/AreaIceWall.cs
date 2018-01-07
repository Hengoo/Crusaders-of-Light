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

    private Transform _iceWallTransform;
    private Transform _fireMageTransform;

    [HideInInspector] private List<QuestBase> _questSteps = new List<QuestBase>();

    public AreaIceWall(Transform iceWallTransform, Transform fireMageTransform)
    {
        _iceWallTransform = iceWallTransform;
        _fireMageTransform = fireMageTransform;
    }

    //Generate the quests to be given to the quest controller
    public override QuestBase[] GenerateQuests(SceneryStructure sceneryStructure)
    {
        //Create ice wall in the game world
        var iceWall = Instantiate(IceWallPrefab);
        iceWall.transform.position = _iceWallTransform.position;
        iceWall.transform.rotation = _iceWallTransform.rotation;
        iceWall.transform.localScale = _iceWallTransform.localScale;

        //Give fire mage the special weapon
        var firestaff = FireMage.GetComponent<Character>().StartingWeapons.First(a => a.name.ToLower() == "firestaff");
        
        //Find ice wall
        _questSteps.Add(new QuestReachPlace(iceWall, 5));

        //Find fire mage camp
        _questSteps.Add(new QuestReachPlace(_fireMageTransform.gameObject, 5));

        //Kill fire mage
        _questSteps.Add(new QuestKillEnemy(_fireMageTransform, FireMage));

        //Pickup fire mage staff
        _questSteps.Add(new QuestPickupItem(firestaff));

        //Destroy the ice wall
        _questSteps.Add(new QuestDestroyBuilding(iceWall.GetComponent<CharacterEnemy>()));

        return _questSteps.ToArray();
    } 
}
