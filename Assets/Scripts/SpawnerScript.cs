using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    private float gridSize = 1;
    private Vector3 SpawnPos;
    private int CurrentChicken;
    [SerializeField] private GameObject ChickenPrefab;
    [SerializeField] private Transform GridChicken;
    [SerializeField] private GameObject Boss;
    public static SpawnerScript Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        float height = Camera.main.orthographicSize * 2;
        float width = height * Screen.width / Screen.height;
        SpawnPos = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
        SpawnPos.x += ((gridSize / 2 + (width / 4)));
        SpawnPos.y -= gridSize;
        SpawnPos.z = 0;
        SpawnChicken(Mathf.FloorToInt(height / 2 / gridSize), Mathf.FloorToInt(width / gridSize / 1.5f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnChicken(int row, int numberChicken)
    {
        float x = SpawnPos.x;
        for (int i = 0; i < row; i++)
        {
            for(int j = 0; j < numberChicken; j++)
            {
                SpawnPos.x = SpawnPos.x + gridSize;
                GameObject Chicken = Instantiate(ChickenPrefab, SpawnPos, Quaternion.identity);
                Chicken.transform.parent = GridChicken;
                CurrentChicken++;
            }
            SpawnPos.x = x;
            SpawnPos.y -= gridSize;
        }
    }

    public void DecreaseChicken()
    {
        CurrentChicken--;
        if (CurrentChicken <= 0)
        {
            Boss.gameObject.SetActive(true);
        }
    }
}
