using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingPuzzleManager : MonoBehaviour {
    [SerializeField] private Transform puzzleContainer;
    [SerializeField] private Transform tileTemplate;

    private List<Transform> tileList;
    private int emptyTileIndex;
    private int gridSize;
    private bool isShuffling = false;
    private float elapsedTime = 0f;

    private void Start() {
        tileList = new List<Transform>();
        gridSize = 4; // initially 4x4
        InitializePuzzle(0.01f);
    }

    private void Update() {
        if (!isShuffling && IsPuzzleSolved()) {
            isShuffling = true;
            StartCoroutine(ShuffleAfterDelay(0.5f));
        }

        if (Input.GetMouseButtonDown(0)) {
            HandleTileClick();
        }

        elapsedTime += Time.deltaTime;
    }

    private void InitializePuzzle(float gap) {
        float tileSize = 1f / gridSize;
        for (int row = 0; row < gridSize; row++) {
            for (int col = 0; col < gridSize; col++) {
                Transform tile = Instantiate(tileTemplate, puzzleContainer);
                tileList.Add(tile);
                PositionTile(tile, tileSize, gap, row, col);
                if (IsLastTile(row, col)) {
                    SetEmptyTile(tile, gridSize);
                } else {
                    SetTileUVs(tile, tileSize, gap, row, col);
                }
            }
        }
    }

    private void PositionTile(Transform tile, float tileSize, float gap, int row, int col) {
        tile.localPosition = new Vector3(-1 + (2 * tileSize * col) + tileSize, +1 - (2 * tileSize * row) - tileSize, 0);
        tile.localScale = ((2 * tileSize) - gap) * Vector3.one;
        tile.name = $"{(row * gridSize) + col}";
    }

    private bool IsLastTile(int row, int col) {
        return (row == gridSize - 1) && (col == gridSize - 1);
    }

    private void SetEmptyTile(Transform tile, int size) {
        emptyTileIndex = (size * size) - 1;
        tile.gameObject.SetActive(false);
    }

    private void SetTileUVs(Transform tile, float tileSize, float gap, int row, int col) {
        float halfGap = gap / 2f;
        Mesh tileMesh = tile.GetComponent<MeshFilter>().mesh;
        Vector2[] uvCoordinates = {
            new Vector2((tileSize * col) + halfGap, 1 - ((tileSize * (row + 1)) - halfGap)),
            new Vector2((tileSize * (col + 1)) - halfGap, 1 - ((tileSize * (row + 1)) - halfGap)),
            new Vector2((tileSize * col) + halfGap, 1 - ((tileSize * row) + halfGap)),
            new Vector2((tileSize * (col + 1)) - halfGap, 1 - ((tileSize * row) + halfGap))
        };
        tileMesh.uv = uvCoordinates;
    }

    private void HandleTileClick() {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit) {
            for (int i = 0; i < tileList.Count; i++) {
                if (tileList[i] == hit.transform) {
                    if (SwapTiles(i, -gridSize, gridSize) || SwapTiles(i, gridSize, gridSize) || SwapTiles(i, -1, 0) || SwapTiles(i, 1, gridSize - 1)) {
                        break;
                    }
                }
            }
        }
    }

    private bool SwapTiles(int index, int offset, int columnCondition) {
        if ((index % gridSize != columnCondition) && (index + offset == emptyTileIndex)) {
            Transform tempTile = tileList[index];
            tileList[index] = tileList[index + offset];
            tileList[index + offset] = tempTile;

            Vector3 tempPosition = tileList[index].localPosition;
            tileList[index].localPosition = tileList[index + offset].localPosition;
            tileList[index + offset].localPosition = tempPosition;

            emptyTileIndex = index;
            return true;
        }
        return false;
    }

    private bool IsPuzzleSolved() {
        for (int i = 0; i < tileList.Count; i++) {
            if (tileList[i].name != $"{i}") {
                return false;
            }
        }
        return true;
    }

    private IEnumerator ShuffleAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        AdjustGridSize();
        ShuffleTiles();
        isShuffling = false;
    }

    private void AdjustGridSize() {
        gridSize = elapsedTime < 60f ? Mathf.Min(gridSize + 1, 6) : 4;
        elapsedTime = 0f;
        ResetPuzzle();
    }

    private void ResetPuzzle() {
        foreach (Transform tile in tileList) {
            Destroy(tile.gameObject);
        }
        tileList.Clear();
        InitializePuzzle(0.01f);
    }

    private void ShuffleTiles() {
        int moves = 0;
        int lastEmptyTile = 0;
        int maxMoves = gridSize * gridSize * gridSize;
        while (moves < maxMoves) {
            int randomIndex = Random.Range(0, gridSize * gridSize);
            if (randomIndex == lastEmptyTile) continue;
            lastEmptyTile = emptyTileIndex;
            if (SwapTiles(randomIndex, -gridSize, gridSize) || SwapTiles(randomIndex, gridSize, gridSize) || SwapTiles(randomIndex, -1, 0) || SwapTiles(randomIndex, 1, gridSize - 1)) {
                moves++;
            }
        }
    }
}