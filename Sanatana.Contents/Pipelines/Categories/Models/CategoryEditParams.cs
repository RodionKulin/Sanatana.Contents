using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Pipelines.Categories
{
    public class CategoryEditParams<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        public TCategory Category { get; set; }
        public int MaxNameLength { get; set; } = ContentsConstants.DEFAULT_MAX_CATEGORY_NAME_LENGTH;
    }
}
