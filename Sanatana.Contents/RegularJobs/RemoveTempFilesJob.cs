using Sanatana.Contents;
using Sanatana.Contents.Files;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Files.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.RegularJobs
{
    public class RemoveTempFilesJob : IContentRegularJob
    {
        //fields
        protected IFileService _fileService;
        protected IEnumerable<FilePathProvider> _filePathProviders;


        //init
        public RemoveTempFilesJob(IFileService fileService, IEnumerable<FilePathProvider> filePathProviders)
        {
            _fileService = fileService;
            _filePathProviders = filePathProviders;
        }


        //methods
        public virtual void Execute()
        {
            foreach (FilePathProvider filePathMapper in _filePathProviders)
            {
                if(filePathMapper.RemoveFilesAfterAge == null)
                {
                    continue;
                }
                
                _fileService
                    .Clean(filePathMapper.FilePathProviderId, filePathMapper.RemoveFilesAfterAge.Value)
                    .Wait();
            }
        }

    }
}
