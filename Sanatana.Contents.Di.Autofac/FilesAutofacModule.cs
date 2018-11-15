using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using System.IO;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Files.Resizer;
using Sanatana.Contents.Files.Downloads;
using Sanatana.Contents.Files.Services;

namespace Sanatana.Contents.Di.Autofac
{
    public class FilesAutofacModule : Module
    {
        //fields
        private FileStorageSettings _fileStorageSettings;


        //init
        public FilesAutofacModule(FileStorageSettings fileStorageSettings)
        {
            _fileStorageSettings = fileStorageSettings;
        }


        //methods
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance<FileStorageSettings>(_fileStorageSettings).AsSelf().SingleInstance();
            builder.RegisterType<FileStorage>().As<IFileStorage>().SingleInstance();

            builder.RegisterType<FileDownloader>().As<IFileDownloader>().SingleInstance();
            builder.RegisterType<ImageSharpResizer>().As<IImageResizer>().SingleInstance();
            builder.RegisterType<ImageFileService>().As<IImageFileService>().InstancePerLifetimeScope();
            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();
        }
    }
}
