namespace InventoryMoveSpeed;

public static class PackLoadConstants
{
    /// <summary>Unique <see cref="Entity.Stats"/> layer; do not share with other mods.</summary>
    public const string StatLayer = "inventorymovespeed";

    /// <summary>Each integer percent away from 50% contributes this much additive walkspeed (0.5% = 0.005).</summary>
    public const float MoveAddPerOnePercentFromHalf = 0.005f;
}
