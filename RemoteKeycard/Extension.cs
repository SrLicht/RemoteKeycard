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
        /// <param name="player"><see cref="IPlayer"/> trying to interact.</param>
        /// <param name="permissions">The permission that's gonna be searched for.</param>
        /// <param name="requiresAllPermissions">Whether all permissions are required. Use this for chambers</param>
        /// <returns>Whether the player has the required keycard.</returns>
        public static bool HasKeycardPermission(this Player player, KeycardPermissions permissions, bool requiresAllPermissions = false)
        {
            try
            {
                if (player.EffectsManager.TryGetEffect<CustomPlayerEffects.AmnesiaItems>(out var effect) && effect.IsEnabled)
                {
                    return false;
                }

                bool returnValues = false;

                foreach (var item in player.ReferenceHub.inventory.UserInventory.Items.Values)
                {
                    if (item is not KeycardItem) continue;

                    var keycard = item as KeycardItem;

                    returnValues = requiresAllPermissions ? keycard.Permissions.HasFlag(permissions) : (keycard.Permissions & permissions) != 0;

                    if (returnValues) return true;
                }

                return false;
            }
            catch (System.Exception e)
            {
                Log.Error($"Error on HasKeycardPermission: {e.Message} ---- {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Open or Close a <see cref="DoorVariant"/>
        /// </summary>
        /// <param name="door"> It's a door, what else do you expect?</param>
        public static void Toggle(this DoorVariant door)
        {
            door.NetworkTargetState = !door.NetworkTargetState;
        }

        /// <summary>
        /// Open or close this <see cref="LockerChamber"/>
        /// </summary>
        public static void Toggle(this LockerChamber chamber, Locker locker)
        {
            // Northwood please put RefreshOpenedSyncvar in LockerChamber >:(
            
            chamber.SetDoor(!chamber.IsOpen, locker._grantedBeep); 
            locker.RefreshOpenedSyncvar();
        }
        
        /// <summary>
        /// Check if a Itemtype is SCP Item
        /// </summary>
        /// <returns>true if ItemType is SCP</returns>
        public static bool IsSCP(this ItemType type) => type is ItemType.SCP018 or ItemType.SCP500 or ItemType.SCP268 or ItemType.SCP207 or ItemType.SCP244a or ItemType.SCP244b or ItemType.SCP2176 or ItemType.SCP1853;

        /// <summary>
        /// Check if the player has no items in is inventory.
        /// </summary>
        /// <returns>true if player no have items</returns>
        public static bool IsWithoutItems(this Player ply) =>
            ply.ReferenceHub.inventory.UserInventory.Items.Count == 0;
        
        /// <summary>
        /// Get if this <see cref="Scp079Generator"/> is open
        /// </summary>
        /// <param name="gen"></param>
        /// <returns></returns>
        public static bool IsOpen(this Scp079Generator gen)
        {
            return gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Open);
        }

        /// <summary>
        /// Get if this <see cref="Scp079Generator"/> is unlocked
        /// </summary>
        public static bool IsUnlocked(this Scp079Generator gen)
        {
            return gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Unlocked);
        }
        
        /// <summary>
        /// Open this <see cref="Scp079Generator"/>
        /// </summary>
        public static void Open(this Scp079Generator gen)
        {
            gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Open,  true);
        }

        /// <summary>
        /// Close this <see cref="Scp079Generator"/>
        /// </summary>
        public static void Close(this Scp079Generator gen)
        {
            gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, false);
        }

        /* // For some strange reason using this method does not work, but using generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, !generator.HasFlag(generator._flags, Scp079Generator.GeneratorFlags.Open)); inside the event will...
        /// <summary>
        /// Open or close this <see cref="Scp079Generator"/>
        /// </summary>
        public static void Toggle(this Scp079Generator generator)
        {
            generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, !generator.HasFlag(generator._flags, Scp079Generator.GeneratorFlags.Open));
        }*/

        /// <summary>
        /// Unlock or Lock this <see cref="Scp079Generator"/>.
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="value"></param>
        public static void Unlock(this Scp079Generator gen)
        {
            gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
        }

        public static void ToggleLock(this Scp079Generator gen)
        {
            gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, !gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Unlocked));
        }

        public static bool IsScp(this Player ply) => ply.Role.GetTeam() is Team.SCPs;
    }
}