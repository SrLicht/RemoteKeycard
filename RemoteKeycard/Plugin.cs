using System;
using System.Linq;
using System.Reflection;
using Footprinting;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using MapGeneration.Distributors;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using YamlDotNet.Core.Tokens;

namespace RemoteKeycard
{
    
    public class Plugin
    {
        public static Plugin Singleton;
        
        [PluginConfig] public Config Config;
        
        public const string Version = "1.1.3";
        
        [PluginEntryPoint("RemoteKeycard", Version,
            "Allow player to open doors, lockers and generators without a Keycard in hand", "SrLicht")]
        void LoadPlugin()
        {
            Singleton = this;
            PluginAPI.Events.EventManager.RegisterEvents(this);
        }

        // These damn events need a lot of checks to keep bad things from happening so read it at your own risk.

        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        bool OnPlayerInteractDoor(Player ply, DoorVariant door, bool canOpen)
        {
            if (!Config.IsEnabled || !Config.AffectDoors || ply.IsSCP() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                Config.BlacklistedDoors.Any(d => door.name.StartsWith(d)) || ply.CurrentItem is KeycardItem) return true;
            
            if (door.HasKeycardPermission(ply))
            {
                canOpen = true;
            }
            
            return true;
        }
        
        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        bool OnPlayerInteractLocker(Player ply, Locker locker, LockerChamber lockerChamber, bool canAccess)
        {
            if (!Config.IsEnabled || !Config.AffectLockers || ply.IsSCP() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                Config.BlacklistedLockers.Any(l => locker.name.StartsWith(l)) || ply.CurrentItem is KeycardItem) return true;
            
            if (lockerChamber.HasKeycardPermission(ply))
            {
                canAccess = true;
            }

            return true;
        }

        [PluginEvent(ServerEventType.PlayerInteractGenerator)]
        bool OnPlayerInteractGenerator(Player ply, Scp079Generator generator, Scp079Generator.GeneratorColliderId generatorColliderId)
        {
            if (!Config.IsEnabled || !Config.AffectGenerators || ply.IsSCP() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                ply.CurrentItem is KeycardItem || generatorColliderId != Scp079Generator.GeneratorColliderId.Door) return true;

            if (generator.HasKeycardPermission(ply))
            {
                if (!generator.IsUnlocked())
                {
                    generator.Unlock();
                    generator.ServerGrantTicketsConditionally(new Footprint(ply.ReferenceHub), 0.5f);
                    generator._cooldownStopwatch.Restart();
                    return false;
                }
            }
            
            return true;
        }
    }
}