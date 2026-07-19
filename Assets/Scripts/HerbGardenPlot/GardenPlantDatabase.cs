using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GardenPlantDatabase", menuName = "Grand Healer/Garden Plant Database")]
public class GardenPlantDatabase : ScriptableObject
{
    public List<GardenPlantData> plants = new List<GardenPlantData>();
}