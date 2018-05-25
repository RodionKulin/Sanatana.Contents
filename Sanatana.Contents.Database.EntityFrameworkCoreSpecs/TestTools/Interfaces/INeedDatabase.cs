using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs.TestTools.Interfaces
{
    public interface INeedDatabase : ISpecs
    {
        ContentsDbContext ContentsDb { get; set; }
    }
}
