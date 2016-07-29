using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public enum ULoginError
    {
        InvalidHost,
        TokenExpired,
        InvalidToken,
        NoConnection,
        Unknown
    }
}