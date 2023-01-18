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
        public static Plugin Singleton;
        
        [PluginConfig] public Config Config;
        
        public const string Version = "1.1.7";
        
        [PluginPriority(LoadPriority.Highest)]
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
            
            Log.Debug($"Player {ply.Nickname} ({ply.UserId}) try to open a door with RemoteKeycard", Config.IsDebug);
            
            if (door.HasKeycardPermission(ply))
            {
                if (Config.TraditionalMethods)
                {
                    canOpen = true;
                    Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has permission to open a door | Running traditional method", Config.IsDebug);
                    door.Toggle(ply.ReferenceHub);
                    Log.Debug($"Door should have opened | traditional method", Config.IsDebug);
                    return false;
                }
                Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has permission to open a door", Config.IsDebug);
                canOpen = true;
                Log.Debug($"Door should have opened.", Config.IsDebug);
            }
            
            return true;
        }
        
        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        bool OnPlayerInteractLocker(Player ply, Locker locker, LockerChamber lockerChamber, bool canAccess)
        {
            if (!Config.IsEnabled || !Config.AffectLockers || ply.IsSCP() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                Config.BlacklistedLockers.Any(l => locker.name.StartsWith(l)) || ply.CurrentItem is KeycardItem) return true;
            
            Log.Debug($"Player {ply.Nickname} ({ply.UserId}) try to open a lockerchamber with RemoteKeycard", Config.IsDebug);
            if (lockerChamber.HasKeycardPermission(ply))
            {
                if (Config.TraditionalMethods)
                {
                    canAccess = true;
                    Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has permission to open a lockerchamber | Running traditional method", Config.IsDebug);
                    lockerChamber.Toggle(locker);
                    Log.Debug($"LockerChamber should have opened | traditional method", Config.IsDebug);
                    return false;
                }
                
                Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has permission to open a lockerchamber", Config.IsDebug);
                canAccess = true;
                
                Log.Debug($"LockerChamber should have opened.", Config.IsDebug);
            }

            return true;
        }

        [PluginEvent(ServerEventType.PlayerInteractGenerator)]
        bool OnPlayerInteractGenerator(Player ply, Scp079Generator generator, Scp079Generator.GeneratorColliderId generatorColliderId)
        {
            if (!Config.IsEnabled || !Config.AffectGenerators || ply.IsSCP() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                ply.CurrentItem is KeycardItem || generatorColliderId != Scp079Generator.GeneratorColliderId.Door) return true;

            Log.Debug($"Player {ply.Nickname} ({ply.UserId}) try to open a generator with RemoteKeycard", Config.IsDebug);
            if (generator.HasKeycardPermission(ply))
            {
                Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has permission to unlock a generator", Config.IsDebug);
                if (!generator.IsUnlocked())
                {
                    
                    generator.Unlock();
                    generator.ServerGrantTicketsConditionally(new Footprint(ply.ReferenceHub), 0.5f);
                    generator._cooldownStopwatch.Restart();
                    Log.Debug($"Player {ply.Nickname} ({ply.UserId}) has unlocked a generator.", Config.IsDebug);
                    return false;
                }
            }
            
            return true;
        }
    }
}