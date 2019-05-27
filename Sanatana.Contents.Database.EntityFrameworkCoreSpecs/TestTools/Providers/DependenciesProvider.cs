using Microsoft.EntityFrameworkCore;
using Sanatana.Contents.Database.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore;
using Sanatana.Contents.Database.EntityFrameworkCore.Queries;
using Sanatana.Contents.Database.EntityFrameworkCoreSpecs.SampleEntities;
using Sanatana.Contents.Database.EntityFrameworkCoreSpecs.SampleEntities.DbContext;
using Sanatana.Contents.Objects.Entities;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs.TestTools.Providers
{
    public class DependenciesProvider : Behavior<INeedDatabase>
    {
        //methods
        public override void SpecInit(INeedDatabase instance)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ContentsDbContext"].ConnectionString;
            var connection = new SqlConnectionSettings
            {
                ConnectionString = connectionString,
                Schema = "dbo"
            };
            instance.ContentsDb = new TicketDbContext(connection);

            instance.MockContainer.Configure(
                cfg =>
                {
                    cfg.For<SqlConnectionSettings>().Use(connection);
                    cfg.For<IContentsDbContextFactory>().Use<TicketDbContextFactory>();
                    cfg.For<SqlContentQueries<Content<long>>>().Use<SqlContentQueries<Content<long>>>();
                    cfg.For<SqlContentQueries<Ticket>>().Use<SqlContentQueries<Ticket>>();
                });
        }

        public override void AfterSpec(INeedDatabase instance)
        {
            instance.ContentsDb.Dispose();
        }
    }
}
