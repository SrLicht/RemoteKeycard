using System.Linq;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using MapGeneration.Distributors;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Interfaces;

namespace RemoteKeycard
{
    public static class Extension
    {
        /// <summary>
        /// Checks whether the player has a keycard of a specific permission.
        /// </summary>
        /// <returns>true if player has requiered permission</returns>
        public static bool HasKeycardPermission(this DoorVariant door, Player player)
        {
            if (Plugin.Singleton.Config.AffectAmnesia &&
                player.EffectsManager.TryGetEffect<CustomPlayerEffects.AmnesiaItems>(out var effect) &&
                effect.IsEnabled)
            {
                return false;
            }

            foreach (var keycard in player.Items.Where(t => t is KeycardItem))
            {
                if (door.RequiredPermissions.CheckPermissions(keycard, player.ReferenceHub))
                {
                    return true;
                }
            }
            
            return false;

        }

        /// <summary>
        /// Checks whether the player has a keycard of a specific permission.
        /// </summary>
        /// <returns>true if player has requiered permission</returns>
        public static bool HasKeycardPermission(this LockerChamber chamber, Player player)
        {
            if (Plugin.Singleton.Config.AffectAmnesia &&
                player.EffectsManager.TryGetEffect<CustomPlayerEffects.AmnesiaItems>(out var effect) &&
                effect.IsEnabled)
            {
                return false;
            }
            
            foreach (var keycard in player.Items.Where(t => t is KeycardItem))
            {
                if (((KeycardItem)keycard).Permissions.HasFlagFast(chamber.RequiredPermissions))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the player has a keycard of a specific permission.
        /// </summary>
        /// <returns>true if player has requiered permission</returns>
        public static bool HasKeycardPermission(this Scp079Generator generator, Player player)
        {
            if (Plugin.Singleton.Config.AffectAmnesia &&
                player.EffectsManager.TryGetEffect<CustomPlayerEffects.AmnesiaItems>(out var effect) &&
                effect.IsEnabled)
            {
                return false;
            }
            
            foreach (var keycard in player.Items.Where(t => t is KeycardItem))
            {
                if (((KeycardItem)keycard).Permissions.HasFlagFast(generator._requiredPermission))
                {
                    return true;
                }
            }

            return false;
        }

        #region Toggle Door/Locker

        public static void Toggle(this DoorVariant door)
        {
            door.NetworkTargetState = !door.NetworkTargetState;
        }

        public static void Toggle(this LockerChamber chamber, Locker locker)
        {
            chamber.SetDoor(!chamber.IsOpen, locker._grantedBeep);
            locker.RefreshOpenedSyncvar();
        }
        

        #endregion

        /// <summary>
        /// Check if the player has no items in is inventory.
        /// </summary>
        /// <returns>true if player no have items</returns>
        public static bool IsWithoutItems(this Player ply) =>
            ply.ReferenceHub.inventory.UserInventory.Items.Count == 0;

        /// <summary>
        /// Get if this <see cref="Scp079Generator"/> is unlocked
        /// </summary>
        public static bool IsUnlocked(this Scp079Generator gen)
        {
            return gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Unlocked);
        }

        /// <summary>
        /// Unlock this <see cref="Scp079Generator"/>.
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="value"></param>
        public static void Unlock(this Scp079Generator gen)
        {
            gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
        }
        
        /// <summary>
        /// Get if player is Scp
        /// </summary>
        public static bool IsSCP(this Player ply) => ply.Role.GetTeam() is Team.SCPs;
    }
}