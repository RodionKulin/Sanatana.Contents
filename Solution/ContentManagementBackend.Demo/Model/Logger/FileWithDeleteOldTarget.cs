using NLog;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContentManagementBackend.Demo
{
    [Target("FileWithDeleteOld")]
    public class FileWithDeleteOldTarget : FileTarget
    {
        //свойства
        public int DeleteAfterDays { get; set; }



        //иницилизация
        public FileWithDeleteOldTarget()
        {
            DeleteAfterDays = 7;
            this.Encoding = new UTF8Encoding();
        }


        //методы
        protected override void Write(LogEventInfo logEvent)
        {
            DeleteOldFiles(logEvent);

            base.Write(logEvent);
        }
        
        private void DeleteOldFiles(LogEventInfo logEvent)
        {
            string fileName = FileName.Render(logEvent);
            FileInfo fileInfo = new FileInfo(fileName);
            bool newFile = !fileInfo.Exists;


            // Удаляем файлы позжде периода хранения
            if (newFile)
            {
                DirectoryInfo logDirectory = fileInfo.Directory;

                if (logDirectory.Exists)
                {
                    DateTime oldestArchiveDate = DateTime.Now - new TimeSpan(DeleteAfterDays, 0, 0, 0);
                    foreach (FileInfo fi in logDirectory.GetFiles())
                    {
                        DateTime fileDate = ExtractFileDate(fi);
                        if (fileDate < oldestArchiveDate)
                            fi.Delete();
                    }
                }
            }
        }

        internal DateTime ExtractFileDate(FileInfo file)
        {
            DateTime fileNameDate = DateTime.MaxValue;
            
            //выделить разметку названия файла
            string layoutString = FileName.ToString();
            layoutString = layoutString.Replace('\\', '/').Trim('\'');
            string[] layoutParts = layoutString.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (layoutParts.Length == 0)
                return fileNameDate;

            //ищем входжение даты в названии
            string fileNameLayoutPart = layoutParts.Last();
            string searchPattern = fileNameLayoutPart.Replace("${shortdate}", @"([0-9-]+)");

            Regex dateSearchRegex = new Regex(searchPattern);
            Match dateMatch = dateSearchRegex.Match(file.Name);

            if (!dateMatch.Success || dateMatch.Groups.Count < 2)
                return fileNameDate;


            //парсим дату
            string fileNameDateString = dateMatch.Groups[1].Value;
            if (!DateTime.TryParse(fileNameDateString, out fileNameDate))
            {
                fileNameDate = DateTime.MaxValue;
            }
            return fileNameDate;
        }
    }
}
