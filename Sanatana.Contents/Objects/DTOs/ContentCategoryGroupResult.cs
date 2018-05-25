using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Objects.DTOs
{
    public class ContentCategoryGroupResult<TKey, TContent>
        where TKey : struct
        where TContent : Content<TKey>
    {
        public TKey CategoryId { get; set; }
        public List<TContent> Contents { get; set; }
    }
}
