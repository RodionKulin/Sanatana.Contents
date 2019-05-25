using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs.SampleEntities.DbContext
{
    public class TicketCommentEntityConfiguration : IEntityTypeConfiguration<TicketComment>
    {
        protected SqlConnectionSettings _connectionSettings;

        public TicketCommentEntityConfiguration(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public void Configure(EntityTypeBuilder<TicketComment> builder)
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
            builder.ToTable("TicketComments", _connectionSettings.Schema);
        }
    }
}
