using Common.DataGenerator;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.InitializerModules
{
    //http://ckeditor.com/demo
    //http://ru.texthandler.com/?module=remove_line_breaks
    //<p><iframe allowfullscreen=\"\" frameborder=\"0\" height=\"360\" src=\"//www.youtube.com/embed/2SnQaG_L1KY\" width=\"640\"></iframe></p>

    public class ContentProvider
    {
        //свойства
        public static string AvatarsPath { get; set; }


        //инициализация
        static ContentProvider()
        {
            AvatarsPath = Path.Combine("Content", "Avatars");
        }
        

        //методы
        public static FileInfo[] GetAvatarFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(AvatarsPath);
            return dir.GetFiles();
        }

        public static List<string> GetAvatarNames()
        {
            FileInfo[] files = GetAvatarFiles();
            List<string> names = files.Select(f => Path.GetFileNameWithoutExtension(f.FullName)).ToList();
            
            return names;
        }

        public static List<PostEssentials> GetPosts(string directoryPath)
        {
            List<PostEssentials> essencials = new List<PostEssentials>();
            var directory = new DirectoryInfo(directoryPath);
            FileInfo[] postFiles = directory.GetFiles("*.txt");

            foreach (FileInfo postFile in postFiles)
            {
                string imagePath = string.Format("{0}/{1}.jpg", directory.FullName
                    , Path.GetFileNameWithoutExtension(postFile.FullName));
                
                essencials.Add(new PostEssentials()
                {
                    FullContent = File.ReadAllText(postFile.FullName),
                    Title = Path.GetFileNameWithoutExtension(postFile.FullName),
                    ImageFile = new FileInfo(imagePath)
                });
            }

            return essencials;
        }

        public static List<UserEssentials> GetUsers()
        {
            List<string> imageNames = ContentProvider.GetAvatarNames();
            var users = new List<UserEssentials>();

            List<string> userNames = new List<string>
                { "Master Shake", "Frylock", "Meatwad", "Carl", "Ignignokt", "Boxy Brown", "Err"
                , "Cybernetic Ghost of Christmas Past from the Future", "Markula", "Dr. Weird", "Steve"};
            
            foreach (string userName in userNames)
            {
                users.Add(new UserEssentials()
                {
                    ID = ObjectId.GenerateNewId(),
                    Avatar = RandomHelper.PickFromList(imageNames),
                    Name = userName,
                    Email = userName.Replace(" ", "") + "@fake.mail",
                    Password = null,
                    Roles = null
                });
            }

            return users;
        }
    }
}
