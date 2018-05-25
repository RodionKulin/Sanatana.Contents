using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs.SampleEntities
{
    public class TicketComment : Comment<long>
    {
        public Guid? CommitId { get; set; }
    }
}
