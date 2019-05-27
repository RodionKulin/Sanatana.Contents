using Microsoft.EntityFrameworkCore;
using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using Sanatana.Contents.Database.EntityFrameworkCore.FluentMapping;
using Sanatana.Contents.Objects.Entities;
using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs.SampleEntities.DbContext
{
    public class TicketDbContext : ContentsDbContext
    {
        //init
        public TicketDbContext(SqlConnectionSettings connectionSettings)
            : base(connectionSettings)
        {
        }


        //methods
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CategoryEntityConfiguration(_connectionSettings));
            modelBuilder.ApplyConfiguration(new CategoryRolePermissionEntityConfiguration(_connectionSettings));
           
            modelBuilder.Ignore<Comment<long>>();
            modelBuilder.Ignore<Content<long>>();

            modelBuilder.ApplyConfiguration(new TicketCommentEntityConfiguration(_connectionSettings));
            modelBuilder.ApplyConfiguration(new TicketEntityConfiguration(_connectionSettings));

        }
    }
}
