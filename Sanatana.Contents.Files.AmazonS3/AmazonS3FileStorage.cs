using Amazon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Contents.Files.AmazonS3.Resources;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.S3.Model;
using Sanatana.Contents.Files.Queries;
using System.Runtime.ExceptionServices;

namespace Sanatana.Contents.Files.AmazonS3
{
    public class AmazonS3FileStorage : IFileStorage
    {
        //fields
        protected AmazonS3Settings _settings { get; set; }
        

        //init
        public AmazonS3FileStorage(AmazonS3Settings settings)
        {
            _settings = settings;
        }



        //methods
        protected virtual IAmazonS3 GetS3Client()
        {
            RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(_settings.RegionEndpoint);

            bool isUnknownRegion = AmazonS3Constansts.UNKNOWN_CREDENTIALS_REGION.Equals(
                endpoint.DisplayName, StringComparison.OrdinalIgnoreCase);

            if (isUnknownRegion)
            {
                string comment = string.Format(AmazonS3Messages.RegionNotFound, endpoint);
                throw new KeyNotFoundException(comment);
            }

            return new AmazonS3Client(_settings.AccessKey, _settings.SecretKey, endpoint);
        }
        
        public virtual async Task Create(string namePath, byte[] inputBytes)
        {
            IAmazonS3 client = GetS3Client();
            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = _settings.BucketName,
                Key = namePath,
                InputStream = new MemoryStream(inputBytes),
                AutoCloseStream = true,
                CannedACL = S3CannedACL.PublicRead
            };
            PutObjectResponse response = await client.PutObjectAsync(request).ConfigureAwait(false);
        }

        public virtual async Task<List<FileDetails>> GetList(string directoryPath)
        {
            List<FileDetails> files = new List<FileDetails>();
            
            IAmazonS3 client = GetS3Client();

            ListObjectsRequest listRequest = new ListObjectsRequest()
            {
                BucketName = _settings.BucketName,
                Prefix = directoryPath,
                MaxKeys = AmazonS3Constansts.MAX_OBJECTS_SELECTED
            };
            ListObjectsResponse response = await client.ListObjectsAsync(listRequest).ConfigureAwait(false);

            foreach (S3Object s3Object in response.S3Objects)
            {
                if (s3Object.Size > 0)
                {
                    files.Add(new FileDetails()
                    {
                        NamePath = s3Object.Key,
                        LastModifyTimeUtc = s3Object.LastModified.ToUniversalTime()
                    });
                }
            }

            return files;
        }

        public virtual async Task Move(List<string> oldNamePaths, List<string> newNamePaths)
        {
            await Copy(oldNamePaths, newNamePaths);

            IAmazonS3 client = GetS3Client();
            var requestTasks = new List<Task<DeleteObjectResponse>>();

            for (int i = 0; i < oldNamePaths.Count; i++)
            {
                DeleteObjectRequest deleteRequest = new DeleteObjectRequest()
                {
                    BucketName = _settings.BucketName,
                    Key = oldNamePaths[i]
                };
                Task<DeleteObjectResponse> responseTask = client.DeleteObjectAsync(deleteRequest);
                requestTasks.Add(responseTask);
            }

            DeleteObjectResponse[] responses = await Task.WhenAll(requestTasks.ToArray())
                .ConfigureAwait(false);
        }

        public virtual async Task Copy(List<string> oldNamePaths, List<string> newNamePaths)
        {
            IAmazonS3 client = GetS3Client();
            var requestTasks = new List<Task<CopyObjectResponse>>();

            for (int i = 0; i < oldNamePaths.Count; i++)
            {
                CopyObjectRequest request = new CopyObjectRequest()
                {
                    SourceBucket = _settings.BucketName,
                    SourceKey = oldNamePaths[i],
                    DestinationBucket = _settings.BucketName,
                    DestinationKey = newNamePaths[i],
                    CannedACL = S3CannedACL.PublicRead
                };

                Task<CopyObjectResponse> responseTask = client.CopyObjectAsync(request);
                requestTasks.Add(responseTask);
            }

            CopyObjectResponse[] responses = await Task.WhenAll(requestTasks.ToArray())
                .ConfigureAwait(false);
        }
        
        public virtual async Task Delete(List<string> namePaths)
        {
            IAmazonS3 client = GetS3Client();

            DeleteObjectsRequest request = new DeleteObjectsRequest()
            {
                BucketName = _settings.BucketName,
                Objects = namePaths.Select(p => new KeyVersion() { Key = p }).ToList()
            };

            DeleteObjectsResponse response = await client.DeleteObjectsAsync(request)
                .ConfigureAwait(false);
        }

        public virtual async Task DeleteDirectory(string directoryPath)
        {
            List<FileDetails> files = new List<FileDetails>();
            IAmazonS3 client = GetS3Client();
            
            while (true)
            {
                //list
                ListObjectsRequest listRequest = new ListObjectsRequest()
                {
                    BucketName = _settings.BucketName,
                    Prefix = directoryPath,
                    MaxKeys = AmazonS3Constansts.MAX_OBJECTS_DELETED
                };
                ListObjectsResponse listResponse = await client.ListObjectsAsync(listRequest)
                    .ConfigureAwait(false);

                //delete
                List<KeyVersion> objects = listResponse.S3Objects
                    .Select(p => new KeyVersion() { Key = p.Key })
                    .ToList();

                if (objects.Count == 0)
                {
                    break;
                }

                DeleteObjectsRequest deleteRequest = new DeleteObjectsRequest()
                {
                    BucketName = _settings.BucketName,
                    Objects = objects
                };
                DeleteObjectsResponse deleteResponse = await client.DeleteObjectsAsync(deleteRequest)
                    .ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> Exists(string namePath)
        {
            bool exists = false;

            try
            {
                IAmazonS3 client = GetS3Client();

                GetObjectMetadataRequest request = new GetObjectMetadataRequest()
                {
                    BucketName = _settings.BucketName,
                    Key = namePath
                };

                GetObjectMetadataResponse response
                    = await client.GetObjectMetadataAsync(request)
                    .ConfigureAwait(false);

                exists = true;
            }
            catch (AmazonS3Exception ex)
            {
                if(ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    exists = false;
                }
                else
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                }
            }

            return exists;
        }

    }
}
