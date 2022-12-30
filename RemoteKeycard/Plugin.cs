using System;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using MapGeneration.Distributors;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using YamlDotNet.Core.Tokens;

namespace RemoteKeycard
{
    public class Plugin
    {
        [PluginConfig]
        public Config Config;

        [PluginEntryPoint("RemoteKeycard", "1.1.0", "Allow player to open doors and lockers without a Keycard in hand", "SrLicht")]
        void LoadPlugin()
        {
            PluginAPI.Events.EventManager.RegisterEvents(this);
        }
        
        // These damn events need a lot of checks to keep bad things from happening so read it at your own risk.

        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        bool OnPlayerInteractDoor(Player ply, DoorVariant door, bool canOpen)
        {
            if (!Config.IsEnabled || ply.IsSCP() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                Config.BlacklistedDoors.Any(d => door.name.StartsWith(d))
                || ply.CurrentItem is KeycardItem) return true;

            if (ply.HasKeycardPermission(door.RequiredPermissions.RequiredPermissions))
            {
                canOpen = true;
                door.Toggle();
                return false;
            }

            return true;
        }

        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        bool OnPlayerInteractLocker(Player ply, Locker locker, byte colliderID, bool canAccess)
        {
            if (!Config.IsEnabled || ply.IsSCP() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                Config.BlacklistedLockers.Any(l => locker.name.StartsWith(l)) ||
                ply.CurrentItem is KeycardItem) return true;
            
            if(locker.Chambers.TryGet(colliderID, out var chamber) && ply.HasKeycardPermission(chamber.RequiredPermissions, true))
            {
                canAccess = true;
                locker.Toggle(colliderID);
                return false;
            }

            return true;
        }
    }
}