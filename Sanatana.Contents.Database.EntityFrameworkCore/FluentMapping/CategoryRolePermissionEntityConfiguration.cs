using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.Contents.Objects.Entities;
using Sanatana.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Database.EntityFrameworkCore.FluentMapping
{
    public class CategoryRolePermissionEntityConfiguration : IEntityTypeConfiguration<CategoryRolePermission<long>>
    {
        protected SqlConnectionSettings _connectionSettings;

        public CategoryRolePermissionEntityConfiguration(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public void Configure(EntityTypeBuilder<CategoryRolePermission<long>> builder)
        {
            // Keys
            builder.HasKey(t => t.CategoryRolePermissionId);

            // Columns
            builder.Property(x => x.CategoryType).IsRequired();

            // Table Mappings
            builder.ToTable(DefaultTableNameConstants.CATEGORY_ROLE_PERMISSION_DEFAULT_NAME, _connectionSettings.Schema);
        }
    }
}
