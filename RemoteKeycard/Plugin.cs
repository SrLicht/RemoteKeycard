using System;
using System.Linq;
using Footprinting;
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
        [PluginConfig] public Config Config;

        [PluginEntryPoint("RemoteKeycard", "1.1.2",
            "Allow player to open doors, lockers and generators without a Keycard in hand", "SrLicht")]
        void LoadPlugin()
        {
            PluginAPI.Events.EventManager.RegisterEvents(this);
        }

        // These damn events need a lot of checks to keep bad things from happening so read it at your own risk.

        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        bool OnPlayerInteractDoor(Player ply, DoorVariant door, bool canOpen)
        {
            if (!Config.IsEnabled || !Config.AffectDoors || ply.IsScp() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                Config.BlacklistedDoors.Any(d => door.name.StartsWith(d)) || ply.CurrentItem is KeycardItem) return true;

            if (ply.HasKeycardPermission(door.RequiredPermissions.RequiredPermissions))
            {
                canOpen = true;
                door.Toggle();
                return false;
            }

            return true;
        }

        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        bool OnPlayerInteractLocker(Player ply, Locker locker, LockerChamber lockerChamber, bool canAccess)
        {
            if (!Config.IsEnabled || !Config.AffectLockers || ply.IsScp() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                Config.BlacklistedLockers.Any(l => locker.name.StartsWith(l)) || ply.CurrentItem is KeycardItem) return true;

            if (ply.HasKeycardPermission(lockerChamber.RequiredPermissions, true))
            {
                canAccess = true;
                lockerChamber.Toggle(locker);
                return false;
            }

            return true;
        }

        [PluginEvent(ServerEventType.PlayerInteractGenerator)]
        bool OnPlayerInteractGenerator(Player ply, Scp079Generator generator,
            Scp079Generator.GeneratorColliderId generatorColliderId)
        {
            if (!Config.IsEnabled || !Config.AffectGenerators || ply.IsScp() || Config.BlackListRole.Contains(ply.Role) || ply.IsWithoutItems() ||
                ply.CurrentItem is KeycardItem || generatorColliderId != Scp079Generator.GeneratorColliderId.Door) return true;

            if (ply.HasKeycardPermission(generator._requiredPermission))
            {
                if (generator.IsUnlocked())
                {
                    generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, !generator.HasFlag(generator._flags, Scp079Generator.GeneratorFlags.Open));
                    generator._targetCooldown = generator._doorToggleCooldownTime;
                    generator._cooldownStopwatch.Restart();
                    return false;
                }
                else
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