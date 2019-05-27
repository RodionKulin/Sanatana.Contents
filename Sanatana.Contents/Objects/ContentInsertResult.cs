using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanatana.Contents.Objects
{
    public enum ContentInsertResult
    {
        Success,
        UrlIsNotUnique,
        PublishTimeUtcIsNotUnique
    }
}
