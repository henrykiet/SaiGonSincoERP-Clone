using System;
using System.Collections.Generic;

namespace reportSystem01.Shared
{
    public class FlatMenuPermissionDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentMenuId { get; set; }
        public bool HasAccess { get; set; }
        public bool CanInsert { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool IsExpanded { get; set; }
    }

    public class SaveFlatPermissionRequestDTO
    {
        public int UserId { get; set; }
        public List<FlatMenuPermissionDTO> Permissions { get; set; }
    }
} 