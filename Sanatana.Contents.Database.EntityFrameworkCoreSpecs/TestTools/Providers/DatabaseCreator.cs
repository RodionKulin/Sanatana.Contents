using Microsoft.EntityFrameworkCore;
using Moq;
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

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs.TestTools.Providers
{
    public class DatabaseCreator : Behavior<INeedDatabase>
    {
        //fields
        private static bool _isInitialized;


        //methods
        public override void SpecInit(INeedDatabase instance)
        {
            if (_isInitialized)
            {
                return;
            }
            
            instance.ContentsDb.Database.EnsureCreated();

            _isInitialized = true;
        }
    }
}
