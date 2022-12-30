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

        [PluginEntryPoint("RemoteKeycard", "1.0.2", "Allow player to open doors and lockers without a Keycard in hand", "SrLicht")]
        void LoadPlugin()
        {
            PluginAPI.Events.EventManager.RegisterEvents(this);
        }
        
        // These damn events need a lot of checks to keep bad things from happening so read it at your own risk.

        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        void OnPlayerInteractDoor(Player ply, DoorVariant door, bool canAccess)
        {
            if (Config.IsEnabled && !Config.BlackListRole.Contains(ply.Role))
            {
                if (!ply.IsWithoutItems() && ply.HasKeycardPermission(door.RequiredPermissions.RequiredPermissions) &&
                    ply.CurrentItem is not KeycardItem)
                {
                    canAccess = true;
                    door.Toggle();
                }
            }
        }

        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        void OnPlayerInteractLocker(Player ply, Locker locker, byte colliderID, bool canAccess)
        {
            if (Config.IsEnabled && !Config.BlackListRole.Contains(ply.Role))
            {
                if(locker.Chambers.TryGet(colliderID, out var chamber) && chamber.AcceptableItems.Any(i => i.IsSCP()) && !ply.IsWithoutItems() &&
                ply.HasKeycardPermission(chamber.RequiredPermissions, true) && ply.CurrentItem is not KeycardItem)
                {
                    canAccess = true;
                    locker.Toggle(colliderID);
                }
            }
        }
    }
}