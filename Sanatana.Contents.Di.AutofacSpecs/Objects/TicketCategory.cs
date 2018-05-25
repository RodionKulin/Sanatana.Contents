using MongoDB.Bson;
using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Di.AutofacSpecs.Objects
{
    public class TicketCategory : Category<ObjectId>
    {
        public string Description { get; set; }
    }
}
