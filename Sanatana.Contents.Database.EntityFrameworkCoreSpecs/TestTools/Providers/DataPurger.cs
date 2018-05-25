using Sanatana.Contents.Database.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using SpecsFor;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sanatana.Contents.Database.EntityFrameworkCore.Context;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs.TestTools.Providers
{
    public class DataPurger : Behavior<INeedDatabase>
    {
        public override void SpecInit(INeedDatabase instance)
        {
            ContentsDbContext context = instance.ContentsDb;

            //Disable all foreign keys.
            context.Database
                .ExecuteSqlCommand("EXEC sp_msforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"");

            //Remove all data from tables EXCEPT for the EF Migration History table!
            context.Database
                .ExecuteSqlCommand("EXEC sp_msforeachtable \"SET QUOTED_IDENTIFIER ON; IF '?' != '[dbo].[__MigrationHistory]' DELETE FROM ?\"");

            //Turn FKs back on
            context.Database
                .ExecuteSqlCommand("EXEC sp_msforeachtable \"ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all\"");

        }
    }
}
