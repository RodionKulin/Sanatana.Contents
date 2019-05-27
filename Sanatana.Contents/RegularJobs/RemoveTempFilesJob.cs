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
using Sanatana.Contents.Extensions;

namespace Sanatana.Contents.RegularJobs
{
    public class RemoveTempFilesJob : IContentRegularJob
    {
        //fields
        protected IFileService _fileService;
        protected Dictionary<int, FilePathProvider> _filePathProviders;
        

        //init
        public RemoveTempFilesJob(IFileService fileService, IEnumerable<FilePathProvider> filePathProviders)
        {
            _fileService = fileService;
            _filePathProviders = filePathProviders.ToDictionaryOrThrow();
        }


        //methods
        public virtual void Execute()
        {
            foreach (FilePathProvider filePathProvider in _filePathProviders.Values)
            {
                if(filePathProvider.RemoveFilesAfterAge == null)
                {
                    continue;
                }
                
                _fileService.Clean(filePathProvider.FilePathProviderId, 
                    filePathProvider.RemoveFilesAfterAge.Value)
                    .Wait();
            }
        }

    }
}
