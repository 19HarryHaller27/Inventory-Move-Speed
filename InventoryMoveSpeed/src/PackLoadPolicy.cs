namespace InventoryMoveSpeed;

public static class PackLoadPolicy
{
    /// <summary>
    /// <c>percentFull</c> is 0 (no slots used) to 100 (all backpack slots have something).
    /// +0.5% move per 1% below 50; -0.5% per 1% above 50. Exact 50 → 0.
    /// </summary>
    public static float ComputeMoveAddForPercentFull(float percentFull)
    {
        if (percentFull < 50f)
        {
            return (50f - percentFull) * PackLoadConstants.MoveAddPerOnePercentFromHalf;
        }

        if (percentFull > 50f)
        {
            return -(percentFull - 50f) * PackLoadConstants.MoveAddPerOnePercentFromHalf;
        }

        return 0f;
    }
}
