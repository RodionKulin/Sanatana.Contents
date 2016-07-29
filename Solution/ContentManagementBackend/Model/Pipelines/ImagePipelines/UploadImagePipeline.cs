using Common.Utility.Pipelines;
using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{ 
    public class UploadImagePipeline : Pipeline<ImagePipelineModel, PipelineResult>
    {
        //поля
        protected IFileStorage _fileStorage;
        protected byte[] _inputBytes;
        protected List<byte[]> _formatedBytes;



        //инициализация
        public UploadImagePipeline(IFileStorage fileStorage)
        {
            _fileStorage = fileStorage;
            RegisterModules();
        }



        //запуск
        protected virtual void RegisterModules()
        {
            Register(Download);
            Register(Format);
            Register(Save);
        }

        public override Task<PipelineResult> Process(ImagePipelineModel inputModel)
        {
            PipelineResult outputModel = new PipelineResult()
            {
                Result = true
            };
            return base.Process(inputModel, outputModel);
        }


        //этапы
        public virtual async Task<bool> CheckExisting(
           PipelineContext<ImagePipelineModel, PipelineResult> context)
        {
            for (int i = 0; i < context.Input.TargetNamePaths.Count; i++)
            {
                string namePath = context.Input.TargetNamePaths[i];

                QueryResult<bool> existsResult = await _fileStorage.Exists(namePath);

                if (existsResult.HasExceptions)
                {
                    context.Output = PipelineResult.Fail(MessageResources.Image_SaveException);
                    return false;
                }

                if (existsResult.Result)
                {
                    string message = string.Format(MessageResources.Image_NameInUse, context.Input.FileName);
                    context.Output = PipelineResult.Fail(message);
                    return false;
                }
            }

            return true;
        }

        public virtual Task<bool> Download(
            PipelineContext<ImagePipelineModel, PipelineResult> context)
        {
            if (context.Input.DownloadUrl == null
                && context.Input.InputStream == null)
            {
                context.Output = PipelineResult.Fail(MessageResources.Image_ReceiveException);
                return Task.FromResult(false);
            }

            string error;
            _inputBytes = context.Input.DownloadUrl == null
                ? ImageDownloader.ReceiveStream(context.Input.InputStream
                    , context.Input.ContentLength, context.Input.SizeLimit, out error)
                : ImageDownloader.Download(context.Input.DownloadUrl, context.Input.SizeLimit, out error);

            if (error != null)
            {
                context.Output = PipelineResult.Fail(error);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public virtual Task<bool> Format(
            PipelineContext<ImagePipelineModel, PipelineResult> context)
        {
            _formatedBytes = new List<byte[]>();

            foreach (ImageTargetParameters target in context.Input.Targets)
            {
                QueryResult<byte[]> formatedResult = ImageResizing.FillSize(_inputBytes, target);

                if (formatedResult.HasExceptions)
                {
                    context.Output = PipelineResult.Fail(MessageResources.Image_FormatException);
                    return Task.FromResult(false);
                }

                _formatedBytes.Add(formatedResult.Result);
            }

            return Task.FromResult(true);
        }

        public virtual async Task<bool> Save(
           PipelineContext<ImagePipelineModel, PipelineResult> context)
        {
            for (int i = 0; i < context.Input.Targets.Count; i++)
            {
                byte[] formatedBytes = _formatedBytes[i];
                string namePath = context.Input.TargetNamePaths[i];

                bool completed = await _fileStorage.Create(namePath, formatedBytes);

                if (!completed)
                {
                    context.Output = PipelineResult.Fail(MessageResources.Image_SaveException);
                    return false;
                }
            }
            
            return true;
        }


    }
}
