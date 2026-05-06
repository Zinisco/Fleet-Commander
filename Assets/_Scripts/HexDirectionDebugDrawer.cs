using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HexDirectionDebugDrawer : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private HexCell centerCell;

    [Header("Label Settings")]
    [SerializeField] private TMP_Text labelPrefab;
    [SerializeField] private float labelYOffset = 0.15f;

    private readonly List<GameObject> activeLabels = new();

    private void Awake()
    {
        if (hexGrid == null)
            hexGrid = FindFirstObjectByType<HexGrid>();
    }

    [ContextMenu("Draw Direction Labels")]
    public void DrawDirectionLabels()
    {
        ClearLabels();

        if (hexGrid == null || centerCell == null || labelPrefab == null)
            return;

        List<HexCell> neighbors = hexGrid.GetNeighbors(centerCell);

        foreach (HexCell neighbor in neighbors)
        {
            if (neighbor == null)
                continue;

            HexDirection direction = hexGrid.GetDirection(centerCell, neighbor);

            Vector3 position = neighbor.transform.position + Vector3.up * labelYOffset;

            TMP_Text label = Instantiate(labelPrefab, position, Quaternion.identity, transform);
            label.text = direction.ToString();
            label.name = $"Direction Label - {direction}";

            activeLabels.Add(label.gameObject);
        }
    }

    [ContextMenu("Clear Direction Labels")]
    public void ClearLabels()
    {
        foreach (GameObject label in activeLabels)
        {
            if (label != null)
                DestroyImmediate(label);
        }

        activeLabels.Clear();
    }
}