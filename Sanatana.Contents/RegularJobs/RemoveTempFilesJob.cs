using Sanatana.Contents;
using Sanatana.Contents.Files;
using Sanatana.Contents.Files.Queries;
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
        protected IFileQueries _fileQueries;
        protected IEnumerable<FilePathProvider> _filePathProviders;


        //init
        public RemoveTempFilesJob(IFileQueries fileQueries, IEnumerable<FilePathProvider> filePathProviders)
        {
            _fileQueries = fileQueries;
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
                
                _fileQueries
                    .Clean(filePathMapper.FilePathProviderId, filePathMapper.RemoveFilesAfterAge.Value)
                    .Wait();
            }
        }

    }
}
