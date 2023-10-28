using Footprinting;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using MapGeneration.Distributors;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System.Linq;

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
        private const string Version = "1.1.11";

        [PluginEntryPoint("RemoteKeycard", Version,
            "Allow player to open doors, lockers and generators without a Keycard in hand", "SrLicht")]
        void LoadPlugin()
        {
            Singleton = this;
            Log.Warning("This version of the plugin is only compatible with the \"pluginapi-beta\" branch of SCP:SL.");
            PluginAPI.Events.EventManager.RegisterEvents(this);
        }

        // These damn events need a lot of checks to keep bad things from happening so read it at your own risk.

        [PluginEvent]
        public bool OnPlayerInteractDoor(PlayerInteractDoorEvent ev)
        {
            // Check if the door have any type of lock (Scp079/Warhead/RemoteAdmin)
            if (ev.Door.ActiveLocks > 0 && !ev.Player.IsBypassEnabled) return true;

            if (!Config.IsEnabled || !Config.AffectDoors || ev.Player.IsSCP() || Config.BlackListRole.Contains(ev.Player.Role) || ev.Player.IsWithoutItems() ||
                Config.BlacklistedDoors.Any(d => ev.Door.name.StartsWith(d)) || ev.Player.CurrentItem is KeycardItem) return true;

            //fix for servers with exiled and NWapi causing the event to be called even when the interaction should of been blocked
            if (!ev.Door.AllowInteracting(ev.Player.ReferenceHub, 0)) return false;

            Log.Debug($"Player {ev.Player.Nickname} ({ev.Player.UserId}) try to open a door with RemoteKeycard", Config.IsDebug);
            // Check if the player have a keycard to open the door.
            if (ev.Door.HasKeycardPermission(ev.Player))
            {
                // Unnecessary but I do it anyway
                ev.CanOpen = true;
                Log.Debug($"Player {ev.Player.Nickname} ({ev.Player.UserId}) has permission to open a door", Config.IsDebug);
                // Opens or closes the door that the player is interacting with.
                ev.Door.Toggle(ev.Player.ReferenceHub);
                return false;
            }

            return true;
        }

        [PluginEvent]
        public bool OnPlayerInteractLocker(PlayerInteractLockerEvent ev)
        {
            if (!Config.IsEnabled || !Config.AffectLockers || ev.Player.IsSCP() || Config.BlackListRole.Contains(ev.Player.Role) || ev.Player.IsWithoutItems() ||
                Config.BlacklistedLockers.Any(l => ev.Locker.name.StartsWith(l)) || ev.Player.CurrentItem is KeycardItem) return true;

            Log.Debug($"Player {ev.Player.Nickname} ({ev.Player.UserId}) try to open a locker chamber with RemoteKeycard", Config.IsDebug);
            // Check if the player have permission to open the locker chamber.
            if (ev.Chamber.HasKeycardPermission(ev.Player))
            {
                // Unnecessary but I do it anyway
                ev.CanOpen = true;
                Log.Debug($"Player {ev.Player.Nickname} ({ev.Player.UserId}) has permission to open a locker chamber", Config.IsDebug);
                ev.Chamber.Toggle(ev.Locker);
                return false;
            }

            return true;
        }

        [PluginEvent]
        public bool OnPlayerInteractGenerator(PlayerInteractGeneratorEvent ev)
        {
            if (!Config.IsEnabled || !Config.AffectGenerators || ev.Player.IsSCP() || Config.BlackListRole.Contains(ev.Player.Role) || ev.Player.IsWithoutItems() ||
                ev.Player.CurrentItem is KeycardItem || ev.GeneratorColliderId != Scp079Generator.GeneratorColliderId.Door) return true;

            Log.Debug($"Player {ev.Player.Nickname} ({ev.Player.UserId}) try to open a generator with RemoteKeycard", Config.IsDebug);
            // Check if the player have permission to open the generator.
            if (ev.Generator.HasKeycardPermission(ev.Player))
            {
                Log.Debug($"Player {ev.Player.Nickname} ({ev.Player.UserId}) has permission to unlock a generator", Config.IsDebug);
                // If the generator is Locked
                if (!ev.Generator.IsUnlocked())
                {
                    ev.Generator.Unlock();
                    // Grant tickets to the ply team.
                    ev.Generator.ServerGrantTicketsConditionally(new Footprint(ev.Player.ReferenceHub), 0.5f);
                    // Just in case
                    ev.Generator._cooldownStopwatch.Restart();
                    Log.Debug($"Player {ev.Player.Nickname} ({ev.Player.UserId}) has unlocked a generator.", Config.IsDebug);
                    return false;
                }
            }

            return true;
        }
    }
}
