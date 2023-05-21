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
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace RemoteKeycard
{
    
    public class Plugin
    {
        /// <summary>
        /// Plugin instance.
        /// </summary>
        public static Plugin Singleton;
        
        [PluginConfig] public Config Config;

        /// <summary>
        /// Plugin Version.
        /// </summary>
        private const string Version = "1.1.9";
        
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
            // Check if the door have any type of lock (Scp079/Warhead/RemoteAdmin)
            if (door.ActiveLocks > 0 && !ply.IsBypassEnabled) return false;
            
            if (!Config.IsEnabled || !Config.AffectDoors || ply.IsSCP() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                Config.BlacklistedDoors.Any(d => door.name.StartsWith(d)) || ply.CurrentItem is KeycardItem) return true;

            //fix for servers with exiled and NWapi causing the event to be called even when the interaction should of been blocked
            if (!door.AllowInteracting(ply.ReferenceHub, 0)) return false;

            Log.Debug($"Player {ply.Nickname} ({ply.UserId}) try to open a door with RemoteKeycard", Config.IsDebug);
            // Check if the player have a keycard to open the door.
            if (door.HasKeycardPermission(ply))
            { 
                // Unnecessary but I do it anyway
                canOpen = true;
                Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has permission to open a door", Config.IsDebug);
                // Opens or closes the door that the player is interacting with.
                door.Toggle(ply.ReferenceHub);
                return false;
            }
            
            return true;
        }
        
        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        bool OnPlayerInteractLocker(Player ply, Locker locker, LockerChamber lockerChamber, bool canAccess)
        {
            if (!Config.IsEnabled || !Config.AffectLockers || ply.IsSCP() ||  Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                Config.BlacklistedLockers.Any(l => locker.name.StartsWith(l)) || ply.CurrentItem is KeycardItem) return true;
            
            Log.Debug($"Player {ply.Nickname} ({ply.UserId}) try to open a locker chamber with RemoteKeycard", Config.IsDebug);
            // Check if the player have permission to open the locker chamber.
            if (lockerChamber.HasKeycardPermission(ply))
            {
                // Unnecessary but I do it anyway
                canAccess = true;
                Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has permission to open a locker chamber", Config.IsDebug);
                lockerChamber.Toggle(locker);
                return false;
            }

            return true;
        }

        [PluginEvent(ServerEventType.PlayerInteractGenerator)]
        bool OnPlayerInteractGenerator(Player ply, Scp079Generator generator, Scp079Generator.GeneratorColliderId generatorColliderId)
        {
            if (!Config.IsEnabled || !Config.AffectGenerators || ply.IsSCP() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                ply.CurrentItem is KeycardItem || generatorColliderId != Scp079Generator.GeneratorColliderId.Door) return true;

            Log.Debug($"Player {ply.Nickname} ({ply.UserId}) try to open a generator with RemoteKeycard", Config.IsDebug);
            // Check if the player have permission to open the generator.
            if (generator.HasKeycardPermission(ply))
            {
                Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has permission to unlock a generator", Config.IsDebug);
                // If the generator is Locked
                if (!generator.IsUnlocked())
                {
                    generator.Unlock();
                    // Grant tickets to the ply team.
                    generator.ServerGrantTicketsConditionally(new Footprint(ply.ReferenceHub), 0.5f);
                    // Just in case
                    generator._cooldownStopwatch.Restart();
                    Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has unlocked a generator.", Config.IsDebug);
                    return false;
                }
            }
            
            return true;
        }
    }
}
