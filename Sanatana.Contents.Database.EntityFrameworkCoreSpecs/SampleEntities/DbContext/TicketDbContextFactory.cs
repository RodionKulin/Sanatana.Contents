using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs.SampleEntities.DbContext
{
    public class TicketDbContextFactory : ContentsDbContextFactory
    {
        //init
        public TicketDbContextFactory(SqlConnectionSettings connectionSettings)
            : base(connectionSettings)
        {
        }


        //methods
        public override ContentsDbContext GetDbContext()
        {
            return new TicketDbContext(_connectionSettings);
        }

    }
}
