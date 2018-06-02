using System.Collections.Generic;
using UnityEngine;

public class SceneryStructure
{
    public List<AreaSettings> ChestAreas = new List<AreaSettings>();
    public List<AreaSettings> MainPathAreas = new List<AreaSettings>();
    public List<AreaSettings> SidePathAreas = new List<AreaSettings>();
    public AreaSettings BossArea;

    public SceneryStructure(StoryStructure storyStructure, TerrainStructure terrainStructure)
    {

        // TODO: fill roads and paths - place elements along roads and keep roads clear
        // TODO: fill special areas - build structures
        // TODO: fill boss area - place logic elements
    }
}