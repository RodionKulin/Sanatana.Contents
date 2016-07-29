using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.CloudFront;
using Amazon.S3.Transfer;
using ContentManagementBackend.AmazonS3Files.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.CloudFront.Model;

namespace ContentManagementBackend.AmazonS3Files
{
    public class AmazonFileStorage : IFileStorage
    {
        //поля
        protected ICommonLogger _logger;


        //свойства
        public virtual AmazonS3Settings Settings { get; set; }
        


        //инициализация
        public AmazonFileStorage(ICommonLogger logger, AmazonS3Settings settings)
        {
            _logger = logger;
            Settings = settings;
        }



        //методы
        public virtual AmazonCloudFrontClient GetCloudFrontClient()
        {
            RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(Settings.RegionEndpoint);

            bool isUnknownRegion = AmazonConstansts.UNKNOWN_CREDENTIALS_REGION.Equals(
                endpoint.DisplayName, StringComparison.InvariantCultureIgnoreCase);

            if (isUnknownRegion)
            {
                string comment = string.Format("Amazon region с именем {0} не обнаружен.", endpoint);
                Exception exception = new KeyNotFoundException(comment);
                _logger.Exception(exception);
                throw exception;
            }

            var client = new AmazonCloudFrontClient(Settings.AccessKey, Settings.SecretKey, endpoint);
            return client;            
        }

        protected virtual IAmazonS3 GetS3Client()
        {
            RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(Settings.RegionEndpoint);

            bool isUnknownRegion = AmazonConstansts.UNKNOWN_CREDENTIALS_REGION.Equals(
                endpoint.DisplayName, StringComparison.InvariantCultureIgnoreCase);

            if (isUnknownRegion)
            {
                string comment = string.Format(MessageResources.RegionNotFound, endpoint);
                Exception exception = new KeyNotFoundException(comment);
                _logger.Exception(exception);
                throw exception;
            }
            
            return AWSClientFactory.CreateAmazonS3Client(
                Settings.AccessKey, Settings.SecretKey, endpoint
            );
        }
        
        public virtual async Task<bool> Create(string namePath, byte[] inputBytes)
        {
            bool completed = false;
            
            try
            {

                IAmazonS3 client = GetS3Client();
                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = Settings.BucketName,
                    Key = namePath,
                    InputStream = new MemoryStream(inputBytes),
                    AutoCloseStream = true,
                    CannedACL = S3CannedACL.PublicRead
                };
                PutObjectResponse response = await client.PutObjectAsync(request);
                
                completed = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return completed;
        }

        public virtual async Task<QueryResult<List<FileDetails>>> GetList(string folderPath)
        {
            List<FileDetails> files = new List<FileDetails>();
            bool hasExceptions = false;

            try
            {
                IAmazonS3 client = GetS3Client();

                ListObjectsRequest listRequest = new ListObjectsRequest()
                {
                    BucketName = Settings.BucketName,
                    Prefix = folderPath,
                    MaxKeys = AmazonConstansts.MAX_OBJECTS_SELECTED
                };
                ListObjectsResponse response = await client.ListObjectsAsync(listRequest);

                foreach (S3Object s3Object in response.S3Objects)
                {
                    if (s3Object.Size > 0)
                    {
                        files.Add(new FileDetails()
                        {
                            Key = s3Object.Key,
                            LastModifyTimeUtc = s3Object.LastModified.ToUniversalTime()
                        });
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return new QueryResult<List<FileDetails>>(files, hasExceptions);
        }

        public virtual async Task<bool> Move(string oldNamePath, string newNamePath)
        {
            bool completed = false;

            try
            {
                IAmazonS3 client = GetS3Client();

                CopyObjectRequest request = new CopyObjectRequest()
                {
                    SourceBucket = Settings.BucketName,
                    SourceKey = oldNamePath,
                    DestinationBucket = Settings.BucketName,
                    DestinationKey = newNamePath,
                    CannedACL = S3CannedACL.PublicRead
                };
                
                CopyObjectResponse response = await client.CopyObjectAsync(request);

                DeleteObjectRequest deleteRequest = new DeleteObjectRequest()
                {
                    BucketName = request.SourceBucket,
                    Key = request.SourceKey
                };
                DeleteObjectResponse deleteResponse = await client.DeleteObjectAsync(deleteRequest);

                completed = true;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.Exception(ex);
            }

            return completed;
        }

        public virtual async Task<bool> Copy(List<string> oldNamePaths, List<string> newNamePaths)
        {
            bool completed = false;
            
            try
            {
                IAmazonS3 client = GetS3Client();
                List<Task<CopyObjectResponse>> requestTasks = new List<Task<CopyObjectResponse>>();

                for (int i = 0; i < oldNamePaths.Count; i++)
                {
                    CopyObjectRequest request = new CopyObjectRequest()
                    {
                        SourceBucket = Settings.BucketName,
                        SourceKey = oldNamePaths[i],
                        DestinationBucket = Settings.BucketName,
                        DestinationKey = newNamePaths[i],
                        CannedACL = S3CannedACL.PublicRead
                    };
                    
                    Task<CopyObjectResponse> response = client.CopyObjectAsync(request);
                    requestTasks.Add(response);
                }

                CopyObjectResponse[] responses = await Task.WhenAll(requestTasks.ToArray());
                completed = true;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.Exception(ex);
            }

            return completed;
        }
        
        public virtual async Task<bool> Delete(List<string> namePaths)
        {
            bool completed = false;

            try
            {
                IAmazonS3 client = GetS3Client();

                DeleteObjectsRequest request = new DeleteObjectsRequest()
                {
                    BucketName = Settings.BucketName,
                    Objects = namePaths.Select(p => new KeyVersion() { Key = p }).ToList()
                };

                DeleteObjectsResponse response = await client.DeleteObjectsAsync(request);

                completed = true;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.Exception(ex);
            }
            
            return completed;
        }

        public virtual async Task<bool> DeleteDirectory(string folderPath)
        {
            List<FileDetails> files = new List<FileDetails>();
            bool completed = false;

            try
            {
                IAmazonS3 client = GetS3Client();
                int deleteRounds = 0;

                while(true)
                {
                    if(deleteRounds >= AmazonConstansts.MAX_DELETE_ROUNDS)
                    {                        
                        _logger.Error(MessageResources.MaxDeleteRoundsExceptions, AmazonConstansts.MAX_DELETE_ROUNDS);
                        return false;
                    }
                    deleteRounds++;

                    //list
                    ListObjectsRequest listRequest = new ListObjectsRequest()
                    {
                        BucketName = Settings.BucketName,
                        Prefix = folderPath,
                        MaxKeys = AmazonConstansts.MAX_OBJECTS_DELETED
                    };

                    ListObjectsResponse listResponse = await client.ListObjectsAsync(listRequest);
                                                        
                    //delete
                    List<KeyVersion> objects = listResponse.S3Objects
                        .Select(p => new KeyVersion() { Key = p.Key }).ToList();

                    if(objects.Count == 0)
                    {
                        break;
                    }

                    DeleteObjectsRequest deleteRequest = new DeleteObjectsRequest()
                    {
                        BucketName = Settings.BucketName,
                        Objects = objects
                    };
                    DeleteObjectsResponse deleteResponse = await client.DeleteObjectsAsync(deleteRequest);
                }                

                completed = true;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.Exception(ex);
            }
            
            return completed;
        }

        public virtual async Task<QueryResult<bool>> Exists(string namePath)
        {
            bool exists = false;
            bool hasExceptions = false;

            try
            {
                IAmazonS3 client = GetS3Client();

                GetObjectMetadataRequest request = new GetObjectMetadataRequest()
                {
                    BucketName = Settings.BucketName,
                    Key = namePath
                };

                GetObjectMetadataResponse response
                    = await client.GetObjectMetadataAsync(request);

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
                    _logger.Exception(ex);
                    hasExceptions = true;
                }
            }

            return new QueryResult<bool>(exists, hasExceptions);
        }

        public virtual string GetBaseUrl()
        {
            return string.Format("{0}/{1}/"
                , Settings.BucketDomain
                , Settings.BucketName);
        }
    }
}
