using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.Contents.Objects.Entities;
using Sanatana.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Database.EntityFrameworkCore.FluentMapping
{
    public class CommentEntityConfiguration : IEntityTypeConfiguration<Comment<long>>
    {
        protected SqlConnectionSettings _connectionSettings;

        public CommentEntityConfiguration(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public void Configure(EntityTypeBuilder<Comment<long>> builder)
        {
            // Keys
            builder.HasKey(t => t.CommentId);
            builder.HasIndex(t => t.ContentId).IsUnique(false);
            builder.HasIndex(t => new { t.AuthorId, t.CreatedTimeUtc }).IsUnique(false);

            // Columns
            builder.Property(x => x.CreatedTimeUtc).HasColumnType("datetime2");
            builder.Property(x => x.UpdatedTimeUtc).HasColumnType("datetime2");
            builder.Ignore(x => x.Content);

            // Table Mappings
            builder.ToTable(DefaultTableNameConstants.COMMENT_DEFAULT_NAME, _connectionSettings.Schema);
        }
    }
}
