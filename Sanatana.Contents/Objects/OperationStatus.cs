using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Objects
{
    public enum OperationStatus
    {
        Success,
        VersionChanged,
        NotFound,
        PermissionDenied,
        Error
    }
}
