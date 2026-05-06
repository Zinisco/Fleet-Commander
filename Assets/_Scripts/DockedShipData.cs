[System.Serializable]
public class DockedShipData
{
    public ShipDefinition definition;
    public int level = 1;

    public DockedShipData(ShipDefinition definition, int level = 1)
    {
        this.definition = definition;
        this.level = level;
    }
}