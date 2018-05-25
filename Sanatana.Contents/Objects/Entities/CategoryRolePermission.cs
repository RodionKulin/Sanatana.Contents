using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Objects.Entities
{
    public class CategoryRolePermission<TKey>
        where TKey : struct    
    {
        public TKey CategoryRolePermissionId { get; set; }
        public TKey CategoryId { get; set; }
        public string CategoryType { get; set; }
        public TKey RoleId { get; set; }
        public long PermissionFlags { get; set; }
    }
}
