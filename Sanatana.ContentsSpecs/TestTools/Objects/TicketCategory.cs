using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.ContentsSpecs.TestTools.Objects
{
    public class TicketCategory : Category<MongoDB.Bson.ObjectId>
    {
        public string Description { get; set; }
    }
}
