using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Common.Utility.Pipelines;

namespace ContentManagementBackend
{
    public class FileQueriesBase
    {
        //свойства
        public virtual ImageSettings Settings { get; set; }
        public virtual List<PathCreator> PathCreators
        {
            get
            {
                return Settings.Targets.Select(p => p.PathCreator).ToList();
            }
            protected set { }
        }
        public virtual PathCreator PathCreator
        {
            get
            {
                return PathCreators.FirstOrDefault();
            }
            protected set { }
        }
        public virtual IFileStorage FileStorage { get; set; }
        public virtual UploadImagePipeline UploadImagePipeline { get; set; }
        


        //инициализация
        public FileQueriesBase(IFileStorage fileStorage , ImageSettings settings)
        {
            FileStorage = fileStorage;
            Settings = settings;

            UploadImagePipeline = new UploadImagePipeline(fileStorage);
        }



        //Create
        public virtual Task<bool> CreateTemp(byte[] data, string userID, Guid fileID)
        {
            PathCreator pathCreator = PathCreators.First();
            string namePath = pathCreator.CreateTempNamePath(userID, fileID);
            return FileStorage.Create(namePath, data);
        }

        public virtual Task<bool> CreateStatic(byte[] data, params string[] namePathParts)
        {
            PathCreator pathCreator = PathCreators.First();
            string namePath = pathCreator.CreateStaticNamePath(namePathParts);
            return FileStorage.Create(namePath, data);
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateTempImage(
            Stream stream, string userID)
        {
            var file = new StreamPostedFile(stream);
            return CreateTempImage(file, null, userID);
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateTempImage(
            HttpPostedFileBase file, string userID)
        {
            return CreateTempImage(file, null, userID);
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateTempImage(
            string fileUrl, string userID)
        {
            return CreateTempImage(null, fileUrl, userID);
        }

        protected virtual async Task<PipelineResult<List<ImagePipelineResult>>> CreateTempImage(
            HttpPostedFileBase file, string fileUrl, string userID)
        {
            List<ImagePipelineResult> imageResults = new List<ImagePipelineResult>();
            List<string> namePaths = new List<string>();

            for (int i = 0; i < PathCreators.Count; i++)
            {
                Guid fileID = Guid.NewGuid();

                imageResults.Add(new ImagePipelineResult()
                {
                    FileID = fileID,
                    Url = PathCreators[i].CreateTempUrl(userID, fileID)
                });

                string namePath = PathCreators[i].CreateTempNamePath(userID, fileID);
                namePaths.Add(namePath);
            }

            PipelineResult pipelineResult = await UploadImagePipeline.Process(new ImagePipelineModel()
            {
                Targets = Settings.Targets,
                TargetNamePaths = namePaths,
                DownloadUrl = fileUrl,
                InputStream = file == null ? null : file.InputStream,
                ContentLength = file == null ? 0 : file.ContentLength,
                SizeLimit = Settings.SizeLimit
            });

            return new PipelineResult<List<ImagePipelineResult>>()
            {
                Content = imageResults,
                Message = pipelineResult.Message,
                Result = pipelineResult.Result
            };
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateStaticImage(
            Stream stream, params string[] namePathParts)
        {
            var file = new StreamPostedFile(stream);
            return CreateStaticImage(file, null, namePathParts);
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateStaticImage(
            HttpPostedFileBase file, params string[] namePathParts)
        {
            return CreateStaticImage(file, null, namePathParts);
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateStaticImage(
            string fileUrl, params string[] namePathParts)
        {
            return CreateStaticImage(null, fileUrl, namePathParts);
        }

        protected virtual async Task<PipelineResult<List<ImagePipelineResult>>> CreateStaticImage(
            HttpPostedFileBase file, string fileUrl, params string[] namePathParts)
        {
            List<ImagePipelineResult> imageResults = new List<ImagePipelineResult>();
            List<string> namePaths = new List<string>();

            for (int i = 0; i < PathCreators.Count; i++)
            {
                imageResults.Add(new ImagePipelineResult()
                {
                    Url = PathCreators[i].CreateStaticUrl(namePathParts)
                });

                string namePath = PathCreators[i].CreateStaticNamePath(namePathParts);
                namePaths.Add(namePath);
            }

            var vm = new ImagePipelineModel()
            {
                Targets = Settings.Targets,
                TargetNamePaths = namePaths,
                DownloadUrl = fileUrl,
                InputStream = file == null ? null : file.InputStream,
                ContentLength = file == null ? 0 : file.ContentLength,
                SizeLimit = Settings.SizeLimit
            };
            PipelineResult pipelineResult = await UploadImagePipeline.Process(vm);

            return new PipelineResult<List<ImagePipelineResult>>()
            {
                Content = imageResults,
                Message = pipelineResult.Message,
                Result = pipelineResult.Result
            };
        }



        //Select
        public virtual Task<QueryResult<bool>> TempExists(string userID, Guid fileID)
        {
            PathCreator pathCreator = PathCreators.First();
            string namePath = pathCreator.CreateTempNamePath(userID, fileID);
            return FileStorage.Exists(namePath);
        }

        public virtual Task<QueryResult<bool>> StaticExists(params string[] namePathParts)
        {
            PathCreator pathCreator = PathCreators.First();
            string namePath = pathCreator.CreateStaticNamePath(namePathParts);
            return FileStorage.Exists(namePath);
        }



        //Update
        public virtual Task<bool> MoveTempToStatic(string userID, Guid fileID, params string[] newNamePathParts)
        {
            PathCreator pathCreator = PathCreators.First();
            string oldPath = pathCreator.CreateTempNamePath(userID, fileID);
            string newPath = pathCreator.CreateStaticNamePath(newNamePathParts);
            return FileStorage.Move(oldPath, newPath);
        }

        public virtual Task<bool> RenameStatic(string newName, string oldName)
        {
            return RenameStatic(new[] { newName }, new[] { oldName });
        }

        public virtual Task<bool> RenameStatic(string[] newNamePathParts, string[] oldNamePathParts)
        {
            PathCreator pathCreator = PathCreators.First();
            string oldPath = pathCreator.CreateStaticNamePath(oldNamePathParts);
            string newPath = pathCreator.CreateStaticNamePath(newNamePathParts);
            return FileStorage.Move(oldPath, newPath);
        }

        

        //Delete
        public virtual Task<bool> DeleteTemp(string userID, Guid fileID)
        {
            PathCreator pathCreator = PathCreators.First();
            string namePath = pathCreator.CreateTempNamePath(userID, fileID);
            return FileStorage.Delete(new List<string>() { namePath });
        }

        public virtual Task<bool> DeleteStatic(params string[] namePathParts)
        {
            PathCreator pathCreator = PathCreators.First();
            string namePath = pathCreator.CreateStaticNamePath(namePathParts);
            return FileStorage.Delete(new List<string>() { namePath });
        }

        public virtual Task<bool> DeleteStaticDirectory(params string[] folderPathParts)
        {
            PathCreator pathCreator = PathCreators.First();
            string folderPath = pathCreator.CreateStaticFolderPath(folderPathParts);
            return FileStorage.DeleteDirectory(folderPath);
        }
        
        public virtual async Task<bool> CleanTemp()
        {
            bool completed = true;

            string path = PathCreator.TempRootFolder.TrimEnd('/') + "/";
            QueryResult<List<FileDetails>> filesResult = await FileStorage.GetList(path);
            if (filesResult.HasExceptions)
            {
                return false;
            }

            List<FileDetails> filesToDelete = new List<FileDetails>();
            foreach (FileDetails file in filesResult.Result)
            {
                TimeSpan fileAge = DateTime.UtcNow - file.LastModifyTimeUtc;
                if (fileAge > Settings.TempDeleteAge)
                {
                    filesToDelete.Add(file);
                }
            }

            if (filesToDelete.Count > 0)
            {
                List<string> keysToDelete = filesToDelete.Select(p => p.Key).ToList();
                completed = await FileStorage.Delete(keysToDelete);
            }

            return completed;
        }
    }
}
