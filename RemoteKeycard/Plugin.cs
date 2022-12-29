using System;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using MapGeneration.Distributors;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace RemoteKeycard
{
    public class Plugin
    {
        [PluginConfig]
        public Config Config;

        [PluginEntryPoint("RemoteKeycard", "1.0.0", "Allow player to open doors and lockers without a Keycard in hand", "SrLicht")]
        void LoadPlugin()
        {
            PluginAPI.Events.EventManager.RegisterEvents(this);
        }

        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        void OnPlayerInteractDoor(Player ply, DoorVariant door, bool _)
        {
            
            if (!Config.IsEnabled || !Config.BlackListRole.Contains(ply.Role) && ply.ReferenceHub.inventory.UserInventory.Items.Count > 0 &&
                ply.HasKeycardPermission(door.RequiredPermissions.RequiredPermissions) && ply.CurrentItem is not KeycardItem)
            {
                door.Toggle();
            }
        }

        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        void OnPlayerInteractLocker(Player ply, Locker locker, byte colliderID, bool canAccess)
        {
            if (!Config.IsEnabled || !Config.BlackListRole.Contains(ply.Role) && locker.Chambers.TryGet(colliderID, out var chamber) &&
                ply.HasKeycardPermission(chamber.RequiredPermissions, true) && ply.CurrentItem is not KeycardItem)
            {
                canAccess = true;
                locker.Toggle(colliderID);
            }
        }
        
    }
}