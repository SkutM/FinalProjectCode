using UnityEngine;

public class GameOfLifeManager : MonoBehaviour
{
    public int width = 40;
    public int height = 20;
    public float updateInterval = 1f;
    public GameObject cellPrefab;

    private float timer;
    private bool[,] grid;
    private bool[,] nextGrid;
    private Cell[,] cellObjects;

    void Start()
    {
        AdjustCamera();
        InitializeGrid();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateGrid();
            UpdateCellVisuals();
            CheckGameOver();
        }
    }

    void AdjustCamera()
    {
        Camera.main.orthographicSize = height / 2f;
        float aspectRatio = (float)width / height;
        Camera.main.orthographicSize = height / 2f;
        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
    }

    void InitializeGrid()
    {
        grid = new bool[width, height];
        nextGrid = new bool[width, height];
        cellObjects = new Cell[width, height];

        float cellWidth = 1f;
        float cellHeight = 1f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = Random.value > 0.3f;

                Vector2 position = new Vector2(x * cellWidth, y * cellHeight);
                GameObject cellObject = Instantiate(cellPrefab, position, Quaternion.identity);
                cellObject.transform.localScale = new Vector3(cellWidth, cellHeight, 1);
                Cell cell = cellObject.GetComponent<Cell>();
                cell.Initialize(this, x, y);
                cell.SetAlive(grid[x, y]);
                cellObjects[x, y] = cell;
            }
        }
    }

    public void SetCellState(int x, int y, bool isAlive)
    {
        grid[x, y] = isAlive;
    }

    void UpdateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int liveNeighbors = CountLiveNeighbors(x, y);
                if (grid[x, y])
                {
                    nextGrid[x, y] = liveNeighbors == 2 || liveNeighbors == 3; // adujust ??
                }
                else
                {
                    nextGrid[x, y] = liveNeighbors == 3;  // adjust live neighbors... 3, 4
                }
            }
        }

        bool[,] temp = grid;
        grid = nextGrid;
        nextGrid = temp;
    }

    int CountLiveNeighbors(int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                int nx = x + i;
                int ny = y + j;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    if (grid[nx, ny]) count++;
                }
            }
        }
        return count;
    }

    void UpdateCellVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cellObjects[x, y].SetAlive(grid[x, y]);
            }
        }
    }

    void CheckGameOver()
    {
        bool allDead = true;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y])
                {
                    allDead = false;
                    break;
                }
            }
            if (!allDead)
                break;
        }

        if (allDead)
        {
            Debug.Log("All cells are dead");
            // Debug Log
        }
    }
}