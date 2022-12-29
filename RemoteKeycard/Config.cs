using System.Collections.Generic;
using System.ComponentModel;
using PlayerRoles;

namespace RemoteKeycard
{
    public class Config
    {
        public bool IsEnabled { get; set; } = true;

        [Description("RolesType that are in this list cannot use RemoteKeycard")]
        public List<RoleTypeId> BlackListRole { get; set; } = new List<RoleTypeId>()
        {
            RoleTypeId.None
        };
    }
}