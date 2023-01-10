using System.Collections.Generic;
using System.ComponentModel;
using PlayerRoles;

namespace RemoteKeycard
{
    public class Config
    {
        public bool IsEnabled { get; set; } = true;

        [Description("RemoteKeycard works with Doors ?")]
        public bool AffectDoors { get; set; } = true;

        [Description("RemoteKeycard works with Generators ?")]
        public bool AffectGenerators { get; set; } = true;

        [Description("RemoteKeycard works with Lockers ?")]
        public bool AffectLockers { get; set; } = true;

        [Description("If a player has amnesia, should I disable RemoteKeycard for that player?")]
        public bool AffectAmnesia { get; set; } = true;

        [Description("RolesType that are in this list cannot use RemoteKeycard")]
        public List<RoleTypeId> BlackListRole { get; set; } = new ()
        {
            RoleTypeId.None
        };
        

        [Description("RemoteKeycard will not work with doors that are in this list")]
        public List<string> BlacklistedDoors { get; set; } = new ()
        {
            "HCZ",
            "LCZ",
            "EZ",
            "Prison BreakableDoor",
            "Unsecured Pryable GateDoor",
            "Pryable 173 GateDoor"
        };

        [Description("RemoteKeycard will not work with lockers that are on this list")]
        public List<string> BlacklistedLockers { get; set; } = new ()
        {
            "MiscLocker",
            "Adrenaline",
            "Medkit"
        };
    }
}