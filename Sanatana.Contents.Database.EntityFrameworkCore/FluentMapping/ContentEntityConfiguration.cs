using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.Contents.Objects.Entities;
using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Database.EntityFrameworkCore.FluentMapping
{
    public class ContentEntityConfiguration : IEntityTypeConfiguration<Content<long>>
    {
        protected SqlConnectionSettings _connectionSettings;

        public ContentEntityConfiguration(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public void Configure(EntityTypeBuilder<Content<long>> builder)
        {
            // Keys
            builder.HasKey(t => t.ContentId);
            builder.HasIndex(t => new { t.PublishedTimeUtc, t.State, t.CategoryId }).IsUnique(false);
            builder.HasIndex(t => t.Url).IsUnique(true);

            // Columns
            builder.Property(x => x.CreatedTimeUtc).HasColumnType("datetime2");
            builder.Property(x => x.PublishedTimeUtc).HasColumnType("datetime2");

            // Table Mappings
            builder.ToTable(DefaultTableNameConstants.CONTENT_DEFAULT_NAME, _connectionSettings.Schema);
        }
    }
}
