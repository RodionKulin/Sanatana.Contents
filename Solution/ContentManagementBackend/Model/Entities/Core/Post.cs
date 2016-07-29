using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ContentManagementBackend;
using System.ComponentModel.DataAnnotations;
using ContentManagementBackend.Resources;

namespace ContentManagementBackend
{
    public class Post<TKey> : ContentBase<TKey>
        where TKey : struct
    {
    }
}
