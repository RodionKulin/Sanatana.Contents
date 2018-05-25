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
        //properties
        public int FilePathProviderId { get; set; }
        public string UrlBase { get; set; }
        public string RelativeDirectoryFormat { get; set; }
        public string NameFormat { get; set; }        
        public TimeSpan? RemoveFilesAfterAge { get; set; }



        //init
        public FilePathProvider()
        {
        }

        public FilePathProvider(int filePathProviderId, string urlBase, ImageFormat extension)
        {
            FilePathProviderId = FilePathProviderId;
            UrlBase = urlBase;
            RelativeDirectoryFormat = "{0}";
            NameFormat = "{0}." + extension.ToString().ToLower();
        }
  
        

        //common
        public virtual string CombineFormatStrings(string format1, string format2)
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
        public virtual string GetName(params string[] nameFormatArgs)
        {
            return string.Format(NameFormat, nameFormatArgs).Trim('/');
        }

        public virtual string GetUrlBase()
        {
            return UrlBase.Replace('\\', '/').TrimEnd('/');
        }

        public virtual string GetRootDirectoryPath()
        {
            string rootDirectory = string.Format(RelativeDirectoryFormat, string.Empty);
            return rootDirectory.Replace('\\', '/').Trim('/') + "/";
        }

        public virtual string GetDirectoryPath(params string[] directoryFormatArgs)
        {
            string relativeDirectoryPath = string.Format(RelativeDirectoryFormat, directoryFormatArgs);
            return relativeDirectoryPath.Replace('\\', '/').Trim('/') + "/";
        }
        
        public virtual string GetNamePath(params string[] directoryAndNameFormatArgs)
        {
            string namePathFormat = CombineFormatStrings(RelativeDirectoryFormat, NameFormat);
            string namePath = string.Format(namePathFormat, directoryAndNameFormatArgs);
            return namePath.Replace('\\', '/').Trim('/');
        }



        //url
        public virtual string GetRootDirectoryUrl()
        {
            string urlBase = GetUrlBase();
            string rootDirectory = GetRootDirectoryPath();
            return string.Format("{0}/{1}/", urlBase, rootDirectory);
        }
        
        public virtual string GetFullUrl(params string[] directoryAndNameFormatArgs)
        {
            string urlBase = GetUrlBase();
            string namePath = GetNamePath(directoryAndNameFormatArgs);
            return string.Format("{0}/{1}", urlBase, namePath);
        }

        public virtual string TrimToRelativeUrl(string fullUrl)
        {
            string urlBase = GetUrlBase();
            return fullUrl.Replace(urlBase, string.Empty).Trim('/');
        }
    }
}
