using Common.Utility;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class PathCreator
    { 
        //свойства
        public virtual string UrlBase { get; set; }
        public virtual string TempRootFolder { get; set; }
        public virtual string StaticRootFolder { get; set; }
        
        public virtual string TempFolderFormat { get; set; }
        public virtual string StaticFolderFormat { get; set; }

        public virtual string TempNameFormat { get; set; }
        public virtual string StaticNameFormat { get; set; }

        

        //инициализация
        public PathCreator()
        {
        }

        public PathCreator(string urlBase, ImageFormat extension, string tempFolder, string staticFolder)
        {
            UrlBase = urlBase.Replace('\\', '/');

            TempRootFolder = tempFolder;
            TempFolderFormat = null;
            TempNameFormat = "{0}{1}." + extension.ToString().ToLower();

            StaticRootFolder = staticFolder;
            StaticFolderFormat = null;
            StaticNameFormat = "{0}." + extension.ToString().ToLower();
        }
  
        

        //common
        public virtual string CombineFormatStrings(string input1, string input2)
        {
            int input1Arguments = CountFormatArguments(input1);
            int input2Arguments = CountFormatArguments(input2);

            List<string> updatedArguments = new List<string>();
            for (int i = 0; i < input2Arguments; i++)
            {
                updatedArguments.Add("{" + (input1Arguments + i) + "}");
            }

            input2 = string.Format(input2, updatedArguments.ToArray());

            return input1 + "/" + input2;
        }

        protected virtual int CountFormatArguments(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;

            string pattern = @"{(.*?)}";
            MatchCollection matches = Regex.Matches(input, pattern);
            return matches.OfType<Match>().Select(m => m.Value).Distinct().Count();
        }
                
     
        
        //folder
        public virtual string CreateTempFolderPath(params string[] folderParts)
        {
            if(TempFolderFormat == null)
            {
                return string.Format("{0}/", TempRootFolder);
            }
            else
            {
                string relativePath = string.Format(TempFolderFormat, folderParts).TrimEnd('/');
                return string.Format("{0}/{1}/", TempRootFolder, relativePath);
            }
        }

        public virtual string CreateStaticFolderPath(params string[] folderParts)
        {
            if (StaticFolderFormat == null)
            {
                return string.Format("{0}/", StaticRootFolder);
            }
            else
            {
                string relativePath = string.Format(StaticFolderFormat, folderParts).TrimEnd('/');
                return string.Format("{0}/{1}/", StaticRootFolder, relativePath);
            }
        }



        //name
        public virtual string CreateTempName(string userID, Guid fileID)
        {
            string shortFileID = ShortGuid.Encode(fileID);
            return string.Format(TempNameFormat, userID, shortFileID);
        }

        public virtual string CreateStaticName(params string[] nameParts)
        {
            return string.Format(StaticNameFormat, nameParts);
        }

        public virtual Guid? ExtractFileIDFromTemp(string tempUrl)
        {
            string fileIDPattern = string.Format(TempNameFormat, null, @"([a-zA-Z0-9_-]{22})");

            Match match = Regex.Match(tempUrl, fileIDPattern);

            if (match.Groups.Count < 2 || !match.Groups[1].Success)
            {
                return null;
            }

            return ShortGuid.Decode(match.Groups[1].Value);
        }



        //folder + name
        public virtual string CreateTempNamePath(string userID, Guid fileID, params string[] folderParts)
        {
            string folder = CreateTempFolderPath(folderParts);
            string name = CreateTempName(userID, fileID);
            return string.Format("{0}{1}", folder, name);
        }

        public virtual string CreateStaticNamePath(params string[] namePathParts)
        {
            string namePathFormat = CombineFormatStrings(StaticFolderFormat, StaticNameFormat).Trim('/');
            string relativePath = string.Format(namePathFormat, namePathParts);
            return string.Format("{0}/{1}", StaticRootFolder, relativePath);
        }



        //url
        public virtual string CreateTempUrlRoot()
        {
            return string.Format("{0}/{1}/", UrlBase.TrimEnd('/'), TempRootFolder);
        }

        public virtual string CreateStaticUrlRoot()
        {
            return string.Format("{0}/{1}/", UrlBase.TrimEnd('/'), StaticRootFolder);
        }

        public virtual string CreateTempUrl(string userID, Guid fileID, params string[] folderParts)
        {
            string urlBase = UrlBase.TrimEnd('/');
            string namePath = CreateTempNamePath(userID, fileID, folderParts);

            return string.Format("{0}/{1}", urlBase, namePath);
        }

        public virtual string CreateStaticUrl(params string[] namePathParts)
        {
            string urlBase = UrlBase.TrimEnd('/');
            string namePath = CreateStaticNamePath(namePathParts);

            return string.Format("{0}/{1}", urlBase, namePath);
        }

             
    }
}
