using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class HomeManager : MonoBehaviour
{
    public static HomeManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private HomeData homeData;
    private List<GameObject> lands = new List<GameObject>();
    private List<GameObject> buildings = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GenerateLands();
        PlaceBuildings();
    }

    private void GenerateLands()
    {
        foreach (var landData in homeData.lands)
        {
            GameObject land = Instantiate(landData.landPrefab, landData.position, quaternion.identity);
            lands.Add(land);
        }
    }

    private void PlaceBuildings()
    {
        List<BuildingData> buildings = homeData.buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            if (buildings[i].landIndex >= lands.Count) continue;
            if (buildings[i].buildingType == BuildingType.None) continue;

            GameObject buildingPrefab =
                                        Instantiate(
                                            buildings[i].buildingPrefab,
                                            homeData.lands[i].position + buildings[i].offset,
                                            Quaternion.identity
                                        );

            this.buildings.Add(buildingPrefab);
        }
    }

    public Vector3 GetCamPos()
    {
        return homeData.lands[0].position;
    }
}