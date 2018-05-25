using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects
{
    public class SampleEntity
    {
        public long Version { get; set; }
        public int IntNotNullProperty { get; set; }
        public int? IntProperty { get; set; }
        public string StringProperty { get; set; }
        public DateTime? DateProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public Guid? GuidNullableProperty { get; set; }
        public long ContentId { get; set; }
    }
}
