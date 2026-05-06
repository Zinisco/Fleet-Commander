using UnityEngine;

public class HexCell : MonoBehaviour
{
    public int q;
    public int r;

    public bool isOccupied;
    public GameObject currentShip;

    [SerializeField] private Transform shipAnchor;

    private MeshRenderer meshRenderer;
    private Material originalMaterial;

    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material neighborHighlightMaterial;

    private bool isSelected;

    void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        originalMaterial = meshRenderer.material;

        if (shipAnchor == null)
            shipAnchor = transform;
    }

    public Vector3 GetShipPosition()
    {
        return shipAnchor.position;
    }

    public void Init(int qCoord, int rCoord)
    {
        q = qCoord;
        r = rCoord;
    }

    public void SetOccupied(GameObject ship)
    {
        isOccupied = true;
        currentShip = ship;
    }

    public void Clear()
    {
        isOccupied = false;
        currentShip = null;
    }

    public void SetHighlight(bool state)
    {
        if (isSelected) return;

        meshRenderer.material = state ? hoverMaterial : originalMaterial;
    }

    public void SetSelected(bool state)
    {
        isSelected = state;
        meshRenderer.material = state ? selectedMaterial : originalMaterial;
    }

    public void SetShip(GameObject ship)
    {
        isOccupied = ship != null;
        currentShip = ship;
    }

    public void SetNeighborHighlight(bool state)
    {
        if (isSelected) return;

        meshRenderer.material = state ? neighborHighlightMaterial : originalMaterial;
    }
}