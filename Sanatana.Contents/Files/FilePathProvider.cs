using Sanatana.Contents.Files.Resizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sanatana.Contents.Files
{
    public class FilePathProvider
    {
        protected string _nameFormat;


        //properties
        /// <summary>
        /// Identifier used to match FilePathProvider to uploaded file.
        /// </summary>
        public int FilePathProviderId { get; set; }
        /// <summary>
        /// Url domain and optional starting path.
        /// </summary>
        public string BaseUrl { get; set; }
        /// <summary>
        /// Relative path format used to construct file path.
        /// Used in both file storage and file url to serve it. 
        /// Default format is {0} that will just return argument provided.
        /// </summary>
        public string RelativePathFormat { get; set; }
        /// <summary>
        /// Name format used to construct file name. 
        /// Default format is {0} that will just return argument provided.
        /// </summary>
        public string NameFormat
        {
            get { return _nameFormat; }
            set
            {
                int formattingArgsCount = CountFormatArguments(value);
                if(formattingArgsCount > 1)
                {
                    throw new NotSupportedException($"Number of formatting arguments in {nameof(NameFormat)} should not be greater than 1. Provided value holds {value} format.");
                }
                _nameFormat = value;
            }
        }
        /// <summary>
        /// Optional age after which file will be removed by RemoveTempFilesJob.
        /// </summary>
        public TimeSpan? RemoveFilesAfterAge { get; set; }



        //init
        public FilePathProvider()
        {
            RelativePathFormat = "{0}";
            NameFormat = "{0}";
        }

        public FilePathProvider(int filePathProviderId, string baseUrl, ImageFormat extension)
        {
            FilePathProviderId = filePathProviderId;
            BaseUrl = baseUrl;
            RelativePathFormat = "{0}";
            NameFormat = "{0}." + extension.ToString().ToLower();
        }
  
        

        //common
        protected virtual string CombineFormatStrings(string format1, string format2)
        {
            int input1Arguments = CountFormatArguments(format1);
            int input2Arguments = CountFormatArguments(format2);

            List<string> updatedArguments = new List<string>();
            for (int i = 0; i < input2Arguments; i++)
            {
                updatedArguments.Add("{" + (input1Arguments + i) + "}");
            }

            format2 = string.Format(format2, updatedArguments.ToArray());

            return format1 + "/" + format2;
        }

        protected virtual int CountFormatArguments(string format)
        {
            if (string.IsNullOrEmpty(format))
                return 0;

            string pattern = @"{(.*?)}";
            MatchCollection matches = Regex.Matches(format, pattern);
            return matches.OfType<Match>().Select(m => m.Value).Distinct().Count();
        }



        //relative path
        /// <summary>
        /// Constuct name form NameFormat and provided arguments.
        /// </summary>
        /// <param name="nameFormatArgs"></param>
        /// <returns></returns>
        public virtual string GetName(params string[] nameFormatArgs)
        {
            return string.Format(NameFormat, nameFormatArgs).Trim('/');
        }

        /// <summary>
        /// Constuct relative path replacing all RelativePathFormat arguments with empty string.
        /// </summary>
        /// <returns></returns>
        public virtual string GetRootPath()
        {
            int argumentsCount = CountFormatArguments(RelativePathFormat);
            string[] arguments = Enumerable.Range(0, argumentsCount)
                .Select(x => string.Empty)
                .ToArray();

            string rootPath = string.Format(RelativePathFormat, arguments);
            return rootPath.Replace('\\', '/').Trim('/') + "/";
        }

        /// <summary>
        /// Constuct relative path form RelativePathFormat and provided arguments.
        /// </summary>
        /// <param name="directoryFormatArgs"></param>
        /// <returns></returns>
        public virtual string GetPath(params string[] directoryFormatArgs)
        {
            string relativeDirectoryPath = string.Format(RelativePathFormat, directoryFormatArgs);
            return relativeDirectoryPath.Replace('\\', '/').Trim('/') + "/";
        }

        /// <summary>
        /// Constuct relative path and name form RelativePathFormat, NameFormat combined to single format string and provided arguments.
        /// </summary>
        /// <param name="directoryAndNameFormatArgs"></param>
        /// <returns></returns>
        public virtual string GetPathAndName(params string[] directoryAndNameFormatArgs)
        {
            string relativePathFormat = RelativePathFormat.Replace('\\', '/').Trim('/');
            string nameFormat = NameFormat.Replace('\\', '/').Trim('/');

            string namePathFormat = CombineFormatStrings(relativePathFormat, nameFormat);
            string namePath = string.Format(namePathFormat, directoryAndNameFormatArgs);
            return namePath.Replace('\\', '/').Trim('/').Replace("//", "/");
        }



        //url
        /// <summary>
        /// Get base url and replacing '\' symbol with '/'
        /// </summary>
        /// <returns></returns>
        public virtual string GetBaseUrl()
        {
            return BaseUrl.Replace('\\', '/').TrimEnd('/');
        }

        /// <summary>
        /// Get base url and path shared with by files created by this FilePathProvider
        /// </summary>
        /// <returns></returns>
        public virtual string GetRootPathUrl()
        {
            string urlBase = GetBaseUrl();
            string rootPath = GetRootPath();
            return string.Format("{0}/{1}/", urlBase, rootPath);
        }

        /// <summary>
        /// Create full url including base url, relative path and file name from provided arguments
        /// </summary>
        /// <param name="directoryAndNameFormatArgs">Arguments supplied to RelativeDirectoryFormat combined with NameFormat.</param>
        /// <returns></returns>
        public virtual string GetFullUrl(params string[] directoryAndNameFormatArgs)
        {
            string urlBase = GetBaseUrl();
            string namePath = GetPathAndName(directoryAndNameFormatArgs);
            return string.Format("{0}/{1}", urlBase, namePath);
        }

        /// <summary>
        /// Remove BaseUrl from full url
        /// </summary>
        /// <param name="fullUrl"></param>
        /// <returns></returns>
        public virtual string TrimToRelativeUrl(string fullUrl)
        {
            string urlBase = GetBaseUrl();
            return fullUrl.Replace(urlBase, string.Empty).Trim('/');
        }
    }
}
