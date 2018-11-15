using Sanatana.Contents.Files;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sanatana.Contents.Extensions
{
    public static class FilePathProviderExtensions
    {
        public static Dictionary<int, FilePathProvider> ToDictionaryOrThrow(
            this IEnumerable<FilePathProvider> filePathProviders)
        {
            int totalCount = filePathProviders.Count();
            int distinctCount = filePathProviders
                .Select(x => x.FilePathProviderId)
                .Distinct()
                .Count();
            

            if(distinctCount < totalCount)
            {
                throw new MissingMemberException($"All registered {nameof(FilePathProvider)} must have unique {nameof(FilePathProvider.FilePathProviderId)}.");
            }

            return filePathProviders.ToDictionary(x => x.FilePathProviderId);
        }
    }
}
