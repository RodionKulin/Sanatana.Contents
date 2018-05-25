using Microsoft.EntityFrameworkCore;
using Sanatana.Contents.Database.EntityFrameworkCore.FluentMapping;
using Sanatana.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Database.EntityFrameworkCore.Context
{
    public class ContentsDbContext : DbContext
    {
        //fields
        protected SqlConnectionSettings _connectionSettings;



        //init
        public ContentsDbContext(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
               .UseSqlServer(_connectionSettings.ConnectionString, options => options.CommandTimeout(30));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CategoryEntityConfiguration(_connectionSettings));
            modelBuilder.ApplyConfiguration(new CategoryRolePermissionEntityConfiguration(_connectionSettings));
            modelBuilder.ApplyConfiguration(new CommentEntityConfiguration(_connectionSettings));
            modelBuilder.ApplyConfiguration(new ContentEntityConfiguration(_connectionSettings));

            base.OnModelCreating(modelBuilder);
        }
    }
}
