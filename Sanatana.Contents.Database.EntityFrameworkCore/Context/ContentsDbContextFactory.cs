using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Database.EntityFrameworkCore.Context
{
    public class ContentsDbContextFactory : IContentsDbContextFactory
    {
        //fields
        protected SqlConnectionSettings _connectionSettings;


        //init
        public ContentsDbContextFactory(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        //methods
        public virtual ContentsDbContext GetDbContext()
        {
            return new ContentsDbContext(_connectionSettings);
        }

    }
}
