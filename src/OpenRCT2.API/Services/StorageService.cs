using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using OpenRCT2.API.Configuration;

namespace OpenRCT2.API.Services
{
    public class StorageService
    {
        private readonly AmazonS3Client _client;
        private readonly string _bucketName;
        private readonly string _publicUrl;

        public StorageService(IOptions<StorageConfig> storageOptions)
        {
            var storageConfig = storageOptions.Value;
            _client = CreateS3Client(storageConfig);
            _bucketName = storageConfig.Bucket;
            _publicUrl = storageConfig.PublicUrl;
        }

        private static AmazonS3Client CreateS3Client(StorageConfig config)
        {
            var clientConfig = new AmazonS3Config()
            {
                ServiceURL = config.Endpoint
            };
            return new AmazonS3Client(config.Key, config.Secret, clientConfig);
        }

        public async Task<string> UploadPublicFileAsync(Stream stream, string key, string contentType)
        {
            var response = await _client.PutObjectAsync(new PutObjectRequest()
            {
                AutoCloseStream = false,
                AutoResetStreamPosition = false,
                CannedACL = S3CannedACL.PublicRead,
                BucketName = _bucketName,
                ContentType = contentType,
                InputStream = stream,
                Key = key
            });
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to upload file.");
            }
            return _publicUrl + key;
        }

        public async Task DeleteAsync(string key)
        {
            var response = await _client.DeleteObjectAsync(new DeleteObjectRequest()
            {
                BucketName = _bucketName,
                Key = key
            });
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to delete file.");
            }
        }
    }
}
