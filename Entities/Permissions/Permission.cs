using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public enum PermissionResource
    { 
        Any,
        User,
        Event,
        Request,
        Location,
        Meeting,
        Schedule
    }

    public enum PermissionAction
    {
        Any,
        Create,
        View,
        Edit,
        Delete
    }

    public enum PermissionRelationship
    {
        Any,
        Owner,
        Member
    }

    public enum PermissionAttr
    {
        Any,
        Description,
        Approved
        // Add more as needed
    }

    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PermissionResource Resource { get; set; }
        public PermissionAction Action { get; set; }
        public PermissionRelationship Relationship { get; set; }
        public PermissionAttr Attr { get; set; }
    }
}
