﻿using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Patterns.Pipelines;
using Sanatana.Contents.Files.Downloads;
using Sanatana.Contents.Files.Resizer;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Files;
using Sanatana.Contents.Utilities;

namespace Sanatana.Contents.Pipelines.Images
{
    public class UploadImagePipeline : Pipeline<UploadImageParams, PipelineResult<List<UploadImageResult>>>
    {
        //fields
        protected IPipelineExceptionHandler _exceptionHandler;
        protected Dictionary<int, FilePathProvider> _filePathProviders;
        protected IFileStorage _fileStorage;
        protected byte[] _inputBytes;
        protected List<byte[]> _formatedBytes;
        protected IFileDownloader _fileDownloader;
        protected IImageResizer _imageResizer;
        protected List<string> _targetNamePaths;


        //init
        public UploadImagePipeline(IPipelineExceptionHandler exceptionHandler, IEnumerable<FilePathProvider> filePathProviders
            , IFileStorage fileStorage, IFileDownloader fileDownloader, IImageResizer imageResizer)
        {
            _exceptionHandler = exceptionHandler;
            _filePathProviders = filePathProviders.ToDictionary(x => x.FilePathProviderId);
            _fileStorage = fileStorage;
            _fileDownloader = fileDownloader;
            _imageResizer = imageResizer;

            RegisterModules();
        }
        

        //bootstrap
        protected virtual void RegisterModules()
        {
            Register(GenerateTargetsNamePaths);
            Register(Download);
            Register(Format);
            Register(Store);
        }

        public override async Task RollBack(
            PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context)
        {
            await base.RollBack(context);

            if(context.Exceptions != null)
            {
                _exceptionHandler.Handle(context);
            }
            if(context.Output == null)
            {
                context.Output = PipelineResult<List<UploadImageResult>>
                    .Error(ContentsMessages.Common_ProcessingError);
            }

            return;
        }


        //modules
        public virtual Task<bool> GenerateTargetsNamePaths(
           PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context)
        {
            _targetNamePaths = new List<string>();
            context.Output = new PipelineResult<List<UploadImageResult>>()
            {
                Completed = true,
                Data = new List<UploadImageResult>()
            };

            for (int i = 0; i < context.Input.Destinations.Count; i++)
            {
                ImageDestinationParams targetFile = context.Input.Destinations[i];
                FilePathProvider pathMapper = _filePathProviders[targetFile.FilePathMapperId];

                string fileName = context.Input.DestinationFileNames == null
                    ? ShortGuid.NewGuid().Value
                    : context.Input.DestinationFileNames[i];

                context.Output.Data.Add(new UploadImageResult()
                {
                    FileName = fileName,
                    Url = pathMapper.GetFullUrl(context.Input.UserId, fileName)
                });

                string namePath = pathMapper.GetNamePath(context.Input.UserId, fileName);
                _targetNamePaths.Add(namePath);
            }

            return Task.FromResult(true);
        }

        public virtual async Task<bool> CheckExistingNames(
           PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context)
        {
            for (int i = 0; i < _targetNamePaths.Count; i++)
            {
                bool exists = await _fileStorage.Exists(_targetNamePaths[i])
                    .ConfigureAwait(false);

                if (exists)
                {
                    string message = string.Format(ContentsMessages.Image_NameInUse, _targetNamePaths[i]);
                    context.Output = PipelineResult<List<UploadImageResult>>.Error(message);
                    return false;
                }
            }

            return true;
        }

        public virtual async Task<bool> Download(
            PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context)
        {
            if (context.Input.DownloadUrl == null
                && context.Input.FileStream == null)
            {
                context.Output = PipelineResult<List<UploadImageResult>>.Error(ContentsMessages.Image_SourceNotSpecified);
                return false;
            }

            PipelineResult<byte[]> downloadResult = context.Input.DownloadUrl == null
                ? await _fileDownloader.Download(context.Input.FileStream.Stream
                    , context.Input.FileStream.ContentLength, context.Input.ContentLengthLimit)
                    .ConfigureAwait(false)
                : await _fileDownloader.Download(context.Input.DownloadUrl, context.Input.ContentLengthLimit)
                    .ConfigureAwait(false);

            if (downloadResult.Completed == false)
            {
                context.Output = PipelineResult<List<UploadImageResult>>.Error(downloadResult.Messages);
                return false;
            }

            _inputBytes = downloadResult.Data;
            return true;
        }

        public virtual Task<bool> Format(
            PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context)
        {
            _formatedBytes = new List<byte[]>();

            foreach (ImageDestinationParams target in context.Input.Destinations)
            {
                PipelineResult<byte[]> formatedResult = _imageResizer.Resize(_inputBytes
                    , target.TargetFormat, target.ResizeType, target.Width, target.Height, target.RoundCorners);

                if (formatedResult.Completed == false)
                {
                    context.Output = PipelineResult<List<UploadImageResult>>.Error(formatedResult.Messages);
                    return Task.FromResult(false);
                }

                _formatedBytes.Add(formatedResult.Data);
            }

            return Task.FromResult(true);
        }

        public virtual async Task<bool> Store(
           PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context)
        {
            for (int i = 0; i < context.Input.Destinations.Count; i++)
            {
                byte[] formatedBytes = _formatedBytes[i];
                string namePath = _targetNamePaths[i];

                await _fileStorage.Create(namePath, formatedBytes).ConfigureAwait(false);
            }

            return true;
        }


    }
}
