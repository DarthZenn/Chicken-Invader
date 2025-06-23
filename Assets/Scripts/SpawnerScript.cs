using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] float gridSize = 1f;
    public Transform gridChicken;

    [Header("References")]
    [SerializeField] GameManager gameManager;

    public static SpawnerScript Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        float h = Camera.main.orthographicSize * 2f;
        float w = h * Screen.width / Screen.height;

        Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
        Vector3 start = new(topLeft.x + gridSize * .5f + w * .25f,
                              topLeft.y - gridSize,
                              0);

        int rows = Mathf.FloorToInt(h * .5f / gridSize);
        int cols = Mathf.FloorToInt(w / (gridSize * 1.5f));

        SpawnGrid(rows, cols, start);
    }

    void SpawnGrid(int rows, int cols, Vector3 anchor)
    {
        for (int r = 0; r < rows; ++r)
        {
            Vector3 rowPos = anchor - new Vector3(0, r * gridSize, 0);

            for (int c = 0; c < cols; ++c)
            {
                Vector3 spawnPos = rowPos + new Vector3((c + 1) * gridSize, 0, 0);

                gameManager.SpawnChicken(spawnPos);
            }
        }
    }
}
