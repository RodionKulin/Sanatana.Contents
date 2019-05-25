using Autofac;
using Sanatana.Contents.DI.Autofac;
using Sanatana.Contents.Files.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Files.AmazonS3.DI.Autofac
{
    public class AmazonS3FilesAutofacModule : Module
    {
        //methods
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AmazonS3FileStorage>()
                .Named<IFileStorage>(DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY)
                .InstancePerDependency();
        }
    }
}
