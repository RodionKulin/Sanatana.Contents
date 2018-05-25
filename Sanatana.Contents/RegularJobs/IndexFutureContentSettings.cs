using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.Contents.RegularJobs
{
    public class IndexFutureContentSettings<TKey, TContent>
        where TKey : struct
        where TContent : Content<TKey>
    {
        public int PageSize { get; set; } = ContentsConstants.DEFAULT_INDEX_FUTURE_CONTENTS_SELECT_COUNT;
        public Expression<Func<TContent, bool>> AdditionalFilters { get; set; }
    }
}
