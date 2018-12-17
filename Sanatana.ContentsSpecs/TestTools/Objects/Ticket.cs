using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.ContentsSpecs.TestTools.Objects
{
    public class Ticket<TKey> : Content<TKey>
           where TKey : struct
    {
        public TKey IssueId { get; set; }
        public TKey UserAssignedTo { get; set; }
    }
}
