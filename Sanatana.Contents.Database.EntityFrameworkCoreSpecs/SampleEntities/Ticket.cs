using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs.SampleEntities
{
    public class Ticket : Content<long>
    {
        public long IssueId { get; set; }
        public long UserAssignedTo { get; set; }
    }
}
