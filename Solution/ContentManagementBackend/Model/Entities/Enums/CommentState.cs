using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public enum CommentState
    {
        Inserted,
        Moderated,
        Deleted,
        AuthorizationRequired,
        AuthorizationRequiredModerated
    }
}
