using UnityEngine;

public class Cell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool isAlive;
    private GameOfLifeManager gameOfLifeManager;
    private int x;
    private int y;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetAlive(bool isAlive)
    {
        this.isAlive = isAlive;
        spriteRenderer.color = isAlive ? Color.black : Color.white;
    }

    public void Initialize(GameOfLifeManager manager, int x, int y)
    {
        this.gameOfLifeManager = manager;
        this.x = x;
        this.y = y;
    }

    void OnMouseDown()
    {
        isAlive = !isAlive;
        SetAlive(isAlive);
        gameOfLifeManager.SetCellState(x, y, isAlive);
    }
}