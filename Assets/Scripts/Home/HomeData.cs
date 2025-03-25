using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HomeData", menuName = "Game/Home Data")]
public class HomeData : ScriptableObject
{


    [Header("Cấu hình Lands")]
    public List<BuildingData> buildings = new List<BuildingData>();

    [Header("Cấu hình Buildings")]
    public List<LandData> lands = new List<LandData>();

    [Header("Giới hạn Camera")]
    public float cameraBoundaryX = 50f;
    public float cameraBoundaryY = 50f;
}

[System.Serializable]
public class BuildingData
{
    public int landIndex;
    public BuildingType buildingType;
    public string buildingName;
    public bool isLocked;
    public GameObject buildingPrefab;
    public Vector2 offset = Vector2.zero;
}

[System.Serializable]
public class LandData
{
    public Vector2 position;
    public GameObject landPrefab;
}

public enum BuildingType
{
    None,
    House,
    Farm,
    Storage,
    Market,
    Workshop
}