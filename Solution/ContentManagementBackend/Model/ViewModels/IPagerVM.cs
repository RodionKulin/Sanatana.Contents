using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend
{
    public interface IPagerVM
    {
        int Page { get; }
        int PageSize { get; }
        int TotalItems { get; }
        int TotalPages { get; }
    }
}