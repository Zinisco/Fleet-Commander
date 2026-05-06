using UnityEngine;
using UnityEngine.InputSystem;

public class HexHoverManager : MonoBehaviour
{
    [SerializeField] private LayerMask hoverLayers;

    private HexCell currentHovered;
    private Camera cam;

    public static bool IsDraggingShip;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleHover();
    }

    void HandleHover()
    {
        if (Mouse.current == null) return;

        if (IsDraggingShip)
        {
            ClearHover();
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, hoverLayers);

        HexCell hoveredCell = null;

        foreach (RaycastHit hit in hits)
        {
            PlacedShip hoveredShip = hit.collider.GetComponentInParent<PlacedShip>();

            if (hoveredShip != null)
            {
                hoveredCell = hoveredShip.CurrentCell;
                break;
            }

            if (hoveredCell == null)
                hoveredCell = hit.collider.GetComponentInParent<HexCell>();
        }

        if (hoveredCell != null)
        {
            if (currentHovered != hoveredCell)
            {
                ClearHexHoverOnly();

                currentHovered = hoveredCell;
                currentHovered.SetHighlight(true);
            }

            return;
        }

        ClearHover();
    }

    void ClearHover()
    {
        ClearHexHoverOnly();
    }

    void ClearHexHoverOnly()
    {
        if (currentHovered != null)
        {
            currentHovered.SetHighlight(false);
            currentHovered = null;
        }
    }
}