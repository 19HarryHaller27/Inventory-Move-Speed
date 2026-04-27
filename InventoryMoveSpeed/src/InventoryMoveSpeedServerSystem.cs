using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace InventoryMoveSpeed;

public class InventoryMoveSpeedServerSystem : ModSystem
{
    private ICoreServerAPI? sapi;
    private long tickId = -1;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    public override void StartServerSide(ICoreServerAPI api)
    {
        sapi = api;
        tickId = api.Event.RegisterGameTickListener(OnServerTick, 1000, 0);
    }

    public override void Dispose()
    {
        if (sapi is not null)
        {
            if (tickId != -1)
            {
                sapi.Event.UnregisterGameTickListener(tickId);
                tickId = -1;
            }

            sapi = null;
        }
    }

    private void OnServerTick(float _dt)
    {
        ICoreServerAPI? api = sapi;
        if (api is null)
        {
            return;
        }

        foreach (IPlayer pl in api.Server.Players)
        {
            if (pl is not IServerPlayer sp)
            {
                continue;
            }

            if (sp.Entity is null)
            {
                continue;
            }

            IInventory? inv = sp.InventoryManager.GetOwnInventory(GlobalConstants.backpackInvClassName);
            if (inv is null || inv.Count < 1)
            {
                ClearOurLayer(sp.Entity);
                continue;
            }

            int used = 0;
            for (int i = 0; i < inv.Count; i++)
            {
                if (!inv[i].Empty)
                {
                    used++;
                }
            }

            float percentFull = 100f * used / inv.Count;
            float add = PackLoadPolicy.ComputeMoveAddForPercentFull(percentFull);
            if (add == 0f)
            {
                sp.Entity.Stats.Remove("walkspeed", PackLoadConstants.StatLayer);
                sp.Entity.Stats.Remove("sprintSpeed", PackLoadConstants.StatLayer);
            }
            else
            {
                sp.Entity.Stats.Set("walkspeed", PackLoadConstants.StatLayer, add, persistent: false);
                sp.Entity.Stats.Set("sprintSpeed", PackLoadConstants.StatLayer, add, persistent: false);
            }
        }
    }

    private static void ClearOurLayer(Entity e)
    {
        e.Stats.Remove("walkspeed", PackLoadConstants.StatLayer);
        e.Stats.Remove("sprintSpeed", PackLoadConstants.StatLayer);
    }
}
