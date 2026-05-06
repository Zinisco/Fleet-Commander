using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public GameObject hexPrefab;
    public float hexSize = 1f;
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 4;
    [SerializeField] private float spacing = 0.85f;

    private Dictionary<Vector2Int, HexCell> grid = new();

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int r = 0; r < height; r++)
        {
            for (int q = 0; q < width; q++)
            {
                // Offset every other row (this creates the hex stagger)
                float xOffset = (r % 2 == 0) ? 0 : hexSize * Mathf.Sqrt(3f) / 2f;

                float x = (Mathf.Sqrt(3f) * hexSize * q + xOffset) * spacing;
                float z = (1.5f * hexSize * r) * spacing;

                Vector3 pos = new Vector3(x, 0, z);

                GameObject hexObj = Instantiate(hexPrefab, pos, Quaternion.identity, transform);

                HexCell cell = hexObj.GetComponent<HexCell>();
                cell.Init(q, r);

                grid[new Vector2Int(q, r)] = cell;
            }
        }
    }

    Vector3 HexToWorld(int q, int r)
    {
        float x = hexSize * (Mathf.Sqrt(3f) * q + Mathf.Sqrt(3f) / 2f * r) * spacing;
        float z = hexSize * (1.5f * r) * spacing;

        return new Vector3(x, 0, z);
    }

    public HexCell GetCell(int q, int r)
    {
        grid.TryGetValue(new Vector2Int(q, r), out HexCell cell);
        return cell;
    }

    public bool TryGetCell(int q, int r, out HexCell cell)
    {
        return grid.TryGetValue(new Vector2Int(q, r), out cell);
    }

    public List<HexCell> GetNeighbors(HexCell cell)
    {
        List<HexCell> neighbors = new();

        if (cell == null)
            return neighbors;

        Vector2Int[] directions;

        // Odd-row offset layout
        if (cell.r % 2 == 0)
        {
            directions = new Vector2Int[]
            {
            new Vector2Int(-1, 0),  // left
            new Vector2Int(1, 0),   // right
            new Vector2Int(0, -1),  // upper right
            new Vector2Int(-1, -1), // upper left
            new Vector2Int(0, 1),   // lower right
            new Vector2Int(-1, 1)   // lower left
            };
        }
        else
        {
            directions = new Vector2Int[]
            {
            new Vector2Int(-1, 0),  // left
            new Vector2Int(1, 0),   // right
            new Vector2Int(1, -1),  // upper right
            new Vector2Int(0, -1),  // upper left
            new Vector2Int(1, 1),   // lower right
            new Vector2Int(0, 1)    // lower left
            };
        }

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborCoord = new Vector2Int(cell.q + dir.x, cell.r + dir.y);

            if (grid.TryGetValue(neighborCoord, out HexCell neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public HexDirection GetDirection(HexCell from, HexCell to)
    {
        int dq = to.q - from.q;
        int dr = to.r - from.r;

        if (dq == -1 && dr == 0) return HexDirection.Right;
        if (dq == 1 && dr == 0) return HexDirection.Left;

        if (from.r % 2 == 0)
        {
            // Even row
            if (dq == -1 && dr == -1) return HexDirection.FrontRight;
            if (dq == 0 && dr == -1) return HexDirection.FrontLeft;

            if (dq == -1 && dr == 1) return HexDirection.BackRight;
            if (dq == 0 && dr == 1) return HexDirection.BackLeft;
        }
        else
        {
            // Odd row
            if (dq == 0 && dr == -1) return HexDirection.FrontRight;
            if (dq == 1 && dr == -1) return HexDirection.FrontLeft;

            if (dq == 0 && dr == 1) return HexDirection.BackRight;
            if (dq == 1 && dr == 1) return HexDirection.BackLeft;
        }

        Debug.LogWarning($"Cells are not neighbors: {from.q},{from.r} -> {to.q},{to.r}");
        return HexDirection.Right;
    }
}