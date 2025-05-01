using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

public class ResourcePointManager : MonoBehaviour
{
    public Tilemap tilemap;
    public Vector3Int[] mapData = {
        new (0, 0, 0),
        new (2, 10, 20),
        new (-2, -50, -20),
        new (0, 1, -2)
    };

    private TileBase tileToPlace;
    public Vector2 tileSize = new Vector2(1f, 1f);
    private List<ResourceBattleground> battlegrounds = new List<ResourceBattleground>();

    private async void Start()
    {
        tileToPlace = Resources.Load<TileBase>("Tiles/cave1");
        await LoadResourcePoints();
    }

    private async Task LoadResourcePoints()
    {
        // Mock API call
        string mockApiUrl = "https://mockapi.io/resource-points";

        using (UnityWebRequest request = UnityWebRequest.Get(mockApiUrl))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;

                // JSONUtility không hỗ trợ danh sách gốc, cần wrapper nếu không có Newtonsoft
                // battlegrounds = JsonUtility.FromJson<List<ResourceBattleground>>(jsonResponse); ❌
                // Tạm thời dùng mock
                CreateMockData();
            }
            else
            {
                Debug.LogWarning("Failed to load, using mock: " + request.error);
                CreateMockData();
            }

            SpawnResourcePoints();
        }
    }

    private void CreateMockData()
    {
        battlegrounds = new List<ResourceBattleground>
        {
            new()
            {
                id = 1,
                cardInfos = new List<CardInfo>
                {
                    new () { id = "assassin"},
                    new () { id = "black_smith"},
                    new () { id = "deadeye"},
                    new () { id = "eira_lumina"},
                    new () { id = "elderwood_guardian"}
                },
                levels = new List<int> {1, 2, 1, 1, 1}
            },
            new() {
                id = 2,
                cardInfos = new List<CardInfo>
                {
                    new () { id = "black_smith"},
                    new () { id = "everlasting_esentinel"},
                    new () { id = "flame_sorcerer"},
                    new () { id = "furious_cavalry"},
                    new () { id = "guardian"},
                },
                levels = new List<int> {1, 2, 1, 1, 1}
            },
            new()
            {
                id = 1,
                cardInfos = new List<CardInfo>
                {
                    new () { id = "assassin"},
                    new () { id = "black_smith"},
                    new () { id = "deadeye"},
                    new () { id = "eira_lumina"},
                    new () { id = "elderwood_guardian"}
                },
                levels = new List<int> {1, 2, 1, 1, 1}
            },
            new() {
                id = 2,
                cardInfos = new List<CardInfo>
                {
                    new () { id = "black_smith"},
                    new () { id = "everlasting_esentinel"},
                    new () { id = "flame_sorcerer"},
                    new () { id = "furious_cavalry"},
                    new () { id = "guardian"},
                },
                levels = new List<int> {1, 2, 1, 1, 1}
            }
        };
    }

    private void SpawnResourcePoints()
    {
        int battlegroundIndex = 0;

        foreach (var battleground in battlegrounds)
        {
            tilemap.SetTile(mapData[battlegroundIndex], tileToPlace);
            battlegroundIndex++;
        }
    }
}
