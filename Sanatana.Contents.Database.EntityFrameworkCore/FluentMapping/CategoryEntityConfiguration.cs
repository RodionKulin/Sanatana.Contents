using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.Contents.Objects.Entities;
using Sanatana.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Database.EntityFrameworkCore.FluentMapping
{
    public class CategoryEntityConfiguration : IEntityTypeConfiguration<Category<long>>
    {
        protected SqlConnectionSettings _connectionSettings;

        public CategoryEntityConfiguration(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public void Configure(EntityTypeBuilder<Category<long>> builder)
        {
            // Keys
            builder.HasKey(t => t.CategoryId);

            // Columns
            builder.Property(x => x.AddTimeUtc).HasColumnType("datetime2");

            // Table Mappings
            builder.ToTable(DefaultTableNameConstants.CATEGORY_DEFAULT_NAME, _connectionSettings.Schema);
        }
    }
}
